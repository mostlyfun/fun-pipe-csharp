namespace Fun;
using System;
using System.Threading.Tasks;

/// <summary>
/// Pipe operation holding a <see cref="Res"/>.
/// (Try)-Run/Map methods do nothing when <see cref="Res.IsErr"/> other than creating a new pipe state with the error; i.e. the operations are bypassed and the error is carried on.
/// When <see cref="Res.IsOk"/>, a new pipe state is created with result of the operation.
/// </summary>
public struct Pipe
{
    // Data
    /// <summary>
    /// Result, state, of the pipe.
    /// </summary>
    public readonly Res Res;
    readonly OnErr onErr;

    // Ctor
    internal Pipe(Res res, OnErr onErr, bool errorAlreadyHandled = false)
    {
        Res = res;
        this.onErr = onErr;
        if (errorAlreadyHandled)
            return;
        switch (onErr)
        {
            case OnErr.Log:
                res.LogOnErr(false);
                break;
            case OnErr.Throw:
                res.ThrowOnErr();
                break;
        }
    }
    /// <summary>
    /// Creates a new pipe with initial state of <see cref="Res.IsOk"/>.
    /// </summary>
    /// <param name="onError">Defines the way errors will be handled (None/Log/Throw), the behavior will be carried on succeeding pipe states.</param>
    public static Pipe New(OnErr onError) => new(Res.Ok, onError);


    // Prop
    /// <summary>
    /// <inheritdoc cref="Res.IsOk"/>
    /// </summary>
    public bool IsOk => Res.IsOk;
    /// <summary>
    /// <inheritdoc cref="Res.IsErr"/>
    /// </summary>
    public bool IsErr => Res.IsErr;


    // Method - Run
    /// <summary>
    /// When <see cref="Res.IsOk"/>, runs the <paramref name="action"/>; and returns a new Ok pipe state.
    /// Action is ignored and error is carried when <see cref="Res.IsErr"/>.
    /// </summary>
    public Pipe Run(Action action)
    {
        if (Res.IsOk)
            action();
        return this;
    }
    /// <summary>
    /// When <see cref="Res.IsOk"/>, runs the <paramref name="getResult"/>; and returns a new pipe with the state returned by <paramref name="getResult"/>.
    /// Method is ignored and error is carried when <see cref="Res.IsErr"/>.
    /// </summary>
    public Pipe Run(Func<Res> getResult)
    {
        if (Res.IsErr)
            return this;
        return new(getResult(), onErr);
    }
    /// <summary>
    /// When <see cref="Res.IsOk"/>, tries to run the <paramref name="action"/>; and returns a new Ok pipe state if succeeds, Err state if action throws.
    /// Action is ignored and error is carried when <see cref="Res.IsErr"/>.
    /// </summary>
    public Pipe TryRun(Action action)
    {
        if (Res.IsErr)
            return this;
        try
        {
            action();
            return this;
        }
        catch (Exception ex)
        {
            return new(Res.Err(ex), onErr);
        }
    }
    /// <summary>
    /// When <see cref="Res.IsOk"/>, tries to run the <paramref name="getResult"/>; and returns a new Ok pipe state method returns Ok, or Err state if method returns Err or throws.
    /// Method is ignored and error is carried when <see cref="Res.IsErr"/>.
    /// </summary>
    public Pipe TryRun(Func<Res> getResult)
    {
        if (Res.IsErr)
            return this;
        try
        {
            return new(getResult(), onErr);
        }
        catch (Exception ex)
        {
            return new(Res.Err(ex), onErr);
        }
    }

