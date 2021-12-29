using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Fun;

/// <summary>
/// Static extension or utility methods for Opt, Res, or Pipe.
/// </summary>
public static class Extensions
{
    // Constants
    const string errNone = "None->Res";

    // New
    /// <summary>
    /// Creates Some of <typeparamref name="T"/> with the given <paramref name="value"/>.
    /// Note that 'null' is not allowed and automatically mapped to None.
    /// </summary>
    public static Opt<T> Some<T>(T value) => new(value);
    /// <summary>
    /// Creates None of <typeparamref name="T"/>.
    /// </summary>
    public static Opt<T> None<T>() => new();
    /// <summary>
    /// Creates Ok result.
    /// </summary>
    public static Res Ok() => new();
    /// <summary>
    /// Creates an Err result with the given <paramref name="errorMessage"/>.
    /// </summary>
    public static Res Err(string errorMessage) => new(errorMessage, null);
    /// <summary>
    /// Creates an Err result occured during <paramref name="when"/> with the given <paramref name="errorMessage"/>.
    /// </summary>
    public static Res Err(string errorMessage, string when) => new(errorMessage, when);
    /// <summary>
    /// Creates an Err result with the given <paramref name="exception"/>.
    /// </summary>
    public static Res Err(Exception exception) => new(exception, null);
    /// <summary>
    /// Creates an Err result occured during <paramref name="when"/> with the given <paramref name="exception"/>.
    /// </summary>
    public static Res Err(Exception exception, string when) => new(exception, when);
    /// <summary>
    /// Creates Ok of <typeparamref name="T"/> with the given <paramref name="value"/>.
    /// Note that 'null' is not allowed and automatically mapped to Err.
    /// </summary>
    public static Res<T> Ok<T>(T value) => new(value);
    /// <summary>
    /// Creates an Err result with the given <paramref name="errorMessage"/>.
    /// </summary>
    public static Res<T> Err<T>(string errorMessage) => new(errorMessage, null);
    /// <summary>
    /// Creates an Err result occured during <paramref name="when"/> with the given <paramref name="errorMessage"/>.
    /// </summary>
    public static Res<T> Err<T>(string errorMessage, string when) => new(errorMessage, when);
    /// <summary>
    /// Creates an Err result with the given <paramref name="exception"/>.
    /// </summary>
    public static Res<T> Err<T>(Exception exception) => new(exception, null);
    /// <summary>
    /// Creates an Err result occured during <paramref name="when"/> with the given <paramref name="exception"/>.
    /// </summary>
    public static Res<T> Err<T>(Exception exception, string when) => new(exception, when);

    // New - ResFromStatus
    /// <summary>
    /// Creates a result: Ok if <paramref name="successCondition"/> is true; Err with the given <paramref name="failureMessage"/> if false.
    /// </summary>
    public static Res ResFromStatus(bool successCondition, string failureMessage) => successCondition ? new() : new(failureMessage, null);
    /// <summary>
    /// Creates a result: Ok if <paramref name="httpStatusCode"/> is 200-OK; Err with the given <paramref name="failureMessage"/> otherwise.
    /// </summary>
    public static Res ResFromStatus(HttpStatusCode httpStatusCode, string failureMessage) => httpStatusCode == HttpStatusCode.OK ? new() : new(failureMessage, null);
    /// <summary>
    /// Returns Ok(<paramref name="response"/>) if response.StatusCode is 200-OK; Err with the given <paramref name="failureMessage"/> otherwise.
    /// </summary>
    public static Res<HttpResponseMessage> ResFromStatus(this HttpResponseMessage response, string failureMessage) => response.StatusCode == HttpStatusCode.OK ? Ok(response) : new($"[StatusCode: {response.StatusCode}] {failureMessage}", null);
    /// <summary>
    /// Returns back <paramref name="result"/> if result.IsOk and StatusCode of the HttpResponseMessage is 200-OK; Err with the given <paramref name="failureMessage"/> otherwise.
    /// </summary>
    public static Res<HttpResponseMessage> ResFromStatus(this Res<HttpResponseMessage> result, string failureMessage) => result.IsErr ? new(result.ErrorMessage.value, null) : ResFromStatus(result.value, failureMessage);

    // New - Guard
    /// <summary>
    /// Nested options are not useful; hence, just returns back <paramref name="maybe"/>.
    /// </summary>
    public static Opt<T> Some<T>(Opt<T> maybe) => maybe;
    /// <summary>
    /// Nested options and results are not useful; maps <paramref name="result"/>'s value to Some when IsOk, to None when IsErr.
    /// </summary>
    public static Opt<T> Some<T>(Res<T> result) => result.IsOk ? new(result.value) : new();
    /// <summary>
    /// Creates None of <typeparamref name="T"/>, regardless of what <paramref name="maybe"/> is.
    /// </summary>
    public static Opt<T> None<T>(Opt<T> maybe) => new Opt<T>();
    /// <summary>
    /// Creates None of <typeparamref name="T"/>, regardless of what <paramref name="maybe"/> is.
    /// </summary>
    public static Opt<T> None<T>(Res<T> maybe) => new Opt<T>();
    /// <summary>
    /// Nested results are not useful; hence, just returns back <paramref name="result"/>.
    /// </summary>
    public static Res<T> Ok<T>(Res<T> result) => result;
    /// <summary>
    /// Nested options and results are not useful; maps <paramref name="maybe"/>'s value to Ok when IsSome, to Err when IsNone.
    /// </summary>
    public static Res<T> Ok<T>(Opt<T> maybe) => maybe.IsSome ? new(maybe.value) : new($"None->Res", $"Ok<{typeof(T).Name}>");
    static Res<T> Ok<T>(Opt<T> maybe, string when) => maybe.IsSome ? new(maybe.value) : new($"None->Res", when);

    // Conversion
    /// <summary>
    /// Converts Res to Opt: maps <paramref name="result"/> to Some of its value when IsOk; to None when IsErr.
    /// </summary>
    public static Opt<T> ToOpt<T>(this Res<T> result) => result.IsErr ? None<T>() : Some(result.value);
    /// <summary>
    /// Converts Opt to Res: maps <paramref name="maybe"/> to Ok of its value when IsSome; to Err when IsNone.
    /// </summary>
    public static Res<T> ToRes<T>(this Opt<T> maybe) => maybe.IsNone ? Err<T>("None->Res", $"ToRes<{typeof(T).Name}>") : Ok(maybe.value);
    /// <summary>
    /// Converts Res{T} to just Res without the value: Ok(val)->Ok(); Err(msg)-Err(msg).
    /// </summary>
    public static Res ToRes<T>(this Res<T> result) => result.IsErr ? new(result.ErrorMessage.value) : new();

