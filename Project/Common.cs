using System.Text.RegularExpressions;

namespace Sharara.EntityCodeGen
{

    static class Common
    {
        public const string GrpcServiceName = "Service";
        public const string ProtocOutputNamespace = "Sharara.Services.Kumusha.Generated.Proto";
        public const string DateTimeFormatString = "yyyyMMddHHmmsss.zzz";
        public static readonly Regex PascalCaseRegex = new Regex("^[A-Z]([a-z0-9])+(?:[A-Za-z0-9])*$");
        public static readonly Regex PascalCaseWordRegex = new Regex("[A-Z][a-z0-9]*");


        static void AssertIsPascalCase(this string name)
        {
            if (!Common.PascalCaseRegex.IsMatch(name))
            {
                throw new InvalidOperationException($"Name '{name}' must be in PascalCase");
            }
        }

        public static string ToGrpcNamingConv(this string name)
        {
            name.AssertIsPascalCase();
            var matches = PascalCaseWordRegex.Matches(name);
            return string.Join("_",  matches.Select(w => w.ToString().ToLower()));
        }

        public static string ToCamelCase(this string name)
        {
            name.AssertIsPascalCase();
            var matches = PascalCaseWordRegex.Matches(name);
            var words = matches.Select(w => w.ToString()).ToList();
            words[0] = words[0].ToLower();
            return string.Join("", words);
        }

    }

}