    // Method - RunAsync
    /// <summary>
    /// When <see cref="Res.IsOk"/>, asynchronously runs the <paramref name="action"/>; and returns a new Ok pipe state.
    /// Action is ignored and error is carried when <see cref="Res.IsErr"/>.
    /// </summary>
    public async Task<Pipe> RunAsync(Func<Task> action)
    {
        if (Res.IsOk)
            await action();
        return this;
    }
    /// <summary>
    /// When <see cref="Res.IsOk"/>, asynchronously runs the <paramref name="getResult"/>; and returns a new pipe with the state returned by <paramref name="getResult"/>.
    /// Method is ignored and error is carried when <see cref="Res.IsErr"/>.
    /// </summary>
    public async Task<Pipe> RunAsync(Func<Task<Res>> getResult)
    {
        if (Res.IsErr)
            return this;
        var newRes = await getResult();
        return new(newRes, onErr);
    }
    /// <summary>
    /// When <see cref="Res.IsOk"/>, tries to asynchronously run the <paramref name="action"/>; and returns a new Ok pipe state if succeeds, Err state if action throws.
    /// Action is ignored and error is carried when <see cref="Res.IsErr"/>.
    /// </summary>
    public async Task<Pipe> TryRunAsync(Func<Task> action)
    {
        if (Res.IsErr)
            return this;
        try
        {
            await action();
            return this;
        }
        catch (Exception ex)
        {
            return new(Res.Err(ex), onErr);
        }
    }
    /// <summary>
    /// When <see cref="Res.IsOk"/>, tries to asynchronously run the <paramref name="getResult"/>; and returns a new Ok pipe state method returns Ok, or Err state if method returns Err or throws.
    /// Method is ignored and error is carried when <see cref="Res.IsErr"/>.
    /// </summary>
    public async Task<Pipe> TryRunAsync(Func<Task<Res>> getResult)
    {
        if (Res.IsErr)
            return this;
        try
        {
            var newRes = await getResult();
            return new(newRes, onErr);
        }
        catch (Exception ex)
        {
            return new(Res.Err(ex), onErr);
        }
    }

    // Method - Map
    /// <summary>
    /// When <see cref="IsOk"/>, runs the <paramref name="mapper"/>; and create a new Ok pipe state with the returned value of <typeparamref name="TOut"/>.
    /// Method is ignored and error is carried when <see cref="Res.IsErr"/>.
    /// </summary>
    public Pipe<TOut> Map<TOut>(Func<TOut> mapper)
    {
        if (Res.IsErr)
            return new(Res<TOut>.ErrFrom(Res), onErr, true);
        var value = mapper();
        return new(Res<TOut>.Ok(value), onErr);
    }
    /// <summary>
    /// When <see cref="IsOk"/>, runs the <paramref name="maybeMapper"/>; and create a new Ok pipe state with the returned value of <typeparamref name="TOut"/> if <see cref="Opt{TOut}.Some"/>, or Err if <see cref="Opt{TOut}.None"/>.
    /// Method is ignored and error is carried when <see cref="Res.IsErr"/>.
    /// </summary>
    public Pipe<TOut> Map<TOut>(Func<Opt<TOut>> maybeMapper)
    {
        if (Res.IsErr)
            return new(Res<TOut>.ErrFrom(Res), onErr, true);
        var value = maybeMapper();
        return value.IsNone ? new(Res<TOut>.Err("mapped-to-None"), onErr) : new(Res<TOut>.Ok(value.Unwrap()), onErr);
    }
    /// <summary>
    /// When <see cref="IsOk"/>, tries to run the <paramref name="mapper"/>; and create a new Ok pipe state with the returned value of <typeparamref name="TOut"/> if succeeds, or Err if the method throws.
    /// Method is ignored and error is carried when <see cref="Res.IsErr"/>.
    /// </summary>
    public Pipe<TOut> TryMap<TOut>(Func<TOut> mapper)
    {
        if (Res.IsErr)
            return new(Res<TOut>.ErrFrom(Res), onErr, true);
        try
        {
            var value = mapper();
            return new(Res<TOut>.Ok(value), onErr);
        }
        catch (Exception ex)
        {
            return new(Res<TOut>.Err(ex), onErr);
        }
    }
    /// <summary>
    /// When <see cref="IsOk"/>, tries to run the <paramref name="maybeMapper"/>; and create a new Ok pipe state with the returned value of <typeparamref name="TOut"/> if <see cref="Opt{TOut}.Some"/>, or Err if the method throws or returns <see cref="Opt{TOut}.None"/>.
    /// Method is ignored and error is carried when <see cref="Res.IsErr"/>.
    /// </summary>
    public Pipe<TOut> TryMap<TOut>(Func<Opt<TOut>> maybeMapper)
    {
        if (Res.IsErr)
            return new(Res<TOut>.ErrFrom(Res), onErr, true);
        try
        {
            var value = maybeMapper();
            return value.IsNone ? new(Res<TOut>.Err("mapped-to-None"), onErr) : new(Res<TOut>.Ok(value.Unwrap()), onErr);
        }
        catch (Exception ex)
        {
            return new(Res<TOut>.Err(ex), onErr);
        }
    }

