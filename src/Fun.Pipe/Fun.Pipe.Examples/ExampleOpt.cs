using System.Collections.Generic;
using System.Data;

namespace Fun.Pipe.Examples;

public static class ExampleOpt
{
    internal static void Run()
    {
        // Some of T
        var someInt = Some(42);     // implicit T
        someInt = Some<int>(42);    // explicit T
        someInt = 42;               // implicit conversion from value to Some(value)
        Assert(someInt.IsSome);
        Assert(someInt == Some(42));
        Assert(someInt == 42, "values can be compared to corresponding Opt types");

        // None of T
        var noneFloat = None<float>();  // T has to be explicit
        Assert(noneFloat.IsNone, "must be IsNone");
        Assert(noneFloat == None<float>(), "must be equal to None");
        //Assert(noneFloat == None<string>(), "this is not correct; further, does not compile, type-safe");

        // null-free
        var nullString = Some<string>(null);
        Assert(nullString.IsNone, "null's must be mapped to None; Some's must be null-free: if it IsSome, it is not null");

        // Some.Unwrap(): when sure that it IsSome
        var optDuration = Some(TimeSpan.FromSeconds(42));
        var duration = optDuration.Unwrap();    // would've thrown if it were IsNone, so must be called only when the Opt is checked to be IsSome
        Assert(duration.Seconds == 42, "must be unwrapped to 42 secs");

        // Some.Unwrap(T): with fallback value
        duration = optDuration.Unwrap(TimeSpan.FromSeconds(10)); // would've returned 10-secs if it were IsNone
        Assert(duration.Seconds == 42, "must be unwrapped to 42 secs");

        // None.Unwrap(): what should be avoided
        optDuration = None<TimeSpan>();
        try
        {
            duration = optDuration.Unwrap();
            Assert(false, "must have thrown an exception while unwrapping None");
        }
        catch { /*expected to end up here*/ }

        // None.Unwrap(T): with fallback value
        duration = optDuration.Unwrap(TimeSpan.FromSeconds(10));
        Assert(duration.Seconds == 10, "must be unwrapped to 10 secs");

        // Keep it flat, none of the nested options is useful:
        // * None(None)     -> just None
        // * None(Some(x))  -> just None
        // * Some(None)     -> just None
        // * Some(Some(x))  -> just Some(x)
        // None(None) -> None
        Assert(None<int>(None<int>()) == None<int>(), "options must be flattened");
        Assert(None<int>(None<int>()).GetType() == typeof(Opt<int>), "options must be flattened");
        // None(Some(x)) -> None
        Assert(None<int>(Some(12)) == None<int>(), "options must be flattened");
        Assert(None<int>(Some(12)).GetType() == typeof(Opt<int>), "options must be flattened");
        // Some(None) -> None
        Assert(Some(None<int>()) == None<int>(), "options must be flattened");
        Assert(Some(None<int>()).GetType() == typeof(Opt<int>), "options must be flattened");
        // Some(Some(x)) -> Some(x)
        Assert(Some(Some(12)) == Some<int>(12), "options must be flattened");
        Assert(Some(Some(12)).GetType() == typeof(Opt<int>), "options must be flattened");
        Assert(Some(Some(Some(12))) == Some<int>(12), "options must be flattened");
        Assert(Some(Some(Some(12))).GetType() == typeof(Opt<int>), "options must be flattened");
        //Assert(Some<int>(Some<float>(12)), "this is not correct; further, does not compile, type-safe");

        // Flatness must also be preserved with Res, none of the following combinations is useful:
        // * None(Err)      -> just None
        // * None(Ok(x))    -> just None
        // * Some(Err)      -> just None
        // * Some(Ok(x))    -> just Some(x)
        // None(Err) -> None
        Assert(None<int>(Err<int>("bad")) == None<int>(), "option-of-result must be flattened");
        Assert(None<int>(Err<int>("bad")).GetType() == typeof(Opt<int>), "option-of-result must be flattened");
        // None(Ok(x)) -> None
        Assert(None<int>(Ok(12)) == None<int>(), "option-of-result must be flattened");
        Assert(None<int>(Ok(12)).GetType() == typeof(Opt<int>), "option-of-result must be flattened");
        // Some(Err) -> None
        Assert(Some(Err<int>("bad")) == None<int>(), "option-of-result must be flattened");
        Assert(Some(Err<int>("bad")).GetType() == typeof(Opt<int>), "option-of-result must be flattened");
        // Some(Ok(x)) -> Some(x)
        Assert(Some(Ok(12)) == Some<int>(12), "option-of-result must be flattened");
        Assert(Some(Ok(12)).GetType() == typeof(Opt<int>), "option-of-result must be flattened");
        Assert(Some(Ok(Some(12))) == Some<int>(12), "option-of-result must be flattened");
        Assert(Some(Ok(Some(12))).GetType() == typeof(Opt<int>), "option-of-result must be flattened");

        // Opt for optional parameters
        static DataTable GetQuery(string query, Opt<int> timeoutMilliseconds)
        {
            // use general timeout when timeoutMilliseconds.IsNone; use timeoutMilliseconds.Unwrap() otherwise.
            return new();
        }
        var getPersons = GetQuery("select persons", None<int>());
        var getPersonsWithSpecificTimeout = GetQuery("select-pesons", Some(10800));
        getPersonsWithSpecificTimeout = GetQuery("select-pesons", 10800);   // implicitly: 10800 -> Some(10800)

        // Map, where None track is bypassed
        var someNumber = Some(42f);
        var lessThan100 = someNumber.Map(x => MathF.Sqrt(x)).Map(sqrt => sqrt < 10);
        Assert(lessThan100 == Some(true), "Map must run on IsSome and lead to true");

        var noNumber = None<float>();
        lessThan100 = noNumber.Map(x => MathF.Sqrt(x)).Map(sqrt => sqrt < 10);
        Assert(lessThan100.IsNone, "Map should be bypassed on IsNone; and should just return None");

        // Run, where None track is bypassed
        someNumber = Some(42f);
        float sideEffect = 10f;
        someNumber.Run(v => { sideEffect += v; });
        Assert(sideEffect == 52f, "Run must run when IsSome, incrementing sideEffect");

        noNumber = None<float>();
        sideEffect = 10f;
        noNumber.Run(v => { sideEffect += v; });
        Assert(sideEffect == 10f, "Run must be bypassed when IsNone, leaving sideEffect unchanged");

        // Note that there exist TryMap, TryRun, MapAsync, RunAsync, TryMapAsync, TryRunAsync versions,
        // which only operate when IsSome and bypass when IsNone, and
        // do what they are expected to do by their name.

        // Complementary methods that run only when IsNone: RunIfNone, LogIfNone, ThrowIfNone
        someNumber = Some(42f);
        sideEffect = 0f;
        someNumber.RunIfNone(() => { sideEffect += 1f; });
        Assert(sideEffect == 0f, "RunIfNone must be bypassed when IsSome, leaving sideEffect unchanged");

        noNumber = None<float>();
        sideEffect = 0f;
        noNumber.RunIfNone(() => { sideEffect += 1f; });
        Assert(sideEffect == 1f, "RunIfNone must be run when IsNone, adding 1f to the sideEffect");

        // Res<T>.ToOpt: Err->None, Ok(x)->Some(x)
        static Wizard ParseWizardMaybe(string str)
        {
            var parts = str.Split('-');
            return new(Name: parts[0], NbSpells: int.Parse(parts[1]));
        }
        Res<Wizard> okWizard = TryMap(() => ParseWizardMaybe("Merlin-42"));
        Opt<Wizard> merlin = okWizard.ToOpt();
        Assert(merlin == Some(new Wizard("Merlin", 42)), "ToOpt must map Ok(x) to Some(x)");
        Res<Wizard> errWizard = TryMap(() => ParseWizardMaybe("badwizardinput"));
        Opt<Wizard> noneWizard = errWizard.ToOpt();
        Assert(noneWizard == None<Wizard>(), "ToOpt must map Err to None");
        // Opt<T>.ToRes: None->Err, Some(x)->Ok(x)
        Assert(merlin.ToRes() == Ok(new Wizard("Merlin", 42)), "ToRes must map Some(x) to Ok(x)");
        Assert(noneWizard.ToRes().IsErr, "ToRes must map None to Err");


        // regular collections
        var valueCollection = new List<Wizard>();
        // note that FirstOrDefault would return 'null' that we want to avoid
        Assert(valueCollection.FirstOrNone() == None<Wizard>(), "FirstOrNone of empty collection must return None");

        Wizard unfortunatelyNullPerson = null;
        valueCollection.Add(unfortunatelyNullPerson);
        Assert(valueCollection.FirstOrNone() == None<Wizard>(), "FirstOrNone must skip null's; hence, should return None here");

        valueCollection.Add(new Wizard("Saruman", 42));
        valueCollection.Add(new Wizard("Glinda", 42)); // collection at this point: [ null, Saruman, Glinda ]
        Assert(valueCollection.FirstOrNone().IsSome, "FirstOrNone must return Some, since the collection has some non-null values");
        Assert(valueCollection.FirstOrNone() == new Wizard("Saruman", 42), "FirstOrNone must return Saruman, skipping the null");

        // Opt collections
        var noWizards = new List<Opt<Wizard>>() { None<Wizard>(), None<Wizard>() };
        Assert(noWizards.FirstOrNone().IsNone, "FirstOrNone must return None");
        Assert(noWizards.UnwrapValues().Any() == false, "UnwrapValues not yield any values, since there is no Some in the collection");

        var optPersons = new Opt<Wizard>[] { None<Wizard>(), merlin, new Wizard("Morgana", 42), None<Wizard>() };
        Assert(optPersons.FirstOrNone() == merlin, "FirstOrNone must return someWizard, which is Merlin");
        Assert(optPersons.UnwrapValues().Count() == 2, "UnwrapValues must yield two unwrapped Wizard values: Merlin and Morgana");
        Assert(string.Join(" | ", optPersons.UnwrapValues().Select(p => p.Name)) == "Merlin | Morgana");
    }
}
