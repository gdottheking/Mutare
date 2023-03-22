using System.Xml;
using Sharara.EntityCodeGen.Core;
using Sharara.EntityCodeGen.Core.Fields;

namespace Sharara.EntityCodeGen
{
    class EnumLoader : BaseXmlLoader
    {
        public EnumEntity Load(XmlElement entityXmlElement)
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

        private EnumValue ReadEnumValue(XmlElement el)
        {
            var ev = new EnumValue();
            ev.Name = MustGetString(el, NameAttribute);
            ev.Value = MustGetInt(el);
            return ev;
        }
    }
}