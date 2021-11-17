using System;
using System.Threading.Tasks;
namespace Fun;

/// <summary>
/// Pipe operation holding a <see cref="Res{T}"/> which is an Err or Ok with Some captured value.
/// (Try)-Run/Map methods do nothing when <see cref="Res.IsErr"/> other than creating a new pipe state with the error; i.e. the operations are bypassed and the error is carried on.
/// When <see cref="Res.IsOk"/>, a new pipe state is created with result of the operation.
/// </summary>
public readonly struct Pipe<T>
{
    // Data
    /// <summary>
    /// Result, state, of the pipe, which is either an Err with captured exception or error message, or Ok with Some captured value.
    /// </summary>
    public readonly Res<T> Res;
    readonly OnErr onErr;


    // Ctor
    internal Pipe(Res<T> res, OnErr onErr, bool errorAlreadyHandled = false)
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


    // Prop
    /// <summary>
    /// <inheritdoc cref="Res.IsOk"/>
    /// </summary>
    public bool IsOk => Res.IsOk;
    /// <summary>
    /// <inheritdoc cref="Res.IsErr"/>
    /// </summary>
    public bool IsErr => Res.IsErr;
    // Method
    /// <summary>
    /// Returns the result value when <see cref="Res.IsOk"/>; or throws when <see cref="Res.IsErr"/>.
    /// </summary>
    public T Unwrap() => Res.Unwrap();
    /// <summary>
    /// Returns the result value when <see cref="Res.IsOk"/>; or returns the <paramref name="fallbackValue"/> when <see cref="Res.IsErr"/>.
    /// </summary>
    /// <param name="fallbackValue"></param>
    public T Unwrap(T fallbackValue) => Res.Unwrap(fallbackValue);


    // Method - Run
    /// <summary>
    /// <inheritdoc cref="Pipe.Run(Action)"/>
    /// </summary>
    public Pipe Run(Action action)
    {
        if (Res.IsErr)
            return new Pipe(Res.res, onErr, true);
        action();
        return new(Fun.Res.Ok, onErr);
    }
    /// <summary>
    /// When <see cref="Res.IsOk"/>, runs the <paramref name="action"/> with captured value <see cref="Res{T}.Unwrap"/>; and returns a new Ok pipe state.
    /// Action is ignored and error is carried when <see cref="Res.IsErr"/>.
    /// </summary>
    public Pipe Run(Action<T> action)
    {
        if (Res.IsErr)
            return new Pipe(Res.res, onErr, true);
        action(Res.Unwrap());
        return new(Fun.Res.Ok, onErr);
    }
    /// <summary>
    /// <inheritdoc cref="Pipe.Run(Func{Res})"/>
    /// </summary>
    public Pipe Run(Func<Res> getResult)
    {
        if (Res.IsErr)
            return new Pipe(Res.res, onErr, true);
        var newRes = getResult();
        return new(newRes, onErr);
    }
    /// <summary>
    /// When <see cref="Res.IsOk"/>, runs the <paramref name="getResult"/> with captured value <see cref="Res{T}.Unwrap"/>; and returns a new pipe with the state returned by <paramref name="getResult"/>.
    /// Method is ignored and error is carried when <see cref="Res.IsErr"/>.
    /// </summary>
    public Pipe Run(Func<T, Res> getResult)
    {
        if (Res.IsErr)
            return new Pipe(Res.res, onErr, true);
        var newRes = getResult(Res.Unwrap());
        return new(newRes, onErr);
    }
    /// <summary>
    /// <inheritdoc cref="Pipe.TryRun(Action)"/>
    /// </summary>
    public Pipe TryRun(Action action)
    {
        if (Res.IsErr)
            return new Pipe(Res.res, onErr, true);
        try
        {
            action();
            return new Pipe(Fun.Res.Ok, onErr);
        }
        catch (Exception ex)
        {
            return new Pipe(Fun.Res.Err(ex), onErr);
        }
    }
    /// <summary>
    /// When <see cref="Res.IsOk"/>, tries to run the <paramref name="action"/> with captured value <see cref="Res{T}.Unwrap"/>; and returns a new Ok pipe state if succeeds, Err state if action throws.
    /// Action is ignored and error is carried when <see cref="Res.IsErr"/>.
    /// </summary>
    public Pipe TryRun(Action<T> action)
    {
        if (Res.IsErr)
            return new Pipe(Res.res, onErr, true);
        try
        {
            action(Res.Unwrap());
            return new Pipe(Fun.Res.Ok, onErr);
        }
        catch (Exception ex)
        {
            return new Pipe(Fun.Res.Err(ex), onErr);
        }
    }
    /// <summary>
    /// <inheritdoc cref="Pipe.TryRun(Func{Res})"/>
    /// </summary>
    public Pipe TryRun(Func<Res> getResult)
    {
        if (Res.IsErr)
            return new Pipe(Res.res, onErr, true);
        try
        {
            var newRes = getResult();
            return new Pipe(newRes, onErr);
        }
        catch (Exception ex)
        {
            return new Pipe(Fun.Res.Err(ex), onErr);
        }
    }
    /// <summary>
    /// When <see cref="Res.IsOk"/>, tries to run the <paramref name="getResult"/> with captured value <see cref="Res{T}.Unwrap"/>; and returns a new Ok pipe state method returns Ok, or Err state if method returns Err or throws.
    /// Method is ignored and error is carried when <see cref="Res.IsErr"/>.
    /// </summary>
    public Pipe TryRun(Func<T, Res> getResult)
    {
        if (Res.IsErr)
            return new Pipe(Res.res, onErr, true);
        try
        {
            var newRes = getResult(Res.Unwrap());
            return new Pipe(newRes, onErr);
        }
        catch (Exception ex)
        {
            return new Pipe(Fun.Res.Err(ex), onErr);
        }
    }

    // Method - RunAsync
    /// <summary>
    /// <inheritdoc cref="Pipe.RunAsync(Func{Task})"/>
    /// </summary>
    public async Task<Pipe> RunAsync(Func<Task> action)
    {
        if (Res.IsErr)
            return new Pipe(Res.res, onErr, true);
        await action();
        return new(Fun.Res.Ok, onErr);
    }
    /// <summary>
    /// When <see cref="Res.IsOk"/>, asynchronously runs the <paramref name="action"/> with captured value <see cref="Res{T}.Unwrap"/>; and returns a new Ok pipe state.
    /// Action is ignored and error is carried when <see cref="Res.IsErr"/>.
    /// </summary>
    public async Task<Pipe> RunAsync(Func<T, Task> action)
    {
        if (Res.IsErr)
            return new Pipe(Res.res, onErr, true);
        await action(Res.Unwrap());
        return new(Fun.Res.Ok, onErr);
    }
    /// <summary>
    /// <inheritdoc cref="Pipe.RunAsync(Func{Task{Res}})"/>
    /// </summary>
    public async Task<Pipe> RunAsync(Func<Task<Res>> getResult)
    {
        if (Res.IsErr)
            return new Pipe(Res.res, onErr, true);
        var newRes = await getResult();
        return new(newRes, onErr);
    }
    /// <summary>
    /// When <see cref="Res.IsOk"/>, asynchronously runs the <paramref name="getResult"/> with captured value <see cref="Res{T}.Unwrap"/>; and returns a new pipe with the state returned by <paramref name="getResult"/>.
    /// Method is ignored and error is carried when <see cref="Res.IsErr"/>.
    /// </summary>
    public async Task<Pipe> RunAsync(Func<T, Task<Res>> getResult)
    {
        if (Res.IsErr)
            return new Pipe(Res.res, onErr, true);
        var newRes = await getResult(Res.Unwrap());
        return new(newRes, onErr);
    }
    /// <summary>
    /// <inheritdoc cref="Pipe.TryRunAsync(Func{Task})"/>
    /// </summary>
    public async Task<Pipe> TryRunAsync(Func<Task> action)
    {
        if (Res.IsErr)
            return new Pipe(Res.res, onErr, true);
        try
        {
            await action();
            return new Pipe(Fun.Res.Ok, onErr);
        }
        catch (Exception ex)
        {
            return new Pipe(Fun.Res.Err(ex), onErr);
        }
    }
    /// <summary>
    /// When <see cref="Res.IsOk"/>, tries to asynchronously run the <paramref name="action"/> with captured value <see cref="Res{T}.Unwrap"/>; and returns a new Ok pipe state if succeeds, Err state if action throws.
    /// Action is ignored and error is carried when <see cref="Res.IsErr"/>.
    /// </summary>
    public async Task<Pipe> TryRunAsync(Func<T, Task> action)
    {
        if (Res.IsErr)
            return new Pipe(Res.res, onErr, true);
        try
        {
            await action(Res.Unwrap());
            return new Pipe(Fun.Res.Ok, onErr);
        }
        catch (Exception ex)
        {
            return new Pipe(Fun.Res.Err(ex), onErr);
        }
    }
    /// <summary>
    /// <inheritdoc cref="Pipe.TryRunAsync(Func{Task{Res}})"/>
    /// </summary>
    public async Task<Pipe> TryRunAsync(Func<Task<Res>> getResult)
    {
        if (Res.IsErr)
            return new Pipe(Res.res, onErr, true);
        try
        {
            var newRes = await getResult();
            return new Pipe(newRes, onErr);
        }
        catch (Exception ex)
        {
            return new Pipe(Fun.Res.Err(ex), onErr);
        }
    }
    /// <summary>
    /// When <see cref="Res.IsOk"/>, tries to asynchronously run the <paramref name="getResult"/> with captured value <see cref="Res{T}.Unwrap"/>; and returns a new Ok pipe state method returns Ok, or Err state if method returns Err or throws.
    /// Method is ignored and error is carried when <see cref="Res.IsErr"/>.
    /// </summary>
    public async Task<Pipe> TryRunAsync(Func<T, Task<Res>> getResult)
    {
        if (Res.IsErr)
            return new Pipe(Res.res, onErr, true);
        try
        {
            var newRes = await getResult(Res.Unwrap());
            return new Pipe(newRes, onErr);
        }
        catch (Exception ex)
        {
            return new Pipe(Fun.Res.Err(ex), onErr);
        }
    }

    // Method - Map
    /// <summary>
    /// <inheritdoc cref="Pipe.Map{TOut}(Func{TOut})"/>
    /// </summary>
    public Pipe<TOut> Map<TOut>(Func<TOut> mapper)
    {
        if (Res.IsErr)
            return new Pipe<TOut>(Res<TOut>.ErrFrom(Res), onErr, true);
        var newVal = mapper();
        return new(Res<TOut>.Ok(newVal), onErr);
    }
    /// <summary>
    /// When <see cref="IsOk"/>, runs the <paramref name="mapper"/> with captured value <see cref="Res{T}.Unwrap"/>; and create a new Ok pipe state with the returned value of <typeparamref name="TOut"/>.
    /// Method is ignored and error is carried when <see cref="Res.IsErr"/>.
    /// </summary>
    public Pipe<TOut> Map<TOut>(Func<T, TOut> mapper)
    {
        if (Res.IsErr)
            return new Pipe<TOut>(Res<TOut>.ErrFrom(Res), onErr, true);
        var newVal = mapper(Res.Unwrap());
        return new(Res<TOut>.Ok(newVal), onErr);
    }
    /// <summary>
    /// <inheritdoc cref="Pipe.TryMap{TOut}(Func{TOut})"/>
    /// </summary>
    public Pipe<TOut> TryMap<TOut>(Func<TOut> mapper)
    {
        if (Res.IsErr)
            return new Pipe<TOut>(Res<TOut>.ErrFrom(Res), onErr, true);
        try
        {
            var newVal = mapper();
            return new(Res<TOut>.Ok(newVal), onErr);
        }
        catch (Exception ex)
        {
            return new(Res<TOut>.Err(ex), onErr);
        }
    }
    /// <summary>
    /// When <see cref="IsOk"/>, tries to run the <paramref name="mapper"/> with captured value <see cref="Res{T}.Unwrap"/>; and create a new Ok pipe state with the returned value of <typeparamref name="TOut"/> if succeeds, or Err if the method throws.
    /// Method is ignored and error is carried when <see cref="Res.IsErr"/>.
    /// </summary>
    public Pipe<TOut> TryMap<TOut>(Func<T, TOut> mapper)
    {
        if (Res.IsErr)
            return new Pipe<TOut>(Res<TOut>.ErrFrom(Res), onErr, true);
        try
        {
            var newVal = mapper(Res.Unwrap());
            return new(Res<TOut>.Ok(newVal), onErr);
        }
        catch (Exception ex)
        {
            return new(Res<TOut>.Err(ex), onErr);
        }
    }
    /// <summary>
    /// <inheritdoc cref="Pipe.Map{TOut}(Func{Opt{TOut}})"/>
    /// </summary>
    public Pipe<TOut> Map<TOut>(Func<Opt<TOut>> maybeMapper)
    {
        if (Res.IsErr)
            return new Pipe<TOut>(Res<TOut>.ErrFrom(Res), onErr, true);
        var newVal = maybeMapper();
        return newVal.IsNone ? new(Res<TOut>.Err("mapped-to-None"), onErr) : new(Res<TOut>.Ok(newVal.Unwrap()), onErr);
    }
    /// <summary>
    /// When <see cref="IsOk"/>, runs the <paramref name="maybeMapper"/> with captured value <see cref="Res{T}.Unwrap"/>; and create a new Ok pipe state with the returned value of <typeparamref name="TOut"/> if <see cref="Opt{TOut}.Some"/>, or Err if <see cref="Opt{TOut}.None"/>.
    /// Method is ignored and error is carried when <see cref="Res.IsErr"/>.
    /// </summary>
    public Pipe<TOut> Map<TOut>(Func<T, Opt<TOut>> maybeMapper)
    {
        if (Res.IsErr)
            return new Pipe<TOut>(Res<TOut>.ErrFrom(Res), onErr, true);
        var newVal = maybeMapper(Res.Unwrap());
        return newVal.IsNone ? new(Res<TOut>.Err("mapped-to-None"), onErr) : new(Res<TOut>.Ok(newVal.Unwrap()), onErr);
    }
    /// <summary>
    /// <inheritdoc cref="Pipe.TryMap{TOut}(Func{Opt{TOut}})"/>
    /// </summary>
    public Pipe<TOut> TryMap<TOut>(Func<Opt<TOut>> maybeMapper)
    {
        if (Res.IsErr)
            return new Pipe<TOut>(Res<TOut>.ErrFrom(Res), onErr, true);
        try
        {
            var newVal = maybeMapper();
            return newVal.IsNone ? new(Res<TOut>.Err("mapped-to-None"), onErr) : new(Res<TOut>.Ok(newVal.Unwrap()), onErr);
        }
        catch (Exception ex)
        {
            return new(Res<TOut>.Err(ex), onErr);
        }
    }
    /// <summary>
    /// When <see cref="IsOk"/>, tries to run the <paramref name="maybeMapper"/> with captured value <see cref="Res{T}.Unwrap"/>; and create a new Ok pipe state with the returned value of <typeparamref name="TOut"/> if <see cref="Opt{TOut}.Some"/>, or Err if the method throws or returns <see cref="Opt{TOut}.None"/>.
    /// Method is ignored and error is carried when <see cref="Res.IsErr"/>.
    /// </summary>
    public Pipe<TOut> TryMap<TOut>(Func<T, Opt<TOut>> maybeMapper)
    {
        if (Res.IsErr)
            return new Pipe<TOut>(Res<TOut>.ErrFrom(Res), onErr, true);
        try
        {
            var newVal = maybeMapper(Res.Unwrap());
            return newVal.IsNone ? new(Res<TOut>.Err("mapped-to-None"), onErr) : new(Res<TOut>.Ok(newVal.Unwrap()), onErr);
        }
        catch (Exception ex)
        {
            return new(Res<TOut>.Err(ex), onErr);
        }
    }

    // Method - MapAsync
    /// <summary>
    /// <inheritdoc cref="Pipe.MapAsync{TOut}(Func{Task{Opt{TOut}}})"/>
    /// </summary>
    public async Task<Pipe<TOut>> MapAsync<TOut>(Func<Task<TOut>> mapper)
    {
        if (Res.IsErr)
            return new Pipe<TOut>(Res<TOut>.ErrFrom(Res), onErr, true);
        var newVal = await mapper();
        return new(Res<TOut>.Ok(newVal), onErr);
    }
    /// <summary>
    /// When <see cref="IsOk"/>, asynchronously runs the <paramref name="mapper"/> with captured value <see cref="Res{T}.Unwrap"/>; and create a new Ok pipe state with the returned value of <typeparamref name="TOut"/>.
    /// Method is ignored and error is carried when <see cref="Res.IsErr"/>.
    /// </summary>
    public async Task<Pipe<TOut>> MapAsync<TOut>(Func<T, Task<TOut>> mapper)
    {
        if (Res.IsErr)
            return new Pipe<TOut>(Res<TOut>.ErrFrom(Res), onErr, true);
        var newVal = await mapper(Res.Unwrap());
        return new(Res<TOut>.Ok(newVal), onErr);
    }
    /// <summary>
    /// <inheritdoc cref="Pipe.TryMapAsync{TOut}(Func{Task{Opt{TOut}}})"/>
    /// </summary>
    public async Task<Pipe<TOut>> TryMapAsync<TOut>(Func<Task<TOut>> mapper)
    {
        if (Res.IsErr)
            return new Pipe<TOut>(Res<TOut>.ErrFrom(Res), onErr, true);
        try
        {
            var newVal = await mapper();
            return new(Res<TOut>.Ok(newVal), onErr);
        }
        catch (Exception ex)
        {
            return new(Res<TOut>.Err(ex), onErr);
        }
    }
    /// <summary>
    /// When <see cref="IsOk"/>, tries to asynchronously run the <paramref name="mapper"/> with captured value <see cref="Res{T}.Unwrap"/>; and create a new Ok pipe state with the returned value of <typeparamref name="TOut"/> if succeeds, or Err if the method throws.
    /// Method is ignored and error is carried when <see cref="Res.IsErr"/>.
    /// </summary>
    public async Task<Pipe<TOut>> TryMapAsync<TOut>(Func<T, Task<TOut>> mapper)
    {
        if (Res.IsErr)
            return new Pipe<TOut>(Res<TOut>.ErrFrom(Res), onErr, true);
        try
        {
            var newVal = await mapper(Res.Unwrap());
            return new(Res<TOut>.Ok(newVal), onErr);
        }
        catch (Exception ex)
        {
            return new(Res<TOut>.Err(ex), onErr);
        }
    }
    /// <summary>
    /// <inheritdoc cref="MapAsync{TOut}(Func{T, Task{Opt{TOut}}})"/>
    /// </summary>
    public async Task<Pipe<TOut>> MapAsync<TOut>(Func<Task<Opt<TOut>>> maybeMapper)
    {
        if (Res.IsErr)
            return new Pipe<TOut>(Res<TOut>.ErrFrom(Res), onErr, true);
        var newVal = await maybeMapper();
        return newVal.IsNone ? new(Res<TOut>.Err("mapped-to-None"), onErr) : new(Res<TOut>.Ok(newVal.Unwrap()), onErr);
    }
    /// <summary>
    /// When <see cref="IsOk"/>, asynchronously runs the <paramref name="maybeMapper"/> with captured value <see cref="Res{T}.Unwrap"/>; and create a new Ok pipe state with the returned value of <typeparamref name="TOut"/> if <see cref="Opt{TOut}.Some"/>, or Err if <see cref="Opt{TOut}.None"/>.
    /// Method is ignored and error is carried when <see cref="Res.IsErr"/>.
    /// </summary>
    public async Task<Pipe<TOut>> MapAsync<TOut>(Func<T, Task<Opt<TOut>>> maybeMapper)
    {
        if (Res.IsErr)
            return new Pipe<TOut>(Res<TOut>.ErrFrom(Res), onErr, true);
        var newVal = await maybeMapper(Res.Unwrap());
        return newVal.IsNone ? new(Res<TOut>.Err("mapped-to-None"), onErr) : new(Res<TOut>.Ok(newVal.Unwrap()), onErr);
    }
    /// <summary>
    /// <inheritdoc cref="TryMapAsync{TOut}(Func{T, Task{Opt{TOut}}})"/>
    /// </summary>
    public async Task<Pipe<TOut>> TryMapAsync<TOut>(Func<Task<Opt<TOut>>> maybeMapper)
    {
        if (Res.IsErr)
            return new Pipe<TOut>(Res<TOut>.ErrFrom(Res), onErr, true);
        try
        {
            var newVal = await maybeMapper();
            return newVal.IsNone ? new(Res<TOut>.Err("mapped-to-None"), onErr) : new(Res<TOut>.Ok(newVal.Unwrap()), onErr);
        }
        catch (Exception ex)
        {
            return new(Res<TOut>.Err(ex), onErr);
        }
    }
    /// <summary>
    /// When <see cref="IsOk"/>, tries to asynchronously run the <paramref name="maybeMapper"/> with captured value <see cref="Res{T}.Unwrap"/>; and create a new Ok pipe state with the returned value of <typeparamref name="TOut"/> if <see cref="Opt{TOut}.Some"/>, or Err if the method throws or returns <see cref="Opt{TOut}.None"/>.
    /// Method is ignored and error is carried when <see cref="Res.IsErr"/>.
    /// </summary>
    public async Task<Pipe<TOut>> TryMapAsync<TOut>(Func<T, Task<Opt<TOut>>> maybeMapper)
    {
        if (Res.IsErr)
            return new Pipe<TOut>(Res<TOut>.ErrFrom(Res), onErr, true);
        try
        {
            var newVal = await maybeMapper(Res.Unwrap());
            return newVal.IsNone ? new(Res<TOut>.Err("mapped-to-None"), onErr) : new(Res<TOut>.Ok(newVal.Unwrap()), onErr);
        }
        catch (Exception ex)
        {
            return new(Res<TOut>.Err(ex), onErr);
        }
    }
}
