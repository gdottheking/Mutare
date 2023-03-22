﻿using System.Xml;
using Sharara.EntityCodeGen.Core;
using Sharara.EntityCodeGen.Core.Fields;

namespace Sharara.EntityCodeGen
{
    class SchemaLoader
    {
        const string NS_API = "https://codegen.sharara.com/api/v1";
        const string NS_DB = "https://codegen.sharara.com/database/v1";
        const string NS_CSHARP = "https://codegen.sharara.com/csharp/v1";
        public const string RecordsElementName = "records";
        public const string RecordElementName = "record";
        public const string EnumsElementName = "enums";
        public const string EnumElementName = "enum";
        public const string FieldsElementName = "fields";
        private const string DefaultAttribute = "default";
        private const string MinValueAttribute = "minValue";
        private const string MaxValueAttribute = "maxValue";
        private const string MinLengthAttribute = "minLength";
        private const string MaxLengthAttribute = "maxLength";
        private const string RegexAttribute = "regex";
        private const string TransformAttribute = "transform";
        private const string NameAttribute = "name";
        private const string OfAttribute = "of";
        private const string PluralAttribute = "plural";
        private const string RequiredAttribute = "required";
        private const string KeyAttribute = "key";
        private const string EntityAttribute = "entity";
        private const string CheckOnUpdateAttribute = "checkOnUpdate";

        const string NS_PROTO = "https://codegen.sharara.com/protobuf/v1";
        private const string ProtoIdAttribute = "pb:id";


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

            Action<FieldType.Entity> resolve = (nameRef) =>
            {
                ArgumentNullException.ThrowIfNull(nameRef.UnresolvedEntityName);
                if (!entityById.ContainsKey(nameRef.UnresolvedEntityName))
                {
                    throw new InvalidOperationException($"Entity {nameRef.UnresolvedEntityName} not found");
                }
                nameRef.ResolveTo(entityById[nameRef.UnresolvedEntityName]);
            };

