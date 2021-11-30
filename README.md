# fun-pipe-csharp
Functional pipe methods for csharp. This library aims to provide the ergonomy of functional approaches in C# to improve expressiveness and reduce noise of the code.
* `Opt<T>`: As the Option of Rust or F#: _The option type in F# is used when an actual value might not exist for a named value or variable. An option has an underlying type and can hold a value of that type, or it might not have a value.
* `Res` and `Res<T>`: As the Result of Rust or F#: _The Result<'T,'TFailure> type lets you write error-tolerant code that can be composed._ Here, `Res` is just as an enum holding having Ok or Err states. `Res<T>`, on the other hand, can hold a value of T when `IsOk`. Both hold an `ErrMsg` when `IsErr` that can be customized; furthermore, they can hold on a caught exception, which can be logged, thrown any time or silently ignored.
* `Pipe` and `Pipe<T>`: Pipes work similar to forward pipe operator of F#. `Pipe` (`Pipe<T>`) always holds a `Res` (`Res<T>`), and can be chained with all sorts of Run (Action) and Map methods (Func). Whenever, the pipe reaches the `Err` state at any time, further steps are immediately bypassed. The caller eventually decides what to do with the error (ignore, log, throw). TryRun and TryMap methods hides the lengthy try-catch blocks, while mapping any caught exception to an Err result. Finally, all chaining methods contain also the async versions.

