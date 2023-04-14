using Intelligent.OTC.Business;
using Intelligent.OTC.Business.Interfaces;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Exceptions;
using Intelligent.OTC.Common.Utils;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.DomainModel;
using Intelligent.OTC.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Validation;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;

namespace Intelligent.OTC.WebApi.Controllers
{

    public class CaBankStatementController : ApiController
    {
        [HttpGet]
        [Route("api/caBankStatementController/getCaBankStatementList")]
        public CaBankStatementDtoPage getCaBankStatementList(string ishistory, string statusselect, string legalEntity, string transNumber, string transcurrency, string transamount, string transCustomer, string transaForward, string valueDataF, string valueDataT, string createDateF, string createDateT,string bsType, int page,int pageSize)
        {
            ICaBankStatementService service = SpringFactory.GetObjectImpl<ICaBankStatementService>("CaBankStatementService");
            var res = service.getCaBankStatementList(statusselect, legalEntity,transNumber, transcurrency, transamount, transCustomer, transaForward, valueDataF, valueDataT, createDateF, createDateT, ishistory, bsType, page, pageSize);
            return res;
        }

        [HttpGet]
        [Route("api/caBankStatementController/getCaPmtDetailList")]
        public CaPmtDtoPage getCaPmtDetailList(string groupNo, string legalEntity, string customerNum, string currency, string amount,string transactionNumber, string invoiceNum, string valueDateF, string valueDateT, string createDateF, string createDateT, string isClosed, string hasBS, string hasMatched, string hasInv, int page, int pageSize)
        {
            ICaBankStatementService service = SpringFactory.GetObjectImpl<ICaBankStatementService>("CaBankStatementService");
            var res = service.getCaPmtDetailList(groupNo, legalEntity, customerNum, currency, amount, transactionNumber, invoiceNum, valueDateF, valueDateT, createDateF, createDateT, isClosed, hasBS, hasInv,hasMatched, page, pageSize);
            return res;
        }


        [HttpGet]
        [Route("api/caBankStatementController/getCaPmtDetailListByBsId")]
        public CaPmtDtoPage getCaPmtDetailListByBsId(string bsId)
        {
            ICaBankStatementService service = SpringFactory.GetObjectImpl<ICaBankStatementService>("CaBankStatementService");
            var res = service.getCaPmtDetailListByBsId(bsId);
            return res;
        }

        [HttpGet]
        [Route("api/caBankStatementController/changePMTBsId")]
        public void changePMTBsId(string bsId,string pmtId)
        {
            CaPaymentDetailService service = SpringFactory.GetObjectImpl<CaPaymentDetailService>("CaPaymentDetailService");
            service.changePMTBsId(bsId, pmtId);
        }

        [HttpGet]
        [Route("api/caBankStatementController/deletePmtBsByBsId")]
        public void deletePmtBsByBsId(string bsId)
        {
            CaPaymentDetailService service = SpringFactory.GetObjectImpl<CaPaymentDetailService>("CaPaymentDetailService");
            service.deletePmtBSByBsId(bsId);
        }

        [HttpGet]
        [Route("api/caBankStatementController/getCaPmtBsList")]
        public List<CaPMTBSDto> getCaPmtBsList(string reconid)
        {
            ICaBankStatementService service = SpringFactory.GetObjectImpl<ICaBankStatementService>("CaBankStatementService");
            var res = service.getCaPmtBsListById(reconid);
            return res;
        }

        [HttpGet]
        [Route("api/caBankStatementController/getCaPmtDetailDetailList")]
        public List<CaPMTDetailDto> getCaPmtDetailDetailList(string reconid)
        {
            ICaBankStatementService service = SpringFactory.GetObjectImpl<ICaBankStatementService>("CaBankStatementService");
            var res = service.GetCaPMTDetailListById(reconid);
            return res;
        }

        [HttpGet]
        [Route("api/caBankStatementController/getCaPmtDetailById")]
        public CaPMTDto getCaPmtDetailById(string id)
        {
            ICaPaymentDetailService service = SpringFactory.GetObjectImpl<ICaPaymentDetailService>("CaPaymentDetailService");
            var res = service.getPMTById(id);
            return res;
        }


