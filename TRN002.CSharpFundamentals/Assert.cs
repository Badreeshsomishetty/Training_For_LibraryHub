namespace TRN002;

public class AssertionException : Exception
{
    public AssertionException(string message) : base(message) { }
}

public static class Assert
{
    public static void AreEqual<T>(T expected, T actual, string hint = "")
    {
        if (!Equals(expected, actual))
            throw new AssertionException(
                $"Expected [{expected}] but got [{actual}]" +
                (hint.Length > 0 ? $"\n         Hint: {hint}" : ""));
    }

    public static void IsTrue(bool condition, string hint = "")
    {
        if (!condition)
            throw new AssertionException(
                "Expected true" + (hint.Length > 0 ? $"\n         Hint: {hint}" : ""));
    }

    public static void IsFalse(bool condition, string hint = "")
    {
        if (condition)
            throw new AssertionException(
                "Expected false" + (hint.Length > 0 ? $"\n         Hint: {hint}" : ""));
    }

    public static void IsNull(object? value, string hint = "")
    {
        if (value is not null)
            throw new AssertionException(
                $"Expected null but got [{value}]" +
                (hint.Length > 0 ? $"\n         Hint: {hint}" : ""));
    }

    public static void IsNotNull(object? value, string hint = "")
    {
        if (value is null)
            throw new AssertionException(
                "Expected a non-null value but got null" +
                (hint.Length > 0 ? $"\n         Hint: {hint}" : ""));
    }

    public static void Throws<TException>(Action action, string hint = "") where TException : Exception
    {
        try
        {
            action();
            throw new AssertionException(
                $"Expected {typeof(TException).Name} to be thrown but no exception was thrown." +
                (hint.Length > 0 ? $"\n         Hint: {hint}" : ""));
        }
        catch (TException) { /* expected */ }
        catch (AssertionException) { throw; }
        catch (Exception ex)
        {
            throw new AssertionException(
                $"Expected {typeof(TException).Name} but got {ex.GetType().Name}: {ex.Message}");
        }
    }
}
