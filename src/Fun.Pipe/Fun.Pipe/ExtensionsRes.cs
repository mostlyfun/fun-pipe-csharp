using System.Net;
using System.Net.Http;
namespace Fun;

/// <summary>
/// Static extension or utility methods for Opt, Res, or Pipe.
/// </summary>
public static partial class Extensions
{
    // New
    /// <summary>
    /// Creates Ok result.
    /// </summary>
    public static Res Ok()
        => new(null);
    /// <summary>
    /// Creates an Err result with the given <paramref name="errorMessage"/>.
    /// </summary>
    public static Res Err(string errorMessage)
        => new(errorMessage, null);
    /// <summary>
    /// Creates an Err result occured during <paramref name="when"/> with the given <paramref name="errorMessage"/>.
    /// </summary>
    public static Res Err(string errorMessage, string when)
        => new(errorMessage, when);
    /// <summary>
    /// Creates an Err result with the given <paramref name="exception"/>.
    /// </summary>
    public static Res Err(Exception exception)
        => new(exception, null);
    /// <summary>
    /// Creates an Err result occured during <paramref name="when"/> with the given <paramref name="exception"/>.
    /// </summary>
    public static Res Err(Exception exception, string when)
        => new(exception, when);
    /// <summary>
    /// Creates Ok of <typeparamref name="T"/> with the given <paramref name="value"/>.
    /// Note that 'null' is not allowed and automatically mapped to Err.
    /// </summary>
    public static Res<T> Ok<T>(T value)
        => new(value);
    /// <summary>
    /// Creates an Err result with the given <paramref name="errorMessage"/>.
    /// </summary>
    public static Res<T> Err<T>(string errorMessage)
        => new(errorMessage, null);
    /// <summary>
    /// Creates an Err result occured during <paramref name="when"/> with the given <paramref name="errorMessage"/>.
    /// </summary>
    public static Res<T> Err<T>(string errorMessage, string when)
        => new(errorMessage, when);
    /// <summary>
    /// Creates an Err result with the given <paramref name="exception"/>.
    /// </summary>
    public static Res<T> Err<T>(Exception exception)
        => new(exception, null);
    /// <summary>
    /// Creates an Err result occured during <paramref name="when"/> with the given <paramref name="exception"/>.
    /// </summary>
    public static Res<T> Err<T>(Exception exception, string when)
        => new(exception, when);


    // New - ResFromStatus
    /// <summary>
    /// Creates a result: Ok if <paramref name="successCondition"/> is true; Err with the given <paramref name="failureMessage"/> if false.
    /// </summary>
    public static Res ResFromStatus(this bool successCondition, string failureMessage = "status-false")
        => successCondition ? new() : new(failureMessage, null);
    /// <summary>
    /// Creates a result: Ok if <paramref name="httpStatusCode"/> is 200-OK; Err with the given <paramref name="failureMessage"/> otherwise.
    /// </summary>
    public static Res ResFromStatus(this HttpStatusCode httpStatusCode, string failureMessage = "HttpStatusCode != OK")
        => httpStatusCode == HttpStatusCode.OK ? new() : new(failureMessage, null);
    /// <summary>
    /// Returns Ok(<paramref name="response"/>) if response.StatusCode is 200-OK; Err with the given <paramref name="failureMessage"/> otherwise.
    /// </summary>
    public static Res<HttpResponseMessage> ResFromStatus(this HttpResponseMessage response, string failureMessage = "HttpStatusCode != OK")
        => response.StatusCode == HttpStatusCode.OK ? Ok(response) : new($"[StatusCode: {response.StatusCode}] {failureMessage}", null);
    /// <summary>
    /// Returns back <paramref name="result"/> if result.IsOk and StatusCode of the HttpResponseMessage is 200-OK; Err with the given <paramref name="failureMessage"/> otherwise.
    /// </summary>
    public static Res<HttpResponseMessage> ResFromStatus(this Res<HttpResponseMessage> result, string failureMessage = "HttpStatusCode != OK")
        => result.IsErr ? new(result.ErrorMessage.value, null) : ResFromStatus(result.value, failureMessage);


    // ToRes
    /// <summary>
    /// Converts Opt to Res: maps <paramref name="maybe"/> to Ok of its value when IsSome; to Err when IsNone.
    /// </summary>
    public static Res<T> AsRes<T>(this Opt<T> maybe, string errorMessage = "None->Res")
        => maybe.IsNone ? Err<T>(errorMessage) : Ok(maybe.value);
    /// <summary>
    /// Converts Res of T to just Res; error message is transferred when IsErr; value is forgotten when IsOk.
    /// </summary>
    public static Res AsRes<T>(this Res<T> result)
        => result.IsErr ? new(result.ErrorMessage.Unwrap(), null) : Ok();


    // ToRes - From Value
    /// <summary>
    /// <inheritdoc cref="Ok{T}(T)"/>
    /// </summary>
    public static Res<T> ToRes<T>(this T value)
        => Ok(value);
    /// <summary>
    /// Returns Ok(<paramref name="value"/>) if <paramref name="validator"/>(<paramref name="value"/>) returns true; Err otherwise.
    /// </summary>
    public static Res<T> ToRes<T>(this T value, Func<T, bool> validator)
        => validator(value) ? Ok(value) : Err<T>("Validation failed for value: " + value);
    /// <summary>
    /// Returns Ok(<paramref name="value"/>) if <paramref name="validator"/>(<paramref name="value"/>) returns true; Err with the error message returned by the validator otherwise.
    /// </summary>
    public static Res<T> ToRes<T>(this T value, Func<T, Opt<string>> validator)
    {
        var errorMessage = validator(value);
        return errorMessage.IsNone ? Ok(value) : Err<T>(errorMessage.Unwrap());
    }