        [HttpPost]
        [Route("api/caBankStatementController/deletePMTDetailById")]
        public void deletePMTDetailById(CaPMTDto dto)
        {
            try 
            {
                ICaPaymentDetailService paymentService = SpringFactory.GetObjectImpl<ICaPaymentDetailService>("CaPaymentDetailService");
                paymentService.deletePMTById(dto.ID);
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw new OTCServiceException("Delete pmt faild." + ex.Message);
            }
            
        }

        [HttpPost]
        [Route("api/caBankStatementController/deletePMTDetailByIds")]
        public void deletePMTDetailByIds(List<string> ids)
        {
            try
            {
                ICaPaymentDetailService paymentService = SpringFactory.GetObjectImpl<ICaPaymentDetailService>("CaPaymentDetailService");
                foreach(string id in ids)
                {
                    paymentService.deletePMTById(id);
                }
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw new OTCServiceException("Delete pmt faild." + ex.Message);
            }

        }

        [HttpPost]
        [Route("api/caBankStatementController/savePMTDetail")]
        public string savePMTDetail(CaPMTDto dto)
        {
            try
            {
                ICaPaymentDetailService service = SpringFactory.GetObjectImpl<ICaPaymentDetailService>("CaPaymentDetailService");
                var result = service.savePMTDetail(dto);
                return result;
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw new OTCServiceException("Save pmt detail faild." + ex.Message);
            }

        }


        [HttpPost]
        [Route("api/caBankStatementController/updateBank")]
        public void updateBank(CaBankStatementDto dto)
        {
            ICaBankStatementService service = SpringFactory.GetObjectImpl<ICaBankStatementService>("CaBankStatementService");
            service.updateBank(dto);
        }

        [HttpPost]
        [Route("api/caBankStatementController/saveBank")]
        public void saveBank(CaBankStatementDto dto)
        {
            try 
            {
                ICaBankStatementService service = SpringFactory.GetObjectImpl<ICaBankStatementService>("CaBankStatementService");
                    service.saveBank(dto);
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw new OTCServiceException("Save Bank Statement failure." + ex.Message);
            }          
        }

        [HttpPost]
        [Route("api/caBankStatementController/deleteBank")]
        public void deleteBank(CaBankStatementDto dto)
        {
            try
            {
                ICaBankStatementService service = SpringFactory.GetObjectImpl<ICaBankStatementService>("CaBankStatementService");
                service.deleteBank(dto.ID);
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw new OTCServiceException("Delete bank faild." + ex.Message);
            }
        } 
        
        [HttpPost]
        [Route("api/caBankStatementController/revert")]
        public string revert(CaBankStatementDto dto)
        {
            try
            {
                ICaBankStatementService service = SpringFactory.GetObjectImpl<ICaBankStatementService>("CaBankStatementService");
                return service.revert(dto);
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw new OTCServiceException("Revert faild." + ex.Message);
            }
        }

        [HttpGet]
        [Route("api/caBankStatementController/isExistedTransactionNum")]
        public int isExistedTransactionNum(string bankId, string transactionNum)
        {
            try
            {
                ICaBankStatementService service = SpringFactory.GetObjectImpl<ICaBankStatementService>("CaBankStatementService");
                int count = service.isExistedTransactionNum(bankId, transactionNum);
                return count;
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw new OTCServiceException("Check TransactionNum faild." + ex.Message);
            }
        }


        [HttpPost]
        [Route("api/caBankStatementController/identifyCustomer")]
        public void identifyCustomer(string[] bankIds)
        {
            CaBankStatementService service = SpringFactory.GetObjectImpl<CaBankStatementService>("CaBankStatementService");
            List<string> bsIds = service.filterUnlockBankIds(bankIds);
            service.identifyCustomer(bsIds,3);
        }


        [HttpPost]
        [Route("api/caBankStatementController/unknownCashAdvisor")]
        public void unknownCashAdvisor(string[] bankIds)
        {
            CaBankStatementService service = SpringFactory.GetObjectImpl<CaBankStatementService>("CaBankStatementService");
            List<string> avaBsIds = service.filterAvailableBankIds(bankIds);
            if(avaBsIds.Count == 0)
            {
                // 抛出提示信息
                throw new OTCServiceException("There is no bank statements to unknown!");
            }
            service.pmtUnknownCashAdvisor(avaBsIds, "unknown");
            List<string> bsIds = service.filterUnKnownBankIds(bankIds);
            if(bsIds.Count > 0)
            {
                service.unknownCashAdvisor(bsIds, "");
            }
        }


