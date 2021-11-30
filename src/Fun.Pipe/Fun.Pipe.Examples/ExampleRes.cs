using System.Collections.Generic;

namespace Fun.Pipe.Examples;

// Res and especially Res<T> has semantic similarities with Opt<T>.
// However, they are distinguished in the following:
// * Opt<T> is used to explicitly handle optional values which helps avoiding 'null' problems in addition to its expressiveness,
// * Res<T>, on the other hand, is the building block of the Pipe operator which always holds a result.
public static class ExampleRes
{
    internal static void Run()
    {
        // just Ok
        var justOk = Ok();
        Assert(justOk.IsOk, "mut-be-IsOk");

        // Ok of T
        var okInt = Ok(42);         // implicit T
        okInt = Ok<int>(42);        // explicit T
        Assert(okInt.IsOk, "must-be-IsOk");
        Assert(okInt == Ok(42), "must-be-Ok(12)");
        Assert(okInt == 42, "must-be-implicitly-equal-to-12");


        // Err
        var errFloat = Err<float>();  // T has to be explicit with None
        Assert(errFloat.IsErr, "must-be-IsErr");
        Assert(errFloat == Err<float>(), "must-be-Err");


        // Err with explicit message
        var justErr = Err("sth-went-wrong");
        Assert(justErr.ErrMsg == "sth-went-wrong", "correct-error-message");

        // error message can be appended any time if IsErr; notice reassignment onto self as Res is readonly
        justErr = justErr.AddMessageWhenErr("also-another-problem");
        Assert(justErr.ErrMsg == "sth-went-wrong\nalso-another-problem", "correct-error-message");

        // ErrMsg is of Opt<string> which is None when IsOk
        Assert(justOk.ErrMsg.IsNone, "no-error-message-when-IsOk");
        // AddMessageWhenErr can safely be called, which does nothing when IsOk.
        justOk.AddMessageWhenErr("problem-if-IsErr");
        Assert(justOk.ErrMsg.IsNone, "no-error-message-when-IsOk");

        // Nulls are None, not Some !
        var nullString = Some<string>(null);
        Assert(nullString.IsNone, "null-must-be-mapped-to-None");

        // caught exception Exc is likewise Opt<Exception> which is Some only when result IsErr;
        // they are created as results of Try... methods of Pipe which catches the exception and holds in the Res.
        var wronglyParsed = NewPipe(OnErr.None).TryMap(() => int.Parse("nothing-numeric")).Res;
        Assert(wronglyParsed.IsErr, "must-be-IsErr");
        Assert(wronglyParsed.Exc.IsSome, "must-have-caught-the-exception");
        Assert(wronglyParsed.Exc.Unwrap().GetType() == typeof(FormatException), "must-have-caught-FormatException");
        Assert(wronglyParsed.ErrMsg == "Input string was not in a correct format.", "Exc-message-should-also-be-kept-in-ErrMsg");


        // Get underlying value
        string textTimespan = "42";
        var resDuration = NewPipe().Map(() => TimeSpan.FromSeconds(int.Parse(textTimespan))).Res;
        var duration = resDuration.Unwrap();
        Assert(duration.Seconds == 42, "must-be-unwrapped-to-42-secs");
        // Get underlying value of Err
        string textWrongTimespan = "-42-";
        resDuration = NewPipe().TryMap(() => TimeSpan.FromSeconds(int.Parse(textWrongTimespan))).Res;
        try
        {
            duration = resDuration.Unwrap();
            Assert(false, "must-have-thrown-an-exception-while-unwrapping-Err");
        }
        catch { }
        // Get underlying value with a fallback value when Err
        duration = resDuration.Unwrap(TimeSpan.FromSeconds(1));
        Assert(duration.Seconds == 1, "must-be-unwrapped-to-fallback-value-of-1-sec");


        // Always flat
        var mage = Ok(new Person(Name: "Gandalf", NbHobbies: 42));
        var sameMage = Ok(mage);
        Assert(sameMage.GetType() == typeof(Res<Person>), "should-be-flat-and-never-be-Res<Res<T>>");
        Assert(sameMage.IsOk, "one-Unwrap-should-suffice-to-get-value");
        Assert(sameMage.Unwrap() == new Person("Gandalf", 42), "one-Unwrap-should-suffice-to-get-value");

        var errMage = Err<Person>();
        var stillErrMage = Ok(errMage); // Ok of Err is still Err
        Assert(stillErrMage.GetType() == typeof(Res<Person>), "should-be-flat-and-never-be-Res<Res<T>>");
        Assert(stillErrMage.IsErr, "one-Unwrap-should-suffice-to-get-value");


        // Map where None track is bypassed
        var okayNumber = Ok(42f);
        var lessThan100 = okayNumber.Map(x => MathF.Sqrt(x)).Map(sqrt => sqrt < 10);
        Assert(lessThan100 == Ok(true), "two-maps-over-42-must-lead-to-true");

        var noNumber = Err<float>();
        lessThan100 = noNumber.Map(x => MathF.Sqrt(x)).Map(sqrt => sqrt < 10);
        Assert(lessThan100.IsErr, "Err-should-always-be-mapped-to-Err");


        // Similarly, Run can be used that works only when IsSome
        bool ranOnOk = false;
        okayNumber.Run(() => ranOnOk = true);
        Assert(ranOnOk == true, "Run-must-run-when-IsOk");

        ranOnOk = false;
        okayNumber.Run(num => ranOnOk = true);
        Assert(ranOnOk == true, "Run-must-run-when-IsOk");

        bool ranOnErr = false;
        noNumber.Run(num => ranOnErr = true);
        Assert(ranOnErr == false, "Run-must-not-run-when-IsErr");


        // finally, there is the speceial one that runs when IsErr
        ranOnErr = false;
        noNumber.RunWhenErr(() => ranOnErr = true);
        Assert(ranOnErr == true, "RunWhenErr-must-run-when-IsErr");


        // Res collections
        var errPersons = new List<Res<Person>>() { errMage, Err<Person>() };         // Err, Err
        Assert(errPersons.FirstOrNone().IsNone, "FirstOrNone-must-return-None");
        Assert(errPersons.UnwrapValues().Any() == false, "UnwrapValues-must-not-yield-any");

        var resPersons = new Res<Person>[] { errMage, mage, Err<Person>(), new Person("John", 42) };  // Err, Gandalf, Err, John
        Assert(resPersons.FirstOrNone() == new Person("Gandalf", 42), "FirstOrNone-must-return-Gandalf");
        Assert(resPersons.UnwrapValues().Count() == 2, "UnwrapValues-must-yield-two-unwrapped-value");
        Assert(string.Join(" | ", resPersons.UnwrapValues().Select(p => p.Name)) == "Gandalf | John", "UnwrapValues-must-directly-yield-unwrapped-persons-Gandalf-&-John");
    }
}
