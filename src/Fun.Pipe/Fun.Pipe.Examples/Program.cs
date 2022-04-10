await ExampleOpt.Run();
ExampleRes.Run();
await ExampleResT.Run();
return;
ExamplePipeParse.Run();     // pipe examples will lead to different result in each run
ExamplePipeWebReq.Run();    // due to randomization to simulate different cases.
