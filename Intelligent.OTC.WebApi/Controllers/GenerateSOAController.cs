using Intelligent.OTC.Business;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Attr;
using Intelligent.OTC.Common.Exceptions;
using Intelligent.OTC.Common.Utils;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.Dtos;
using Intelligent.OTC.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Http;
using System.Linq;
using System.Data;

namespace Intelligent.OTC.WebApi.Controllers
{
    [UserAuthorizeFilter(actionSet: "soa")]
    public class GenerateSOAController : ApiController
    {
        public const string strMailAttachmentPath = "MailAttachmentPath";//GenerateSOA路径的config保存名
        public const string strArchiveMailAttachmentPath = "ArchiveMailAttachmentPath";//ArchiveSOA

        [HttpPost]
        public void Post([FromBody]SendMailDto mailDto)
        {
            MailTmp mail = mailDto.mailInstance;
            ISoaService soaservice = SpringFactory.GetObjectImpl<ISoaService>("SoaService");
            MailService mailService = SpringFactory.GetObjectImpl<MailService>("MailService");

            soaservice.sendSoaSaveInfoToDB(mail,null);
        }

        [HttpPost]
        [Route("api/generateSOA/generateCheck")]
        public bool GetSOAGenerateCheck(string customerNums, string siteUseId, [FromBody]List<int> intIds, string mType, string fileType)
        {
            ISoaService service = SpringFactory.GetObjectImpl<ISoaService>("SoaService");
            IMailService Mailservice = SpringFactory.GetObjectImpl<IMailService>("MailService");
            if (mType != "099")
            {
                var defaultlang = Mailservice.getCustomerLanguageByCusnum(customerNums, siteUseId);
                List<string> legalEntity = Mailservice.getCustomerLegalEntityByCusnum(customerNums, siteUseId);
                if (legalEntity.Count == 0 || legalEntity.Count > 1)
                {
                    throw new OTCServiceException("Please select only one LegalEntity !");
                }
                List<string> RegionList = Mailservice.getCustomerRegionByCusnum(customerNums, siteUseId);
                if (RegionList.Count == 0 || RegionList.Count > 1)
                {
                    throw new OTCServiceException("Please select only one Region !");
                }
                string Region = RegionList[0];
                if (string.IsNullOrEmpty(Region))
                {
                    throw new OTCServiceException("Not set Region, Please set first!");
                }
                //判断这些发票是ToTitle是否会发送给多个人
                SysTypeDetail d = Mailservice.getMailSoaInfoByAlert((Convert.ToInt32(mType)).ToString(), Region);
                if (d == null) {
                    throw new OTCServiceException("No Templete !");
                }
                string Collector = AppContext.Current.User.EID;
                string CCTitle = d.DetailValue3;
                string ToTitle = d.DetailValue2;
                string[] ToTitleGroup = ToTitle.Split(',');
                string ToName = "";

                bool lb_return = false;
                foreach (string totitleCur in ToTitleGroup)
                {
                    lb_return = Mailservice.CheckMailToOnly(totitleCur, mType, intIds);
                    if (lb_return)
                    {
                        ToTitle = totitleCur;
                        break;
                    }
                }
                if (lb_return == false)
                {
                    throw new OTCServiceException("Please select only one " + ToTitle + " (or no customer email)!");
                }

                ToName = Mailservice.getMailToContactName(ToTitle, mType, intIds);

                //根据不同的SOA类型，获得实际需要发送的发票
                List<int> intIds_New = Mailservice.CheckMailToFactInv(ToTitle, ToName, mType, intIds, defaultlang);
                if (intIds_New.Count == 0)
                {
                    throw new OTCServiceException("No invoice need to send !");
                }
            }

            return true;
        }

