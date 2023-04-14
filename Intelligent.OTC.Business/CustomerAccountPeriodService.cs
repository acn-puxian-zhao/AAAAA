using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.Repositories;
using Intelligent.OTC.Business.Interfaces;
using Intelligent.OTC.Common.Utils;
using System.Web;
using System.Configuration;
using System.IO;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Exceptions;

namespace Intelligent.OTC.Business
{
    public class CustomerAccountPeriodService : ICustomerAccountPeriodService
    {
        public OTCRepository CommonRep { get; set; }

        #region Get AccountPeriod By NumAndSiteUseId
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IQueryable<T_Customer_AccountPeriod> GetByNumAndSiteUseId(T_Customer_AccountPeriod customerAccountPeriod)
        {
            var result = CommonRep.GetQueryable<T_Customer_AccountPeriod>().Where(o => o.CUSTOMER_NUM == customerAccountPeriod.CUSTOMER_NUM
            && o.SiteUseId == customerAccountPeriod.SiteUseId);
            return result;
        }
        #endregion

        private IQueryable<T_Customer_AccountPeriod> getAccountPeriod()
        {
            return CommonRep.GetQueryable<T_Customer_AccountPeriod>();
        }


        #region Save AccountPeriod
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string SaveAccountPeriod(T_Customer_AccountPeriod customerAccountPeriod,string isAdd)
        {
            var allAccountPeriod = getAccountPeriod();

            var result = allAccountPeriod.Where(o => o.CUSTOMER_NUM == customerAccountPeriod.CUSTOMER_NUM
            && o.SiteUseId == customerAccountPeriod.SiteUseId && o.AccountYear == customerAccountPeriod.AccountYear
            && o.AccountMonth == customerAccountPeriod.AccountMonth);

            if (isAdd.Equals("1"))
            {
                //该客户的账期年月已经存在
                if (result.Count() > 0)
                {
                    return "Account Period exist";
                }
                else
                {
                    CommonRep.Add(customerAccountPeriod);
                    CommonRep.Commit();
                    return "Add Success!";
                }
            }
            else
            {
                T_Customer_AccountPeriod old = CommonRep.FindBy<T_Customer_AccountPeriod>(customerAccountPeriod.Id);
                ObjectHelper.CopyObjectWithUnNeed(customerAccountPeriod, old, new string[] { "Id" });
                CommonRep.Commit();
                return "Update Success!";
            }
        }

        /// <summary>
        /// Delete AccountPeriod for the given Id
        /// </summary>
        /// <param name="id"></param>
        public void DeleteAccountPeriod(int id)
        {
            AssertUtils.IsTrue(id > 0, "AccountPeriod Id");

            T_Customer_AccountPeriod old = CommonRep.FindBy<T_Customer_AccountPeriod>(id);
            if (old != null)
            {
                CommonRep.Remove(old);
                CommonRep.Commit();
            }
        }

        #endregion


        public string ImportAccountPeriod()
        {
            FileType fileT = FileType.AccountPeriod;
            string archivePath = string.Empty;
            string archiveFileName = string.Empty;
            try
            {
                //upload file to server
                string strMasterDataKey = "ImportAccountPeriod";
                HttpFileCollection files = HttpContext.Current.Request.Files;
                archivePath = ConfigurationManager.AppSettings[strMasterDataKey].ToString();
                archivePath = archivePath + DateTime.Now.Year.ToString() + "\\" + DateTime.Now.Month.ToString();
                if (Directory.Exists(archivePath) == false)
                {
                    Directory.CreateDirectory(archivePath);
                }
                archiveFileName = archivePath + "\\" + DateTime.Now.ToString("yyyy-MM-dd") + "-" + fileT.ToString() +
                            "-" + AppContext.Current.User.EID + "-" + DateTime.Now.ToString("HHmmssf") + ".xlsx";

                FileService service = SpringFactory.GetObjectImpl<FileService>("FileService");
                service.UploadFile(files[0], archiveFileName, fileT);

                return ImportAP();
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw new OTCServiceException("Uploaded file failed!");
            }
        }

