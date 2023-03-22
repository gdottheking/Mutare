using Sharara.EntityCodeGen.Core.Fields;

namespace Sharara.EntityCodeGen.Core
{
    interface IEntityVisitor
    {
        void VisitRecord(RecordEntity entity);
        void VisitEnum(EnumEntity entity);
        void VisitEnumValue(EnumValue value, int index, int count);
    }

}