using System.Threading.Tasks;
namespace Fun.Pipe.Examples;

public static class ExamplePipeParse
{
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
    static Res<int> PipeExplicit(double flip)
    {
        // one operation per line to see all type maps; note that 'var's on the lhs's would also be perfectly fine
        Opt<string> filepath = GetFilepathFromUser(flip);               // note that GetFilepathFromUser now rightfully returns Opt<string> as the user might choose not to provide
        Res<int[]> numbers = filepath.TryMap(f => RiskyParse(f));       // we use 'filepath.TryMap' rather than 'TryMap' to prevent the RiskyParse call when filepath.IsNone
        Res<int> sum = numbers.TryMap(n => LogAndGetSumAmounts(n));     // note that int[]->int method LogAndGetSumAmounts is mapped to Res<int[]>->Res<int> with TryMap.
        return sum.MsgIfErr("failed to get sum from file").LogIfErr();  // just to make the error message contain the whole story and log; one could've just returned sum
    }
    static Res<int> PipeChain(double flip)
    {
        return GetFilepathFromUser(flip)
            .TryMap(filepath => RiskyParse(filepath))
            .TryMap(numbers => LogAndGetSumAmounts(numbers))
            .MsgIfErr("failed to get sum from file").LogIfErr();
    }
    static async Task<Res<int>> PipeExplicitAsync(double flip)
    {
        // note that this is exactly same as Example.PipeExplicit; except that methods are replaced with their async counterparts
        Opt<string> filepath = await GetFilepathFromUserAsync(flip);
        Res<int[]> numbers = await filepath.TryMapAsync(f => RiskyParseAsync(f));
        Res<int> sum = await numbers.TryMapAsync(n => LogAndGetSumAmountsAsync(n));
        return sum.MsgIfErr("failed to get sum from file").LogIfErr();
    }
    internal static void Run()
    {
        Log($"\n\n\n--- {nameof(ExamplePipeParse)} ---");
        double flip = new Random().NextDouble();
        string resultMustBe = "result-must-be: " + (GetFilepathFromUser(flip).IsNone ? "filepath-not-provided" : GetFilepathFromUser(flip).Unwrap());
        bool willFail = flip < 0.75;
        Log($"flipped: {flip}\n{resultMustBe}\nwill-fail: {willFail}");

        RunExample("Imperative", () =>
        {
            var sum = Imperative(flip);
            Assert(sum == -1 || sum == 45);
        });
        RunExample("PipeExplicit", () =>
        {
            var sum = PipeExplicit(flip);
            Assert(sum.IsErr || sum.Unwrap() == 45);        // result is explicitly an Err or Ok with a value
        });
        RunExample("PipeChain", () =>
        {
            var sum = PipeChain(flip);
            Assert(sum.IsErr || sum.Unwrap() == 45);        // result is explicitly an Err or Ok with a value
        });
        RunExample("PipeExplicitAsync", () =>
        {
            var sum = PipeExplicitAsync(flip).GetAwaiter().GetResult();
            Assert(sum.IsErr || sum.Unwrap() == 45);        // result is explicitly an Err or Ok with a value
        });
    }
}
