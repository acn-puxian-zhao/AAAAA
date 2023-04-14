using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Intelligent.OTC.Business
{
    public interface ITemplateParser
    {
        void RegistContext(string objkey, object contextObj);

        object GetContext(string objkey);
        void ParseTemplate(string template, out string templateInstance);
    }
}
