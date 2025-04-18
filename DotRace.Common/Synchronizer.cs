namespace DotRace.Common
{
   public sealed class Synchronizer
   {
      private static Synchronizer _current;

      public static Synchronizer Current => _current ?? throw new InvalidOperationException("Synchronizer not created. Call Create first.");

      private Synchronizer()
      {
         // Private to force creation through Create
      }

      public static void Create()
      {
         if (_current != null)
            throw new InvalidOperationException("Synchronizer already created.");

         _current = new Synchronizer();
      }

      public void Synchronize(int number, string text)
      {
         // Your synchronization logic here
         Console.WriteLine($"Synchronizing: {number}, {text}");
      }

   }
}