        [HttpPost]
        [Route("api/generateSOA/generate")]
        public MailTmp GetSOAMailInstance(string customerNums, string siteUseId, [FromBody]List<int> intIds, string mType, string fileType)
        {
            MailTmp mail = new MailTmp();
            ISoaService service = SpringFactory.GetObjectImpl<ISoaService>("SoaService");
            IMailService Mailservice = SpringFactory.GetObjectImpl<IMailService>("MailService");
            if (mType != "099")
            {
                var defaultlang = Mailservice.getCustomerLanguageByCusnum(customerNums, siteUseId);
                List<string> legalEntity = Mailservice.getCustomerLegalEntityByCusnum(customerNums, siteUseId);
                if (legalEntity.Count == 0 || legalEntity.Count > 1)
                {
                    throw new OTCServiceException("Please select only one LegalEntity !");
                }
                List<string> RegionList = Mailservice.getCustomerRegionByCusnum(customerNums, siteUseId);
                if (RegionList.Count == 0 || RegionList.Count > 1)
                {
                    throw new OTCServiceException("Please select only one Region !");
                }
                string Region = RegionList[0];
                if (string.IsNullOrEmpty(Region))
                {
                    throw new OTCServiceException("Not set Region, Please set first!");
                }
                //判断这些发票是ToTitle是否会发送给多个人
                SysTypeDetail d = Mailservice.getMailSoaInfoByAlert((Convert.ToInt32(mType)).ToString(), Region);
                string Collector = AppContext.Current.User.EID;
                string CCTitle = d.DetailValue3;
                string ToTitle = d.DetailValue2;
                string[] ToTitleGroup = ToTitle.Split(',');
                string ToName = "";

                bool lb_return = false;
                foreach (string totitleCur in ToTitleGroup)
                {
                    lb_return = Mailservice.CheckMailToOnly(totitleCur, mType, intIds);
                    if (lb_return)
                    {
                        ToTitle = totitleCur;
                        break;
                    }
                }
                if (lb_return == false)
                {
                    throw new OTCServiceException("Please select only one " + ToTitle + " !");
                }

                ToName = Mailservice.getMailToContactName(ToTitle, mType, intIds);

                //根据不同的SOA类型，获得实际需要发送的发票
                List<int> intIds_New = Mailservice.CheckMailToFactInv(ToTitle, ToName, mType, intIds, defaultlang);
                if (intIds_New.Count == 0)
                {
                    throw new OTCServiceException("No invoice need to send !");
                }

                //获得响应时间
                string ResponseDate = Mailservice.getMailResponseDate(mType);

                mail = service.GetNewMailInstance(customerNums, siteUseId, mType, defaultlang, intIds_New, Collector, ToTitle, ToName, CCTitle, ResponseDate, Region, "", fileType);
            }
            else {
                string senderMailbox = Mailservice.GetSenderMailAddress();
                StringBuilder sb = new StringBuilder();
                sb.Append("<p class=\"MsoNormal\">Dear,\n");
                sb.Append("<Table cellspacing=\"0\" cellpadding=\"0\" style=\"font-family:'Microsoft YaHei';font-size:10px\">" + Environment.NewLine);
                sb.Append("<tr style=\"background:#F0FFFF\"><th style=\"border-left:#000000 solid 1px;border-top:#000000 solid 1px;border-bottom:#000000 solid 1px;min-width:20px\">#</th><th  style=\"border-left:#000000 solid 1px;border-top:#000000 solid 1px;border-bottom:#000000 solid 1px;min-width:200px\">CustomerName</th><th style=\"border-left:#000000 solid 1px;border-top:#000000 solid 1px;border-bottom:#000000 solid 1px;min-width:50px\">Accnt Number</th><th style=\"border-left:#000000 solid 1px;border-top:#000000 solid 1px;border-bottom:#000000 solid 1px;min-width:50px\">SiteUseId</th><th style=\"border-left:#000000 solid 1px;border-top:#000000 solid 1px;border-bottom:#000000 solid 1px;min-width:30px\">Class</th><th style=\"border-left:#000000 solid 1px;border-top:#000000 solid 1px;border-bottom:#000000 solid 1px;min-width:50px\">InvoiceNum</th><th style=\"border-left:#000000 solid 1px;border-top:#000000 solid 1px;border-bottom:#000000 solid 1px;min-width:50px\">Trx Date</th><th style=\"border-left:#000000 solid 1px;border-top:#000000 solid 1px;border-bottom:#000000 solid 1px;min-width:50px\">Due Date</th><th style=\"border-left:#000000 solid 1px;border-top:#000000 solid 1px;border-bottom:#000000 solid 1px;min-width:30px\">Func CurrCode</th><th style=\"border-left:#000000 solid 1px;border-top:#000000 solid 1px;border-bottom:#000000 solid 1px;min-width:30px\">Inv CurrCode</th><th style=\"border-left:#000000 solid 1px;border-top:#000000 solid 1px;border-bottom:#000000 solid 1px;min-width:30px\">Due Days</th><th style=\"border-left:#000000 solid 1px;border-top:#000000 solid 1px;border-bottom:#000000 solid 1px;min-width:50px\">BalanceAMT</th><th style=\"border-left:#000000 solid 1px;border-top:#000000 solid 1px;border-bottom:#000000 solid 1px;min-width:100px\">Payment Term Desc</th><th style=\"border-left:#000000 solid 1px;border-top:#000000 solid 1px;border-bottom:#000000 solid 1px;min-width:200px\">Ebname</th><th style=\"border-left:#000000 solid 1px;border-top:#000000 solid 1px;border-bottom:#000000 solid 1px;min-width:30px\">Org Id</th></tr>\n");
                List <MyinvoicesDto> invAging = service.GetInvoiceByIds(intIds);
                int i = 1;
                decimal ldec_total = 0;
                string strCustomerNum = "";
                string strSizeUseId = "";
                string strCustomerName = "";
                if (invAging.Count > 0) {
                    strCustomerNum = invAging[0].CustomerNum;
                    strSizeUseId = invAging[0].SiteUseId;
                    strCustomerName = service.getCustomerName(strCustomerNum, strSizeUseId);
                }
                foreach (MyinvoicesDto item in invAging)
                {
                    DateTime duedate = item.DueDate == null ? AppContext.Current.User.Now : Convert.ToDateTime(item.DueDate);
                    TimeSpan sp = AppContext.Current.User.Now.Subtract(duedate);
                    int duedays = sp.Days;
                    ldec_total += Math.Round(Convert.ToDecimal(item.BalanceAmt), 2);
                    sb.Append("<tr><td align = \"center\" style=\"border-left:#000000 solid 1px;border-bottom:#000000 solid 1px;\">" + i + "</td><td style=\"border-left:#000000 solid 1px;border-bottom:#000000 solid 1px;\">" + item.CustomerName + "</td><td align = \"center\" style=\"border-left:#000000 solid 1px;border-bottom:#000000 solid 1px;\">" + item.CustomerNum + "</td><td align = \"center\" style=\"border-left:#000000 solid 1px;border-bottom:#000000 solid 1px;\">" + item.SiteUseId + "</td><td align = \"center\" style=\"border-left:#000000 solid 1px;border-bottom:#000000 solid 1px;\">" + item.Class + "</td><td style=\"border-left:#000000 solid 1px;border-bottom:#000000 solid 1px;\">" + item.InvoiceNum + "</td><td align = \"center\" style=\"border-left:#000000 solid 1px;border-bottom:#000000 solid 1px;\">" + Convert.ToDateTime(item.InvoiceDate).ToString("yyyy-MM-dd") + "</td><td align = \"center\" style=\"border-left:#000000 solid 1px;border-bottom:#000000 solid 1px;\">" + Convert.ToDateTime(item.DueDate).ToString("yyyy-MM-dd") + "</td><td align = \"center\" style=\"border-left:#000000 solid 1px;border-bottom:#000000 solid 1px;\">" + item.FuncCurrCode + "</td><td align = \"center\" style=\"border-left:#000000 solid 1px;border-bottom:#000000 solid 1px;\">" + item.Currency + "</td><td align = \"right\" style=\"border-left:#000000 solid 1px;border-bottom:#000000 solid 1px;\">" + duedays + "</td><td align = \"right\" style=\"border-left:#000000 solid 1px;border-bottom:#000000 solid 1px;\">" + Math.Round(Convert.ToDecimal(item.BalanceAmt),2).ToString("#,##0.00") + "</td><td style=\"border-left:#000000 solid 1px;border-bottom:#000000 solid 1px;\">" + item.CreditTrem + "</td><td style=\"border-left:#000000 solid 1px;border-bottom:#000000 solid 1px;\">" + item.Ebname + "</td><td align = \"center\" style=\"border-left:#000000 solid 1px;border-bottom:#000000 solid 1px;\">" + item.LegalEntity + "</td></tr>\n");
                    i++;
                }
                sb.Append("<tr style=\"font-weight:bold\"><td style=\"border-left:#000000 solid 1px;border-bottom:#000000 solid 1px;\">" + "" + "</td><td align = \"right\" style=\"border-left:#000000 solid 1px;border-bottom:#000000 solid 1px;\">" + "Total:" + "</td><td style=\"border-left:#000000 solid 1px;border-bottom:#000000 solid 1px;\">" + "" + "</td><td style=\"border-left:#000000 solid 1px;border-bottom:#000000 solid 1px;\">" + "" + "</td><td style=\"border-left:#000000 solid 1px;border-bottom:#000000 solid 1px;\">" + "" + "</td><td style=\"border-left:#000000 solid 1px;border-bottom:#000000 solid 1px;\">" + "" + "</td><td style=\"border-left:#000000 solid 1px;border-bottom:#000000 solid 1px;\">" + "" + "</td><td style=\"border-left:#000000 solid 1px;border-bottom:#000000 solid 1px;\">" + "" + "</td><td style=\"border-left:#000000 solid 1px;border-bottom:#000000 solid 1px;\">" + "" + "</td><td style=\"border-left:#000000 solid 1px;border-bottom:#000000 solid 1px;\">" + "" + "</td><td style=\"border-left:#000000 solid 1px;border-bottom:#000000 solid 1px;\">" + "" + "</td><td align = \"right\" style=\"border-left:#000000 solid 1px;border-bottom:#000000 solid 1px;\">" + Math.Round(ldec_total, 2).ToString("#,##0.00") + "</td><td style=\"border-left:#000000 solid 1px;border-bottom:#000000 solid 1px;\">" + "" + "</td><td style=\"border-left:#000000 solid 1px;border-bottom:#000000 solid 1px;\">" +"" + "</td><td style=\"border-left:#000000 solid 1px;border-bottom:#000000 solid 1px;\">" + "" + "</td></tr>\n");
                sb.Append("</table >\n</p>");
                CollectorSignatureService SignatureService = SpringFactory.GetObjectImpl<CollectorSignatureService>("CollectorSignatureService"); 
                CollectorSignature SIG = SignatureService.GetCollectSignture("001");
                sb.Append("<br>");
                sb.Append("<br>");
                if(SIG != null) { 
                    sb.Append(SIG.Signature==null ? "": SIG.Signature);
                }
                string strTo = Mailservice.getContactorMailByInv(intIds,"CS;Sales");
                string strCC = Mailservice.getContactorMailByInv(intIds, "Credit Officer;Collector");
                mail.Subject = strCustomerNum + "/" + strSizeUseId + "/" + strCustomerName + " (" + AppContext.Current.User.EID.ToUpper() + " From IOTC)";
                mail.Body = sb.ToString();
                mail.From = senderMailbox;
                mail.To = strTo;
                mail.Cc = strCC;
                mail.Attachment = "";
                mail.Deal = AppContext.Current.User.Deal;
                mail.Type = "OUT";
                mail.Category = "Sent";
                mail.MailBox = senderMailbox;
                mail.CreateTime = DateTime.Now;
                mail.Operator = "System";
            }

            return mail;
        }

