// TRN-002 · Exercise 03: Interfaces & Polymorphism
// ──────────────────────────────────────────────────────────────
// LEARNING GOALS:
//   • Define and implement interfaces.
//   • Work with interface references (polymorphism without inheritance).
//   • Understand why interfaces are the foundation of Dependency Injection.
//
// KEY INSIGHT:
//   An interface is a CONTRACT — a list of things an object promises to do.
//   Code that works with the interface doesn't care WHICH class it receives,
//   only that the class honours the contract.

using TRN002;

namespace TRN002.Exercises;

public static class InterfacesAndPolymorphism
{
    public static void Run()
    {
        // All three notify via the same interface — no if/switch needed
        INotifiable[] notifiers =
        {
            new EmailNotifier("alice@library.com"),
            new SmsNotifier("+91-9876543210"),
            new ConsoleNotifier()
        };

        foreach (var n in notifiers)
        {
            // Calling through the interface — runtime dispatch to the correct class
            string result = n.Notify("Your reserved book is now available.");
            Assert.IsTrue(result.Length > 0,
                $"{n.GetType().Name}.Notify() must return a non-empty confirmation string");
        }

        // ── Sorting strategy: demonstrate IComparer<T> as an interface ────────
        var books = new List<SortableBook>
        {
            new("Refactoring",               4.6m, 2018),
            new("Clean Code",                4.7m, 2008),
            new("The Pragmatic Programmer",  4.8m, 1999),
            new("Domain-Driven Design",      4.4m, 2003),
        };

        // TODO: Sort books by Rating DESCENDING using RatingDescendingComparer.
        //       Use books.Sort(new RatingDescendingComparer()) or List.Sort overload.
        //       Then assert the first book has the highest rating.
        //throw new NotImplementedException("Sort books and assert ordering");

        books.Sort(new RatingDescendingComparer());

        // After sorting:
        Assert.AreEqual("The Pragmatic Programmer", books[0].Title,
            "Highest-rated book should be first after sort");
        Assert.AreEqual("Domain-Driven Design", books[3].Title,
            "Lowest-rated book should be last");
    }
}

// ── Notification interface — DO NOT change ────────────────────────────────────

public interface INotifiable
{
    /// <summary>Sends a notification and returns a delivery confirmation string.</summary>
    string Notify(string message);

    /// <summary>Human-readable description of this notifier's channel.</summary>
    string Channel { get; }
}

// ── Implementations — complete the TODOs ─────────────────────────────────────

public class EmailNotifier : INotifiable
{
    private readonly string _email;
    public EmailNotifier(string email) => _email = email;

    // TODO: Implement Channel property — return "Email"
    public string Channel => "Email";

    // TODO: Implement Notify — simulate sending an email.
    //       Return a confirmation like "Email sent to alice@library.com"
    public string Notify(string message)
    {
        return $"Email sent to {_email}";
    }

}

public class SmsNotifier : INotifiable
{
    private readonly string _phoneNumber;
    public SmsNotifier(string phoneNumber) => _phoneNumber = phoneNumber;

    // TODO: Implement Channel — return "SMS"
    public string Channel => "SMS";

    // TODO: Implement Notify — return "SMS sent to +91-9876543210"
    public string Notify(string message) => $"SMS sent to {_phoneNumber}";
}

public class ConsoleNotifier : INotifiable
{
    // TODO: Implement Channel — return "Console"
    public string Channel => "Console";

    // TODO: Implement Notify — write the message to Console and
    //       return "Console notification displayed"
    public string Notify(string message)
    {
        Console.WriteLine(message);

        return "Console notification displayed";
    }
}

// ── Sorting interface exercise ────────────────────────────────────────────────

public record SortableBook(string Title, decimal Rating, int Year);

// TODO: Implement RatingDescendingComparer : IComparer<SortableBook>
//       Compare two books: the one with the HIGHER rating should come first.
//       If ratings are equal, sort by Title ascending as a tiebreaker.
//
//       IComparer<T> contract:
//         int Compare(T? x, T? y)
//         Returns: negative if x < y,  0 if equal,  positive if x > y
//       For descending: return y.Rating.CompareTo(x.Rating)  (swap x and y)
//
// Write RatingDescendingComparer below:
public class RatingDescendingComparer : IComparer<SortableBook>
{
    public int Compare(SortableBook? x, SortableBook? y)
    {
        int ratingCompare = y.Rating.CompareTo(x.Rating);
        
        if (ratingCompare != 0)
        {
            return ratingCompare;
        }

        return x.Title.CompareTo(y.Title);

    }
}

