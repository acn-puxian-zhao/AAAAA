using Intelligent.OTC.Business;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Attr;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.Dtos;
using Intelligent.OTC.WebApi.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;


namespace Intelligent.OTC.WebApi.Controllers
{
    [UserAuthorizeFilter(actionSet: "soa")]
    public class CollectorSoaController : ApiController
    {
        [HttpGet]
        [PagingQueryable]
        public IEnumerable<SoaDto> Get(string invoiceState , string invoiceTrackState , string legalEntity , string invoiceNum, string soNum, string poNum, string invoiceMemo, string customerNum,string customerName,string customerClass, string siteUseId,string EB)
        {
            ISoaService service = SpringFactory.GetObjectImpl<ISoaService>("SoaService");
            return service.GetSoaList(invoiceState, invoiceTrackState, legalEntity, invoiceNum, soNum, poNum, invoiceMemo,customerNum,customerName, customerClass, siteUseId, EB).AsQueryable();
        }


        [HttpGet]
        public IEnumerable<SoaDto> GetNoPaging(string ListType)
        {
            ISoaService service = SpringFactory.GetObjectImpl<ISoaService>("SoaService");
            return service.GetNoPaging(ListType).AsQueryable();
        }

        [HttpGet]
        [Route("api/CollectorSoa/GetInvoicesStatusData")]
        public IEnumerable<InvoicesStatusDto> GetInvoicesStatusData()
        {
            ISoaService service = SpringFactory.GetObjectImpl<ISoaService>("SoaService");
            return service.GetInvoicesStatusList().AsQueryable();
        }
        [HttpGet]
        [Route("api/CollectorSoa/GetCustomerCommentStatusData")]
        public List<CustomerCommentStatusDto> GetCustomerCommentStatusData()
        {
            ISoaService service = SpringFactory.GetObjectImpl<ISoaService>("SoaService");
            return service.GetCustomerCommentStatusData();
        }
        [HttpPost]
        [Route("api/CollectorSoa/SetInvoicesStatusData")]
        public String SetInvoicesStatusData()
        {
            ISoaService service = SpringFactory.GetObjectImpl<ISoaService>("SoaService");
            return service.SetInvoicesStatusList();
        }
        [HttpPost]
        [Route("api/CollectorSoa/DelInvoicesStatusData")]
        public String DelInvoicesStatusData()
        {
            ISoaService service = SpringFactory.GetObjectImpl<ISoaService>("SoaService");
            return service.DelInvoicesStatusData();
        }

        [HttpPost]
        [Route("api/CollectorSoa/saveCustomerAgingComments")]
        public String saveCustomerAgingComments(string LegalEntity, string CustomerNo, string SiteUseId, string Comments)
        {
            ISoaService service = SpringFactory.GetObjectImpl<ISoaService>("SoaService");
            return service.saveCustomerAgingComments(LegalEntity, CustomerNo, SiteUseId, Comments);
        }

        [HttpGet]
        public IEnumerable<CusExpDateHisDto> GetCommDateHistory(string CustomerCode, string SiteUseId)
        {
            ISoaService service = SpringFactory.GetObjectImpl<ISoaService>("SoaService");
            return service.getCommDateHistory(CustomerCode, SiteUseId).AsQueryable();
        }
        [HttpGet]
        public IEnumerable<AgingExpDateHisDto> GetAgingDateHistory(int invId)
        {
            ISoaService service = SpringFactory.GetObjectImpl<ISoaService>("SoaService");
            return service.getAgingDateHistory(invId).AsQueryable();
        }

        //execute soa (Send Soa Page)
        /// <summary>
        /// execute soa (Send Soa Page)
        /// </summary>
        /// <param name="ColSoa">custNums</param>
        /// <param name="Type">create/view soa Detail  create:start WF;view: not start WF</param>
        /// <returns></returns>
        [HttpGet]
        public IEnumerable<SendSoaHead> CreateSoa(string ColSoa,string Type,string siteuseid)
        {
            ISoaService service = SpringFactory.GetObjectImpl<ISoaService>("SoaService");
            return service.CreateSoaForArrow(ColSoa, Type, siteuseid).AsQueryable();
        }

        //open a soa in work in process(collectorSoa Page)
        [HttpGet]
        public CollectorAlert GetSoa(string TaskNo) 
        {
            ISoaService service = SpringFactory.GetObjectImpl<ISoaService>("SoaService");
            return service.GetSoa(TaskNo);
        }

        //get status of a soa
        [HttpGet]
        public CollectorAlert GetStatus(string ReferenceNo)
        {
            ISoaService service = SpringFactory.GetObjectImpl<ISoaService>("SoaService");
            return service.GetStatus(ReferenceNo);
        }

        //view invoice log in sendsoa
        [HttpGet]
        public IEnumerable<InvoiceLog> GetInvLog(string InvNum) 
        {
            ISoaService service = SpringFactory.GetObjectImpl<ISoaService>("SoaService");
            return service.GetInvLog(InvNum).AsQueryable();
        }

