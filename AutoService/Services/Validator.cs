using System.Text.RegularExpressions;
using AutoService.Models;

namespace AutoService.Services;

public static class Validator
{
    private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
    private static readonly Regex PhoneCharsRegex = new(@"^\+?[\d\s\-()]+$", RegexOptions.Compiled);
    private static readonly Regex VinRegex = new(@"^[A-HJ-NPR-Z0-9]{17}$", RegexOptions.Compiled);
    private static readonly Regex PlateRegex = new(@"^[АВЕКМНОРСТУХ]\d{3}[АВЕКМНОРСТУХ]{2}\d{2,3}$", RegexOptions.Compiled);

    private const string LatinPlateLetters = "ABEKMHOPCTYX";
    private const string CyrillicPlateLetters = "АВЕКМНОРСТУХ";

    public const decimal MaxMoney = 99_999_999m;

    public static void ValidateClient(Client c)
    {
        Required(c.LastName, "Фамилия", 50);
        Required(c.FirstName, "Имя", 50);
        if (!string.IsNullOrWhiteSpace(c.MiddleName) && c.MiddleName.Trim().Length > 50)
            throw new BusinessException("Отчество не может быть длиннее 50 символов.");

        var phone = c.Phone?.Trim() ?? "";
        var digits = phone.Count(char.IsDigit);
        if (phone.Length == 0)
            throw new BusinessException("Поле «Телефон» обязательно для заполнения.");
        if (!PhoneCharsRegex.IsMatch(phone) || digits < 10 || digits > 12 || phone.Length > 20)
            throw new BusinessException("Некорректный телефон. Пример правильного формата: +7 900 123-45-67.");

        if (!string.IsNullOrWhiteSpace(c.Email))
        {
            var email = c.Email.Trim();
            if (email.Length > 50 || !EmailRegex.IsMatch(email))
                throw new BusinessException("Некорректный адрес электронной почты (не длиннее 50 символов). Пример: client@mail.ru.");
        }
    }

    public static string NormalizePlate(string? plate)
    {
        var chars = (plate ?? "").Trim().ToUpperInvariant().Replace(" ", "").ToCharArray();
        for (var i = 0; i < chars.Length; i++)
        {
            var idx = LatinPlateLetters.IndexOf(chars[i]);
            if (idx >= 0) chars[i] = CyrillicPlateLetters[idx];
        }
        return new string(chars);
    }

    public static void ValidateCar(Car car)
    {
        if (car.ClientId <= 0) throw new BusinessException("Выберите владельца автомобиля.");
        if (car.ModelId <= 0) throw new BusinessException("Выберите марку и модель автомобиля.");

        if (string.IsNullOrWhiteSpace(car.RegistrationNumber))
            throw new BusinessException("Поле «Государственный номер» обязательно для заполнения.");
        if (!PlateRegex.IsMatch(car.RegistrationNumber))
            throw new BusinessException("Некорректный государственный номер. Пример правильного формата: А123ВС33 или А123ВС133.");

        if (!string.IsNullOrWhiteSpace(car.VIN))
        {
            var vin = car.VIN.Trim().ToUpperInvariant();
            if (!VinRegex.IsMatch(vin))
                throw new BusinessException("VIN должен состоять из 17 символов: латинские буквы (кроме I, O, Q) и цифры.");
        }

        var maxYear = DateTime.Now.Year + 1;
        if (car.Year < 1950 || car.Year > maxYear)
            throw new BusinessException($"Год выпуска должен быть в диапазоне 1950-{maxYear}.");

        if (string.IsNullOrWhiteSpace(car.EngineType))
            throw new BusinessException("Выберите тип двигателя.");

        if (car.Mileage < 0 || car.Mileage > 5_000_000)
            throw new BusinessException("Пробег должен быть неотрицательным числом (не более 5 000 000 км).");
    }

    public static void ValidateService(Service s)
    {
        Required(s.Name, "Наименование услуги", 200);
        if (!string.IsNullOrWhiteSpace(s.Description) && s.Description.Trim().Length > 500)
            throw new BusinessException("Описание не может быть длиннее 500 символов.");
        if (s.Price < 0) throw new BusinessException("Стоимость услуги не может быть отрицательной.");
        if (s.Price > MaxMoney) throw new BusinessException("Стоимость услуги слишком велика.");
    }

    public static void ValidatePart(Part p)
    {
        Required(p.Article, "Артикул", 50);
        Required(p.Name, "Наименование", 200);
        if (p.Price < 0) throw new BusinessException("Цена не может быть отрицательной.");
        if (p.Price > MaxMoney) throw new BusinessException("Цена слишком велика.");
        if (p.StockQuantity < 0) throw new BusinessException("Остаток на складе не может быть отрицательным.");
    }

    public static void ValidateEmployee(Employee e)
    {
        Required(e.LastName, "Фамилия", 50);
        Required(e.FirstName, "Имя", 50);
        Required(e.Position, "Должность", 50);
        if (!string.IsNullOrWhiteSpace(e.MiddleName) && e.MiddleName.Trim().Length > 50)
            throw new BusinessException("Отчество не может быть длиннее 50 символов.");

        var phone = e.Phone?.Trim() ?? "";
        var digits = phone.Count(char.IsDigit);
        if (phone.Length == 0) throw new BusinessException("Поле «Телефон» обязательно для заполнения.");
        if (!PhoneCharsRegex.IsMatch(phone) || digits < 10 || digits > 12 || phone.Length > 20)
            throw new BusinessException("Некорректный телефон. Пример правильного формата: +7 900 123-45-67.");

        if (!Roles.All.Contains(e.Role)) throw new BusinessException("Выберите роль сотрудника.");

        if (!string.IsNullOrWhiteSpace(e.Login))
        {
            var login = e.Login.Trim();
            if (login.Length < 3 || login.Length > 50 || login.Any(char.IsWhiteSpace))
                throw new BusinessException("Логин: от 3 до 50 символов без пробелов.");
        }
    }

    public static bool IsValidEmail(string? email) =>
        !string.IsNullOrWhiteSpace(email) && email.Trim().Length <= 254 && EmailRegex.IsMatch(email.Trim());

    private static void Required(string? value, string field, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new BusinessException($"Поле «{field}» обязательно для заполнения.");
        if (value.Trim().Length > maxLength)
            throw new BusinessException($"Поле «{field}» не может быть длиннее {maxLength} символов.");
    }
}