            foreach (var rec in records)
            {
                foreach (var field in rec.Fields)
                {
                    if (field.FieldType is FieldType.Entity nameRef1 &&
                        !nameRef1.HasEntity)
                    {
                        resolve(nameRef1);
                    }
                    else if (field.FieldType is FieldType.List listf &&
                        listf.ItemType is FieldType.Entity nameRef2 &&
                        !nameRef2.HasEntity)
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
            var recName = MustGetString(entityXmlElement, NameAttribute);
            var recPluralName = MustGetString(entityXmlElement, PluralAttribute);
            RecordEntity entity = new RecordEntity(recName, recPluralName);

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
                        entity.Fields.AddRange(ReadFields(el));
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
            var enumName = MustGetString(entityXmlElement, NameAttribute);
            var pluralName = MustGetString(entityXmlElement, PluralAttribute);
            EnumEntity entity = new EnumEntity(enumName, pluralName);

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
            var field = (Float64Field)CreateField(el);
            OptGetFloat64(el, MinValueAttribute, x => field.MinValue = x);
            OptGetFloat64(el, MaxValueAttribute, x => field.MaxValue = x);
            return field;
        }

        private ReferenceField ReadRefField(XmlElement el)
        {
            return (ReferenceField)CreateField(el);
        }

        Int64Field ReadInt64Field(XmlElement el)
        {
            var field = (Int64Field)CreateField(el);
            OptGetInt64(el, MinValueAttribute, x => field.MinValue = x);
            OptGetInt64(el, MaxValueAttribute, x => field.MaxValue = x);
            return field;
        }

        Int32Field ReadInt32Field(XmlElement el)
        {
            var field = (Int32Field)CreateField(el);
            OptGetInt32(el, MinValueAttribute, x => field.MinValue = x);
            OptGetInt32(el, MaxValueAttribute, x => field.MaxValue = x);
            return field;
        }

        StringField ReadStringField(XmlElement el)
        {
            var field = (StringField)CreateField(el);
            OptGetInt32(el, MinLengthAttribute, x => field.MinLength = x);
            OptGetInt32(el, MaxLengthAttribute, x => field.MaxLength = x);
            OptGetString(el, RegexAttribute, x => field.RegexPattern = x);
            OptGetString(el, TransformAttribute, strTransAttr =>
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

        DateTimeField ReadDateTimeField(XmlElement el)
        {
            return (DateTimeField)CreateField(el);
        }

        EnumValue ReadEnumValue(XmlElement el)
        {
            var ev = new EnumValue();
            ev.Name = MustGetString(el, NameAttribute);
            ev.Value = MustGetInt(el);
            return ev;
        }

        FieldType GetListItemType(XmlElement el)
        {
            string itemTypeName = MustGetString(el, OfAttribute);
            return itemTypeName switch
            {
                DateTimeField.XmlTypeName => FieldType.DateTime.Instance,
                Float64Field.XmlTypeName => FieldType.Float64.Instance,
                Int32Field.XmlTypeName => FieldType.Int32.Instance,
                Int64Field.XmlTypeName => FieldType.Int64.Instance,
                StringField.XmlTypeName => FieldType.String.Instance,
                _ => new FieldType.Entity(itemTypeName)
            };
        }

        ListField CreateListField(XmlElement el, string fieldName)
        {
            var itemType = GetListItemType(el);
            return new ListField(itemType, fieldName);
        }

        Field CreateField(XmlElement el)
        {
            var fieldName = MustGetString(el, NameAttribute);
            var fieldType = el.Name;

            Field field = fieldType switch
            {
                DateTimeField.XmlTypeName => new DateTimeField(fieldName),
                Float64Field.XmlTypeName => new Float64Field(fieldName),
                Int32Field.XmlTypeName => new Int32Field(fieldName),
                Int64Field.XmlTypeName => new Int64Field(fieldName),
                ReferenceField.XmlTypeName => new ReferenceField(new FieldType.Entity(MustGetString(el, EntityAttribute)), fieldName),
                ListField.XmlTypeName => CreateListField(el, fieldName),
                StringField.XmlTypeName => new StringField(fieldName),
                _ => throw new NotImplementedException(fieldType + " Unknown")
            };

            ReadCommonFieldAttributes(field, el);

            return field;
        }

        void ReadCommonFieldAttributes(Field field, XmlElement el)
        {
            field.ProtoId = MustGetInt(el, ProtoIdAttribute);
            field.IsRequired = GetBool(el, RequiredAttribute);
            field.IsKey = GetBool(el, KeyAttribute);
            field.CheckOnUpdate = GetBool(el, CheckOnUpdateAttribute);
        }

        string MustGetString(XmlElement el, string attribName)
        {
            ArgumentNullException.ThrowIfNull(el);
            ArgumentNullException.ThrowIfNull(attribName);
            string? value = el.Attributes?[attribName]?.Value;
            ArgumentException.ThrowIfNullOrEmpty(value, attribName);
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
            ArgumentException.ThrowIfNullOrEmpty(value, attribName);
            return int.Parse(value);
        }

        void OptGetInt32(XmlElement el, string attribName, Action<int> callback)
        {
            ArgumentNullException.ThrowIfNull(el);
            ArgumentNullException.ThrowIfNull(attribName);
            string? attribValue = el.Attributes?[attribName]?.Value;
            if (int.TryParse(attribValue, out int parsedValue))
            {
                callback(parsedValue);
            }
        }

        void OptGetInt64(XmlElement el, string attribName, Action<long> callback)
        {
            ArgumentNullException.ThrowIfNull(el);
            ArgumentNullException.ThrowIfNull(attribName);
            string? attribValue = el.Attributes?[attribName]?.Value;
            if (long.TryParse(attribValue, out long parsedValue))
            {
                callback(parsedValue);
            }
        }

        void OptGetFloat64(XmlElement el, string attribName, Action<double> callback)
        {
            ArgumentNullException.ThrowIfNull(el);
            ArgumentNullException.ThrowIfNull(attribName);
            string? attribValue = el.Attributes?[attribName]?.Value;
            if (double.TryParse(attribValue, out double parsedValue))
            {
                callback(parsedValue);
            }
        }

        void OptGetString(XmlElement el, string attribName, Action<string> callback)
        {
            ArgumentNullException.ThrowIfNull(el);
            ArgumentNullException.ThrowIfNull(attribName);
            string? attribValue = el.Attributes?[attribName]?.Value;
            if (attribValue != null)
            {
                callback(attribValue);
            }
        }

    }

}