        [HttpPost]
        [Route("api/generateSOA/generatepmt")]
        public MailTmp GetPMTMailInstance(string customerNums, string siteUseId, string mType)
        {
            ISoaService service = SpringFactory.GetObjectImpl<ISoaService>("SoaService");
            IMailService Mailservice = SpringFactory.GetObjectImpl<IMailService>("MailService");
            var defaultlang = Mailservice.getCustomerLanguageByCusnum(customerNums, siteUseId);
            return service.GetPmtMailInstance(customerNums, siteUseId, mType, defaultlang);
        }

        [HttpPost]
        [Route("api/generateSOA/generateTemp")]
        public MailTmp GetSOAMailInstance(string customerNums, int templateId, string siteUseId,string templateType,string templatelang, [FromBody]List<int> intIds)
        {
            ISoaService service = SpringFactory.GetObjectImpl<ISoaService>("SoaService");
            return service.GetNewMailInstance(customerNums, siteUseId, templateType, templatelang, intIds, "", "", "" , "" , "", "", "XLS");
        }

        [HttpPost]
        [Route("api/generateSOA/generateTemp")]
        public MailTmp GetSOAMailInstance(GetSOAMailInstanceDto getSoaMailDto)
        {
            ISoaService service = SpringFactory.GetObjectImpl<ISoaService>("SoaService");
            return service.GetNewMailInstance(getSoaMailDto.customerNums, getSoaMailDto.siteUseId, getSoaMailDto.templateType, getSoaMailDto.templatelang, getSoaMailDto.intIds, "", "", "", "", "", "", "XLS");
        }
        


        [HttpPost]
        [Route("api/generateSOA/generateAtta")]
        public IList<string> Post(string Type, [FromBody]List<int> intIds, string customerNum, string siteUseId)
        {
            List<InvoiceAging> invoicelist = new List<InvoiceAging>();

            InvoiceService service = SpringFactory.GetObjectImpl<InvoiceService>("InvoiceService");

            System.Data.DataTable[] reportItemList;
            List<string> lstPath = service.setContent(intIds, Type, out reportItemList, customerNum, siteUseId, null);
            return lstPath;
        }

    }
}