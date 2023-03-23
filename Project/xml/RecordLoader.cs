using System.Xml;
using Sharara.EntityCodeGen.Core;
using Sharara.EntityCodeGen.Core.Fields;
using Sharara.EntityCodeGen.Core.Fields.Types;

namespace Sharara.EntityCodeGen
{
    class RecordLoader : BaseXmlLoader
    {
        public RecordEntity Load(XmlElement entityXmlElement)
        {
            var recName = GetOrThrowString(entityXmlElement, NameAttribute);
            var recPluralName = GetOrThrowString(entityXmlElement, PluralAttribute);
            RecordEntity record = new RecordEntity(recName, recPluralName);

            foreach (XmlNode child in entityXmlElement.ChildNodes)
            {
                if (child.NodeType != XmlNodeType.Element)
                {
                    continue;
                }

                var el = (XmlElement)child;
                switch (el.Name)
                {
                    case FieldsElementName:
                        record.Fields.AddRange(ReadFields(record, el));
                        break;

                    default:
                        throw new InvalidDataException($"Unknown element: <{el.Name}> contained under <{RecordElementName}>");
                }
            }

            return record;
        }

        private List<Field> ReadFields(RecordEntity record, XmlElement fieldsListXmlElement)
        {
            var fields = new List<Field>();
            foreach (XmlNode child in fieldsListXmlElement.ChildNodes)
            {
                if (child.NodeType != XmlNodeType.Element)
                {
                    continue;
                }

                var el = (XmlElement)child;
                Field field;
                switch (el.Name)
                {
                    case Int64Field.XmlTypeName:
                        field = ReadInt64Field(record, el);
                        break;

                    case Int32Field.XmlTypeName:
                        field = ReadInt32Field(record, el);
                        break;

                    case DateTimeField.XmlTypeName:
                        field = ReadDateTimeField(record, el);
                        break;

                    case Float64Field.XmlTypeName:
                        field = ReadFloat64Field(record, el);
                        break;

                    case StringField.XmlTypeName:
                        field = ReadStringField(record, el);
                        break;

                    case ReferenceField.XmlTypeName:
                        field = ReadRefField(record, el);
                        break;

                    case ListField.XmlTypeName:
                        field = ReadListField(record, el);
                        break;

                    default:
                        throw new InvalidDataException($"Unknown element: <{el.Name}> contained under <{FieldsElementName}>");
                }
                fields.Add(field);
            }
            return fields;
        }

        private void ReadCommonFieldAttributes(Field field, XmlElement el)
        {
            field.ProtoId = GetOrThrowInt32(el, ProtoIdAttribute);
            field.IsRequiredOnCreate = GetBool(el, RequiredAttribute);
            field.IsKey = GetBool(el, KeyAttribute);
            field.CheckOnUpdate = GetBool(el, CheckOnUpdateAttribute);
        }

        private ListField ReadListField(RecordEntity record, XmlElement el)
        {
            return (ListField)CreateField(record, el);
        }

        private Float64Field ReadFloat64Field(RecordEntity record, XmlElement el)
        {
            var field = (Float64Field)CreateField(record, el);
            GetOptionalFloat64(el, MinValueAttribute, x => field.MinValue = x);
            GetOptionalFloat64(el, MaxValueAttribute, x => field.MaxValue = x);
            return field;
        }

        private ReferenceField ReadRefField(RecordEntity record, XmlElement el)
        {
            return (ReferenceField)CreateField(record, el);
        }

        private Int64Field ReadInt64Field(RecordEntity record, XmlElement el)
        {
            var field = (Int64Field)CreateField(record, el);
            GetOptionalInt64(el, MinValueAttribute, x => field.MinValue = x);
            GetOptionalInt64(el, MaxValueAttribute, x => field.MaxValue = x);
            return field;
        }

        private Int32Field ReadInt32Field(RecordEntity record, XmlElement el)
        {
            var field = (Int32Field)CreateField(record, el);
            GetOptionalInt32(el, MinValueAttribute, x => field.MinValue = x);
            GetOptionalInt32(el, MaxValueAttribute, x => field.MaxValue = x);
            return field;
        }

        private StringField ReadStringField(RecordEntity record, XmlElement el)
        {
            var field = (StringField)CreateField(record, el);
            GetOptionalInt32(el, MinLengthAttribute, x => field.MinLength = x);
            GetOptionalInt32(el, MaxLengthAttribute, x => field.MaxLength = x);
            GetOptionalString(el, RegexAttribute, x => field.RegexPattern = x);
            GetOptionalString(el, TransformAttribute, strTransAttr =>
            {
                StringTransform transform = StringTransform.None;
                if (!string.IsNullOrWhiteSpace(strTransAttr))
                {
                    var splitBy = new string[] { ",", "|" };
                    var parts = strTransAttr.Split(splitBy,
                        StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

                    foreach (var str in parts)
                    {
                        if (Enum.TryParse<StringTransform>(str, true, out StringTransform temp))
                        {
                            transform |= temp;
                        }
                        else
                        {
                            throw new ArgumentException($"Invalid Transform '{str}' on field {field.Name}");
                        }
                    }
                }
                field.Transforms = transform;
            });
            return field;
        }

        private DateTimeField ReadDateTimeField(RecordEntity record, XmlElement el)
        {
            return (DateTimeField)CreateField(record, el);
        }

        private FieldType.Entity GetListItemType(XmlElement el)
        {
            string name = GetOrThrowString(el, NameAttribute);
            string itemTypeName = GetOrThrowString(el, RecordAttribute);
            return itemTypeName switch
            {
                DateTimeField.XmlTypeName or
                Float64Field.XmlTypeName or
                Int32Field.XmlTypeName or
                Int64Field.XmlTypeName or
                StringField.XmlTypeName => throw new InvalidOperationException($"List '{name}' should reference a record"),
                _ => new FieldType.Entity(itemTypeName)
            };
        }

        private ListField CreateListField(RecordEntity record, XmlElement el, string fieldName)
        {
            var itemType = GetListItemType(el);
            return new ListField(record, itemType, fieldName);
        }

        private Field CreateField(RecordEntity record, XmlElement el)
        {
            var fieldName = GetOrThrowString(el, NameAttribute);
            var fieldType = el.Name;

            Field field = fieldType switch
            {
                DateTimeField.XmlTypeName => new DateTimeField(record, fieldName),
                Float64Field.XmlTypeName => new Float64Field(record, fieldName),
                Int32Field.XmlTypeName => new Int32Field(record, fieldName),
                Int64Field.XmlTypeName => new Int64Field(record, fieldName),
                ReferenceField.XmlTypeName => new ReferenceField(record, new FieldType.Entity(GetOrThrowString(el, EntityAttribute)), fieldName),
                ListField.XmlTypeName => CreateListField(record, el, fieldName),
                StringField.XmlTypeName => new StringField(record, fieldName),
                _ => throw new NotImplementedException(fieldType + " Unknown")
            };

            ReadCommonFieldAttributes(field, el);

            return field;
        }
    }
}