using System.Collections.Generic;

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
public static class ExampleRes
{
    internal static void Run()
    {
        Log("\nRunning Res Examples");
        
        
        // Ok
        var justOk = Ok();
        Assert(justOk.IsOk);
        Assert(justOk.ErrorMessage.IsNone); // no error message when Ok

        
        // Err
        var justErr = Err("something went wrong");
        Assert(justErr.IsErr);
        Assert(justErr.ErrorMessage == Some("something went wrong"));


        // Res from Try method
        int oneOverFive, divider = 5;
        var res = Try(() => oneOverFive = 1 / divider);
        Assert(res.IsOk);

        divider = 0;
        res = Try(() => oneOverFive = 1 / divider); // Try methods run within try-catch blocks;
        Assert(res.IsErr);                          // and exception messages are captured from the exception if the operation fails.
        Assert(res.ErrorMessage.IsSome);
        Assert(res.ErrorMessage.Unwrap().Contains("DivideByZeroException: Attempted to divide by zero."));

       
        // Res return for actions that can fail
        static Res PutWizard(string databaseName, Wizard wizard, bool simulateException)
        {
            if (databaseName == "bad-db")
                return Err("wrong database"); // validation error

            // even if the connection is valid, transaction might fail
            try
            {
                // try to push the wizard here, which will fail if simulateException=true
                if (simulateException)
                    throw new Exception("unlucky");
                return Ok();
            }
            catch (Exception e)
            {
                return Err(e);
            }
        }
        static Res PutWizardConcise(string databaseName, Wizard wizard, bool simulateException)
        {
            if (databaseName == "bad-db")
                return Err("wrong database"); // validation error
            
            return Try(() => // try-catch block above are implicitly present within the Try method.
            {
                if (simulateException)
                    throw new Exception("unlucky");
            });
        }
        Wizard morgana = new("Morgana", 42);
        var pushed = PutWizard("good-db", morgana, false);
        Assert(pushed.IsOk);

        pushed = PutWizardConcise("bad-db", morgana, false);
        Assert(pushed.IsErr);
        Assert(pushed.ErrorMessage.Unwrap().Contains("wrong database"));

        pushed = PutWizardConcise("good-db", morgana, true /*unlucky*/);
        Assert(pushed.IsErr);
        Assert(pushed.ErrorMessage.Unwrap().Contains("unlucky"));


        // Match: both Ok and Err to values
        res = Ok();
        bool isValid = res.Match(true, false); // match to a value; true if IsOk, false if IsErr
        string resultMessage = res.Match("valid", msg => "Err: " + msg); // error case can use ErrorMessage.Unwrap


        // Match: Ok and Err to actions
        static void FakeLog(object message) { }
        res.Match(() => FakeLog("all good"), msg => FakeLog("We got an error: " + msg));


        // Pipe where Err track is bypassed
        static Res PutRecord(Wizard _record, bool simulateErr)
            => simulateErr ? Err("Failed to put record.") : Ok();
        static Res PushLog(bool simulateErr)
            => simulateErr ? Err("Failed to push log.") : Ok();

        res = PutRecord(new("Merlin", 42), false)
                .Map(() => PushLog(false));         // will successfully put-record & push-log
        Assert(res.IsOk);

        res = PutRecord(new("Merlin", 42), false)   // will successfully put-record,
                .Map(() => PushLog(true));          // but fail to push-log
        Assert(res.IsErr && res.ErrorMessage == "Failed to push log.");

        res = PutRecord(new("Merlin", 42), true)    // will fail to put-record,
                .Map(() => PushLog(false));         // PushLog will never be called!
        Assert(res.IsErr && res.ErrorMessage == "Failed to put record.");


        // Alternatively with Try methods
        static void RiskyPutRecord(Wizard _record, bool simulateException)
        {
            if (simulateException)
                throw new Exception("Failed to put record.");
        }
        static void RiskyPushLog(bool simulateException)
        {
            if (simulateException)
                throw new Exception("Failed to push log.");
        }

        res = Try(() => RiskyPutRecord(new Wizard("Merlin", 42), false))    // lambda will be wrapped within a try-catch block
                .Try(() => RiskyPushLog(false));
        Assert(res.IsOk);

        res = Try(() => RiskyPutRecord(new Wizard("Merlin", 42), false))    // will successfully put-record,
                .Try(() => RiskyPushLog(true));                             // but fail to push-log
        Assert(res.IsErr && res.ErrorMessage.Unwrap().Contains("Failed to push log."));

        res = Try(() => RiskyPutRecord(new Wizard("Merlin", 42), true))     // will fail to put-record,
                .Try(() => RiskyPushLog(false));                            // PushLog will never be called!
        Assert(res.IsErr && res.ErrorMessage.Unwrap().Contains("Failed to put record."));
    }
}
