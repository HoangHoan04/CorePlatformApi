namespace CorePlatformApi.Application.Common.Exceptions;

/// <summary>
/// Lỗi nghiệp vụ dự kiến (dữ liệu không hợp lệ, không tìm thấy, trùng mã...):
/// WebApi trả về { code, message } với HTTP status tương ứng, không ghi log stack trace.
/// </summary>
/// <remarks>
/// <paramref name="message"/> nên là khóa dịch (VD <c>mdm.country.codeExists</c>, khai báo trong
/// <see cref="Localization.MessageKeys"/>) — bộ lọc lỗi ở WebApi dịch theo ngôn ngữ của request,
/// thay <c>{{tham_số}}</c> bằng <paramref name="args"/>. Chuỗi không phải khóa được trả nguyên văn.
/// </remarks>
public class AppException(int statusCode, string code, string message, object? args = null) : Exception(message)
{
    public int StatusCode { get; } = statusCode;

    public string Code { get; } = code;

    /// <summary>
    /// Tham số thay vào <c>{{tên}}</c> trong bản dịch (object ẩn danh hoặc dictionary).
    /// </summary>
    public object? Args { get; } = args;

    public static AppException BadRequest(string code, string message, object? args = null) => new(400, code, message, args);

    public static AppException NotFound(string message, object? args = null) => new(404, "NOT_FOUND", message, args);

    public static AppException Conflict(string code, string message, object? args = null) => new(409, code, message, args);

    public static AppException Forbidden(string message, object? args = null) => new(403, "FORBIDDEN", message, args);
}
