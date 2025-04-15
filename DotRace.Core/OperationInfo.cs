using System.Reflection;

namespace DotRace.Core
{
    public class OperationInfo
    {

      public required object Instance { get; init; }
      
      public required MethodInfo Method { get; init; }

    }
}
