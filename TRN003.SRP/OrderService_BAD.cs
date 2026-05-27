// TRN-003.SRP · OrderService — Before & After
// ──────────────────────────────────────────────────────────────
// THE VIOLATION:
//   OrderService_BAD is a God class — it has at least SIX reasons to change:
//     1. The finance team changes the tax rate or discount formula  → pricing logic
//     2. The warehouse team changes stock tracking rules           → inventory logic
//     3. The ops team changes how orders are persisted            → repository logic
//     4. The marketing team changes the email template            → notification logic
//     5. The platform team changes the logging framework          → logging logic
//     6. The QA team adds new validation rules                    → validation logic
//
//   When one concern changes, you must open this file and risk breaking the others.
//   That is what SRP is trying to prevent.
//
// YOUR TASK:
//   1. Read OrderService_BAD.PlaceOrder() in full.
//   2. List the responsibilities in the STEP 1 comment block below.
//   3. Extract each into its own class in the AFTER section.
//   4. Create an OrderCoordinator that wires them together.
//   5. Uncomment and pass the tests in Program.cs.

using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Tracing;
using System.Numerics;
using TRN003.SRP;

namespace TRN003.SRP;

// ── Shared domain types — DO NOT change ──────────────────────────────────────

public class Order
{
    public int        Id              { get; set; }
    public string     CustomerId      { get; set; } = "";
    public string     CustomerEmail   { get; set; } = "";
    public List<OrderLine> Lines      { get; set; } = new();
    public decimal    DiscountPercent { get; set; }
    public OrderStatus Status         { get; set; } = OrderStatus.Pending;
}

public class OrderLine
{
    public string  ProductId   { get; set; } = "";
    public string  ProductName { get; set; } = "";
    public int     Quantity    { get; set; }
    public decimal UnitPrice   { get; set; }
}

public enum OrderStatus { Pending, Confirmed, Shipped, Cancelled }

// ── Result type returned by the coordinator ───────────────────────────────────
public record OrderResult(bool IsSuccess, int OrderId, decimal Total, string? Error = null);

// =============================================================================
// ── BEFORE (BAD) — God class. Read carefully, then build your fix below. ─────
// =============================================================================

/// <summary>
/// GOD CLASS — violates SRP: validation, pricing, inventory,
/// persistence, notifications, and logging all in one method.
/// </summary>
public class OrderService_BAD
{
    private static readonly Dictionary<int, Order> _database = new();
    private static int _nextId = 1;
    private static readonly Dictionary<string, int> _inventory = new()
    {
        ["BOOK-001"] = 50,
        ["BOOK-002"] = 3,
        ["BOOK-003"] = 0,    // out of stock
    };

    public string PlaceOrder(Order order)
    {
        // RESPONSIBILITY 1 — Logging
        Console.WriteLine($"[LOG {DateTime.UtcNow:HH:mm:ss}] PlaceOrder called for customer {order.CustomerId}");

        // RESPONSIBILITY 2 — Validation
        if (string.IsNullOrWhiteSpace(order.CustomerId))
            throw new ArgumentException("CustomerId is required");
        if (string.IsNullOrWhiteSpace(order.CustomerEmail) || !order.CustomerEmail.Contains('@'))
            throw new ArgumentException("Valid CustomerEmail is required");
        if (order.Lines.Count == 0)
            throw new ArgumentException("Order must have at least one line");
        foreach (var line in order.Lines)
        {
            if (line.Quantity <= 0)   throw new ArgumentException($"Quantity for {line.ProductId} must be > 0");
            if (line.UnitPrice <= 0)  throw new ArgumentException($"UnitPrice for {line.ProductId} must be > 0");
        }

        // RESPONSIBILITY 3 — Inventory check
        foreach (var line in order.Lines)
        {
            if (!_inventory.ContainsKey(line.ProductId))
                throw new InvalidOperationException($"Product {line.ProductId} not found in inventory");
            if (_inventory[line.ProductId] < line.Quantity)
                throw new InvalidOperationException(
                    $"Insufficient stock for {line.ProductId}. " +
                    $"Requested: {line.Quantity}, Available: {_inventory[line.ProductId]}");
        }

        // RESPONSIBILITY 4 — Pricing
        decimal subtotal = order.Lines.Sum(l => l.Quantity * l.UnitPrice);
        decimal discount = subtotal * (order.DiscountPercent / 100m);
        decimal tax      = (subtotal - discount) * 0.18m;   // 18% GST
        decimal total    = subtotal - discount + tax;
        Console.WriteLine($"[LOG] Subtotal=₹{subtotal:N2}  Discount=₹{discount:N2}  Tax=₹{tax:N2}  Total=₹{total:N2}");

        // RESPONSIBILITY 5 — Inventory reservation
        foreach (var line in order.Lines)
            _inventory[line.ProductId] -= line.Quantity;

        // RESPONSIBILITY 6 — Persistence
        order.Id     = _nextId++;
        order.Status = OrderStatus.Confirmed;
        _database[order.Id] = order;
        Console.WriteLine($"[LOG] Order {order.Id} saved to database");

        // RESPONSIBILITY 7 — Email notification
        Console.WriteLine($"[EMAIL] → {order.CustomerEmail}: Order #{order.Id} confirmed. Total: ₹{total:N2}");

        return $"Order {order.Id} confirmed. Total: ₹{total:N2}";
    }

