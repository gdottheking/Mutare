namespace Sharara.EntityCodeGen.Core.Fields
{
    static class FieldTypeHelper
    {
        // Is this primitive nullable, when output as an entity property
        // Applies to Int32, Int64, Float64
        public static bool IsMandatory(this Field field)
        {
            return (field.IsRequired || field.IsKey);
        }

        public static bool IsNumericPrimitive(this FieldType fieldType)
        {
            return fieldType is FieldType.Int32 ||
                    fieldType is FieldType.Int64 ||
                    fieldType is FieldType.Float64;
        }

        public static bool IsSimpleAssignable(this FieldType fieldType)
        {
            return fieldType is FieldType.String ||
                    fieldType is FieldType.Int64 ||
                    fieldType is FieldType.Int32 ||
                    fieldType is FieldType.Float64;
        }

    }
}