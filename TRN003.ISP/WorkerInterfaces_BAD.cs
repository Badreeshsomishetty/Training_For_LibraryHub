// TRN-003.ISP · Worker Interfaces — Before & After
// ──────────────────────────────────────────────────────────────
// THE VIOLATION:
//   IWorker_BAD is a "fat interface" — it forces every implementing class
//   to provide methods it does not support, resulting in NotImplementedException.
//
//   • HumanWorker_BAD  — cannot weld or recharge   → throws
//   • WeldingRobot_BAD — cannot eat or take breaks  → throws
//   • RemoteContractor_BAD — cannot weld, recharge, or access internals → throws
//
// ISP RULE:
//   "Clients should not be forced to depend on methods they do not use."
//   When a class throws NotImplementedException for an interface method,
//   that is a sure sign the interface is too fat — split it.
//
// YOUR TASK:
//   Split IWorker_BAD into focused interfaces.
//   Each class implements only what it genuinely supports.
//   No NotImplementedException anywhere in the hierarchy.

using System.Xml.Serialization;

namespace TRN003.ISP;

// =============================================================================
// ── BEFORE (BAD) — fat interface ─────────────────────────────────────────────
// =============================================================================

public interface IWorker_BAD
{
    void   Work();
    void   Eat();                    // robots don't eat
    void   TakeBreak();              // robots don't take breaks
    void   Weld();                   // only some workers weld
    void   AccessInternalSystems();  // contractors shouldn't have this
    void   Recharge();               // only robots need this
    string GetWorkerId();
}

public class HumanWorker_BAD : IWorker_BAD
{
    private readonly string _name;
    public HumanWorker_BAD(string name) => _name = name;
    public void   Work()                  => Console.WriteLine($"{_name} is working");
    public void   Eat()                   => Console.WriteLine($"{_name} is eating lunch");
    public void   TakeBreak()             => Console.WriteLine($"{_name} is on a break");
    public void   AccessInternalSystems() => Console.WriteLine($"{_name} accessing HR portal");
    public string GetWorkerId()           => $"HUM-{_name}";
    public void   Weld()                  => throw new NotImplementedException("Humans don't weld");
    public void   Recharge()              => throw new NotImplementedException("Humans don't recharge");
}

public class WeldingRobot_BAD : IWorker_BAD
{
    private readonly string _id;
    public WeldingRobot_BAD(string id) => _id = id;
    public void   Work()                  => Console.WriteLine($"Robot {_id} performing tasks");
    public void   Weld()                  => Console.WriteLine($"Robot {_id} welding at 3000°C");
    public void   Recharge()              => Console.WriteLine($"Robot {_id} recharging");
    public void   AccessInternalSystems() => Console.WriteLine($"Robot {_id} accessing maintenance log");
    public string GetWorkerId()           => _id;
    public void   Eat()                   => throw new NotImplementedException("Robots don't eat");
    public void   TakeBreak()             => throw new NotImplementedException("Robots don't take breaks");
}

public class RemoteContractor_BAD : IWorker_BAD
{
    private readonly string _id;
    public RemoteContractor_BAD(string id) => _id = id;
    public void   Work()      => Console.WriteLine($"Contractor {_id} working remotely");
    public void   Eat()       => Console.WriteLine($"Contractor {_id} eating at home office");
    public void   TakeBreak() => Console.WriteLine($"Contractor {_id} taking informal break");
    public string GetWorkerId() => $"CONT-{_id}";
    public void   Weld()                  => throw new NotImplementedException("Contractors don't weld on-site");
    public void   Recharge()              => throw new NotImplementedException("Contractors don't recharge");
    public void   AccessInternalSystems() => throw new NotImplementedException("Security: no internal access");
}

/// <summary>
/// Only calls Work() — but forced to depend on the fat IWorker_BAD interface.
/// After refactoring, change the parameter type to your focused IWorkable.
/// </summary>
public class WorkScheduler_BAD
{
    public void ScheduleShift(IWorker_BAD worker, int hours)
    {
        Console.WriteLine($"Scheduling {hours}h shift for {worker.GetWorkerId()}");
        worker.Work();
    }
}

// =============================================================================
// ── AFTER (YOUR SOLUTION) ────────────────────────────────────────────────────
// =============================================================================
//
// STEP 1: Define focused interfaces. Suggested names:
//         IWorkable       — Work(), GetWorkerId()
//         IEatable        — Eat()
//         IBreakable      — TakeBreak()
//         IWeldable       — Weld()
//         IRechargeable   — Recharge()
//         IInternalAccess — AccessInternalSystems()
//
//         Combine related ones if it makes sense. Justify your grouping in comments.
//
// STEP 2: Rewrite HumanWorker, WeldingRobot, RemoteContractor
//         implementing ONLY interfaces that match their real capabilities.
//         Zero NotImplementedException.
//
// STEP 3: Rewrite WorkScheduler to accept IWorkable only.
//
// STEP 4: Add LunchBreakManager that accepts IEatable.
//         Add WeldingManager that accepts IWeldable.
//         These prove that callers only depend on what they actually use.
//
// Write your solution below:
// ─────────────────────────────────────────────────────────────────────────────

public interface IWorkable
{
    void Work();
    string GetWorkerId();

}
public interface IEatBreak               // eating and taking break both are avalible for humans but not he robot.
{
    void Eat();
    void TakeBreak();
}

public interface IWeldRecharge            // Sicne only Robots weld and recharge we can group but not for humans.
{
    void Recharge();
    void Weld();
}

public interface IInternalAccess
{
    void AccessInternalSystems();
}

public class HumanWorker : IWorkable, IEatBreak, IInternalAccess
{
    private string _name;

    public HumanWorker(string name)
    {
        _name = name;
    }

    public void Work() => Console.WriteLine($"{_name} is working");
    public string GetWorkerId() => $"HUM-{_name}";

    public void Eat() => Console.WriteLine($"{_name} is eating Lunch");
    public void TakeBreak() => Console.WriteLine($"{_name} is on a break");

    public void AccessInternalSystems() => Console.WriteLine($"{_name} accessing HR portal");

}


public class WeldingRobot : IWorkable, IWeldRecharge
{
    private string _id;

    public WeldingRobot(string id)
    {
        _id = id;
    }
    public void Work() => Console.WriteLine($"{_id} is working");
    public string GetWorkerId() => $"HUM-{_id}";
    public void Recharge() => Console.WriteLine($"Robot {_id} recharging");
    public void Weld() => Console.WriteLine($"Robot {_id} welding at 3000°C");
}

public class RemoteContractor : IWorkable, IEatBreak
{
    private string _name;

    public RemoteContractor(string name)
    {
        _name = name;
    }
    public void Work() => Console.WriteLine($"{_name} is working");
    public string GetWorkerId() => $"HUM-{_name}";

    public void Eat() => Console.WriteLine($"{_name} is eating Lunch");
    public void TakeBreak() => Console.WriteLine($"{_name} is on a break");
}

public class WorkScheduler
{
    public void ScheduleShift(IWorkable worker, int hrs)
    {
        Console.WriteLine($"Scheduling {hrs}h shift for {worker.GetWorkerId()}");
        worker.Work();
    }
}

public class LunchBreakManager
{
    public void CallForLunch(IEatBreak worker)
    {
        worker.Eat();
    }
}

public class WeldingManager
{
    public void StartWelding(IWeldRecharge worker)
    {
        worker.Weld();
    }
}