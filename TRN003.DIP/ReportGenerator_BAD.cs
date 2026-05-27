// TRN-003.DIP · ReportGenerator — Before & After
// ──────────────────────────────────────────────────────────────
// THE VIOLATION:
//   ReportGenerator_BAD creates its dependencies using 'new' inside the class.
//   This makes it:
//     UNTESTABLE  — unit tests must hit real SMTP, SQL Server, and disk
//     INFLEXIBLE  — swapping the email provider requires editing ReportGenerator
//     OPAQUE      — callers can't see what infrastructure this class uses
//
// DIP RULES:
//   a) High-level modules must not depend on low-level modules. Both depend on abstractions.
//   b) Abstractions must not depend on details. Details depend on abstractions.
//
// PRACTICAL TRANSLATION:
//   Define an interface for every external dependency.
//   Inject the concrete implementation through the constructor.
//   ReportGenerator never knows which implementation it received — it only calls the interface.
//
// REAL-WORLD PAYOFF:
//   In production: DI container injects SmtpEmailSender, SqlServerRepo, DiskFileWriter.
//   In tests:      inject FakeEmailSender, FakeReportRepository, FakeFileWriter.
//   ReportGenerator code is identical in both environments.

namespace TRN003.DIP;

// =============================================================================
// ── Low-level concrete classes (simulate real infrastructure) ─────────────────
// =============================================================================

public class SmtpEmailSender
{
    public void SendEmail(string to, string subject, string body)
        => Console.WriteLine($"[SMTP] → {to}: {subject}");
}

public class SqlServerReportRepository
{
    public List<ReportData> GetReportsForMonth(int year, int month)
    {
        Console.WriteLine($"[DB] Querying reports for {year}-{month:D2}");
        return new List<ReportData>
        {
            new("Total Loans",     142),
            new("New Members",      23),
            new("Overdue Returns",   7),
        };
    }

    public void SaveReport(string name, string content)
        => Console.WriteLine($"[DB] Saving report '{name}'");
}

public class DiskFileWriter
{
    public void WriteFile(string path, string content)
        => Console.WriteLine($"[FILE] Writing {content.Length} chars to {path}");
}

//public record ReportData(string MetricName, int Value);

// =============================================================================
// ── BEFORE (BAD) — 'new' dependencies inside the class ───────────────────────
// =============================================================================

/// <summary>
/// HIGH-LEVEL class that DIRECTLY instantiates low-level infrastructure.
/// ❌ Cannot be unit-tested without real DB, SMTP, and disk.
/// ❌ Changing the email provider means changing THIS class.
/// </summary>
public class ReportGenerator_BAD
{
    // ❌ DIP VIOLATIONS — concrete classes instantiated with 'new':
    private readonly SmtpEmailSender _emailSender          = new();
    private readonly SqlServerReportRepository _repository  = new();
    private readonly DiskFileWriter _fileWriter             = new();

    private readonly string _recipientEmail;

    public ReportGenerator_BAD(string recipientEmail)
        => _recipientEmail = recipientEmail;

    public void GenerateMonthlyReport(int year, int month)
    {
        Console.WriteLine($"Generating report for {year}-{month:D2}...");

        var data    = _repository.GetReportsForMonth(year, month);
        var content = BuildContent(year, month, data);

        _fileWriter.WriteFile($"reports/{year}-{month:D2}-monthly.txt", content);
        _repository.SaveReport($"Monthly-{year}-{month:D2}", content);
        _emailSender.SendEmail(_recipientEmail, $"Monthly Library Report — {year}-{month:D2}", content);

        Console.WriteLine("Report generated.\n");
    }

    private string BuildContent(int year, int month, List<ReportData> data)
    {
        var lines = new List<string>
        {
            $"=== LibraryHub Monthly Report: {year}-{month:D2} ===",
            $"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC", ""
        };
        foreach (var d in data)
            lines.Add($"  {d.MetricName,-25}: {d.Value}");
        return string.Join(Environment.NewLine, lines);
    }
}

