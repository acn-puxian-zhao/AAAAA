using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Intelligent.OTC.Common;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.Dtos;

namespace Intelligent.OTC.Business
{
    public interface IMailService
    {
        IQueryable<MailTemplate> GetMailTemplates();
        List<MailTemplateDto> GetMailTemplates(string language, string type);
        string getCustomerLanguageByCusnum(string custnum, string siteUseId);
        void SaveOrUpdateTemplate(MailTemplate template);
        void BulkSaveMail(List<MailTmp> mailInstance);
        MailTemplate GetMailTemplate(string type);
        MailTemplate GetMailTemplatebytype(string type, string lang);
        MailTemplate GetMailTemplateById(int templateId);
        MailTmp GetInstanceFromTemplate(MailTemplate template, Action<ITemplateParser> registContextCallBack, string Region="");
        MailTmp GetInstanceFromTemplate(MailTemplate template, Action<ITemplateParser> registContextCallBack, SysUser User, string Region="");
        MailTmp SaveMail(MailTmp mailInstance, string collector = "");
        MailTmp SendMail(MailTmp mail, string TempleteLanguage = "", string Collector = "", bool lb_FactSend = false);
        void UpdateDateAfterSendMail(MailTmp mail);
        string GetSenderMailAddress(string Region = "");
        string GetWarningSenderMailAddress();
        string GetWarningSenderMailAddress(string email);
        MailCountDto QueryMailCount();
        SysTypeDetail getMailSoaInfoByAlert(string type, string region);
        Boolean CheckMailToOnly(string toTitle, string alertType, List<int> intIds);
        List<int> CheckMailToFactInv(string toTitle, string toName, string alertType, List<int> intIds, string strTempleteLanguage);
        string getMailToContactName(string toTitle, string alertType, List<int> intIds);
        string getMailResponseDate(string alertType);
        List<string> getCustomerLegalEntityByCusnum(string custnum, string siteUseId);
        List<string> getCustomerRegionByCusnum(string custnum, string siteUseId);
        void deleteFsrLsrChange();
        string getContactorMailByInv(List<int> invs, string title);

        void findContactor(SendContactorNameDto finder);
        string GetSenderMailAddressByOperator(string strOperator);
    }
}