    // Res - Err
    /// <summary>
    /// Does nothing and returns itself when <paramref name="result"/> IsOk; throws when IsErr.
    /// </summary>
    public static Res ThrowIfErr(this Res result)
    { if (result.IsErr) throw new ArgumentException(result.ErrorMessage.value); return result; }
    /// <summary>
    /// Does nothing and returns itself when <paramref name="result"/> IsOk; throws with the given additional <paramref name="errorMessage"/> when IsErr.
    /// </summary>
    public static Res ThrowIfErr(this Res result, string errorMessage)
    { if (result.IsErr) { result = result.MsgIfErr(errorMessage); throw new ArgumentException(result.ErrorMessage.value); } return result; }
    /// <summary>
    /// Does nothing when <paramref name="result"/> IsOk; logs its <see cref="Res.ErrorMessage"/> when IsErr.
    /// Returns itself.
    /// </summary>
    public static Res LogIfErr(this Res result)
    { if (result.IsErr) { Console.WriteLine(result); } return result; }
    /// <summary>
    /// Does nothing when <paramref name="result"/> IsOk; logs its <see cref="Res.ErrorMessage"/> with th additional <paramref name="errorMessage"/> when IsErr.
    /// Returns itself.
    /// </summary>
    public static Res LogIfErr(this Res result, string errorMessage)
    { if (result.IsErr) { result = result.MsgIfErr(errorMessage); Console.WriteLine(result); } return result; }
    /// <summary>
    /// Logs the <paramref name="result"/> of the <paramref name="operationName"/>; whether Ok or Err.
    /// </summary>
    public static Res Log(this Res result, string operationName)
    {
        if (result.IsOk)
        {
            Console.WriteLine($"[ok] {operationName}");
            return result;
        }
        return result.LogIfErr($"error while: {operationName}");

    }
    /// <summary>
    /// Logs the <paramref name="result"/> of the <paramref name="operationName"/> with timestamp using given <paramref name="timeFormat"/>; whether Ok or Err.
    /// </summary>
    public static Res LogWithTime(this Res result, string operationName, string timeFormat = "HH:mm:ss")
    {
        string strBegin = DateTime.Now.ToString(timeFormat);
        if (result.IsOk)
            Console.WriteLine($"[ok | {strBegin}] {operationName}");
        else
        {
            Console.WriteLine($"[err | {strBegin}] {operationName}");
            result.LogIfErr();
        }
        return result;
    }
    /// <summary>
    /// Does nothing when <paramref name="result"/> IsOk; runs the given <paramref name="action"/> when IsErr.
    /// Returns itself.
    /// </summary>
    public static Res RunIfErr(this Res result, Action action)
    { if (result.IsErr) action(); return result; }
    /// <summary>
    /// <inheritdoc cref="ThrowIfErr(Res)"/>
    /// </summary>
    public static Res<T> ThrowIfErr<T>(this Res<T> result)
    { if (result.IsErr) throw new ArgumentException(result.ErrorMessage.value); return result; }
    /// <summary>
    /// <inheritdoc cref="ThrowIfErr(Res, string)"/>
    /// </summary>
    public static Res<T> ThrowIfErr<T>(this Res<T> result, string errorMessage)
    {
        if (result.IsErr)
        {
            result = result.MsgIfErr(errorMessage);
            throw new ArgumentException(result.ErrorMessage.value);
        }
        return result;
    }
    /// <summary>
    /// <inheritdoc cref="LogIfErr(Res)"/>
    /// </summary>
    public static Res<T> LogIfErr<T>(this Res<T> result)
    { if (result.ErrorMessage.IsSome) { Console.WriteLine(result); } return result; }
    /// <summary>
    /// <inheritdoc cref="LogIfErr(Res, string)"/>
    /// </summary>
    public static Res<T> LogIfErr<T>(this Res<T> result, string errorMessage)
    { if (result.IsErr) { result = result.MsgIfErr(errorMessage); Console.WriteLine(result); } return result; }
    /// <summary>
    /// Logs the <paramref name="result"/> of the <paramref name="operationName"/>; whether Ok or Err.
    /// </summary>
    public static Res<T> Log<T>(this Res<T> result, string operationName)
    {
        if (result.IsOk)
        {
            Console.WriteLine($"[ok] {operationName}");
            return result;
        }
        return result.LogIfErr($"error while: {operationName}");
    }
    /// <summary>
    /// Logs the <paramref name="result"/> of the <paramref name="operationName"/> with timestamp using given <paramref name="timeFormat"/>; whether Ok or Err.
    /// </summary>
    public static Res<T> LogWithTime<T>(this Res<T> result, string operationName, string timeFormat = "HH:mm:ss")
    {
        string strBegin = DateTime.Now.ToString(timeFormat);
        if (result.IsOk)
            Console.WriteLine($"[ok | {strBegin}] {operationName}");
        else
        {
            Console.WriteLine($"[err | {strBegin}] {operationName}");
            result.LogIfErr();
        }
        return result;
    }
    /// <summary>
    /// <inheritdoc cref="RunIfErr(Res, Action)"/>
    /// </summary>
    public static Res<T> RunIfErr<T>(this Res<T> result, Action action)
    { if (result.IsErr) action(); return result; }


    // Match - Res
    /// <summary>
    /// Maps <paramref name="result"/> into <paramref name="ok"/> whenever result.IsOk; and into <paramref name="err"/>(result.ErrorMessage.Unwrap()) otherwise.
    /// </summary>
    public static TOut Match<TOut>(this Res result, TOut ok, Func<string, TOut> err)
    {
        if (result.IsOk) return ok;
        else return err(result.ErrorMessage.Unwrap());
    }
    /// <summary>
    /// Maps <paramref name="result"/> into <paramref name="ok"/>() whenever result.IsOk; and into <paramref name="err"/>(result.ErrorMessage.Unwrap()) otherwise.
    /// </summary>
    public static TOut Match<TOut>(this Res result, Func<TOut> ok, Func<string, TOut> err)
    {
        if (result.IsOk) return ok();
        else return err(result.ErrorMessage.Unwrap());
    }
    /// <summary>
    /// Maps <paramref name="result"/> into <paramref name="ok"/> whenever result.IsOk; and into <paramref name="err"/> otherwise.
    /// </summary>
    public static TOut Match<TOut>(this Res result, TOut ok, TOut err)
    {
        if (result.IsOk) return ok;
        else return err;
    }
    /// <summary>
    /// Maps <paramref name="result"/> into <paramref name="ok"/>() whenever result.IsOk; and into <paramref name="err"/> otherwise.
    /// </summary>
    public static TOut Match<TOut>(this Res result, Func<TOut> ok, TOut err)
    {
        if (result.IsOk) return ok();
        else return err;
    }
    /// <summary>
    /// Executes <paramref name="ok"/>() whenever <paramref name="result"/>.IsOk; <paramref name="err"/>(result.ErrorMessage.Unwrap()) otherwise.
    /// </summary>
    public static void Match(this Res result, Action ok, Action<string> err)
    {
        if (result.IsOk) ok();
        else err(result.ErrorMessage.Unwrap());
    }
    /// <summary>
    /// Executes <paramref name="ok"/>() whenever <paramref name="result"/>.IsOk; <paramref name="err"/>() otherwise.
    /// </summary>
    public static void Match(this Res result, Action ok, Action err)
    {
        if (result.IsOk) ok();
        else err();
    }
    // Match - Res<T>
    /// <summary>
    /// Maps <paramref name="result"/> into <paramref name="ok"/>(result.Unwrap()) whenever result.IsOk; and into <paramref name="err"/>(result.ErrorMessage.Unwrap()) otherwise.
    /// </summary>
    public static TOut Match<T, TOut>(this Res<T> result, Func<T, TOut> ok, Func<string, TOut> err)
    {
        if (result.IsOk) return ok(result.value);
        else return err(result.ErrorMessage.Unwrap());
    }
    /// <summary>
    /// Maps <paramref name="result"/> into <paramref name="ok"/>(result.Unwrap()) whenever result.IsOk; and into <paramref name="err"/> otherwise.
    /// </summary>
    public static TOut Match<T, TOut>(this Res<T> result, Func<T, TOut> ok, TOut err)
    {
        if (result.IsOk) return ok(result.value);
        else return err;
    }
    /// <summary>
    /// Executes <paramref name="ok"/>(result.Unwrap()) whenever <paramref name="result"/>.IsOk; <paramref name="err"/>(result.ErrorMessage.Unwrap()) otherwise.
    /// </summary>
    public static void Match<T>(this Res<T> result, Action<T> ok, Action<string> err)
    {
        if (result.IsOk) ok(result.value);
        else err(result.ErrorMessage.Unwrap());
    }
    /// <summary>
    /// Executes <paramref name="ok"/>(result.Unwrap()) whenever <paramref name="result"/>.IsOk; <paramref name="err"/>() otherwise.
    /// </summary>
    public static void Match<T>(this Res<T> result, Action<T> ok, Action err)
    {
        if (result.IsOk) ok(result.value);
        else err();
    }


