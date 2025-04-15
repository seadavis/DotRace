using DotRace.Common;
using DotRace.Test.Examples.DataStructures;

namespace DotRace.Test.Examples
{
    public class QueueTest
    {
      private DotRaceQueue _queue = new DotRaceQueue();

      [Operation]
      public void Enqueue(int v)
      {
         _queue.Enqueue(v);
      }

      [Operation]
      public int Dequeue()
      {
         return _queue.Dequeue();
      }

      [Operation]
      public int Peek()
      {
         return _queue.Peek();
      }


    }
}
