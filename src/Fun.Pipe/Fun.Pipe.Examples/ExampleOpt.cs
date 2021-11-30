using System.Collections.Generic;
using System.Data;

namespace Fun.Pipe.Examples;

public static class ExampleOpt
{
    internal static void Run()
    {
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
    }
}