    // Run
    /// <summary>
    /// Runs <paramref name="action"/>() only if result.IsOk, and returns back <paramref name="result"/>.
    /// </summary>
    public static Res Run(this Res result, Action action)
    { if (result.IsOk) action(); return result; }
    /// <summary>
    /// <inheritdoc cref="Run(Res, Action)"/>
    /// </summary>
    public static Res<T> Run<T>(this Res<T> result, Action action)
    { if (result.IsOk) action(); return result; }
    /// <summary>
    /// Runs <paramref name="action"/>(<paramref name="result"/>.Unwrap()) only if result.IsOk, and returns back <paramref name="result"/>.
    /// </summary>
    public static Res<T> Run<T>(this Res<T> result, Action<T> action)
    { if (result.IsOk) action(result.value); return result; }


    // Try: ()->Res
    /// <summary>
    /// Runs the <paramref name="action"/> within a try-catch block.
    /// Returns Ok if succeeds; Err with corresponding message if fails.
    /// </summary>
    public static Res Try(Action action)
    {
        try { action(); return Ok(); }
        catch (Exception e) { return new(e, null); }
    }
    /// <summary>
    /// Does nothing and returns back <paramref name="result"/> when already result.IsErr.
    /// When returns.IsOk; runs the <paramref name="action"/> within a try-catch block; returns Ok if succeeds; Err with corresponding message if fails.
    /// </summary>
    public static Res Try(this Res result, Action action)
    {
        if (result.IsErr) return result;
        try { action(); return Ok(); }
        catch (Exception e) { return new(e, null); }
    }
    /// <summary>
    /// <inheritdoc cref="Try(Res, Action)"/>
    /// </summary>
    public static Res Try<T>(this Res<T> result, Action action)
    {
        if (result.IsErr) new Res(result.ErrorMessage.value, null);
        try { action(); return Ok(); }
        catch (Exception e) { return new(e, null); }
    }
    /// <summary>
    /// Does nothing and returns and returns Err when already <paramref name="maybe"/>.IsNone.
    /// When maybe.IsSome; runs the <paramref name="action"/> within a try-catch block; returns Ok if succeeds; Err with corresponding message if fails.
    /// </summary>
    public static Res Try<T>(this Opt<T> maybe, Action action)
    {
        if (maybe.IsNone) new Res(errNone, null);
        try { action(); return Ok(); }
        catch (Exception e) { return new(e, null); }
    }


    // Try: t->Res
    /// <summary>
    /// Does nothing and returns back <paramref name="result"/> when already result.IsErr.
    /// When returns.IsOk; runs the <paramref name="action"/>(result.Unwrap()) within a try-catch block; returns Ok if succeeds; Err with corresponding message if fails.
    /// </summary>
    public static Res Try<T>(this Res<T> result, Action<T> action)
    {
        if (result.IsErr) return new(result.ErrorMessage.value);
        try { action(result.value); return Ok(); }
        catch (Exception e) { return new(e, null); }
    }
    /// <summary>
    /// Does nothing and returns and returns Err when already <paramref name="maybe"/>.IsNone.
    /// When maybe.IsSome; runs the <paramref name="action"/>(maybe.Unwrap()) within a try-catch block; returns Ok if succeeds; Err with corresponding message if fails.
    /// </summary>
    public static Res Try<T>(this Opt<T> maybe, Action<T> action)
    {
        if (maybe.IsNone) return new(errNone);
        try { action(maybe.value); return Ok(); }
        catch (Exception e) { return new(e, null); }
    }


