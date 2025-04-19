public sealed class ThreadMonitor
{
   private int _continueCount;
   private int _threadId;
   private int _pauseIterations;
   private readonly AutoResetEvent _continueSignal = new(false);
   private readonly AutoResetEvent _checkpointSignal = new(false);

   public ThreadMonitor(int threadId, int pauseIterations) 
   {
      _threadId = threadId;
      _pauseIterations = pauseIterations;
   }

   public void Continue() => _continueSignal.Set();

   public void WaitForContinue()
   {
      if (_continueCount % _pauseIterations == 0)
      {
         _checkpointSignal.Set();
         _continueSignal.WaitOne();
      }
      else
      {
         _continueCount++;
      }
   }

   public void WaitForCheckpoint() => _checkpointSignal.WaitOne();

   public void Checkpoint() => _checkpointSignal.Set();

   public void Synchronize(int injectedThreadId, string fullMethodName, int result)
   {
      Console.WriteLine(
          $"[Thread {injectedThreadId}] Synchronize(ThreadId={_threadId}, " +
          $"Method={fullMethodName}, Result={result})");
   }
}
