// TRN-002 · Exercise 06: LINQ
// ──────────────────────────────────────────────────────────────
// LEARNING GOALS:
//   • Write expressive queries using LINQ method syntax.
//   • Understand deferred execution, projection, grouping, and pagination.
//   • Avoid imperative foreach loops in favour of declarative LINQ chains.
//
// RULE: Every TODO method must be a single LINQ expression (no foreach/for loops).

using TRN002;

namespace TRN002.Exercises;

public static class Linq
{
    // ── Sample data ────────────────────────────────────────────────────────────
    private static readonly IReadOnlyList<LibraryBook> Books = new List<LibraryBook>
    {
        new(1,  "Clean Code",                    "Martin",     2008, "Programming",  4.7m, 312),
        new(2,  "The Pragmatic Programmer",       "Hunt",       1999, "Programming",  4.8m, 352),
        new(3,  "Refactoring",                   "Fowler",     2018, "Programming",  4.6m, 448),
        new(4,  "Design Patterns",               "GoF",        1994, "Architecture", 4.5m, 395),
        new(5,  "Domain-Driven Design",          "Evans",      2003, "Architecture", 4.4m, 560),
        new(6,  "The Phoenix Project",           "Kim",        2013, "Management",   4.6m, 382),
        new(7,  "Accelerate",                    "Forsgren",   2018, "Management",   4.5m, 288),
        new(8,  "A Philosophy of Software Design","Ousterhout",2018, "Programming",  4.3m, 190),
        new(9,  "Working Effectively with Legacy Code","Feathers",2004,"Programming",4.4m, 464),
        new(10, "Continuous Delivery",           "Humble",     2010, "DevOps",       4.5m, 512),
        new(11, "Site Reliability Engineering",  "Beyer",      2016, "DevOps",       4.4m, 552),
        new(12, "The Mythical Man-Month",        "Brooks",     1975, "Management",   4.2m, 336),
        new(13, "Code Complete",                 "McConnell",  2004, "Programming",  4.6m, 914),
        new(14, "Introduction to Algorithms",    "Cormen",     2009, "Academic",     4.5m, 1292),
        new(15, "You Don't Know JS",             "Simpson",    2015, "Programming",  4.3m, 278),
        new(16, "The Clean Coder",               "Martin",     2011, "Programming",  4.4m, 256),
        new(17, "Pragmatic Unit Testing",        "Hunt",       2003, "Programming",  4.3m, 256),
        new(18, "Release It!",                   "Nygard",     2018, "Architecture", 4.5m, 376),
        new(19, "Building Microservices",        "Newman",     2015, "Architecture", 4.5m, 616),
        new(20, "Software Engineering at Google","Winters",    2020, "Engineering",  4.6m, 602),
    };

    public static void Run()
    {
        // 1 — Filter
        var prog = GetBooksInCategory("Programming");
        Assert.AreEqual(9, prog.Count(), "There are 9 Programming books in the data set");

        // 2 — Sort
        var byRating = GetBooksSortedByRatingDescending();
        Assert.AreEqual("The Pragmatic Programmer", byRating.First().Title, "Highest rated book first");
        Assert.AreEqual("The Mythical Man-Month",   byRating.Last().Title,  "Lowest rated book last");

        // 3 — Projection
        var titles = GetTitlesPublishedAfter(2010);
        Assert.IsTrue(titles.Contains("Accelerate"),              "Accelerate (2018) should be included");
        Assert.IsFalse(titles.Contains("Clean Code"),             "Clean Code (2008) should be excluded");
        Assert.IsTrue(titles.All(t => t is string && t.Length > 0), "All projected values must be strings");

        // 4 — FirstOrDefault
        var martin = FindFirstByAuthor("Martin");
        Assert.IsNotNull(martin,              "Martin has books in the data set");
        Assert.AreEqual("Martin", martin!.Author);
        Assert.IsNull(FindFirstByAuthor("Tolkien"), "Tolkien has no books — must return null");

        // 5 — Aggregation
        var avg = GetAverageRating("Architecture");
        Assert.IsTrue(avg > 4m && avg < 5m,   "Average Architecture rating must be between 4 and 5");
        Assert.AreEqual(0m, GetAverageRating("NonExistentCategory"),
            "Average of an empty set should return 0 (not throw)");

        // 6 — Grouping
        var grouped = GroupByCategory();
        Assert.IsTrue(grouped.ContainsKey("Programming"),  "Must have a Programming group");
        Assert.IsTrue(grouped.ContainsKey("DevOps"),       "Must have a DevOps group");
        Assert.AreEqual(9, grouped["Programming"].Count,   "Programming group must have 9 books");

        // 7 — Pagination  (Books sorted by Id, 1-based page numbers)
        var page1 = GetPage(1, 5);
        var page2 = GetPage(2, 5);
        Assert.AreEqual(5, page1.Count(),  "Page 1 should contain 5 items");
        Assert.AreEqual(5, page2.Count(),  "Page 2 should contain 5 items");
        Assert.AreEqual(1, page1.First().Id, "Page 1 first item should be book Id=1");
        Assert.AreEqual(6, page2.First().Id, "Page 2 first item should be book Id=6");
    }

    // ── TODO methods ──────────────────────────────────────────────────────────

    // TODO: Return books whose Category exactly matches 'category'.
    public static IEnumerable<LibraryBook> GetBooksInCategory(string category)
        => Books.Where(b => b.Category == category);

    // TODO: Sort by Rating descending, then Title ascending as tiebreaker.
    public static IEnumerable<LibraryBook> GetBooksSortedByRatingDescending()
        => Books.OrderByDescending(b => b.Rating).ThenBy(b => b.Title);

    // TODO: Return only the TITLE STRINGS of books published strictly after 'year'.
    public static IEnumerable<string> GetTitlesPublishedAfter(int year)
        => Books.Where(b => b.Year > year).Select(b => b.Title);

    // TODO: Return the first book by this author, or null.
    public static LibraryBook? FindFirstByAuthor(string author)
        => Books.FirstOrDefault(b => b.Author == author);

    // TODO: Average Rating of books in category. Return 0m if category is empty.
    // HINT: .Where(...).DefaultIfEmpty() then .Average()
    //   OR: check .Any() first.
    public static decimal GetAverageRating(string category)
        => Books.All(b => b.Category != category) ? 0m : Books.Where(b => b.Category == category).Average(b => b.Rating);

    // TODO: Group books by Category. Return Dictionary<string, List<LibraryBook>>.
    // HINT: .GroupBy(b => b.Category).ToDictionary(g => g.Key, g => g.ToList())
    public static Dictionary<string, List<LibraryBook>> GroupByCategory()
        => Books.GroupBy(b => b.Category).ToDictionary(a => a.Key, g => g.ToList());

    // TODO: Return books sorted by Id, skip (pageNumber-1)*pageSize, take pageSize.
    // pageNumber is 1-based.
    public static IEnumerable<LibraryBook> GetPage(int pageNumber, int pageSize)
        => Books.OrderBy(b => b.Id).Skip((pageNumber - 1) * pageSize).Take(pageSize);
}

// ── Supporting record — DO NOT change ────────────────────────────────────────
public record LibraryBook(int Id, string Title, string Author, int Year, string Category, decimal Rating, int Pages);
