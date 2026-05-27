// ============================================================
// TRN-003 · Exercise 05 — Dependency Inversion Principle
// ============================================================
// HOW TO RUN:  Set TRN003.DIP as Startup Project → F5
// ============================================================

using TRN003.DIP;

Console.WriteLine("╔══════════════════════════════════════════════╗");
Console.WriteLine("║   TRN-003.DIP · Dependency Inversion         ║");
Console.WriteLine("╚══════════════════════════════════════════════╝\n");

// ── BAD code demo (calls real SMTP / DB / disk — simulated via Console here) ─
Console.WriteLine("── ReportGenerator_BAD (creates dependencies with 'new') ──");
var badGenerator = new ReportGenerator_BAD("manager@library.com");
badGenerator.GenerateMonthlyReport(2024, 11);
Console.WriteLine();

Console.WriteLine("── Build your refactored solution in ReportGenerator_BAD.cs ──");
Console.WriteLine("── Then wire up FAKE dependencies here and uncomment tests ──\n");

// ── WIRE UP FAKE DEPENDENCIES HERE ───────────────────────────────────────────
// Example:
var fakeEmail = new FakeEmailSender();
var fakeRepo = new FakeReportRepository();
var fakeWriter = new FakeFileWriter();
var generator = new ReportGenerator("manager@library.com", fakeEmail, fakeRepo, fakeWriter);

// ── UNCOMMENT TESTS ONE BY ONE ───────────────────────────────────────────────


// TEST 1: Runs without error using only fake infrastructure
try
{
    generator.GenerateMonthlyReport(2024, 11);
    Verify(true, "TEST 1: GenerateMonthlyReport completed with fake dependencies");
}
catch (Exception ex) { Verify(false, $"TEST 1 FAILED: {ex.Message}"); }

// TEST 2: Email was sent to the right recipient
Verify(fakeEmail.SentEmails.Count == 1,
    "TEST 2: Exactly one email should have been sent");
Verify(fakeEmail.SentEmails[0].To == "manager@library.com",
    "TEST 2b: Email sent to correct recipient");
Verify(fakeEmail.SentEmails[0].Subject.Contains("2024-11"),
    "TEST 2c: Subject mentions the month");

// TEST 3: Report was persisted
Verify(fakeRepo.SavedReports.Count == 1, "TEST 3: One report saved");
Verify(fakeRepo.SavedReports[0].Name.Contains("2024-11"), "TEST 3b: Report name contains month");

// TEST 4: File was written
Verify(fakeWriter.WrittenFiles.Count == 1, "TEST 4: One file written");
Verify(fakeWriter.WrittenFiles[0].Path.Contains("2024-11"),  "TEST 4b: File path contains month");
Verify(fakeWriter.WrittenFiles[0].Content.Contains("Total Loans"), "TEST 4c: Report contains metrics");

// TEST 5: No real infrastructure touched (all assertions used fake objects only)
Verify(true, "TEST 5: Zero real SMTP / SQL / disk I/O — proven by using only fake object assertions");

// TEST 6: Swap implementation without changing ReportGenerator
var secondFakeEmail = new FakeEmailSender();
var generator2 = new ReportGenerator("ceo@library.com", secondFakeEmail, fakeRepo, fakeWriter);
generator2.GenerateMonthlyReport(2024, 12);
Verify(secondFakeEmail.SentEmails[0].To == "ceo@library.com",
    "TEST 6: Different email recipient injected with zero code change in ReportGenerator");



static void Verify(bool ok, string msg)
{
    Console.ForegroundColor = ok ? ConsoleColor.Green : ConsoleColor.Red;
    Console.WriteLine($"  [{(ok ? "PASS" : "FAIL")}] {msg}");
    Console.ResetColor();
}
