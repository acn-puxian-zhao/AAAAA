using Intelligent.OTC.Business;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Exceptions;
using Intelligent.OTC.Common.Utils;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace Intelligent.OTC.WebApi.Controllers
{
    public class CaBankFileController : ApiController
    {
        [HttpPost]
        [Route("api/CaBankFileController/UploadFile")]
        public String UploadFile(string bankId, string legalEntity, string transactionNum)
        {
            HttpFileCollection files = HttpContext.Current.Request.Files;
            string strMessage = string.Empty;

            try
            {
                if (files.Count > 0)
                {
                    //判断文件扩展名
                    string strdFileName = Path.GetExtension(files[0].FileName).ToLower();
                    BaseDataService servicebase = SpringFactory.GetObjectImpl<BaseDataService>("BaseDataService");

                    List<SysTypeDetail> list = servicebase.GetSysTypeDetail("098");
                    if (list.Find(o => o.DetailName == strdFileName) == null)
                    {
                        throw new OTCServiceException("Rejected file types! Please contact the system administrator.");
                    }

                    CaBankFileService service = SpringFactory.GetObjectImpl<CaBankFileService>("CaBankFileService");
                    CaBankStatementDto bank = new CaBankStatementDto();
                    bank.ID = bankId;
                    bank.LegalEntity = legalEntity;
                    bank.TRANSACTION_NUMBER = transactionNum;
                    strMessage = service.saveBSFile(files[0], bank);
                }
                else
                {
                    strMessage = "Please upload at list one File.";
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
            catch (OTCServiceException exotc) {
                Helper.Log.Error(exotc.Message, exotc);
                throw new OTCServiceException(exotc.Message);
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw new OTCServiceException("Upload faild." + ex.Message);
            }
        }

        [HttpGet]
        [Route("api/CaBankFileController/GetFilesByBankId")]
        public List<CaBsFileWithPathDto> GetFilesByBankId(string bankId)
        {
            CaBankFileService service = SpringFactory.GetObjectImpl<CaBankFileService>("CaBankFileService");
            var res = service.GetFilesByBankId(bankId);
            return res;
        }

        [HttpGet]
        [Route("api/CaBankFileController/GetFileList")]
        public CaBsFileDtoPage GetFileList(string transactionNum, string fileName, string fileType, string createDateF, string createDateT, int page, int pageSize)
        {
            CaBankFileService service = SpringFactory.GetObjectImpl<CaBankFileService>("CaBankFileService");
            var res = service.GetFileList(transactionNum, fileName, fileType, createDateF, createDateT, page, pageSize);
            return res;
        }

        [HttpGet]
        [Route("api/CaBankFileController/deleteFileById")]
        public void deleteFileById(string fileId)
        {
            try
            {
                CaBankFileService service = SpringFactory.GetObjectImpl<CaBankFileService>("CaBankFileService");
                service.deleteFileById(fileId);
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw new OTCServiceException("Delete files faild." + ex.Message);
            }
        }
    }
}