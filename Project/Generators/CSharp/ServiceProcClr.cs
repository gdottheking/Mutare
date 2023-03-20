using Sharara.EntityCodeGen.Core.Rpc;

namespace Sharara.EntityCodeGen.Generators.CSharp
{
    record ServiceProcClr(string MethodName, string RequestTypeName, string ResponseTypeName)
    {
        public ServiceProcClr(IProcedure proc)
            : this($"{proc.Name}Async",
                $"{proc.Name}Request",
                $"{proc.Name}Response"
        )
        {
        }
    }
}
