// TRN-003.LSP · File Hierarchy — Before & After
// ──────────────────────────────────────────────────────────────
// THE VIOLATIONS (two bugs — find and fix both):
//
//   VIOLATION A — ReadOnlyFile_BAD throws NotSupportedException on Write()/Delete().
//     Code that accepts a WritableFile_BAD and calls Write() CRASHES at runtime
//     if it receives a ReadOnlyFile_BAD. The caller cannot safely substitute one for the other.
//
//   VIOLATION B — TemporaryFile_BAD overrides Delete() to silently do nothing.
//     Code that calls Delete() and expects cleanup will be silently broken.
//     No exception, no feedback.
//
// HOW TO SPOT LSP VIOLATIONS:
//   ❌ Subclass throws NotSupportedException / NotImplementedException for an inherited method.
//   ❌ Subclass overrides a method with an empty body (no-op) to "block" it.
//   ❌ Callers need  if (file is ReadOnlyFile_BAD)  before calling Write().
//   ❌ Comments like "// only call this on writable files!"
//
// THE FIX — think about your interface boundaries:
//   "What do ALL files have in common?"       → IReadableFile
//   "What only WRITABLE files can do?"         → IWritableFile (extends IReadableFile)
//
//   Then: ReadableFile  : IReadableFile           (no Write, no Delete)
//         WritableFile  : IWritableFile            (can do everything)
//         TemporaryFile : IWritableFile            (can write; Delete clears content)
//
//   With this design, AppendTimestamp(IWritableFile file) physically cannot accept
//   a ReadableFile — the compiler rejects it at build time.

using Microsoft.VisualBasic.FileIO;
using System.Reflection.Metadata.Ecma335;
using System.Xml.Serialization;

namespace TRN003.LSP;

// =============================================================================
// ── BEFORE (BAD) — two LSP violations ────────────────────────────────────────
// =============================================================================

public class WritableFile_BAD
{
    protected string _path;
    protected string _content;

    public WritableFile_BAD(string path, string initialContent = "")
    {
        _path    = path;
        _content = initialContent;
    }

    public virtual string Read()   => _content;
    public virtual long   GetSize() => _content.Length;
    public virtual string GetPath() => _path;

    public virtual void Write(string content)
    {
        _content = content;
        Console.WriteLine($"[FILE] Written {content.Length} bytes to {_path}");
    }

    public virtual void Delete()
    {
        _content = "";
        Console.WriteLine($"[FILE] Deleted {_path}");
    }
}

/// <summary>
/// ❌ VIOLATION A: Throws on Write() and Delete().
///    Any caller using WritableFile_BAD references will crash at runtime.
/// </summary>
public class ReadOnlyFile_BAD : WritableFile_BAD
{
    public ReadOnlyFile_BAD(string path, string content) : base(path, content) { }

    public override void Write(string content)
        => throw new NotSupportedException("This file is read-only.");

    public override void Delete()
        => throw new NotSupportedException("This file is read-only.");
}

/// <summary>
/// ❌ VIOLATION B: Delete() is a silent no-op — callers can't rely on it.
/// </summary>
public class TemporaryFile_BAD : WritableFile_BAD
{
    public TemporaryFile_BAD(string path) : base(path, "") { }

    public override void Delete()
    {
        // Does nothing observable — no logging, content not cleared, no notification.
        // A caller that expects the file to be cleaned up is silently wrong.
    }
}

/// <summary>Code that SHOULD work for all files — crashes with ReadOnlyFile_BAD.</summary>
public static class FileProcessor_BAD
{
    public static string ReadAndUppercase(WritableFile_BAD file)
        => file.Read().ToUpper();

    public static void AppendTimestamp(WritableFile_BAD file)
    {
        var existing = file.Read();
        file.Write(existing + $"\n[{DateTime.UtcNow:O}]");   // ← crashes on ReadOnlyFile_BAD
    }
}

// =============================================================================
// ── AFTER (YOUR SOLUTION) ────────────────────────────────────────────────────
// =============================================================================
//
// STEP 1: Define interface IReadableFile
//         Methods: string Read(),  long GetSize(),  string GetPath()
//
// STEP 2: Define interface IWritableFile : IReadableFile
//         Additional methods: void Write(string content),  void Delete()
//
// STEP 3: Implement concrete classes:
//         ReadableFile   : IReadableFile   — no Write/Delete
//         WritableFile   : IWritableFile   — full implementation
//         TemporaryFile  : IWritableFile   — Delete() clears content and logs
//
// STEP 4: Refactor FileProcessor:
//         ReadAndUppercase  accepts IReadableFile   (ALL file types)
//         AppendTimestamp   accepts IWritableFile   (compile-time safety)
//
// PROVE IT WORKS:
//   Uncomment the "// FileProcessor.AppendTimestamp(myReadable);" line in Program.cs.
//   It should be a compile error — ReadableFile does not implement IWritableFile.
//
// Write your solution below:
// ─────────────────────────────────────────────────────────────────────────────


public interface IReadableFile
{
    string Read();
    long GetSize();
    string GetPath();

}

public interface IWritableFile : IReadableFile
{
    public void Write(string content);

    public void Delete();
}

public class ReadableFile : IReadableFile
{
    private string _path;
    private string _content;

    public ReadableFile(string path, string content)
    {
        _path = path;
        _content = content;

    }

    public string Read()
    {
        return _content;
    }
    public long GetSize()
    {
        return _content.Length;

    }

    public string GetPath()
    {
        return _path;
    }

}

public class WritableFile : IWritableFile
{
    private string _path;
    private string _content;

    public WritableFile(string path, string content)
    {
        _path = path;
        _content = content;

    }
    public string Read()
    {
        return _content;
    }
    public long GetSize()
    {
        return _content.Length;
    }
    public string GetPath()
    {
        return _path;
    }
    public void Write(string content)
    {
        _content = content;
    }
    public void Delete()
    {
        _content = "";
    }
}

public class TemporaryFile : IWritableFile
{
    private string _path;
    private string _content;
    


    public TemporaryFile(string path)
    {
        _path = path;
        _content = "";
    }
    public string Read()
    {
        return _content;
    }
    public long GetSize()
    {
        return _content.Length;
    }
    public string GetPath()
    {
        return _path;
    }
    public void Write(string content)
    {
        _content = content;

    }
    public void Delete()
    {
        _content = "";
    }

}

public static class FileProcessor
{
    public static string ReadAndUppercase(IReadableFile file)
    {
       return file.Read().ToUpper();
    }

    public static void AppendTimestamp(IWritableFile file)
    {
        var existing = ReadAndUppercase(file);
        var updated = existing + $"\n[{DateTime.UtcNow:O}]";
        file.Write(updated);
    }
}


