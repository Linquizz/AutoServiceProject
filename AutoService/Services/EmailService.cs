using AutoService.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace AutoService.Services;

public class EmailService
{
    private static SmtpSettings Smtp => AppSettings.Current.Smtp;

    private static string FromAddress =>
        string.IsNullOrWhiteSpace(Smtp.FromAddress) ? Smtp.UserName : Smtp.FromAddress;

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(Smtp.Host)
        && !string.IsNullOrWhiteSpace(Smtp.UserName)
        && !string.IsNullOrWhiteSpace(Smtp.Password)
        && !string.IsNullOrWhiteSpace(FromAddress);

    public async Task SendWorkOrderAsync(WorkOrder order, string pdfPath, string toAddress)
    {
        if (!IsConfigured)
            throw new BusinessException("Отправка почты не настроена. Заполните раздел Smtp в файле appsettings.json (см. README).");
        if (!Validator.IsValidEmail(toAddress))
            throw new BusinessException("Некорректный адрес электронной почты получателя.");
        if (!File.Exists(pdfPath))
            throw new BusinessException("Файл PDF не найден.");

        var client = order.Car?.Client;

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(Smtp.FromName, FromAddress));
        message.To.Add(new MailboxAddress(client?.FullName ?? toAddress, toAddress.Trim()));
        message.Subject = $"Заказ-наряд № {order.Id}";

        var body = new BodyBuilder
        {
            TextBody =
                $"Здравствуйте, {client?.FirstName}!\n\n" +
                $"Работы по вашему автомобилю {order.Car?.Title} ({order.RegistrationNumber}) выполнены.\n" +
                $"Во вложении - заказ-наряд № {order.Id} на сумму {ParseHelper.Money(order.Total)} руб.\n\n" +
                "С уважением, автосервис."
        };
        body.Attachments.Add(pdfPath);
        message.Body = body.ToMessageBody();

        try
        {
            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(Smtp.Host, Smtp.Port, ParseMode(Smtp.SecureMode));
            await smtp.AuthenticateAsync(Smtp.UserName, Smtp.Password);
            await smtp.SendAsync(message);
            await smtp.DisconnectAsync(true);
        }
        catch (AuthenticationException)
        {
            throw new BusinessException(
                "Почтовый сервер отклонил логин или пароль. Для Яндекс/Mail.ru/Gmail нужен специальный «пароль приложения», а не пароль от ящика.");
        }
        catch (Exception ex)
        {
            throw new BusinessException("Не удалось отправить письмо: " + ex.Message);
        }
    }

    private static SecureSocketOptions ParseMode(string? mode) =>
        Enum.TryParse<SecureSocketOptions>(mode, true, out var parsed) ? parsed : SecureSocketOptions.Auto;
}