        [HttpPost]
        [Route("api/caBankStatementController/recon")]
        public void recon(string[] bankIds)
        {
            CaBankStatementService service = SpringFactory.GetObjectImpl<CaBankStatementService>("CaBankStatementService");
            List<string> bsIds = service.filterUnmatchBankIds(bankIds);
            service.recon(bsIds,"",AppContext.Current.User.EID);
        }

        [HttpPost]
        [Route("api/caBankStatementController/GetFileFromWebApi")]
        public HttpResponseMessage GetFileFromWebApi(string[] pathgroup)
        {
            if (pathgroup[0].IndexOf("../") >= 0 || pathgroup[0].IndexOf("％5c") >= 0 || pathgroup[0].IndexOf("％00") >= 0) {
                return null;
            }

            var browser = String.Empty;
            if (HttpContext.Current.Request.UserAgent != null)
            {
                browser = HttpContext.Current.Request.UserAgent.ToUpper();
            }
            string strFileName = Path.GetFileName(pathgroup[0]);
            MediaTypeHeaderValue _mediaType = MediaTypeHeaderValue.Parse("application/octet-stream");//指定文件类型
            ContentDispositionHeaderValue _disposition = ContentDispositionHeaderValue.Parse("attachment;filename=" + System.Web.HttpUtility.UrlEncode(strFileName));//指定文件名称（编码中文）
            
            HttpResponseMessage httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
            if (File.Exists(pathgroup[0]))
            {
                FileStream fileStream = new FileStream(pathgroup[0], FileMode.Open);
                HttpResponseMessage fullResponse = Request.CreateResponse(HttpStatusCode.OK);
                fullResponse.Content = new StreamContent(fileStream);

                fullResponse.Content.Headers.ContentType = _mediaType;
                fullResponse.Content.Headers.ContentDisposition = _disposition;
                return fullResponse;
            }
            return null;
        }

        [HttpPost]
        [Route("api/caBankStatementController/GetFileFromWebApiById")]
        public HttpResponseMessage GetFileFromWebApiById(string[] pathgroup)
        {

            CaBankFileService service = SpringFactory.GetObjectImpl<CaBankFileService>("CaBankFileService");
            var res = service.GetFilesByBankIdWithPath(pathgroup[0]);
            if (res == null || res.Count == 0) { return null; }

            string strFilePath = res[0].PHYSICAL_PATH;

            var browser = String.Empty;
            if (HttpContext.Current.Request.UserAgent != null)
            {
                browser = HttpContext.Current.Request.UserAgent.ToUpper();
            }
            string strFileName = Path.GetFileName(strFilePath);
            MediaTypeHeaderValue _mediaType = MediaTypeHeaderValue.Parse("application/octet-stream");//指定文件类型
            ContentDispositionHeaderValue _disposition = ContentDispositionHeaderValue.Parse("attachment;filename=" + System.Web.HttpUtility.UrlEncode(strFileName));//指定文件名称（编码中文）

            HttpResponseMessage httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
            if (File.Exists(strFilePath))
            {
                FileStream fileStream = new FileStream(strFilePath, FileMode.Open);
                HttpResponseMessage fullResponse = Request.CreateResponse(HttpStatusCode.OK);
                fullResponse.Content = new StreamContent(fileStream);

                fullResponse.Content.Headers.ContentType = _mediaType;
                fullResponse.Content.Headers.ContentDisposition = _disposition;
                return fullResponse;
            }
            return null;
        }

