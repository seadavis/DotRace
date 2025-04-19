using DotRace.Common;
using System.Threading;

namespace DotRace.Core
{
    public class TestRunner
    {
      private record WorkerInfo(ThreadMonitor ThreadMonitor, TestFixture Fixture, Generator Generator, int Iterations);
      private record Worker(Thread Thread, int ThreadId, WorkerInfo WorkerInfo);


      /// <summary>
      /// Interesting side note, in this 
      /// very preliminary prototype there is only one fixture.
      /// Don't get the fixture mixed up with the threads.
      /// </summary>
      /// <param name="testFixtures"></param>
      /// <param name="iterations"></param>
      /// <param name="numberOfThreads"></param>
      public void Run(IEnumerable<TestFixture> testFixtures, int iterations, int pauseIterations, int numberOfThreads)
      {
         var generator = new Generator();
         Queue<Worker> workers = new Queue<Worker>(numberOfThreads);

         foreach (var testFixture in testFixtures)
         {
            for (int i = 0; i <  numberOfThreads; i++)
            {
               var threadMonitor = new ThreadMonitor(i, pauseIterations);
               var workerInfo = new WorkerInfo(threadMonitor,testFixture, generator, iterations); 
               var thread = new Thread(() => Run(workerInfo));
               var worker = new Worker(thread, i, workerInfo);
               thread.Start();
               workers.Enqueue(worker);
            }

            while (workers.Any())
            {
               var worker = workers.Dequeue();
               if (worker.Thread.Join(0))
               {
                  Console.WriteLine($"Thread {worker.ThreadId} Terminated");
               }
               else
               {

                  Console.WriteLine($"Continuing: {worker.ThreadId}");
                  worker.WorkerInfo.ThreadMonitor.Continue();

                  Console.WriteLine($"Waiting on checkpoint for: {worker.ThreadId}");
                  worker.WorkerInfo.ThreadMonitor.WaitForCheckpoint();

                  Console.WriteLine($"Thread: {worker.ThreadId} re-queued");
                  workers.Enqueue(worker);
                  
               }
            }

         }


      }

      private static void Run(WorkerInfo workerInfo)
      {
         workerInfo.ThreadMonitor.WaitForContinue();
         for (int i = 0; i < workerInfo.Iterations; i++)
         {
            var methodNumber = workerInfo.Generator.NextInt(workerInfo.Fixture.Operations.Count() - 1);
            var operation = workerInfo.Fixture.Operations[methodNumber];
            var generatedParameters = new List<int>();
            var parameters = operation.Method.GetParameters();

            for (int p = 0; p < parameters.Length; p++)
            {
               generatedParameters.Add(workerInfo.Generator.NextInt(100));
            }

            try
            {
               Console.WriteLine($"Calling {operation.Method.Name} with Parameters: {string.Join(",", generatedParameters)}");
               var result = operation.Method.Invoke(operation.Instance, generatedParameters.Cast<object?>().ToArray());
               Console.WriteLine($"Result: {result}, Iteration: {i}");
            }
            catch (Exception ex)
            {
               Console.WriteLine($"Found Exception: {ex.InnerException?.Message}, StackTrace: {ex.InnerException?.StackTrace}, Iteration: {i}");
            }
         }
      }
   }
}