    // Map: Res->Res
    /// <summary>
    /// Returns back <paramref name="result"/> when IsErr; returns <paramref name="map"/>() when IsOk.
    /// </summary>
    public static Res Map(this Res result, Func<Res> map)
        => result.IsErr ? result : map();
    
    
    // Map: Res->Res<T>
    /// <summary>
    /// Returns back <paramref name="result"/> of <typeref name="TOut"/> when IsErr; returns Ok(<paramref name="map"/>()) when IsOk.
    /// </summary>
    public static Res<TOut> Map<TOut>(this Res result, Func<TOut> map)
        => result.IsErr ? new(result.ErrorMessage.value, null) : Ok(map());
    /// <summary>
    /// Returns back <paramref name="result"/> of <typeparam name="TOut"/> when IsErr; returns <paramref name="map"/>() when IsOk.
    /// </summary>
    public static Res<TOut> Map<TOut>(this Res result, Func<Res<TOut>> map)
        => result.IsErr ? new(result.ErrorMessage.value, null) : map();

    
    // Map: Res<t>->Res
    /// <summary>
    /// <inheritdoc cref="Map(Res, Func{Res})"/>
    /// </summary>
    public static Res Map<T>(this Res<T> result, Func<Res> map)
        => result.IsErr ? new(result.ErrorMessage.value, null) : map();
    /// <summary>
    /// Returns back <paramref name="result"/> when IsErr; returns <paramref name="map"/>(result.Unwrap()) when IsOk.
    /// </summary>
    public static Res Map<T>(this Res<T> result, Func<T, Res> map)
        => result.IsErr ? new(result.ErrorMessage.value, null) : map(result.value);
    
    
    // Map: Res<t>->Res<T>
    /// <summary>
    /// Returns back <paramref name="result"/> of <typeparamref name="TOut"/> when IsErr; returns Ok(<paramref name="map"/>()) when IsOk.
    /// </summary>
    public static Res<TOut> Map<T, TOut>(this Res<T> result, Func<TOut> map)
        => result.IsErr ? new(result.ErrorMessage.value, null) : Ok(map());
    /// <summary>
    /// Returns back <paramref name="result"/> of <typeparamref name="TOut"/> when IsErr; returns Ok(<paramref name="map"/>(result.Unwrap())) when IsOk.
    /// </summary>
    public static Res<TOut> Map<T, TOut>(this Res<T> result, Func<T, TOut> map)
        => result.IsErr ? new(result.ErrorMessage.value, null) : Ok(map(result.value));
    /// <summary>
    /// Returns back <paramref name="result"/> of <typeparamref name="TOut"/> when IsErr; returns <paramref name="map"/>() when IsOk.
    /// </summary>
    public static Res<TOut> Map<T, TOut>(this Res<T> result, Func<Res<TOut>> map)
        => result.IsErr ? new(result.ErrorMessage.value, null) : map();
    /// <summary>
    /// Returns back <paramref name="result"/> of <typeparamref name="TOut"/> when IsErr; returns <paramref name="map"/>(result.Unwrap()) when IsOk.
    /// </summary>
    public static Res<TOut> Map<T, TOut>(this Res<T> result, Func<T, Res<TOut>> map)
        => result.IsErr ? new(result.ErrorMessage.value, null) : map(result.value);


    // Map: Opt<t>->Res<T>
    /// <summary>
    /// Returns Err when <paramref name="maybe"/> IsNone; <paramref name="map"/>() when IsSome.
    /// </summary>
    public static Res<TOut> Map<T, TOut>(this Opt<T> maybe, Func<Res<TOut>> map)
        => maybe.IsNone ? new(errNone, null) : map();
    /// <summary>
    /// Returns Err when <paramref name="maybe"/> IsNone; <paramref name="map"/>(maybe.Unwrap()) when IsSome.
    /// </summary>
    public static Res<TOut> Map<T, TOut>(this Opt<T> maybe, Func<T, Res<TOut>> map)
        => maybe.IsNone ? new(errNone, null) : map(maybe.value);


    // TryMap: ()->Res<T>
    /// <summary>
    /// Tries to return Ok(<paramref name="map"/>()), but returns Err if the method throws.
    /// </summary>
    public static Res<TOut> TryMap<TOut>(Func<TOut> map)
    {
        try { return Ok(map()); }
        catch (Exception e) { return new Res<TOut>(e, null); }
    }
    /// <summary>
    /// Tries to return <paramref name="map"/>(), but returns Err if the method throws.
    /// </summary>
    public static Res<TOut> TryMap<TOut>(Func<Res<TOut>> map)
    {
        try { return map(); }
        catch (Exception e) { return new Res<TOut>(e, null); }
    }
    /// <summary>
    /// Tries to return <paramref name="map"/>().ToRes(), but returns Err if the method throws.
    /// </summary>
    public static Res<TOut> TryMap<TOut>(Func<Opt<TOut>> map)
    {
        try
        {
            var result = map();
            return result.IsNone ? new(errNone, null) : Ok(result.value);
        }
        catch (Exception e)
        {
            return new(e, null);
        }
    }


    // TryMap: t->Res<T>
    /// <summary>
    /// Tries to return Ok(<paramref name="map"/>()); returns Err if the method throws.
    /// </summary>
    public static Res<TOut> TryMap<T, TOut>(this T value, Func<TOut> map)
    {
        try { return Ok(map()); }
        catch (Exception e) { return new Res<TOut>(e, null); }
    }
    /// <summary>
    /// Tries to return <paramref name="map"/>(); returns Err if the method throws.
    /// </summary>
    public static Res<TOut> TryMap<T, TOut>(this T value, Func<Res<TOut>> map)
    {
        try { return map(); }
        catch (Exception e) { return new Res<TOut>(e, null); }
    }
    /// <summary>
    /// Tries to return <paramref name="map"/>().ToRes(); returns Err if the method throws.
    /// </summary>
    public static Res<TOut> TryMap<T, TOut>(this T value, Func<Opt<TOut>> map)
        => TryMap(map);
    /// <summary>
    /// Tries to return Ok(<paramref name="map"/>(<paramref name="value"/>)); returns Err if the method throws.
    /// </summary>
    public static Res<TOut> TryMap<T, TOut>(this T value, Func<T, TOut> map)
    {
        try { return Ok(map(value)); }
        catch (Exception e) { return new Res<TOut>(e, null); }
    }
    /// <summary>
    /// Tries to return <paramref name="map"/>(<paramref name="value"/>); returns Err if the method throws.
    /// </summary>
    public static Res<TOut> TryMap<T, TOut>(this T value, Func<T, Res<TOut>> map)
    {
        try { return map(value); }
        catch (Exception e) { return new Res<TOut>(e, null); }
    }
    /// <summary>
    /// Tries to return <paramref name="map"/>(<paramref name="value"/>).ToRes(); returns Err if the method throws.
    /// </summary>
    public static Res<TOut> TryMap<T, TOut>(this T value, Func<T, Opt<TOut>> map)
    {
        try
        {
            var result = map(value);
            return result.IsNone ? new(errNone, null) : Ok(result.value);
        }
        catch (Exception e)
        {
            return new(e, null);
        }
    }


