
using Sharara.EntityCodeGen.Core.Fields;

namespace Sharara.EntityCodeGen.Core.Rpc
{
    interface IProcedure
    {
        string Name { get; }
        Argument[] Arguments { get; }
        FieldType ReturnType { get; }


        // TODO: These props are problematic because they aren't generic enough
        // They are tied to HOW the procedure works
        RecordEntity Record { get; }
        OperationType ProcedureType { get; }
    }

}