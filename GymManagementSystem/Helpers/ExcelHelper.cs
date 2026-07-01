using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Helpers
{
    public enum ExcelCellKind
    {
        Text,
        Integer,
        Decimal,
        Currency,
        Date
    }

    public readonly record struct ExcelColumnSpec(string Header, ExcelCellKind Kind = ExcelCellKind.Text);

    public readonly record struct ExcelReportHeader(
        string CompanyName,
        string? CompanyAddress = null,
        string? ReportTitle = null,
        string? PeriodLabel = null);

    public static class ExcelHelper
    {
        public static FileContentResult ToFileResult(
            string fileName,
            string sheetName,
            IReadOnlyList<ExcelColumnSpec> columns,
            IEnumerable<object?[]> rows) =>
            ToFileResult(fileName, sheetName, null, columns, rows);

        public static FileContentResult ToFileResult(
            string fileName,
            string sheetName,
            ExcelReportHeader? reportHeader,
            IReadOnlyList<ExcelColumnSpec> columns,
            IEnumerable<object?[]> rows)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(sheetName);

            var headerRow = reportHeader == null
                ? 1
                : WriteReportHeader(worksheet, reportHeader.Value, columns.Count);

            for (var col = 0; col < columns.Count; col++)
            {
                var headerCell = worksheet.Cell(headerRow, col + 1);
                headerCell.Value = columns[col].Header;
                headerCell.Style.Font.Bold = true;
                headerCell.Style.Font.FontColor = XLColor.White;
                headerCell.Style.Fill.BackgroundColor = XLColor.FromHtml("#2563EB");
                headerCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                headerCell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            }

            var rowIndex = headerRow + 1;
            foreach (var row in rows)
            {
                for (var col = 0; col < columns.Count; col++)
                {
                    var value = col < row.Length ? row[col] : null;
                    ApplyValue(worksheet.Cell(rowIndex, col + 1), value, columns[col].Kind);
                }

                rowIndex++;
            }

            worksheet.Row(headerRow).Height = 22;
            worksheet.SheetView.FreezeRows(headerRow);
            worksheet.Range(headerRow, 1, Math.Max(headerRow, rowIndex - 1), columns.Count).SetAutoFilter();

            var lastDataRow = Math.Max(headerRow + 1, rowIndex - 1);
            worksheet.Columns(1, columns.Count).AdjustToContents(1, lastDataRow);
            foreach (var column in worksheet.Columns(1, columns.Count))
            {
                if (column.Width < 11)
                    column.Width = 11;
                if (column.Width > 48)
                    column.Width = 48;
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            var downloadName = fileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase)
                ? fileName
                : fileName + ".xlsx";

            return new FileContentResult(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                FileDownloadName = downloadName
            };
        }

        private static int WriteReportHeader(IXLWorksheet worksheet, ExcelReportHeader header, int columnCount)
        {
            var row = 1;
            var colSpan = Math.Max(columnCount, 4);

            MergeHeaderLine(worksheet, row, colSpan, header.CompanyName, line =>
            {
                line.Style.Font.Bold = true;
                line.Style.Font.FontSize = 16;
                line.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            });
            row++;

            if (!string.IsNullOrWhiteSpace(header.CompanyAddress))
            {
                MergeHeaderLine(worksheet, row, colSpan, header.CompanyAddress!, line =>
                {
                    line.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    line.Style.Font.FontColor = XLColor.FromHtml("#64748B");
                });
                row++;
            }

            if (!string.IsNullOrWhiteSpace(header.ReportTitle))
            {
                MergeHeaderLine(worksheet, row, colSpan, header.ReportTitle!, line =>
                {
                    line.Style.Font.Bold = true;
                    line.Style.Font.FontSize = 12;
                    line.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                });
                row++;
            }

            if (!string.IsNullOrWhiteSpace(header.PeriodLabel))
            {
                MergeHeaderLine(worksheet, row, colSpan, header.PeriodLabel!, line =>
                {
                    line.Style.Font.Bold = true;
                    line.Style.Font.FontSize = 12;
                    line.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    line.Style.Font.FontColor = XLColor.FromHtml("#1E40AF");
                });
                row++;
            }

            row++;
            return row;
        }

        private static void MergeHeaderLine(
            IXLWorksheet worksheet,
            int row,
            int columnCount,
            string text,
            Action<IXLRange> style)
        {
            var range = worksheet.Range(row, 1, row, columnCount);
            range.Merge();
            range.FirstCell().Value = text;
            style(range);
        }

        private static void ApplyValue(IXLCell cell, object? value, ExcelCellKind kind)
        {
            if (value == null || value == DBNull.Value)
                return;

            switch (kind)
            {
                case ExcelCellKind.Integer:
                    cell.Value = Convert.ToInt32(value);
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    break;

                case ExcelCellKind.Decimal:
                    cell.Value = Convert.ToDecimal(value);
                    cell.Style.NumberFormat.Format = "0.00";
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    break;

                case ExcelCellKind.Currency:
                    cell.Value = Convert.ToDecimal(value);
                    cell.Style.NumberFormat.Format = "#,##0.00";
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    break;

                case ExcelCellKind.Date:
                    if (value is DateTime dateTime)
                        cell.Value = dateTime;
                    else if (DateTime.TryParse(value.ToString(), out var parsed))
                        cell.Value = parsed;
                    else
                        cell.Value = value.ToString();
                    cell.Style.NumberFormat.Format = "dd-MM-yyyy";
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    break;

                default:
                    cell.Value = value.ToString();
                    break;
            }
        }
    }
}
