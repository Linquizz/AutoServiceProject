using System.Globalization;

namespace AutoService.Services;

public static class ParseHelper
{
    public static bool TryDecimal(string? text, out decimal value)
    {
        value = 0;
        if (string.IsNullOrWhiteSpace(text)) return false;
        var normalized = text.Trim().Replace(" ", "").Replace("\u00A0", "").Replace(',', '.');
        return decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out value);
    }

    public static bool TryInt(string? text, out int value)
    {
        value = 0;
        if (string.IsNullOrWhiteSpace(text)) return false;
        return int.TryParse(text.Trim().Replace(" ", ""), NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
    }

    public static string Money(decimal value) => value.ToString("N2", CultureInfo.GetCultureInfo("ru-RU"));
}
