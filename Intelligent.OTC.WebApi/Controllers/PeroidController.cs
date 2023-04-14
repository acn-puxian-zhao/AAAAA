using Intelligent.OTC.Business;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Attr;
using Intelligent.OTC.Common.Exceptions;
using Intelligent.OTC.Common.Utils;
using Intelligent.OTC.Domain;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.WebApi.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;

namespace Intelligent.OTC.WebApi.Controllers
{
    [UserAuthorizeFilter(actionSet: "period")]
    public class PeroidController : ApiController
    {
        [HttpGet]
        [PagingQueryable]
        public IQueryable<PeroidReport> Get()
        {
            PeroidService service = SpringFactory.GetObjectImpl<PeroidService>("PeroidService");
            return service.GetAllPeroidReports().AsQueryable();

        }

        [HttpPost]
        public string post([FromBody]List<string> lstEndDate)
        {
            try
            {
                PeroidService service = SpringFactory.GetObjectImpl<PeroidService>("PeroidService");
                int id = 0;
                string strStartDate = "";
                string strEndDate = "";
                foreach(var str in lstEndDate)
                {
                    int idTemp;
                    if(int.TryParse(str,out idTemp))
                    {
                        id = idTemp;
                    }
                    else
                    {
                        DateTime dt;
                        if(DateTime.TryParse(str,out dt))
                        {
                            if (strStartDate == "") strStartDate = str;
                            if (strEndDate == "") strEndDate = str;
                            if (DateTime.Parse(strStartDate) > dt) strStartDate = str;
                            if (DateTime.Parse(strEndDate) < dt) strEndDate = str;
                        }
                    }
                }
                return service.AddOrUpdatePeriod(id,strStartDate,strEndDate);
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw new OTCServiceException("DB error!" + ex.Message);
            }

        }

        [HttpPost]
        [Route("api/peroid/delete")]
        public string deletePeriod(int id)
        {
            try
            {
                PeroidService service = SpringFactory.GetObjectImpl<PeroidService>("PeroidService");
                return service.DeletePeriod(id);
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw new OTCServiceException("DB error!" + ex.Message);
            }
        }

        [HttpPost]
        public void post(string type)
        {
            //TODO
            //SOA START
            PeroidService service = SpringFactory.GetObjectImpl<PeroidService>("PeroidService");
            service.StartSOATask();
        }

        [HttpGet]
        public IQueryable<UploadInfo> Get(string Id)
        {
            PeroidService service = SpringFactory.GetObjectImpl<PeroidService>("PeroidService");
            return service.getCurrentPeroidUploadTimes(Convert.ToInt32(Id)).AsQueryable();
        }

        [HttpGet]
        public string get(string type)
        {
            if (type == "Deal")
            {
                return AppContext.Current.User.Deal.ToString();
            }
            else
            {
                string periodEndDate = type;
                DateTime dtNow = AppContext.Current.User.Now;
                DateTime dt = Convert.ToDateTime(periodEndDate + " 23:59:59");

                if (dt < dtNow)
                {
                    return "2";
                }

                PeroidService service = SpringFactory.GetObjectImpl<PeroidService>("PeroidService");
                var renlist = service.getcurrentPeroid();
                if (renlist == null)
                {
                    return "0";
                }
                else if (renlist.IsCurrentFlg == "0")
                {
                    return "0";
                }
                else
                {
                    return "1";
                }
            }
        }
        [HttpGet]
        public IQueryable<UploadInfo> getInfo(string reportType)
        {
            //Aging
            PeroidService service = SpringFactory.GetObjectImpl<PeroidService>("PeroidService");
            return service.getCurrentPeroidDateSize(reportType).AsQueryable();
        }

        [HttpGet]
        [PagingQueryable]
        public IQueryable<UploadInfoHis> getInfohis(string history)
        {
            FileService service = SpringFactory.GetObjectImpl<FileService>("FileService");
            return service.getFileUploadHis().AsQueryable();
            
        }

