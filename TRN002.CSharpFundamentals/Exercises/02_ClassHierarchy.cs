// TRN-002 · Exercise 02: Inheritance, abstract, virtual/override
// ──────────────────────────────────────────────────────────────
// LEARNING GOALS:
//   • Build a class hierarchy using inheritance.
//   • Understand abstract classes vs interfaces.
//   • Use virtual/override to customise behaviour in subclasses.
//   • Experience polymorphism: calling a base-type reference at runtime
//     dispatches to the correct subclass method.

using TRN002;

namespace TRN002.Exercises;

public static class ClassHierarchy
{
    public static void Run()
    {
        var dog = new Dog("Rex");
        Assert.AreEqual("Rex",   dog.Name,        "Dog name set by constructor");
        Assert.AreEqual("Woof!", dog.MakeSound(),  "Dog.MakeSound() must return 'Woof!'");
        Assert.AreEqual("Dog",   dog.Species,      "Dog.Species must return 'Dog'");
        Assert.AreEqual("Rex is sleeping...", dog.Sleep(), "Sleep uses the animal's name");

        var cat = new Cat("Whiskers");
        Assert.AreEqual("Meow!", cat.MakeSound(), "Cat.MakeSound() must return 'Meow!'");
        Assert.AreEqual("Cat",   cat.Species,     "Cat.Species must return 'Cat'");
        Assert.IsTrue(cat.IsIndoor, "Cat.IsIndoor should default to true");

        // Polymorphism: work through the base-type reference
        Animal[] animals = { new Dog("Buddy"), new Cat("Luna") };
        foreach (var a in animals)
        {
            Assert.IsTrue(a.MakeSound().Length > 0,
                $"{a.Name} must return a non-empty sound when called via Animal reference");
        }

        // A Cat should be usable anywhere an Animal is expected (LSP preview)
        Animal animalRef = cat;
        Assert.AreEqual("Meow!", animalRef.MakeSound(),
            "Calling MakeSound() via Animal reference should still dispatch to Cat's override");
    }
}

// ── Base class — complete the TODOs ──────────────────────────────────────────

public abstract class Animal
{
    public string Name { get; } // readonly keyword is mainly for fields(variables)
    public abstract string Species { get; }

    public Animal(string name)
    {
        Name = name;
    }

    public abstract string MakeSound();

    // TODO: Add a public read-only property 'Name' (string).
    //       It must be set only through the constructor.
    //       Use a get-only auto-property:  public string Name { get; }

    // TODO: Add a public abstract read-only property 'Species' (string).
    //       Abstract means: no body here; every subclass MUST override it.

    // TODO: Constructor — accept 'name' parameter, assign to Name.

    // TODO: Declare MakeSound() as public abstract returning string.
    //       No body — the subclass provides the implementation.

    // ── Shared behaviour: do NOT change ──────────────────────────────────────
    public string Sleep() => $"{Name} is sleeping...";
}

// ── Dog — complete the TODOs ─────────────────────────────────────────────────

public class Dog : Animal
{
    public Dog(string name) : base(name) { }
    // TODO: Constructor — accept name, pass to base(name).
    public override string Species => "Dog";
    // TODO: Override Species to return "Dog".
    public override string MakeSound() => "Woof!";
    // TODO: Override MakeSound() to return "Woof!".
}

// ── Cat — create from scratch ────────────────────────────────────────────────
//

public class Cat : Animal
{
    public bool IsIndoor { get; init; } = true;
    public Cat(string name) : base(name) { }
    public override string Species => "Cat";
    public override string MakeSound() => "Meow!";

}
// Requirements:
//   • Inherits from Animal
//   • Constructor: accepts name (string), passes to base
//   • Species returns "Cat"
//   • MakeSound() returns "Meow!"
//   • Property IsIndoor (bool, get; init;) defaults to true
//     so  new Cat("Whiskers")  has IsIndoor == true
//     and new Cat("Stray") { IsIndoor = false }  has IsIndoor == false
//
// Write the Cat class below:
