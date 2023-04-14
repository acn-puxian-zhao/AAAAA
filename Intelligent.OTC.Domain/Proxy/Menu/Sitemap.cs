using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Xml;
using System.Collections.Specialized;
using Intelligent.OTC.Domain.Dtos;

namespace Xccelerator.Sitemap.Domain
{
    [Serializable]
    public class Sitemap
    {

        private static string _path
        {
            get
            {
                if (System.Configuration.ConfigurationManager.AppSettings["IsDev"] == "true")
                {
                    return System.Web.HttpContext.Current.Server.MapPath("~/Sitemap.dev.sitemap");
                }
                else
                {
                    return System.Web.HttpContext.Current.Server.MapPath("~/Sitemap.sitemap");
                }
            }
        }

        public bool IsRoot { get; set; }

        public int Level { get; set; }

        public long OperativeId { get; set; }

        public bool HasPermission { get; set; }

        public string Domain { get; set; }

        public string Icon { get; set; }

        public string Url { get; set; }

        private string _fullUrl = null;
        public string FullUrl
        {
            get
            {
                if (_fullUrl == null)
                {
                    _fullUrl = CombineUrl(Domain, Url);
                }
                return _fullUrl;
            }
        }

        public string Title { get; set; }

        public string ClsClass { get; set; }

        public string MenuId { get; set; }

        private List<MapProperty> _Propertys = new List<MapProperty>();
        public List<MapProperty> Propertys { get { return _Propertys; } set { _Propertys = value; } }

        private List<Sitemap> maps = new List<Sitemap>();
        public List<Sitemap> Maps { get { return maps; } set { maps = value; } }

        public void Save()
        {
            SaveSerialize(typeof(Sitemap), this, _path);
        }

        public string GetUrl(Dictionary<string, string> replaceStrings)
        {
            string domain = Domain;

            if (replaceStrings != null)
            {
                foreach (var item in replaceStrings)
                {
                    domain = domain.Replace(item.Key, item.Value);
                }
            }            

            string result = CombineUrl(domain, Url);

            System.Diagnostics.Debug.WriteLine("Domain:{0}, Url:{1}, Result:{2}", Domain, Url, result);


            //result = result.Replace("///", "/");
            //if (result.ToLower().StartsWith("http://") == false)
            //{
            //    //result = "http://" + result;
            //}

            return result;
        }

        private static string CombineUrl(string domian, string url)
        {
            if (domian == null || domian.Length == 0)
            {
                return url;
            }

            if (url == null || url.Length == 0)
            {
                return domian;
            }

            domian = domian.TrimEnd('/', '\\');
            url = url.TrimStart('/', '\\');

            return String.Format("{0}/{1}", domian, url);
        }

        public string GetPropertysString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in this.Propertys)
            {
                sb.AppendFormat(" {0}=\"{1}\" ", item.Key, item.Value);
            }
            return sb.ToString();
        }

        public static Sitemap Initial()
        {
            //var cache = CacheHelper.GetCache("_UserMenuCatch");
            //if (cache == null)
            //{
            //    using (StreamReader reader = new StreamReader(_path, Encoding.UTF8))
            //    {
            //        string str = reader.ReadToEnd();
            //        CacheHelper.SetCache("_UserMenuCatch", DeSerialize(typeof(Sitemap), str));
            //    }
            //}


            //var resultMap = ((Sitemap)CacheHelper.GetCache("_UserMenuCatch"));

            using (StreamReader reader = new StreamReader(_path, Encoding.UTF8))
            {
                string str = reader.ReadToEnd();
                var resultMap = (Sitemap)DeSerialize(typeof(Sitemap), str);



                resultMap.Maps.ForEach(p =>
                {
                    if (p.Title != "Home")
                    {
                        //if (permissions.Contains(p.OperativeId) == false && p.OperativeId != 0)
                        //{
                        //    resultMap.Maps.Remove(p);
                        //}
                        //p.Maps.ForEach(x =>
                        //{
                        //    if (permissions.Contains(x.OperativeId) == false && x.OperativeId != 0)
                        //    {
                        //        p.Maps.Remove(x);
                        //    }
                        //});
                    }
                });
                return resultMap;
            }

        }

        public static void FixPermissionAndUrl(Sitemap sitemap, long[] permissions, NameValueCollection urlSettings)
        {

            sitemap.HasPermission = permissions == null || sitemap.OperativeId == 0 || permissions.Contains(sitemap.OperativeId);
            sitemap.Domain = urlSettings[sitemap.Domain];

            if (sitemap.Propertys != null)
            {
                foreach (var p in sitemap.Propertys)
                {
                    for (int i = 0; i < urlSettings.Count; i++)
                    {
                        p.Value = p.Value.Replace(urlSettings.Keys[i], urlSettings[i]);
                    }
                }
            }

            if (sitemap.Maps != null)
            {
                foreach (var submap in sitemap.Maps)
                {
                    FixPermissionAndUrl(submap, permissions, urlSettings);
                }
            }
        }

        private static void SaveSerialize(Type Types, object Info, string FilePath)
        {
            FileInfo file = new FileInfo(FilePath);

            if (!Directory.Exists(file.DirectoryName))//目录是否存在
            {
                Directory.CreateDirectory(file.DirectoryName);//不存在则创建
            }
            FileStream stream = null;
            stream = new FileStream(FilePath, FileMode.Create, FileAccess.Write, FileShare.Write);
            StreamWriter writer = new StreamWriter(stream, Encoding.UTF8);
            writer.Write(Serialize(Info));
            writer.Close();
            stream.Close();
        }

        private static string Serialize(object obj)
        {
            string returnStr = "";
            XmlSerializer serializer = new XmlSerializer(obj.GetType());
            MemoryStream ms = new MemoryStream();
            XmlTextWriter xtw = null;
            StreamReader sr = null;
            try
            {
                xtw = new System.Xml.XmlTextWriter(ms, Encoding.UTF8);
                xtw.Formatting = System.Xml.Formatting.Indented;
                serializer.Serialize(xtw, obj);
                ms.Seek(0, SeekOrigin.Begin);
                sr = new StreamReader(ms);
                returnStr = sr.ReadToEnd();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (xtw != null)
                    xtw.Close();
                if (sr != null)
                    sr.Close();
                ms.Close();
            }
            return returnStr;

        }

        private static object DeSerialize(Type type, string s)
        {
            byte[] b = System.Text.Encoding.UTF8.GetBytes(s);
            try
            {
                XmlSerializer serializer = new XmlSerializer(type);
                return serializer.Deserialize(new MemoryStream(b));
            }
            catch
            {
                return null;
            }
        }

        public Sitemap Clone()
        {
            return (Sitemap)this.MemberwiseClone();
        }


    }
}
