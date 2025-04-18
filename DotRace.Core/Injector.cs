using DotRace.Common;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DotRace.Core
{
    public class Injector
    {
      public static MemoryStream InjectSynchronizeCall(AssemblyDefinition assembly, IEnumerable<MethodDefinition> methodDefinitions)
      { 
         var module = assembly.MainModule;

         var currentProp = module.ImportReference(typeof(Synchronizer).GetProperty("Current").GetGetMethod());
         var syncMethod = module.ImportReference(typeof(Synchronizer).GetMethod("Synchronize"));

         foreach (var method in methodDefinitions)
         {
            if (!method.HasBody)
               continue;

            var il = method.Body.GetILProcessor();
            var first = method.Body.Instructions.First();

            il.InsertBefore(first, il.Create(OpCodes.Call, currentProp));             // Synchronizer.Current
            il.InsertBefore(first, il.Create(OpCodes.Ldc_I4, 123));                   // push int
            il.InsertBefore(first, il.Create(OpCodes.Ldstr, "Injected Hello"));            // push string
            il.InsertBefore(first, il.Create(OpCodes.Callvirt, syncMethod));         // call Synchronize
         }
         

         var ms = new MemoryStream();
         assembly.Write(ms);
         ms.Position = 0;
         return ms;
      }
   }
}
