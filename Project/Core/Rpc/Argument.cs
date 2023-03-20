using Sharara.EntityCodeGen.Core.Fields;

namespace Sharara.EntityCodeGen.Core.Rpc
{
    record struct Argument(FieldType Type, string? Name)
    {
    }
}