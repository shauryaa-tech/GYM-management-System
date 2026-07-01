using GymManagement.ViewModels;

namespace GymManagement.Services.Interfaces
{
    public interface IModuleCsvService
    {
        CsvFileData BuildExport(string module, IQueryCollection query);
        CsvImportResult Import(string module, Stream stream);
    }

    public class CsvFileData
    {
        public byte[] Content { get; set; } = Array.Empty<byte>();
        public string FileName { get; set; } = "export.csv";
        public string ContentType { get; set; } = "text/csv; charset=utf-8";
    }
}
