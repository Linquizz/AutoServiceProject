using AutoService.Models;

namespace AutoService.Services;

public record CompletionResult(string PdfPath, string Message);

public class WorkOrderDocumentService
{
    private readonly WorkOrderService _orders = new();
    private readonly WorkOrderPdfGenerator _pdf = new();
    private readonly EmailService _email = new();

    public bool IsEmailConfigured => _email.IsConfigured;

    public async Task<string> GeneratePdfAsync(int orderId)
    {
        var order = await _orders.GetAsync(orderId) ?? throw new BusinessException("Заказ-наряд не найден.");
        return await Task.Run(() => _pdf.Generate(order, AppPaths.WorkOrdersFolder));
    }

    public async Task<string> SendToClientAsync(int orderId, string? addressOverride = null)
    {
        var order = await _orders.GetAsync(orderId) ?? throw new BusinessException("Заказ-наряд не найден.");
        var address = string.IsNullOrWhiteSpace(addressOverride) ? order.Car?.Client?.Email : addressOverride.Trim();

        if (string.IsNullOrWhiteSpace(address))
            throw new BusinessException("У клиента не указан адрес электронной почты. Укажите его в карточке клиента или введите адрес вручную.");

        var path = await Task.Run(() => _pdf.Generate(order, AppPaths.WorkOrdersFolder));
        await _email.SendWorkOrderAsync(order, path, address);
        return address;
    }

    public async Task<CompletionResult> CompleteAndSendAsync(int orderId)
    {
        await _orders.SetCompletedAsync(orderId, true);

        var order = await _orders.GetAsync(orderId) ?? throw new BusinessException("Заказ-наряд не найден.");
        var pdfPath = await Task.Run(() => _pdf.Generate(order, AppPaths.WorkOrdersFolder));

        var email = order.Car?.Client?.Email;
        if (string.IsNullOrWhiteSpace(email))
            return new CompletionResult(pdfPath,
                "Заказ-наряд завершён, PDF сформирован.\nУ клиента не указана электронная почта - письмо не отправлялось.");

        try
        {
            await _email.SendWorkOrderAsync(order, pdfPath, email);
            return new CompletionResult(pdfPath, $"Заказ-наряд завершён, PDF сформирован и отправлен клиенту на {email}.");
        }
        catch (BusinessException ex)
        {
            return new CompletionResult(pdfPath,
                $"Заказ-наряд завершён, PDF сформирован, но письмо на {email} не отправлено:\n{ex.Message}");
        }
    }
}
