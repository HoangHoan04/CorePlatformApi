using System.Globalization;

namespace CorePlatformApi.Application.Common.Excel;

/// <summary>
/// Chuyển giá trị ô Excel (chuỗi) sang kiểu dữ liệu khi nhập file — dùng chung cho mọi màn hình.
/// </summary>
public static class ExcelValues
{
    /// <summary>
    /// Ô trống -> <paramref name="defaultValue"/>. Chấp nhận true/false, 1/0, x, và chữ "Có"/"Không"
    /// theo ngôn ngữ đang dùng (<paramref name="yesText"/>/<paramref name="noText"/> lấy từ bản dịch common.yes/common.no).
    /// </summary>
    public static bool TryParseBool(string value, string yesText, string noText, bool defaultValue, out bool result)
    {
        result = defaultValue;
        var v = value.Trim();
        if (v.Length == 0)
        {
            return true;
        }

        if (v.Equals(yesText, StringComparison.OrdinalIgnoreCase) || v is "1" or "x" or "X"
            || v.Equals("true", StringComparison.OrdinalIgnoreCase))
        {
            result = true;
            return true;
        }

        if (v.Equals(noText, StringComparison.OrdinalIgnoreCase) || v == "0"
            || v.Equals("false", StringComparison.OrdinalIgnoreCase))
        {
            result = false;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Ô trống -> <paramref name="defaultValue"/>; chấp nhận "1.000" / "1,000" / "1000".
    /// </summary>
    public static bool TryParseInt(string value, int defaultValue, out int result)
    {
        result = defaultValue;
        var v = value.Trim();
        if (v.Length == 0)
        {
            return true;
        }

        return int.TryParse(v.Replace(".", string.Empty).Replace(",", string.Empty), NumberStyles.Integer,
            CultureInfo.InvariantCulture, out result);
    }

    private static readonly string[] DateFormats =
        ["yyyy-MM-dd", "dd/MM/yyyy", "d/M/yyyy", "dd-MM-yyyy", "d-M-yyyy", "dd.MM.yyyy", "yyyy/MM/dd"];

    /// <summary>
    /// Ô trống -> null. Chấp nhận ngày dạng ISO (ô kiểu ngày được ExcelService đọc ra yyyy-MM-dd) hoặc dd/MM/yyyy.
    /// </summary>
    public static bool TryParseDate(string value, out DateOnly? result)
    {
        result = null;
        var v = value.Trim();
        if (v.Length == 0)
        {
            return true;
        }

        if (DateOnly.TryParseExact(v, DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
        {
            result = d;
            return true;
        }

        return false;
    }

    private static readonly string[] DateTimeFormats =
    [
        "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd HH:mm", "yyyy-MM-dd",
        "dd/MM/yyyy HH:mm:ss", "dd/MM/yyyy HH:mm", "dd/MM/yyyy", "d/M/yyyy H:mm", "d/M/yyyy",
    ];

    /// <summary>
    /// Ô trống -> null. Ngày giờ trong file hiểu theo giờ địa phương của máy chủ (giống khi xuất file).
    /// </summary>
    public static bool TryParseDateTime(string value, out DateTimeOffset? result)
    {
        result = null;
        var v = value.Trim();
        if (v.Length == 0)
        {
            return true;
        }

        if (DateTime.TryParseExact(v, DateTimeFormats, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var dt))
        {
            result = new DateTimeOffset(dt).ToUniversalTime();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Ô trống -> null; chấp nhận dấu thập phân "." hoặc ",".
    /// </summary>
    public static bool TryParseDecimal(string value, out decimal? result)
    {
        result = null;
        var v = value.Trim().Replace(',', '.');
        if (v.Length == 0)
        {
            return true;
        }

        if (decimal.TryParse(v, NumberStyles.Number, CultureInfo.InvariantCulture, out var d))
        {
            result = d;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Chuỗi rỗng -> null.
    /// </summary>
    public static string? NullIfEmpty(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
