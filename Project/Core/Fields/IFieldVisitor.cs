using Sharara.EntityCodeGen.Core.Fields;

namespace Sharara.EntityCodeGen.Core
{
    interface IFieldVisitor
    {
        void VisitField(Field field);
        void VisitStringField(StringField field);
        void VisitInt32Field(Int32Field field);
        void VisitInt64Field(Int64Field field);
        void VisitFloat64Field(Float64Field field);
        void VisitDateTimeField(DateTimeField field);
        void VisitReferenceField(ReferenceField field);
        void VisitListField(ListField listField);
    }
}