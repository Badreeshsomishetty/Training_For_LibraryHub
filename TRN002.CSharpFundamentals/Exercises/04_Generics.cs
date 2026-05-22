// TRN-002 · Exercise 04: Generics
// ──────────────────────────────────────────────────────────────
// LEARNING GOALS:
//   • Write a type-safe, reusable generic class.
//   • Use generic constraints (where T : IEntity) to restrict type parameters.
//   • Understand why generics eliminate duplication without losing type safety.
//
// KEY INSIGHT:
//   Without generics you would write BookRepository, MemberRepository, LoanRepository
//   with near-identical code. With generics, one Repository<T> handles all of them.

using System.Security.Cryptography.X509Certificates;
using TRN002;

namespace TRN002.Exercises;

public static class Generics
{
    public static void Run()
    {
        // ── Same repository works with different entity types ─────────────────
        IRepository<BookEntity> books = new InMemoryRepository<BookEntity>();
        books.Add(new BookEntity(1, "Clean Code"));
        books.Add(new BookEntity(2, "The Pragmatic Programmer"));
        books.Add(new BookEntity(3, "Refactoring"));

        Assert.AreEqual(3, books.GetAll().Count(), "Should have 3 books after adding 3");

        var found = books.GetById(2);
        Assert.IsNotNull(found,                              "Should find book with Id = 2");
        Assert.AreEqual("The Pragmatic Programmer", found!.Title);

        books.Delete(2);
        Assert.AreEqual(2,   books.GetAll().Count(), "Should have 2 books after deleting one");
        Assert.IsNull(books.GetById(2),              "Deleted book must not be findable");

        // ── Same Repository<T> with Members — zero code duplication ──────────
        IRepository<MemberEntity> members = new InMemoryRepository<MemberEntity>();
        members.Add(new MemberEntity(1, "Alice"));
        members.Add(new MemberEntity(2, "Bob"));

        Assert.AreEqual(2,       members.GetAll().Count());
        Assert.AreEqual("Alice", members.GetById(1)!.FullName);

        // ── GetAll() must return a defensive copy, not expose internals ───────
        var snapshot = books.GetAll().ToList();
        snapshot.Clear();  // clearing the returned list must NOT clear the repository
        Assert.AreEqual(2, books.GetAll().Count(),
            "Clearing the list returned by GetAll() must not affect the repository");
    }
}

// ── Entity marker interface — DO NOT change ───────────────────────────────────

/// <summary>All entities that can be stored in the repository must have an int Id.</summary>
public interface IEntity
{
    int Id { get; }
}

// ── TODO: Define IRepository<T> interface ────────────────────────────────────
//
// Constraint: T must implement IEntity  (where T : IEntity)
//
// Methods:
//   void           Add(T entity)
//   T?             GetById(int id)         ← return null if not found
//   IEnumerable<T> GetAll()               ← returns defensive copy
//   void           Delete(int id)          ← silently ignore if id not found
//
// Write IRepository<T> here:
public interface IRepository<T> where T : IEntity
{
    void Add(T entity);
    T? GetById(int id);
    IEnumerable<T> GetAll();
    void Delete(int id);
}
// ── TODO: Implement InMemoryRepository<T> ────────────────────────────────────
//
// • Use a private List<T> to store entities.
// • GetAll() returns a new list (defensive copy) so callers can't mutate internals.
// • GetById() uses LINQ .FirstOrDefault(e => e.Id == id)
// • Delete() removes the entity with the matching Id (no-op if not found).
//
// Write InMemoryRepository<T> here:
public class InMemoryRepository<T> : IRepository<T> where T : IEntity
{
    private List<T> _items = new List<T>();

    public IEnumerable<T> GetAll()
    {
        return new List<T>(_items);
    }
    public T? GetById(int id)
    {
        return _items.FirstOrDefault(e => e.Id == id);
    }
    public void Add(T entity)
    {
        _items.Add(entity);
    }
    public void Delete(int id)
    {
        var e = GetById(id);
        if (e != null)
        {
            _items.Remove(e);
        }
    }

}
// ── Entity types — DO NOT change ─────────────────────────────────────────────

public record BookEntity(int Id, string Title) : IEntity;
public record MemberEntity(int Id, string FullName) : IEntity;
