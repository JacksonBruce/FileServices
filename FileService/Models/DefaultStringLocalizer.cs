using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Ufangx.FileServices.Models
{
    internal class DefaultStringLocalizer : IStringLocalizer
    {
        LocalizedString IStringLocalizer.this[string name] => new LocalizedString(name, name,false);

        LocalizedString IStringLocalizer.this[string name, params object[] arguments] =>new LocalizedString(name, string.Format(name, arguments),false) ;

        IEnumerable<LocalizedString> IStringLocalizer.GetAllStrings(bool includeParentCultures)
        {
            throw new NotImplementedException();
        }

        IStringLocalizer IStringLocalizer.WithCulture(CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
