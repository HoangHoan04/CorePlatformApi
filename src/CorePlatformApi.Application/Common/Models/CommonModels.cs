namespace CorePlatformApi.Application.Common.Models;

/// <summary>
/// Body chỉ chứa Id — dùng cho các hành động trên một bản ghi (kích hoạt, ngừng hoạt động...).
/// </summary>
public sealed class IdRequest
{
    public Guid Id { get; set; }
}

/// <summary>
/// Một lựa chọn cho ô chọn (dropdown) ở các màn hình khác: GET .../select-box.
/// </summary>
public sealed record SelectBoxItem(Guid Id, string Code, string Name);

/// <summary>
/// Kết quả nhập Excel. <see cref="Errors"/> đã dịch theo ngôn ngữ của request, mỗi dòng dạng "Dòng 5: ...".
/// </summary>
public sealed record ImportResult(int TotalRows, int SuccessCount, int ErrorCount, IReadOnlyList<string> Errors)
{
    /// <summary>
    /// Số bản ghi được tạo mới / cập nhật (trong <see cref="SuccessCount"/>).
    /// </summary>
    public int CreatedCount { get; init; }

    public int UpdatedCount { get; init; }
}

/// <summary>
/// Một lỗi kiểm tra dữ liệu: khóa dịch + tham số (dịch khi trả về client).
/// </summary>
public sealed record ValidationError(string Key, object? Args = null);
