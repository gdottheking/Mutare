using System.Xml;
using Sharara.EntityCodeGen.Core;

namespace Sharara.EntityCodeGen
{
    class SchemaLoader
    {
        public const string RecordsElementName = "records";
        public const string RecordElementName = "record";
        public const string EnumsElementName = "enums";
        public const string EnumElementName = "enum";
        public const string FieldsElementName = "fields";

        public Schema ReadDocument(string path)
        {
            // Load the XML file
            XmlDocument doc = new XmlDocument();
            doc.Load(path);

            // Get the root element
            XmlElement root = doc.DocumentElement;

            List<RecordEntity> records = new List<RecordEntity>();
            List<EnumEntity> enums = new List<EnumEntity>();

            // Loop through each child element
            if (root == null)
            {
                throw new InvalidOperationException("Xml document root is null");
            }

            foreach (XmlNode node in root.ChildNodes)
            {
                if (node.NodeType != XmlNodeType.Element)
                {
                    continue;
                }

                var element = (XmlElement)node;
                switch (element.Name)
                {
                    case RecordsElementName:
                        records = ReadRecords(element);
                        break;

                    case EnumsElementName:
                        enums = ReadEnums(element);
                        break;

                    default:
                        throw new InvalidDataException($"Unknown element: <{element.Name}> with parent :root");
                }
            }

             return new Schema(records, enums);
        }

        List<RecordEntity> ReadRecords(XmlElement entitiesListElement)
        {
            var recordById = new List<RecordEntity>();
            foreach (XmlNode node in entitiesListElement.ChildNodes)
            {
                if (node.NodeType != XmlNodeType.Element)
                {
                    continue;
                }

                var element = (XmlElement)node;
                switch (element.Name)
                {
                    case RecordElementName:
                        var rec = ReadRecord(element);
                        recordById.Add(rec);
                        break;

                    default:
                        throw new InvalidDataException($"Unknown element: <{element.Name}> with parent :root");
                }
            }
            return recordById;
        }

        RecordEntity ReadRecord(XmlElement entityXmlElement)
        {
            RecordEntity entity = new RecordEntity();
            entity.Name = MustGetString(entityXmlElement, "name");

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
                        entity.fields.AddRange(ReadFields(el));
                        break;

                    default:
                        throw new InvalidDataException($"Unknown element: {el.Name} contained under <{RecordElementName}>");
                }
            }

            return entity;
        }

        List<EnumEntity> ReadEnums(XmlElement el)
        {
            var enumById = new List<EnumEntity>();
            foreach (XmlNode node in el.ChildNodes)
            {
                if (node.NodeType != XmlNodeType.Element)
                {
                    continue;
                }

                var element = (XmlElement)node;
                switch (element.Name)
                {
                    case EnumElementName:
                        var enumEntity = ReadEnum(element);
                        enumById.Add(enumEntity);
                        break;

                    default:
                        throw new InvalidDataException($"Unknown element: <{element.Name}> with parent :root");
                }
            }
            return enumById;
        }

        EnumEntity ReadEnum(XmlElement entityXmlElement)
        {
            EnumEntity entity = new EnumEntity();
            entity.Name = MustGetString(entityXmlElement, "name");

            foreach (XmlNode child in entityXmlElement.ChildNodes)
            {
                if (child.NodeType != XmlNodeType.Element)
                {
                    continue;
                }

                var el = (XmlElement)child;
                switch (el.Name)
                {
                    case EnumValue.ElementName:
                        entity.Values.Add(ReadEnumValue(el));
                        break;

                    default:
                        throw new InvalidDataException($"Unknown element: {el.Name} contained under <{EnumElementName}>");
                }
            }

            return entity;
        }

        List<Field> ReadFields(XmlElement fieldsListXmlElement)
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
                        field = ReadInt64Field(el);
                        break;

                    case DateTimeField.XmlTypeName:
                        field = ReadDateTimeField(el);
                        break;

                    case Float64Field.XmlTypeName:
                        field = ReadFloat64Field(el);
                        break;

                    case StringField.XmlTypeName:
                        field = ReadStringField(el);
                        break;

                    case ReferenceField.XmlTypeName:
                        field = ReadRefField(el);
                        break;

                    default:
                        throw new InvalidDataException($"Unknown element: {el.Name} contained under <fields>");
                }
                fields.Add(field);
            }
            return fields;
        }

        private Float64Field ReadFloat64Field(XmlElement el)
        {
            var field = new Float64Field();
            ReadCoreFieldAttributes(field, el);
            return field;
        }

        private ReferenceField ReadRefField(XmlElement el)
        {
            var field = new ReferenceField();
            ReadCoreFieldAttributes(field, el);
            field.EntityName = MustGetString(el, "entity");

            return field;
        }

        Int64Field ReadInt64Field(XmlElement el)
        {
            var field = new Int64Field();
            ReadCoreFieldAttributes(field, el);
            return field;
        }

        StringField ReadStringField(XmlElement el)
        {
            var field = new StringField();
            ReadCoreFieldAttributes(field, el);
            return field;
        }

        DateTimeField ReadDateTimeField(XmlElement el)
        {
            var field = new DateTimeField();
            ReadCoreFieldAttributes(field, el);
            return field;
        }

        EnumValue ReadEnumValue(XmlElement el)
        {
            var ev = new EnumValue();
            ev.Name = MustGetString(el, "name");
            ev.Value = MustGetInt(el);
            return ev;
        }

        void ReadCoreFieldAttributes(Field field, XmlElement fieldXmlElement)
        {
            field.Name = MustGetString(fieldXmlElement, "name");
            field.Required = GetBool(fieldXmlElement, "required");
            field.IsKey = GetBool(fieldXmlElement, "key");
            field.CheckOnUpdate = GetBool(fieldXmlElement, "checkOnUpdate");
        }

        string MustGetString(XmlElement el, string attribName)
        {
            ArgumentNullException.ThrowIfNull(el);
            ArgumentNullException.ThrowIfNull(attribName);
            string? value = el.Attributes?[attribName]?.Value;
            ArgumentException.ThrowIfNullOrEmpty(value);
            return value;
        }

        bool GetBool(XmlElement el, string attribName)
        {
            ArgumentNullException.ThrowIfNull(el);
            ArgumentNullException.ThrowIfNull(attribName);
            string? value = el.Attributes?[attribName]?.Value;

            bool.TryParse(value, out bool valIsTrue);
            return valIsTrue;
        }

        int MustGetInt(XmlElement el)
        {
            ArgumentNullException.ThrowIfNull(el);
            ArgumentException.ThrowIfNullOrEmpty(el.InnerText);
            return int.Parse(el.InnerText);
        }

    }

}