## Example Pipe
Complete example can be found here: [/src/Fun.Pipe/Fun.Pipe.Examples/ExamplePipe.cs](https://github.com/mostlyfun/fun-pipe-csharp/blob/main/src/Fun.Pipe/Fun.Pipe.Examples/ExamplePipe.cs).

Consider a classical file parsing scenario:
* We get the filepath from user. As common, the user may or maynot provide the input. Therefore, `null` check is required for the traditional `GetFilepathFromUserMaybeNull` method; while `GetFilepathFromUser` returns an option which can be automatically handled in a pipe.
* We perform `RiskyParse` text into an integer array, and we are aware that we may encounter exceptions while parsing.
* Finally, we analyze the parsed numbers by calling `LogSumAmounts` on it. This method also throws if it encounters any negative value; logs the sum otherwise.

#### Imperative
We encounter several issues with the imperative style:
* Null-checks are always easy to forget. Although it is added here, it is verbose.
* Two lines for the numbers is lengthy: one is for defining `int[] numbers;` outside the try-block, and one for the assignment within the block.
* Try-catch blocks are necessary; however, they make the code verbose and add unnecessary scopes.

```csharp
static void Imperative(double flip)
{
    string filepath = GetFilepathFromUserMaybeNull(flip);
    if (filepath == null)
    {
        Log("Aborting as the filepath is not provided.");
        return;
    }

    int[] numbers;
    try
    {
        numbers = RiskyParse(filepath);
    }
    catch (Exception e)
    {
        Log("Failed parsing amounts: " + e.Message);
        return;
    }

    try
    {
        LogSumAmounts(numbers);
    }
    catch (Exception e)
    {
        Log("Failed getting total amount: " + e.Message);
    }
}
```

#### With Pipe
Exact same flow can be achieved with pipes without sacrificing expressiveness of the code.
```csharp
static void PipeCompact(double flip)
{
    NewPipe(OnErr.Log)                            // initiates a new pipe which will do nothing but log the errors
    .Map(() => GetFilepathFromUser(flip))         // result at this point is: Ok(filepath) or Err (None's are automatically mapped to Err).
    .TryMap(filepath => RiskyParse(filepath))     // result at this point is: Ok(numbers) or Err (TryMap catches if the parsing throws and maps to Err)
    .TryRun(numbers => LogSumAmounts(numbers));   // result at this point is: Ok or Err (TryRun catches if the summing throws and maps to Err)
}
```
Note that, manual early exits (as `return`s in the imperative) are not required. Furthermore, Err encountered at any stage will not be lost but carried forward.

## Opt
Complete example can be found here: [/src/Fun.Pipe/Fun.Pipe.Examples/ExampleOpt.cs](https://github.com/mostlyfun/fun-pipe-csharp/blob/main/src/Fun.Pipe/Fun.Pipe.Examples/ExampleOpt.cs).

```csharp
// Some of T
var someInt = Some(42);         // implicit T
someInt = Some<int>(42);        // explicit T
Assert(someInt.IsSome, "must-be-IsSome");
Assert(someInt == Some(42), "must-be-Some(12)");
Assert(someInt == 42, "must-be-implicitly-equal-to-12");


// None
var noneFloat = None<float>();  // T has to be explicit with None
Assert(noneFloat.IsNone, "must-be-IsNone");
Assert(noneFloat == None<float>(), "must-be-None");


// Nulls are None, not Some !
var nullString = Some<string>(null);
Assert(nullString.IsNone, "null-must-be-mapped-to-None");


// Get underlying value
var optDuration = Some(TimeSpan.FromSeconds(42));
var duration = optDuration.Unwrap();
Assert(duration.Seconds == 42, "must-be-unwrapped-to-42-secs");
// Get underlying value of None
optDuration = None<TimeSpan>();
try
{
    duration = optDuration.Unwrap();
    Assert(false, "must-have-thrown-an-exception-while-unwrapping-None");
}
catch { }
// Get underlying value with a fallback value when None
duration = optDuration.Unwrap(TimeSpan.FromSeconds(1));
Assert(duration.Seconds == 1, "must-be-unwrapped-to-fallback-value-of-1-secs");


// Always flat
var mage = Some(new Person(Name: "Gandalf", NbHobbies: 42));
var sameMage = Some(mage);
Assert(sameMage.GetType() == typeof(Opt<Person>), "should-be-flat-and-never-be-Opt<Opt<T>>");
Assert(sameMage.IsSome, "one-Unwrap-should-suffice-to-get-value");
Assert(sameMage.Unwrap() == new Person("Gandalf", 42), "one-Unwrap-should-suffice-to-get-value");

var noMage = None<Person>();
var stillNoMage = Some(noMage); // Some of None is still None
Assert(stillNoMage.GetType() == typeof(Opt<Person>), "should-be-flat-and-never-be-Opt<Opt<T>>");
Assert(stillNoMage.IsNone, "one-Unwrap-should-suffice-to-get-value");


// Opt for optional parameters
DataTable GetQuery(string query, Opt<int> timeoutMilliseconds)
{
    // use general timeout when timeoutMilliseconds.IsNone;
    // use timeoutMilliseconds.Unwrap() othewise.
    return new();
}
var getPersons = GetQuery("select persons", None<int>());
var getPersonsWithSpecificTimeout = GetQuery("select-pesons", Some(10800));

// Implicit conversion of values to Opt which is safe; but not vice-versa
getPersonsWithSpecificTimeout = GetQuery("select-pesons", 10800);   // 10800 -> Some(10800)


// Map where None track is bypassed
var someNumber = Some(42f);
var lessThan100 = someNumber.Map(x => MathF.Sqrt(x)).Map(sqrt => sqrt < 10);
Assert(lessThan100 == Some(true), "two-maps-over-42-must-lead-to-true");

var noNumber = None<float>();
lessThan100 = noNumber.Map(x => MathF.Sqrt(x)).Map(sqrt => sqrt < 10);
Assert(lessThan100.IsNone, "None-should-always-be-mapped-to-None");


// Similarly, Run can be used that works only when IsSome
bool ranOnSome = false;
someNumber.Run(() => ranOnSome = true);
Assert(ranOnSome == true, "Run-must-run-when-IsSome");

ranOnSome = false;
someNumber.Run(num => ranOnSome = true);
Assert(ranOnSome == true, "Run-must-run-when-IsSome");

bool ranOnNone = false;
noNumber.Run(num => ranOnNone = true);
Assert(ranOnNone == false, "Run-must-not-run-when-IsNone");


// finally, there is the speceial one that runs when IsNone
ranOnNone = false;
noNumber.RunWhenNone(() => ranOnNone = true);
Assert(ranOnNone == true, "RunWhenNone-must-run-when-IsNone");


// parse
var wick = Opt<Person>.Parse("John-42", s =>
{
    var parts = s.Split('-');
    return new(parts[0], int.Parse(parts[1]));
});
Assert(wick.IsSome, "must-be-parsed-into-Some");
Assert(wick == new Person("John", 42), "must-be-parsed-into-Some");

var badParser = Opt<Person>.Parse("John-42", s => null);
Assert(badParser.IsNone, "null-must-be-mapped-to-None");


// try-parse
var badInput = Opt<Person>.TryParse("expected dash separated string and int, got this, bad input", s =>
{
    var parts = s.Split('-');   // must throw here, TryParse catches it and maps to None
    return new(parts[0], int.Parse(parts[1]));
});
Assert(badInput.IsNone, "exception-must-be-mapped-to-None");


// Opt collections
var noPersons = new List<Opt<Person>>() { noMage, None<Person>() };         // None, None
Assert(noPersons.FirstOrNone().IsNone, "FirstOrNone-must-return-None");
Assert(noPersons.UnwrapValues().Any() == false, "UnwrapValues-must-not-yield-any");

var optPersons = new Opt<Person>[] { noMage, mage, wick, None<Person>() };  // None, Gandalf, None, John
Assert(optPersons.FirstOrNone() == new Person("Gandalf", 42), "FirstOrNone-must-return-Gandalf");
Assert(optPersons.UnwrapValues().Count() == 2, "UnwrapValues-must-yield-two-unwrapped-values");
Assert(string.Join(" | ", optPersons.UnwrapValues().Select(p => p.Name)) == "Gandalf | John", "UnwrapValues-must-directly-yield-unwrapped-persons-Gandalf-&-John");


// regular collections
var valueCollection = new List<Person>();
// note that FirstOrDefault would return 'null' that we want to avoid
Assert(valueCollection.FirstOrNone() == None<Person>(), "FirstOrNone-of-empty-collection-must-return-None");

Person unfortunatelyNullPerson = null;
valueCollection.Add(unfortunatelyNullPerson);
Assert(valueCollection.FirstOrNone() == None<Person>(), "FirstOrNone-of-only-nulls-collection-must-return-None");

valueCollection.Add(new("first-real-person", 42));
valueCollection.Add(new("another-one", 42));
Assert(valueCollection.FirstOrNone().IsSome, "FirstOrNone-must-return-Some");
Assert(valueCollection.FirstOrNone() == new Person("first-real-person", 42), "FirstOrNone-returns-Some-of-the-first-nonnull");
```

## Res
Complete example can be found here: [/src/Fun.Pipe/Fun.Pipe.Examples/ExampleRes.cs](https://github.com/mostlyfun/fun-pipe-csharp/blob/main/src/Fun.Pipe/Fun.Pipe.Examples/ExampleRes.cs).

```csharp
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
```