        [HttpGet]
        public IEnumerable<Contactor> GetSoaContact(string CustNumFCon) 
        {
            ContactService service = SpringFactory.GetObjectImpl<ContactService>("ContactService");
            return service.GetContactByCustomer(CustNumFCon);
        }

        [HttpGet]
        public IEnumerable<CustomerPaymentBank> GetSoaPayment(string CustNumFPb)
        {
            ISoaService service = SpringFactory.GetObjectImpl<ISoaService>("SoaService");
            return service.GetSoaPayment(CustNumFPb);
        }

        [HttpGet]
        public IEnumerable<CustomerPaymentCircle> GetSoaPaymentCircle(string CustNumFPc, string SiteUseIdFPc)
        {
            ISoaService service = SpringFactory.GetObjectImpl<ISoaService>("SoaService");
            return service.GetSoaPaymentCircle(CustNumFPc, SiteUseIdFPc);
        }

        [HttpGet]
        public IEnumerable<ContactorDomain> GetSoaContactDomain(string CustNumFPd)
        {
            ISoaService service = SpringFactory.GetObjectImpl<ISoaService>("SoaService");
            return service.GetSoaContactDomain(CustNumFPd);
        }

        [HttpGet]
        public IEnumerable<ContactHistory> GetContactHistory(string CustNumsFCH)
        {
            ISoaService service = SpringFactory.GetObjectImpl<ISoaService>("SoaService");
            return service.GetContactHistory(CustNumsFCH);
        }

        [HttpGet]
        public IEnumerable<PeriodControl> GetAllPeriod(string Period)
        {
            ISoaService service = SpringFactory.GetObjectImpl<ISoaService>("SoaService");
            return service.GetAllPeriod();
        }

        [HttpGet]
        public IEnumerable<SoaDto> SelectChangePeriod(int PeriodId)
        {
            ISoaService service = SpringFactory.GetObjectImpl<ISoaService>("SoaService");
            return service.SelectChangePeriod(PeriodId);
        }

        [HttpPost]
        public void Wfchange(string referenceNo, string type) {
            ISoaService service = SpringFactory.GetObjectImpl<ISoaService>("SoaService");
            service.Wfchange("4", referenceNo, type);
            
        }

        //batch sendsoa 
        [HttpPost]
        public void BatchSoa(string Cusnums,string siteUseId)
        {
            ISoaService service = SpringFactory.GetObjectImpl<ISoaService>("SoaService");
            service.BatchSoa(Cusnums,siteUseId);
        }

        //save invoice comment
        [HttpPost]
        public void SaveComm(int Invid, string Comm,string CommDate)
        {
            if (Invid != 0)
            {
                ISoaService service = SpringFactory.GetObjectImpl<ISoaService>("SoaService");
                service.SaveComm(Invid, Comm, CommDate);
            }
        }

        [HttpPost]
        //save Notes
        public void SaveNotes(string Cus, string SpNotes)
        {
            ISoaService service = SpringFactory.GetObjectImpl<ISoaService>("SoaService");
            service.SaveNotes(Cus, SpNotes);
        }

        //common Post
        [HttpPost]
        public void CommonSave([FromBody]List<string> list)
        {
            
            if (list[0] == "1") 
            {
                SpecialNotesService service = SpringFactory.GetObjectImpl<SpecialNotesService>("SpecialNotesService");
                service.AddOrUpdateByParaByArrow(list[1], list[2], list[3],list[4], list[5], list[6]);
            }
            else if (list[0] == "2") 
            {
                ISoaService service = SpringFactory.GetObjectImpl<ISoaService>("SoaService");
                service.SaveComm(Convert.ToInt32(list[1]), list[2], list[3]);
            }
            else if (list[0] == "3") 
            {
                // payment 
                ContactService service = SpringFactory.GetObjectImpl<ContactService>("ContactService");
                service.insertInvoiceLogForNotice(list);
            }
            else if (list[0] == "4")
            {
                // PTP
                ContactService service = SpringFactory.GetObjectImpl<ContactService>("ContactService");
                service.insertInvoiceLogForPtp(list);
            }
            else if (list[0] == "5") {
                ISoaService service = SpringFactory.GetObjectImpl<ISoaService>("SoaService");
                service.BatchSaveComm(list[1], list[2], list[3]);
            }
        }


        [HttpGet]
        public int CheckPermission(string ColSoa)
        {
            ISoaService service = SpringFactory.GetObjectImpl<ISoaService>("SoaService");
            return service.CheckPermission(ColSoa);
        }

        [HttpGet]
        public IEnumerable<T_Invoice_Detail> GetInvoiceDetail(string InvNumNo)
        {
            ISoaService service = SpringFactory.GetObjectImpl<ISoaService>("SoaService");
            return service.GetInvoiceDetail(InvNumNo).AsQueryable();
        }


    }
}