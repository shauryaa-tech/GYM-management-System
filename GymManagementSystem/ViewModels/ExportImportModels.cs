namespace GymManagement.ViewModels
{
    public class ExportImportToolbarViewModel
    {
        public string Module { get; set; } = "";
        public string Controller { get; set; } = "";
        public string ExportAction { get; set; } = "Export";
        public string ImportAction { get; set; } = "Import";
        public string RedirectAction { get; set; } = "Index";
        public bool AllowImport { get; set; }
        public string QueryString { get; set; } = "";
        public Dictionary<string, string> RouteValues { get; set; } = new();
        public bool CanExport { get; set; }
        public bool CanImport { get; set; }
        public bool ExportAsExcel { get; set; }
    }

    public class CsvImportResult
    {
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public List<string> Errors { get; set; } = new();

        public string Message =>
            FailedCount == 0
                ? $"{SuccessCount} record(s) imported successfully."
                : $"{SuccessCount} imported, {FailedCount} failed." +
                  (Errors.Count > 0 ? " " + string.Join(" | ", Errors.Take(3)) : "");
    }
}
