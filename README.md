# fun-pipe-csharp
Functional pipe methods for csharp. This library aims to provide to some extent the expresiveness of functional approaches in C#.
The library is available as a NuGet package: [https://www.nuget.org/packages/fun-pipe/](https://www.nuget.org/packages/fun-pipe/).

```powershell
PM> Install-Package fun-pipe
```


## What it helps with
* Proper handling of nulls; make it explicit whenever someting might lack a value with [`Opt`](#opt-in-a-nutshell).
* Proper handling of failures; do not throw or handle, just return Ok(value) or propagate the error and let the caller decide what to do with [`Res`](#res-in-a-nutshell).
* Writing more expressive code using the [continuation methods](#continuation-methods) free of vebose error handling, early returns, etc.

## Briefly
The library provides three types `Opt<T>`, `Res` and `Res<T>` which enable the map and run methods that act similar to the pipeline operator.
* `Opt<T>`: Learning from Option or Maybe types in functional languages: _The option type in F# is used when an actual value might not exist for a named value or variable. An option has an underlying type and can hold a value of that type, or it might not have a value._
  * `Opt<T>`: either `Some(T value)` or `None`.
* `Res` and `Res<T>`: Similar to the Result of F# or Rust: _The Result<'T,'TFailure> type lets you write error-tolerant code that can be composed._ Here, `Res` is just as an enum having Ok or Err states. `Res<T>`, on the other hand, holds a non-null value of T when `IsOk`. Both hold an `ErrMsg` when `IsErr` that can be customized; which can be logged, thrown at any time or silently ignored.
  * `Res`: either `Ok` or `Err(errorMessage)`.
  * `Res<T>`: either `Ok(T value)` or `Err(errorMessage)`.
  * -> _Why not a proper `Res<TOk, TErr>` type?_ The reason is the same why we do not have a proper `Choice` or `Either` type in C#. The problem is explained here: https://github.com/dotnet/runtime/issues/43486, feel free to upvote / watch.
* `Map`, `TryMap`. `Run`, `Try`, `MapAsync`, `TryMapAsync`. `RunAsync`, `TryAsync`: Extension methods with these names are implemented for any `T`, `Opt<T>`, `Res` and `Res<T>`. They work expectedly for `T`:
  * `Map` methods transform the input to another value,
  * `Run` methods execute an action, and returns back the value,
  * `Try...` methods execute the map-or-run lambda within a `try-catch` block, and always return `Res` or `Res<T>` since exceptions/errors are expected,
  * `...Async` methods are exact async counterparts.
* Furthermore, they have a special behavior with `Opt<T>`, `Res` and `Res<T>`:
  * lambdas are executed only for the good paths: `Some` or `Ok`,
  * the methods do nothing but carry on the result when the input is `None` / `Err`; result of any abovementioned methods while the input is `None` / `Err` is also `None` / `Err`;
  * this enables continuation as excellently explained by Scott Wlaschin with [railway analogy](https://www.youtube.com/watch?v=srQt1NAHYC0).
 
# Continuation Methods
## for any T
* `Map`, `TryMap`, `MapAsync`, `TryMapAsync` -> converts value to another, which might be wrapped in `Opt<T>` or `Res<T>`
* `Run`, `Try`, `RunAsync`, `TryAsync` -> executes an action, returns back itself, which might be wrapped in `Res<T>` for `Try` methods.
## for Opt<T>
 * `ThrowIfNone`, `LogIfNone` -> throws or logs the error only if the option is None, returns back itself.
 * `RunIfNone` -> excecutes the parameterless action only if the option is None, returns back itself.
 * `Match` -> maps the option into a value depending on either it is Some or None.
 ## for Res<T>
 * `ThrowIfErr`, `LogIfErr` -> throws or logs the error only if the option is Err, returns back itself.
 * `RunIfErr` -> excecutes the parameterless action only if the option is Err, returns back itself.
 * `Match` -> maps the result into a value depending on either it is Ok or Err.

## Example Pipe: Parse
Complete example can be found here: [/src/Fun.Pipe/Fun.Pipe.Examples/ExamplePipeParse.cs](https://github.com/mostlyfun/fun-pipe-csharp/blob/main/src/Fun.Pipe/Fun.Pipe.Examples/ExamplePipeParse.cs).

Consider a very simplified version of a classical file parsing scenario:
* We get the filepath from user, the user may or may not provide the input. Therefore, `null` check is required for the traditional `GetFilepathFromUserMaybeNull` method, which might easily be forgotten. `GetFilepathFromUser`, on the other hand, returns an option which makes it explicit that `None` case must be handled. Further, options and results are automatically handled in pipe methods (Map, Run, TryMap, TryRun, and their async counterparts).
* We perform `RiskyParse` text into an integer array, and we are aware that we may encounter exceptions while parsing.
* Finally, we analyze the parsed numbers by calling `LogSumAmounts` on it. This method also throws if it encounters any negative value; logs the sum otherwise.

#### Imperative
We encounter several issues with the imperative style:
* Null-checks are always easy to forget. And they are verbose when added.
* Declaring and assigning `numbers` in two separate lines due to the try-catch block is excessive.

```csharp
static int Imperative(double flip)
{
    string filepath = GetFilepathFromUserMaybeNull(flip);
    if (filepath == null)
    {
        Log("Aborting as the filepath is not provided.");
        return -1; // misuse -1 to denote error!
    }

    int[] numbers;
    try
    {
        numbers = RiskyParse(filepath);
    }
    catch (Exception e)
    {
        Log("Failed parsing amounts: " + e.Message);
        return -1; // misuse -1 to denote error!
    }

    try
    {
        return LogAndGetSumAmounts(numbers);
    }
    catch (Exception e)
    {
        Log("Failed getting total amount: " + e.Message);
        return -1; // misuse -1 to denote error!
    }
}
```

#### With Pipe
Exact same flow can be achieved with pipes without sacrificing expressiveness of the code.
```csharp
static Res<int> PipeExplicit(double flip)
{
    // one operation per line to see all type maps; note that 'var's on the lhs's would also be perfectly fine
    Opt<string> filepath = GetFilepathFromUser(flip);               // note that GetFilepathFromUser now rightfully returns Opt<string> as the user might choose not to provide
    Res<int[]> numbers = filepath.TryMap(f => RiskyParse(f));       // we use 'filepath.TryMap' rather than 'TryMap' to prevent the RiskyParse call when filepath.IsNone
    Res<int> sum = numbers.TryMap(n => LogAndGetSumAmounts(n));     // note that int[]->int method LogAndGetSumAmounts is mapped to Res<int[]>->Res<int> with TryMap.
    return sum.MsgIfErr("failed to get sum from file").LogIfErr();  // just to make the error message contain the whole story and log; one could've just returned sum
}
```
or as a single chain:
```csharp
static Res<int> PipeChain(double flip)
{
    return Map(() => GetFilepathFromUser(flip))
        .TryMap(filepath => RiskyParse(filepath))
        .TryMap(numbers => LogAndGetSumAmounts(numbers))
        .MsgIfErr("failed to get sum from file").LogIfErr();
}
```
and finally the corresponding async version:
```csharp
static async Task<Res<int>> PipeExplicitAsync(double flip)
{
    // note that this is exactly same as Example.PipeExplicit; except that methods are replaced with their async counterparts
    Opt<string> filepath = await GetFilepathFromUserAsync(flip);
    Res<int[]> numbers = await filepath.TryMapAsync(f => RiskyParseAsync(f));
    Res<int> sum = await numbers.TryMapAsync(n => LogAndGetSumAmountsAsync(n));
    return sum.MsgIfErr("failed to get sum from file").LogIfErr();
}
```
Note that, manual early exits (as `return`s in the imperative) are not required. `Err`s encountered at any stage will not be lost but carried on. Note that the method ideally  would just return the result, leaving the decision to throw, log, or ignore the error to the caller.

## Example Pipe: Web Request
Complete example can be found here: [/src/Fun.Pipe/Fun.Pipe.Examples/ExamplePipeWebReq.cs](https://github.com/mostlyfun/fun-pipe-csharp/blob/main/src/Fun.Pipe/Fun.Pipe.Examples/ExamplePipeWebReq.cs).

Now assume that:
* we make a web request to get a wizard,
* calculate its updated state, and
* make another request to update the wizard's record.

#### Imperative
```csharp
static async Task<bool> Imperative(double flip, string wizardGuid)
{
    static async Task<Wizard> GetWizardImperative(double flip, string wizardGuid)
    {
        // below lines aim to simulate the failure possilibity of the request, which hopefully is smaller than 0.25 in real life
        bool willResultInNotFound = flip < 0.25;
        string url = willResultInNotFound ? "https://httpbin.org/status/404" : $"https://httpbin.org/anything?data={wizardGuid}";

        // simulate a request to get the object
        HttpResponseMessage response;
        try
        {
            response = await client.GetAsync(url);
        }
        catch (HttpRequestException e)
        {
            Log($"wizard request failed: {e.Message}");
            return null;    // don't know what to return!
        }
        if (response.StatusCode != HttpStatusCode.OK)
        {
            Log($"wizard request failed, status code: {response.StatusCode}");
            return null;    // not really a good idea! maybe `response.EnsureSuccessStatusCode()` can be called to throw instead; but is this meathod allowed to throw here?
                            // actually, it is the caller who must decide what to do with the result.
        }

        // lets simulate deserialization with a simple dash-separated parser
        string content = await response.Content.ReadAsStringAsync();
        var jObj = (JObject)JToken.Parse(content);
        string data = (string)jObj["args"]["data"];
        try
        {
            var wizard = FakeWizardDeserializer(data);
            return wizard;
        }
        catch (Exception e)
        {
            Log($"wizard deserialization failed: {e.Message}");
            return null;    // again, don't know what to return!
        }
    }
    static async Task<bool> UpdateWizardImperative(double flip, string wizardGuid, Wizard updatedWizard)
    {
        // below lines aim to simulate the failure possilibity of the request
        bool willResultInForbidden = flip < 0.25;
        string url = willResultInForbidden ? "https://httpbin.org/status/403" : "https://httpbin.org/status/200";

        // post the updated wizard
        var content = new StringContent($"{wizardGuid}-{updatedWizard.NbSpells}");
        try
        {
            var response = await client.PostAsync(url, content);
            return response.StatusCode == HttpStatusCode.OK; // misuse bool as the status
        }
        catch (Exception e)
        {
            Log($"wizard could not be updated: {e.Message}");
            return false;
        }
    }
    // Run
    var wizard = await GetWizardImperative(flip, wizardGuid);
    if (wizard == null) // misuse of null
        return false;   // misuse of bool as status
    var updatedWizard = DuelBalrogDemon(wizard);
    bool pushed = await UpdateWizardImperative(flip, wizardGuid, wizard);
    return pushed;
}
```

#### With Pipe
```csharp
static async Task<Res> Pipe(double flip, string wizardGuid)
{
    static async Task<Res<Wizard>> GetWizard(double flip, string wizardGuid)
    {
        // below lines aim to simulate the failure possilibity of the request, which hopefully is smaller than 0.25 in real life
        bool willResultInNotFound = flip < 0.25;
        string url = willResultInNotFound ? "https://httpbin.org/status/404" : $"https://httpbin.org/anything?data={wizardGuid}";

        // simulate a request to get the object
        var response = await TryMapAsync(() => client.GetAsync(url)); // HttpRequestException will be caught if there is a connection error
        var okResponse = response.ResFromStatus($"wizard request failed"); // ResFromStatus has special overloads for HttpResponseMessage accepting only 200-OK as Ok, and any other code as Err
        var content = await okResponse.TryMapAsync(response => response.Content.ReadAsStringAsync());

        // lets simulate deserialization with a simple dash-separated parser
        return content.TryMap(c => // the lambda is executed only if `content.IsOk`
        {
            var jObj = (JObject)JToken.Parse(c);
            string data = (string)jObj["args"]["data"];
            return FakeWizardDeserializer(data);
        });
    }
    static async Task<Res> UpdateWizard(double flip, string wizardGuid, Wizard updatedWizard)
    {
        // below lines aim to simulate the failure possilibity of the request
        bool willResultInForbidden = flip < 0.25;
        string url = willResultInForbidden ? "https://httpbin.org/status/403" : "https://httpbin.org/status/200";

        // post the updated wizard
        var content = new StringContent($"{wizardGuid}-{updatedWizard.NbSpells}");
        var response = await TryMapAsync(() => client.PostAsync(url, content));
        return response.Map((HttpResponseMessage x) => x.ResFromStatus("wizard could not be updated")).ToRes();
    }
    // Run
    var wizard = await GetWizard(flip, wizardGuid);
    var pushed = await wizard.Map(w => DuelBalrogDemon(w))
                            .MapAsync(w => UpdateWizard(flip, wizardGuid, w));
    return pushed;
}
```

## Opt in a nutshell
Complete example can be found here: [/src/Fun.Pipe/Fun.Pipe.Examples/ExampleOpt.cs](https://github.com/mostlyfun/fun-pipe-csharp/blob/main/src/Fun.Pipe/Fun.Pipe.Examples/ExampleOpt.cs).

```csharp
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

// Opt as result of value Validation
int number1 = -10, number2 = 42;
var nonneg1 = number1.Validate(x => x >= 0);
Assert(nonneg1.IsNone, "Validate must map value that does not satisfy the validation rule to None");
var nonneg2 = number2.Validate(x => x >= 0);
Assert(nonneg2 == Some(42), "Validate must map value that satisfies the validation rule to Some(value)");


// Parse-or-None by string.Parse{Type}OrNone methods
var notInt = "not-a-number".ParseIntOrNone();
Assert(notInt.IsNone, "ParseIntOrNone must return None when input string is not correct");
var someDate = "2021-05-05".ParseDateOnlyOrNone();
Assert(someDate.IsSome, "ParseDateOnlyOrNone must return Some-DateOnly when input string is correct");


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


// Match
var someWizard = Some(new Wizard("Merlin", 42));
int nbSpells = someWizard.Match(    // match with explicit argument names
    some: w => w.NbSpells,
    none: () => 0);
nbSpells = someWizard.Match(w => w.NbSpells, () => 0);  // match with argument order
nbSpells = someWizard.Match(w => w.NbSpells, 0);        // match with default value for the None case
Assert(nbSpells == 42);
int nbSpellsOfNone = None<Wizard>().Match(w => w.NbSpells, 0);
Assert(nbSpellsOfNone == 0);


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
var valueList = new List<Wizard>();
// note that FirstOrDefault would return 'null' that we want to avoid
Assert(valueList.FirstOrNone() == None<Wizard>(), "FirstOrNone of empty collection must return None");
Assert(valueList.LastOrNone() == None<Wizard>(), "LastOrNone of empty collection must return None");

Wizard unfortunatelyNullPerson = null;
valueList.Add(unfortunatelyNullPerson);
Assert(valueList.FirstOrNone() == None<Wizard>(), "FirstOrNone must skip null's; hence, should return None here");
Assert(valueList.LastOrNone() == None<Wizard>(), "LastOrNone must skip null's; hence, should return None here");

valueList.Add(new Wizard("Saruman", 42));
valueList.Add(new Wizard("Glinda", 42));
valueList.Add(null);  // collection at this point: [ null, Saruman, Glinda, null ]
Assert(valueList.FirstOrNone().IsSome, "FirstOrNone must return Some, since the collection has some non-null values");
Assert(valueList.FirstOrNone() == new Wizard("Saruman", 42), "FirstOrNone must return Saruman, skipping the null");
Assert(valueList.LastOrNone().IsSome, "LastOrNone must return Some, since the collection has some non-null values");
Assert(valueList.LastOrNone() == new Wizard("Glinda", 42), "LastOrNone must return Glinda, skipping the null");


// GetValueOrNone as counterpart of Dictionary.TryGetValue
var dictWizards = new Dictionary<string, Wizard>();
dictWizards.Add("Merlin", new Wizard("Merlin", 42));
dictWizards.Add("Bad Wizard", null);
var gotMerlin = dictWizards.GetValueOrNone("Merlin");
Assert(gotMerlin == Some(new Wizard("Merlin", 42)), "GetValueOrNone must return Some of value when the key exists");
var gotNoWizard = dictWizards.GetValueOrNone("no wizard");
Assert(gotNoWizard.IsNone, "GetValueOrNone must return None when the key is absent");

// eleavate regular collections to Opt collections
List<Opt<Wizard>> optList = valueList.ToOptList();    // must map null's to None
Assert(optList.Count == valueList.Count);
for (var i = 0; i < optList.Count; i++)
    Assert(valueList[i] == null ? optList[i].IsNone : optList[i].IsSome);
// can similarly convert to other enumerables
Opt<Wizard>[] optArr = valueList.ToOptArray();
IEnumerable<Opt<Wizard>> optEnumerable = valueList.ToOptEnumerable();
// finally, Dictionary<TKey, TValue> can be converted into Dictionary<TKey, Opt<TValue>>
var dictMaybeWizards = dictWizards.ToOptDictionary();
Assert(dictMaybeWizards["Merlin"] == new Wizard("Merlin", 42));
Assert(dictMaybeWizards["Bad Wizard"].IsNone);


// Opt collections
var noWizards = new List<Opt<Wizard>>() { None<Wizard>(), None<Wizard>() };
Assert(noWizards.FirstOrNone().IsNone, "FirstOrNone must return None");
Assert(noWizards.LastOrNone().IsNone, "LastOrNone must return None");
Assert(noWizards.UnwrapValues().Any() == false, "UnwrapValues not yield any values, since there is no Some in the collection");

var optPersons = new Opt<Wizard>[] { None<Wizard>(), merlin, new Wizard("Morgana", 42), None<Wizard>() };
Assert(optPersons.FirstOrNone() == merlin, "FirstOrNone must return some Wizard, which is Merlin");
Assert(optPersons.LastOrNone() == new Wizard("Morgana", 42), "LastOrNone must return some Wizard, which is Morgana");
Assert(optPersons.UnwrapValues().Count() == 2, "UnwrapValues must yield two unwrapped Wizard values: Merlin and Morgana");
Assert(string.Join(" | ", optPersons.UnwrapValues().Select(p => p.Name)) == "Merlin | Morgana");
```

## Res in a nutshell
Complete example can be found here: [/src/Fun.Pipe/Fun.Pipe.Examples/ExampleRes.cs](https://github.com/mostlyfun/fun-pipe-csharp/blob/main/src/Fun.Pipe/Fun.Pipe.Examples/ExampleRes.cs).

```csharp
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
Assert(resOneOverZero.ErrorMessage.IsSome && resOneOverZero.ErrorMessage.Unwrap().Contains("[exc] DivideByZeroException: Attempted to divide by zero."));

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


// Keep it flat, none of the nested options is useful:
// * Ok(Err)        -> just Err
// * Ok(Ok(x))      -> just Ok(x)
// Ok(Err) -> Err
Assert(Ok(Err<int>("faulty")).IsErr, "results must be flattened");
Assert(Ok(Err<int>("faulty")).GetType() == typeof(Res<int>), "results must be flattened");
// Ok(Ok(x)) -> Ok(x)
Assert(Ok(Ok(12)) == Ok<int>(12), "results must be flattened");
Assert(Ok(Ok(12)).GetType() == typeof(Res<int>), "results must be flattened");
Assert(Ok(Ok(Ok(12))) == Ok<int>(12), "results must be flattened");
Assert(Ok(Ok(Ok(12))).GetType() == typeof(Res<int>), "results must be flattened");
//Assert(Ok<int>(Ok<float>(12)), "this is not correct; further, does not compile, type-safe");

// Flatness must also be preserved with Res, none of the following combinations is useful:
// * Ok(None)       -> just Err
// * Ok(Some(x))    -> just Ok(x)
// Ok(None) -> Err
Assert(Ok(None<int>()).IsErr, "result-of-option must be flattened");
Assert(Ok(None<int>()).GetType() == typeof(Res<int>), "result-of-option must be flattened");
// Some(Ok(x)) -> Some(x)
Assert(Ok(Some(12)) == Ok<int>(12), "result-of-option must be flattened");
Assert(Ok(Some(12)).GetType() == typeof(Res<int>), "result-of-option must be flattened");
Assert(Ok(Some(Ok(12))) == Ok<int>(12), "result-of-option must be flattened");
Assert(Ok(Some(Ok(12))).GetType() == typeof(Res<int>), "result-of-option must be flattened");


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
Assert(merlin.ToOpt() == Some(new Wizard("Merlin", 42)), "ToOpt must map Ok(x) to Some(x)");
Assert(wizardFromNull.ToOpt() == None<Wizard>(), "ToOpt must map Err to None");
// Opt<T>.ToRes: None->Err, Some(x)->Ok(x)
Assert(Some(new Wizard("Merlin", 42)).ToRes() == Ok(new Wizard("Merlin", 42)), "ToRes must map Some(x) to Ok(x)");
Assert(None<Wizard>().ToRes().IsErr, "ToRes must map None to Err");

// Res collections
var errPersons = new List<Res<Wizard>>() { wizardFromException, Err<Wizard>("problem in grabbing wizard") };  // Err, Err
Assert(errPersons.FirstOrNone().IsNone, "FirstOrNone must return None");
Assert(errPersons.LastOrNone().IsNone, "LastOrNone must return None");
Assert(errPersons.UnwrapValues().Any() == false, "UnwrapValues must not yield any");

var resPersons = new Res<Wizard>[]
   { wizardFromException, new Wizard("Jafar", 42), Err<Wizard>("wrong name"), new Wizard("Albus", 42) };  // Err, Jafar, Err, Albus
Assert(resPersons.FirstOrNone() == new Wizard("Jafar", 42), "FirstOrNone must return Jafar");
Assert(resPersons.LastOrNone() == new Wizard("Albus", 42), "LastOrNone must return Albus");
Assert(resPersons.UnwrapValues().Count() == 2, "UnwrapValues must yield two unwrapped value");
Assert(string.Join(" | ", resPersons.UnwrapValues().Select(p => p.Name)) == "Jafar | Albus");
```
