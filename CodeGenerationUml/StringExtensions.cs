using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenerationUml
{
    public static class StringExtensions
    {
        public static string Capitalize(this String original)
        {
            if (String.IsNullOrEmpty(original))
                return String.Empty;

            char[] capitalized = original.ToCharArray();
            capitalized[0] = char.ToUpper(capitalized[0]);

            return new String(capitalized);
        }
    }
}
