using System.Threading.Tasks;
namespace Fun.Pipe.Examples;

internal static class ExamplePipeHelpers
{
    // General
    internal record Wizard(string Name, int NbSpells);
    internal static void Log(object value) => Console.WriteLine(value);
    internal static void RunExample(string name, Action action)
    {
        Log($"\n[ {name} ]");
        action();
    }
    internal static void Assert(bool expected)
    {
        if (expected) return;
        Err($"Assertion failed").LogIfErr();
    }
    internal static void Assert(bool expected, string errorMessage)
    {
        if (expected) return;
        Err($"Assertion failed: {errorMessage}").LogIfErr();
    }


    // Scenario
    internal static string GetFilepathFromUserMaybeNull(double flip)
    {
        // assume there is a user interaction, where the user might:
        // * cancel and not provide any path,
        // * provide a nonnumeric-file path that does not have numbers,
        // * provide a negative-file path that has negative values which will be a problem later, or
        // * provide a good filepath, that works.
        return flip switch
        {
            < 0.25 => null,
            < 0.50 => "nonnumeric-file",
            < 0.75 => "negative-file",
            _ => "good-file",
        };
    }
    internal static Opt<string> GetFilepathFromUser(double flip)
    {
        return flip switch
        {
            < 0.25 => None<string>(),
            < 0.50 => Some<string>("nonnumeric-file"),
            < 0.75 => Some<string>("negative-file"),
            _ => Some<string>("good-file"),
        };
    }
    internal static int[] RiskyParse(string filepath)
    {
        return filepath switch
        {
            "good-file" => Enumerable.Range(0, 10).ToArray(),
            "negative-file" => Enumerable.Range(-5, 5).ToArray(),
            "nonnumeric-file" => throw new ArgumentException($"error while parsing {filepath}"),
            _ => throw new ArgumentException("unknown file"),
        };
    }
    internal static int LogAndGetSumAmounts(int[] numbers)
    {
        int sum = 0;
        for (int i = 0; i < numbers.Length; i++)
        {
            if (numbers[i] < 0)
                throw new ArgumentException($"Numbers must be nonnegative, but found {numbers[i]}");
            sum += numbers[i];
        }
        Log($"Total amount: {sum}");
        return sum;
    }

    // Fake async versions for demo purposes
    internal static Task<string> GetFilepathFromUserAsync(double flip)
        => Task.FromResult(GetFilepathFromUserMaybeNull(flip));
    internal static Task<int[]> RiskyParseAsync(string filepath)
        => Task.FromResult(RiskyParse(filepath));
    internal static async Task<int> LogAndGetSumAmountsAsync(int[] numbers)
    {
        await Task.Delay(10);
        return LogAndGetSumAmounts(numbers);
    }
}