    // TryMap: Res->Res
    /// <summary>
    /// Returns back <paramref name="result"/> when IsErr.
    /// When IsOk, tries to return <paramref name="map"/>(); returns Err if the method throws.
    /// </summary>
    public static Res TryMap(this Res result, Func<Res> map)
    {
        if (result.IsErr) return result;
        try { return map(); }
        catch (Exception e) { return new(e, null); }
    }
    
    
    // TryMap: Res->Res<T>
    /// <summary>
    /// Returns Err of <typeref name="TOut"/> when <paramref name="result"/> IsErr.
    /// When IsOk, tries to return Ok(<paramref name="map"/>()); returns Err if the method throws.
    /// </summary>
    public static Res<TOut> TryMap<TOut>(this Res result, Func<TOut> map)
    {
        if (result.IsErr) return new(result.ErrorMessage.value, null);
        try { return Ok(map()); }
        catch (Exception e) { return new Res<TOut>(e, null); }
    }
    /// <summary>
    /// Returns Err of <typeref name="TOut"/> when <paramref name="result"/> IsErr.
    /// When IsOk, tries to return <paramref name="map"/>(); returns Err if the method throws.
    /// </summary>
    public static Res<TOut> TryMap<TOut>(this Res result, Func<Res<TOut>> map)
    {
        if (result.IsErr) return new(result.ErrorMessage.value, null);
        try { return map(); }
        catch (Exception e) { return new Res<TOut>(e, null); }
    }
    /// <summary>
    /// Returns Err of <typeref name="TOut"/> when <paramref name="result"/> IsErr.
    /// When IsOk, tries to return ToRes(<paramref name="map"/>()); returns Err if the method throws.
    /// Note that ToRes maps Some to Ok, and None to Err.
    /// </summary>
    public static Res<TOut> TryMap<TOut>(this Res result, Func<Opt<TOut>> map)
    {
        if (result.IsErr) return new(result.ErrorMessage.value, null);
        return TryMap(map);
    }
    
    
    // TryMap: Res<t>->Res<T>
    /// <summary>
    /// Returns Err of <typeref name="TOut"/> when <paramref name="result"/> IsErr.
    /// When IsOk, tries to return Ok(<paramref name="map"/>()); returns Err if the method throws.
    /// </summary>
    public static Res<TOut> TryMap<T, TOut>(this Res<T> result, Func<TOut> map)
    {
        if (result.IsErr) return new(result.ErrorMessage.value, null);
        try { return Ok(map()); }
        catch (Exception e) { return new Res<TOut>(e, null); }
    }
    /// <summary>
    /// Returns Err of <typeref name="TOut"/> when <paramref name="result"/> IsErr.
    /// When IsOk, tries to return <paramref name="map"/>(); returns Err if the method throws.
    /// </summary>
    public static Res<TOut> TryMap<T, TOut>(this Res<T> result, Func<Res<TOut>> map)
    {
        if (result.IsErr) return new(result.ErrorMessage.value, null);
        try { return map(); }
        catch (Exception e) { return new Res<TOut>(e, null); }
    }
    /// <summary>
    /// Returns Err of <typeref name="TOut"/> when <paramref name="result"/> IsErr.
    /// When IsOk, tries to return Ok(<paramref name="map"/>()); returns Err if the method throws.
    /// </summary>
    public static Res<TOut> TryMap<T, TOut>(this Res<T> result, Func<T, TOut> map)
    {
        if (result.IsErr) return new(result.ErrorMessage.value, null);
        try { return Ok(map(result.value)); }
        catch (Exception e) { return new Res<TOut>(e, null); }
    }
    /// <summary>
    /// Returns Err of <typeref name="TOut"/> when <paramref name="result"/> IsErr.
    /// When IsOk, tries to return <paramref name="map"/>(result.Unwrap()); returns Err if the method throws.
    /// </summary>
    public static Res<TOut> TryMap<T, TOut>(this Res<T> result, Func<T, Res<TOut>> map)
    {
        if (result.IsErr) return new(result.ErrorMessage.value, null);
        try { return map(result.value); }
        catch (Exception e) { return new Res<TOut>(e, null); }
    }

    
    // TryMap: Opt<t>->Res<T>
    /// <summary>
    /// Returns Err of <typeref name="TOut"/> when <paramref name="maybe"/> IsNone.
    /// When IsSome, tries to return Ok(<paramref name="map"/>()); returns Err if the method throws.
    /// </summary>
    public static Res<TOut> TryMap<T, TOut>(this Opt<T> maybe, Func<TOut> map)
    {
        if (maybe.IsNone) return new(errNone, null);
        try { return Ok(map()); }
        catch (Exception e) { return new Res<TOut>(e, null); }
    }
    /// <summary>
    /// Returns Err of <typeref name="TOut"/> when <paramref name="maybe"/> IsNone.
    /// When IsSome, tries to return <paramref name="map"/>().ToRes(); returns Err if the method throws.
    /// Note that ToRes maps Some to Ok, and None to Err.
    /// </summary>
    public static Res<TOut> TryMap<T, TOut>(this Opt<T> maybe, Func<Opt<TOut>> map)
    {
        if (maybe.IsNone) return new(errNone, null);
        return TryMap(map);
    }
    /// <summary>
    /// Returns Err of <typeref name="TOut"/> when <paramref name="maybe"/> IsNone.
    /// When IsSome, tries to return <paramref name="map"/>(); returns Err if the method throws.
    /// </summary>
    public static Res<TOut> TryMap<T, TOut>(this Opt<T> maybe, Func<Res<TOut>> map)
    {
        if (maybe.IsNone) return new(errNone, null);
        try { return map(); }
        catch (Exception e) { return new Res<TOut>(e, null); }
    }
    /// <summary>
    /// Returns Err of <typeref name="TOut"/> when <paramref name="maybe"/> IsNone.
    /// When IsSome, tries to return Ok(<paramref name="map"/>(maybe.Unwrap())); returns Err if the method throws.
    /// </summary>
    public static Res<TOut> TryMap<T, TOut>(this Opt<T> maybe, Func<T, TOut> map)
    {
        if (maybe.IsNone) return new(errNone, null);
        try { return Ok(map(maybe.value)); }
        catch (Exception e) { return new Res<TOut>(e, null); }
    }
    /// <summary>
    /// Returns Err of <typeref name="TOut"/> when <paramref name="maybe"/> IsNone.
    /// When IsSome, tries to return <paramref name="map"/>(maybe.Unwrap()).ToRes(); returns Err if the method throws.
    /// Note that ToRes maps Some to Ok, and None to Err.
    /// </summary>
    public static Res<TOut> TryMap<T, TOut>(this Opt<T> maybe, Func<T, Opt<TOut>> map)
    {
        if (maybe.IsNone) return new(errNone, null);
        return TryMap(maybe.value, map);
    }
    /// <summary>
    /// Returns Err of <typeref name="TOut"/> when <paramref name="maybe"/> IsNone.
    /// When IsSome, tries to return <paramref name="map"/>(maybe.Unwrap()); returns Err if the method throws.
    /// </summary>
    public static Res<TOut> TryMap<T, TOut>(this Opt<T> maybe, Func<T, Res<TOut>> map)
    {
        if (maybe.IsNone) return new(errNone, null);
        try { return map(maybe.value); }
        catch (Exception e) { return new Res<TOut>(e, null); }
    }


