using ClosedXML.Excel;
using GymManagement.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Helpers
{
    public static class SalaryStatementExcelExporter
    {
        private const int TotalColumns = 32;

        private const int EmpStart = 1;
        private const int EmpEnd = 5;
        private const int AttStart = 6;
        private const int AttEnd = 19;
        private const int GrossStart = 20;
        private const int GrossEnd = 29;
        private const int PayStart = 30;
        private const int PayEnd = 32;

        private const int ColTotalDays = 19;
        private const int ColGross = 27;
        private const int ColNetPay = 31;

        private static readonly XLColor TealHeader = XLColor.FromHtml("#0F5C6B");
        private static readonly XLColor HighlightFill = XLColor.FromHtml("#B8E8E8");

        private static readonly string[] ColumnHeaders =
        {
            "EmpCode", "Employee", "Company", "Department", "Designation",
            "P", "A", "WO", "WOP", "H", "HP", "OD", "PL", "CL", "COFF", "SL", "OT HRS", "ML", "TotalDays",
            "BASIC", "BASICDA", "HRA", "FIX_INCEN", "OTHERS", "Monthly I", "SpecialAll", "Gross", "EBASIC", "EBASICDA",
            "Deduction", "Net Pay", "Status"
        };

        public static FileContentResult Export(SalaryStatementReportViewModel report)
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("SalaryStatement");

            WriteTopRightHeader(ws, report);

            const int groupRow = 4;
            const int headerRow = 5;
            var dataStartRow = 6;

            WriteGroupHeader(ws, groupRow, EmpStart, EmpEnd, "Employee Detail");
            WriteGroupHeader(ws, groupRow, AttStart, AttEnd, "Attendance Detail");
            WriteGroupHeader(ws, groupRow, GrossStart, GrossEnd, "Fixed Gross");
            WriteGroupHeader(ws, groupRow, PayStart, PayEnd, "Pay Summary");

            for (var col = 1; col <= TotalColumns; col++)
            {
                var cell = ws.Cell(headerRow, col);
                cell.Value = ColumnHeaders[col - 1];
                StyleHeaderCell(cell);

                if (col == ColTotalDays || col == ColGross || col == ColNetPay)
                    cell.Style.Fill.BackgroundColor = HighlightFill;
            }

            var row = dataStartRow;
            foreach (var r in report.Rows)
            {
                WriteRow(ws, row, r);
                row++;
            }

            if (report.Rows.Count > 0)
            {
                var totalRow = row;
                ws.Cell(totalRow, EmpEnd).Value = "TOTAL";
                ws.Cell(totalRow, EmpEnd).Style.Font.Bold = true;
                ws.Cell(totalRow, ColGross).Value = report.TotalGross;
                ws.Cell(totalRow, ColGross).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(totalRow, ColGross).Style.Font.Bold = true;
                ws.Cell(totalRow, PayStart).Value = report.TotalDeductions;
                ws.Cell(totalRow, PayStart).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(totalRow, ColNetPay).Value = report.TotalNet;
                ws.Cell(totalRow, ColNetPay).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(totalRow, ColNetPay).Style.Font.Bold = true;
                StyleTotalRow(ws, totalRow);
                row++;
            }

            var tableEndRow = Math.Max(dataStartRow, row - 1);
            ws.Range(groupRow, 1, tableEndRow, TotalColumns).Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
            ws.Range(groupRow, 1, tableEndRow, TotalColumns).Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            ws.SheetView.FreezeRows(headerRow);
            ws.Columns(1, TotalColumns).AdjustToContents(groupRow, tableEndRow);
            foreach (var col in ws.Columns(1, TotalColumns))
            {
                if (col.Width < 8)
                    col.Width = 8;
                if (col.Width > 22)
                    col.Width = 22;
            }

            ws.Column(2).Width = Math.Max(ws.Column(2).Width, 18);
            ws.Column(3).Width = Math.Max(ws.Column(3).Width, 14);

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            return new FileContentResult(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                FileDownloadName = $"SalaryStatement_{report.MonthLabel}.xlsx"
            };
        }

        private static void WriteTopRightHeader(IXLWorksheet ws, SalaryStatementReportViewModel report)
        {
            const int headerStartCol = 20;
            var title = $"Salary Statement Report For the Month Of {report.MonthLabel}";

            MergeHeaderRight(ws, 1, headerStartCol, TotalColumns, report.CompanyName, 14, true);
            if (!string.IsNullOrWhiteSpace(report.CompanyAddress))
                MergeHeaderRight(ws, 2, headerStartCol, TotalColumns, report.CompanyAddress, 10, false);
            MergeHeaderRight(ws, 3, headerStartCol, TotalColumns, title, 11, true);
        }

        private static void MergeHeaderRight(
            IXLWorksheet ws,
            int row,
            int colStart,
            int colEnd,
            string text,
            double fontSize,
            bool bold)
        {
            var range = ws.Range(row, colStart, row, colEnd);
            range.Merge();
            var cell = range.FirstCell();
            cell.Value = text;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            cell.Style.Font.FontSize = fontSize;
            cell.Style.Font.Bold = bold;
        }

        private static void WriteGroupHeader(IXLWorksheet ws, int row, int colStart, int colEnd, string title)
        {
            var range = ws.Range(row, colStart, row, colEnd);
            range.Merge();
            var cell = range.FirstCell();
            cell.Value = title;
            cell.Style.Font.Bold = true;
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Fill.BackgroundColor = TealHeader;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        }

        private static void StyleHeaderCell(IXLCell cell)
        {
            cell.Style.Font.Bold = true;
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Fill.BackgroundColor = TealHeader;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            cell.Style.Alignment.WrapText = true;
        }

        private static void WriteRow(IXLWorksheet ws, int row, SalaryStatementRowViewModel r)
        {
            SetText(ws, row, 1, r.EmpCode);
            SetText(ws, row, 2, r.EmployeeName);
            SetText(ws, row, 3, r.Company);
            SetText(ws, row, 4, r.Department);
            SetText(ws, row, 5, r.Designation);

            SetInt(ws, row, 6, r.PresentDays);
            SetInt(ws, row, 7, r.AbsentDays);
            SetInt(ws, row, 8, r.WeeklyOffDays);
            SetInt(ws, row, 9, 0);
            SetInt(ws, row, 10, 0);
            SetInt(ws, row, 11, 0);
            SetInt(ws, row, 12, 0);
            SetInt(ws, row, 13, r.LeaveDays);
            SetInt(ws, row, 14, 0);
            SetInt(ws, row, 15, 0);
            SetInt(ws, row, 16, 0);
            SetText(ws, row, 17, r.OtHours);
            SetInt(ws, row, 18, 0);
            SetDecimal(ws, row, 19, r.TotalDays, highlight: true);

            SetMoney(ws, row, 20, r.Basic);
            SetMoney(ws, row, 21, 0);
            SetMoney(ws, row, 22, 0);
            SetMoney(ws, row, 23, 0);
            SetMoney(ws, row, 24, 0);
            SetMoney(ws, row, 25, 0);
            SetMoney(ws, row, 26, 0);
            SetMoney(ws, row, 27, r.Gross, highlight: true);
            SetMoney(ws, row, 28, r.EarnedGross);
            SetMoney(ws, row, 29, 0);

            SetMoney(ws, row, 30, r.Deductions);
            SetMoney(ws, row, 31, r.NetPay, highlight: true);
            SetText(ws, row, 32, string.IsNullOrWhiteSpace(r.PaymentStatus) ? "Pending" : r.PaymentStatus);
        }

        private static void StyleTotalRow(IXLWorksheet ws, int row)
        {
            var range = ws.Range(row, 1, row, TotalColumns);
            range.Style.Fill.BackgroundColor = XLColor.FromHtml("#F1F5F9");
            range.Style.Font.Bold = true;
        }

        private static void SetText(IXLWorksheet ws, int row, int col, string? value)
        {
            ws.Cell(row, col).Value = value ?? "";
        }

        private static void SetInt(IXLWorksheet ws, int row, int col, int value)
        {
            ws.Cell(row, col).Value = value;
            ws.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        private static void SetDecimal(IXLWorksheet ws, int row, int col, decimal value, bool highlight = false)
        {
            var cell = ws.Cell(row, col);
            cell.Value = value;
            cell.Style.NumberFormat.Format = "0.##";
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell.Style.Font.Bold = true;
            if (highlight)
                cell.Style.Fill.BackgroundColor = HighlightFill;
        }

        private static void SetMoney(IXLWorksheet ws, int row, int col, decimal value, bool highlight = false)
        {
            var cell = ws.Cell(row, col);
            cell.Value = value;
            cell.Style.NumberFormat.Format = "#,##0.00";
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            if (highlight)
            {
                cell.Style.Fill.BackgroundColor = HighlightFill;
                cell.Style.Font.Bold = true;
            }
        }
    }
}
