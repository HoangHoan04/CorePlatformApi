using ClosedXML.Excel;
using CorePlatformApi.Application.Common.Exceptions;
using CorePlatformApi.Application.Common.Interfaces;
using CorePlatformApi.Application.Common.Localization;

namespace CorePlatformApi.Infrastructure.Excel;

/// <summary>
/// Cài đặt <see cref="IExcelService"/> bằng ClosedXML. Tiêu đề cột / tên sheet dịch qua <see cref="IAppLocalizer"/>.
/// </summary>
public sealed class ExcelService(IAppLocalizer localizer) : IExcelService
{
    private static readonly XLColor HeaderBackground = XLColor.FromHtml("#1F4E78");
    private static readonly XLColor RequiredBackground = XLColor.FromHtml("#C00000");

    public async Task<byte[]> WriteAsync<T>(
        string sheetNameKey,
        IReadOnlyList<ExcelColumn<T>> columns,
        IEnumerable<T> rows,
        CancellationToken ct = default)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add(SafeSheetName(await localizer.GetAsync(sheetNameKey, ct: ct)));

        for (var c = 0; c < columns.Count; c++)
        {
            var column = columns[c];
            var header = await localizer.GetAsync(column.HeaderKey, ct: ct);
            var cell = sheet.Cell(1, c + 1);
            cell.Value = column.Required ? header + " *" : header;
            cell.Style.Font.Bold = true;
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Fill.BackgroundColor = column.Required ? RequiredBackground : HeaderBackground;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            cell.Style.Alignment.WrapText = true;

            var xlColumn = sheet.Column(c + 1);
            xlColumn.Width = column.Width;
            if (column.NumberFormat == "@")
            {
                // Cột văn bản: giữ nguyên số 0 đầu (mã số, điện thoại...) kể cả khi người dùng gõ vào file mẫu
                xlColumn.Style.NumberFormat.Format = "@";
            }
            else if (column.NumberFormat is not null)
            {
                xlColumn.Style.NumberFormat.Format = column.NumberFormat;
            }

            if (column.ListValues is { Count: > 0 } list)
            {
                // Dropdown chọn giá trị cho các dòng nhập liệu
                var validation = sheet.Range(2, c + 1, 5000, c + 1).CreateDataValidation();
                validation.List("\"" + string.Join(",", list.Select(v => v.Replace(",", " ").Replace("\"", "'"))) + "\"", true);
                validation.IgnoreBlanks = true;
            }
        }

        sheet.Row(1).Height = 30;

        var r = 2;
        foreach (var row in rows)
        {
            ct.ThrowIfCancellationRequested();
            for (var c = 0; c < columns.Count; c++)
            {
                var cell = sheet.Cell(r, c + 1);
                SetValue(cell, columns[c].Value(row));
                if (columns[c].NumberFormat is { } format)
                {
                    cell.Style.NumberFormat.Format = format;
                }
            }

            r++;
        }

        var range = sheet.Range(1, 1, Math.Max(1, r - 1), columns.Count);
        range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        range.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        sheet.SheetView.FreezeRows(1);
        if (r > 2)
        {
            sheet.Range(1, 1, r - 1, columns.Count).SetAutoFilter();
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public IReadOnlyList<ExcelRow> Read(Stream stream, int expectedColumns, int maxRows = 10_000)
    {
        XLWorkbook workbook;
        try
        {
            workbook = new XLWorkbook(stream);
        }
        catch (Exception)
        {
            throw AppException.BadRequest("IMPORT_INVALID_FILE", MessageKeys.Import.InvalidFile);
        }

        using (workbook)
        {
            var sheet = workbook.Worksheets.FirstOrDefault()
                        ?? throw AppException.BadRequest("IMPORT_EMPTY_FILE", MessageKeys.Import.EmptyFile);

            var headerCount = sheet.Row(1).LastCellUsed()?.Address.ColumnNumber ?? 0;
            if (headerCount < expectedColumns)
            {
                throw AppException.BadRequest("IMPORT_MISSING_COLUMNS", MessageKeys.Import.MissingColumns,
                    new { columns = expectedColumns });
            }

            var lastRow = sheet.LastRowUsed()?.RowNumber() ?? 1;
            var rows = new List<ExcelRow>();
            for (var r = 2; r <= lastRow; r++)
            {
                var cells = new string[expectedColumns];
                var hasValue = false;
                for (var c = 0; c < expectedColumns; c++)
                {
                    cells[c] = ReadCell(sheet.Cell(r, c + 1));
                    hasValue |= cells[c].Length > 0;
                }

                if (!hasValue)
                {
                    continue;
                }

                if (rows.Count >= maxRows)
                {
                    throw AppException.BadRequest("IMPORT_TOO_MANY_ROWS", MessageKeys.Import.TooManyRows, new { max = maxRows });
                }

                rows.Add(new ExcelRow(r, cells));
            }

            if (rows.Count == 0)
            {
                throw AppException.BadRequest("IMPORT_EMPTY_FILE", MessageKeys.Import.EmptyFile);
            }

            return rows;
        }
    }

    private static void SetValue(IXLCell cell, object? value)
    {
        switch (value)
        {
            case null:
                cell.Value = Blank.Value;
                break;
            case string s:
                cell.Value = s;
                break;
            case bool b:
                cell.Value = b;
                break;
            case DateTimeOffset dto:
                // Ghi theo giờ địa phương của máy chủ (khi nhập lại, ExcelValues.TryParseDateTime hiểu theo cùng múi giờ)
                cell.Value = dto.ToLocalTime().DateTime;
                break;
            case DateTime dt:
                cell.Value = dt;
                break;
            case DateOnly d:
                cell.Value = d.ToDateTime(TimeOnly.MinValue);
                break;
            case int or long or short or decimal or double or float:
                cell.Value = Convert.ToDouble(value);
                break;
            default:
                cell.Value = value.ToString();
                break;
        }
    }

    private static string ReadCell(IXLCell cell)
    {
        if (cell.IsEmpty())
        {
            return string.Empty;
        }

        // Số nguyên lưu dạng số (704) -> "704", không phải "704.0"; ngày -> định dạng hiển thị trong file
        return cell.DataType switch
        {
            XLDataType.Number when cell.GetDouble() is var d && Math.Abs(d % 1) < double.Epsilon => ((long)d).ToString(),
            XLDataType.Number => cell.GetDouble().ToString(System.Globalization.CultureInfo.InvariantCulture),
            XLDataType.Boolean => cell.GetBoolean() ? "true" : "false",
            // Ô kiểu ngày -> chuỗi ISO, không phụ thuộc định dạng hiển thị trong file
            XLDataType.DateTime => cell.GetDateTime() is var dt && dt.TimeOfDay == TimeSpan.Zero
                ? dt.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture)
                : dt.ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture),
            _ => cell.GetFormattedString().Trim(),
        };
    }

    private static string SafeSheetName(string name)
    {
        var invalid = new[] { ':', '\\', '/', '?', '*', '[', ']' };
        var clean = new string(name.Where(ch => !invalid.Contains(ch)).ToArray()).Trim();
        if (clean.Length == 0)
        {
            clean = "Sheet1";
        }

        return clean.Length > 31 ? clean[..31] : clean;
    }
}
