using DotRace.Core;

class Program
{

   static void Main(string[] args)
   {
      int processorCount = Environment.ProcessorCount;
      if (args.Length != 5 ||
         !int.TryParse(args[2], out int threadCount) ||
         threadCount <= 0 ||
         !int.TryParse(args[3], out int iterations) ||
         iterations <= 0 ||
         !int.TryParse(args[4], out int pauseIterations) ||
         pauseIterations <= 0)
      {
         Console.WriteLine("Usage: dotnet run <assemblyname> <baseName_of_thread_file> <threadCount> <iterations> <pause_iterations>");
         return;
      }

      var testAssembly = args[1];
      var testRunner = new TestRunner();
      var fixtures = TestDiscoverer.GetTestFixtures(testAssembly);
      testRunner.Run(fixtures, iterations, pauseIterations, threadCount);
   }

  
}
