using System;
using System.IO;
using System.Threading;

class Program
{
   static void Main(string[] args)
   {

      int processorCount = Environment.ProcessorCount;
      if (args.Length != 3 || !int.TryParse(args[1], out int threadCount) || threadCount <= 0 || !int.TryParse(args[2], out int iterations))
      {
         Console.WriteLine("Usage: dotnet run <baseName> <threadCount> <iterations> ");
         return;
      }

      string baseName = args[0];
      var barrier = new Barrier(threadCount, (b) =>
      {
         Console.WriteLine("All threads are ready. Starting execution...");
      });

      Thread[] threads = new Thread[threadCount];

      for (int i = 0; i < threadCount; i++)
      {
         int threadId = i;
         string filePath = $"{baseName}_{threadId}.log";

         threads[i] = new Thread(() => WriteToFile(filePath, threadId, iterations, barrier));
         threads[i].Start();
      }

      foreach (var thread in threads)
      {
         thread.Join();
      }

      Console.WriteLine("All threads finished.");
   }

   static void WriteToFile(string filePath, int threadId, int iterations, Barrier barrier)
   {
      Console.WriteLine($"Thread {threadId} is ready.");
      barrier.SignalAndWait();

      using (StreamWriter writer = new StreamWriter(filePath, append: false))
      {
         for (int i = 0; i < 100; i++) // Example workload
         {
            writer.WriteLine($"Thread {threadId} writing line {i} at {DateTime.Now:O}");
         }
      }

      Console.WriteLine($"Thread {threadId} finished writing.");
   }
}