    public Order? GetOrder(int id)
    {
        Console.WriteLine($"[LOG] GetOrder({id})");
        return _database.GetValueOrDefault(id);
    }

    public void CancelOrder(int id)
    {
        Console.WriteLine($"[LOG] CancelOrder({id})");
        if (!_database.TryGetValue(id, out var order))
            throw new InvalidOperationException($"Order {id} not found");
        if (order.Status == OrderStatus.Shipped)
            throw new InvalidOperationException("Cannot cancel an already-shipped order");
        foreach (var line in order.Lines)
            if (_inventory.ContainsKey(line.ProductId))
                _inventory[line.ProductId] += line.Quantity;
        order.Status = OrderStatus.Cancelled;
        Console.WriteLine($"[EMAIL] → {order.CustomerEmail}: Order #{id} cancelled.");
    }
}

// =============================================================================
// ── AFTER (YOUR SOLUTION) ─────────────────────────────────────────────────────
// =============================================================================
//
// STEP 1: List the responsibilities you identified from OrderService_BAD here:
//
//   Responsibility 1: ???  verification
//   Responsibility 2: ???   invenory updates 
//   Responsibility 3: ???   prices
//   Responsibility 4: ???   orders
//   Responsibility 5: ???   email notifications
//   Responsibility 6: ???   order logging
//
// STEP 2: Create one focused class per responsibility below.
//         Suggested names — rename freely:
//           OrderValidator       — validates the order object
//           PricingCalculator    — computes subtotal, discount, tax, total
//           InMemoryInventory    — tracks stock levels
//           InMemoryOrderRepo    — persists and retrieves orders
//           ConsoleNotifier      — sends notifications (console for now)
//           OrderLogger          — logs events (console for now)
//
// STEP 3: Create OrderCoordinator that accepts the above via constructor injection
//         and orchestrates them — no business logic of its own.
//         Its PlaceOrder method should call the right class in the right order.
//
// STEP 4: OrderCoordinator.PlaceOrder returns an OrderResult record (defined above).
//
// Write your solution below this line:
// ─────────────────────────────────────────────────────────────────────────────

public class OrderVerification
{
    // here i want ot take the validations of costomerid, email, count, quantity > 0 and after these checks i log in teh time adn alll
    public void validate(Order order)
    {
        if (string.IsNullOrWhiteSpace(order.CustomerId))
            throw new ArgumentException("CustomerId is required");
        if (string.IsNullOrWhiteSpace(order.CustomerEmail) || !order.CustomerEmail.Contains('@'))
            throw new ArgumentException("Valid CustomerEmail is required");
        if (order.Lines.Count == 0)
            throw new ArgumentException("Order must have at least one line");
        foreach (var line in order.Lines)
        {
            if (line.Quantity <= 0) throw new ArgumentException($"Quantity for {line.ProductId} must be > 0");
            if (line.UnitPrice <= 0) throw new ArgumentException($"UnitPrice for {line.ProductId} must be > 0");
        }
    }
}

public class CostCalculator
{
    public decimal CalculateTotal(Order order)
    {
        decimal subtotal = SubTotal(order);
        decimal discount = Discounts(subtotal, order);
        decimal tax = TaxedAmount(subtotal, discount);
        decimal total = TotalAmount(subtotal, discount, tax);
        return total;
    }
    private decimal SubTotal(Order order)
    { 
        return order.Lines.Sum(l => l.Quantity * l.UnitPrice); 
    }

