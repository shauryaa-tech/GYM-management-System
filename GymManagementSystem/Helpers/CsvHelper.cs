using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Helpers
{
    public static class CsvHelper
    {
        public static FileContentResult ToFileResult(string fileName, IReadOnlyList<string> headers, IEnumerable<IReadOnlyList<string?>> rows)
        {
            var sb = new StringBuilder();
            sb.AppendLine(string.Join(",", headers.Select(Escape)));
            foreach (var row in rows)
            {
                sb.AppendLine(string.Join(",", row.Select(v => Escape(v ?? ""))));
            }

            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
            return new FileContentResult(bytes, "text/csv; charset=utf-8")
            {
                FileDownloadName = fileName
            };
        }

        public static List<Dictionary<string, string>> Parse(Stream stream)
        {
            using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
            var content = reader.ReadToEnd();
            return ParseText(content);
        }

        public static List<Dictionary<string, string>> ParseText(string content)
        {
            var result = new List<Dictionary<string, string>>();
            var rows = ParseRows(content);
            if (rows.Count == 0)
                return result;

            var headers = rows[0]
                .Select(h => h.Trim())
                .ToArray();

            for (int i = 1; i < rows.Count; i++)
            {
                var cells = rows[i];
                if (cells.All(string.IsNullOrWhiteSpace))
                    continue;

                var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                for (int c = 0; c < headers.Length; c++)
                {
                    if (string.IsNullOrWhiteSpace(headers[c]))
                        continue;
                    dict[headers[c]] = c < cells.Count ? cells[c].Trim() : "";
                }
                result.Add(dict);
            }

            return result;
        }

        private static List<List<string>> ParseRows(string content)
        {
            var rows = new List<List<string>>();
            var row = new List<string>();
            var cell = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < content.Length; i++)
            {
                char ch = content[i];

                if (inQuotes)
                {
                    if (ch == '"')
                    {
                        if (i + 1 < content.Length && content[i + 1] == '"')
                        {
                            cell.Append('"');
                            i++;
                        }
                        else
                        {
                            inQuotes = false;
                        }
                    }
                    else
                    {
                        cell.Append(ch);
                    }
                    continue;
                }

                if (ch == '"')
                {
                    inQuotes = true;
                }
                else if (ch == ',')
                {
                    row.Add(cell.ToString());
                    cell.Clear();
                }
                else if (ch == '\r')
                {
                    // skip
                }
                else if (ch == '\n')
                {
                    row.Add(cell.ToString());
                    cell.Clear();
                    rows.Add(row);
                    row = new List<string>();
                }
                else
                {
                    cell.Append(ch);
                }
            }

            if (cell.Length > 0 || row.Count > 0)
            {
                row.Add(cell.ToString());
                rows.Add(row);
            }

            return rows;
        }

        private static string Escape(string value)
        {
            if (value.Contains('"') || value.Contains(',') || value.Contains('\n') || value.Contains('\r'))
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            return value;
        }
    }

    public static class CsvRowExtensions
    {
        public static string GetValue(this Dictionary<string, string> row, params string[] keys)
        {
            foreach (var key in keys)
            {
                if (row.TryGetValue(key, out var val) && !string.IsNullOrWhiteSpace(val))
                    return val.Trim();
            }
            return "";
        }

        public static int? GetInt(this Dictionary<string, string> row, params string[] keys)
        {
            var val = row.GetValue(keys);
            return int.TryParse(val, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) ? n : null;
        }

        public static decimal? GetDecimal(this Dictionary<string, string> row, params string[] keys)
        {
            var val = row.GetValue(keys);
            return decimal.TryParse(val, NumberStyles.Number, CultureInfo.InvariantCulture, out var n) ? n : null;
        }

        public static bool GetBool(this Dictionary<string, string> row, bool defaultValue, params string[] keys)
        {
            var val = row.GetValue(keys).ToLowerInvariant();
            if (val is "1" or "true" or "yes" or "y" or "active") return true;
            if (val is "0" or "false" or "no" or "n" or "inactive") return false;
            return defaultValue;
        }

        public static DateTime? GetDate(this Dictionary<string, string> row, params string[] keys)
        {
            var val = row.GetValue(keys);
            if (DateTime.TryParse(val, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
                return d;
            if (DateTime.TryParse(val, CultureInfo.CurrentCulture, DateTimeStyles.None, out d))
                return d;
            return null;
        }

        public static TimeSpan? GetTime(this Dictionary<string, string> row, params string[] keys)
        {
            var val = row.GetValue(keys);
            return TimeSpan.TryParse(val, CultureInfo.InvariantCulture, out var t) ? t : null;
        }
    }
}
