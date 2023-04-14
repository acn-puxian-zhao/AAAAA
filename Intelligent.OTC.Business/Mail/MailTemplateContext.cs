using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Repository;
using Intelligent.OTC.Domain.DataModel;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Intelligent.OTC.Business
{
    public class MailTemplateContext
    {
        public IRepository CommonRep { get; set; }

        public ITemplateParser Parser { get; set; }

        /// <summary>
        /// Get mail tempate wording: default pay cycle
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
        public string GetDefaultPayCycle(string format = "yyyy/MM/dd")
        {
            PeroidService ps = SpringFactory.GetObjectImpl<PeroidService>("PeroidService");
            PeriodControl pc = ps.getcurrentPeroid();
            return pc.EndDate.ToString(format);
        }

        /// <summary>
        /// Get mail tempate wording: collector's signature
        /// </summary>
        /// <returns></returns>
        public string GetCollectorSignature()
        {
            string res = string.Empty;
            SysUser collector = Parser.GetContext("collector") as SysUser;
            string collectoreid = string.Empty;

            // 判断是否自动用户，自动用户获取邮件 collector ，未完待续
            if (collector == null || collector.EID == "BATCH_USER")
            {
                List<string> collectors = GetSiteCollectors();
                if (collectors != null && collectors.Count > 0)
                    collectoreid = collectors.FirstOrDefault();
                else
                    return res;
            }
            else
            {
                collectoreid = collector.EID;
            }

           
            var signature = CommonRep.GetQueryable<CollectorSignature>().Where(coll => coll.Collector == collectoreid && coll.LANGUAGE == "002").FirstOrDefault();
            if (signature != null)
            {
                return signature.Signature;
            }

            return res;
        }

        public string GetCollectorSignatureEn()
        {
            string res = string.Empty;
            string collector = Parser.GetContext("collector") as string;
            string language = Parser.GetContext("templatelang") as string;
            string collectoreid = string.Empty;

            // 判断是否自动用户，自动用户获取邮件 collector ，未完待续
            if (collector == null || collector == "BATCH_USER")
            {
                List<string> collectors = GetSiteCollectors();
                if (collectors != null && collectors.Count > 0)
                    collectoreid = collectors.FirstOrDefault();
                else
                    return res;
            }
            else
            {
                collectoreid = collector;
            }

            var signature = CommonRep.GetQueryable<CollectorSignature>().Where(coll => coll.Collector == collectoreid && coll.LANGUAGE == language).FirstOrDefault();
            if (signature != null)
            {
                return signature.Signature;
            }

            return res;
        }

        public List<string> GetSiteCollectors()
        {
            string siteUseId = Parser.GetContext("siteUseId") as string;
            if (string.IsNullOrEmpty(siteUseId))
                return null;

            siteUseId = siteUseId.TrimEnd(',');
            string[] siteUseIds = siteUseId.Split(',');

            return CommonRep.GetQueryable<Customer>().Where(x => siteUseIds.Contains(x.SiteUseId)).Select(x=>x.Collector).ToList();
        }

        /// <summary>
        /// Get mail tempate wording: Contacts (mail to)
        /// </summary>
        /// <returns></returns>
        public string GetContacts()
        {
            return Parser.GetContext("contactNames") as string;
        }

        public string GetOldMailBody()
        {
            return Parser.GetContext("body") as string;
        }

        public string GetNow(string format = "yyyy/MM/dd")
        {
            return AppContext.Current.User.Now.ToString(format);
        }

        //=========added by alex body中显示附件名+Currency============
        public string GetAttachmentInfo()
        {
            return Parser.GetContext("attachmentInfo") as string;
        }

        public string GetIssuess()
        {
            return Parser.GetContext("issuedescription") as string;
        }
        public string GetCustomerMessage()
        {
            return Parser.GetContext("customerMessage") as string;
        }
        public string GetCompany()
        {
            return Parser.GetContext("company") as string;
        }
        public string GetDueDate()
        {
            return Parser.GetContext("dueDate") as string;
        }

        public string GetCustomerName()
        {
            return Parser.GetContext("customerName") as string;
        }

        public string GetSiteUseId()
        {
            return Parser.GetContext("siteUseId") as string;
        }

        public string GetContextSiteUseId()
        {
            string siteUseId = Parser.GetContext("siteUseId") as string;
            if (string.IsNullOrEmpty(siteUseId))
                return "";

            siteUseId = siteUseId.TrimEnd(',');
            string[] siteUseIds = siteUseId.Split(',');
            string siteUseIdStr = string.Join(@""",""", siteUseIds);
            string contextStr = @"${""SiteUseId"":[""" + siteUseIdStr + @"""]}$";

            return contextStr;
        }

        public static List<string> GetSiteUseIdsInContext(string mailContext)
        {
            List<string> siteUseIdList = new List<string>();

            string nFindText = mailContext.Replace(@"&quot;",@"""");
            string nLeft = @"${""SiteUseId"":[";
            string nRight = @"]}$";

            int nStartIndex = nFindText.IndexOf(nLeft);
            int nEndIndex = nFindText.IndexOf(nRight);

            while (nStartIndex >= 0)
            {
                string nSiteUseIdStr = nFindText.Substring(nStartIndex + nLeft.Length, nEndIndex - nStartIndex - nLeft.Length);
                nSiteUseIdStr = nSiteUseIdStr.Replace(@"""", "");
                string[] nSiteUseIdArray = nSiteUseIdStr.TrimEnd(',').Split(',');
                siteUseIdList.AddRange(nSiteUseIdArray);

                nFindText = nFindText.Substring(nEndIndex + nRight.Length, nFindText.Length - nEndIndex - nRight.Length);

                nStartIndex = nFindText.IndexOf(nLeft);
                nEndIndex = nFindText.IndexOf(nRight);
            }
            return siteUseIdList.Distinct().ToList();
        }
    }
    
}
