using Sharara.EntityCodeGen.Core.Fields.Types;

namespace Sharara.EntityCodeGen.Core.Rpc
{
    record struct Argument(FieldType Type, string? Name)
    {
    }
}