using System.Xml;
using Sharara.EntityCodeGen.Core;

namespace Sharara.EntityCodeGen
{
    class SchemaLoader
    {
        const string NS_API = "https://codegen.sharara.com/api/v1";
        const string NS_PROTO = "https://codegen.sharara.com/protobuf/v1";
        const string NS_DB = "https://codegen.sharara.com/database/v1";
        const string NS_CSHARP = "https://codegen.sharara.com/csharp/v1";
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

            var xnsmgr = new XmlNamespaceManager(doc.NameTable);
            xnsmgr.AddNamespace("api", NS_API);
            xnsmgr.AddNamespace("pb", NS_PROTO);
            xnsmgr.AddNamespace("db", NS_DB);
            xnsmgr.AddNamespace("csharp", NS_CSHARP);

            // Get the root element
            XmlElement? root = doc.DocumentElement;
            // Loop through each child element
            if (root == null)
            {
                throw new InvalidOperationException("Xml document root is null");
            }

            var cSharpNamespace = MustGetString(root, "csharp:namespace");
            var protoPackageName = MustGetString(root, "pb:package");

            var records = new List<RecordEntity>();
            var enums = new List<EnumEntity>();
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

            ResolveReferences(records, enums);
            var schemaConfig = new SchemaConfig(cSharpNamespace, protoPackageName);
            var schema = new Schema(schemaConfig, records, enums);
            schema.Validate();
            return schema;
        }

        private void ResolveReferences(List<RecordEntity> records, List<EnumEntity> enums)
        {
            var entityById = new Dictionary<string, Entity>();
            records.ForEach(x => entityById.Add(x.Name!, x));
            enums.ForEach(x => entityById.Add(x.Name!, x));

            Action<FieldType.EntityNameRef> resolve = (nameRef) =>
            {
                if (!entityById.ContainsKey(nameRef.EntityName))
                {
                    throw new InvalidOperationException($"Entity {nameRef.EntityName} not found");
                }
                nameRef.ResolveTo(entityById[nameRef.EntityName]);
            };

            foreach (var rec in records)
            {
                foreach (var field in rec.fields)
                {
                    if (field.FieldType is FieldType.EntityNameRef nameRef1)
                    {
                        resolve(nameRef1);
                    }
                    else if (field.FieldType is FieldType.List listf &&
                        listf.ItemType is FieldType.EntityNameRef nameRef2)
                    {
                        resolve(nameRef2);
                    }
                }
            }
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
            var recName = MustGetString(entityXmlElement, "name");
            RecordEntity entity = new RecordEntity(recName);

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
                        throw new InvalidDataException($"Unknown element: <{el.Name}> contained under <{RecordElementName}>");
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
            var enumName = MustGetString(entityXmlElement, "name");
            EnumEntity entity = new EnumEntity(enumName);

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
                        throw new InvalidDataException($"Unknown element: <{el.Name}> contained under <{EnumElementName}>");
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

                    case Int32Field.XmlTypeName:
                        field = ReadInt32Field(el);
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

                    case ListField.XmlTypeName:
                        field = ReadListField(el);
                        break;

                    default:
                        throw new InvalidDataException($"Unknown element: <{el.Name}> contained under <{FieldsElementName}>");
                }
                fields.Add(field);
            }
            return fields;
        }

        private ListField ReadListField(XmlElement el)
        {
            return (ListField)CreateField(el);
        }

        private Float64Field ReadFloat64Field(XmlElement el)
        {
            return (Float64Field)CreateField(el);
        }

        private ReferenceField ReadRefField(XmlElement el)
        {
            return (ReferenceField)CreateField(el);
        }

        Int64Field ReadInt64Field(XmlElement el)
        {
            return (Int64Field)CreateField(el);
        }

        Int32Field ReadInt32Field(XmlElement el)
        {
            return (Int32Field)CreateField(el);
        }

        StringField ReadStringField(XmlElement el)
        {
            return (StringField)CreateField(el);
        }

        DateTimeField ReadDateTimeField(XmlElement el)
        {
            return (DateTimeField)CreateField(el);
        }

        EnumValue ReadEnumValue(XmlElement el)
        {
            var ev = new EnumValue();
            ev.Name = MustGetString(el, "name");
            ev.Value = MustGetInt(el);
            return ev;
        }

        FieldType GetListItemType(XmlElement el)
        {
            string itemTypeName = MustGetString(el, "of");
            return itemTypeName switch
            {
                DateTimeField.XmlTypeName => FieldType.DateTime.Instance,
                Float64Field.XmlTypeName => FieldType.Float64.Instance,
                Int32Field.XmlTypeName => FieldType.Int32.Instance,
                Int64Field.XmlTypeName => FieldType.Int64.Instance,
                StringField.XmlTypeName => FieldType.String.Instance,
                _ => new FieldType.EntityNameRef(itemTypeName)
            };
        }

        ListField CreateListField(XmlElement el, string fieldName)
        {
            var itemType = GetListItemType(el);
            return new ListField(itemType, fieldName);
        }

        Field CreateField(XmlElement el)
        {
            var fieldName = MustGetString(el, "name");
            var fieldType = el.Name;

            Field field = fieldType switch
            {
                DateTimeField.XmlTypeName => new DateTimeField(fieldName),
                Float64Field.XmlTypeName => new Float64Field(fieldName),
                Int32Field.XmlTypeName => new Int32Field(fieldName),
                Int64Field.XmlTypeName => new Int64Field(fieldName),
                ReferenceField.XmlTypeName => new ReferenceField(new FieldType.EntityNameRef(MustGetString(el, "entity")), fieldName),
                ListField.XmlTypeName => CreateListField(el, fieldName),
                StringField.XmlTypeName => new StringField(fieldName),
                _ => throw new NotImplementedException(fieldType + " Unknown")
            };

            ReadCommonFieldAttributes(field, el);

            return field;
        }

        void ReadCommonFieldAttributes(Field field, XmlElement el)
        {
            field.ProtoId = MustGetInt(el, "pb:id");
            field.Required = GetBool(el, "required");
            field.IsKey = GetBool(el, "key");
            field.CheckOnUpdate = GetBool(el, "checkOnUpdate");
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

        int MustGetInt(XmlElement el, string attribName)
        {
            ArgumentNullException.ThrowIfNull(el);
            ArgumentNullException.ThrowIfNull(attribName);
            string? value = el.Attributes?[attribName]?.Value;
            ArgumentException.ThrowIfNullOrEmpty(value);
            return int.Parse(value);
        }

    }

}
