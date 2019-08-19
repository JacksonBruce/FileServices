using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
namespace System.Helper
{
    public static class PatulousMethods
    {
        public static bool IsNumber(this string s)
        {
            return Regex.IsMatch(s, @"\d+");
        }
    }
}
