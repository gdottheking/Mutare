using System.Xml;
using Sharara.EntityCodeGen.Core;
using Sharara.EntityCodeGen.Core.Fields;
using Sharara.EntityCodeGen.Core.Fields.Types;

namespace Sharara.EntityCodeGen
{
    class SchemaLoader : BaseXmlLoader
    {
        private RecordLoader recordLoader = new RecordLoader();
        private EnumLoader enumLoader = new EnumLoader();

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
                throw new SchemaException("Xml document root is null");
            }

            return Load(root);
        }

        public Schema Load(XmlElement root)
        {
            var cSharpNamespace = GetOrThrowString(root, "csharp:namespace");
            var protoPackageName = GetOrThrowString(root, "pb:package");
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
                        throw new SchemaException($"Unknown element: <{element.Name}> with parent :root");
                }
            }

            try
            {
                ResolveReferences(records, enums);
            }
            catch (Exception e)
            {
                throw new SchemaException("Failed to resolve some references", e);
            }
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
                    throw new SchemaException($"Entity {nameRef.UnresolvedEntityName} not found");
                }
                var targetEntity = entityById[nameRef.UnresolvedEntityName];
                nameRef.ResolveTo(targetEntity);
            };

            foreach (var record in records)
            {
                for (int i = record.Fields.Count - 1; i > -1; i--)
                {
                    Field? field = record.Fields[i];
                    if (field.FieldType is FieldType.Entity nameRef && !nameRef.ResolvesToEntity)
                    {
                        resolve(nameRef);
                    }
                    else if (field.FieldType is FieldType.List listFieldType &&
                        listFieldType.ItemType is FieldType.Entity itemNameRef &&
                        !itemNameRef.ResolvesToEntity)
                    {
                        resolve(itemNameRef);
                        if (itemNameRef.GetEntity() is RecordEntity targetRecord)
                        {
                            targetRecord.IncomingPointers.Add((ListField)field);
                        }
                    }
                }
            }
        }

        private List<RecordEntity> ReadRecords(XmlElement entitiesListElement)
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
                        var rec = recordLoader.Load(element);
                        recordById.Add(rec);
                        break;

                    default:
                        throw new SchemaException($"Unknown element: <{element.Name}> with parent :root");
                }
            }
            return recordById;
        }

        private List<EnumEntity> ReadEnums(XmlElement el)
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
                        var enumEntity = enumLoader.Load(element);
                        enumById.Add(enumEntity);
                        break;

                    default:
                        throw new SchemaException($"Unknown element: <{element.Name}> with parent :root");
                }
            }
            return enumById;
        }

    }

}