    // Opt - None
    /// <summary>
    /// Does nothing and returns itself when <paramref name="maybe"/> IsSome; throws when IsNone.
    /// </summary>
    public static Opt<T> ThrowIfNone<T>(this Opt<T> maybe) { if (maybe.IsNone) throw new ArgumentException("[err] None"); return maybe; }
    /// <summary>
    /// Does nothing and returns itself when <paramref name="maybe"/> IsSome; throws with the given <paramref name="errorMessage"/> when IsNone.
    /// </summary>
    public static Opt<T> ThrowIfNone<T>(this Opt<T> maybe, string errorMessage) { if (maybe.IsNone) throw new ArgumentException($"[err] None: {errorMessage}"); return maybe; }
    /// <summary>
    /// Does nothing when <paramref name="maybe"/> IsSome; logs the given <paramref name="errorMessage"/> when IsNone.
    /// Returns itself.
    /// </summary>
    public static Opt<T> LogIfNone<T>(this Opt<T> maybe, string errorMessage) { if (maybe.IsNone) Console.WriteLine($"[warn] None: {errorMessage}"); return maybe; }
    /// <summary>
    /// Does nothing when <paramref name="maybe"/> IsSome; runs the given <paramref name="action"/> when IsNone.
    /// Returns itself.
    /// </summary>
    public static Opt<T> RunIfNone<T>(this Opt<T> maybe, Action action) { if (maybe.IsNone) action(); return maybe; }
    /// <summary>
    /// Maps <paramref name="maybe"/> into <paramref name="some"/>(maybe.Unwrap()) whenever maybe.IsSome; and into <paramref name="none"/>() otherwise.
    /// </summary>
    public static TOut Match<T, TOut>(this Opt<T> maybe, Func<T, TOut> some, Func<TOut> none)
    {
        if (maybe.IsSome) return some(maybe.value);
        else return none();
    }
    /// <summary>
    /// Maps <paramref name="maybe"/> into <paramref name="some"/>(maybe.Unwrap()) whenever maybe.IsSome; and into <paramref name="none"/> otherwise.
    /// </summary>
    public static TOut Match<T, TOut>(this Opt<T> maybe, Func<T, TOut> some, TOut none)
    {
        if (maybe.IsSome) return some(maybe.value);
        else return none;
    }

    // Res - Err
    /// <summary>
    /// Does nothing and returns itself when <paramref name="result"/> IsOk; throws when IsErr.
    /// </summary>
    public static Res ThrowIfErr(this Res result) { if (result.IsErr) throw new ArgumentException(result.ErrorMessage.value); return result; }
    /// <summary>
    /// Does nothing and returns itself when <paramref name="result"/> IsOk; throws with the given additional <paramref name="errorMessage"/> when IsErr.
    /// </summary>
    public static Res ThrowIfErr(this Res result, string errorMessage) { if (result.IsErr) { result.MsgIfErr(errorMessage); throw new ArgumentException(result.ErrorMessage.value); } return result; }
    /// <summary>
    /// Does nothing when <paramref name="result"/> IsOk; logs its <see cref="Res.ErrorMessage"/> when IsErr.
    /// Returns itself.
    /// </summary>
    public static Res LogIfErr(this Res result) { if (result.IsErr) { Console.WriteLine(result.ErrorMessage.value); } return result; }
    /// <summary>
    /// Does nothing when <paramref name="result"/> IsOk; logs its <see cref="Res.ErrorMessage"/> with th additional <paramref name="errorMessage"/> when IsErr.
    /// Returns itself.
    /// </summary>
    public static Res LogIfErr(this Res result, string errorMessage) { if (result.IsErr) { result.MsgIfErr(errorMessage); Console.WriteLine(result.ErrorMessage.value); } return result; }
    /// <summary>
    /// Logs the <paramref name="result"/> of the <paramref name="operationName"/>; whether Ok or Err.
    /// </summary>
    public static Res Log(this Res result, string operationName)
    {
        if (result.IsOk)
            Console.WriteLine($"[ok] {operationName}");
        else
            result.LogIfErr(operationName);
        return result;
    }
    /// <summary>
    /// Logs the <paramref name="result"/> of the <paramref name="operationName"/> with timestamp using given <paramref name="timeFormat"/>; whether Ok or Err.
    /// </summary>
    public static Res LogWithTime(this Res result, string operationName, string timeFormat = "HH-mm-ss")
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
    public static Res RunIfErr(this Res result, Action action) { if (result.IsErr) action(); return result; }
    /// <summary>
    /// <inheritdoc cref="ThrowIfErr(Res)"/>
    /// </summary>
    public static Res<T> ThrowIfErr<T>(this Res<T> result) { if (result.IsErr) throw new ArgumentException(result.ErrorMessage.value); return result; }
    /// <summary>
    /// <inheritdoc cref="ThrowIfErr(Res, string)"/>
    /// </summary>
    public static Res<T> ThrowIfErr<T>(this Res<T> result, string errorMessage) { if (result.IsErr) { result.MsgIfErr(errorMessage); throw new ArgumentException(result.ErrorMessage.value); } return result; }
    /// <summary>
    /// <inheritdoc cref="LogIfErr(Res)"/>
    /// </summary>
    public static Res<T> LogIfErr<T>(this Res<T> result) { if (result.ErrorMessage.IsSome) { Console.WriteLine(result.ErrorMessage.value); } return result; }
    /// <summary>
    /// <inheritdoc cref="LogIfErr(Res, string)"/>
    /// </summary>
    public static Res<T> LogIfErr<T>(this Res<T> result, string errorMessage) { if (result.IsErr) { result.MsgIfErr(errorMessage); Console.WriteLine(result.ErrorMessage.value); } return result; }
    /// <summary>
    /// Logs the <paramref name="result"/> of the <paramref name="operationName"/>; whether Ok or Err.
    /// </summary>
    public static Res<T> Log<T>(this Res<T> result, string operationName)
    {
        if (result.IsOk)
            Console.WriteLine($"[ok] {operationName}");
        else
            result.LogIfErr(operationName);
        return result;
    }
    /// <summary>
    /// Logs the <paramref name="result"/> of the <paramref name="operationName"/> with timestamp using given <paramref name="timeFormat"/>; whether Ok or Err.
    /// </summary>
    public static Res<T> LogWithTime<T>(this Res<T> result, string operationName, string timeFormat = "HH-mm-ss")
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
    public static Res<T> RunIfErr<T>(this Res<T> result, Action action) { if (result.IsErr) action(); return result; }
    /// <summary>
    /// Maps <paramref name="result"/> into <paramref name="ok"/>(maybe.Unwrap()) whenever maybe.IsSome; and into <paramref name="err"/>(maybe.ErrorMessage.Unwrap()) otherwise.
    /// </summary>
    public static TOut Match<T, TOut>(this Res<T> result, Func<T, TOut> ok, Func<string, TOut> err)
    {
        if (result.IsOk) return ok(result.value);
        else return err(result.ErrorMessage.Unwrap());
    }
    /// <summary>
    /// Maps <paramref name="result"/> into <paramref name="ok"/>(maybe.Unwrap()) whenever maybe.IsSome; and into <paramref name="err"/> otherwise.
    /// </summary>
    public static TOut Match<T, TOut>(this Res<T> result, Func<T, TOut> ok, TOut err)
    {
        if (result.IsOk) return ok(result.value);
        else return err;
    }