    #region ASYNC

    // Run
    /// <summary>
    /// <inheritdoc cref="Run{T}(Opt{T}, Action)"/>
    /// </summary>
    public static Task<Res<T>> RunAsync<T>(this Res<T> result, Func<Task> action)
    { if (result.IsOk) action(); return Task.FromResult(result); }
    /// <summary>
    /// <inheritdoc cref="Run{T}(Res{T}, Action{T})"/>
    /// </summary>
    public static Task<Res<T>> RunAsync<T>(this Res<T> result, Func<T, Task> action)
    { if (result.IsOk) action(result.value); return Task.FromResult(result); }


    // Try: ()->Res
    /// <summary>
    /// <inheritdoc cref="Try(Action)"/>
    /// </summary>
    public static async Task<Res> TryAsync(Func<Task> action)
    {
        try { await action(); return new(); }
        catch (Exception e) { return new(e, null); }
    }
    /// <summary>
    /// <inheritdoc cref="Try(Res, Action)"/>
    /// </summary>
    public static async Task<Res> TryAsync(this Res result, Func<Task> action)
    {
        if (result.IsErr) return result;
        try { await action(); return new(); }
        catch (Exception e) { return new(e, null); }
    }
    /// <summary>
    /// <inheritdoc cref="Try{T}(Res{T}, Action)"/>
    /// </summary>
    public static async Task<Res> TryAsync<T>(this Res<T> result, Func<Task> action)
    {
        if (result.IsErr) return new(result.ErrorMessage.value);
        try { await action(); return new(); }
        catch (Exception e) { return new(e, null); }
    }
    /// <summary>
    /// <inheritdoc cref="Try{T}(Opt{T}, Action)"/>
    /// </summary>
    public static async Task<Res> TryAsync<T>(this Opt<T> maybe, Func<Task> action)
    {
        if (maybe.IsNone) return new(errNone);
        try { await action(); return new(); }
        catch (Exception e) { return new(e, null); }
    }


    // Try: t->Res
    /// <summary>
    /// <inheritdoc cref="Try{T}(Opt{T}, Action{T})"/>
    /// </summary>
    public static async Task<Res> TryAsync<T>(this Res<T> result, Func<T, Task> action)
    {
        if (result.IsErr) return new(result.ErrorMessage.value);
        try { await action(result.value); return new(); }
        catch (Exception e) { return new(e, null); }
    }
    /// <summary>
    /// <inheritdoc cref="Try{T}(Opt{T}, Action{T})"/>
    /// </summary>
    public static async Task<Res> TryAsync<T>(this Opt<T> maybe, Func<T, Task> action)
    {
        if (maybe.IsNone) return new(errNone);
        try { await action(maybe.value); return new(); }
        catch (Exception e) { return new(e, null); }
    }


    // Map: Res->Res
    /// <summary>
    /// <inheritdoc cref="Map(Res, Func{Res})"/>
    /// </summary>
    public static Task<Res> MapAsync(this Res result, Func<Task<Res>> map)
        => result.IsErr ? Task.FromResult(new Res(result.ErrorMessage.value, null)) : map();
    
    
    // Map: Res->Res<T>
    /// <summary>
    /// <inheritdoc cref="Map{TOut}(Res, Func{TOut})"/>
    /// </summary>
    public static async Task<Res<TOut>> MapAsync<TOut>(this Res result, Func<Task<TOut>> map)
        => result.IsErr ? new(result.ErrorMessage.value, null) : Ok(await map());
    /// <summary>
    /// <inheritdoc cref="Map{TOut}(Res, Func{Res{TOut}})"/>
    /// </summary>
    public static Task<Res<TOut>> MapAsync<TOut>(this Res result, Func<Task<Res<TOut>>> map)
        => result.IsErr ? Task.FromResult<Res<TOut>>(new(result.ErrorMessage.value, null)) : map();


    // Map: Res<t>->Res
    /// <summary>
    /// <inheritdoc cref="Map(Res, Func{Res})"/>
    /// </summary>
    public static Task<Res> MapAsync<T>(this Res<T> result, Func<Task<Res>> map)
        => result.IsErr ? Task.FromResult(new Res(result.ErrorMessage.value, null)) : map();
    /// <summary>
    /// <inheritdoc cref="Map{T}(Res{T}, Func{T, Res})"/>
    /// </summary>
    public static Task<Res> MapAsync<T>(this Res<T> result, Func<T, Task<Res>> map)
        => result.IsErr ? Task.FromResult(new Res(result.ErrorMessage.value, null)) : map(result.value);
    
    
    // Map: Res<t>->Res<T>
    /// <summary>
    /// <inheritdoc cref="Map{T, TOut}(Res{T}, Func{TOut})"/>
    /// </summary>
    public static async Task<Res<TOut>> MapAsync<T, TOut>(this Res<T> result, Func<Task<TOut>> map)
        => result.IsErr ? new(result.ErrorMessage.value, null) : Ok(await map());
    /// <summary>
    /// <inheritdoc cref="Map{T, TOut}(Res{T}, Func{T, TOut})"/>
    /// </summary>
    public static async Task<Res<TOut>> MapAsync<T, TOut>(this Res<T> result, Func<T, Task<TOut>> map)
        => result.IsErr ? new(result.ErrorMessage.value, null) : Ok(await map(result.value));
    /// <summary>
    /// <inheritdoc cref="Map{T, TOut}(Res{T}, Func{Res{TOut}})"/>
    /// </summary>
    public static Task<Res<TOut>> MapAsync<T, TOut>(this Res<T> result, Func<Task<Res<TOut>>> map)
        => result.IsErr ? Task.FromResult(new Res<TOut>(result.ErrorMessage.value, null)) : map();
    /// <summary>
    /// <inheritdoc cref="Map{T, TOut}(Res{T}, Func{T, Res{TOut}})"/>
    /// </summary>
    public static Task<Res<TOut>> MapAsync<T, TOut>(this Res<T> result, Func<T, Task<Res<TOut>>> map)
        => result.IsErr ? Task.FromResult(new Res<TOut>(result.ErrorMessage.value, null)) : map(result.value);


