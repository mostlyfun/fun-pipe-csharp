using System.Text.RegularExpressions;

string input = @"   at Program.<<Main>$>g__Dbl|0_0(Int32 a) in C:\Users\uarikan\Documents\GitHub\fun-pipe-csharp\src\Fun.Pipe\Fun.Pipe.Examples\Program.cs:line 15";
string pattern = @"( at )|( in )|(:line )";
var parts = Regex.Split(input, pattern);
int indLastSlash = parts[4].LastIndexOf('\\');
string file = indLastSlash < 1 ? parts[4] : parts[4].Substring(indLastSlash + 1);

/*
Console.Write(file);
Console.Write(":");
Console.Write(parts[6]);
Console.Write(" |    ");
Console.Write(parts[2]);
Console.WriteLine("\n\n");
return;
//*/


/*
static int Dbl(int a) => throw new ArgumentException("problem with a");
static int Halve(int a) => a / 2;
static int Triple(int a) => a * 3;

Console.WriteLine("\n\n\n");
Try(() => Console.WriteLine(Triple(Halve(Dbl(3))))).MsgIfErr("sth has went wrong!").MsgIfErr("again!!!!").Log("working on my homework");
return;
//*/

ExampleOpt.Run();
ExampleRes.Run();
ExamplePipeParse.Run();     // pipe examples will lead to different result in each run
ExamplePipeWebReq.Run();    // due to randomization to simulate different cases.