    // Method - MapAsync
    /// <summary>
    /// When <see cref="IsOk"/>, asynchronously runs the <paramref name="mapper"/>; and create a new Ok pipe state with the returned value of <typeparamref name="TOut"/>.
    /// Method is ignored and error is carried when <see cref="Res.IsErr"/>.
    /// </summary>
    public async Task<Pipe<TOut>> MapAsync<TOut>(Func<Task<TOut>> mapper)
    {
        if (Res.IsErr)
            return new(Res<TOut>.ErrFrom(Res), onErr, true);
        var value = await mapper();
        return new(Res<TOut>.Ok(value), onErr);
    }
    /// <summary>
    /// When <see cref="IsOk"/>, asynchronously runs the <paramref name="maybeMapper"/>; and create a new Ok pipe state with the returned value of <typeparamref name="TOut"/> if <see cref="Opt{TOut}.Some"/>, or Err if <see cref="Opt{TOut}.None"/>.
    /// Method is ignored and error is carried when <see cref="Res.IsErr"/>.
    /// </summary>
    public async Task<Pipe<TOut>> MapAsync<TOut>(Func<Task<Opt<TOut>>> maybeMapper)
    {
        if (Res.IsErr)
            return new(Res<TOut>.ErrFrom(Res), onErr, true);
        var value = await maybeMapper();
        return value.IsNone ? new(Res<TOut>.Err("mapped-to-None"), onErr) : new(Res<TOut>.Ok(value.Unwrap()), onErr);
    }
    /// <summary>
    /// When <see cref="IsOk"/>, tries to asynchronously run the <paramref name="mapper"/>; and create a new Ok pipe state with the returned value of <typeparamref name="TOut"/> if succeeds, or Err if the method throws.
    /// Method is ignored and error is carried when <see cref="Res.IsErr"/>.
    /// </summary>
    public async Task<Pipe<TOut>> TryMapAsync<TOut>(Func<Task<TOut>> mapper)
    {
        if (Res.IsErr)
            return new(Res<TOut>.ErrFrom(Res), onErr, true);
        try
        {
            var value = await mapper();
            return new(Res<TOut>.Ok(value), onErr);
        }
        catch (Exception ex)
        {
            return new(Res<TOut>.Err(ex), onErr);
        }
    }
    /// <summary>
    /// When <see cref="IsOk"/>, tries to asynchronously run the <paramref name="maybeMapper"/>; and create a new Ok pipe state with the returned value of <typeparamref name="TOut"/> if <see cref="Opt{TOut}.Some"/>, or Err if the method throws or returns <see cref="Opt{TOut}.None"/>.
    /// Method is ignored and error is carried when <see cref="Res.IsErr"/>.
    /// </summary>
    public async Task<Pipe<TOut>> TryMapAsync<TOut>(Func<Task<Opt<TOut>>> maybeMapper)
    {
        if (Res.IsErr)
            return new(Res<TOut>.ErrFrom(Res), onErr, true);
        try
        {
            var value = await maybeMapper();
            return value.IsNone ? new(Res<TOut>.Err("mapped-to-None"), onErr) : new(Res<TOut>.Ok(value.Unwrap()), onErr);
        }
        catch (Exception ex)
        {
            return new(Res<TOut>.Err(ex), onErr);
        }
    }
}