    private decimal Discounts(decimal subtotal, Order order)
    {
        return subtotal * (order.DiscountPercent / 100m);
    }

    private decimal TaxedAmount(decimal subtotal, decimal discount)
    {
        return (subtotal - discount) * 0.18m;
    }
    private decimal TotalAmount(decimal subtotal, decimal discount, decimal tax)
    {
        return subtotal - discount + tax;
    }
}

public class InMemoryInventory
{
    private static readonly Dictionary<string, int> _inventory = new()
    {
        ["BOOK-001"] = 50,
        ["BOOK-002"] = 3,
        ["BOOK-003"] = 0,    // out of stock
    };
    public void InventoryCheck(Order order)
    {
        foreach (var line in order.Lines)
        {
            if (!_inventory.ContainsKey(line.ProductId))
                throw new InvalidOperationException($"Product {line.ProductId} not found in inventory");
            if (_inventory[line.ProductId] < line.Quantity)
                throw new InvalidOperationException(
                    $"Insufficient stock for {line.ProductId}. " +
                    $"Requested: {line.Quantity}, Available: {_inventory[line.ProductId]}");
        }
    }

    public void StockReserve(Order order)
    {
        foreach (var line in order.Lines)
            _inventory[line.ProductId] -= line.Quantity;
    }
    public void StockStock(Order order)
    {
        foreach (var line in order.Lines)
            _inventory[line.ProductId] += line.Quantity;
    }

}

public class InMemoryOrderRepo
{
    private static readonly Dictionary<int, Order> _database = new();
    private static int _nextId = 1;

    public void UpdateOrder(Order order)
    {
        order.Id = _nextId++;
        order.Status = OrderStatus.Confirmed;
        _database[order.Id] = order;
        Console.WriteLine($"[LOG] Order {order.Id} saved to database");
    }

    public Order? GetOrder(int id)
    {
        Console.WriteLine($"[LOG] GetOrder({id})");
        return _database.GetValueOrDefault(id);

    }

}

public class ConsoleNotifier
{
    public void OrderNotification(Order order, decimal total)
    {
        Console.WriteLine($"[EMAIL] → {order.CustomerEmail}: Order #{order.Id} confirmed. Total: ₹{total:N2}");
    }
}

public class OrderLogger
{
    public void OrderLog(Order order)
    {
        Console.WriteLine($"[LOG {DateTime.UtcNow:HH:mm:ss}] PlaceOrder called for customer {order.CustomerId}");
    }
}





public class OrderCoordinator
{
    private readonly OrderVerification _verify;
    private readonly CostCalculator _calculator;
    private readonly InMemoryInventory _inventorylist;
    private readonly InMemoryOrderRepo _repo;
    private readonly ConsoleNotifier _notifier;
    private readonly OrderLogger _logger;

    public OrderCoordinator(OrderVerification verify,
        CostCalculator calculator,
        InMemoryInventory inventorylist,
        InMemoryOrderRepo repo,
        ConsoleNotifier notifier,
        OrderLogger logger)
    {
        _verify = verify;
        _calculator = calculator;
        _inventorylist = inventorylist;
        _repo = repo;
        _notifier = notifier;
        _logger = logger;



    }
        public void CancelOrder(int id)
        {
            Console.WriteLine($"[LOG] CancelOrder({id})");
            Order? order = _repo.GetOrder(id);
            if (order == null)
                throw new InvalidOperationException(
                    $"Order {id} not found");
            if (order.Status == OrderStatus.Shipped)
                throw new InvalidOperationException(
                    "Cannot cancel an already-shipped order");
            _inventorylist.StockStock(order);
            order.Status = OrderStatus.Cancelled;
            Console.WriteLine(
                $"[EMAIL] → {order.CustomerEmail}: " +
                $"Order #{id} cancelled.");
        }
        public OrderResult PlaceOrder(Order order)
        {
            _logger.OrderLog(order);
            _verify.validate(order);
            _inventorylist.InventoryCheck(order);
            decimal total = _calculator.CalculateTotal(order);
            _inventorylist.StockReserve(order);
            _repo.UpdateOrder(order);
            _notifier.OrderNotification(order, total);
            return new OrderResult(
                true,
                order.Id,
                total);

        }
}