    // Run
    /// <summary>
    /// Runs <paramref name="action"/>, and returns back <paramref name="value"/>.
    /// </summary>
    public static T Run<T>(this T value, Action action) { action(); return value; }
    /// <summary>
    /// Runs <paramref name="action"/>(<paramref name="value"/>), and returns back <paramref name="value"/>.
    /// </summary>
    public static T Run<T>(this T value, Action<T> action) { action(value); return value; }
    /// <summary>
    /// Runs <paramref name="action"/>() only if maybe.IsSome, and returns back <paramref name="maybe"/>.
    /// </summary>
    public static Opt<T> Run<T>(this Opt<T> maybe, Action action) { if (maybe.IsSome) action(); return maybe; }
    /// <summary>
    /// Runs <paramref name="action"/>(<paramref name="maybe"/>.Unwrap()) only if maybe.IsSome, and returns back <paramref name="maybe"/>.
    /// </summary>
    public static Opt<T> Run<T>(this Opt<T> maybe, Action<T> action) { if (maybe.IsSome) action(maybe.value); return maybe; }
    /// <summary>
    /// Runs <paramref name="action"/>() only if result.IsOk, and returns back <paramref name="result"/>.
    /// </summary>
    public static Res Run(this Res result, Action action) { if (result.IsOk) action(); return result; }
    /// <summary>
    /// <inheritdoc cref="Run(Res, Action)"/>
    /// </summary>
    public static Res<T> Run<T>(this Res<T> result, Action action) { if (result.IsOk) action(); return result; }
    /// <summary>
    /// Runs <paramref name="action"/>(<paramref name="result"/>.Unwrap()) only if result.IsOk, and returns back <paramref name="result"/>.
    /// </summary>
    public static Res<T> Run<T>(this Res<T> result, Action<T> action) { if (result.IsOk) action(result.value); return result; }


    // Try: ()->Res
    /// <summary>
    /// Runs the <paramref name="action"/> within a try-catch block.
    /// Returns Ok if succeeds; Err with corresponding message if fails.
    /// </summary>
    public static Res Try(Action action)
    {
        try { action(); return Ok(); }
        catch (Exception e) { return new(e); }
    }
    /// <summary>
    /// Does nothing and returns back <paramref name="result"/> when already result.IsErr.
    /// When returns.IsOk; runs the <paramref name="action"/> within a try-catch block; returns Ok if succeeds; Err with corresponding message if fails.
    /// </summary>
    public static Res Try(this Res result, Action action)
    {
        if (result.IsErr) return result;
        try { action(); return Ok(); }
        catch (Exception e) { return new(e); }
    }
    /// <summary>
    /// <inheritdoc cref="Try(Res, Action)"/>
    /// </summary>
    public static Res Try<T>(this Res<T> result, Action action)
    {
        if (result.IsErr) new Res(result.ErrorMessage.value, null);
        try { action(); return Ok(); }
        catch (Exception e) { return new(e); }
    }
    /// <summary>
    /// Does nothing and returns and returns Err when already <paramref name="maybe"/>.IsNone.
    /// When maybe.IsSome; runs the <paramref name="action"/> within a try-catch block; returns Ok if succeeds; Err with corresponding message if fails.
    /// </summary>
    public static Res Try<T>(this Opt<T> maybe, Action action)
    {
        if (maybe.IsNone) new Res(errNone, null);
        try { action(); return Ok(); }
        catch (Exception e) { return new(e); }
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
        catch (Exception e) { return new(e); }
    }
    /// <summary>
    /// Does nothing and returns and returns Err when already <paramref name="maybe"/>.IsNone.
    /// When maybe.IsSome; runs the <paramref name="action"/>(maybe.Unwrap()) within a try-catch block; returns Ok if succeeds; Err with corresponding message if fails.
    /// </summary>
    public static Res Try<T>(this Opt<T> maybe, Action<T> action)
    {
        if (maybe.IsNone) return new(errNone);
        try { action(maybe.value); return Ok(); }
        catch (Exception e) { return new(e); }
    }

    // Map: ()->T
    /// <summary>
    /// Returns <paramref name="map"/>().
    /// </summary>
    public static TOut Map<TOut>(Func<TOut> map) => map();

    // Map: t->T
    /// <summary>
    /// Returns <paramref name="map"/>().
    /// </summary>
    public static TOut Map<T, TOut>(this T value, Func<TOut> map) => map();
    /// <summary>
    /// Returns <paramref name="map"/>(<paramref name="value"/>).
    /// </summary>
    public static TOut Map<T, TOut>(this T value, Func<T, TOut> map) => map(value);

    // Map: Res->Res
    /// <summary>
    /// Returns back <paramref name="result"/> when IsErr; returns <paramref name="map"/>() when IsOk.
    /// </summary>
    public static Res Map(this Res result, Func<Res> map) => result.IsErr ? result : map();
    // Map: Res->Res<T>
    /// <summary>
    /// Returns back <paramref name="result"/> of <typeref name="TOut"/> when IsErr; returns Ok(<paramref name="map"/>()) when IsOk.
    /// </summary>
    public static Res<TOut> Map<TOut>(this Res result, Func<TOut> map) => result.IsErr ? new(result.ErrorMessage.value, null) : Ok(map());
    /// <summary>
    /// Returns back <paramref name="result"/> of <typeparam name="TOut"/> when IsErr; returns <paramref name="map"/>() when IsOk.
    /// </summary>
    public static Res<TOut> Map<TOut>(this Res result, Func<Res<TOut>> map) => result.IsErr ? new(result.ErrorMessage.value, null) : map();
    /// <summary>
    /// Returns back <paramref name="result"/> of <typeparam name="TOut"/> when IsErr; returns <paramref name="map"/>().ToRes() when IsOk.
    /// </summary>
    public static Res<TOut> Map<TOut>(this Res result, Func<Opt<TOut>> map) => result.IsErr ? new(result.ErrorMessage.value, null) : Ok(map());

