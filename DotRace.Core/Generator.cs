namespace DotRace.Core
{
   public class Generator
   {
      private readonly Random _random = new Random();

      public int NextInt(int n)
      {
         if (n < 1)
            throw new ArgumentOutOfRangeException(nameof(n), "n must be >= 1");

         return _random.Next(0, n + 1); // Next is exclusive of upper bound
      }
   }
}