        [HttpPost]
        [Route("api/caBankStatementController/GetFileByFileId")]
        public HttpResponseMessage GetFileByFileId(string[] pathgroup)
        {
            //判断用户是否有BankStatement权限
            ICaBankStatementService service = SpringFactory.GetObjectImpl<ICaBankStatementService>("CaBankStatementService");
            
            FileDto fileDto = service.getFileById(pathgroup[0]);
            string path = "";
            if (fileDto != null)
            {
                path = fileDto.PhysicalPath;
            }

            var browser = String.Empty;
            if (HttpContext.Current.Request.UserAgent != null)
            {
                browser = HttpContext.Current.Request.UserAgent.ToUpper();
            }
            string strFileName = Path.GetFileName(path);
            MediaTypeHeaderValue _mediaType = MediaTypeHeaderValue.Parse("application/octet-stream");//指定文件类型
            ContentDispositionHeaderValue _disposition = ContentDispositionHeaderValue.Parse("attachment;filename=" + System.Web.HttpUtility.UrlEncode(strFileName));//指定文件名称（编码中文）

            HttpResponseMessage httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
            if (File.Exists(path))
            {
                FileStream fileStream = new FileStream(path, FileMode.Open);
                HttpResponseMessage fullResponse = Request.CreateResponse(HttpStatusCode.OK);
                fullResponse.Content = new StreamContent(fileStream);

                fullResponse.Content.Headers.ContentType = _mediaType;
                fullResponse.Content.Headers.ContentDisposition = _disposition;
                return fullResponse;
            }
            return null;
        }

        [HttpGet]
        [Route("api/caBankStatementController/allAgentCustomer")]
        public CustomerMenuDtoPage allAgentCustomerDataDetails (int page, int pageSize ,string legalEntity)
        {

            ICaBankStatementService service = SpringFactory.GetObjectImpl<ICaBankStatementService>("CaBankStatementService");
            var res = service.allAgentCustomerDataDetails(page, pageSize, legalEntity);
            return res;
        }
 

 
 

        [HttpGet]
        [Route("api/caBankStatementController/likeAgentCustomer")]
        public CustomerMenuDtoPage likeAgentCustomerDataDetails(int page, int pageSize, string bankid)
        {
            ICaBankStatementService service = SpringFactory.GetObjectImpl<ICaBankStatementService>("CaBankStatementService");
            var res = service.likeAgentCustomerDataDetails(page, pageSize, bankid);
            return res;
        }
 

 
 

        [HttpGet]
 

        [Route("api/caBankStatementController/likePaymentCustomer")]
 

        public CustomerMenuDtoPage likePaymentCustomerDataDetails(int page, int pageSize, string bankid)
 

        {
 

            ICaBankStatementService service = SpringFactory.GetObjectImpl<ICaBankStatementService>("CaBankStatementService");
 

            var res = service.likePaymentCustomerDataDetails(page, pageSize, bankid);
 

            return res;
 

        }


        [HttpGet]
 

        [Route("api/caBankStatementController/ArHisDataDetails")]
 

        public CaARViewDtoAndAmtTotal getArHisDataDetails(string customerNum, string legalEntity)
        {
            ICaBankStatementService service = SpringFactory.GetObjectImpl<ICaBankStatementService>("CaBankStatementService");
            var res = service.getArHisDataDetails(customerNum, legalEntity);
            return res;
        }


        [HttpGet]
        [Route("api/caBankStatementController/GetBankStatementByTranINC")]
        public CaBankStatementDto GetBankStatementByTranINC(string transactionNumber)
        {
            ICaPaymentDetailService service = SpringFactory.GetObjectImpl<ICaPaymentDetailService>("CaPaymentDetailService");
            var res = service.GetBankStatementByTranINC(transactionNumber);
            return res;
        }

        [HttpGet]
        [Route("api/caBankStatementController/GetInvoiceInfoByNum")]
        public CaPMTDetailDto GetInvoiceInfoByNum(string invoiceNum)
        {
            ICaPaymentDetailService service = SpringFactory.GetObjectImpl<ICaPaymentDetailService>("CaPaymentDetailService");
            var res = service.GetInvoiceInfoByNum(invoiceNum);
            return res;
        }


        [HttpPost]


        [Route("api/caBankStatementController/needSendMail")]


        public void changeNeedSendMail(CustomerMenuDto data)


        {


            ICaBankStatementService service = SpringFactory.GetObjectImpl<ICaBankStatementService>("CaBankStatementService");


            service.changeNeedSendMail(data.Id, data.NeedSendMail ?? false);


        }


        [HttpPost]


        [Route("api/caBankStatementController/allNeedSendMail")]

        public void changeNeedSendMailAll(CustomerMenuDto data)


        {


            ICaBankStatementService service = SpringFactory.GetObjectImpl<ICaBankStatementService>("CaBankStatementService");


            service.changeNeedSendMailAll(data.BankStatementId, data.NeedSendMail ?? false);


        }