    // Map: Res<t>->Res
    /// <summary>
    /// <inheritdoc cref="Map(Res, Func{Res})"/>
    /// </summary>
    public static Res Map<T>(this Res<T> result, Func<Res> map) => result.IsErr ? new(result.ErrorMessage.value, null) : map();
    /// <summary>
    /// Returns back <paramref name="result"/> when IsErr; returns <paramref name="map"/>(result.Unwrap()) when IsOk.
    /// </summary>
    public static Res Map<T>(this Res<T> result, Func<T, Res> map) => result.IsErr ? new(result.ErrorMessage.value, null) : map(result.value);
    // Map: Res<t>->Res<T>
    /// <summary>
    /// Returns back <paramref name="result"/> of <typeparamref name="TOut"/> when IsErr; returns Ok(<paramref name="map"/>()) when IsOk.
    /// </summary>
    public static Res<TOut> Map<T, TOut>(this Res<T> result, Func<TOut> map) => result.IsErr ? new(result.ErrorMessage.value, null) : Ok(map());
    /// <summary>
    /// Returns back <paramref name="result"/> of <typeparamref name="TOut"/> when IsErr; returns Ok(<paramref name="map"/>(result.Unwrap())) when IsOk.
    /// </summary>
    public static Res<TOut> Map<T, TOut>(this Res<T> result, Func<T, TOut> map) => result.IsErr ? new(result.ErrorMessage.value, null) : Ok(map(result.value));
    /// <summary>
    /// Returns back <paramref name="result"/> of <typeparamref name="TOut"/> when IsErr; returns <paramref name="map"/>() when IsOk.
    /// </summary>
    public static Res<TOut> Map<T, TOut>(this Res<T> result, Func<Res<TOut>> map) => result.IsErr ? new(result.ErrorMessage.value, null) : map();
    /// <summary>
    /// Returns back <paramref name="result"/> of <typeparamref name="TOut"/> when IsErr; returns <paramref name="map"/>(result.Unwrap()) when IsOk.
    /// </summary>
    public static Res<TOut> Map<T, TOut>(this Res<T> result, Func<T, Res<TOut>> map) => result.IsErr ? new(result.ErrorMessage.value, null) : map(result.value);
    /// <summary>
    /// Returns back <paramref name="result"/> of <typeparamref name="TOut"/> when IsErr; returns <paramref name="map"/>().ToRes() when IsOk.
    /// Note that Opt->Res mapping is as follows: Some(value)->Ok(value); None->Err.
    /// </summary>
    public static Res<TOut> Map<T, TOut>(this Res<T> result, Func<Opt<TOut>> map) => result.IsErr ? new(result.ErrorMessage.value, null) : Ok(map());
    /// <summary>
    /// Returns back <paramref name="result"/> of <typeparamref name="TOut"/> when IsErr; returns <paramref name="map"/>(result.Unwrap()).ToRes() when IsOk.
    /// Note that Opt->Res mapping is as follows: Some(value)->Ok(value); None->Err.
    /// </summary>
    public static Res<TOut> Map<T, TOut>(this Res<T> result, Func<T, Opt<TOut>> map) => result.IsErr ? new(result.ErrorMessage.value, null) : Ok(map(result.value));


