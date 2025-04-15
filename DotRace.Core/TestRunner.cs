namespace DotRace.Core
{
    public class TestRunner
    {
      public void Run(IEnumerable<TestFixture> testFixtures, int iterations)
      {
         var generator = new Generator();

         foreach (var fixture in testFixtures)
         {
            for (int i = 0; i < iterations; i++)
            {
               var methodNumber = generator.NextInt(fixture.Operations.Count() - 1);
               var operation = fixture.Operations[methodNumber];
               var generatedParameters = new List<int>();
               var parameters = operation.Method.GetParameters();

               for (int p = 0; p < parameters.Length; p++)
               {
                  generatedParameters.Add(generator.NextInt(100));
               }

               try
               {
                  Console.WriteLine($"Calling {operation.Method.Name} with Parameters: {string.Join(",", generatedParameters)}");
                  var result = operation.Method.Invoke(operation.Instance, generatedParameters.Cast<object?>().ToArray());
                  Console.WriteLine($"Result: {result}");
               }
               catch (Exception ex)
               {
                  Console.WriteLine($"Found Exception: {ex.InnerException?.Message}, StackTrace: {ex.InnerException?.StackTrace}");
               }
            }
         }
      }

    }
}