        [HttpGet]
        public string getCurrentPer(string cur)
        {
            PeroidService service = SpringFactory.GetObjectImpl<PeroidService>("PeroidService");
            return service.getcurrentPer();

        }

        [HttpGet]
        [Route("api/dataprepare/getLegalHisByDate")]
        public IQueryable<UploadLegal> getLegalHisByDate(string searchDate)
        {
            ICustomerService service = SpringFactory.GetObjectImpl<ICustomerService>("CustomerService");
            return service.getLegalHisByDate(searchDate).AsQueryable();
        }

        [HttpGet]
        [Route("api/dataprepare/getLegalByDash")]
        public Dictionary<string,bool> getLegalByDash()
        {
            ICustomerService service = SpringFactory.GetObjectImpl<ICustomerService>("CustomerService");
            return service.getLegalByDash();
        }

        [HttpGet]
        [Route("api/dataprepare/getFileHisByDate")]
        public UploadLegalHisModel getFileHisByDate(int pageindex, int pagesize, string searchDate)
        {         
            ICustomerService service = SpringFactory.GetObjectImpl<ICustomerService>("CustomerService");
            UploadLegalHisModel model = new UploadLegalHisModel();
            model = service.getFileHisByDate(pageindex, pagesize, searchDate) ;
            return model;
        }

        [HttpGet]
        [Route("api/dataprepare/getSubmitWaitInvDet")]
        public submitWaitInvDetModel getSubmitWaitInvDet(int pageindex, int pagesize)
        {
            ICustomerService service = SpringFactory.GetObjectImpl<ICustomerService>("CustomerService");
            submitWaitInvDetModel model = new submitWaitInvDetModel();
            model = service.getSubmitWaitInvDet(pageindex, pagesize);
            return model;
        }

        [HttpGet]
        [Route("api/dataprepare/getSubmitWaitVat")]
        public submitWaitVatModel getSubmitWaitVat(int pageindex, int pagesize)
        {
            ICustomerService service = SpringFactory.GetObjectImpl<ICustomerService>("CustomerService");
            submitWaitVatModel model = new submitWaitVatModel();
            model = service.getSubmitWaitVat(pageindex, pagesize);
            return model;
        }

        [HttpPost]
        [Route("api/dataprepare/uploadAg")]
        public string uploadAg(string acc,string inv)
        {
            ICustomerService service = SpringFactory.GetObjectImpl<ICustomerService>("CustomerService");
             return service.uploadAg(acc, inv);  
        }

        [HttpPost]
        [Route("api/dataprepare/uploadVat")]
        public string uploadVat(string vat)
        {
            ICustomerService service = SpringFactory.GetObjectImpl<ICustomerService>("CustomerService");
            return service.uploadVat(vat);
        }
        [HttpGet]
        [Route("api/dataprepare/GetFileFromWebApi")]
        public IHttpActionResult GetFileFromWebApi(string path)
        {
            var browser = String.Empty;
            if (HttpContext.Current.Request.UserAgent != null)
            {
                browser = HttpContext.Current.Request.UserAgent.ToUpper();
            }
            HttpResponseMessage httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
            if (File.Exists(path))
            {
                FileStream fileStream = File.OpenRead(path);
                httpResponseMessage.Content = new StreamContent(fileStream);
                httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("application/ms-excel");
                httpResponseMessage.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") ;
           }
            return ResponseMessage(httpResponseMessage);
        }

        [HttpGet]
        [Route("api/dataprepare/getLegalNewFile")]
        public string getLegalNewFile(string legal, int type)
        {
            string result = string.Empty;
            ICustomerService service = SpringFactory.GetObjectImpl<ICustomerService>("CustomerService");
            result = service.GetLegalNewFile(legal,type);        
            return result;
        }

        [HttpGet]
        [Route("api/dataprepare/batch")]
        public void batch()
        {
            string result = string.Empty;
            ICustomerService service = SpringFactory.GetObjectImpl<ICustomerService>("CustomerService");
            service.Batch();      
        }

    }
}