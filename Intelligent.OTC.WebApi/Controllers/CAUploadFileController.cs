using Intelligent.OTC.Business;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Attr;
using Intelligent.OTC.Common.Exceptions;
using Intelligent.OTC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace Intelligent.OTC.WebApi.Controllers
{
    //[UserAuthorizeFilter(actionSet: "caUploadFile")]
    public class CAUploadFileController : ApiController
    {
        [HttpPost]
        public String UploadFile(string fileType)
        {
            HttpFileCollection files = HttpContext.Current.Request.Files;
            string strMessage = string.Empty;

            try
            {
                if (files.Count > 0)
                {
                    CaCommonService caCommonService = SpringFactory.GetObjectImpl<CaCommonService>("CaCommonService");
                    switch (fileType) {
                        case "updBSFile":
                            string archivePathBankStatement = ConfigurationManager.AppSettings["BankStatementPath"].ToString();
                            archivePathBankStatement = archivePathBankStatement + DateTime.Now.Year.ToString() + "\\" + DateTime.Now.Month.ToString();
                            if (Directory.Exists(archivePathBankStatement) == false)
                            {
                                Directory.CreateDirectory(archivePathBankStatement);
                            }
                            string strTargetFileNameBankStatement = archivePathBankStatement + "\\" + files[0].FileName;
                            strMessage = caCommonService.UploadBankStatementFile(files[0], strTargetFileNameBankStatement);
                            break;
                        case "updRemittanceFile":
                            string archivePathRemittance = ConfigurationManager.AppSettings["RemittancePath"].ToString();
                            archivePathRemittance = archivePathRemittance + DateTime.Now.Year.ToString() + "\\" + DateTime.Now.Month.ToString();
                            if (Directory.Exists(archivePathRemittance) == false)
                            {
                                Directory.CreateDirectory(archivePathRemittance);
                            }
                            string strTargetFileNameRemittance = archivePathRemittance + "\\" + files[0].FileName;
                            strMessage = caCommonService.UploadRemittance(files[0], strTargetFileNameRemittance, files[0].FileName);
                            break;
                        case "updpmtFile":
                            string archivePathpmt = ConfigurationManager.AppSettings["pmtPath"].ToString();
                            archivePathpmt = archivePathpmt + DateTime.Now.Year.ToString() + "\\" + DateTime.Now.Month.ToString();
                            if (Directory.Exists(archivePathpmt) == false)
                            {
                                Directory.CreateDirectory(archivePathpmt);
                            }
                            string strTargetFileNamepmt = archivePathpmt + "\\" + files[0].FileName;
                            strMessage = caCommonService.Uploadpmthis(files[0], strTargetFileNamepmt);
                            break;
                    }
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
                throw new OTCServiceException("Uploaded file error!" + ex.Message);
            }
        }
    }
}
