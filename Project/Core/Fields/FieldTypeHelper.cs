using Sharara.EntityCodeGen.Core.Fields.Types;

namespace Sharara.EntityCodeGen.Core.Fields
{
    static class FieldTypeHelper
    {
        // Is this primitive nullable, when output as an entity property
        // Applies to Int32, Int64, Float64
        public static bool IsMandatory(this Field field)
        {
            return (field.IsRequiredOnCreate || field.IsKey);
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

        public static bool IsEnumOrListOfEnums(this FieldType f)
        {
            if (f is FieldType.Entity fte && fte.GetEntity().EntityType == EntityType.Enum)
            {
                return true;
            }
            else if (f is FieldType.List fls)
            {
                return IsEnumOrListOfEnums(fls.ItemType);
            }

            return false;
        }

        public static bool IsRecOrListOfRecs(this FieldType f)
        {
            if (f is FieldType.Entity fte && fte.GetEntity().EntityType == EntityType.Record)
            {
                return true;
            }
            else if (f is FieldType.List fls)
            {
                return IsRecOrListOfRecs(fls.ItemType);
            }

            return false;
        }

    }
}