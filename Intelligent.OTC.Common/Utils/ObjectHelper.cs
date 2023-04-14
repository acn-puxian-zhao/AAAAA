using Intelligent.OTC.Common.Attr;
using System;
using System.Reflection;

namespace Intelligent.OTC.Common.Utils
{
    public class ObjectHelper
    {
        public static void CopyObject(Object src, Object dest)
        {
            CopyObject(src, dest, null);
        }

        public delegate bool IfCopyDelegate(string propName);

        public static void CopyObject(Object src, Object dest, IfCopyDelegate ifCopyHandler)
        {
            try
            {
                if (src == null)
                {
                    Exception ex = new ArgumentNullException("src", "source object is null");
                    Helper.Log.Error(ex.Message, ex);
                    throw ex;
                }

                if (dest == null)
                {
                    Exception ex = new ArgumentNullException("dest", "destination object is null");
                    Helper.Log.Error(ex.Message, ex);
                    throw ex;
                }

                PropertyInfo[] ps = src.GetType().GetProperties();
                foreach (PropertyInfo p in ps)
                {
                    //check if copy
                    if (ifCopyHandler != null && !ifCopyHandler(p.Name)) continue;

                    object srcValue = p.GetValue(src, null);
                    //check copy ignore property in src object
                    bool ignore = false;
                    foreach(CopyIgnoreAttribute ci in p.GetCustomAttributes(typeof(CopyIgnoreAttribute),false))
                    {
                        if (ci.IsIgnore(srcValue))
                        {
                            ignore = true;
                            break;
                        }
                    }
                    if (ignore) continue;

                    //do copy
                    PropertyInfo dp = dest.GetType().GetProperty(p.Name);

                    if (dp != null && dp.CanWrite)
                    {
                        if (p.PropertyType.Equals(dp.PropertyType))
                        {
                            dp.SetValue(dest, srcValue, null);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw new Exception("srceo=" + src + ",deseo=" + dest, ex);
            }
        }

        public static void CopyObjectWithNeed(Object src, Object dest, params string[] includeList)
        {
            if (includeList == null)
            {
                Exception ex = new ArgumentNullException("includeList", "include fields can not be null if CopyObjectWithNeed is called");
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }

            CopyObject(src, dest, delegate(string propName)
            {
                return Array.IndexOf<string>(includeList, propName) >= 0; 
            });
        }

        public static void CopyObjectWithUnNeed(Object src, Object dest, params string[] excludeList)
        {
            if (excludeList == null)
            {
                Exception ex = new ArgumentNullException("excludeList", "excludeList fields can not be null if CopyObjectWithUnNeed is called");
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }

            CopyObject(src, dest, delegate(string propName)
            {
                return Array.IndexOf<string>(excludeList, propName) < 0;
            });
        }
    }
}
