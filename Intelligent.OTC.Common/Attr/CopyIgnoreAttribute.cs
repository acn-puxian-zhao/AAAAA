using System;
using System.Collections.Generic;
using System.Text;

namespace Intelligent.OTC.Common.Attr
{
    public class CopyIgnoreAttribute : Attribute
    {
        private object ignoreCaseValue;

        public object IgnoreCaseValue
        {
            get { return ignoreCaseValue; }
            set { ignoreCaseValue = value; }
        }

        private string ignoreDescription;

        public string IgnoreDescription
        {
            get { return ignoreDescription; }
            set { ignoreDescription = value; }
        }

        public Type PropType { get; set; }

        public bool IsIgnore(object propertyValue)
        {
            if (propertyValue == null)
            {
                //ignore anyway
                return true;
            }
            //ignore depend on the given case
            //TODO: ignore case only support simple .NET simple type
            return ignoreCaseValue.ToString().Equals(propertyValue.ToString());
        }
    }
}
