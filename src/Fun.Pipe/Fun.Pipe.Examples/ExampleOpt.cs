using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Fun.Pipe.Examples;

public static class ExampleOpt
{
    internal static async Task Run()
    {
        Log("\nRunning Opt Examples");
        
        // Some(T)
        var someInt = Some(42);     // implicit T
        someInt = Some<int>(42);    // explicit T
        someInt = 42;               // implicit conversion from value to Some(value)
        Opt<int> anotherInt = 42;   // implicit conversion from value to Some(value)
        Assert(someInt.IsSome && someInt.Unwrap() == 42);


        // None of T
        var noneFloat = None<float>();      // T has to be explicit
        Assert(noneFloat.IsNone);
        
        Opt<string> noneString = default;   // expectedly, default is None
        Assert(noneString.IsNone);


        // Some: returns None when null; Opt is guaranteed to be null-free
        noneString = Some<string>(null);
        Assert(noneString.IsNone);


        // ToOpt: from value
        someInt = 12.ToOpt();
        Assert(someInt.IsSome && someInt.Unwrap() == 12);
        
        string nullString = null;
        noneString = nullString.ToOpt();
        Assert(noneString.IsNone);


        // ToOpt: from value with validation
        var nonneg1 = 42.ToOpt(x => x >= 0);
        Assert(nonneg1 == 42);

        var nonneg2 = (-10).ToOpt(x => x >= 0);
        Assert(nonneg2 != -10);
        Assert(nonneg2.IsNone);


        // ParseXOrNone: for common primitives (Opt counterpart of TryParseX methods)
        var maybeInt = "not-a-number".ParseIntOrNone();
        Assert(maybeInt.IsNone);

        var maybeDate = "2022-05-05".ParseDateOnlyOrNone();
        Assert(maybeDate == new DateOnly(2022, 5, 5));


        // ParseOrNone: general version that parses within a try (Some) catch (None) block
        var maybeDateTime = "05-05-2022".ParseOrNone(s => DateTime.ParseExact(s, "yyyy-MM-dd", default));
        Assert(maybeDateTime.IsNone);
        maybeDateTime = "2022-05-05".ParseOrNone(s => DateTime.ParseExact(s, "yyyy-MM-dd", default));
        Assert(maybeDateTime.IsSome);


        // AsOpt: from Res to Opt:
        // * Err -> None
        // * Ok(x) -> Some(x)
        static Wizard ParseWizard(string str)
        {
            var parts = str.Split('-');
            return new(Name: parts[0], NbSpells: int.Parse(parts[1]));
        }
        Res<Wizard> okWizard = TryMap(() => ParseWizard("Merlin-42"));
        Assert(okWizard == Ok(new Wizard("Merlin", 42)));

        Opt<Wizard> someWizard = okWizard.AsOpt(); // Ok(Merlin) -> Some(Merlin)
        Assert(someWizard == Some(new Wizard("Merlin", 42)));

        Res<Wizard> errWizard = TryMap(() => ParseWizard("badwizardinput"));
        Assert(errWizard.IsErr);

        Opt<Wizard> noneWizard = errWizard.AsOpt(); // Err -> None
        Assert(noneWizard.IsNone);


        // Unwrap(): only when sure that it IsSome
        var optDuration = Some(TimeSpan.FromSeconds(42));
        var duration = optDuration.Unwrap();    // would throw if it were IsNone
        Assert(duration.Seconds == 42);


        // None.Unwrap(): avoid by all means
        optDuration = None<TimeSpan>();
        try
        {
            duration = optDuration.Unwrap();
            Assert(false, "must have thrown an exception while unwrapping None");
        }
        catch { /*will end up here*/ }


        // Unwrap(T): with a fallback value
        optDuration = Some(TimeSpan.FromSeconds(42));
        duration = optDuration.Unwrap(TimeSpan.FromSeconds(10));
        Assert(duration.Seconds == 42); // fallback value is ignored since optDuration.IsSome

        optDuration = None<TimeSpan>();
        duration = optDuration.Unwrap(TimeSpan.FromSeconds(10)); // never throws
        Assert(duration.Seconds == 10);


        // Unwrap(() => T): with a lazy fallback value
        Opt<Wizard> maybeWizard = new Wizard("Merlin", 42); // assume it is expensive to create a wizard, needless to say
        
        var wizard = maybeWizard.Unwrap(new Wizard("Gandalf", 42));
        Assert(wizard.Name == "Merlin");    // Gandalf is already created, although it is immediately thrown away

        wizard = maybeWizard.Unwrap(() => new("Gandalf", 42));
        Assert(wizard.Name == "Merlin");    // with lazy version, Gandalf is never created

        wizard = await maybeWizard.Unwrap(() => Task.FromResult(new Wizard("Gandalf", 42)));
        Assert(wizard.Name == "Merlin");    // async counterpart of lazy; for instance, when the wizard is queried


        // Match: both Some(val) and None to values
        maybeWizard = Some(new Wizard("Merlin", 42));
        
        int nbSpells = maybeWizard.Match(some: w => w.NbSpells, none: 0);
        nbSpells = maybeWizard.Match(w => w.NbSpells, 0);
        nbSpells = maybeWizard.Match(w => w.NbSpells, () => 0); // lazy version for the none case
        Assert(nbSpells == 42);
        
        int nbSpellsOfNone = None<Wizard>().Match(w => w.NbSpells, 0);
        Assert(nbSpellsOfNone == 0);


        // Match: both Some(val) and None to actions
        static void FakeLog(object message) { }
        maybeWizard = Some(new Wizard("Merlin", 42));
        maybeWizard.Match(w => FakeLog(w.Name), () => FakeLog("no-wizard-found")); // this would log Merlin if it were not fake
        None<Wizard>().Match(w => FakeLog(w.Name), () => FakeLog("no-wizard-found")); // this would log no-wizard-found


        // Equality checks
        Assert(Some(12.42) == Some(12.42));         // value equality
        Assert(Some(12.42) != None<double>());      // None is not equal to anything
        Assert(12.42 != None<double>());            // None is not equal to anything
        Assert(None<double>() != None<double>());   // None is not equal to anything
        
        Assert(Some(12.42) == 12.42);               // implicit equality with value
        Assert(Some(12.42) != 42.12);               // implicit equality with value
        Assert(Some(12.42) == Ok(12.42));           // implicit equality with Ok of T (Res)
        Assert(Some(12.42) != Ok(42.12));           // implicit equality with Ok of T (Res)


        // Opt for optional parameters
        static DataTable GetQuery(string query, Opt<int> timeoutMilliseconds)
        {
            // use default timeout when timeoutMilliseconds.IsNone; use timeoutMilliseconds.Unwrap() otherwise.
            return new();
        }
        var getPersons = GetQuery("select persons", None<int>());
        var getPersonsWithSpecificTimeout = GetQuery("select-pesons", Some(10800));
        getPersonsWithSpecificTimeout = GetQuery("select-pesons", 10800);   // implicitly: 10800 -> Some(10800)

        
        // Opt for optional parameters: default->None
        static DataTable GetQueryWithDefault(string query, Opt<int> timeoutMilliseconds = default)
        {
            // use default timeout when timeoutMilliseconds.IsNone; use timeoutMilliseconds.Unwrap() otherwise.
            return new();
        }
        getPersons = GetQueryWithDefault("select persons");
        getPersonsWithSpecificTimeout = GetQueryWithDefault("select-pesons", 10800);    // implicitly: 10800 -> Some(10800)


        // Map: bypass None track
        var someNumber = Some(42f);
        var lessThan100 = someNumber.Map(x => MathF.Sqrt(x)).Map(sqrt => sqrt < 10);
        Assert(lessThan100 == Some(true));  // methods are applied on the optional input's value, returns Some(result)

        var noNumber = None<float>();
        lessThan100 = noNumber.Map(x => MathF.Sqrt(x)).Map(sqrt => sqrt < 10);
        Assert(lessThan100.IsNone);         // all map methods are bypassed; just returns None
        

        // Run: bypassed None track
        someNumber = Some(42f);
        float sideEffect = 10f;
        someNumber.Run(v => { sideEffect += v; });
        Assert(sideEffect == 52f);  // Run method is executed when IsSome, incrementing sideEffect

        noNumber = None<float>();
        sideEffect = 10f;
        noNumber.Run(v => { sideEffect += v; });
        Assert(sideEffect == 10f);  // Run method is bypassed when IsNone, leaving sideEffect unchanged


        // Note that there exist TryMap, TryRun, MapAsync, RunAsync, TryMapAsync, TryRunAsync versions,
        // which only operate when IsSome and bypass when IsNone, and do what their names suggest:
        // * Map hints Func, and Run hints Action;
        // * Try means that the lambda will be executed within a try-catch block and result is always a Res type;
        // * Async methods are async counterparts of the map/run methods.


        // Complementary methods that run only when IsNone:
        // RunIfNone, LogIfNone, ThrowIfNone
        someNumber = Some(42f);
        sideEffect = 0f;
        someNumber.RunIfNone(() => { sideEffect += 1f; });
        Assert(sideEffect == 0f);   // RunIfNone is bypassed when IsSome, leaving sideEffect unchanged.

        noNumber = None<float>();
        sideEffect = 0f;
        noNumber.RunIfNone(() => { sideEffect += 1f; });
        Assert(sideEffect == 1f);   // RunIfNone runs when IsNone, adding 1f to the sideEffect.


        // FirstOrNone & LastOrNone: on IEnumerable<T>
        var list = new List<Wizard>();
        Assert(list.FirstOrNone().IsNone); // Opt counterpart of FirstOrDefault,
        Assert(list.LastOrNone().IsNone);  // where absence of a value is explicit.

        Wizard unfortunatelyNullPerson = null;
        list = new List<Wizard>() { unfortunatelyNullPerson };
        Assert(list.FirstOrNone().IsNone); // All null's are skipped,
        Assert(list.LastOrNone().IsNone);  // the list is empty in terms of values.

        list = new List<Wizard>() { unfortunatelyNullPerson, new("Saruman", 42), new("Shaman", 42), null };
        Assert(list.FirstOrNone() == new Wizard("Saruman", 42));
        Assert(list.LastOrNone() == new Wizard("Shaman", 42));


        // GetValueOrNone: on Dictionary and ConcurrentDictionary
        var dictWizards = new Dictionary<string, Wizard>
        {
            { "Merlin", new Wizard("Merlin", 42) },
            { "Bad Wizard", null }
        };
        var gotMerlin = dictWizards.GetValueOrNone("Merlin"); // Opt counterpart of TryGetValue
        Assert(gotMerlin == Some(new Wizard("Merlin", 42))); // key exists; value is nonnull

        var gotNoWizard = dictWizards.GetValueOrNone("no wizard");
        Assert(gotNoWizard.IsNone); // key does not exist

        var gotNoneWizard = dictWizards.GetValueOrNone("Bad Wizard");
        Assert(gotNoneWizard.IsNone); // key exists, but value is null -> None


        // Opt collections: IEnumerable<Opt<T>>
        List<Opt<Wizard>> wizards;

        // AnySome
        wizards = new() { };
        Assert(wizards.AnySome() == false);

        wizards = new() { default, new Wizard("Morgana", 33) };
        Assert(wizards.AnySome() == true);
        Assert(wizards.AnySome(w => w.NbSpells > 100) == false);    // predicate is applied directly on the values.
        Assert(wizards.AnySome(w => w.NbSpells > 10) == true);

        // AllSome
        wizards = new() { default, new Wizard("Morgana", 42) };
        Assert(wizards.AllSome() == false);

        wizards = new() { new Wizard("Merlin", 42), new Wizard("Morgana", 33) };
        Assert(wizards.AllSome() == true);
        Assert(wizards.AllSome(w => w.NbSpells > 40) == false);
        Assert(wizards.AllSome(w => w.NbSpells > 10) == true);

        // FirstSomeOrNone
        wizards = new() { };
        Assert(wizards.FirstSomeOrNone().IsNone);

        wizards = new() { None<Wizard>(), new Wizard("Merlin", 42), new Wizard("Morgana", 33), default/*also None*/ };
        Assert(wizards.FirstSomeOrNone() == new Wizard("Merlin", 42));
        Assert(wizards.FirstSomeOrNone(w => w.NbSpells < 40) == new Wizard("Morgana", 33));

        // LastSomeOrNone
        wizards = new() { None<Wizard>() };
        Assert(wizards.LastSomeOrNone().IsNone);

        wizards = new() { None<Wizard>(), new Wizard("Merlin", 42), new Wizard("Morgana", 33), default/*also None*/ };
        Assert(wizards.LastSomeOrNone() == new Wizard("Morgana", 33));
        Assert(wizards.LastSomeOrNone(w => w.NbSpells > 40) == new Wizard("Merlin", 42));

        // CountSome
        wizards = new() { None<Wizard>() };
        Assert(wizards.CountSome() == 0);

        wizards = new() { new Wizard("Merlin", 42), new Wizard("Morgana", 33), None<Wizard>() };
        Assert(wizards.CountSome() == 2); // count is only on Some's
        Assert(wizards.CountSome(w => w.NbSpells > 10) == 2);
        Assert(wizards.CountSome(w => w.NbSpells > 40) == 1);

        // SelectSome
        wizards = new() { None<Wizard>() };
        var nbSpellsArray = wizards.SelectSome(w => w.NbSpells).ToArray(); // selector is directly on the values
        Assert(nbSpellsArray.Length == 0);

        wizards = new() { new Wizard("Merlin", 42), new Wizard("Morgana", 33), None<Wizard>() };
        nbSpellsArray = wizards.SelectSome(w => w.NbSpells).ToArray(); // selector is directly on the values
        Assert(nbSpellsArray.SequenceEqual(new int[] { 42, 33 }));

        // WhereSome
        wizards = new() { None<Wizard>() };
        var filteredWizards = wizards.WhereSome(w => w.NbSpells > 40); // directly returns IEnumerable<Wizard>
        Assert(filteredWizards.Any() == false);

        wizards = new() { new Wizard("Merlin", 42), new Wizard("Morgana", 33), None<Wizard>() };
        filteredWizards = wizards.WhereSome(w => w.NbSpells > 40).ToArray();
        Assert(filteredWizards.SequenceEqual(new Wizard[] { new Wizard("Merlin", 42) }));

        // ForEachSome
        int sideEffectCounter = 0;
        wizards = new() { None<Wizard>() };
        wizards.ForEachSome(w => sideEffectCounter += w.NbSpells);
        Assert(sideEffectCounter == 0);

        wizards = new() { new Wizard("Merlin", 42), new Wizard("Morgana", 33), None<Wizard>() };
        
        sideEffectCounter = 0;
        wizards.ForEachSome(w => sideEffectCounter += w.NbSpells);
        Assert(sideEffectCounter == 42 + 33);

        sideEffectCounter = 0;
        wizards.ForEachSome(w => w.NbSpells > 40, w => sideEffectCounter += w.NbSpells);
        Assert(sideEffectCounter == 42);


        // UnwrapValues
        wizards = new() { None<Wizard>() };
        var wizardValues = wizards.UnwrapValues(); // directly returns IEnumerable<Wizard>
        Assert(wizardValues.Any() == false);

        wizards = new() { new Wizard("Merlin", 42), new Wizard("Morgana", 33), None<Wizard>() };
        
        wizardValues = wizards.UnwrapValues(); // directly returns IEnumerable<Wizard>
        Assert(wizardValues.SequenceEqual(new Wizard[] { new Wizard("Merlin", 42), new Wizard("Morgana", 33) }));

        wizardValues = wizards.UnwrapValues(w => w.NbSpells > 40); // predicate is applied on the values
        Assert(wizardValues.SequenceEqual(new Wizard[] { new Wizard("Merlin", 42) }));
    }
}
