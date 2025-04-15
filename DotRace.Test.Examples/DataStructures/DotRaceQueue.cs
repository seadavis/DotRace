
using System.ComponentModel;

namespace DotRace.Test.Examples.DataStructures
{
    public class DotRaceQueue
    {

      private int[] _buffer;
      private int _head;
      private int _tail;
      private int _count;

      public DotRaceQueue(int capacity = 16)
      {
         _buffer = new int[capacity];
         _head = 0;
         _tail = 0;
         _count = 0;
      }

      public void Enqueue(int v)
      {
         if (_count == _buffer.Length)
            Grow();

         _buffer[_tail] = v;
         _tail = (_tail + 1) % _buffer.Length;
         _count++;
      }

      public int Dequeue()
      {
         if (_count == 0)
            throw new InvalidOperationException("Queue is empty");

         int value = _buffer[_head];
         _head = (_head + 1) % _buffer.Length;
         _count--;
         return value;
      }

      public void Clear()
      {
         _head = 0;
         _tail = 0;
         _count = 0;
         // Optional: clear array if needed
      }

      public int Peek()
      {
         if (_count == 0)
            throw new InvalidOperationException("Queue is empty");

         return _buffer[_head];
      }

      private void Grow()
      {
         int newCapacity = _buffer.Length * 2;
         int[] newBuffer = new int[newCapacity];

         for (int i = 0; i < _count; i++)
         {
            newBuffer[i] = _buffer[(_head + i) % _buffer.Length];
         }

         _buffer = newBuffer;
         _head = 0;
         _tail = _count;
      }

   }
}
