using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JSonParser.SDK.Utils
{
    public static class StringHelper
    {
        public static string FirstLetterUpper(this string source)
        {
            if (string.IsNullOrEmpty(source))
                throw new ArgumentNullException("source");

            return source.First().ToString().ToUpper() + source.Substring(1);
        }

        public static string FirstLetterLower(this string source)
        {
            if (string.IsNullOrEmpty(source))
                throw new ArgumentNullException("source");

            return source.First().ToString().ToLower() + source.Substring(1);
        }
    }
}
