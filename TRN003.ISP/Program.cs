// ============================================================
// TRN-003 · Exercise 04 — Interface Segregation Principle
// ============================================================
// HOW TO RUN:  Set TRN003.ISP as Startup Project → F5
// ============================================================

using TRN003.ISP;

Console.WriteLine("╔══════════════════════════════════════════════╗");
Console.WriteLine("║   TRN-003.ISP · Interface Segregation        ║");
Console.WriteLine("╚══════════════════════════════════════════════╝\n");

// ── BAD code demo ─────────────────────────────────────────────────────────────
Console.WriteLine("── Demonstrating ISP violations (fat IWorker_BAD interface) ──\n");

IWorker_BAD[] workers_bad =
{
    new HumanWorker_BAD("Alice"),
    new WeldingRobot_BAD("R2-D2"),
    new RemoteContractor_BAD("Bob"),
};

foreach (var w in workers_bad)
{
    w.Work();
    Try($"{w.GetType().Name}.Eat()",      () => w.Eat());
    Try($"{w.GetType().Name}.Weld()",     () => w.Weld());
    Try($"{w.GetType().Name}.Recharge()", () => w.Recharge());
    Console.WriteLine();
}

Console.WriteLine("── Build your refactored solution below the BAD code in this file ──");
Console.WriteLine("── Uncomment tests as you finish ──\n");



// ── WIRE UP YOUR REFACTORED CLASSES HERE ──────────────────────────────────────

// TEST 1: WorkScheduler accepts anything IWorkable — no fat interface required
var scheduler = new WorkScheduler();
scheduler.ScheduleShift(new HumanWorker("Alice"), 8);
scheduler.ScheduleShift(new WeldingRobot("R2-D2"), 24);
scheduler.ScheduleShift(new RemoteContractor("Bob"), 6);
Verify(true, "TEST 1: All worker types can be scheduled via IWorkable");

// TEST 2: LunchBreakManager only manages IEatable workers
var lunchManager = new LunchBreakManager();
lunchManager.CallForLunch(new HumanWorker("Alice"));
lunchManager.CallForLunch(new RemoteContractor("Bob"));
Verify(true, "TEST 2: HumanWorker and RemoteContractor can have lunch");

// TEST 3: WeldingRobot cannot be passed to LunchBreakManager — compile-time error.
// Uncomment the line below — it must NOT compile:
//lunchManager.CallForLunch(new WeldingRobot("R2-D2"));
Verify(true, "TEST 3: WeldingRobot cannot be passed to LunchBreakManager (compile-time safety)");

// TEST 4: WeldingManager only manages IWeldable workers
var weldingManager = new WeldingManager();
weldingManager.StartWelding(new WeldingRobot("R2-D2"));
Verify(true, "TEST 4: Only IWeldable workers can be passed to WeldingManager");

//TEST 5: No NotImplementedException from any worker in the hierarchy
bool clean = true;
try
{
    var alice = new HumanWorker("Alice");
    alice.Work(); alice.Eat(); alice.TakeBreak();
    var robot = new WeldingRobot("R2-D2");
    robot.Work(); robot.Weld(); robot.Recharge();
    var bob = new RemoteContractor("Bob");
    bob.Work(); bob.Eat();
}
catch (NotImplementedException) { clean = false; }
Verify(clean, "TEST 5: No NotImplementedException from any worker type");


static void Try(string label, Action a)
{
    try { a(); Console.ForegroundColor = ConsoleColor.Green; Console.WriteLine($"  [OK]        {label}"); }
    catch (NotImplementedException ex) { Console.ForegroundColor = ConsoleColor.Red; Console.WriteLine($"  [VIOLATION] {label}: {ex.Message}"); }
    finally { Console.ResetColor(); }
}

static void Verify(bool ok, string msg)
{
    Console.ForegroundColor = ok ? ConsoleColor.Green : ConsoleColor.Red;
    Console.WriteLine($"  [{(ok ? "PASS" : "FAIL")}] {msg}");
    Console.ResetColor();
}
