using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Intelligent.OTC.Common.Attr
{
    public class EnumCodeAttribute : Attribute
    {
        public EnumCodeAttribute(string variableType)
        {
            VariableType = variableType;
        }
        public string VariableType { get; set; }
    }
}
