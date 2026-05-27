// TRN-003.OCP · DiscountCalculator — Before & After
// ──────────────────────────────────────────────────────────────
// THE VIOLATION:
//   DiscountCalculator_BAD uses a switch/if-else chain.
//   Adding a new discount type means EDITING this existing class —
//   it is CLOSED for extension and OPEN for modification. That is backwards.
//
//   OCP says code should be:
//     OPEN for extension   → add new behaviour via new classes
//     CLOSED for modification → never touch existing, tested code to add a feature
//
// YOUR TASK:
//   1. Define interface IDiscountStrategy with: decimal Calculate(OrderSummary order)
//   2. Extract each discount type into its own class implementing IDiscountStrategy.
//   3. Refactor DiscountCalculator to accept IEnumerable<IDiscountStrategy>.
//   4. Add FlashSaleDiscount (30% off if subtotal > ₹5000) WITHOUT touching any
//      of your existing classes — only by adding a new file/class.
//   5. Pass all tests in Program.cs.

namespace TRN003.OCP;

// ── Shared types — DO NOT change ─────────────────────────────────────────────

public class OrderSummary
{
    public decimal Subtotal        { get; set; }
    public string  MembershipTier  { get; set; } = "Standard";
    public bool    IsFirstOrder    { get; set; }
    public int     LoyaltyPoints   { get; set; }
    public string  PromoCode       { get; set; } = "";
}

// =============================================================================
// ── BEFORE (BAD) ─────────────────────────────────────────────────────────────
// =============================================================================

/// <summary>
/// VIOLATES OCP — every new promotion means editing the switch/if-else chain.
/// </summary>
public class DiscountCalculator_BAD
{
    public decimal Calculate(OrderSummary order)
    {
        decimal discount = 0;

        // Membership tier — add new tier? Edit this switch.
        switch (order.MembershipTier)
        {
            case "Gold":      discount += order.Subtotal * 0.10m; break;
            case "Platinum":  discount += order.Subtotal * 0.20m; break;
            case "Standard":  break;
            default: throw new ArgumentException($"Unknown tier: {order.MembershipTier}");
        }

        // First-order bonus — change the %, edit this file.
        if (order.IsFirstOrder)
            discount += order.Subtotal * 0.05m;

        // Promo codes — new promo? Add another else-if here.
        if      (order.PromoCode == "SUMMER10") discount += order.Subtotal * 0.10m;
        else if (order.PromoCode == "WINTER15") discount += order.Subtotal * 0.15m;
        else if (order.PromoCode == "STAFF25")  discount += order.Subtotal * 0.25m;
        // ← Every new promo adds another branch. Fragile and easy to break.

        // Loyalty points — 100 pts = ₹10 off
        if (order.LoyaltyPoints >= 100)
            discount += Math.Floor(order.LoyaltyPoints / 100m) * 10m;

        return Math.Min(discount, order.Subtotal);
    }
}

// =============================================================================
// ── AFTER (YOUR SOLUTION) ────────────────────────────────────────────────────
// =============================================================================
//
// STEP 1: Define IDiscountStrategy interface here.
//         Single method: decimal Calculate(OrderSummary order)
//
// STEP 2: Extract each discount rule into its own class:
//         GoldMemberDiscount       — 10% for Gold tier
//         PlatinumMemberDiscount   — 20% for Platinum tier
//         FirstOrderDiscount       — 5% if IsFirstOrder == true
//         PromoCodeDiscount        — constructor takes (string code, decimal percentage)
//                                    only applies if order.PromoCode matches
//         LoyaltyPointsDiscount    — floor(points/100) × ₹10
//
// STEP 3: Refactor DiscountCalculator to:
//         - Constructor takes IEnumerable<IDiscountStrategy>
//         - Calculate() sums all strategies and caps at Subtotal
//
// STEP 4: Create FlashSaleDiscount (30% off when Subtotal > ₹5000).
//         Add it to the strategies list in Program.cs WITHOUT changing
//         DiscountCalculator or any other existing class.
//
// Write your solution below:
// ─────────────────────────────────────────────────────────────────────────────

public interface IDiscountStrategy
{
    decimal Calculate(OrderSummary order);
}

public class GoldMember : IDiscountStrategy
{
    public decimal Calculate(OrderSummary order)
    {
        if(order.MembershipTier == "Gold")
        {
            return order.Subtotal * 0.10m;
        }
        return 0;
    }
}

public class PlatinumMember : IDiscountStrategy
{
    public decimal Calculate(OrderSummary order)
    {
        if(order.MembershipTier == "Platinum")
        {
            return order.Subtotal * 0.20m;
        }

        return 0;
    }
}

public class StandardMember : IDiscountStrategy
{
    public decimal Calculate(OrderSummary order)
    {
        if (order.MembershipTier == "Standard")
        {
            return order.Subtotal * 0.0m;
        }

        return 0;
    }
}

public class FirstOrder : IDiscountStrategy
{
    public decimal Calculate(OrderSummary order)
    {
        if (order.IsFirstOrder == true)
        {
            return order.Subtotal * 0.05m;
        }

        return 0;
    }
}

public class PromoCodeDiscount : IDiscountStrategy
{
    private readonly string _code;
    private readonly decimal _percent;

    public PromoCodeDiscount(string code, decimal percent)
    {
        _code = code;
        _percent = percent;
    }
    public decimal Calculate(OrderSummary order)
    {
        if (order.PromoCode == _code)
        {
            return order.Subtotal * _percent;
        }

        return 0;
    }
}


public class LoyaltyPts : IDiscountStrategy
{
    public decimal Calculate(OrderSummary order)
    {
        if (order.LoyaltyPoints >= 100)
        {
            return Math.Floor(order.LoyaltyPoints / 100m) * 10m;
        }

        return 0;
    }
}

public class DiscountCalculator
{
    private readonly IEnumerable<IDiscountStrategy> _strategies;

    public DiscountCalculator(IEnumerable<IDiscountStrategy> strategies)
    {
        _strategies = strategies;
    }

    public decimal Calculate(OrderSummary order)
    {
        decimal TotalDiscount = 0;

        foreach(var strategy in _strategies)
        {
            TotalDiscount += strategy.Calculate(order);
        }
        return Math.Min(TotalDiscount, order.Subtotal);
    }
}

public class FlashSale : IDiscountStrategy
{
    public decimal Calculate(OrderSummary order)
    {
        if(order.Subtotal > 5000)
        {
            return order.Subtotal * 0.30m;
        }
        return 0;
    }
}