        [HttpPost]
        [Route("api/caBankStatementController/sendMail")]
        public void sendMail(CaBankCustomerDto dto)
        {
            try
            {
                IMailSendService service = SpringFactory.GetObjectImpl<IMailSendService>("MailSendService");
                List<CustomerMenuDto> customerList = likePaymentCustomer(dto.bank.ID);
                foreach (CustomerMenuDto c in customerList)
                {
                    if ((c.NeedSendMail == null ? false : Convert.ToBoolean(c.NeedSendMail)) 
                        && string.IsNullOrEmpty(c.MailId)) { 
                        service.sendCustomerBankMail(dto.bank, c);
                    }
                }
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw new OTCServiceException("Send mail faild." + ex.Message);
            }

        }

        [HttpGet]
        [Route("api/caBankStatementController/getIdentifyList")]
        public CaBankStatementDtoPage getIdentifyList(string taskId, int page, int pageSize)
        {
            ICaBankStatementService service = SpringFactory.GetObjectImpl<ICaBankStatementService>("CaBankStatementService");
            var res = service.getBankHistoryListByTaskType(taskId, "3", page, pageSize);//indentify
            return res;
        }
        

        [HttpGet]
        [Route("api/caBankStatementController/getAdvisortList")]
        public CaBankStatementDtoPage getAdvisortList(string taskId, int page, int pageSize)
        {
            ICaBankStatementService service = SpringFactory.GetObjectImpl<ICaBankStatementService>("CaBankStatementService");
            var res = service.getBankHistoryListByTaskType(taskId, "4",page, pageSize);//unkown
            return res;
        }


        [HttpGet]
        [Route("api/unknownAdjustment/likePaymentCustomer")]


        public List<CustomerMenuDto> likePaymentCustomer(string bankid)


        {
            ICaBankStatementService service = SpringFactory.GetObjectImpl<ICaBankStatementService>("CaBankStatementService");
            var res = service.likePaymentCustomer(bankid);
            return res;
        }

        [HttpGet]
        [Route("api/caBankStatementController/allPaymentCustomer")]
        public CustomerMenuDtoPage allPaymentCustomerDataDetails(int page, int pageSize, string legalEntity)
        {
            ICaBankStatementService service = SpringFactory.GetObjectImpl<ICaBankStatementService>("CaBankStatementService");
            var res = service.allPaymentCustomerDataDetails(page, pageSize, legalEntity);
            return res;
        }

        [HttpGet]
        [Route("api/caBankStatementController/UploadPMTNoFile")]
        public String UploadPMTNoFile(string fileId)
        {
            string strMessage = string.Empty;
            try
            {
                CaCommonService caCommonService = SpringFactory.GetObjectImpl<CaCommonService>("CaCommonService");
                strMessage = caCommonService.UploadPMTDetailByFileId(fileId);
                return strMessage;
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw new OTCServiceException("Upload pmt faild." + ex.Message);
            }
        }

        [HttpGet]
        [Route("api/caBankStatementController/GetBankByTaskId")]
        public List<CaBankStatementDto> GetBankByTaskId(string taskId)
        {
            ICaBankStatementService service = SpringFactory.GetObjectImpl<ICaBankStatementService>("CaBankStatementService");
            var res = service.GetBankByTaskId(taskId);
            return res;
        }

        [HttpGet]
        [Route("api/caBankStatementController/GetBankByTranc")]
        public List<CaBankStatementDto> GetBankByTranc(string transactionNum)
        {
            ICaBankStatementService service = SpringFactory.GetObjectImpl<ICaBankStatementService>("CaBankStatementService");
            var res = service.GetBankByTranc(transactionNum);
            return res;
        }

        [HttpGet]
        [Route("api/caBankStatementController/exporBankStatementAll")]
        public HttpResponseMessage ExporAll(string ishistory, string statusselect, string legalEntity, string transNumber, string transcurrency, string transamount, string transCustomer, string transaForward, string valueDataF, string valueDataT, string createDateF, string createDateT, string bsType)
        {
            ICaBankStatementService service = SpringFactory.GetObjectImpl<ICaBankStatementService>("CaBankStatementService");
            return service.exporBankStatementAll(statusselect, legalEntity, transNumber, transcurrency, transamount, transCustomer, transaForward, valueDataF, valueDataT, createDateF, createDateT, ishistory, bsType);
        }