    // Map: Opt<t>->Opt<T>
    /// <summary>
    /// Returns None when <paramref name="maybe"/> IsNone; Some(<paramref name="map"/>()) when IsSome.
    /// </summary>
    public static Opt<TOut> Map<T, TOut>(this Opt<T> maybe, Func<TOut> map) => maybe.IsNone ? None<TOut>() : Some(map());
    /// <summary>
    /// Returns None when <paramref name="maybe"/> IsNone; Some(<paramref name="map"/>(maybe.Unwrap())) when IsSome.
    /// </summary>
    public static Opt<TOut> Map<T, TOut>(this Opt<T> maybe, Func<T, TOut> map) => maybe.IsNone ? None<TOut>() : Some(map(maybe.value));
    /// <summary>
    /// <inheritdoc cref="Map{T, TOut}(Opt{T}, Func{TOut})"/>
    /// </summary>
    public static Opt<TOut> Map<T, TOut>(this Opt<T> maybe, Func<Opt<TOut>> map) => maybe.IsNone ? None<TOut>() : map();
    /// <summary>
    /// <inheritdoc cref="Map{T, TOut}(Opt{T}, Func{T, TOut})"/>
    /// </summary>
    public static Opt<TOut> Map<T, TOut>(this Opt<T> maybe, Func<T, Opt<TOut>> map) => maybe.IsNone ? None<TOut>() : map(maybe.value);
    // Map: Opt<t>->Res<T>
    /// <summary>
    /// Returns Err when <paramref name="maybe"/> IsNone; <paramref name="map"/>() when IsSome.
    /// </summary>
    public static Res<TOut> Map<T, TOut>(this Opt<T> maybe, Func<Res<TOut>> map) => maybe.IsNone ? new(errNone, null) : map();
    /// <summary>
    /// Returns Err when <paramref name="maybe"/> IsNone; <paramref name="map"/>(maybe.Unwrap()) when IsSome.
    /// </summary>
    public static Res<TOut> Map<T, TOut>(this Opt<T> maybe, Func<T, Res<TOut>> map) => maybe.IsNone ? new(errNone, null) : map(maybe.value);


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
        try { return map().ToRes(); }
        catch (Exception e) { return new Res<TOut>(e, null); }
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
    {
        try { return map().ToRes(); }
        catch (Exception e) { return new Res<TOut>(e, null); }
    }
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
        try { return map(value).ToRes(); }
        catch (Exception e) { return new Res<TOut>(e, null); }
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
        catch (Exception e) { return new(e); }
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
        try { return ToRes(map()); }
        catch (Exception e) { return new Res<TOut>(e, null); }
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
    /// Returns Err of <typeref name="TOut"/> when IsErr.
    /// When IsOk, tries to return ToRes(<paramref name="map"/>()); returns Err if the method throws.
    /// Note that ToRes maps Some to Ok, and None to Err.
    /// </summary>
    public static Res<TOut> TryMap<T, TOut>(this Res<T> result, Func<Opt<TOut>> map)
    {
        if (result.IsErr) return new(result.ErrorMessage.value, null);
        try { return Ok(map()); }
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
    /// <summary>
    /// Returns Err of <typeref name="TOut"/> when IsErr.
    /// When IsOk, tries to return ToRes(<paramref name="map"/>(result.Unwrap())); returns Err if the method throws.
    /// Note that ToRes maps Some to Ok, and None to Err.
    /// </summary>
    public static Res<TOut> TryMap<T, TOut>(this Res<T> result, Func<T, Opt<TOut>> map)
    {
        if (result.IsErr) return new(result.ErrorMessage.value, null);
        try { return Ok(map(result.value)); }
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
        try { return ToRes(map()); }
        catch (Exception e) { return new Res<TOut>(e, null); }
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
        try { return ToRes(map(maybe.value)); }
        catch (Exception e) { return new Res<TOut>(e, null); }
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
    /// <inheritdoc cref="Run{T}(T, Action)"/>
    /// </summary>
    public static Task<T> RunAsync<T>(this T value, Func<Task> action) { action(); return Task.FromResult(value); }
    /// <summary>
    /// <inheritdoc cref="Run{T}(T, Action{T})"/>
    /// </summary>
    public static Task<T> RunAsync<T>(this T value, Func<T, Task> action) { action(value); return Task.FromResult(value); }
    /// <summary>
    /// <inheritdoc cref="Run{T}(Opt{T}, Action)"/>
    /// </summary>
    public static Task<Opt<T>> RunAsync<T>(this Opt<T> maybe, Func<Task> action) { if (maybe.IsSome) action(); return Task.FromResult(maybe); }
    /// <summary>
    /// <inheritdoc cref="Run{T}(Opt{T}, Action{T})"/>
    /// </summary>
    public static Task<Opt<T>> RunAsync<T>(this Opt<T> maybe, Func<T, Task> action) { if (maybe.IsSome) action(maybe.value); return Task.FromResult(maybe); }
    /// <summary>
    /// <inheritdoc cref="Run{T}(Opt{T}, Action)"/>
    /// </summary>
    public static Task<Res<T>> RunAsync<T>(this Res<T> result, Func<Task> action) { if (result.IsOk) action(); return Task.FromResult(result); }
    /// <summary>
    /// <inheritdoc cref="Run{T}(Res{T}, Action{T})"/>
    /// </summary>
    public static Task<Res<T>> RunAsync<T>(this Res<T> result, Func<T, Task> action) { if (result.IsOk) action(result.value); return Task.FromResult(result); }

    // Try: ()->Res
    /// <summary>
    /// <inheritdoc cref="Try(Action)"/>
    /// </summary>
    public static async Task<Res> TryAsync(Func<Task> action)
    {
        try { await action(); return new(); }
        catch (Exception e) { return new(e); }
    }
    /// <summary>
    /// <inheritdoc cref="Try(Res, Action)"/>
    /// </summary>
    public static async Task<Res> TryAsync(this Res result, Func<Task> action)
    {
        if (result.IsErr) return result;
        try { await action(); return new(); }
        catch (Exception e) { return new(e); }
    }
    /// <summary>
    /// <inheritdoc cref="Try{T}(Res{T}, Action)"/>
    /// </summary>
    public static async Task<Res> TryAsync<T>(this Res<T> result, Func<Task> action)
    {
        if (result.IsErr) return new(result.ErrorMessage.value);
        try { await action(); return new(); }
        catch (Exception e) { return new(e); }
    }
    /// <summary>
    /// <inheritdoc cref="Try{T}(Opt{T}, Action)"/>
    /// </summary>
    public static async Task<Res> TryAsync<T>(this Opt<T> maybe, Func<Task> action)
    {
        if (maybe.IsNone) return new(errNone);
        try { await action(); return new(); }
        catch (Exception e) { return new(e); }
    }

    // Try: t->Res
    /// <summary>
    /// <inheritdoc cref="Try{T}(Opt{T}, Action{T})"/>
    /// </summary>
    public static async Task<Res> TryAsync<T>(this Res<T> result, Func<T, Task> action)
    {
        if (result.IsErr) return new(result.ErrorMessage.value);
        try { await action(result.value); return new(); }
        catch (Exception e) { return new(e); }
    }
    /// <summary>
    /// <inheritdoc cref="Try{T}(Opt{T}, Action{T})"/>
    /// </summary>
    public static async Task<Res> TryAsync<T>(this Opt<T> maybe, Func<T, Task> action)
    {
        if (maybe.IsNone) return new(errNone);
        try { await action(maybe.value); return new(); }
        catch (Exception e) { return new(e); }
    }

    // Map: ()->T
    /// <summary>
    /// <inheritdoc cref="Map{TOut}(Func{TOut})"/>
    /// </summary>
    public static Task<TOut> MapAsync<TOut>(Func<Task<TOut>> map) => map();

    // Map: t->T
    /// <summary>
    /// <inheritdoc cref="Map{T, TOut}(T, Func{T, TOut})"/>
    /// </summary>
    public static Task<TOut> MapAsync<T, TOut>(this T value, Func<T, Task<TOut>> map) => map(value);

    // Map: Res->Res
    /// <summary>
    /// <inheritdoc cref="Map(Res, Func{Res})"/>
    /// </summary>
    public static Task<Res> MapAsync(this Res result, Func<Task<Res>> map) => result.IsErr ? Task.FromResult(new Res(result.ErrorMessage.value, null)) : map();
    // Map: Res->Res<T>
    /// <summary>
    /// <inheritdoc cref="Map{TOut}(Res, Func{TOut})"/>
    /// </summary>
    public static async Task<Res<TOut>> MapAsync<TOut>(this Res result, Func<Task<TOut>> map) => result.IsErr ? new(result.ErrorMessage.value, null) : Ok(await map());
    /// <summary>
    /// <inheritdoc cref="Map{TOut}(Res, Func{Res{TOut}})"/>
    /// </summary>
    public static Task<Res<TOut>> MapAsync<TOut>(this Res result, Func<Task<Res<TOut>>> map) => result.IsErr ? Task.FromResult<Res<TOut>>(new(result.ErrorMessage.value, null)) : map();
    /// <summary>
    /// <inheritdoc cref="Map{TOut}(Res, Func{Opt{TOut}})"/>
    /// </summary>
    public static async Task<Res<TOut>> MapAsync<TOut>(this Res result, Func<Task<Opt<TOut>>> map) => result.IsErr ? new(result.ErrorMessage.value, null) : Ok(await map());

    // Map: Res<t>->Res
    /// <summary>
    /// <inheritdoc cref="Map(Res, Func{Res})"/>
    /// </summary>
    public static Task<Res> MapAsync<T>(this Res<T> result, Func<Task<Res>> map) => result.IsErr ? Task.FromResult(new Res(result.ErrorMessage.value, null)) : map();
    /// <summary>
    /// <inheritdoc cref="Map{T}(Res{T}, Func{T, Res})"/>
    /// </summary>
    public static Task<Res> MapAsync<T>(this Res<T> result, Func<T, Task<Res>> map) => result.IsErr ? Task.FromResult(new Res(result.ErrorMessage.value, null)) : map(result.value);
    // Map: Res<t>->Res<T>
    /// <summary>
    /// <inheritdoc cref="Map{T, TOut}(Res{T}, Func{TOut})"/>
    /// </summary>
    public static async Task<Res<TOut>> MapAsync<T, TOut>(this Res<T> result, Func<Task<TOut>> map) => result.IsErr ? new(result.ErrorMessage.value, null) : Ok(await map());
    /// <summary>
    /// <inheritdoc cref="Map{T, TOut}(Res{T}, Func{T, TOut})"/>
    /// </summary>
    public static async Task<Res<TOut>> MapAsync<T, TOut>(this Res<T> result, Func<T, Task<TOut>> map) => result.IsErr ? new(result.ErrorMessage.value, null) : Ok(await map(result.value));
    /// <summary>
    /// <inheritdoc cref="Map{T, TOut}(Res{T}, Func{Res{TOut}})"/>
    /// </summary>
    public static Task<Res<TOut>> MapAsync<T, TOut>(this Res<T> result, Func<Task<Res<TOut>>> map) => result.IsErr ? Task.FromResult(new Res<TOut>(result.ErrorMessage.value, null)) : map();
    /// <summary>
    /// <inheritdoc cref="Map{T, TOut}(Res{T}, Func{T, Res{TOut}})"/>
    /// </summary>
    public static Task<Res<TOut>> MapAsync<T, TOut>(this Res<T> result, Func<T, Task<Res<TOut>>> map) => result.IsErr ? Task.FromResult(new Res<TOut>(result.ErrorMessage.value, null)) : map(result.value);
    /// <summary>
    /// <inheritdoc cref="Map{T, TOut}(Res{T}, Func{Opt{TOut}})"/>
    /// </summary>
    public static async Task<Res<TOut>> MapAsync<T, TOut>(this Res<T> result, Func<Task<Opt<TOut>>> map) => result.IsErr ? new Res<TOut>(result.ErrorMessage.value, null) : Ok(await map());
    /// <summary>
    /// <inheritdoc cref="Map{T, TOut}(Res{T}, Func{T, Opt{TOut}})"/>
    /// </summary>
    public static async Task<Res<TOut>> MapAsync<T, TOut>(this Res<T> result, Func<T, Task<Opt<TOut>>> map) => result.IsErr ? new Res<TOut>(result.ErrorMessage.value, null) : Ok(await map(result.value));

    // Map: Opt<t>->Opt<T>
    /// <summary>
    /// <inheritdoc cref="Map{T, TOut}(Opt{T}, Func{TOut})"/>
    /// </summary>
    public static async Task<Opt<TOut>> MapAsync<T, TOut>(this Opt<T> maybe, Func<Task<TOut>> map) => maybe.IsNone ? None<TOut>() : Some(await map());
    /// <summary>
    /// <inheritdoc cref="Map{T, TOut}(Opt{T}, Func{T, TOut})"/>
    /// </summary>
    public static async Task<Opt<TOut>> MapAsync<T, TOut>(this Opt<T> maybe, Func<T, Task<TOut>> map) => maybe.IsNone ? None<TOut>() : Some(await map(maybe.value));
    /// <summary>
    /// <inheritdoc cref="Map{T, TOut}(Opt{T}, Func{TOut})"/>
    /// </summary>
    public static Task<Opt<TOut>> MapAsync<T, TOut>(this Opt<T> maybe, Func<Task<Opt<TOut>>> map) => maybe.IsNone ? Task.FromResult(None<TOut>()) : map();
    /// <summary>
    /// <inheritdoc cref="Map{T, TOut}(Opt{T}, Func{T, TOut})"/>
    /// </summary>
    public static Task<Opt<TOut>> MapAsync<T, TOut>(this Opt<T> maybe, Func<T, Task<Opt<TOut>>> map) => maybe.IsNone ? Task.FromResult(None<TOut>()) : map(maybe.value);
    // Map: Opt<t>->Res<T>
    /// <summary>
    /// <inheritdoc cref="Map{T, TOut}(Opt{T}, Func{Res{TOut}})"/>
    /// </summary>
    public static Task<Res<TOut>> MapAsync<T, TOut>(this Opt<T> maybe, Func<Task<Res<TOut>>> map) => maybe.IsNone ? Task.FromResult(new Res<TOut>("None->Res", "Map")) : map();
    /// <summary>
    /// <inheritdoc cref="Map{T, TOut}(Opt{T}, Func{T, Res{TOut}})"/>
    /// </summary>
    public static Task<Res<TOut>> MapAsync<T, TOut>(this Opt<T> maybe, Func<T, Task<Res<TOut>>> map) => maybe.IsNone ? Task.FromResult(new Res<TOut>("None->Res", "Map")) : map(maybe.value);

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
        try { return (await map()).ToRes(); }
        catch (Exception e) { return new(e, null); }
    }

    // TryMap: t->Res<T>
    /// <summary>
    /// <inheritdoc cref="TryMap{TOut}(Func{TOut})"/>
    /// </summary>
    public static async Task<Res<TOut>> TryMapAsync<T, TOut>(Func<Task<TOut>> map)
    {
        try { return new(await map()); }
        catch (Exception e) { return new(e, null); }
    }
    /// <summary>
    /// <inheritdoc cref="TryMap{TOut}(Func{Res{TOut}})"/>
    /// </summary>
    public static async Task<Res<TOut>> TryMapAsync<T, TOut>(Func<Task<Res<TOut>>> map)
    {
        try { return await map(); }
        catch (Exception e) { return new(e, null); }
    }
    /// <summary>
    /// <inheritdoc cref="TryMap{TOut}(Func{Opt{TOut}})"/>
    /// </summary>
    public static async Task<Res<TOut>> TryMapAsync<T, TOut>(Func<Task<Opt<TOut>>> map)
    {
        try { return (await map()).ToRes(); }
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
        try { return (await map(value)).ToRes(); }
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
        try { return ToRes(await map()); }
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
        try { return ToRes(await map()); }
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
        try { return ToRes(await map(result.value)); }
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
        try { return ToRes(await map()); }
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
        try { return ToRes(await map(maybe.value)); }
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


    // TryParse
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    public static Opt<int> ParseIntOrNone(this string text) { bool s = int.TryParse(text, out var val); return s ? Some(val) : None<int>(); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    public static Opt<int> ParseIntOrNone(this ReadOnlySpan<char> text) { bool s = int.TryParse(text, out var val); return s ? Some(val) : None<int>(); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    public static Opt<double> ParseDoubleOrNone(this string text) { bool s = double.TryParse(text, out var val); return s ? Some(val) : None<double>(); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    public static Opt<double> ParseDoubleOrNone(this ReadOnlySpan<char> text) { bool s = double.TryParse(text, out var val); return s ? Some(val) : None<double>(); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    public static Opt<float> ParseFloatOrNone(this string text) { bool s = float.TryParse(text, out var val); return s ? Some(val) : None<float>(); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    public static Opt<float> ParseFloatOrNone(this ReadOnlySpan<char> text) { bool s = float.TryParse(text, out var val); return s ? Some(val) : None<float>(); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    public static Opt<short> ParseShortOrNone(this string text) { bool s = short.TryParse(text, out var val); return s ? Some(val) : None<short>(); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    public static Opt<short> ParseShortOrNone(this ReadOnlySpan<char> text) { bool s = short.TryParse(text, out var val); return s ? Some(val) : None<short>(); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    public static Opt<long> ParseLongOrNone(this string text) { bool s = long.TryParse(text, out var val); return s ? Some(val) : None<long>(); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    public static Opt<long> ParseLongOrNone(this ReadOnlySpan<char> text) { bool s = long.TryParse(text, out var val); return s ? Some(val) : None<long>(); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    public static Opt<Half> ParseHalfOrNone(this string text) { bool s = Half.TryParse(text, out var val); return s ? Some(val) : None<Half>(); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    public static Opt<Half> ParseHalfOrNone(this ReadOnlySpan<char> text) { bool s = Half.TryParse(text, out var val); return s ? Some(val) : None<Half>(); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    public static Opt<bool> ParseBoolOrNone(this string text) { bool s = bool.TryParse(text, out var val); return s ? Some(val) : None<bool>(); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    public static Opt<bool> ParseBoolOrNone(this ReadOnlySpan<char> text) { bool s = bool.TryParse(text, out var val); return s ? Some(val) : None<bool>(); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    public static Opt<DateTime> ParseDateTimeOrNone(this string text) { bool s = DateTime.TryParse(text, out var val); return s ? Some(val) : None<DateTime>(); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    public static Opt<DateTime> ParseDateTimeOrNone(this ReadOnlySpan<char> text) { bool s = DateTime.TryParse(text, out var val); return s ? Some(val) : None<DateTime>(); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    public static Opt<DateOnly> ParseDateOnlyOrNone(this string text) { bool s = DateOnly.TryParse(text, out var val); return s ? Some(val) : None<DateOnly>(); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    public static Opt<DateOnly> ParseDateOnlyOrNone(this ReadOnlySpan<char> text) { bool s = DateOnly.TryParse(text, out var val); return s ? Some(val) : None<DateOnly>(); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    public static Opt<TimeOnly> ParseTimeOnlyOrNone(this string text) { bool s = TimeOnly.TryParse(text, out var val); return s ? Some(val) : None<TimeOnly>(); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    public static Opt<TimeOnly> ParseTimeOnlyOrNone(this ReadOnlySpan<char> text) { bool s = TimeOnly.TryParse(text, out var val); return s ? Some(val) : None<TimeOnly>(); }

    // TryGetValue
    /// <summary>
    /// Returns Some of value from <paramref name="dictionary"/> with the given <paramref name="key"/> if exists; None if the key is absent.
    /// </summary>
    public static Opt<TValue> GetValueOrNone<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key) { bool s = dictionary.TryGetValue(key, out var val); return s ? Some(val) : None<TValue>(); }
    /// <summary>
    /// Returns Some of value from <paramref name="dictionary"/> with the given <paramref name="key"/> if exists; None if the key is absent.
    /// </summary>
    public static Opt<TValue> GetValueOrNone<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey key) { bool s = dictionary.TryGetValue(key, out var val); return s ? Some(val) : None<TValue>(); }

    // Collections
    /// <summary>
    /// Creates and returns and IEnumerable yielding unwrapped values of elements that are <see cref="Opt{T}.IsSome"/>.
    /// </summary>
    /// <typeparam name="T">Underlying type of the Opt.</typeparam>
    /// <param name="collection">Collection of opt values.</param>
    public static IEnumerable<T> UnwrapValues<T>(this IEnumerable<Opt<T>> collection) => collection.Where(x => x.IsSome).Select(x => x.Unwrap());
    /// <summary>
    /// Opt counterpart of FirstOrDefault, which returns the first non-null element if <paramref name="enumerable"/> has any, None otherwise.
    /// </summary>
    public static Opt<T> FirstOrNone<T>(this IEnumerable<T> enumerable)
    {
        if (typeof(T).IsClass)
        {
            foreach (var item in enumerable)
                if (item != null)
                    return new(item);
            return new();
        }
        else
        {
            foreach (var item in enumerable)
                return new(item);
            return new();
        }
    }
    /// <summary>
    /// Opt counterpart of FirstOrDefault, which returns the first Some element if <paramref name="enumerable"/> has any, None otherwise.
    /// </summary>
    public static Opt<T> FirstOrNone<T>(this IEnumerable<Opt<T>> enumerable)
    {
        foreach (var item in enumerable)
            if (item.IsSome)
                return item;
        return new();
    }
    /// <summary>
    /// Opt counterpart of LastOrDefault, which returns the last non-null element if <paramref name="enumerable"/> has any, None otherwise.
    /// </summary>
    public static Opt<T> LastOrNone<T>(this IEnumerable<T> enumerable) => FirstOrNone(enumerable.Reverse());
    /// <summary>
    /// Opt counterpart of LastOrDefault, which returns the last Some element if <paramref name="enumerable"/> has any, None otherwise.
    /// </summary>
    public static Opt<T> LastOrNone<T>(this IEnumerable<Opt<T>> enumerable) => FirstOrNone(enumerable.Reverse());
    /// <summary>
    /// Creates and returns and IEnumerable yielding unwrapped values of elements that are <see cref="Res{T}.IsOk"/>.
    /// </summary>
    /// <typeparam name="T">Underlying type of the Res.</typeparam>
    /// <param name="collection">Collection of result values.</param>
    public static IEnumerable<T> UnwrapValues<T>(this IEnumerable<Res<T>> collection) => collection.Where(x => x.IsOk).Select(x => x.Unwrap());
    /// <summary>
    /// Opt counterpart of FirstOrDefault over results collection, which returns the first Ok element of <paramref name="enumerable"/> if any, None otherwise.
    /// </summary>
    public static Opt<T> FirstOrNone<T>(this IEnumerable<Res<T>> enumerable)
    {
        foreach (var item in enumerable)
            if (item.IsOk)
                return new(item.value);
        return new();
    }
    /// <summary>
    /// Opt counterpart of LastOrDefault over results collection, which returns the last Ok element of <paramref name="enumerable"/> if any, None otherwise.
    /// </summary>
    public static Opt<T> LastOrNone<T>(this IEnumerable<Res<T>> enumerable) => FirstOrNone(enumerable.Reverse());
    // Collections - Elevate
    /// <summary>
    /// Converts <paramref name="list"/> of <typeparamref name="T"/> into a list of Opt&lt;<typeparamref name="T"/>>.
    /// If <typeparamref name="T"/> is a reference type; null's will be mapped into None.
    /// </summary>
    public static List<Opt<T>> ToOptList<T>(this List<T> list)
    {
        if (typeof(T).IsClass)
        {
            var opt = new List<Opt<T>>(list.Count);
            for (var i = 0; i < list.Count; i++)
                opt.Add(Some(list[i]));
            return opt;
        }
        else
            return list.Select(x => Some(x)).ToList();
    }
    /// <summary>
    /// Converts <paramref name="array"/> of <typeparamref name="T"/> into a list of Opt&lt;<typeparamref name="T"/>>.
    /// If <typeparamref name="T"/> is a reference type; null's will be mapped into None.
    /// </summary>
    public static List<Opt<T>> ToOptList<T>(this T[] array)
    {
        if (typeof(T).IsClass)
        {
            var opt = new List<Opt<T>>(array.Length);
            for (var i = 0; i < array.Length; i++)
                opt.Add(Some(array[i]));
            return opt;
        }
        else
            return array.Select(x => Some(x)).ToList();
    }
    /// <summary>
    /// Converts <paramref name="enumerable"/> of <typeparamref name="T"/> into a list of Opt&lt;<typeparamref name="T"/>>.
    /// If <typeparamref name="T"/> is a reference type; null's will be mapped into None.
    /// </summary>
    public static List<Opt<T>> ToOptList<T>(this IEnumerable<T> enumerable) => enumerable.Select(x => Some(x)).ToList();
    /// <summary>
    /// Converts <paramref name="list"/> of <typeparamref name="T"/> into an array of Opt&lt;<typeparamref name="T"/>>.
    /// If <typeparamref name="T"/> is a reference type; null's will be mapped into None.
    /// </summary>
    public static Opt<T>[] ToOptArray<T>(this List<T> list)
    {
        if (typeof(T).IsClass)
        {
            var opt = new Opt<T>[list.Count];
            for (var i = 0; i < list.Count; i++)
                opt[i] = Some(list[i]);
            return opt;
        }
        else
            return list.Select(x => Some(x)).ToArray();
    }
    /// <summary>
    /// Converts <paramref name="array"/> of <typeparamref name="T"/> into an array of Opt&lt;<typeparamref name="T"/>>.
    /// If <typeparamref name="T"/> is a reference type; null's will be mapped into None.
    /// </summary>
    public static Opt<T>[] ToOptArray<T>(this T[] array)
    {
        if (typeof(T).IsClass)
        {
            var opt = new Opt<T>[array.Length];
            for (var i = 0; i < array.Length; i++)
                opt[i] = Some(array[i]);
            return opt;
        }
        else
            return array.Select(x => Some(x)).ToArray();
    }
    /// <summary>
    /// Converts <paramref name="enumerable"/> of <typeparamref name="T"/> into an array of Opt&lt;<typeparamref name="T"/>>.
    /// If <typeparamref name="T"/> is a reference type; null's will be mapped into None.
    /// </summary>
    public static Opt<T>[] ToOptArray<T>(this IEnumerable<T> enumerable) => enumerable.Select(x => Some(x)).ToArray();
    /// <summary>
    /// Converts <paramref name="enumerable"/> of <typeparamref name="T"/> into an IEnumerable of Opt&lt;<typeparamref name="T"/>>.
    /// If <typeparamref name="T"/> is a reference type; null's will be mapped into None.
    /// </summary>
    public static IEnumerable<Opt<T>> ToOptEnumerable<T>(this IEnumerable<T> enumerable)
    {
        foreach (var item in enumerable)
            yield return Some(item);
    }
    /// <summary>
    /// Converts <paramref name="dictionary"/> of <typeparamref name="TKey"/>-<typeparamref name="TValue"/> pair into a dictionary with of Opt&lt;<typeparamref name="TValue"/>> as the value type.
    /// If <typeparamref name="TValue"/> is a reference type; null's will be mapped into None.
    /// </summary>
    public static Dictionary<TKey, Opt<TValue>> ToOptDictionary<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
    {
        var opt = new Dictionary<TKey, Opt<TValue>>(dictionary.Count);
        foreach (var item in dictionary)
            opt.Add(item.Key, Some(item.Value));
        return opt;
    }

    // Validation
    /// <summary>
    /// Returns Some(<paramref name="value"/>) if <paramref name="validator"/>(<paramref name="value"/>) returns true; None otherwise.
    /// </summary>
    public static Opt<T> Validate<T>(this T value, Func<T, bool> validator) => validator(value) ? Some(value) : None<T>();
    /// <summary>
    /// Returns Ok(<paramref name="value"/>) if <paramref name="validator"/>(<paramref name="value"/>) returns true; Err(<paramref name="errorMessage"/>) otherwise.
    /// </summary>
    public static Res<T> Validate<T>(this T value, Func<T, bool> validator, string errorMessage) => validator(value) ? Ok(value) : Err<T>(errorMessage);
}
