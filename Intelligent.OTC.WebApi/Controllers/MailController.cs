using Intelligent.OTC.Business;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Attr;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.Dtos;
using Intelligent.OTC.WebApi.Core;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace Intelligent.OTC.WebApi.Controllers
{
    [UserAuthorizeFilter(actionSet: "common")]
    public class MailController : ApiController
    {
        [HttpPost]
        public MailTmp Post([FromBody]SendMailDto maildto)
        {
            MailTmp mail = maildto.mailInstance;
            MailService mailService = SpringFactory.GetObjectImpl<MailService>("MailService");
            return mailService.SendMail(mail,"","",true);
        }

        [HttpPost]
        [Route("api/mail/saveMail")]
        public MailTmp SaveMail([FromBody]MailTmp mail)
        {
            MailService mailService = SpringFactory.GetObjectImpl<MailService>("MailService");
            return mailService.SaveMail(mail);
        }

        [HttpGet]
        public string GetMailCountDistinct(string category)
        {
            MailService mailService = SpringFactory.GetObjectImpl<MailService>("MailService");
            return mailService.GetMailCountDistinct(category);
        }

        //add by zhangYu customerContact reply and forward
        /// <summary>
        /// Reply/Foward mail
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpGet]
        public MailTmp GetMailInstance(int id, string type)
        {
            MailService service = SpringFactory.GetObjectImpl<MailService>("MailService");
            MailTmp ms = service.GetMailInstance(id, type);
            return ms;
        }

        [HttpGet]
        public MailTmp GetMailInstanceByTemplateId(int templateId)
        {
            MailService service = SpringFactory.GetObjectImpl<MailService>("MailService");
            MailTmp ms = service.GetMailInstance(templateId);
            return ms;
        }

        [HttpGet]
        public MailTmp GetMailInstanceByTemplateIdWithCustomerNum(string templateType, string language)
        {
            //return null;
            MailService service = SpringFactory.GetObjectImpl<MailService>("MailService");
            return service.GetMailInstancebyCusnum(templateType, language);
        }

        //add by zhangYu customerContact contact detail
        /// <summary>
        /// View Mail
        /// </summary>
        /// <param name="messageId"></param>
        /// <returns></returns>
        [HttpGet]
        public MailTmp GetMailInstanceById(string messageId)
        {
            MailService service = SpringFactory.GetObjectImpl<MailService>("MailService");
            MailTmp ms = service.GetMailByMessageId(messageId);
            return ms;
        }

        /// <summary>
        /// Get mail list
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [PagingQueryable]
        public IQueryable<MailDto> Get()
        {
            MailService service = SpringFactory.GetObjectImpl<MailService>("MailService");

            return service.GetMailList(string.Empty, string.Empty).AsQueryable();
        }

        /// <summary>
        /// Get mail list
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("api/mail/querymails")]
        public MailDtoPage QueryMails(MailQueryDto dto)
        {
            MailService service = SpringFactory.GetObjectImpl<MailService>("MailService");

            return service.QueryMails(dto);
        }

        /// <summary>
        /// get mail count of all category
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/mail/querycount")]
        public MailCountDto QueryMailCount()
        {
            MailService mailService = SpringFactory.GetObjectImpl<MailService>("MailService");
            return mailService.QueryMailCount();
        }

        /// <summary>
        /// update selected mail status
        /// </summary>
        /// <param name="id"></param>
        [HttpPost]
        [Route("api/Mail/updateMailCategory")]
        public void UpdateMailCategory([FromBody]List<int> ids, string category)
        {
            MailService service = SpringFactory.GetObjectImpl<MailService>("MailService");
            service.UpdateMailCategory(ids, category);
        }

        /// <summary>
        /// Get mail list with providing customer related conditions
        /// </summary>
        /// <param name="customerNum"></param>
        /// <param name="customerName"></param>
        /// <returns></returns>
        [HttpGet]
        [PagingQueryable]
        public IEnumerable<MailDto> GetMailList(string customerNum, string customerName)
        {
            MailService service = SpringFactory.GetObjectImpl<MailService>("MailService");

            return service.GetMailList(customerNum, customerName).AsQueryable();
        }

        /// <summary>
        /// assign customer
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cusNum"></param>
        [HttpPost]
        public void UpdateMailReference(string id, string cusNum, string siteUseId)
        {
            MailService service = SpringFactory.GetObjectImpl<MailService>("MailService");
            service.UpdateMailReferenceAndAssignedFlg(id, cusNum, siteUseId);
        }

        /// <summary>
        /// delete selected mail
        /// </summary>
        /// <param name="id"></param>
        [HttpPost]
        [Route("api/Mail/deletemail")]
        public void DeleteSelectedMail([FromBody]List<int> ids)
        {
            MailService service = SpringFactory.GetObjectImpl<MailService>("MailService");
            service.DeleteSelectedMail(ids);
        }

        [HttpPost]
        public void MarkAsRead(string msgId)
        {
            MailService service = SpringFactory.GetObjectImpl<MailService>("MailService");
            service.MarkAsRead(msgId);
        }

        [HttpGet]
        public IEnumerable<CustomerMasterData> GetCustomerByMailId(int mailId)
        {
            MailService service = SpringFactory.GetObjectImpl<MailService>("MailService");
            return service.GetCustomerByMessageId(mailId);
        }

        [HttpGet]
        public IEnumerable<CustomerMasterData> GetCustomers(string mailCustNums, string siteUseId)
        {
            MailService service = SpringFactory.GetObjectImpl<MailService>("MailService");
            return service.GetCustomers(mailCustNums, siteUseId);
        }

        [HttpPost]
        [Route("api/Mail/GetInvoiceByMailId")]
        public IEnumerable<MailInvoiceDto> GetInvoiceByMailId([FromBody]List<CustomerKey> customer)
        {
            MailService service = SpringFactory.GetObjectImpl<MailService>("MailService");
            return service.GetInvoiceByMailId(customer);
        }

        [HttpGet]
        public IEnumerable<InvoiceAging> GetInvoiceByInputNums(string mailCustNumsForInv, string inputNums)
        {
            MailService service = SpringFactory.GetObjectImpl<MailService>("MailService");
            return service.GetInvoiceByInputNums(mailCustNumsForInv, inputNums);
        }

        [HttpPost]
        public void UpdateCusMails(string messageId, string cusNums)
        {
            MailService service = SpringFactory.GetObjectImpl<MailService>("MailService");
            service.UpdateCusMails(messageId, cusNums);
        }

        [HttpPost]
        public void RemoveCus(string messageId, string cusNum, string siteUseId)
        {
            MailService service = SpringFactory.GetObjectImpl<MailService>("MailService");
            service.RemoveCus(messageId, cusNum, siteUseId);
        }
    }
}