        [HttpGet]
        [Route("api/caBankStatementController/exporPmtDetail")]
        public HttpResponseMessage exporPmtDetail(string groupNo, string legalEntity, string customerNum, string currency, string amount, string transactionNumber, string invoiceNum, string valueDateF, string valueDateT, string createDateF, string createDateT, string isClosed, string hasBS, string hasMatched, string hasInv)
        {
            ICaBankStatementService service = SpringFactory.GetObjectImpl<ICaBankStatementService>("CaBankStatementService");
            return service.exporPmtDetail(groupNo, legalEntity, customerNum, currency, amount, transactionNumber, invoiceNum, valueDateF, valueDateT, createDateF, createDateT, isClosed, hasBS, hasInv, hasMatched);
        }


        [HttpGet]


        [Route("api/caBankStatementController/reconArHisDataDetails")]


        public CaARViewDtoAndAmtTotal getReconArHisDataDetails(string reconId)


        {
            ICaBankStatementService service = SpringFactory.GetObjectImpl<ICaBankStatementService>("CaBankStatementService");
            var res = service.getReconArHisDataDetails(reconId);
     
            return res;
        }


        [HttpDelete]
        [Route("api/pmt/deletePmtBs")]
        public void DeletePmtBs(string id)
        {
            ICaBankStatementService service = SpringFactory.GetObjectImpl<ICaBankStatementService>("CaBankStatementService");
            service.RemovePmtBs(id);

            ICaReconService reconService = SpringFactory.GetObjectImpl<ICaReconService>("CaReconService");
            string reconId = reconService.getReconIdByBsId(id);
            reconService.deleteReconGroupByReconId(reconId);
        }

        [HttpPost]
        [Route("api/caBankStatementController/autoRecon")]
        public void autoRecon(string ishistory, string statusselect, string legalEntity, string transNumber, string transcurrency, string transamount, string transCustomer, string transaForward, string valueDataF, string valueDataT, string createDateF, string createDateT, string bsType)
        {
            CaBankStatementService service = SpringFactory.GetObjectImpl<CaBankStatementService>("CaBankStatementService");
            List<string> bsIds = service.getAllAvailableBSIds(statusselect, legalEntity, transNumber, transcurrency, transamount, transCustomer, transaForward, valueDataF, valueDataT, ishistory, createDateF, createDateT, bsType);
            service.identifyCustomer(bsIds,7);
        }

        [HttpPost]
        [Route("api/caBankStatementController/batchChangeINC")]
        public String batchChangeINC()
        {
            CaBankStatementService service = SpringFactory.GetObjectImpl<CaBankStatementService>("CaBankStatementService");

            HttpFileCollection files = HttpContext.Current.Request.Files;
            string strMessage = string.Empty;

            try
            {
                if (files.Count > 0)
                {
                    strMessage = service.UploadBatchChangeINC();
                }
                return strMessage;
            }
            catch (DbEntityValidationException dbex)
            {
                if (dbex.EntityValidationErrors != null)
                {
                    foreach (var error in dbex.EntityValidationErrors)
                    {
                        Helper.Log.Error(error, dbex);
                    }
                }
                throw new OTCServiceException("Uploaded file error!");
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw new OTCServiceException("Change INC faild." + ex.Message);
            }
        }

        [HttpPost]
        [Route("api/caBankStatementController/batchManualClose")]
        public String batchManualClose()
        {
            CaBankStatementService service = SpringFactory.GetObjectImpl<CaBankStatementService>("CaBankStatementService");

            HttpFileCollection files = HttpContext.Current.Request.Files;
            string strMessage = string.Empty;

            try
            {
                if (files.Count > 0)
                {
                    strMessage = service.UploadBatchManualClose();
                }
                return strMessage;
            }
            catch (DbEntityValidationException dbex)
            {
                if (dbex.EntityValidationErrors != null)
                {
                    foreach (var error in dbex.EntityValidationErrors)
                    {
                        Helper.Log.Error(error, dbex);
                    }
                }
                throw new OTCServiceException("Uploaded file error!");
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw new OTCServiceException("Manual Close faild." + ex.Message);
            }
        }

