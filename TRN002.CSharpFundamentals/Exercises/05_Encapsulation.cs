// TRN-002 · Exercise 05: Encapsulation & Access Modifiers
// ──────────────────────────────────────────────────────────────
// LEARNING GOALS:
//   • Use access modifiers to hide internal state.
//   • Distinguish public/private/protected/internal.
//   • Use auto-properties, init-only setters, and backing fields where appropriate.
//   • Enforce invariants — never let an object enter an invalid state.

using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using TRN002;

namespace TRN002.Exercises;

public static class Encapsulation
{
    public static void Run()
    {
        // ── BankAccount: balance must never go below zero ─────────────────────
        var account = new BankAccount("ACC-001", 1000m);
        Assert.AreEqual(1000m, account.Balance, "Initial balance should be 1000");

        account.Deposit(500m);
        Assert.AreEqual(1500m, account.Balance, "After deposit of 500, balance should be 1500");

        account.Withdraw(200m);
        Assert.AreEqual(1300m, account.Balance, "After withdrawal of 200, balance should be 1300");

        // Withdrawing more than balance should throw — not silently go negative
        Assert.Throws<InvalidOperationException>(
            () => account.Withdraw(9999m),
            "Withdrawing more than balance must throw InvalidOperationException");

        Assert.AreEqual(1300m, account.Balance,
            "Balance must remain unchanged after a failed withdrawal");

        // Depositing zero or negative should throw
        Assert.Throws<ArgumentException>(() => account.Deposit(0m),
            "Depositing 0 must throw ArgumentException");
        Assert.Throws<ArgumentException>(() => account.Deposit(-50m),
            "Depositing a negative amount must throw ArgumentException");

        // ── LibraryCard: immutable after creation ─────────────────────────────
        var card = new LibraryCard("CARD-42", "Alice", DateOnly.FromDateTime(DateTime.Today));
        Assert.AreEqual("CARD-42", card.CardNumber);
        Assert.AreEqual("Alice",   card.HolderName);

        // CardNumber must not be settable from outside — verified at compile time.
        // (This is enforced by the type system; no runtime assertion needed.)
        // If you accidentally made CardNumber have a public setter, the mentor will catch it in review.

        // ── Transaction history is internal, not directly modifiable ──────────
        account.Deposit(100m);
        account.Withdraw(50m);
        var history = account.GetTransactionHistory();
        Assert.IsTrue(history.Count >= 3,  // deposit 500, withdraw 200, deposit 100, withdraw 50
            "GetTransactionHistory() should return all recorded transactions");

        // Modifying the returned list must not affect the account's internal history
        var countBefore = account.GetTransactionHistory().Count;
        history.Clear();
        Assert.AreEqual(countBefore, account.GetTransactionHistory().Count,
            "Clearing the returned history list must NOT affect the account's internal state");
    }
}

// ── BankAccount — complete the TODOs ─────────────────────────────────────────

public class BankAccount
{
    // TODO: Store the balance in a PRIVATE backing field (decimal _balance).
    //       Expose it via a PUBLIC read-only property (no public setter).
    private decimal _balance;

    //public decimal Balance => _balance;

    // TODO: Store transaction history in a private List<string> _transactions.
    //       Do not expose the field directly.
    private List<string> _transactions = new List<string>();

    public string AccountNumber { get; }  // init-only — set in constructor, never changed

    // TODO: Constructor — accept accountNumber and initialBalance.
    //       Throw ArgumentException if initialBalance < 0.
    //       Record the initial balance as the first transaction: "Initial deposit: ₹{amount}"
    public BankAccount(string accountNumeber, decimal initialBalance)
    {
        if(initialBalance < 0)
        {
            throw new ArgumentException("Initial balance can't be less than 0");
        }
        AccountNumber = accountNumeber;
        _balance = initialBalance;
        _transactions.Add($"Initial deposit: ₹{initialBalance}");
    }

    // TODO: Deposit(decimal amount)
    //       Throw ArgumentException if amount <= 0.
    //       Add amount to balance.
    //       Record transaction: "Deposit: ₹{amount}"
    public void Deposit(decimal amount)
    {
        if(amount <= 0)
        {
            throw new ArgumentException("Deposit value is less than or equal to zero");
        }
        _balance = amount + _balance;
        _transactions.Add($"Deposit : ₹{amount}");
    }

    // TODO: Withdraw(decimal amount)
    //       Throw ArgumentException if amount <= 0.
    //       Throw InvalidOperationException if amount > Balance.
    //       Subtract amount from balance.
    //       Record transaction: "Withdrawal: ₹{amount}"
    public void Withdraw(decimal amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentException();
        }
        else if (amount > _balance)
        {
            throw new InvalidOperationException();
        }
        _balance = _balance - amount;
        _transactions.Add($"Withdrawal: ₹{amount}");
    }

    // TODO: GetTransactionHistory() — returns a COPY of the internal list.
    //       Use: return new List<string>(_transactions);
    public List<string> GetTransactionHistory()
    {
        return new List<string>(_transactions);
    }

    // Balance property stub — replace with proper implementation
    public decimal Balance => _balance;
}

// ── LibraryCard — complete the TODOs ─────────────────────────────────────────

public class LibraryCard
{
    // TODO: All three properties below must be:
    //   - Public read (get)
    //   - Set ONLY in the constructor (no public setter, no init setter)
    //   Use: public string CardNumber { get; }  and assign in constructor body.

    public string CardNumber    { get; }      // already declared — just implement constructor
    public string HolderName { get; }
    public DateOnly IssueDate { get; }

    // TODO: Constructor — accept cardNumber, holderName, issueDate.
    //       Throw ArgumentException if cardNumber or holderName is null/empty.
    public LibraryCard(string cardNumber, string holderName, DateOnly issueDate)
    {
        if (string.IsNullOrEmpty(cardNumber) || string.IsNullOrEmpty(holderName))
        {
            throw new ArgumentException();

        }
        CardNumber = cardNumber;
        HolderName = holderName;
        IssueDate = issueDate;
    }
}
