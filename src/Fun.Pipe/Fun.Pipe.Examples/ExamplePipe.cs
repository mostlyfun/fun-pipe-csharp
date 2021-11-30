using System.Threading.Tasks;

namespace Fun.Pipe.Examples;

public static class ExamplePipe
{
    static void Example(double flip)
    {
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
                return;
            }
        }
        static void PipeWithSideEff(double flip)
        {
            string filepath = null;
            int[] numbers = null;
            var pipe = NewPipe(OnErr.Log)
            .Run(() =>
            {
                filepath = GetFilepathFromUserMaybeNull(flip);
                return filepath != null ? Res.Ok : Res.Err("filepath is not provided");
            })
            .TryRun(() => { numbers = RiskyParse(filepath); })
            .TryRun(() => LogSumAmounts(numbers));
            // keeping pipe var, final result can be queried
            Log($"[final result] {pipe.Res}");
        }
        static void PipeCompactWithSideEff(double flip)
        {
            string filepath = null;
            int[] numbers = null;
            NewPipe(OnErr.Log)
            .Run(() => (filepath = GetFilepathFromUserMaybeNull(flip)) != null ? Res.Ok : Res.Err("filepath is not provided"))
            .TryRun(() => { numbers = RiskyParse(filepath); })
            .TryRun(() => LogSumAmounts(numbers));
        }
        static void PipeCompact(double flip)
        {
            NewPipe(OnErr.Log)
            .Map(() => GetFilepathFromUser(flip))
            .TryMap(filepath => RiskyParse(filepath))
            .TryRun(numbers => LogSumAmounts(numbers));
        }

        Log("\n\nEXAMPLE");
        RunExample("PipeWithSideEff", () => PipeWithSideEff(flip));
        RunExample("PipeCompactWithSideEff", () => PipeCompactWithSideEff(flip));
        RunExample("Imperative", () => Imperative(flip));
        RunExample("PipeCompact", () => PipeCompact(flip));
    }
    static void ExampleAsync(double flip)
    {
        static async Task<Res> PipeExplicitAsync(double flip)
        {
            var pipeFilepath = await NewPipe(OnErr.Log).MapAsync(async () => await GetFilepathFromUserAsync(flip));
            var pipeNumbers = await pipeFilepath.TryMapAsync(async filepath => await RiskyParseAsync(filepath));
            var pipe = await pipeNumbers.TryRunAsync(async numbers => await LogSumAmountsAsync(numbers));
            return pipe.Res;
        }
        static async Task<Res> PipeChainedAsync(double flip)
        {
            // chaining is not necessarily convenient with async/await
            return (await (await (await NewPipe(OnErr.Log)
            .MapAsync(async () => await GetFilepathFromUserAsync(flip)))
            .TryMapAsync(async filepath => await RiskyParseAsync(filepath)))
            .TryRunAsync(async numbers => await LogSumAmountsAsync(numbers)))
            .Res;
        }

        Log("\n\nEXAMPLE-ASYNC");
        RunExample("PipeExplicitAsync", () => Log("async-result: " + PipeExplicitAsync(flip).GetAwaiter().GetResult()));
        RunExample("PipeChainedAsync", () => Log("async-result: " + PipeChainedAsync(flip).GetAwaiter().GetResult()));
    }
    static void ExampleThrowImmediately(double flip)
    {
        // method definition with inline-chain
        static Res PipeThrow(double flip)
            => NewPipe(OnErr.Throw)
            .Map(() => GetFilepathFromUser(flip))
            .TryMap(filepath => RiskyParse(filepath))
            .TryRun(numbers => LogSumAmounts(numbers))
            .Res;

        Log("\n\nEXAMPLE-THROW IMMEDIATELY");
        RunExample("PipeThrow", () => Log("must-be-ok-if-reached-here: " + PipeThrow(flip)));
    }
    static void ExampleSilentlyPass(double flip)
    {
        // method definition with inline-chain
        static Res PipeSilentBypass(double flip)
            => NewPipe(OnErr.None)
            .Map(() => GetFilepathFromUser(flip))
            .TryMap(filepath => RiskyParse(filepath))
            .TryRun(numbers => LogSumAmounts(numbers))
            .Res;

        Log("\n\nEXAMPLE - SILENT BYPASS");
        RunExample("PipeSilentBypass", () => Log("must-reach-here-silently: " + PipeSilentBypass(flip)));
    }

    // run all examples
    internal static void Run()
    {
        double flip = new Random().NextDouble();
        string resultMustBe = "result-must-be: " + (GetFilepathFromUser(flip).IsNone ? "filepath-not-provided" : GetFilepathFromUser(flip).Unwrap());
        bool mustThrow = flip < 0.25;
        Log($"flipped: {flip}\n{resultMustBe}\nwill-throw: {mustThrow}\n\n");
        Example(flip);
        ExampleAsync(flip);
        ExampleSilentlyPass(flip);
        if (mustThrow)
            ExampleThrowImmediately(flip);
    }
}