    // Map: Opt<t>->Res<T>
    /// <summary>
    /// <inheritdoc cref="Map{T, TOut}(Opt{T}, Func{Res{TOut}})"/>
    /// </summary>
    public static Task<Res<TOut>> MapAsync<T, TOut>(this Opt<T> maybe, Func<Task<Res<TOut>>> map)
        => maybe.IsNone ? Task.FromResult(new Res<TOut>("None->Res", "Map")) : map();
    /// <summary>
    /// <inheritdoc cref="Map{T, TOut}(Opt{T}, Func{T, Res{TOut}})"/>
    /// </summary>
    public static Task<Res<TOut>> MapAsync<T, TOut>(this Opt<T> maybe, Func<T, Task<Res<TOut>>> map)
        => maybe.IsNone ? Task.FromResult(new Res<TOut>("None->Res", "Map")) : map(maybe.value);


    // TryMap: ()->Res<T>
    /// <summary>
    /// <inheritdoc cref="TryMap{TOut}(Func{TOut})"/>
    /// </summary>
    public static async Task<Res<TOut>> TryMapAsync<TOut>(Func<Task<TOut>> map)
    {
        try { return new(await map()); }
        catch (Exception e) { return new(e, null); }
    }
    /// <summary>
    /// <inheritdoc cref="TryMap{TOut}(Func{Res{TOut}})"/>
    /// </summary>
    public static async Task<Res<TOut>> TryMapAsync<TOut>(Func<Task<Res<TOut>>> map)
    {
        try { return await map(); }
        catch (Exception e) { return new(e, null); }
    }
    /// <summary>
    /// <inheritdoc cref="TryMap{TOut}(Func{Opt{TOut}})"/>
    /// </summary>
    public static async Task<Res<TOut>> TryMapAsync<TOut>(Func<Task<Opt<TOut>>> map)
    {
        try
        {
            var result = await map();
            return result.IsNone ? new(errNone, null) : Ok(result.value);
        }
        catch (Exception e) { return new(e, null); }
    }


    // TryMap: t->Res<T>
    /// <summary>
    /// <inheritdoc cref="TryMap{TOut}(Func{TOut})"/>
    /// </summary>
    public static async Task<Res<TOut>> TryMapAsync<T, TOut>(this T value, Func<Task<TOut>> map)
    {
        try { return new(await map()); }
        catch (Exception e) { return new(e, null); }
    }
    /// <summary>
    /// <inheritdoc cref="TryMap{TOut}(Func{Res{TOut}})"/>
    /// </summary>
    public static async Task<Res<TOut>> TryMapAsync<T, TOut>(this T value, Func<Task<Res<TOut>>> map)
    {
        try { return await map(); }
        catch (Exception e) { return new(e, null); }
    }
    /// <summary>
    /// <inheritdoc cref="TryMap{TOut}(Func{Opt{TOut}})"/>
    /// </summary>
    public static async Task<Res<TOut>> TryMapAsync<T, TOut>(this T value, Func<Task<Opt<TOut>>> map)
    {
        try
        {
            var result = await map();
            return result.IsNone ? new(errNone, null) : Ok(result.value);
        }
        catch (Exception e) { return new(e, null); }
    }
    /// <summary>
    /// <inheritdoc cref="TryMap{T, TOut}(T, Func{T, TOut})"/>
    /// </summary>
    public static async Task<Res<TOut>> TryMapAsync<T, TOut>(this T value, Func<T, Task<TOut>> map)
    {
        try { return new(await map(value)); }
        catch (Exception e) { return new(e, null); }
    }
    /// <summary>
    /// <inheritdoc cref="TryMap{T, TOut}(T, Func{T, Res{TOut}})"/>
    /// </summary>
    public static async Task<Res<TOut>> TryMapAsync<T, TOut>(this T value, Func<T, Task<Res<TOut>>> map)
    {
        try { return await map(value); }
        catch (Exception e) { return new(e, null); }
    }
    /// <summary>
    /// <inheritdoc cref="TryMap{T, TOut}(T, Func{T, Opt{TOut}})"/>
    /// </summary>
    public static async Task<Res<TOut>> TryMapAsync<T, TOut>(this T value, Func<T, Task<Opt<TOut>>> map)
    {
        try
        {
            var result = await map(value);
            return result.IsNone ? new(errNone, null) : Ok(result.value);
        }
        catch (Exception e) { return new(e, null); }
    }


    // TryMap: Res->Res
    /// <summary>
    /// <inheritdoc cref="TryMap(Res, Func{Res})"/>
    /// </summary>
    public static async Task<Res> TryMapAsync(this Res result, Func<Task<Res>> map)
    {
        try { return await map(); }
        catch (Exception e) { return new(e, null); }
    }
    
    
    // TryMap: Res->Res<T>
    /// <summary>
    /// <inheritdoc cref="TryMap{TOut}(Res, Func{TOut})"/>
    /// </summary>
    public static async Task<Res<TOut>> TryMapAsync<TOut>(this Res result, Func<Task<TOut>> map)
    {
        if (result.IsErr) return new(result.ErrorMessage.value, null);
        try { return Ok(await map()); }
        catch (Exception e) { return new(e, null); }
    }
    /// <summary>
    /// <inheritdoc cref="TryMap{TOut}(Res, Func{Res{TOut}})"/>
    /// </summary>
    public static Task<Res<TOut>> TryMapAsync<TOut>(this Res result, Func<Task<Res<TOut>>> map)
    {
        if (result.IsErr) return Task.FromResult(new Res<TOut>(result.ErrorMessage.value, null));
        try { return map(); }
        catch (Exception e) { return Task.FromResult(new Res<TOut>(e, null)); }
    }
    /// <summary>
    /// <inheritdoc cref="TryMap{TOut}(Res, Func{Opt{TOut}})"/>
    /// </summary>
    public static async Task<Res<TOut>> TryMapAsync<TOut>(this Res result, Func<Task<Opt<TOut>>> map)
    {
        if (result.IsErr) return new(result.ErrorMessage.value, null);
        try
        {
            var res = await map();
            return res.IsNone ? new(errNone, null) : Ok(res.value);
        }
        catch (Exception e) { return new(e, null); }
    }