        public string ImportAP()
        {
            string strCode = "";
            FileUploadHistory fileUpHis = new FileUploadHistory();
            FileService fileService = SpringFactory.GetObjectImpl<FileService>("FileService");

            try
            {
                strCode = Helper.EnumToCode<FileType>(FileType.AccountPeriod);
                fileUpHis = fileService.GetSuccessData(strCode);
                string strpath = "";
                T_Customer_AccountPeriod customerAccountPeriod = new T_Customer_AccountPeriod();

                IQueryable<T_Customer_AccountPeriod> APlist = getAccountPeriod();
                T_Customer_AccountPeriod cap = new T_Customer_AccountPeriod();
                if (fileUpHis == null)
                {
                    Exception ex = new Exception("import file is not found!");
                    Helper.Log.Error(ex.Message, ex);
                    throw ex;
                }
                strpath = fileUpHis.ArchiveFileName;
                NpoiHelper helper = new NpoiHelper(strpath);
                string sheetName = "";
                sheetName = "AccountPeriod";
                helper.ActiveSheetName = sheetName;
                int i = 1;
                //added by zhangYu 20160128
                List<T_Customer_AccountPeriod> listAP = new List<T_Customer_AccountPeriod>();

                //when excel have one row
                while (helper.GetValue(i, 1) != null)
                {
                    //get value from excel
                    var num = helper.GetValue(i, 0).ToString();
                    var siteUseId = helper.GetValue(i, 1).ToString();
                    var year = Convert.ToInt32(helper.GetValue(i, 2).ToString());
                    var month = Convert.ToInt32(helper.GetValue(i, 3).ToString());
                    cap = APlist.Where(o => o.CUSTOMER_NUM == num && o.SiteUseId == siteUseId
                    && o.AccountYear == year && o.AccountMonth == month).FirstOrDefault();
                    if (cap == null)
                    {
                        T_Customer_AccountPeriod newCAP = new T_Customer_AccountPeriod();

                        if (helper.GetValue(i, 0) != null)
                        {
                            newCAP.CUSTOMER_NUM = helper.GetValue(i, 0).ToString();
                        }
                        else
                        {
                            newCAP.CUSTOMER_NUM = "";
                        }

                        if (helper.GetValue(i, 1) != null)
                        {
                            newCAP.SiteUseId = helper.GetValue(i, 1).ToString();
                        }
                        else
                        {
                            newCAP.SiteUseId = "";
                        }

                        if (helper.GetValue(i, 2) != null)
                        {
                            newCAP.AccountYear = Convert.ToInt32(helper.GetValue(i, 2).ToString());
                        }
                        else
                        {
                            newCAP.AccountYear = 0;
                        }

                        if (helper.GetValue(i, 3) != null)
                        {
                            newCAP.AccountMonth = Convert.ToInt32(helper.GetValue(i, 3).ToString());
                        }
                        else
                        {
                            newCAP.AccountMonth = 0;
                        }

                        if (helper.GetValue(i, 4) != null)
                        {
                            newCAP.ReconciliationDay = Convert.ToInt32(helper.GetValue(i, 4).ToString());
                        }
                        else
                        {
                            newCAP.ReconciliationDay = 0;
                        }


                        listAP.Add(newCAP);

                    }
                    else
                    {
                        var old = cap;
                        if (helper.GetValue(i, 0) != null)
                        {
                            cap.CUSTOMER_NUM = helper.GetValue(i, 0).ToString();
                        }
                        else
                        {
                            cap.CUSTOMER_NUM = "";
                        }

                        if (helper.GetValue(i, 1) != null)
                        {
                            cap.SiteUseId = helper.GetValue(i, 1).ToString();
                        }
                        else
                        {
                            cap.SiteUseId = "";
                        }

                        if (helper.GetValue(i, 2) != null)
                        {
                            cap.AccountYear = Convert.ToInt32(helper.GetValue(i, 2).ToString());
                        }
                        else
                        {
                            cap.AccountYear = 0;
                        }

                        if (helper.GetValue(i, 3) != null)
                        {
                            cap.AccountMonth = Convert.ToInt32(helper.GetValue(i, 3).ToString());
                        }
                        else
                        {
                            cap.AccountMonth = 0;
                        }

                        if (helper.GetValue(i, 4) != null)
                        {
                            cap.ReconciliationDay = Convert.ToInt32(helper.GetValue(i, 4).ToString());
                        }
                        else
                        {
                            cap.ReconciliationDay = 0;
                        }
                        ObjectHelper.CopyObjectWithUnNeed(cap, old, new string[] { "Id", "CustomerNum" });
                    }
                    i = i + 1;
                }

                CommonRep.AddRange(listAP);
                CommonRep.Commit();
                return "Import Finished!";
            }
            catch (Exception ex)
            {
                fileUpHis.ProcessFlag = Helper.EnumToCode<UploadStates>(UploadStates.Failed);
                fileService.CommonRep.Commit();
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }
    }
}
