using DotRace.Common;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Linq;

namespace DotRace.Core
{
    internal class Injector
    {
      internal static MemoryStream Inject(AssemblyDefinition assembly, IEnumerable<MethodDefinition> methodDefinitions)
      { 
         var module = assembly.MainModule;
         var monitorType = module.ImportReference(typeof(ThreadMonitor));

         var types = methodDefinitions.Select(methodDefinition => methodDefinition.DeclaringType).Distinct();
         Dictionary<TypeDefinition, FieldDefinition> fields = new Dictionary<TypeDefinition, FieldDefinition>();

         foreach(var type in types)
         {
            // Add: private ThreadMonitor _monitor;
            var field = new FieldDefinition("_monitor", FieldAttributes.Private, monitorType);
            type.Fields.Add(field);
            fields.Add(type, field);

            // Add: public void SetMonitor(ThreadMonitor monitor)
            var setMethod = new MethodDefinition("SetMonitor", MethodAttributes.Public, module.TypeSystem.Void);
            var param = new ParameterDefinition("monitor", ParameterAttributes.None, monitorType);
            setMethod.Parameters.Add(param);

            var ilSet = setMethod.Body.GetILProcessor();
            ilSet.Append(ilSet.Create(OpCodes.Ldarg_0)); // this
            ilSet.Append(ilSet.Create(OpCodes.Ldarg_1)); // monitor
            ilSet.Append(ilSet.Create(OpCodes.Stfld, field));
            ilSet.Append(ilSet.Create(OpCodes.Ret));
            type.Methods.Add(setMethod);
         }

         foreach (var method in methodDefinitions)
         {
            // Inject: this._monitor.WaitForContinue();
            var waitMethod = typeof(ThreadMonitor).GetMethod("WaitForContinue")!;
            var waitMethodRef = module.ImportReference(waitMethod);

            var il = method.Body.GetILProcessor();
            var first = method.Body.Instructions.First();
            var field = fields[method.DeclaringType];

            il.InsertBefore(first, il.Create(OpCodes.Ldarg_0));         // this
            il.InsertBefore(first, il.Create(OpCodes.Ldfld, field));   // this._monitor
            il.InsertBefore(first, il.Create(OpCodes.Callvirt, waitMethodRef)); // call WaitForContinue()

         }

         var ms = new MemoryStream();
         assembly.Write(ms);
         ms.Position = 0;
         return ms;
      }
   }
}
