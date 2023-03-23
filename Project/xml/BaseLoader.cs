using System.Xml;

namespace Sharara.EntityCodeGen
{
    abstract class BaseXmlLoader
    {
        public const string NS_API = "https://codegen.sharara.com/api/v1";
        public const string NS_DB = "https://codegen.sharara.com/database/v1";
        public const string NS_CSHARP = "https://codegen.sharara.com/csharp/v1";
        public const string RecordsElementName = "records";
        public const string RecordElementName = "record";
        public const string EnumsElementName = "enums";
        public const string EnumElementName = "enum";
        public const string FieldsElementName = "fields";
        public const string DefaultAttribute = "default";
        public const string MinValueAttribute = "minValue";
        public const string MaxValueAttribute = "maxValue";
        public const string MinLengthAttribute = "minLength";
        public const string MaxLengthAttribute = "maxLength";
        public const string RegexAttribute = "regex";
        public const string TransformAttribute = "transform";
        public const string NameAttribute = "name";
        public const string RecordAttribute = "record";
        public const string PluralAttribute = "plural";
        public const string RequiredAttribute = "required";
        public const string KeyAttribute = "key";
        public const string EntityAttribute = "entity";
        public const string CheckOnUpdateAttribute = "checkOnUpdate";
        public const string NS_PROTO = "https://codegen.sharara.com/protobuf/v1";
        public const string ProtoIdAttribute = "pb:id";


        protected string GetOrThrowString(XmlElement el, string attribName)
        {
            ArgumentNullException.ThrowIfNull(el);
            ArgumentNullException.ThrowIfNull(attribName);
            string? value = el.Attributes?[attribName]?.Value;
            ArgumentException.ThrowIfNullOrEmpty(value, attribName);
            return value;
        }

        protected bool GetBool(XmlElement el, string attribName)
        {
            ArgumentNullException.ThrowIfNull(el);
            ArgumentNullException.ThrowIfNull(attribName);
            string? value = el.Attributes?[attribName]?.Value;

            bool.TryParse(value, out bool valIsTrue);
            return valIsTrue;
        }

        protected int GetOrThrowInt32(XmlElement el)
        {
            ArgumentNullException.ThrowIfNull(el);
            ArgumentException.ThrowIfNullOrEmpty(el.InnerText);
            return int.Parse(el.InnerText);
        }

        protected int GetOrThrowInt32(XmlElement el, string attribName)
        {
            ArgumentNullException.ThrowIfNull(el);
            ArgumentNullException.ThrowIfNull(attribName);
            string? value = el.Attributes?[attribName]?.Value;
            ArgumentException.ThrowIfNullOrEmpty(value, attribName);
            return int.Parse(value);
        }

        protected void GetOptionalInt32(XmlElement el, string attribName, Action<int> callback)
        {
            ArgumentNullException.ThrowIfNull(el);
            ArgumentNullException.ThrowIfNull(attribName);
            string? attribValue = el.Attributes?[attribName]?.Value;
            if (int.TryParse(attribValue, out int parsedValue))
            {
                callback(parsedValue);
            }
        }

        protected void GetOptionalInt64(XmlElement el, string attribName, Action<long> callback)
        {
            ArgumentNullException.ThrowIfNull(el);
            ArgumentNullException.ThrowIfNull(attribName);
            string? attribValue = el.Attributes?[attribName]?.Value;
            if (long.TryParse(attribValue, out long parsedValue))
            {
                callback(parsedValue);
            }
        }

        protected void GetOptionalFloat64(XmlElement el, string attribName, Action<double> callback)
        {
            ArgumentNullException.ThrowIfNull(el);
            ArgumentNullException.ThrowIfNull(attribName);
            string? attribValue = el.Attributes?[attribName]?.Value;
            if (double.TryParse(attribValue, out double parsedValue))
            {
                callback(parsedValue);
            }
        }

        protected void GetOptionalString(XmlElement el, string attribName, Action<string> callback)
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