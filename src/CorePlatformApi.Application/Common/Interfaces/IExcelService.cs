namespace CorePlatformApi.Application.Common.Interfaces;

/// <summary>
/// Một cột của file Excel: tiêu đề là KHÓA DỊCH (dịch theo ngôn ngữ của request khi xuất file).
/// Thứ tự cột trong danh sách = thứ tự cột trong file mẫu / file xuất / file nhập.
/// </summary>
/// <param name="HeaderKey">Khóa dịch của tiêu đề cột (dùng chung với tiêu đề cột trên bảng giao diện).</param>
/// <param name="Value">Lấy giá trị ô từ một dòng dữ liệu (bỏ qua khi tạo file mẫu).</param>
/// <param name="Width">Độ rộng cột (ký tự).</param>
/// <param name="Required">Cột bắt buộc khi nhập — tiêu đề có dấu <c>*</c>.</param>
/// <param name="NumberFormat">Định dạng số/ngày của Excel, VD <c>#,##0</c>, <c>dd/MM/yyyy</c>; <c>@</c> = văn bản (giữ số 0 đầu).</param>
/// <param name="ListValues">Giá trị được phép — file tạo ra có dropdown chọn ở cột này (VD mã loại, Có/Không).</param>
public sealed record ExcelColumn<T>(
    string HeaderKey,
    Func<T, object?> Value,
    double Width = 20,
    bool Required = false,
    string? NumberFormat = null,
    IReadOnlyList<string>? ListValues = null);

/// <summary>
/// Một dòng dữ liệu đọc từ file Excel (đã bỏ dòng tiêu đề và dòng trống).
/// </summary>
/// <param name="RowNumber">Số dòng thật trong Excel (để báo lỗi "Dòng 5: ...").</param>
/// <param name="Cells">Giá trị từng ô theo thứ tự cột, đã trim.</param>
public sealed record ExcelRow(int RowNumber, IReadOnlyList<string> Cells)
{
    public string this[int column] => column < Cells.Count ? Cells[column] : string.Empty;
}

/// <summary>
/// Tạo / đọc file Excel (.xlsx) dùng chung cho mọi màn hình: file mẫu, xuất dữ liệu, nhập dữ liệu.
/// </summary>
public interface IExcelService
{
    /// <summary>
    /// Tạo file .xlsx: dòng 1 là tiêu đề (đã dịch, cột bắt buộc có *), các dòng sau là dữ liệu.
    /// <paramref name="rows"/> rỗng = file mẫu để nhập.
    /// </summary>
    /// <param name="sheetNameKey">Khóa dịch của tên sheet (VD tên đối tượng).</param>
    Task<byte[]> WriteAsync<T>(
        string sheetNameKey,
        IReadOnlyList<ExcelColumn<T>> columns,
        IEnumerable<T> rows,
        CancellationToken ct = default);

    /// <summary>
    /// Đọc sheet đầu tiên: dòng 1 là tiêu đề, trả các dòng dữ liệu có ít nhất một ô khác rỗng.
    /// Kiểm tra số cột tiêu đề tối thiểu = <paramref name="expectedColumns"/>; file sai định dạng ném AppException.
    /// </summary>
    IReadOnlyList<ExcelRow> Read(Stream stream, int expectedColumns, int maxRows = 10_000);
}