    // TryMap: Res<t>->Res<T>
    /// <summary>
    /// <inheritdoc cref="TryMap{T, TOut}(Res{T}, Func{TOut})"/>
    /// </summary>
    public static async Task<Res<TOut>> TryMapAsync<T, TOut>(this Res<T> result, Func<Task<TOut>> map)
    {
        if (result.IsErr) return new(result.ErrorMessage.value, null);
        try { return Ok(await map()); }
        catch (Exception e) { return new(e, null); }
    }
    /// <summary>
    /// <inheritdoc cref="TryMap{T, TOut}(Res{T}, Func{Res{TOut}})"/>
    /// </summary>
    public static Task<Res<TOut>> TryMapAsync<T, TOut>(this Res<T> result, Func<Task<Res<TOut>>> map)
    {
        if (result.IsErr) return Task.FromResult(new Res<TOut>(result.ErrorMessage.value, null));
        try { return map(); }
        catch (Exception e) { return Task.FromResult(new Res<TOut>(e, null)); }
    }
    /// <summary>
    /// <inheritdoc cref="TryMap{T, TOut}(Res{T}, Func{Opt{TOut}})"/>
    /// </summary>
    public static async Task<Res<TOut>> TryMapAsync<T, TOut>(this Res<T> result, Func<Task<Opt<TOut>>> map)
    {
        if (result.IsErr) return new(result.ErrorMessage.value, null);
        try
        {
            var res = await map();
            return res.IsNone ? new(errNone, null) : Ok(res.value);
        }
        catch (Exception e) { return new(e, null); }
    }
    /// <summary>
    /// <inheritdoc cref="TryMap{T, TOut}(Res{T}, Func{T, TOut})"/>
    /// </summary>
    public static async Task<Res<TOut>> TryMapAsync<T, TOut>(this Res<T> result, Func<T, Task<TOut>> map)
    {
        if (result.IsErr) return new(result.ErrorMessage.value, null);
        try { return Ok(await map(result.value)); }
        catch (Exception e) { return new(e, null); }
    }
    /// <summary>
    /// <inheritdoc cref="TryMap{T, TOut}(Res{T}, Func{T, Res{TOut}})"/>
    /// </summary>
    public static Task<Res<TOut>> TryMapAsync<T, TOut>(this Res<T> result, Func<T, Task<Res<TOut>>> map)
    {
        if (result.IsErr) return Task.FromResult(new Res<TOut>(result.ErrorMessage.value, null));
        try { return map(result.value); }
        catch (Exception e) { return Task.FromResult(new Res<TOut>(e, null)); }
    }
    /// <summary>
    /// <inheritdoc cref="TryMap{T, TOut}(Res{T}, Func{T, Opt{TOut}})"/>
    /// </summary>
    public static async Task<Res<TOut>> TryMapAsync<T, TOut>(this Res<T> result, Func<T, Task<Opt<TOut>>> map)
    {
        if (result.IsErr) return new(result.ErrorMessage.value, null);
        try
        {
            var res = await map(result.value);
            return res.IsNone ? new(errNone, null) : Ok(res.value);
        }
        catch (Exception e) { return new(e, null); }
    }


    // TryMap: Opt<t>->Res<T>
    /// <summary>
    /// <inheritdoc cref="TryMap{T, TOut}(Opt{T}, Func{TOut})"/>
    /// </summary>
    public static async Task<Res<TOut>> TryMapAsync<T, TOut>(this Opt<T> maybe, Func<Task<TOut>> map)
    {
        if (maybe.IsNone) return new(errNone, null);
        try { return Ok(await map()); }
        catch (Exception e) { return new(e, null); }
    }
    /// <summary>
    /// <inheritdoc cref="TryMap{T, TOut}(Opt{T}, Func{Opt{TOut}})"/>
    /// </summary>
    public static async Task<Res<TOut>> TryMapAsync<T, TOut>(this Opt<T> maybe, Func<Task<Opt<TOut>>> map)
    {
        if (maybe.IsNone) return new(errNone, null);
        try
        {
            var res = await map();
            return res.IsNone ? new(errNone, null) : Ok(res.value);
        }
        catch (Exception e) { return new(e, null); }
    }
    /// <summary>
    /// <inheritdoc cref="TryMap{T, TOut}(Opt{T}, Func{Res{TOut}})"/>
    /// </summary>
    public static Task<Res<TOut>> TryMapAsync<T, TOut>(this Opt<T> maybe, Func<Task<Res<TOut>>> map)
    {
        if (maybe.IsNone) return Task.FromResult(new Res<TOut>(errNone, null));
        try { return map(); }
        catch (Exception e) { return Task.FromResult(new Res<TOut>(e, null)); }
    }
    /// <summary>
    /// <inheritdoc cref="TryMap{T, TOut}(Opt{T}, Func{T, TOut})"/>
    /// </summary>
    public static async Task<Res<TOut>> TryMapAsync<T, TOut>(this Opt<T> maybe, Func<T, Task<TOut>> map)
    {
        if (maybe.IsNone) return new(errNone, null);
        try { return Ok(await map(maybe.value)); }
        catch (Exception e) { return new(e, null); }
    }
    /// <summary>
    /// <inheritdoc cref="TryMap{T, TOut}(Opt{T}, Func{T, Opt{TOut}})"/>
    /// </summary>
    public static async Task<Res<TOut>> TryMapAsync<T, TOut>(this Opt<T> maybe, Func<T, Task<Opt<TOut>>> map)
    {
        if (maybe.IsNone) return new(errNone, null);
        try
        {
            var res = await map(maybe.value);
            return res.IsNone ? new(errNone, null) : Ok(res.value);
        }
        catch (Exception e) { return new(e, null); }
    }
    /// <summary>
    /// <inheritdoc cref="TryMap{T, TOut}(Opt{T}, Func{T, Res{TOut}})"/>
    /// </summary>
    public static Task<Res<TOut>> TryMapAsync<T, TOut>(this Opt<T> maybe, Func<T, Task<Res<TOut>>> map)
    {
        if (maybe.IsNone) return Task.FromResult(new Res<TOut>(errNone, null));
        try { return map(maybe.value); }
        catch (Exception e) { return Task.FromResult(new Res<TOut>(e, null)); }
    }

    #endregion
    

    // Validation
    /// <summary>
    /// Returns Ok(<paramref name="value"/>) if <paramref name="validator"/>(<paramref name="value"/>) returns true; Err(<paramref name="errorMessage"/>) otherwise.
    /// </summary>
    public static Res<T> Validate<T>(this T value, Func<T, bool> validator, string errorMessage)
        => validator(value) ? Ok(value) : Err<T>(errorMessage);
}
