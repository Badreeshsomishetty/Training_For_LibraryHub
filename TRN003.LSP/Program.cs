// ============================================================
// TRN-003 · Exercise 03 — Liskov Substitution Principle
// ============================================================
// HOW TO RUN:  Set TRN003.LSP as Startup Project → F5
// ============================================================

using TRN003.LSP;

Console.WriteLine("╔══════════════════════════════════════════════╗");
Console.WriteLine("║   TRN-003.LSP · Liskov Substitution          ║");
Console.WriteLine("╚══════════════════════════════════════════════╝\n");

// ── PART 1: Watch the violation happen ───────────────────────────────────────
Console.WriteLine("── Demonstrating LSP violations in the BAD hierarchy ──\n");

var writable  = new WritableFile_BAD("notes.txt", "Hello World");
var readOnly  = new ReadOnlyFile_BAD("readme.txt", "Read Only Content");

// ReadAndUppercase SHOULD work for ALL files — watch it crash on ReadOnlyFile:
Try("ReadAndUppercase(writable)",  () => Console.WriteLine($"  Result: '{FileProcessor_BAD.ReadAndUppercase(writable)}'"));
Try("ReadAndUppercase(readOnly)",  () => Console.WriteLine($"  Result: '{FileProcessor_BAD.ReadAndUppercase(readOnly)}'"));
Try("AppendTimestamp(readOnly)",   () => FileProcessor_BAD.AppendTimestamp(readOnly));

Console.WriteLine();
Console.WriteLine("── Now fix the hierarchy in FileHierarchy_BAD.cs ──");
Console.WriteLine("── Uncomment tests below as you build your solution ──\n");

// ── PART 2: Your refactored solution ─────────────────────────────────────────


var myReadable = new ReadableFile("config.txt", "hello world");
var myWritable = new WritableFile("data.txt", "some content");
var myTemp     = new TemporaryFile("temp_01.txt");

// TEST 1: ReadableFile works with ReadAndUppercase
Verify(FileProcessor.ReadAndUppercase(myReadable) == "HELLO WORLD",
    "TEST 1: ReadableFile readable via interface");

// TEST 2: WritableFile also readable (it's also a readable file)
Verify(FileProcessor.ReadAndUppercase(myWritable) == "SOME CONTENT",
    "TEST 2: WritableFile readable via interface");

// TEST 3: AppendTimestamp on WritableFile
FileProcessor.AppendTimestamp(myWritable);
Verify(myWritable.Read().Contains("SOME CONTENT"), "TEST 3a: original content preserved");
Verify(myWritable.Read().Contains("Z") || myWritable.Read().Contains("+"),
    "TEST 3b: ISO timestamp appended");

// TEST 4: Compile-time safety — the line below must NOT compile.
// Uncomment it — if your design is correct, you'll get a compiler error,
// proving LSP is enforced at compile time, not runtime.
//FileProcessor.AppendTimestamp(myReadable);   // ← should NOT compile
Console.WriteLine("  [PASS] TEST 4: ReadableFile cannot be passed to AppendTimestamp (compile-time)");

// TEST 5: TemporaryFile write and delete
myTemp.Write("temporary content");
Verify(myTemp.Read() == "temporary content", "TEST 5a: TemporaryFile writable");
myTemp.Delete();
Verify(myTemp.Read() == "", "TEST 5b: TemporaryFile content cleared after delete");

// TEST 6: No NotSupportedException from any type in the hierarchy
var allReadable = new IReadableFile[] { myReadable, myWritable, myTemp };
bool noExceptions = true;
foreach (var f in allReadable)
{
    try { f.Read(); }
    catch (NotSupportedException) { noExceptions = false; }
}
Verify(noExceptions, "TEST 6: No NotSupportedException from any readable file");



static void Try(string label, Action action)
{
    try
    {
        action();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"  [OK]       {label}");
    }
    catch (NotSupportedException ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"  [VIOLATION] {label} threw NotSupportedException: {ex.Message}");
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"  [ERR]      {label}: {ex.GetType().Name}: {ex.Message}");
    }
    finally { Console.ResetColor(); }
}

static void Verify(bool ok, string msg)
{
    Console.ForegroundColor = ok ? ConsoleColor.Green : ConsoleColor.Red;
    Console.WriteLine($"  [{(ok ? "PASS" : "FAIL")}] {msg}");
    Console.ResetColor();
}
