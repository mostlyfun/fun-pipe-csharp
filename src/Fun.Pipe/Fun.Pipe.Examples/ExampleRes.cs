using System.Collections.Generic;

namespace Fun.Pipe.Examples;

// Res and especially Res<T> have similarities with Opt<T>.
// However, they are distinguished in the following:
// * Opt<T> is used to explicitly handle optional values which helps avoiding 'null' problems in addition to its expressiveness,
// * Res<T>, on the other hand, is used for proper handling errors.
// Note that, every the pipeline method with a name that start with 'Try' returns a Res type.
// For instance, if a method returns a Res type, this hints that the method might fail:
// - if the result is Ok() or Ok(value), we know that it succeeded; however,
// - if the result is Err, we would know that it has failed with an associated ErrorMessage.
// -- then, the caller can perform a proper action: throw, pass on the result, etc.
public static class ExampleRes
{
    internal static void Run()
    {
        Log("\nRunning Res Examples");
        // just Ok
        var justOk = Ok();
        Assert(justOk.IsOk);
        Assert(justOk.ErrorMessage.IsNone, "no error message when Ok");
        // just Err
        var justErr = Err("something went wrong");
        Assert(justErr.IsErr);
        Assert(justErr.ErrorMessage.IsSome && justErr.ErrorMessage.Unwrap() == "something went wrong");

        // just Res from Try method
        int oneOverFive, divider = 5;
        var res = Try(() => oneOverFive = 1 / divider);
        Assert(res.IsOk);

        divider = 0;
        res = Try(() => oneOverFive = 1 / divider);
        Assert(res.IsErr);
        // Try methods run within try-catch blocks; and exception messages are captured from the exception if the operation fails.
        Assert(res.ErrorMessage.IsSome && res.ErrorMessage.Unwrap().Contains("DivideByZeroException: Attempted to divide by zero."));

        // Ok<T>: like Ok, but additionally holds a non-null value that can be Unwrap'ped.
        var okInt = Ok(42);     // implicit T
        okInt = Ok<int>(42);    // explicit T
        okInt = 42;             // implicit conversion from value to Ok(value)
        Assert(okInt.IsOk);
        Assert(okInt == Ok(42));
        Assert(okInt == 42, "values can be compared to corresponding Res types");

        // Err<T>: just like Err holding nothing but the ErrorMessage
        var errFloat = Err<float>("something went wrong");  // T has to be explicit
        Assert(errFloat.IsErr, "must be IsErr");
        Assert(errFloat != Err<float>("something went wrong"), "Errors are never equal to anything");

        // Res<T> from TryMap method
        divider = 5;
        var resOneOverFive = TryMap(() => 1 / divider);
        Assert(resOneOverFive.IsOk);
        Assert(resOneOverFive.Unwrap() == 0);
        divider = 0;
        var resOneOverZero = TryMap(() => 1 / divider);
        Assert(resOneOverZero.IsErr);
        Assert(resOneOverZero.ErrorMessage.IsSome && resOneOverZero.ErrorMessage.Unwrap().Contains("DivideByZeroException: Attempted to divide by zero."));

        // null-free
        var nullString = Ok<string>(null);
        Assert(nullString.IsErr, "null's must be mapped to Err; Ok's must be null-free: if it IsOk, it is not null");

        // Res as result of value Validation
        int number1 = -10, number2 = 42;
        var nonneg1 = number1.Validate(x => x >= 0, "found negative");
        Assert(nonneg1.IsErr, "Validate must map value that does not satisfy the validation rule to Err");
        var nonneg2 = number2.Validate(x => x >= 0, "found negative");
        Assert(nonneg2 == Ok(42), "Validate must map value that satisfies the validation rule to Ok(value)");


        // Ok<T>.Unwrap(): when sure that it IsOk
        var resDuration = Ok(TimeSpan.FromSeconds(42));
        var duration = resDuration.Unwrap();    // would've thrown if it were IsErr, so must be called only when the Res is checked to be IsOk
        Assert(duration.Seconds == 42, "must be unwrapped to 42 secs");

        // Ok<T>.Unwrap(T): with fallback value
        duration = resDuration.Unwrap(TimeSpan.FromSeconds(10)); // would've returned 10-secs if it were IsErr
        Assert(duration.Seconds == 42, "must be unwrapped to 42 secs");

        // Err<T>.Unwrap(): what should be avoided
        resDuration = Err<TimeSpan>("sth wrong");
        try
        {
            duration = resDuration.Unwrap();
            Assert(false, "must have thrown an exception while unwrapping None");
        }
        catch { /*expected to end up here*/ }

        // Err<T>.Unwrap(T): with fallback value
        duration = resDuration.Unwrap(TimeSpan.FromSeconds(10));
        Assert(duration.Seconds == 10, "must be unwrapped to 10 secs");


        // Res for actions that can fail
        static Res PutWizard(string databaseName, Wizard wizard, double someNumber)
        {
            if (databaseName == "bad-db")
            {
                // no way we can push to the bad database
                return Err("wrong database");
            }
            // even if the connection is valid, transaction might fail
            try
            {
                // try to push the wizard here, which will fail if someNumber < 0.1
                if (someNumber < 0.1)
                    throw new Exception("unlucky");
                return Ok();
            }
            catch (Exception e)
            {
                return Err(e, nameof(PutWizard));
            }
        }
        Wizard morgana = new("Morgana", 42);
        var pushed = PutWizard("good-db", morgana, 1.0);
        Assert(pushed.IsOk);

        pushed = PutWizard("bad-db", morgana, 1.0);
        Assert(pushed.IsErr);
        Assert(pushed.ErrorMessage.Unwrap().Contains("wrong database"));

        pushed = PutWizard("good-db", morgana, 0.05 /*unlucky*/);
        Assert(pushed.IsErr);
        Assert(pushed.ErrorMessage.Unwrap().Contains("unlucky"));


        // Res<T> for functions that can fail
        static Res<Wizard> ParseWizardRisky(string str)
        {
            if (str == null) // apply validation rules manually
                return Err<Wizard>(errorMessage: "null is passed as wizard str", when: nameof(ParseWizardRisky)); // or just: Err<Wizard>("error message")

            var parts = str.Split('-');             // this should not fail
            try // use try-catch blocks to create errors from caught exceptions
            {
                return Ok(new Wizard(Name: parts[0], NbSpells: int.Parse(parts[1])));
            }
            catch (Exception e)
            {
                return Err<Wizard>(exception: e, when: nameof(ParseWizardRisky)); // or just: Err<Wizard>(e)
            }
        }
        var merlin = ParseWizardRisky("Merlin-42");
        Assert(merlin.IsOk);
        Assert(merlin.Unwrap() == new Wizard("Merlin", 42));

        var wizardFromNull = ParseWizardRisky(null);
        Assert(wizardFromNull.IsErr);
        Assert(wizardFromNull.ErrorMessage.Unwrap().Contains("null is passed as wizard str"));

        var wizardFromException = ParseWizardRisky("badwizardinput"); // will throw due to index out of bounds
        Assert(wizardFromException.IsErr);
        Assert(wizardFromException.ErrorMessage.Unwrap().Contains("IndexOutOfRangeException"));


        // Match
        var okWizard = Ok(new Wizard("Merlin", 42));
        int nbSpells = okWizard.Match(  // match with explicit argument names
            ok: w => w.NbSpells,
            err: _errMsg => 0);
        nbSpells = okWizard.Match(w => w.NbSpells, _ => 0); // match with argument order
        nbSpells = okWizard.Match(w => w.NbSpells, 0);  // match with default value for the None case
        Assert(nbSpells == 42);
        int nbSpellsOfErr = Err<Wizard>("magical error").Match(w => w.NbSpells, 0);
        Assert(nbSpellsOfErr == 0);


        // Map, where Err track is bypassed
        var okHasSpells = merlin.Map(w => w.NbSpells > 0);
        Assert(okHasSpells.IsOk);
        Assert(okHasSpells.Unwrap() == true);

        var errHasSpells = wizardFromNull.Map(w => w.NbSpells > 0); // the map function will never be called
        Assert(errHasSpells.IsErr, "map-lambda is never called on Err, and Err is always mapped to Err");
        Assert(errHasSpells.ErrorMessage.Unwrap() == wizardFromNull.ErrorMessage.Unwrap(), "error message is moved forward");

        // Run, where Err track is bypassed
        var okNumber = Ok(42f);
        float sideEffect = 10f;
        okNumber.Run(v => { sideEffect += v; });
        Assert(sideEffect == 52f, "Run must run when IsOk, incrementing sideEffect");

        var errNumber = Err<float>("for some reason");
        sideEffect = 10f;
        errNumber.Run(v => { sideEffect += v; });
        Assert(sideEffect == 10f, "Run must be bypassed when IsErr, leaving sideEffect unchanged");

        // Complementary methods that run only when IsNone: RunIfErr, LogIfErr, ThrowIfErr
        okNumber = Ok(42f);
        sideEffect = 0f;
        okNumber.RunIfErr(() => { sideEffect += 1f; });
        Assert(sideEffect == 0f, "RunIfErr must be bypassed when IsOk, leaving sideEffect unchanged");

        errNumber = Err<float>("for some reason");
        sideEffect = 0f;
        errNumber.RunIfErr(() => { sideEffect += 1f; });
        Assert(sideEffect == 1f, "RunIfErr must be run when IsErr, adding 1f to the sideEffect");

        // Res<T>.ToOpt: Err->None, Ok(x)->Some(x)
        var zz = merlin.AsOpt();
        Assert(merlin.AsOpt() == Some(new Wizard("Merlin", 42)), "ToOpt must map Ok(x) to Some(x)");
        Assert(wizardFromNull.AsOpt().IsNone, "ToOpt must map Err to None");

        // Res collections
        var errPersons = new List<Res<Wizard>>() { wizardFromException, Err<Wizard>("problem in grabbing wizard") };  // Err, Err
        Assert(errPersons.FirstOkOrNone().IsNone, "FirstOrNone must return None");
        Assert(errPersons.LastOkOrNone().IsNone, "LastOrNone must return None");
        Assert(errPersons.UnwrapValues().Any() == false, "UnwrapValues must not yield any");

        var resPersons = new Res<Wizard>[]
            { wizardFromException, new Wizard("Jafar", 42), Err<Wizard>("wrong name"), new Wizard("Albus", 42) };  // Err, Jafar, Err, Albus
        Assert(resPersons.FirstOkOrNone() == new Wizard("Jafar", 42), "FirstOrNone must return Jafar");
        Assert(resPersons.LastOkOrNone() == new Wizard("Albus", 42), "LastOrNone must return Albus");
        Assert(resPersons.UnwrapValues().Count() == 2, "UnwrapValues must yield two unwrapped value");
        Assert(string.Join(" | ", resPersons.UnwrapValues().Select(p => p.Name)) == "Jafar | Albus");
    }
}
