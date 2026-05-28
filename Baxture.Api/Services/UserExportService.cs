using System.Globalization;
using System.Text;
using Baxture.Api.Dtos;
using Baxture.Api.Models;

namespace Baxture.Api.Services;

public sealed class UserExportService : IUserExportService
{
    public ExportResult Export(IReadOnlyCollection<User> users, string format) =>
        format.Equals("pdf", StringComparison.OrdinalIgnoreCase)
            ? ExportPdf(users)
            : format.Equals("excel", StringComparison.OrdinalIgnoreCase) || format.Equals("xlsx", StringComparison.OrdinalIgnoreCase)
                ? ExportExcel(users)
                : throw new InvalidOperationException("Export format must be PDF or EXCEL.");

    private static ExportResult ExportExcel(IReadOnlyCollection<User> users)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"Current Date,{DateTime.UtcNow:yyyy-MM-dd}");
        builder.AppendLine("Id,Username,IsAdmin,Age,Hobbies");
        foreach (var user in users)
        {
            builder.AppendLine(string.Join(',', [
                EscapeCsv(user.Id),
                EscapeCsv(user.Username),
                user.IsAdmin.ToString(CultureInfo.InvariantCulture),
                user.Age.ToString(CultureInfo.InvariantCulture),
                EscapeCsv(string.Join('|', user.Hobbies))
            ]));
        }

        builder.AppendLine("Page,1");
        return new ExportResult(Encoding.UTF8.GetBytes(builder.ToString()), "text/csv", "users-export.csv");
    }

    private static ExportResult ExportPdf(IReadOnlyCollection<User> users)
    {
        var lines = new List<string> { $"Current Date: {DateTime.UtcNow:yyyy-MM-dd}", "" };
        lines.AddRange(users.Select(user =>
            $"{user.Id} | {user.Username} | Admin: {user.IsAdmin} | Age: {user.Age} | Hobbies: {string.Join(", ", user.Hobbies)}"));
        lines.Add("");
        lines.Add("Page 1");

        var streamText = string.Join("\\n", lines.Select(EscapePdfText));
        var content = $"BT /F1 10 Tf 50 760 Td ({streamText}) Tj ET";
        var pdf = BuildSinglePagePdf(content);
        return new ExportResult(pdf, "application/pdf", "users-export.pdf");
    }

    private static byte[] BuildSinglePagePdf(string content)
    {
        var objects = new List<string>
        {
            "<< /Type /Catalog /Pages 2 0 R >>",
            "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
            "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>",
            "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>",
            $"<< /Length {Encoding.ASCII.GetByteCount(content)} >>\nstream\n{content}\nendstream"
        };

        var builder = new StringBuilder("%PDF-1.4\n");
        var offsets = new List<int> { 0 };
        foreach (var item in objects.Select((value, index) => new { value, index }))
        {
            offsets.Add(Encoding.ASCII.GetByteCount(builder.ToString()));
            builder.Append(CultureInfo.InvariantCulture, $"{item.index + 1} 0 obj\n{item.value}\nendobj\n");
        }

        var xrefOffset = Encoding.ASCII.GetByteCount(builder.ToString());
        builder.Append(CultureInfo.InvariantCulture, $"xref\n0 {objects.Count + 1}\n");
        builder.Append("0000000000 65535 f \n");
        foreach (var offset in offsets.Skip(1))
        {
            builder.Append(CultureInfo.InvariantCulture, $"{offset:0000000000} 00000 n \n");
        }

        builder.Append(CultureInfo.InvariantCulture, $"trailer << /Size {objects.Count + 1} /Root 1 0 R >>\nstartxref\n{xrefOffset}\n%%EOF");
        return Encoding.ASCII.GetBytes(builder.ToString());
    }

    private static string EscapeCsv(string value) =>
        value.Contains(',') || value.Contains('"') || value.Contains('\n')
            ? $"\"{value.Replace("\"", "\"\"")}\""
            : value;

    private static string EscapePdfText(string value) =>
        value.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");
}
