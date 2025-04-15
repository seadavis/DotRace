
using DotRace.Common;
using System.Reflection;

namespace DotRace.Core
{
   public class TestDiscoverer
   {

      public static IEnumerable<TestFixture> GetTestFixtures(Assembly testAssembly)
      {
         foreach (var type in testAssembly.GetTypes())
         {
         
            var operationMethods = GetOperationMethods(type);

            if(operationMethods.Any())
            {
               object instance = Activator.CreateInstance(type);
               var operations = operationMethods.Select(m =>
                                                         new OperationInfo()
                                                         {
                                                            Instance = instance,
                                                            Method = m
                                                         });

               yield return new TestFixture()
               {
                  Operations = operations.ToArray()
               };
            }
         }

      }

      private static IEnumerable<MethodInfo> GetOperationMethods(Type type)
      {
         foreach (var method in type.GetMethods())
         {
            var attr = method.GetCustomAttribute(typeof(Operation));

            if (attr != null)
            {
               yield return method;
            }
            
         }
      }
   }
}
