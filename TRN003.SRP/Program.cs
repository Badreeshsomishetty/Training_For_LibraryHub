// ============================================================
// TRN-003 · Exercise 01 — Single Responsibility Principle
// ============================================================
// HOW TO RUN:
//   Set TRN003.SRP as Startup Project → F5
//
// WHAT YOU WILL DO:
//   1. Read the God-class OrderService_BAD carefully.
//   2. Identify its distinct responsibilities (reasons to change).
//   3. Refactor into focused classes at the bottom of OrderService_BAD.cs.
//   4. Wire up your classes here and uncomment the tests one by one.
// ============================================================

using TRN003.SRP;

Console.WriteLine("╔══════════════════════════════════════════════╗");
Console.WriteLine("║   TRN-003.SRP · Single Responsibility        ║");
Console.WriteLine("╚══════════════════════════════════════════════╝");
Console.WriteLine();

// ── STEP 1: Run the BAD code first — observe the output ──────────────────────
Console.WriteLine("── Running OrderService_BAD (God class) ──");
var badService = new OrderService_BAD();
var badOrder = new Order
{
    CustomerId    = "CUST-001",
    CustomerEmail = "alice@example.com",
    Lines = new List<OrderLine>
    {
        new() { ProductId = "BOOK-001", ProductName = "Clean Code", Quantity = 2, UnitPrice = 500 }
    }
};

try
{
    var result = badService.PlaceOrder(badOrder);
    Console.WriteLine($"Result: {result}");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}

Console.WriteLine();
Console.WriteLine("── Now build your refactored solution in OrderService_BAD.cs ──");
Console.WriteLine("── Then replace the section below with your classes and uncomment the tests ──");
Console.WriteLine();

//============================================================
//REPLACE THIS SECTION with construction of your refactored classes.
//Example (your class names will differ):

var validator = new OrderVerification();
var pricer = new CostCalculator();
var inventory = new InMemoryInventory();
var repo = new InMemoryOrderRepo();
var notifier = new ConsoleNotifier();
var logging = new OrderLogger();
var coordinator = new OrderCoordinator(validator, pricer, inventory, repo, notifier, logging);
//============================================================

// ── UNCOMMENT TESTS ONE BY ONE AS YOU BUILD YOUR SOLUTION ────────────────────



// TEST 1: Valid order succeeds
var validOrder = new Order
{
    CustomerId = "CUST-001", CustomerEmail = "alice@example.com",
    Lines = new() { new() { ProductId = "BOOK-001", ProductName = "Clean Code", Quantity = 2, UnitPrice = 500 } }
};
var r1 = coordinator.PlaceOrder(validOrder);
Verify(r1.IsSuccess,   "TEST 1: Valid order should succeed");
Verify(r1.Total > 0m,  "TEST 1b: Total should be greater than 0");
Console.WriteLine($"       Total was: ₹{r1.Total:N2}");

// TEST 2: Missing CustomerId is rejected
try
{
    coordinator.PlaceOrder(new Order { CustomerId = "", CustomerEmail = "x@x.com",
        Lines = new() { new() { ProductId = "BOOK-001", Quantity = 1, UnitPrice = 100 } } });
    Verify(false, "TEST 2: Should have thrown for empty CustomerId");
}
catch (ArgumentException) { Verify(true, "TEST 2: Empty CustomerId correctly rejected"); }

// TEST 3: Out-of-stock book is rejected
try
{
    coordinator.PlaceOrder(new Order { CustomerId = "C2", CustomerEmail = "b@b.com",
        Lines = new() { new() { ProductId = "BOOK-003", Quantity = 1, UnitPrice = 300 } } });
    Verify(false, "TEST 3: Should have rejected out-of-stock item");
}
catch (InvalidOperationException) { Verify(true, "TEST 3: Out-of-stock correctly rejected"); }

// TEST 4: Pricing is correct  (2 × ₹500 = ₹1000, 18% GST = ₹180, total = ₹1180)
var pricingOrder = new Order { CustomerId = "C3", CustomerEmail = "c@c.com", DiscountPercent = 0,
    Lines = new() { new() { ProductId = "BOOK-001", Quantity = 2, UnitPrice = 500 } } };
var r4 = coordinator.PlaceOrder(pricingOrder);
Verify(r4.Total == 1180m, $"TEST 4: Expected ₹1180 but got ₹{r4.Total}");

// TEST 5: Cancel restores inventory
var cancelOrder = new Order { CustomerId = "C4", CustomerEmail = "d@d.com",
    Lines = new() { new() { ProductId = "BOOK-002", Quantity = 1, UnitPrice = 400 } } };
var r5 = coordinator.PlaceOrder(cancelOrder);
coordinator.CancelOrder(r5.OrderId);
Verify(true, "TEST 5: Cancel completed without throwing");


static void Verify(bool condition, string message)
{
    if (condition) { Console.ForegroundColor = ConsoleColor.Green;  Console.WriteLine($"  [PASS] {message}"); }
    else           { Console.ForegroundColor = ConsoleColor.Red;    Console.WriteLine($"  [FAIL] {message}"); }
    Console.ResetColor();
}
