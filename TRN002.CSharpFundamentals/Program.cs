// ============================================================
// TRN-002 — C# Fundamentals & OOP Foundations
// Exercise Runner
// ============================================================
// HOW TO RUN:
//   Right-click TRN002.CSharpFundamentals in Solution Explorer → Set as Startup Project
//   Press F5 or Ctrl+F5
//
// GOAL: All six lines should print [PASS].
// When a method is not yet implemented you will see [TODO].
// When your implementation is wrong you will see [FAIL] with a hint.
// ============================================================

using TRN002.Exercises;

Console.WriteLine("╔══════════════════════════════════════════════╗");
Console.WriteLine("║   TRN-002 · C# Fundamentals & OOP           ║");
Console.WriteLine("╚══════════════════════════════════════════════╝");
Console.WriteLine();

Run("01 · Types & Nullables",        () => TypesAndNullables.Run());
Run("02 · Class Hierarchy",          () => ClassHierarchy.Run());
Run("03 · Interfaces & Polymorphism",() => InterfacesAndPolymorphism.Run());
Run("04 · Generics",                 () => Generics.Run());
Run("05 · Encapsulation", () => Encapsulation.Run());
Run("06 · LINQ", () => Linq.Run());

Console.WriteLine();
Console.WriteLine("════════════════════════════════════════════════");

static void Run(string name, Action exercise)
{
    try
    {
        exercise();
        Console.WriteLine($"  [PASS] {name}");
    }
    catch (NotImplementedException ex)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"  [TODO] {name}  ← {ex.Message}");
        Console.ResetColor();
    }
    catch (TRN002.AssertionException ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"  [FAIL] {name}");
        Console.WriteLine($"         {ex.Message}");
        Console.ResetColor();
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"  [ERR]  {name}  ← {ex.GetType().Name}: {ex.Message}");
        Console.ResetColor();
    }
}
