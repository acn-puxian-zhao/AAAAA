using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Exceptions;
using Intelligent.OTC.Common.Utils;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace Intelligent.OTC.Business
{
    public class CustomerPaymentCircleService
    {
        public OTCRepository CommonRep { get; set; }
        public IBaseDataService BDService { get; set; }
        public const string strPaymentDateCircleKey = "PaymentDateCirclePath";

        public IList<CustomerPaymentCircle> GetCustomerPaymentCircle()
        {
            return CommonRep.GetQueryable<CustomerPaymentCircle>().Where(o => o.Deal == AppContext.Current.User.Deal).ToList();
        }

        public IList<CustomerPaymentCircle> GetCustPaymentCircle(string strCustNum)
        {

            return CommonRep.GetQueryable<CustomerPaymentCircle>().Where(o => o.CustomerNum == strCustNum && o.Deal == AppContext.Current.User.Deal).ToList();

        }

        public IList<CustomerPaymentCircle> GetCustPaymentCircle(string strCustNum,string siteUseId)
        {
            DateTime dt_monthFirstDay = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM") + "-01 00:00:00");
            return CommonRep.GetQueryable<CustomerPaymentCircle>().Where(o => o.CustomerNum == strCustNum 
            && o.Deal == AppContext.Current.User.Deal && o.SiteUseId == siteUseId).Where(o=>o.Reconciliation_Day >= dt_monthFirstDay).OrderBy(o=>o.Reconciliation_Day).ToList();

        }

        public IQueryable<CustomerPaymentCircle> GetCircleByCondtion(string custNum,string legal) 
        {
            IQueryable<CustomerPaymentCircle> cusPaymentcircleList;
            if (legal == "null" || string.IsNullOrEmpty(legal) || legal=="undefined")
            {
                 cusPaymentcircleList = CommonRep.GetQueryable<CustomerPaymentCircle>().Where(o => o.CustomerNum == custNum && o.Deal == AppContext.Current.User.Deal);
            }
            else {
                 cusPaymentcircleList = CommonRep.GetQueryable<CustomerPaymentCircle>().Where(o => o.CustomerNum == custNum && o.Deal == AppContext.Current.User.Deal && o.LegalEntity == legal);
            }
            List<CustomerPaymentCircleP> CustomerPaymentcircleList = new List<CustomerPaymentCircleP>();
            CustomerPaymentCircleP cuspaycircle = new CustomerPaymentCircleP();
            int ind = 0;
            foreach (var item in cusPaymentcircleList)
            {
                ind++;
                cuspaycircle = new CustomerPaymentCircleP();
                cuspaycircle.Id = item.Id;
                cuspaycircle.PaymentDay = item.PaymentDay;
                cuspaycircle.weekDay = (item.PaymentDay.HasValue ? item.PaymentDay.Value.DayOfWeek.ToString() : "");
                cuspaycircle.Flg = item.Flg;
                cuspaycircle.Description = item.Description;
                cuspaycircle.CustomerNum = item.CustomerNum;
                cuspaycircle.CreatePersonId = item.CreatePersonId;
                cuspaycircle.CreateDate = item.CreateDate;
                cuspaycircle.LegalEntity = item.LegalEntity;
                cuspaycircle.sortId = ind;
                CustomerPaymentcircleList.Add(cuspaycircle);
            }
            return CustomerPaymentcircleList.AsQueryable<CustomerPaymentCircle>();
        }

        public IQueryable<CustomerPaymentCircle> GetCircleByCondtion(string custNum,string siteUseId, string legal)
        {
            IQueryable<CustomerPaymentCircle> cusPaymentcircleList;
            if (legal == "null" || string.IsNullOrEmpty(legal) || legal == "undefined")
            {
                cusPaymentcircleList = CommonRep.GetQueryable<CustomerPaymentCircle>().Where(o => o.CustomerNum == custNum 
                && o.SiteUseId == siteUseId && o.Deal == AppContext.Current.User.Deal);
            }
            else
            {
                cusPaymentcircleList = CommonRep.GetQueryable<CustomerPaymentCircle>().Where(o => o.CustomerNum == custNum && o.Deal == AppContext.Current.User.Deal 
                && o.SiteUseId == siteUseId && o.LegalEntity == legal);
            }
            List<CustomerPaymentCircleP> CustomerPaymentcircleList = new List<CustomerPaymentCircleP>();
            CustomerPaymentCircleP cuspaycircle = new CustomerPaymentCircleP();
            int ind = 0;
            foreach (var item in cusPaymentcircleList)
            {
                ind++;
                cuspaycircle = new CustomerPaymentCircleP();
                cuspaycircle.Id = item.Id;
                cuspaycircle.PaymentDay = item.PaymentDay;
                cuspaycircle.Reconciliation_Day = item.Reconciliation_Day;
                cuspaycircle.weekDay = (item.PaymentDay.HasValue ? item.PaymentDay.Value.DayOfWeek.ToString() : item.Reconciliation_Day.Value.DayOfWeek.ToString());
                cuspaycircle.Flg = item.Flg;
                cuspaycircle.Description = item.Description;
                cuspaycircle.CustomerNum = item.CustomerNum;
                cuspaycircle.CreatePersonId = item.CreatePersonId;
                cuspaycircle.CreateDate = item.CreateDate;
                cuspaycircle.LegalEntity = item.LegalEntity;
                cuspaycircle.sortId = ind;
                CustomerPaymentcircleList.Add(cuspaycircle);
            }
            return CustomerPaymentcircleList.AsQueryable<CustomerPaymentCircle>();
        }

        public string UploadPaymentCircle(string customerNum, string siteUseId ,string legal)
        {
            List<CustomerPaymentCircle> resultList = new List<CustomerPaymentCircle>();
            string strline = "";
            string[] aryline;
            StreamReader mysr = null;
            string strCode = "";
            string strpath = "";
            FileUploadHistory fileUpHis = new FileUploadHistory();
            FileService fileService = SpringFactory.GetObjectImpl<FileService>("FileService");
            ICustomerService customerService = SpringFactory.GetObjectImpl<ICustomerService>("CustomerService");


            try
            {
                strCode = Helper.EnumToCode<FileType>(FileType.PaymentDateCircle);
                fileUpHis = fileService.GetSuccessData(strCode);
                
                //read csv file
                strpath = fileUpHis.ArchiveFileName;
                mysr = new StreamReader(strpath, System.Text.Encoding.Default);
                //先把列头读了
                mysr.ReadLine();
                while ((strline = mysr.ReadLine()) != null)
                {
                    CustomerPaymentCircle cusfrFile = new CustomerPaymentCircle();
                    aryline = strline.Split(new char[] { ',' });
                    int i = 0;
                    bool custNumIsEmpty = false;
                    bool siteUseIdIsEmpty = false;
                    string custNum = "";
                    string sui = "";
                    foreach(string ary in aryline){
                        if (i == 0 && String.IsNullOrEmpty(ary))
                        {
                            custNumIsEmpty = true;
                        }
                        else if (i == 0 && !String.IsNullOrEmpty(ary))
                        {
                            custNum = ary;
                        }
                        if (i == 1 && String.IsNullOrEmpty(ary))
                        {
                            siteUseIdIsEmpty = true;
                        }
                        else if (i == 1 && !String.IsNullOrEmpty(ary))
                        {
                            sui = ary;
                        }
                        if (i == 1 && custNumIsEmpty && !String.IsNullOrEmpty(ary))
                        {
                            return "CustomerNumber is empty and SiteUseID must be empty";
                        }
                        if (i == 1 && !custNumIsEmpty && String.IsNullOrEmpty(ary)) 
                        {
                            return "SiteUseID is empty and CustomerNumber must be empty";
                        }
                        if (i == 1 && !custNumIsEmpty && !String.IsNullOrEmpty(ary))
                        {
                            if(customerService.GetOneCustomer(custNum,ary) == null)
                            {
                                return "Customer and SiteUseId is not exist";
                            }
                        }
                        if (String.IsNullOrEmpty(ary) && (i == 2 || i == 3))
                        {
                            i++;
                            continue;
                        }
                        DateTime d = new DateTime();
                        if ((i== 2 || i == 3) && !DateTime.TryParse(ary,out d))
                        {
                            return "Upload fail,Data should be date type";
                        }
                        IList<CustomerPaymentCircle> paymentcirclelist = null;
                        if (!String.IsNullOrEmpty(custNum) && !String.IsNullOrEmpty(sui))
                        {
                            paymentcirclelist = GetCustPaymentCircle(custNum, sui);
                        }
                        else
                        {
                            paymentcirclelist = GetCustPaymentCircle(customerNum, siteUseId);
                        }

                        foreach (var item in paymentcirclelist)
                        {
                            if (i == 2)
                            {
                                if (item.Reconciliation_Day != null && Convert.ToDateTime(ary).Date == item.Reconciliation_Day.Value.Date && item.LegalEntity == legal)
                                {
                                    return "PaymentCircle exist repeat!";
                                }
                            }
                            else if (i == 3)
                            {
                                if (item.PaymentDay != null && Convert.ToDateTime(ary) == item.PaymentDay.Value.Date && item.LegalEntity == legal)
                                {
                                    return "PaymentCircle exist repeat!";
                                }
                            }
                            
                        }
                        
                        if (i == 2)
                        {
                            var payday = ary + ' ' + "23:59:59";
                            cusfrFile.Reconciliation_Day = Convert.ToDateTime(payday);
                        }
                        else if (i == 3)
                        {
                            var payday = ary + ' ' + "23:59:59";
                            cusfrFile.PaymentDay = Convert.ToDateTime(payday);
                            var week = Convert.ToInt32(Convert.ToDateTime(ary).DayOfWeek);
                            string weekday = week.ToString();
                            cusfrFile.PaymentWeek = weekday;
                        }
                        i++;
                    }

                    if (!custNumIsEmpty && !siteUseIdIsEmpty)
                    {
                        cusfrFile.CustomerNum = custNum;
                        cusfrFile.SiteUseId = sui;
                    }
                    else
                    {
                        cusfrFile.CustomerNum = customerNum;
                        cusfrFile.SiteUseId = siteUseId;
                    }
                    cusfrFile.LegalEntity = legal;
                    cusfrFile.CreatePersonId = AppContext.Current.User.EID;
                    cusfrFile.Flg = "0";
                    cusfrFile.CreateDate = AppContext.Current.User.Now;
                    cusfrFile.Deal = AppContext.Current.User.Deal;
                    resultList.Add(cusfrFile);
                    
                }
                CommonRep.BulkInsert(resultList);
                CommonRep.Commit();
                return "Add Success!";
            }
            catch (Exception ex)
            {
                fileUpHis.ProcessFlag = Helper.EnumToCode<UploadStates>(UploadStates.Failed);
                fileService.CommonRep.Commit();
                throw ex;
            }
            finally
            {
                if (mysr != null) { mysr.Close(); }
            }

        }

        public string AddPamentCircle(List<string> pay)
        {
            try
            {
                CustomerPaymentCircle circle = new CustomerPaymentCircle();

                if (!String.IsNullOrEmpty(pay[4]))
                {
                    var rd = pay[4] + ' ' + "00:00:00";
                    circle.Reconciliation_Day = Convert.ToDateTime(rd);
                    var paymentcirclelist = GetCustPaymentCircle(pay[1].ToString(), pay[3].ToString());
                    foreach (var item in paymentcirclelist)
                    {
                        if (circle.Reconciliation_Day == item.Reconciliation_Day)
                        {
                            return "PaymentCircle exist repeat!";
                        }
                    }
                }

                circle.CustomerNum = pay[1].ToString();
                circle.SiteUseId = pay[3].ToString();

                circle.Deal = AppContext.Current.User.Deal;

                circle.CreatePersonId = AppContext.Current.User.EID;
                circle.Flg = "1";
                circle.CreateDate = AppContext.Current.User.Now;
                if (circle.Id == 0)
                {
                    CommonRep.Add(circle);
                }
                CommonRep.Commit();
                return "Add Success!";
            }
            catch (DbEntityValidationException ex)
            {
                Helper.Log.Error(ex.Message, ex);

                StringBuilder errors = new StringBuilder();
                IEnumerable<DbEntityValidationResult> validationResult = ex.EntityValidationErrors;
                foreach (DbEntityValidationResult result in validationResult)
                {
                    ICollection<DbValidationError> validationError = result.ValidationErrors;
                    foreach (DbValidationError err in validationError)
                    {
                        errors.Append(err.PropertyName + ":" + err.ErrorMessage + "\r\n");
                    }
                }
                return "error";
            }
        }

        public string DelAllPamentCircle(string customerNum, string siteUseId)
        {
            try
            {
                DateTime dtTodday = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd") + " 00:00:00");
                var circleList = CommonRep.GetDbSet<CustomerPaymentCircle>().Where(o => o.CustomerNum == customerNum && o.SiteUseId == siteUseId && o.Reconciliation_Day > dtTodday);
                CommonRep.BulkDelete<CustomerPaymentCircle>(circleList);
                CommonRep.Commit();
                return "Delete Success!";
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                return "error";
            }
        }

        public string AddUploadCircle(string customerNum, string siteUseId ,string legal)
        {
            FileType fileT = FileType.PaymentDateCircle;
            string archivePath = string.Empty;
            string archiveFileName = string.Empty;
            try
            {
                //upload file to server
                HttpFileCollection files = HttpContext.Current.Request.Files;
                archivePath = ConfigurationManager.AppSettings[strPaymentDateCircleKey].ToString();
                archivePath = archivePath + DateTime.Now.Year.ToString() + "\\" + DateTime.Now.Month.ToString();
                if (Directory.Exists(archivePath) == false)
                {
                    Directory.CreateDirectory(archivePath);
                }

                archiveFileName = archivePath + "\\" + DateTime.Now.ToString("yyyy-MM-dd") + "-" + fileT.ToString() +
                            "-" + AppContext.Current.User.EID + "-" + DateTime.Now.ToString("HHmmssf") + ".csv";

                FileService service = SpringFactory.GetObjectImpl<FileService>("FileService");
                service.UploadFile(files[0], archiveFileName, fileT);

                return UploadPaymentCircle(customerNum, siteUseId, legal);    
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw new OTCServiceException("Uploaded file failed!");
            }
        }
    }
}
