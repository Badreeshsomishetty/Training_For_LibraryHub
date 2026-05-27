// ============================================================
// TRN-003 · Exercise 02 — Open/Closed Principle
// ============================================================
// HOW TO RUN:  Set TRN003.OCP as Startup Project → F5
// ============================================================

using TRN003.OCP;

Console.WriteLine("╔══════════════════════════════════════════════╗");
Console.WriteLine("║   TRN-003.OCP · Open/Closed Principle        ║");
Console.WriteLine("╚══════════════════════════════════════════════╝\n");

// ── BAD code demo ─────────────────────────────────────────────────────────────
Console.WriteLine("── DiscountCalculator_BAD ──");
var bad = new DiscountCalculator_BAD();
Console.WriteLine($"Gold 1000:   ₹{bad.Calculate(new OrderSummary { Subtotal = 1000, MembershipTier = "Gold" }):N2}");
Console.WriteLine($"Platinum 1000: ₹{bad.Calculate(new OrderSummary { Subtotal = 1000, MembershipTier = "Platinum" }):N2}");
Console.WriteLine();
Console.WriteLine("── Build your refactored solution in DiscountCalculator_BAD.cs ──\n");

// ── WIRE UP YOUR SOLUTION BELOW ───────────────────────────────────────────────
// Example:
var strategies = new IDiscountStrategy[]
{
     new GoldMember(),
     new PlatinumMember(),
     new FirstOrder(),
     new PromoCodeDiscount("SUMMER10", 0.10m),
     new PromoCodeDiscount("WINTER15", 0.15m),
     new PromoCodeDiscount("STAFF25",  0.25m),
     new LoyaltyPts(),
     new FlashSale(),   // ← added WITHOUT modifying DiscountCalculator
};
var calculator = new DiscountCalculator(strategies);

// ── UNCOMMENT TESTS ONE BY ONE ────────────────────────────────────────────────


Verify(calculator.Calculate(new() { Subtotal = 1000, MembershipTier = "Gold"     }) == 100m,
    "TEST 1: Gold 10% of ₹1000 = ₹100");

Verify(calculator.Calculate(new() { Subtotal = 1000, MembershipTier = "Platinum" }) == 200m,
    "TEST 2: Platinum 20% of ₹1000 = ₹200");

Verify(calculator.Calculate(new() { Subtotal = 1000, MembershipTier = "Gold", IsFirstOrder = true }) == 150m,
    "TEST 3: Gold (10%) + FirstOrder (5%) of ₹1000 = ₹150");

Verify(calculator.Calculate(new() { Subtotal = 1000, MembershipTier = "Standard", PromoCode = "SUMMER10" }) == 100m,
    "TEST 4: SUMMER10 promo 10% of ₹1000 = ₹100");

Verify(calculator.Calculate(new() { Subtotal = 1000, MembershipTier = "Standard", LoyaltyPoints = 350 }) == 30m,
    "TEST 5: 350 loyalty points = ₹30 (floor(350/100) × ₹10)");

Verify(calculator.Calculate(new() { Subtotal = 100, MembershipTier = "Platinum", IsFirstOrder = true,
                                     PromoCode = "STAFF25", LoyaltyPoints = 1000 }) == 100m,
    "TEST 6: Total discount cannot exceed subtotal (capped at ₹100)");

Verify(calculator.Calculate(new() { Subtotal = 6000, MembershipTier = "Standard" }) == 1800m,
    "TEST 7: FlashSale 30% on ₹6000 = ₹1800 (proves OCP — added without modifying DiscountCalculator)");

Verify(calculator.Calculate(new() { Subtotal = 4000, MembershipTier = "Standard" }) == 0m,
    "TEST 8: FlashSale does NOT apply below ₹5000");

static void Verify(bool ok, string msg)
{
    Console.ForegroundColor = ok ? ConsoleColor.Green : ConsoleColor.Red;
    Console.WriteLine($"  [{(ok ? "PASS" : "FAIL")}] {msg}");
    Console.ResetColor();
}