        [HttpPost]
        [Route("api/caBankStatementController/reuploadPost")]
        public String reuploadPost()
        {
            CaBankStatementService service = SpringFactory.GetObjectImpl<CaBankStatementService>("CaBankStatementService");

            HttpFileCollection files = HttpContext.Current.Request.Files;
            string strMessage = string.Empty;

            try
            {
                if (files.Count > 0)
                {
                    strMessage = service.reuploadPost();
                }
                if (string.IsNullOrEmpty(strMessage))
                {
                    return "Operate Success!";
                }
                else
                {
                    return strMessage;
                }
            }
            catch (DbEntityValidationException dbex)
            {
                if (dbex.EntityValidationErrors != null)
                {
                    foreach (var error in dbex.EntityValidationErrors)
                    {
                        Helper.Log.Error(error, dbex);
                    }
                }
                throw new OTCServiceException("Uploaded file error!");
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw new OTCServiceException("Uploaded file error!" + ex.Message);
            }
        }


        [HttpPost]
        [Route("api/caBankStatementController/reuploadPostClear")]
        public String reuploadPostClear()
        {
            CaBankStatementService service = SpringFactory.GetObjectImpl<CaBankStatementService>("CaBankStatementService");

            HttpFileCollection files = HttpContext.Current.Request.Files;
            string strMessage = string.Empty;

            try
            {
                if (files.Count > 0)
                {
                    strMessage = service.reuploadPostClear();
                }
                if (string.IsNullOrEmpty(strMessage))
                {
                    return "Operate Success!";
                }
                else
                {
                    return strMessage;
                }
            }
            catch (DbEntityValidationException dbex)
            {
                if (dbex.EntityValidationErrors != null)
                {
                    foreach (var error in dbex.EntityValidationErrors)
                    {
                        Helper.Log.Error(error, dbex);
                    }
                }
                throw new OTCServiceException("Uploaded file error!");
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw new OTCServiceException("Uploaded file error!" + ex.Message);
            }
        }

        [HttpPost]
        [Route("api/caBankStatementController/ignore")]
        public void ignore(string[] bankIds)
        {
            CaBankStatementService service = SpringFactory.GetObjectImpl<CaBankStatementService>("CaBankStatementService");
            service.ignore(bankIds);
        }  
        
        [HttpPost]
        [Route("api/caBankStatementController/unlock")]
        public void unlock(string[] bankIds)
        {
            CaBankStatementService service = SpringFactory.GetObjectImpl<CaBankStatementService>("CaBankStatementService");
            service.unlock(bankIds);
        }  
        
        [HttpPost]
        [Route("api/caBankStatementController/batchDelete")]
        public void batchDelete(string[] bankIds)
        {
            CaBankStatementService service = SpringFactory.GetObjectImpl<CaBankStatementService>("CaBankStatementService");
            service.batchDelete(bankIds);
        }

        [HttpPost]
        [Route("api/caBankStatementController/doExportUnknownDataByIds")]
        public string doExportUnknownDataByIds(List<CaBankStatementDto> banks)
        {

            try
            {
                CaBankStatementService service = SpringFactory.GetObjectImpl<CaBankStatementService>("CaBankStatementService");
                FileDto dto = service.doExportUnknownDataByIds(banks);
                return dto.FileId;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        [HttpPost]
        [Route("api/caBankStatementController/pmtUnknownCashAdvisor")]
        public void pmtUnknownCashAdvisor(string[] bankIds)
        {
            CaBankStatementService service = SpringFactory.GetObjectImpl<CaBankStatementService>("CaBankStatementService");
            List<string> bsIds = service.filterAvailableBankIds(bankIds);
            service.pmtUnknownCashAdvisor(bsIds, "");
        }

        [HttpGet]
        [Route("api/caBankStatementController/cancelCaMailAlertbyid")]
        public void cancelCaMailAlertbyid(string id) {

            CaBankStatementService service = SpringFactory.GetObjectImpl<CaBankStatementService>("CaBankStatementService");
            service.cancelCaMailAlertbyid(id);
        }

        [HttpGet]
        [Route("api/caBankStatementController/getCaMailAlertListbybsid")]
        public CaMailAlertDtoPage getCaMailAlertListbybsid(string bsid, string alertType, int page, int pageSize)
        {
            ICaBankStatementService service = SpringFactory.GetObjectImpl<ICaBankStatementService>("CaBankStatementService");
            var res = service.getCaMailAlertListbybsid(bsid, alertType, page, pageSize);
            return res;
        }


    }
}
