using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fun.Pipe.Examples;

// Res and especially Res<T> have similarities with Opt<T>.
// However, they are distinguished in the following:
// * Opt<T> is used to explicitly handle optional values which helps avoiding 'null' problems in addition to its expressiveness,
// * Res<T>, on the other hand, is used for proper handling errors.
//
// Note that, every the pipeline method with a name that start with 'Try' returns a Res type.
// For instance, if a method returns a Res type, this hints that the method might fail:
// - if the result is Ok() or Ok(value), we know that it succeeded; however,
// - if the result is Err, we would know that it has failed with an associated ErrorMessage.
// -- then, the caller can perform a proper action: throw, pass on the result, etc.
public static class ExampleResT
{
    internal static async Task Run()
    {
        Log("\nRunning Res<T> Examples");


        // Ok of T
        var okInt = Ok(42);         // implicit T
        okInt = Ok<int>(42);        // explicit T
        okInt = 42;                 // implicit conversion from value to Ok(value)
        Res<int> anotherInt = 42;   // implicit conversion from value to Ok(value)
        Assert(okInt.IsOk  && okInt.Unwrap() == 42);
        Assert(okInt.ErrorMessage.IsNone); // no error message when Ok


        // Err of T
        var errFloat = Err<float>("something went wrong");  // T needs to be explicit
        Assert(errFloat.IsErr);
        Assert(errFloat.ErrorMessage == Some("something went wrong"));  // there exists Some ErrorMessage when IsErr


        // Err of T from exception
        var resInt = Err<int>(new DivideByZeroException("exception message"));
        Assert(resInt.IsErr);
        Assert(resInt.ErrorMessage.Unwrap().Contains("exception message"));
        // alternatively
        try
        {
            int divider = 0;
            resInt = 1 / divider;
        }
        catch (DivideByZeroException e)
        {
            resInt = Err<int>(e);
        }
        Assert(resInt.IsErr);
        Assert(resInt.ErrorMessage.Unwrap().Contains("DivideByZeroException"));


        // Ok: returns Err when null; Res is guaranteed to be null-free
        var errString = Ok<string>(null);
        Assert(errString.IsErr);
        Assert(errString.ErrorMessage.Unwrap().Contains("null"));


        // ToRes: from value
        resInt = 12.ToRes();
        Assert(resInt.IsOk && resInt.Unwrap() == 12);
        
        string nullString = null;
        errString = nullString.ToRes();
        Assert(errString.IsErr);

        
        // ToRes: from value with validation
        var nonneg1 = 42.ToRes(x => x >= 0);
        Assert(nonneg1 == 42);

        var nonneg2 = (-10).ToRes(x => x >= 0);
        Assert(nonneg2 != -10);
        Assert(nonneg2.IsErr);


        // AsRes: from Opt to Res:
        // * None -> Err
        // * Some(x) -> Ok(x)
        static Wizard ParseWizard(string str)
        {
            var parts = str.Split('-');
            return new(Name: parts[0], NbSpells: int.Parse(parts[1]));
        }
        Opt<Wizard> someWizard = "Merlin-42".ParseOrNone(ParseWizard);
        Assert(someWizard == Some(new Wizard("Merlin", 42)));

        Res<Wizard> okWizard = someWizard.AsRes(); // Some(Merlin) -> Ok(Merlin)
        Assert(okWizard == Ok(new Wizard("Merlin", 42)));

        Opt<Wizard> noneWizard = "badwizardinput".ParseOrNone(ParseWizard);
        Assert(noneWizard.IsNone);

        Res<Wizard> errWizard = noneWizard.AsRes(); // None -> Err
        Assert(errWizard.IsErr);


        // Unwrap(): only when sure that it IsOk
        var resDuration = Ok(TimeSpan.FromSeconds(42));
        var duration = resDuration.Unwrap();    // would throw if it were IsErr
        Assert(duration.Seconds == 42);


        // None.Unwrap(): avoid by all means
        resDuration = Err<TimeSpan>("sth wrong");
        try
        {
            duration = resDuration.Unwrap();
            Assert(false, "must have thrown an exception while unwrapping Err");
        }
        catch { /*will end up here*/ }


        // Unwrap(T): with a fallback value
        resDuration = Ok(TimeSpan.FromSeconds(42));
        duration = resDuration.Unwrap(TimeSpan.FromSeconds(10));
        Assert(duration.Seconds == 42); // fallback value is ignored since resDuration.IsOk

        resDuration = Err<TimeSpan>("sth went wrong");
        duration = resDuration.Unwrap(TimeSpan.FromSeconds(10)); // never throws
        Assert(duration.Seconds == 10);


        // Unwrap(() => T): with a lazy fallback value
        Res<Wizard> resultWizard = new Wizard("Merlin", 42); // assume it is expensive to create a wizard, needless to say

        var wizard = resultWizard.Unwrap(new Wizard("Gandalf", 42));
        Assert(wizard.Name == "Merlin");    // Gandalf is already created, although it is immediately thrown away

        wizard = resultWizard.Unwrap(() => new("Gandalf", 42));
        Assert(wizard.Name == "Merlin");    // with lazy version, Gandalf is never created

        wizard = await resultWizard.Unwrap(() => Task.FromResult(new Wizard("Gandalf", 42)));
        Assert(wizard.Name == "Merlin");    // async counterpart of lazy; for instance, when the wizard is queried


        // Match: both Ok(val) and Err to values
        resultWizard = Ok(new Wizard("Merlin", 42));

        int nbSpells = resultWizard.Match(ok: w => w.NbSpells, err: 0);
        nbSpells = resultWizard.Match(w => w.NbSpells, 0);
        nbSpells = resultWizard.Match(w => w.NbSpells, msg => 0); // lazy version for the none case
        Assert(nbSpells == 42);

        string resultMsg = resultWizard.Match(w => "valid", msg => "error: " + msg);
        Assert(resultMsg == "valid");

        int nbSpellsOfErr = Err<Wizard>("db-conn-err").Match(w => w.NbSpells, 0);
        Assert(nbSpellsOfErr == 0);


        // Match: both Some(val) and None to actions
        static void FakeLog(object message) { }
        resultWizard = Ok(new Wizard("Merlin", 42));
        resultWizard.Match(w => FakeLog(w.Name), () => FakeLog("no-wizard-found")); // this would log Merlin if it were not fake
        Err<Wizard>("Err42").Match(w => FakeLog(w.Name), () => FakeLog("no-wizard-found")); // this would log no-wizard-found
        Err<Wizard>("Err42").Match(w => FakeLog(w.Name), msg => FakeLog(msg)); // this would log Err42


        // Equality checks
        Assert(Ok(12.42) == Ok(12.42));                         // value equality
        Assert(Ok(12.42) != Err<double>("errmsg"));             // Err is not equal to anything
        Assert(12.42 != Err<double>("errmsg"));                 // Err is not equal to anything
        Assert(Err<double>("errmsg") != Err<double>("errmsg")); // Err is not equal to anything, even though the error messages are the same

        Assert(Ok(12.42) == 12.42);                             // implicit equality with value
        Assert(Ok(12.42) != 42.12);                             // implicit equality with value
        Assert(Ok(12.42) == Some(12.42));                       // implicit equality with Some of T (Opt)
        Assert(Ok(12.42) != Some(42.22));                       // implicit equality with Some of T (Opt)


        // Res<T> return for functions that can fail
        static Res<Wizard> GetWizard(string databaseName, Guid guid, bool simulateException)
        {
            if (databaseName == "bad-db")
                return Err<Wizard>("wrong database"); // validation error
            
            // even if the connection is valid, transaction might fail
            try
            {
                // try to push the wizard here, which will fail if simulateException=true
                if (simulateException)
                    throw new Exception("unlucky");
                return Ok(new Wizard("Morgana", 42));
            }
            catch (Exception e)
            {
                return Err<Wizard>(e);
            }
        }
        static Res<Wizard> GetWizardConcise(string databaseName, Guid guid, bool simulateException)
        {
            if (databaseName == "bad-db")
                return Err<Wizard>("wrong database"); // validation error

            return TryMap(() => // try-catch block above are implicitly present within the TryMap method.
            {
                if (simulateException)
                    throw new Exception("unlucky");
                return new Wizard("Morgana", 42);
            });
        }

        resultWizard = GetWizardConcise("good-db", new Guid(), false);
        Assert(resultWizard.IsOk);

        resultWizard = GetWizard("bad-db", new(), false);
        Assert(resultWizard.IsErr);
        Assert(resultWizard.ErrorMessage.Unwrap().Contains("wrong database"));

        resultWizard = GetWizardConcise("good-db", new(), true);
        Assert(resultWizard.IsErr);
        Assert(resultWizard.ErrorMessage.Unwrap().Contains("unlucky"));


        // Pipe where Err track is bypassed
        static Wizard UpdateWizard(Wizard wizard, int newNbSpells) // safe operation that updates wizard in memory
            => wizard with { NbSpells = newNbSpells };
        static Res SaveWizard(Wizard wizard, bool simulateException)
        {
            if (simulateException)
                return Err(new Exception("Failed to save wizard."));
            return Ok();
        }
        var res = GetWizardConcise("good-db", new(), false)     // Ok of got wizard
                    .Map(w => UpdateWizard(w, w.NbSpells * 2))  // Ok of updated wizard
                    .Map(w => SaveWizard(w, false));            // Ok
        Assert(res.IsOk);

        res = GetWizardConcise("good-db", new(), false)         // Ok of got wizard
                    .Map(w => UpdateWizard(w, w.NbSpells * 2))  // Ok of updated wizard
                    .Map(w => SaveWizard(w, true));             // Err
        Assert(res.IsErr && res.ErrorMessage.Unwrap().Contains("Failed to save wizard."));

        res = GetWizardConcise("good-db", new(), true)          // Err
                    .Map(w => UpdateWizard(w, w.NbSpells * 2))  // Err: UpdateWizard is never called
                    .Map(w => SaveWizard(w, false));            // Err: SaveWizard is never called
        Assert(res.IsErr && res.ErrorMessage.Unwrap().Contains("unlucky"));


        // Pipe where Err track is bypassed (cont'd)
        var merlin = Ok(new Wizard("Merlin", 42));              // Res<Wizard> (Ok)
        var okHasSpells = merlin.Map(w => w.NbSpells > 0);      // mapped to Res<bool> (Ok)
        Assert(okHasSpells == true);

        errWizard = GetWizardConcise("bad-db", new(), false);   // Res<Wizard> (Err)
        var errHasSpells = errWizard.Map(w => w.NbSpells > 0);  // mapped to Res<bool> (Err); the lambda is never called!
        Assert(errWizard.IsErr && errHasSpells.IsErr);
        Assert(errHasSpells.ErrorMessage.Unwrap() == errWizard.ErrorMessage.Unwrap()); // error message is mvoed forward


        // Run, where Err track is bypassed
        errWizard.Run(w => Console.WriteLine(w.Name));          // nothing will be written

        // Note that there exist TryMap, TryRun, MapAsync, RunAsync, TryMapAsync, TryRunAsync versions,
        // which only operate when IsOk and bypass when IsErr, and do what their names suggest:
        // * Map hints Func, and Run hints Action;
        // * Try means that the lambda will be executed within a try-catch block and result is always a Res type;
        // * Async methods are async counterparts of the map/run methods.


        // Complementary methods that run only when IsErr:
        // RunIfErr, LogIfErr, ThrowIfErr
        var okNumber = Ok(42f);
        float sideEffect = 0f;
        okNumber.RunIfErr(() => { sideEffect += 1f; });
        Assert(sideEffect == 0f);   // RunIfErr is bypassed when IsOk, leaving sideEffect unchanged.

        okNumber = okNumber.MsgIfErr("additional message"); // will do nothing; but would've appended the additional message if it was Err
        okNumber.LogIfErr();                                // will do nothing; but would've logged the error message if it was Err
        okNumber.LogIfErr("additional message");            // will do nothing; but would've logged the error message if it was Err
        okNumber.ThrowIfErr();                              // will do nothing; but would've thrown if it was Err
        okNumber.ThrowIfErr("additional message");          // will do nothing; but would've thrown if it was Err

        var errNumber = Err<float>("parsing error.");
        sideEffect = 0f;
        errNumber.RunIfErr(() => { sideEffect += 1f; });
        Assert(sideEffect == 1f);   // RunIfErr runs when IsErr, adding 1f to the sideEffect.

        errNumber = errNumber.MsgIfErr("additional message.");  // appends the additional message
        Assert(errNumber.ErrorMessage.Unwrap().Contains("parsing error."));
        Assert(errNumber.ErrorMessage.Unwrap().Contains("additional message."));


        // Res collections: IEnumerable<Res<T>>
        List<Res<Wizard>> wizards;

        // AnyOk
        wizards = new() { };
        Assert(wizards.AnyOk() == false);

        wizards = new() { Err<Wizard>("err-msg"), new Wizard("Morgana", 33) };
        Assert(wizards.AnyOk() == true);
        Assert(wizards.AnyOk(w => w.NbSpells > 100) == false);    // predicate is applied directly on the values.
        Assert(wizards.AnyOk(w => w.NbSpells > 10) == true);

        // AllOk
        wizards = new() { Err<Wizard>("err-msg"), new Wizard("Morgana", 42) };
        Assert(wizards.AllOk() == false);

        wizards = new() { new Wizard("Merlin", 42), new Wizard("Morgana", 33) };
        Assert(wizards.AllOk() == true);
        Assert(wizards.AllOk(w => w.NbSpells > 40) == false);
        Assert(wizards.AllOk(w => w.NbSpells > 10) == true);

        // FirstOkOrNone
        wizards = new() { };
        Assert(wizards.FirstOkOrNone().IsNone);

        wizards = new() { Err<Wizard>("err-msg"), new Wizard("Merlin", 42), new Wizard("Morgana", 33), Err<Wizard>("err-msg") };
        Assert(wizards.FirstOkOrNone() == new Wizard("Merlin", 42));
        Assert(wizards.FirstOkOrNone(w => w.NbSpells < 40) == new Wizard("Morgana", 33));

        // LastOkOrNone
        wizards = new() { Err<Wizard>("err-msg") };
        Assert(wizards.LastOkOrNone().IsNone);

        wizards = new() { Err<Wizard>("err-msg"), new Wizard("Merlin", 42), new Wizard("Morgana", 33), Err<Wizard>("err-msg") };
        Assert(wizards.LastOkOrNone() == new Wizard("Morgana", 33));
        Assert(wizards.LastOkOrNone(w => w.NbSpells > 40) == new Wizard("Merlin", 42));

        // CountOk
        wizards = new() { Err<Wizard>("err-msg") };
        Assert(wizards.CountOk() == 0);

        wizards = new() { new Wizard("Merlin", 42), new Wizard("Morgana", 33), Err<Wizard>("err-msg") };
        Assert(wizards.CountOk() == 2); // count is only on Some's
        Assert(wizards.CountOk(w => w.NbSpells > 10) == 2);
        Assert(wizards.CountOk(w => w.NbSpells > 40) == 1);

        // SelectOk
        wizards = new() { Err<Wizard>("err-msg") };
        var nbSpellsArray = wizards.SelectOk(w => w.NbSpells).ToArray(); // selector is directly on the values
        Assert(nbSpellsArray.Length == 0);

        wizards = new() { new Wizard("Merlin", 42), new Wizard("Morgana", 33), Err<Wizard>("err-msg") };
        nbSpellsArray = wizards.SelectOk(w => w.NbSpells).ToArray(); // selector is directly on the values
        Assert(nbSpellsArray.SequenceEqual(new int[] { 42, 33 }));

        // WhereOk
        wizards = new() { Err<Wizard>("err-msg") };
        var filteredWizards = wizards.WhereOk(w => w.NbSpells > 40); // directly returns IEnumerable<Wizard>
        Assert(filteredWizards.Any() == false);

        wizards = new() { new Wizard("Merlin", 42), new Wizard("Morgana", 33), Err<Wizard>("err-msg") };
        filteredWizards = wizards.WhereOk(w => w.NbSpells > 40).ToArray();
        Assert(filteredWizards.SequenceEqual(new Wizard[] { new Wizard("Merlin", 42) }));

        // ForEachOk
        int sideEffectCounter = 0;
        wizards = new() { Err<Wizard>("err-msg") };
        wizards.ForEachOk(w => sideEffectCounter += w.NbSpells);
        Assert(sideEffectCounter == 0);

        wizards = new() { new Wizard("Merlin", 42), new Wizard("Morgana", 33), Err<Wizard>("err-msg") };

        sideEffectCounter = 0;
        wizards.ForEachOk(w => sideEffectCounter += w.NbSpells);
        Assert(sideEffectCounter == 42 + 33);

        sideEffectCounter = 0;
        wizards.ForEachOk(w => w.NbSpells > 40, w => sideEffectCounter += w.NbSpells);
        Assert(sideEffectCounter == 42);


        // UnwrapValues
        wizards = new() { Err<Wizard>("err-msg") };
        var wizardValues = wizards.UnwrapValues(); // directly returns IEnumerable<Wizard>
        Assert(wizardValues.Any() == false);

        wizards = new() { new Wizard("Merlin", 42), new Wizard("Morgana", 33), Err<Wizard>("err-msg") };

        wizardValues = wizards.UnwrapValues(); // directly returns IEnumerable<Wizard>
        Assert(wizardValues.SequenceEqual(new Wizard[] { new Wizard("Merlin", 42), new Wizard("Morgana", 33) }));

        wizardValues = wizards.UnwrapValues(w => w.NbSpells > 40); // predicate is applied on the values
        Assert(wizardValues.SequenceEqual(new Wizard[] { new Wizard("Merlin", 42) }));
    }
}
