
using DotRace.Common;
using System.Reflection;
using Mono.Cecil;

namespace DotRace.Core
{
   public class TestDiscoverer
   {

      public static IEnumerable<TestFixture> GetTestFixtures(string assemblyPath)
      {
         AssemblyDefinition assemblyDefinition = AssemblyDefinition.ReadAssembly(assemblyPath);
         var methods = FindMethodsWithAttribute(assemblyDefinition, typeof(Operation).FullName);
         var injectedAssembly = Injector.InjectSynchronizeCall(assemblyDefinition, methods);
         var testAssembly = Assembly.Load(injectedAssembly.ToArray());


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

      public static List<MethodDefinition> FindMethodsWithAttribute(AssemblyDefinition assembly, string attributeFullName)
      {
         var results = new List<MethodDefinition>();

         foreach (var type in assembly.MainModule.Types)
         {
            FindInType(type, attributeFullName, results);
         }

         return results;
      }

      private static void FindInType(TypeDefinition type, string attributeFullName, List<MethodDefinition> results)
      {
         // Check nested types too
         foreach (var nested in type.NestedTypes)
         {
            FindInType(nested, attributeFullName, results);
         }

         foreach (var method in type.Methods)
         {
            if (!method.HasCustomAttributes)
               continue;

            if (method.CustomAttributes.Any(attr => attr.AttributeType.FullName == attributeFullName))
            {
               results.Add(method);
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
