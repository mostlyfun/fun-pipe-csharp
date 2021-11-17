using System.Threading.Tasks;
namespace Fun.Pipe.Examples;

public static class ExampleHelpers
{
    // General
    internal static void Log(object value) => Console.WriteLine(value);
    internal static void RunExample(string name, Action action)
    {
        Log($"-- {name} --");
        action();
        Log("\n");
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
            < 0.25 => Opt<string>.None,
            < 0.50 => Opt<string>.Some("nonnumeric-file"),
            < 0.75 => Opt<string>.Some("negative-file"),
            _ => Opt<string>.Some("good-file"),
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
    internal static void LogSumAmounts(int[] numbers)
    {
        int sum = 0;
        for (int i = 0; i < numbers.Length; i++)
        {
            if (numbers[i] < 0)
                throw new ArgumentException($"Numbers must be nonnegative, but found {numbers[i]}");
            sum += numbers[i];
        }
        Log($"Total amount: {sum}");
    }

    // Fake async versions for demo purposes
    internal static Task<string> GetFilepathFromUserAsync(double flip)
        => Task.FromResult(GetFilepathFromUserMaybeNull(flip));
    internal static Task<int[]> RiskyParseAsync(string filepath)
        => Task.FromResult(RiskyParse(filepath));
    internal static Task LogSumAmountsAsync(int[] numbers)
    {
        LogSumAmounts(numbers);
        return Task.Delay(10);
    }
}