// =============================================================================
// ── AFTER (YOUR SOLUTION) ────────────────────────────────────────────────────
// =============================================================================
//
// STEP 1: Define three interfaces:
//         IEmailSender      — SendEmail(string to, string subject, string body)
//         IReportRepository — GetReportsForMonth(int year, int month) : List<ReportData>
//                             SaveReport(string name, string content)
//         IFileWriter       — WriteFile(string path, string content)
//
// STEP 2: Make the existing concrete classes implement your interfaces:
//         SmtpEmailSender           : IEmailSender
//         SqlServerReportRepository : IReportRepository
//         DiskFileWriter            : IFileWriter
//
// STEP 3: Refactor ReportGenerator to:
//         - Accept IEmailSender, IReportRepository, IFileWriter via constructor.
//         - Remove ALL 'new' keywords for dependencies.
//         - Not reference any concrete type — only the interfaces.
//
// STEP 4: Create FAKE implementations for testing (in-memory, no I/O):
//
//         FakeEmailSender:
//           List<(string To, string Subject, string Body)> SentEmails
//           Implement SendEmail: add to SentEmails
//
//         FakeReportRepository:
//           List<ReportData> returns hardcoded data (same as SqlServerReportRepository above)
//           List<(string Name, string Content)> SavedReports
//
//         FakeFileWriter:
//           List<(string Path, string Content)> WrittenFiles
//           Implement WriteFile: add to WrittenFiles
//
// STEP 5: In Program.cs, wire up ReportGenerator with fake dependencies and
//         uncomment the tests. All assertions use the fake objects' in-memory lists.
//
// Write your solution below:
// ─────────────────────────────────────────────────────────────────────────────


public interface IEmailSender
{
    void SendsEmail(string to, string subject, string body);
}

public interface IReportRepository
{
    List<ReportData> GetReportForMonth(int year, int month);
    void SaveReport(string name, string content);
}
public interface IFileWriter
{
    void WriteFile(string path, string content);
}

public class SmtpEmailsSender : IEmailSender
{
    public void SendsEmail(string to, string subject, string body)
    {
        Console.WriteLine($"[SMTP] → {to}: {subject}");
    }
}

public class SqlServerReportsRepository : IReportRepository
{
    public List<ReportData> GetReportForMonth(int year, int month)
    {
        Console.WriteLine($"[DB] Querying reports for {year}-{month:D2}");
        return new List<ReportData>
        {
            new("Total Loans",     142),
            new("New Members",      23),
            new("Overdue Returns",   7),
        };
    }

    public void SaveReport(string name, string content)
    {
        Console.WriteLine($"[DB] Saving report '{name}'");
    }
}

public class DiskFilesWriter : IFileWriter
{
    public void WriteFile(string path, string content)
    {
        Console.WriteLine($"[FILE] Writing {content.Length} chars to {path}");
    }
}

public record ReportData(string MetricName, int Value);


public class ReportGenerator
{
    private readonly IEmailSender _emailSender;
    private readonly IReportRepository _repository;
    private readonly IFileWriter _fileWriter;

    private readonly string _recipientEmail;

    public ReportGenerator(
            string recipientEmail,
            IEmailSender emailSender,
            IReportRepository repository,
            IFileWriter fileWriter)
    {
            _emailSender = emailSender;
            _repository = repository;
            _fileWriter = fileWriter;
            _recipientEmail = recipientEmail;
    }

    public void GenerateMonthlyReport(int year, int month)
    {
        Console.WriteLine($"Generating report for {year}-{month:D2}...");

        var data = _repository.GetReportForMonth(year, month);

        var content = BuildContent(year, month, data);

        _fileWriter.WriteFile(
            $"reports/{year}-{month:D2}-monthly.txt",
            content);

        _repository.SaveReport(
            $"Monthly-{year}-{month:D2}",
            content);

        _emailSender.SendsEmail(
            _recipientEmail,
            $"Monthly Library Report — {year}-{month:D2}",
            content);

        Console.WriteLine("Report generated.\n");

    }

    private string BuildContent(
            int year,
            int month,
            List<ReportData> data)
    {
        var lines = new List<string>
        {
            $"=== LibraryHub Monthly Report: {year}-{month:D2} ===",
            $"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC",
            ""
        };

        foreach (var d in data)
        {
            lines.Add($"  {d.MetricName,-25}: {d.Value}");
        }

        return string.Join(Environment.NewLine, lines);
    }
}

public class FakeEmailSender : IEmailSender
{
    public List<(string To, string Subject, string Body)> SentEmails
        = new();

    public void SendsEmail(string to, string subject, string body)
    {
        SentEmails.Add((to, subject, body));
    }
}

public class FakeReportRepository : IReportRepository
{
    public List<(string Name, string Content)> SavedReports
        = new();

    public List<ReportData> GetReportForMonth(int year, int month)
    {
        return new List<ReportData>
        {
            new("Total Loans", 142),
            new("New Members", 23),
            new("Overdue Returns", 7)
        };
    }

    public void SaveReport(string name, string content)
    {
        SavedReports.Add((name, content));
    }
}

public class FakeFileWriter : IFileWriter
{
    public List<(string Path, string Content)> WrittenFiles
        = new();

    public void WriteFile(string path, string content)
    {
        WrittenFiles.Add((path, content));
    }
}


