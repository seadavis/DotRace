using DotRace.Common;
using DotRace.Core;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;

class Program
{
   class Worker
   {
      public required int ThreadId { get; init; }

      public required Thread Thread { get; init; }

      public required AutoResetEvent ContinueSignal { get; init; }

      public required AutoResetEvent CheckpointSignal { get; init; }
   }

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

      Synchronizer.Create();

      var testAssembly = args[1];
      var testRunner = new TestRunner();
      var fixtures = TestDiscoverer.GetTestFixtures(testAssembly);

      testRunner.Run(fixtures, iterations);
   }

   

   static void WorkerProcessing(string[] args)
   {

      int processorCount = Environment.ProcessorCount;
      if (args.Length != 5 ||
         !int.TryParse(args[1], out int threadCount) ||
         threadCount <= 0 ||
         !int.TryParse(args[2], out int iterations) ||
         iterations <= 0 ||
         !int.TryParse(args[3], out int pauseIterations) ||
         pauseIterations <= 0)
      {
         Console.WriteLine("Usage: dotnet run <assemblyname> <baseName_of_thread_file> <threadCount> <iterations> <pause_iterations>");
         return;
      }

      string baseName = args[0];
      var barrier = new Barrier(threadCount, (b) =>
      {
         Console.WriteLine("All threads are ready. Starting execution...");
      });

      Queue<Worker> workers = new Queue<Worker>(threadCount);

      for (int i = 0; i < threadCount; i++)
      {
         int threadId = i;
         string filePath = $"{baseName}_{threadId}.log";
         var continueSignal = new AutoResetEvent(false);
         var checkpointSignal = new AutoResetEvent(false);

         var thread = new Thread(() => WriteToFile(filePath, threadId, iterations, pauseIterations, continueSignal, checkpointSignal, barrier));
         var worker = new Worker()
         {
            Thread = thread,
            ThreadId = threadId,
            ContinueSignal = continueSignal,
            CheckpointSignal = checkpointSignal
         };
         workers.Enqueue(worker);
         thread.Start();
      }

      while(workers.Any())
      {
         var worker = workers.Dequeue();

         if (worker.Thread.Join(0))
         {
            Console.WriteLine($"Thread {worker.ThreadId} Terminated");
         }
         else
         {
            worker.ContinueSignal.Set();
            Console.WriteLine($"Waiting on checkpoint for: {worker.ThreadId}");
            worker.CheckpointSignal.WaitOne();
            workers.Enqueue(worker);
            Console.WriteLine($"Thread: {worker.ThreadId} re-queued");
         }
      }

      Console.WriteLine("All threads finished.");
      Console.ReadKey();
   }

   static void WriteToFile(string filePath,
                           int threadId,
                           int iterations,
                           int pauseIterations,
                           AutoResetEvent signal,
                           AutoResetEvent checkpointSignal,
                           Barrier barrier)
   {
      Console.WriteLine($"Thread {threadId} is ready.");
      barrier.SignalAndWait();
      Console.WriteLine("All threads are ready");

      signal.WaitOne();

      using (StreamWriter writer = new StreamWriter(filePath, append: false))
      {
         for (int i = 0; i < iterations; i++) // Example workload
         {
            if((i + 1)%pauseIterations == 0)
            {
               checkpointSignal.Set();
               signal.WaitOne();
            }
            writer.WriteLine($"Thread {threadId} writing line {i} at {DateTime.Now:O}");
         }
      }

      checkpointSignal.Set();
      Console.WriteLine($"Thread {threadId} finished writing.");
   }
}
