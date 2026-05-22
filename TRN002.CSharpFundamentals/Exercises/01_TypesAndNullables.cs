// TRN-002 · Exercise 01: Value Types, Reference Types & Nullables
// ──────────────────────────────────────────────────────────────
// LEARNING GOALS:
//   • Understand the copy-vs-reference behaviour of value vs reference types.
//   • Use nullable value types (int?) and the ??, ?.  operators.
//   • Understand nullable reference types and the compiler null checks.
//
// RULES:
//   • Implement every method marked // TODO.
//   • Do NOT change method signatures or the Run() method.
//   • If a test prints [FAIL], read the hint — it tells you exactly what went wrong.

using TRN002;

namespace TRN002.Exercises;

public static class TypesAndNullables
{
    public static void Run()
    {
        // ── Value type: copy behaviour ────────────────────────────────────────
        int original = 42;
        int copy = original;
        copy = 99;
        Assert.AreEqual(42, original,
            "Value types are copied by value. Changing 'copy' must NOT change 'original'.");

        // ── Reference type: shared reference ─────────────────────────────────
        var bookA = new BookRef("Clean Code");
        var bookB = bookA;          // both point to the SAME heap object
        bookB.Title = "Refactoring";
        Assert.AreEqual("Refactoring", bookA.Title,
            "Reference types share the same object. Changing via bookB MUST affect bookA.");

        // ── Nullable value type ───────────────────────────────────────────────
        Assert.AreEqual(18, GetAgeOrDefault(null),   "null age should fall back to 18");
        Assert.AreEqual(25, GetAgeOrDefault(25),     "non-null age should be returned as-is");

        // ── Nullable reference type ───────────────────────────────────────────
        Assert.AreEqual("Anonymous", GetDisplayName(null),    "null name → 'Anonymous'");
        Assert.AreEqual("",          GetDisplayName(""),      "empty string → 'Anonymous'? Check the requirement again.");
        Assert.AreEqual("Alice",     GetDisplayName("Alice"), "non-null name → returned as-is");

        // ── Struct copy behaviour ─────────────────────────────────────────────
        var p1 = new Point2D(1, 2);
        var p2 = p1;                // structs copy by value
        ModifyPoint(ref p2);
        Assert.AreEqual(1,  p1.X, "Modifying p2 must NOT affect p1 (struct = value copy)");
        Assert.AreEqual(10, p2.X, "p2.X should be 10 after ModifyPoint");
    }

    // TODO: Return 'defaultAge' when 'age' is null; otherwise return the age.
    // HINT: Use the null-coalescing operator  ??
    private static int GetAgeOrDefault(int? age, int defaultAge = 18)
    {
        //if (age == null)
        //{
        //    return defaultAge;
        //}
        //else
        //{
        //    return (int)age;
        //}
        return age ?? defaultAge;
    }

    // TODO: Return "Anonymous" when 'name' is null or empty; otherwise return name.
    // HINT: string.IsNullOrEmpty(name)  combined with a ternary  ? :
    private static string GetDisplayName(string? name)
    {
        return name == null ? "Anonymous" : name;
    }


    // ── Helpers (do not change) ───────────────────────────────────────────────
    private static void ModifyPoint(ref Point2D p) => p = new Point2D(10, 20);
}

// Supporting types ────────────────────────────────────────────────────────────

public class BookRef                          // reference type (class)
{
    public string Title { get; set; }
    public BookRef(string title) => Title = title;
}

public readonly struct Point2D                // value type (struct)
{
    public int X { get; }
    public int Y { get; }
    public Point2D(int x, int y) { X = x; Y = y; }
}
