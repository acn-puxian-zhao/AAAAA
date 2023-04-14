using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Exceptions;
using Intelligent.OTC.Common.Utils;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.Dtos;
using Intelligent.OTC.Domain.Repositories;
using Newtonsoft.Json;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web;

namespace Intelligent.OTC.Business
{
    public class ContactService
    {
        public OTCRepository CommonRep { private get; set; }
        public XcceleratorService XccService { get; set; }

        public int CurrentPeriod
        {
            get
            {
                return CommonRep.GetDbSet<PeriodControl>().Where(o => o.Deal == AppContext.Current.User.Deal).Max(o => o.Id);
            }
        }

        /// <summary>
        /// Delete contact for the given Id
        /// </summary>
        /// <param name="id"></param>
        public void DeleteContact(int id)
        {
            AssertUtils.IsTrue(id > 0, "Contact Id");

            Contactor old = CommonRep.FindBy<Contactor>(id);
            if (old != null)
            {
                CommonRep.Remove(old);
                CommonRep.Commit();
            }
        }

        public void CopyContactors(CopyContactDto dto)
        {
            foreach (var contactor in dto.Contactors)
            {
                contactor.Id = 0;
                contactor.CustomerNum = dto.CustomerNum;
                contactor.SiteUseId = dto.SiteUseId;
                contactor.LegalEntity = dto.Legal;
                CommonRep.Add(contactor);
            }
            CommonRep.Commit();
        }

        public void AddOrUpdateContact(Contactor cont)
        {
            try
            {
                Customer cust;
                //新添加contactor
                if (cont.Id == 0)
                {
                    //添加Group级别，如果Group级别但没有对应Group抛出提示，如果有Group备份CustomerNum当降
                    //到Customer级别时，通过BkCustomerNum赋值给CustomerNum
                    if (cont.IsGroupLevel == 1)
                    {
                        cust = CommonRep.GetQueryable<Customer>().Where(o => o.CustomerNum == cont.CustomerNum && o.Deal == AppContext.Current.User.Deal).FirstOrDefault();
                        if (string.IsNullOrEmpty(cust.BillGroupCode))
                        {
                            throw new OTCServiceException("This Customer Doesn't Have Group!");
                        }
                        cont.GroupCode = cust.BillGroupCode;
                        cont.CustomerNum = "";
                        CommonRep.Add(cont);
                    }
                    else
                    {
                        cont.Deal = AppContext.Current.User.Deal;
                        cont.IsDefaultFlg = "1";
                        cont.LegalEntity = "All";
                        CommonRep.Add(cont);
                    }
                }
                else
                {
                    if (cont.IsGroupLevel == 1)
                    {
                        cust = CommonRep.GetQueryable<Customer>().Where(o => o.CustomerNum == cont.CustomerNum && o.Deal == AppContext.Current.User.Deal).FirstOrDefault();
                        if (string.IsNullOrEmpty(cust.BillGroupCode))
                        {
                            Exception ex = new OTCServiceException("This Customer Doesn't Have Group!!");
                            Helper.Log.Error(ex.Message, ex);
                            throw ex;
                        }
                        cont.GroupCode = cust.BillGroupCode;
                        cont.CustomerNum = "";
                    }
                    else
                    {
                        cont.IsDefaultFlg = "1";
                        cont.GroupCode = "";
                    }
                    Contactor old = CommonRep.FindBy<Contactor>(cont.Id);
                    ObjectHelper.CopyObjectWithUnNeed(cont, old, new string[] { "Id", "Customer" });
                }
                CommonRep.Commit();
            }
            catch (OTCServiceException ex)
            {
                Helper.Log.Info(ex);
                throw new OTCServiceException(ex.Message);
            }
            catch (DbEntityValidationException ex)
            {
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
            }
            catch (Exception ex)
            {
                Helper.Log.Info(ex);
                throw new Exception(ex.Message);
            }
        }

        public int BatchUpdate(ContactBatchUpdateDto dto)
        {
            string strOldName = dto.OldName.Trim();
            string strOldMail = dto.OldEmail.Trim();
            string strNewName = dto.NewName.Trim();
            string strNewEmail = dto.NewEmail.Trim();
            var contactors = CommonRep.GetQueryable<Contactor>().Where(o => o.Name == strOldName && o.EmailAddress == strOldMail && o.Deal == AppContext.Current.User.Deal);
            foreach (var c in contactors)
            {
                c.Name = strNewName;
                c.EmailAddress = strNewEmail;
            }
            CommonRep.Commit();
            return contactors.Count();
        }

        // Get all Contactors in one group  
        public List<Contactor> GetGroupContactors(string custNum)
        {
            var paramsList = custNum.Split(',');
            string num = paramsList[0];
            string siteUseId = paramsList[1];
            Customer cust = CommonRep.GetQueryable<Customer>().Where(o => o.CustomerNum == num 
            && o.Deal == AppContext.Current.User.Deal
            && o.SiteUseId == siteUseId).FirstOrDefault();
            string billGroupCode = (cust == null || cust.BillGroupCode == null) ? "" : cust.BillGroupCode;
            List<Contactor> contlist = new List<Contactor>();
            if (!string.IsNullOrEmpty(billGroupCode))
            {
                contlist = CommonRep.GetQueryable<Contactor>().Where(o => o.GroupCode == billGroupCode && o.Deal == AppContext.Current.User.Deal).ToList();
                foreach (Contactor cont in contlist)
                {
                    cont.IsGroupLevel = 1;
                }
            }
            return contlist;
        }

        public List<Contactor> GetCustlevelContactors(string custNum)
        {
            string num = "";
            string siteUseId = "";
            if (custNum.Equals("newCust"))
            {
                num = "";
                siteUseId = "";
            }
            else
            {
                var paramsList = custNum.Split(',');
                num = paramsList[0];
                siteUseId = paramsList[1];
            }

            var s1 = CommonRep.GetQueryable<Contactor>().Where(o => o.CustomerNum == num && o.SiteUseId == siteUseId && o.Deal == AppContext.Current.User.Deal).ToList();

            return s1;
        }

        /// <summary>
        /// Get all contacts for the giving specific customer
        /// </summary>
        /// <param name="strCustNum"></param>
        /// <returns></returns>
        public IList<Contactor> GetContactByCustomer(string strCustNum)
        {
            AssertUtils.ArgumentHasText(strCustNum, "Customer");
            List<Contactor> contlist = new List<Contactor>();
            contlist = GetCustlevelContactors(strCustNum);
            if (strCustNum != "newCust")
            {
                List<Contactor> custlvlist = GetGroupContactors(strCustNum);
                contlist.AddRange(custlvlist);
                return contlist;
            }
            return contlist;
        }

        /// <summary>
        /// Get all contacts for the giving specific customer
        /// </summary>
        /// <param name="strCustNum"></param>
        /// <returns></returns>
        public IList<Contactor> GetContactBySiteUseId(string siteUseId)
        {
            List<Contactor> contlist = CommonRep.GetQueryable<Contactor>().Where(o => o.SiteUseId == siteUseId && o.Deal == AppContext.Current.User.Deal).ToList();

            return contlist;
        }

        /// <summary>
        /// Get all contacts for giving customers
        /// added by zhangYu,get the customer contactor
        /// </summary>
        /// <param name="strCustNums"></param>
        /// <returns></returns>
        public IList<Contactor> GetContactsByCustomers(string strCustNums,string siteUseIds)
        {
            AssertUtils.ArgumentHasText(strCustNums, "Customers");

            string[] cusArray = strCustNums.Split(',');
            string[] siteUseIdArray = siteUseIds.Split(',');
            List<Contactor> listContactor = new List<Contactor>();
            listContactor = (from con in CommonRep.GetQueryable<Contactor>()
                             where con.Deal == AppContext.Current.User.Deal &&
                                   (cusArray.Contains(con.CustomerNum) || strCustNums == "") && 
                                   (siteUseIdArray.Contains(con.SiteUseId) || siteUseIds == "")
                             select con).Distinct().ToList<Contactor>();
            List<Contactor> grouplvlist = GetContactsByGroup(cusArray);
            listContactor.AddRange(grouplvlist);
            return listContactor;
        }

        public IList<ContactorDto> GetContactsByAlert(string Collector,string strCustomerNum, string strSiteUseId,string toTitle, string toName, string ccTitle, List<int> invoiceIdList) {

            string[] toNameArray = toName.Split(';');
            string[] ccTitleArray = ccTitle.Split(',');
            string[] strSiteUseIdArray = strSiteUseId.Split(',');
            if (strCustomerNum == null) { strCustomerNum = ""; }
            string[] strCustomerNumArray = strCustomerNum.Split(',');
            List<string> listAllSite = new List<string>();
            List<ContactorDto> listTo = new List<ContactorDto>();
            List<ContactorDto> listCC = new List<ContactorDto>();
            List<ContactorDto> listContactor = new List<ContactorDto>();

            //根据Collector和To的人获得所有涉及到的SiteUseId
            listAllSite = (from con in CommonRep.GetQueryable<Contactor>()
                      join ca in CommonRep.GetDbSet<Customer>()
                       on new { CustomerNum = con.CustomerNum, SiteUseId = con.SiteUseId }
                         equals new { CustomerNum = ca.CustomerNum, SiteUseId = ca.SiteUseId }
                      join inv in CommonRep.GetDbSet<InvoiceAging>()
                      on new { CustomerNum = con.CustomerNum, SiteUseId = con.SiteUseId }
                         equals new { CustomerNum = inv.CustomerNum, SiteUseId = inv.SiteUseId }
                       where ca.Deal == AppContext.Current.User.Deal &&
                            invoiceIdList.Contains(inv.Id) && 
                            ca.Collector == Collector &&
                            con.Title == toTitle &&
                            (strCustomerNumArray.Contains(con.CustomerNum) || strCustomerNum == "") &&
                            (strSiteUseIdArray.Contains(con.SiteUseId) || strSiteUseId == "") &&
                            (toNameArray.Contains(con.EmailAddress) || toNameArray.Contains(con.Name))
                      select con.SiteUseId).Distinct().ToList();
            Helper.Log.Info("********************************* all site Count:" + listAllSite.Count());
            //获得所有To的人的Email
            listTo = CommonRep.GetQueryable<Contactor>()
                      .Where(o=> listAllSite.Contains(o.SiteUseId) &&
                            o.Title == toTitle)
                       .Select(c=> new ContactorDto() { EmailAddress = c.EmailAddress, Name = c.Name})
                       .Distinct().ToList();
            Helper.Log.Info("********************************* listTo Count:" + listTo.Count());
            //获得所有CC的人的Email
            listCC = CommonRep.GetQueryable<Contactor>()
                .Where(o => listAllSite.Contains(o.SiteUseId) &&
                            ccTitleArray.Contains(o.Title))
                       .Select(c => new ContactorDto() { EmailAddress = c.EmailAddress, Name = c.Name })
                       .Distinct().ToList();
            Helper.Log.Info("********************************* listCC Count:" + listTo.Count());
            foreach (ContactorDto to in listTo) {
                ContactorDto t = new ContactorDto();
                t.ToCc = "1";
                t.EmailAddress = to.EmailAddress;
                t.Name = to.Name;
                listContactor.Add(t);
            }
            foreach (ContactorDto cc in listCC)
            {
                ContactorDto c = new ContactorDto();
                c.ToCc = "2";
                c.EmailAddress = cc.EmailAddress;
                c.Name = cc.Name;
                listContactor.Add(c);
            }
            Helper.Log.Info("*********************************** Contactor Count: " + listContactor.Count());
            return listContactor;
        }

        public IList<ContactorDto> GetCaPmtMailContacts(string strLegalEntity, string strCUSTOMER_NUM, string strSiteUseId, string strTOTITLE, string strCCTITLE, string strEID) {

            string[] toTitleArray = strTOTITLE.Split(',');
            string[] ccTitleArray = strCCTITLE.Split(',');
            string[] siteUseIdArray = strSiteUseId.Split(',');
            List<string> listAllSite = new List<string>();
            List<ContactorDto> listTo = new List<ContactorDto>();
            List<ContactorDto> listCC = new List<ContactorDto>();
            List<ContactorDto> listContactor = new List<ContactorDto>();

            //根据Collector和To的人获得所有涉及到的SiteUseId
            listAllSite = (from ca in CommonRep.GetDbSet<Customer>()
                           where (ca.Organization == strLegalEntity || ca.Organization.ToLower() == "all") &&
                                ca.CustomerNum == strCUSTOMER_NUM &&
                                (siteUseIdArray.Contains(ca.SiteUseId) || strSiteUseId == "")
                           select ca.SiteUseId).Distinct().ToList();

            //获得所有To的人的Email
            listTo = CommonRep.GetQueryable<Contactor>()
                      .Where(o => listAllSite.Contains(o.SiteUseId) &&
                            toTitleArray.Contains(o.Title))
                       .Select(c => new ContactorDto() { EmailAddress = c.EmailAddress, Name = c.Name })
                       .Distinct().ToList();

            //获得所有CC的人的Email
            listCC = CommonRep.GetQueryable<Contactor>()
                .Where(o => listAllSite.Contains(o.SiteUseId) &&
                            ccTitleArray.Contains(o.Title))
                       .Select(c => new ContactorDto() { EmailAddress = c.EmailAddress, Name = c.Name })
                       .Distinct().ToList();

            foreach (ContactorDto to in listTo)
            {
                ContactorDto t = new ContactorDto();
                t.ToCc = "1";
                t.EmailAddress = to.EmailAddress;
                t.Name = to.Name;
                listContactor.Add(t);
            }
            foreach (ContactorDto cc in listCC)
            {
                ContactorDto c = new ContactorDto();
                c.ToCc = "2";
                c.EmailAddress = cc.EmailAddress;
                c.Name = cc.Name;
                listContactor.Add(c);
            }
            //如果要CC Operator,则从Xcc中取用户的邮箱
            if (strCCTITLE.IndexOf("Operator") >= 0)
            {
                ContactorDto c = new ContactorDto();
                c.ToCc = "2";
                c.EmailAddress = XccService.GetUserOrganization(strEID).USER_MAIL;
                c.Name = strEID;
                listContactor.Add(c);
            }
            return listContactor;
        }

        public IList<Contactor> GetContactsByCustomers(List<string> customerNums, List<string> siteUseid)
        {
            List<Contactor> listContactor = new List<Contactor>();
            listContactor = (from con in CommonRep.GetQueryable<Contactor>()
                             where con.Deal == AppContext.Current.User.Deal
                                   && customerNums.Contains(con.CustomerNum) && siteUseid.Contains(con.SiteUseId)
                             select con).Distinct().ToList();
            return listContactor;
        }

        public List<Contactor> GetContactsByGroup(List<string> customerNums, List<string> siteUseid)
        {
            // Bill Group Code 已停用，Edited by Albert 2017-12-08
            return new List<Contactor>();

            List<string> grouplist = new List<string>();
            grouplist = (from cust in CommonRep.GetQueryable<Customer>()
                         where cust.Deal == AppContext.Current.User.Deal
                         && customerNums.Contains(cust.CustomerNum) && siteUseid.Contains(cust.SiteUseId)
                         select cust.BillGroupCode).ToList<string>();
            string[] groupArry = new string[grouplist.Count];
            for (int i = 0; i < grouplist.Count; i++)
            {
                if (grouplist[i] != null)
                {
                    groupArry[i] = grouplist[i];
                }
            }
            List<Contactor> contlist = new List<Contactor>();
            contlist = (from con in CommonRep.GetQueryable<Contactor>()
                        where con.Deal == AppContext.Current.User.Deal &&
                              groupArry.Contains(con.GroupCode)
                        select con).Distinct().ToList<Contactor>();
            return contlist;
        }

        public List<Contactor> GetContactsByGroup(string[] cusArray)
        {
            List<string> grouplist = new List<string>();
            grouplist = (from cust in CommonRep.GetQueryable<Customer>()
                         where cust.Deal == AppContext.Current.User.Deal &&
                               cusArray.Contains(cust.CustomerNum) && cust.BillGroupCode != null
                         select cust.BillGroupCode).Distinct().ToList<string>();
            string[] groupArry = new string[grouplist.Count];
            for (int i = 0; i < grouplist.Count; i++)
            {
                if (grouplist[i] != null)
                {
                    groupArry[i] = grouplist[i];
                }
            }
            List<Contactor> contlist = new List<Contactor>();
            contlist = (from con in CommonRep.GetQueryable<Contactor>()
                        where con.Deal == AppContext.Current.User.Deal &&
                              groupArry.Contains(con.GroupCode)
                        select con).Distinct().ToList<Contactor>();
            return contlist;
        }

        /// <summary>
        /// Get queryable history list for all customer.
        /// </summary>
        /// <returns></returns>
        public IQueryable<ContactHistory> GetContactHistory()
        {
            IQueryable<ContactHistory> li = CommonRep.GetQueryable<ContactHistory>()
                .Where(ch => ch.Deal == AppContext.Current.User.Deal)
                .OrderByDescending(a => a.ContactDate);
            return li;
        }

        public ContactHistory GetContactHistory(string contactId)
        {
           var contactHistory =  CommonRep.GetDbSet<ContactHistory>().FirstOrDefault(o => o.ContactId == contactId);
            return contactHistory;
        }

        public void CreateContactHistory(ContactHistoryCreateDto createDto)
        {
            List<ContactHistory> contactList = new List<ContactHistory>();
            foreach (var cust in createDto.CustomerNum.Split(','))
            {
                var contact = new ContactHistory();
                contact.CustomerNum = cust;

                contact.ContacterId = createDto.ContacterId;
                contact.ContactType = createDto.ContactType;
                contact.LegalEntity = createDto.LegalEntity;
                contact.SiteUseId = createDto.SiteUseId;
                contact.IsCostomerContact = createDto.IsCostomerContact;
                contact.Comments = createDto.Comments;

                contact.ContactId = Guid.NewGuid().ToString();
                contact.Deal = AppContext.Current.User.Deal;
                contact.CollectorId = AppContext.Current.User.EID;
                contact.LastUpdatePerson = AppContext.Current.User.EID;
                contact.LastUpdateTime = DateTime.Now;
                contact.ContactDate = AppContext.Current.User.Now;

                contactList.Add(contact);
            }

            try
            {
                CommonRep.BulkInsert(contactList);
                CommonRep.Commit();
            }
            catch (Exception ex)
            {

            }
          
        }

        public void UpdateContactHistory(ContactHistoryUpdateDto updateDto)
        {
            var entity = CommonRep.GetQueryable<ContactHistory>().FirstOrDefault(o=>o.Id == updateDto.Id);
            if (entity != null)
            {
                entity.Comments = updateDto.Comments;
                entity.LastUpdatePerson = AppContext.Current.User.EID;
                entity.LastUpdateTime = DateTime.Now;
                CommonRep.Commit();
            }
        }

        public string customerNameGet(string strCustNum)
        {
            return CommonRep.GetQueryable<Customer>().Where(o => o.CustomerNum == strCustNum && o.Deal == AppContext.Current.User.Deal).Select(o => o.CustomerName).FirstOrDefault();
        }

        public IEnumerable<SendSoaHead> invoiceAgingGet(string strCustNum, string legalEntity, string condition = "CC")
        {
            string deal = AppContext.Current.User.Deal.ToString();
            string[] arrLegalEntity = legalEntity.Split(',');
            List<CustomerAging> newCusAgingList = new List<CustomerAging>();
            if (condition.Equals("CC") || condition.Equals("BPTP"))
            {
                //CustomerAging
                newCusAgingList = CommonRep.GetQueryable<CustomerAging>()
                    .Where(o => o.Deal == AppContext.Current.User.Deal && o.CustomerNum == strCustNum ).ToList();
            }
            else if (condition.Equals("HC"))
            {
                newCusAgingList = CommonRep.GetQueryable<CustomerAging>()
                .Where(o => o.Deal == AppContext.Current.User.Deal && o.CustomerNum == strCustNum  && arrLegalEntity.Contains(o.LegalEntity)).ToList();
            }
            else
            { newCusAgingList = new List<CustomerAging>(); }

            //Rate
            var rateList = CommonRep.GetQueryable<RateTran>()
                .Where(o => o.Deal == AppContext.Current.User.Deal && o.EffectiveDate <= AppContext.Current.User.Now.Date && o.ExpiredDate >= AppContext.Current.User.Now.Date).ToList();
            //invoice
            var oldinvoiceList = CommonRep.GetQueryable<InvoiceAging>()
                .Where(o => o.Deal == AppContext.Current.User.Deal && o.CustomerNum == strCustNum).ToList();
            List<InvoiceAging> newinvoiceList = new List<InvoiceAging>();
            newinvoiceList = oldinvoiceList;
            foreach (var item in newinvoiceList)
            {
                if (item.Currency != "USD")
                {
                    item.StandardBalanceAmt = rateList.Find(m => m.ForeignCurrency == item.Currency).Rate * item.BalanceAmt;
                }
                else { item.StandardBalanceAmt = item.BalanceAmt; }
            }

            //cus
            Customer cus = new Customer();
            cus = CommonRep.GetQueryable<Customer>().Include<Customer, CustomerGroupCfg>(c => c.CustomerGroupCfg)
                .Where(o => o.Deal == AppContext.Current.User.Deal && o.CustomerNum == strCustNum).SingleOrDefault();

            //sendsoa
            List<SendSoaHead> sendsoaList = new List<SendSoaHead>();
            SendSoaHead sendsoa = new SendSoaHead();

            //SpecialNotes
            var SNList = CommonRep.GetQueryable<SpecialNote>().Where(o => o.Deal == AppContext.Current.User.Deal && o.CustomerNum == strCustNum).ToList();

            //customerchangehis=>class
            var classList = CommonRep.GetQueryable<CustomerLevelView>()
                .Where(o => o.Deal == AppContext.Current.User.Deal && o.CustomerNum == strCustNum).ToList();
            CustomerLevelView level = new CustomerLevelView();

            //agingDT
            DateTime agingDT = new DateTime();
            PeroidService pservice = SpringFactory.GetObjectImpl<PeroidService>("PeroidService");
            PeriodControl currentP = pservice.getcurrentPeroid();
            agingDT = dataConvertToDT(currentP.PeriodEnd.ToString());
            DateTime agingDT90 = new DateTime();
            agingDT90 = agingDT.AddDays(-90);

            sendsoa = new SendSoaHead();
            level = classList.Find(m => m.Deal == AppContext.Current.User.Deal && m.CustomerNum == strCustNum);
            sendsoa.Deal = AppContext.Current.User.Deal;
            sendsoa.CustomerCode = strCustNum;
            sendsoa.CustomerName = cus.CustomerName;
            sendsoa.TotalBalance = newCusAgingList.Sum(m => m.TotalAmt);
            sendsoa.CustomerClass = (string.IsNullOrEmpty(level.ClassLevel) == true ? "LV" : level.ClassLevel)
                    + (string.IsNullOrEmpty(level.RiskLevel) == true ? "LR" : level.RiskLevel);

            List<SoaLegal> sublegalList = new List<SoaLegal>();
            SoaLegal sublegal = new SoaLegal();
            foreach (var legal in newCusAgingList)
            {
                var invoiceList = newinvoiceList
                    .FindAll(m => m.Deal == AppContext.Current.User.Deal && m.CustomerNum == strCustNum && m.LegalEntity == legal.LegalEntity).OrderBy(m => m.DueDate).ToList();
                //modify by zhangYu start
                List<InvoiceAging> invoice = new List<InvoiceAging>();
                if (condition.Equals("BPTP")) //break PTP
                {
                    invoice = invoiceList.FindAll(m => m.States != Helper.EnumToCode<InvoiceStatus>(InvoiceStatus.Closed)
                   ).OrderBy(o => o.DueDate).OrderBy(o => o.PtpDate).OrderByDescending(o => o.States, new PTPKeyComparer()).ToList();
                }
                else if (condition.Equals("HC"))//Hold Customer
                {
                    invoice = invoiceList.FindAll(m => m.States != Helper.EnumToCode<InvoiceStatus>(InvoiceStatus.Closed)
                   ).OrderBy(o => o.DueDate).OrderBy(o => o.PtpDate).OrderByDescending(o => o.States, new BrokenPTPKeyComparer()).ToList();

                }
                else //contact customer
                {
                    invoice = invoiceList.FindAll(m => m.States == Helper.EnumToCode<InvoiceStatus>(InvoiceStatus.Open)
                    || m.States == Helper.EnumToCode<InvoiceStatus>(InvoiceStatus.PTP)
                    || m.States == Helper.EnumToCode<InvoiceStatus>(InvoiceStatus.Dispute)
                    || m.States == Helper.EnumToCode<InvoiceStatus>(InvoiceStatus.PartialPay)
                    || m.States == Helper.EnumToCode<InvoiceStatus>(InvoiceStatus.Broken_PTP)
                    || m.States == Helper.EnumToCode<InvoiceStatus>(InvoiceStatus.Hold)
                    || m.States == Helper.EnumToCode<InvoiceStatus>(InvoiceStatus.Payment));
                }
                //modify by zhangYu End
                sublegal = new SoaLegal();
                sublegal.LegalEntity = legal.LegalEntity;
                sublegal.Country = legal.Country;
                sublegal.CreditLimit = legal.CreditLimit;
                sublegal.TotalARBalance = legal.TotalAmt;
                sublegal.PastDueAmount = legal.TotalAmt - legal.CurrentAmt;
                sublegal.CreditBalance = invoice.FindAll(m => m.BalanceAmt < 0).Sum(m => m.BalanceAmt);
                sublegal.CurrentBalance = legal.CurrentAmt;
                sublegal.FCollectableAmount = invoice
                    .FindAll(m => m.DueDate <= agingDT && (m.Class == "DM" || m.Class == "INV")).Sum(m => m.StandardBalanceAmt);
                sublegal.FOverdue90Amount = invoice
                    .FindAll(m => m.DueDate <= agingDT90 && (m.Class == "DM" || m.Class == "INV")).Sum(m => m.StandardBalanceAmt);
                var SN = SNList.Find(m => m.LegalEntity == legal.LegalEntity);
                if (SN == null)
                {
                    sublegal.SpecialNotes = "";
                }
                else
                {
                    sublegal.SpecialNotes = SN.SpecialNotes;
                }
                List<SoaInvoice> subinvoiceList = new List<SoaInvoice>();
                SoaInvoice subinvoice = new SoaInvoice();
                if (invoice.Count > 0)
                {
                    foreach (var inv in invoice)
                    {
                        subinvoice = new SoaInvoice();
                        subinvoice.InvoiceId = inv.Id;
                        subinvoice.InvoiceNum = inv.InvoiceNum;
                        subinvoice.CustomerNum = inv.CustomerNum;
                        subinvoice.CustomerName = inv.CustomerName;
                        subinvoice.LegalEntity = inv.LegalEntity;
                        subinvoice.InvoiceDate = inv.InvoiceDate;
                        subinvoice.CreditTerm = inv.CreditTrem;
                        subinvoice.DueDate = inv.DueDate;
                        subinvoice.PurchaseOrder = inv.PoNum;
                        subinvoice.SaleOrder = inv.SoNum;
                        subinvoice.RBO = inv.MstCustomer;
                        subinvoice.InvoiceCurrency = inv.Currency;
                        subinvoice.OriginalInvoiceAmount = inv.OriginalAmt.ToString();
                        subinvoice.OutstandingInvoiceAmount = inv.BalanceAmt;
                        subinvoice.DaysLate = new TimeSpan(AppContext.Current.User.Now.Ticks).Subtract(new TimeSpan(Convert.ToDateTime(inv.DueDate).Ticks)).Duration().Days.ToString();
                        subinvoice.InvoiceTrack = !String.IsNullOrEmpty(inv.TrackStates) ? Helper.CodeToEnum<TrackStatus>(inv.TrackStates).ToString().Replace("_", " ") : "";
                        subinvoice.Status = !String.IsNullOrEmpty(inv.States) ? Helper.CodeToEnum<InvoiceStatus>(inv.States).ToString().Replace("_", " ") : "";
                        //added by zhangYu start
                        subinvoice.PtpDate = inv.PtpDate;
                        //added by zhangYu End
                        subinvoice.DocumentType = inv.Class;
                        subinvoice.Comments = inv.Comments;
                        subinvoiceList.Add(subinvoice);
                    }
                }
                else
                {
                    subinvoice = new SoaInvoice();
                    subinvoiceList.Add(subinvoice);
                }
                // logic to build reminder calendars
                IBaseDataService bdSer = SpringFactory.GetObjectImpl<IBaseDataService>("BaseDataService");
                IDunningService service = SpringFactory.GetObjectImpl<IDunningService>("DunningReminderService");
                List<CollectorAlert> reminders = service.GetEstimatedReminders(new List<string>() { strCustNum }, legal.LegalEntity);

                ReminderCalendar calendar = new ReminderCalendar();
                // 1. SOA
                var tracking = calendar.GetTracking(reminders.FindAll(a => a.CustomerNum == strCustNum && string.IsNullOrEmpty(a.LegalEntity)));
                // 2. Other reminders
                tracking = calendar.GetTracking(reminders.FindAll(a => a.CustomerNum == strCustNum && a.LegalEntity == legal.LegalEntity), tracking);
                // 3. Append other information shown in UI;
                sublegal.SubTracking = tracking;

                sublegal.SubInvoice = subinvoiceList;
                sublegalList.Add(sublegal);
            }
            sendsoa.SubLegal = sublegalList;

            sendsoaList.Add(sendsoa);

            return sendsoaList.AsQueryable<SendSoaHead>();
        }

        public class PTPKeyComparer : System.Collections.Generic.Comparer<string>
        {
            public override int Compare(string x, string y)
            {
                var code = Helper.EnumToCode<InvoiceStatus>(InvoiceStatus.PTP);
                if (x == code)
                {
                    if (y == code)
                    {
                        return 0;
                    }
                    else
                    {
                        return 1;
                    }
                }
                else if (y == code)
                {
                    return -1;
                }
                else
                {
                    return x.CompareTo(y);
                }
            }
        }

        public class BrokenPTPKeyComparer : System.Collections.Generic.Comparer<string>
        {
            public override int Compare(string x, string y)
            {
                var code = Helper.EnumToCode<InvoiceStatus>(InvoiceStatus.Broken_PTP);
                if (x == code)
                {
                    if (y == code)
                    {
                        return 0;
                    }
                    else
                    {
                        return 1;
                    }
                }
                else if (y == code)
                {
                    return -1;
                }
                else
                {
                    return x.CompareTo(y);
                }
            }
        }

        public DateTime dataConvertToDT(string strData)
        {
            DateTime dt = new DateTime();
            if (!string.IsNullOrEmpty(strData.Trim()))
            {
                return Convert.ToDateTime(strData);
            }

            return dt;
        }

        /// <summary>
        /// Insert Datas To Invoice_Log Table (Notice)
        /// </summary>
        /// <param name="list">list</param>
        public void insertInvoiceLogForNotice(List<string> list)
        {
            List<InvoiceLog> listInvLog = new List<InvoiceLog>();
            InvoiceLog invLog = new InvoiceLog();
            List<string> invoiceIds = new List<string>();

            int id = 0;
            ISoaService service = SpringFactory.GetObjectImpl<ISoaService>("SoaService");
            try
            {
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 30, 0)))
                {
                    invoiceIds = list[1].Split(',').ToList();
                    var suid = list[13];
                    var cusnum = list[2];
                    var legals = list[14];

                    Call ca = new Call();
                    ca.Comments = list[11];
                    ca.BeginTime = DateTime.Now;
                    ca.EndTime = DateTime.Now;
                    var calid = System.Guid.NewGuid().ToString();
                    ca.ContactId = calid;
                    CommonRep.Add(ca);
                    CommonRep.Commit();

                    var mid = list[3];
                    var callcontact = list[10];
                    var e = CommonRep.GetQueryable<T_PTPPayment>().ToList();
                    DateTime promiseDate = Convert.ToDateTime(list[6]);

                    T_PTPPayment ptpment = new T_PTPPayment();
                    ptpment.Deal = AppContext.Current.User.Deal;
                    ptpment.CustomerNum = cusnum;
                    ptpment.CollectorId = AppContext.Current.User.EID;
                    ptpment.Tracker = list[5];
                    ptpment.PromiseDate = promiseDate;
                    ptpment.IsPartialPay = Convert.ToBoolean(list[7]);
                    ptpment.Payer = list[8];
                    ptpment.PaymentMethod = list[9];
                    ptpment.Contact = callcontact;
                    ptpment.Comments = list[11];
                    ptpment.SiteUseId = suid;
                    ptpment.PromissAmount = Convert.ToDecimal(list[12]);
                    ptpment.IsForwarder = Convert.ToBoolean(list[15]);
                    ptpment.CreateTime = DateTime.Now;
                    ptpment.PTPPaymentType = "Payment";
                    ptpment.PTPStatus = "001";
                    ptpment.Status_Date = DateTime.Now;
                    if (mid != null && mid != "")
                    {
                        ptpment.MailId = mid;
                    }
                    CommonRep.Add(ptpment);
                    CommonRep.Commit();

                    List<T_PTPPayment_Invoice> payinvoice = new List<T_PTPPayment_Invoice>();
                    for (int g = 0; g < invoiceIds.Count; g++)
                    {
                        T_PTPPayment_Invoice pi = new T_PTPPayment_Invoice();
                        pi.PTPPaymentId = ptpment.Id; // pid;
                        pi.InvoiceId = Convert.ToInt32(invoiceIds[g]);
                        payinvoice.Add(pi);
                    }
                    CommonRep.AddRange(payinvoice);
                    CommonRep.Commit();
                    if (callcontact != "" && callcontact != null)
                    {
                        ContactHistory conhistory = new ContactHistory();
                        conhistory.CollectorId = AppContext.Current.User.EID;
                        conhistory.Comments = list[11];
                        conhistory.ContactDate = DateTime.Now;
                        conhistory.ContacterId = callcontact;
                        conhistory.ContactId = calid;
                        conhistory.ContactType = "Call";
                        conhistory.CustomerNum = cusnum;
                        conhistory.Deal = AppContext.Current.User.Deal;
                        conhistory.LegalEntity = legals;
                        conhistory.SiteUseId = suid;
                        conhistory.LastUpdatePerson = AppContext.Current.User.EID;
                        conhistory.LastUpdateTime = DateTime.Now;
                        CommonRep.Add(conhistory);
                        CommonRep.Commit();
                    }
                 
                    string nInvoiceNums = string.Empty;
                    List<int> nInvIds = invoiceIds.Select(x => Int32.Parse(x)).ToList();
                    List<InvoiceAging> invList = CommonRep.GetDbSet<InvoiceAging>().Where(o => nInvIds.Contains(o.Id)).ToList();

                    foreach (var invAging in invList)
                    {
                        nInvoiceNums += invAging.InvoiceNum + ",";

                        invLog = new InvoiceLog();
                        invLog.InvoiceId = invAging.InvoiceNum;
                        invLog.Deal = AppContext.Current.User.Deal;
                        invLog.CustomerNum = invAging.CustomerNum;
                        invLog.LogDate = AppContext.Current.User.Now;
                        invLog.LogPerson = AppContext.Current.User.EID;
                        invLog.LogAction = "CONTACT";
                        invLog.LogType = "5";
                        invLog.OldStatus = invAging.States;
                        invLog.NewStatus = Helper.EnumToCode<InvoiceStatus>(InvoiceStatus.Payment);
                        invLog.OldTrack = invAging.TrackStates;
                        if (list[5] != null && list[5] != "")
                        {
                            string trackName = list[5].Replace(" ", "_");
                            string trackCode = "";
                            invLog.NewTrack = trackCode;
                            invAging.TrackStates = trackCode;
                        }
                        invLog.ProofId = list[3];
                        invLog.ContactPerson = list[4];
                        invLog.Discription = list[11] + Environment.NewLine + invAging.Comments;
                        listInvLog.Add(invLog);

                        //update track_status(invoice_aging)
                        invAging.Payment_Date = promiseDate;
                        invAging.TrackStates = Helper.EnumToCode<TrackStatus>(TrackStatus.Payment_Notice_Received);
                        invAging.States = Helper.EnumToCode<InvoiceStatus>(InvoiceStatus.Payment);
                        if (mid != "" && mid != null)
                        {
                            invAging.MailId = mid;
                        }
                        invAging.CallId = calid;

                        invAging.TRACK_DATE = DateTime.Now;
                        invAging.FinishedStatus = "1";

                        if (Convert.ToBoolean(list[15]) == true)
                        {
                            invAging.IsForwarder = true;
                            invAging.Forwarder = list[8];
                        }
                        else
                        {
                            invAging.IsForwarder = false;
                        }
                    }

                    CommonRep.AddRange<InvoiceLog>(listInvLog);
                    CommonRep.Commit();
                    
                    CommonRep.GetDBContext().Database.ExecuteSqlCommand(string.Format("p_UpdateTaskStatus '','','','{0}','{1}'", nInvoiceNums, DateTime.Now));

                    scope.Complete();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Insert Datas To Invoice_Log Table (PTP)
        /// </summary>
        /// <param name="list">list</param>
        public void insertInvoiceLogForPtp(List<string> list)
        {
            List<InvoiceLog> listInvLog = new List<InvoiceLog>();
            InvoiceLog invLog = new InvoiceLog();
            List<string> invoiceIds = new List<string>();

            int id = 0;
            ISoaService service = SpringFactory.GetObjectImpl<ISoaService>("SoaService");
            try
            {
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 30, 0)))
                {
                    invoiceIds = list[1].Split(',').ToList();
                    var suid = list[12].Split(',')[0];
                    var cusnum = list[2].Split(',')[0];
                    var legals = list[13];

                    string calid = null;
                    if (list[9] != null)
                    {
                        Call ca = new Call();
                        ca.Comments = list[10];
                        ca.contacterId = list[9];
                        ca.BeginTime = DateTime.Now;
                        ca.EndTime = DateTime.Now;
                        calid = System.Guid.NewGuid().ToString();
                        ca.ContactId = calid;

                        CommonRep.Add(ca);
                        CommonRep.Commit();
                    }


                    var mid = list[3];
                    var callcontact = list[9];
                    var e = CommonRep.GetQueryable<T_PTPPayment>().ToList();
                    T_PTPPayment ptpment = new T_PTPPayment();
                    ptpment.Deal = AppContext.Current.User.Deal;
                    ptpment.CustomerNum = cusnum;
                    ptpment.CollectorId = AppContext.Current.User.EID;
                    ptpment.PromiseDate = Convert.ToDateTime(list[5]);
                    ptpment.IsPartialPay = Convert.ToBoolean(list[6]);
                    ptpment.Payer = list[7];
                    ptpment.PaymentMethod = list[8];
                    ptpment.Contact = callcontact;
                    ptpment.Comments = list[10];
                    ptpment.SiteUseId = suid;
                    ptpment.PromissAmount = Convert.ToDecimal(list[11]);
                    ptpment.IsForwarder = Convert.ToBoolean(list[14]);
                    ptpment.CreateTime = DateTime.Now;
                    ptpment.PTPPaymentType = "PTP";
                    ptpment.PTPStatus = "001";
                    ptpment.Status_Date = DateTime.Now;
                    if (mid != null && mid != "")
                    {
                        ptpment.MailId = mid;
                    }
                    CommonRep.Add(ptpment);
                    CommonRep.Commit();

                    bool hasInv = false;
                    List<T_PTPPayment_Invoice> payinvoice = new List<T_PTPPayment_Invoice>();
                    for (int g = 0; g < invoiceIds.Count; g++)
                    {
                        if (!string.IsNullOrEmpty(invoiceIds[g])) {
                            hasInv = true;
                            T_PTPPayment_Invoice pi = new T_PTPPayment_Invoice();
                            pi.PTPPaymentId = ptpment.Id; // pid;
                            pi.InvoiceId = Convert.ToInt32(invoiceIds[g]);
                            payinvoice.Add(pi);
                        }
                    }
                    CommonRep.AddRange(payinvoice);
                    CommonRep.Commit();
                    if (callcontact != "" && callcontact != null)
                    {
                        ContactHistory conhistory = new ContactHistory();
                        conhistory.CollectorId = AppContext.Current.User.EID;
                        conhistory.Comments = list[10];
                        conhistory.ContactDate = DateTime.Now;
                        conhistory.ContacterId = callcontact;
                        conhistory.ContactId = calid;
                        conhistory.ContactType = "Call";
                        conhistory.CustomerNum = cusnum;
                        conhistory.Deal = AppContext.Current.User.Deal;
                        conhistory.LegalEntity = legals;
                        conhistory.SiteUseId = suid;
                        conhistory.LastUpdatePerson = AppContext.Current.User.EID;
                        conhistory.LastUpdateTime = DateTime.Now;
                        CommonRep.Add(conhistory);
                        CommonRep.Commit();
                    }
                 
                    if(hasInv == true) {
                        string nInvoiceNums = string.Empty;
                        List<int> nInvIds = invoiceIds.Select(x => Int32.Parse(x)).ToList();
                        List<InvoiceAging> invList = CommonRep.GetDbSet<InvoiceAging>().Where(o => nInvIds.Contains(o.Id)).ToList();

                        foreach (var invAging in invList)
                        {
                            if (invAging.TrackStates.Contains("PTP"))
                            {
                                Exception ex = new Exception("cannot PTP when the status of invoice is PTP ");
                                Helper.Log.Error(ex.Message, ex);
                                throw ex;
                            }

                            nInvoiceNums += invAging.InvoiceNum + ",";

                            invLog = new InvoiceLog();
                            invLog.InvoiceId = invAging.InvoiceNum;
                            invLog.Deal = AppContext.Current.User.Deal;
                            invLog.CustomerNum = invAging.CustomerNum;
                            invLog.LogDate = AppContext.Current.User.Now;
                            invLog.LogPerson = AppContext.Current.User.EID;
                            invLog.LogAction = "CONTACT";
                            invLog.LogType = "4";
                            invLog.OldStatus = invAging.States;
                            invLog.NewStatus = Helper.EnumToCode<InvoiceStatus>(InvoiceStatus.PTP);
                            invLog.OldTrack = invAging.TrackStates;
                            invLog.NewTrack = Helper.EnumToCode<TrackStatus>(TrackStatus.PTP_Confirmed);
                            invLog.ProofId = list[3];
                            invLog.ContactPerson = list[4];
                            invLog.Discription = list[10] + Environment.NewLine + invAging.Comments;
                            listInvLog.Add(invLog);

                            //update status and ptpDate(invoice_aging)
                            invAging.States = Helper.EnumToCode<InvoiceStatus>(InvoiceStatus.PTP);
                            invAging.TrackStates = Helper.EnumToCode<TrackStatus>(TrackStatus.PTP_Confirmed);
                            invAging.PtpDate = DateTime.Parse(list[5]);
                            if (mid != "" && mid != null)
                            {
                                invAging.MailId = mid;
                            }
                            invAging.CallId = calid;

                            invAging.TRACK_DATE = DateTime.Now;
                            invAging.FinishedStatus = "1";

                            if (Convert.ToBoolean(list[14]) == true)
                            {
                                invAging.IsForwarder = true;
                                invAging.Forwarder = list[7];
                            }
                            else
                            {
                                invAging.IsForwarder = false;
                            }
                        }
                        CommonRep.AddRange<InvoiceLog>(listInvLog);

                        CommonRep.Commit();

                        CommonRep.GetDBContext().Database.ExecuteSqlCommand(string.Format("p_UpdateTaskStatus '','','','{0}','{1}'", nInvoiceNums, DateTime.Now));

                    }
                    scope.Complete();
                }
            }
            catch (DbEntityValidationException ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }

        /// <summary>
        /// Insert Datas To Invoice_Log Table (Dispute)
        /// </summary>
        /// <param name="disInvInstance"></param>
        public void insertInvoiceLogForDispute(DisputeInvoice disInvInstance)
        {
            InvoiceLog invLog = new InvoiceLog();
            DisputeInvoice disInv = new DisputeInvoice();
            List<InvoiceLog> listInvLog = new List<InvoiceLog>();
            List<DisputeInvoice> listDisInvoice = new List<DisputeInvoice>();

            int id = 0;
            string tempCustNum = "";
            try
            {
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 30, 0)))
                {
                    var legals = disInvInstance.LegalEntity;
                    var siteUId = disInvInstance.siteUseId;
                    var cusno = disInvInstance.customerNum;
                    var callcontact = disInvInstance.callContact;
                    var mid = disInvInstance.contactId;

                    Call call = new Call();
                    call.BeginTime = DateTime.Now;
                    call.EndTime = DateTime.Now;
                    call.Comments = disInvInstance.comments;
                    var calid = System.Guid.NewGuid().ToString();
                    call.ContactId = calid;
                    CommonRep.Add(call);
                    CommonRep.Commit();
                    if (callcontact != "" && callcontact != null)
                    {
                        ContactHistory conhistory = new ContactHistory();
                        conhistory.CollectorId = AppContext.Current.User.EID;
                        conhistory.Comments = disInvInstance.comments;
                        conhistory.ContactDate = DateTime.Now;
                        conhistory.ContacterId = callcontact;
                        conhistory.ContactId = calid;
                        conhistory.ContactType = "Call";
                        conhistory.CustomerNum = cusno;
                        conhistory.Deal = AppContext.Current.User.Deal;
                        conhistory.LegalEntity = legals;
                        conhistory.SiteUseId = siteUId;
                        conhistory.LastUpdatePerson = AppContext.Current.User.EID;
                        conhistory.LastUpdateTime = DateTime.Now;
                        CommonRep.Add(conhistory);
                        CommonRep.Commit();
                    }
                    var millenumb = (from mn in CommonRep.GetQueryable<ContactHistory>()
                                     where mn.ContactId == mid && mn.CustomerNum == cusno && mn.SiteUseId == siteUId
                                     select mn.Id
                                    ).Count();
                    if (mid != null && mid != "")
                    {
                        if (millenumb > 0)
                        {
                            ContactHistory ch = CommonRep.GetQueryable<ContactHistory>().Where(o => o.CustomerNum == cusno && o.ContactType == "Mail")
                                .FirstOrDefault();
                            ch.ContactId = mid;
                            ch.LastUpdatePerson = AppContext.Current.User.EID;
                            ch.LastUpdateTime = DateTime.Now;
                            CommonRep.Commit();
                        }

                        else
                        {
                            ContactHistory conhistory = new ContactHistory();
                            conhistory.CollectorId = AppContext.Current.User.EID;
                            conhistory.Comments = disInvInstance.comments;
                            conhistory.ContactDate = DateTime.Now;
                            conhistory.ContacterId = callcontact;
                            conhistory.ContactId = mid;
                            conhistory.ContactType = "Mail";
                            conhistory.CustomerNum = cusno;
                            conhistory.Deal = AppContext.Current.User.Deal;
                            conhistory.LegalEntity = legals;
                            conhistory.SiteUseId = siteUId;
                            conhistory.LastUpdatePerson = AppContext.Current.User.EID;
                            conhistory.LastUpdateTime = DateTime.Now;
                            CommonRep.Add(conhistory);
                            CommonRep.Commit();
                        }
                    }
                    int disputeId = 0;
                    string nInvoiceNums = "";

                    for (int i = 0; i < disInvInstance.invoiceIds.Length; i++)
                    {
                        id = disInvInstance.invoiceIds[i];
                        InvoiceAging invAging = CommonRep.GetDbSet<InvoiceAging>().Where(o => o.Id == id).FirstOrDefault();
                        if (invAging.TrackStates.Contains("Dispute"))
                        {
                            Exception ex = new Exception("cannot dispute when the status of invoice is disputed ");
                            Helper.Log.Error(ex.Message, ex);
                            throw ex;
                        }

                        //insert t_dispute table
                        if (tempCustNum.IndexOf(invAging.CustomerNum) < 0)
                        {
                            tempCustNum += invAging.CustomerNum + ",";
                            disputeId = this.insertDispute(disInvInstance, invAging.CustomerNum);
                        }

                        //insert invoice log
                        invLog = new InvoiceLog();
                        invLog.InvoiceId = invAging.InvoiceNum;
                        invLog.Deal = AppContext.Current.User.Deal;
                        //update by pxc
                        nInvoiceNums += invAging.InvoiceNum + ",";

                        invLog.CustomerNum = invAging.CustomerNum;
                        invLog.LogDate = AppContext.Current.User.Now;
                        invLog.LogPerson = AppContext.Current.User.EID;
                        invLog.LogAction = "CONTACT";
                        invLog.LogType = "3";
                        invLog.OldStatus = invAging.States;
                        invLog.NewStatus = Helper.EnumToCode<InvoiceStatus>(InvoiceStatus.Dispute);
                        invLog.OldTrack = invAging.TrackStates;
                        invLog.NewTrack = Helper.EnumToCode<TrackStatus>(TrackStatus.Dispute_Identified);
                        invLog.ContactPerson = disInvInstance.contactPerson;
                        invLog.ProofId = disInvInstance.contactId;
                        invLog.Discription = disInvInstance.comments + Environment.NewLine + invAging.Comments;
                        listInvLog.Add(invLog);

                        //insert dispute_invoice
                        disInv = new DisputeInvoice();
                        disInv.InvoiceId = invAging.InvoiceNum;
                        disInv.DisputeId = disputeId;
                        listDisInvoice.Add(disInv);

                        //update status(invoice_aging) 
                        invAging.States = Helper.EnumToCode<InvoiceStatus>(InvoiceStatus.Dispute);
                        invAging.TrackStates = Helper.EnumToCode<TrackStatus>(TrackStatus.Dispute_Identified);
                        invAging.TRACK_DATE = DateTime.Now;
                        invAging.FinishedStatus = "0";

                    }
                    CommonRep.AddRange<InvoiceLog>(listInvLog);
                    CommonRep.AddRange<DisputeInvoice>(listDisInvoice);
                    CommonRep.Commit();
                    CommonRep.GetDBContext().Database.ExecuteSqlCommand(string.Format("p_UpdateTaskStatus '','','','{0}','{1}'", string.Join(",", nInvoiceNums), DateTime.Now));

                    scope.Complete();

                }
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }

        /// <summary>
        /// insert t_dispute table
        /// </summary>
        /// <param name="disInvInstance"></param>
        public int insertDispute(DisputeInvoice disInvInstance, string customerNum)
        {
            Dispute dispute = new Dispute();
            DisputeHis disHis = new DisputeHis();
            List<DisputeHis> listDisHis = new List<DisputeHis>();

            try
            {
                //insert dispute
                Customer customer = CommonRep.GetDbSet<Customer>().Where(o => o.CustomerNum == customerNum).FirstOrDefault();
                dispute.Deal = AppContext.Current.User.Deal;
                dispute.Eid = AppContext.Current.User.EID;
                dispute.CustomerNum = customerNum;
                dispute.IssueReason = disInvInstance.issue;
                dispute.CreateDate = AppContext.Current.User.Now;
                dispute.Status = Helper.EnumToCode<DisputeStatus>(DisputeStatus.Dispute_Identified);
                dispute.CreatePerson = AppContext.Current.User.EID;
                dispute.Comments = disInvInstance.comments;
                dispute.SiteUseId = disInvInstance.siteUseId;
                dispute.STATUS_DATE = DateTime.Now;
                dispute.ActionOwnerDepartmentCode = disInvInstance.actionOwnerDepartment;
                CommonRep.Add(dispute);
                CommonRep.Commit();
                int id = dispute.Id;

                //insert dispute_his
                disHis.DisputeId = id;
                disHis.HisType = Helper.EnumToCode<DisputeStatus>(DisputeStatus.Dispute_Identified);
                disHis.HisDate = dispute.CreateDate;
                disHis.EmailId = disInvInstance.contactId;
                disHis.ISSUE_REASON = dispute.IssueReason;
                listDisHis.Add(disHis);
                CommonRep.AddRange<DisputeHis>(listDisHis);
                CommonRep.Commit();

                return id;
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }

        /// <summary>
        /// insert into contact_his
        /// </summary>
        /// <param name="Type">ContactType: 1:mail;2:call</param>
        /// <param name="contactors">customernums</param>
        /// <param name="Contactors">contactors</param>
        public List<ContactHistory> AddContactHistoryList(List<CustomerKey> contactors, string contactTo, string proofId, string inputAlertType, string Collector, string toTitle, string toName, string ccTitle, Action<ContactHistory> supplymentCallBack)
        {
            string deal = AppContext.Current.User.Deal.ToString();
            string eid = AppContext.Current.User.EID.ToString();
            DateTime operDT = AppContext.Current.User.Now;
            //####### add by pxc for alertType ############ s 
            int alertType = 1;
            if (inputAlertType == "001")
            {
            }
            else if (inputAlertType == "002")
            {
                alertType = 2;
            }
            else if (inputAlertType == "003")
            {
                alertType = 3;
            }
            else if (inputAlertType == "005")
            {
                alertType = 5;
            }

            List<ContactHistory> chisList = new List<ContactHistory>();
            ContactHistory chis = new ContactHistory();
            if (contactors.Count > 0)
            {
                foreach (var cus in contactors)
                {
                    CollectorAlert alert = CommonRep.GetDbSet<CollectorAlert>().Where(m => m.CustomerNum == cus.CustomerNum && m.SiteUseId == cus.SiteUseId).FirstOrDefault();
                    chis = new ContactHistory();
                    chis.Deal = deal;
                    chis.LegalEntity = "";
                    chis.CustomerNum = cus.CustomerNum;
                    chis.SiteUseId = cus.SiteUseId;
                    chis.CollectorId = eid;
                    chis.ContacterId = contactTo;
                    chis.ContactDate = operDT;
                    chis.Comments = "";
                    chis.AlertId = alert == null ? 0 : alert.Id;
                    chis.ContactId = proofId;
                    chis.LastUpdatePerson = eid;
                    chis.LastUpdateTime = DateTime.Now;
                    chis.ToTitle = toTitle;
                    chis.ToName = toName;
                    chis.CCTitle = ccTitle;
                    supplymentCallBack(chis);
                    chisList.Add(chis);
                }
            }
            else
            {
                CollectorAlert alert = CommonRep.GetDbSet<CollectorAlert>().Where(m => m.Eid == Collector && m.AlertType == alertType && m.ToTitle == toTitle && m.CCTitle == ccTitle && m.Status == "Initialized").FirstOrDefault();
                chis = new ContactHistory();
                chis.Deal = deal;
                chis.LegalEntity = "";
                chis.CustomerNum = alert.CustomerNum;
                chis.SiteUseId = alert.SiteUseId;
                chis.CollectorId = eid;
                chis.ContacterId = contactTo;
                chis.ContactDate = operDT;
                chis.Comments = "";
                chis.AlertId = alert == null ? 0 : alert.Id;
                chis.ContactId = proofId;
                chis.LastUpdatePerson = eid;
                chis.LastUpdateTime = DateTime.Now;
                chis.ToTitle = toTitle;
                chis.ToName = toName;
                chis.CCTitle = ccTitle;
                supplymentCallBack(chis);
                chisList.Add(chis);
            }

            return chisList;
        }

        public void AddMailContactHistory(string contactor,string siteuseid, string contactTo, string proofId)
        {
            List<ContactHistory> chisList = new List<ContactHistory>();
            ContactHistory chis = new ContactHistory();
            chis.Deal = AppContext.Current.User.Deal;
            chis.LegalEntity = "";
            chis.CustomerNum = contactor;
            chis.SiteUseId = siteuseid;
            chis.CollectorId = AppContext.Current.User.EID;
            chis.ContacterId = contactTo;
            chis.ContactDate = AppContext.Current.User.Now;
            chis.Comments = "";
            chis.ContactId = proofId;
            chis.ContactType = "Mail";
            CommonRep.Add(chis);
            CommonRep.Commit();
        }
        public void AddOrUpdateDomain(ContactorDomain cont)
        {
            try
            {
                if (cont.Id == 0)
                {
                    cont.Deal = AppContext.Current.User.Deal;
                    CommonRep.Add(cont);
                }
                else
                {
                    ContactorDomain old = CommonRep.FindBy<ContactorDomain>(cont.Id);
                    ObjectHelper.CopyObjectWithUnNeed(cont, old, new string[] { "Id", "Customer" });
                }
                CommonRep.Commit();
            }
            catch (Exception ex)
            {
                Helper.Log.Info(ex);
                throw new Exception(ex.Message);
            }
        }

        public void DeleteDomain(int id)
        {
            ContactorDomain old = CommonRep.FindBy<ContactorDomain>(id);
            if (old != null)
            {
                CommonRep.Remove(old);
                CommonRep.Commit();
            }
        }

        public string Export(string custnum, string name, string siteUseId, string legalEntity)
        {

            List<SysUser> listUser = new List<SysUser>();
            listUser = XccService.GetUserTeamList(AppContext.Current.User.EID);
            string[] userGroup = listUser.Select(t => t.EID).Distinct().ToArray();
            string collecotrList = "," + string.Join(",", userGroup.ToArray()) + ",";

            IQueryable<ContactorExportDto> query = (from c1 in CommonRep.GetQueryable<Contactor>()
                                                    join c2 in CommonRep.GetQueryable<Customer>() on new { c1.CustomerNum, c1.SiteUseId } equals new { c2.CustomerNum, c2.SiteUseId }
                                                    where c1.Deal == AppContext.Current.User.Deal &&
                                                        collecotrList.Contains("," + c2.Collector + ",")
                                                    select new ContactorExportDto()
                                                    {
                                                        Id = c1.Id,
                                                        Collector = c2.Collector,
                                                        EbName = c2.Ebname,
                                                        CreditTerm = c2.CreditTrem,
                                                        Region = c2.Region,
                                                        Legal = c2.Organization,
                                                        CustomerName = c2.CustomerName,
                                                        CustomerNum = c1.CustomerNum,
                                                        SiteUseId = c1.SiteUseId,
                                                        Name = c1.Name,
                                                        Title = c1.Title,
                                                        EmailAddress = c1.EmailAddress
                                                    });

            if (!string.IsNullOrWhiteSpace(legalEntity))
            {
                query = query.Where(o=>o.Legal == legalEntity);
            }
            if (!string.IsNullOrWhiteSpace(siteUseId))
            {
                query = query.Where(o => o.SiteUseId == siteUseId);
            }
            if (!string.IsNullOrWhiteSpace(custnum))
            {
                query = query.Where(o => o.CustomerNum == custnum);
            }
            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(o => o.Name.Contains(name));
            }

            string custPathName = "CustPathName";
            string tempFile = HttpContext.Current.Server.MapPath("~/Template/ContactorExport.xlsx");
            string targetFoler = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings[custPathName].ToString());
            string targetFile = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings[custPathName].ToString() + "ContactorExport." + AppContext.Current.User.EID + ".xlsx");
            if (Directory.Exists(targetFoler) == false)
            {
                Directory.CreateDirectory(targetFoler);
            }
            WriteToExcel(tempFile, targetFile, query);

            HttpRequest request = HttpContext.Current.Request;
            StringBuilder appUriBuilder = new StringBuilder(request.Url.Scheme);
            appUriBuilder.Append(Uri.SchemeDelimiter);
            appUriBuilder.Append(request.Url.Authority);
            if (String.Compare(request.ApplicationPath, @"/") != 0)
            {
                appUriBuilder.Append(request.ApplicationPath);
            }


            string virPathName = appUriBuilder.ToString() + ConfigurationManager.AppSettings[custPathName].ToString().Trim('~') + "ContactorExport." + AppContext.Current.User.EID + ".xlsx";
            return virPathName;

        }

        #region Export Contactor
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public void WriteToExcel(string temp, string target, IQueryable<ContactorExportDto> contactors)
        {
            try
            {
                NpoiHelper helper = new NpoiHelper(temp);
                helper.Save(target, true);
                helper = new NpoiHelper(target);

                //向sheet为Customer的excel中写入文件

                ISheet sheet = helper.Book.GetSheetAt(1);
                ICellStyle styleCell = helper.Book.CreateCellStyle();
                IFont font = helper.Book.CreateFont();
                font.FontName = "Arial";
                font.FontHeight = 9;
                styleCell.SetFont(font);
                styleCell.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
                styleCell.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
                styleCell.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;

                var groups = contactors.OrderBy(o=>o.Collector).ThenBy(o=>o.Region).ThenBy(o=>o.EbName).ThenBy(o=>o.CreditTerm).ThenBy(o=>o.CustomerNum).ThenBy(o=>o.SiteUseId).ThenBy(o=>o.Legal).ToList().GroupBy(o => new { o.Collector, o.Region, o.EbName, o.CreditTerm, o.Legal, o.CustomerNum, o.SiteUseId, o.CustomerName });


                int rowNo = 1;
                foreach (var groupContactor in groups)
                {
                    //key: title, value: ContactorExportItem
                    List<ContactorExportItem> items = new List<ContactorExportItem>()
                    {
                        new ContactorExportItem()
                    };

                    foreach (var contactor in groupContactor)
                    {
                        ContactorExportItem item;
                        if (contactor.Title == "Customer")
                        {
                            item = items.FirstOrDefault(o => string.IsNullOrEmpty(o.CustomerEmail));
                            if (item == null)
                            {
                                item = new ContactorExportItem();
                                item.Customer = contactor.Name;
                                item.CustomerEmail = contactor.EmailAddress;
                                items.Add(item);
                            }
                            else
                            {
                                item.Customer = contactor.Name;
                                item.CustomerEmail = contactor.EmailAddress;
                            }
                        }
                        if (contactor.Title == "CS")
                        {
                            item = items.FirstOrDefault(o => string.IsNullOrEmpty(o.CsEmail));
                            if (item == null)
                            {
                                item = new ContactorExportItem();
                                item.Cs = contactor.Name;
                                item.CsEmail = contactor.EmailAddress;
                                items.Add(item);
                            }
                            else
                            {
                                item.Cs = contactor.Name;
                                item.CsEmail = contactor.EmailAddress;
                            }
                        }
                        else if (contactor.Title == "Sales")
                        {
                            item = items.FirstOrDefault(o => string.IsNullOrEmpty(o.SalesEmail));
                            if (item == null)
                            {
                                item = new ContactorExportItem();
                                item.Sales = contactor.Name;
                                item.SalesEmail = contactor.EmailAddress;
                                items.Add(item);
                            }
                            else
                            {
                                item.Sales = contactor.Name;
                                item.SalesEmail = contactor.EmailAddress;
                            }
                        }
                        else if (contactor.Title == "Branch Manager")
                        {
                            item = items.FirstOrDefault();
                            if (string.IsNullOrWhiteSpace(item.BranchManagerEmail))
                            {
                                item.BranchManagerEmail = contactor.EmailAddress;
                            }
                            else
                            {
                                item.BranchManagerEmail += ";" + contactor.EmailAddress;
                            }
                        }
                        else if (contactor.Title == "CS Manager")
                        {
                            item = items.FirstOrDefault();
                            if (string.IsNullOrWhiteSpace(item.CsManagerEmail))
                            {
                                item.CsManagerEmail = contactor.EmailAddress;
                            }
                            else
                            {
                                item.CsManagerEmail += ";" + contactor.EmailAddress;
                            }
                        }
                        else if (contactor.Title == "Sales Manager")
                        {
                            item = items.FirstOrDefault();
                            if (string.IsNullOrWhiteSpace(item.SalesManagerEmail))
                            {
                                item.SalesManagerEmail = contactor.EmailAddress;
                            }
                            else
                            {
                                item.SalesManagerEmail += ";" + contactor.EmailAddress;
                            }
                        }
                        else if (contactor.Title == "Financial Controller")
                        {
                            item = items.FirstOrDefault();
                            if (string.IsNullOrWhiteSpace(item.FinancialControllersEmail))
                            {
                                item.FinancialControllersEmail = contactor.EmailAddress;
                            }
                            else
                            {
                                item.FinancialControllersEmail += ";" + contactor.EmailAddress;
                            }
                        }
                        else if (contactor.Title == "Finance Manager")
                        {
                            item = items.FirstOrDefault();
                            if (string.IsNullOrWhiteSpace(item.FinancialManagersEmail))
                            {
                                item.FinancialManagersEmail = contactor.EmailAddress;
                            }
                            else
                            {
                                item.FinancialManagersEmail += ";" + contactor.EmailAddress;
                            }
                        }
                        else if (contactor.Title == "Credit Officer")
                        {
                            item = items.FirstOrDefault();
                            if (string.IsNullOrWhiteSpace(item.CreditOfficersEmail))
                            {
                                item.CreditOfficersEmail = contactor.EmailAddress;
                            }
                            else
                            {
                                item.CreditOfficersEmail += ";" + contactor.EmailAddress;
                            }
                        }
                        else if (contactor.Title == "Local Finance")
                        {
                            item = items.FirstOrDefault();
                            if (string.IsNullOrWhiteSpace(item.LocalFinanceEmail))
                            {
                                item.LocalFinanceEmail = contactor.EmailAddress;
                            }
                            else
                            {
                                item.LocalFinanceEmail += ";" + contactor.EmailAddress;
                            }
                        }
                        else if (contactor.Title == "Finance Leader")
                        {
                            item = items.FirstOrDefault();
                            if (string.IsNullOrWhiteSpace(item.FinanceLeaderEmail))
                            {
                                item.FinanceLeaderEmail = contactor.EmailAddress;
                            }
                            else
                            {
                                item.FinanceLeaderEmail += ";" + contactor.EmailAddress;
                            }
                        }
                        else if (contactor.Title == "Credit Manager")
                        {
                            item = items.FirstOrDefault();
                            if (string.IsNullOrWhiteSpace(item.FinanceLeaderEmail))
                            {
                                item.CreditManagerEmail = contactor.EmailAddress;
                            }
                            else
                            {
                                item.CreditManagerEmail += ";" + contactor.EmailAddress;
                            }
                        }
                    }

                    foreach (var item in items)
                    {
                        IRow row = sheet.CreateRow(rowNo);
                        //eid
                        ICell cell = row.CreateCell(0);
                        cell.CellStyle = styleCell;
                        cell.SetCellValue(groupContactor.Key.Collector);

                        //region
                        cell = row.CreateCell(1);
                        cell.CellStyle = styleCell;
                        cell.SetCellValue(groupContactor.Key.Region);

                        //EB
                        cell = row.CreateCell(2);
                        cell.CellStyle = styleCell;
                        cell.SetCellValue(groupContactor.Key.EbName);

                        //CreditTerm
                        cell = row.CreateCell(3);
                        cell.CellStyle = styleCell;
                        cell.SetCellValue(groupContactor.Key.CreditTerm);

                        //LegalEntity
                        cell = row.CreateCell(4);
                        cell.CellStyle = styleCell;
                        cell.SetCellValue(groupContactor.Key.Legal);

                        //CustomerName
                        cell = row.CreateCell(5);
                        cell.CellStyle = styleCell;
                        cell.SetCellValue(groupContactor.Key.CustomerName);

                        //CustomerNum
                        cell = row.CreateCell(6);
                        cell.CellStyle = styleCell;
                        cell.SetCellValue(groupContactor.Key.CustomerNum);

                        //SiteUseId
                        cell = row.CreateCell(7);
                        cell.CellStyle = styleCell;
                        cell.SetCellValue(groupContactor.Key.SiteUseId);

                        //Customer
                        cell = row.CreateCell(8);
                        cell.CellStyle = styleCell;
                        cell.SetCellValue(item.Customer);

                        //CS
                        cell = row.CreateCell(9);
                        cell.CellStyle = styleCell;
                        cell.SetCellValue(item.Cs);

                        //Sales
                        cell = row.CreateCell(10);
                        cell.CellStyle = styleCell;
                        cell.SetCellValue(item.Sales);

                        //CustomerEmail
                        cell = row.CreateCell(11);
                        cell.CellStyle = styleCell;
                        cell.SetCellValue(item.CustomerEmail);

                        //CsEmail
                        cell = row.CreateCell(12);
                        cell.CellStyle = styleCell;
                        cell.SetCellValue(item.CsEmail);

                        //SalesEmail
                        cell = row.CreateCell(13);
                        cell.CellStyle = styleCell;
                        cell.SetCellValue(item.SalesEmail);

                        //BranchManagerEmail
                        cell = row.CreateCell(14);
                        cell.CellStyle = styleCell;
                        cell.SetCellValue(item.BranchManagerEmail);

                        //CsManagerEmail
                        cell = row.CreateCell(15);
                        cell.CellStyle = styleCell;
                        cell.SetCellValue(item.CsManagerEmail);

                        //SalesManagerEmail
                        cell = row.CreateCell(16);
                        cell.CellStyle = styleCell;
                        cell.SetCellValue(item.SalesManagerEmail);

                        //FinancialControllersEmail
                        cell = row.CreateCell(17);
                        cell.CellStyle = styleCell;
                        cell.SetCellValue(item.FinancialControllersEmail);

                        //FinancialManagersEmail
                        cell = row.CreateCell(18);
                        cell.CellStyle = styleCell;
                        cell.SetCellValue(item.FinancialManagersEmail);

                        //CreditOfficersEmail
                        cell = row.CreateCell(19);
                        cell.CellStyle = styleCell;
                        cell.SetCellValue(item.CreditOfficersEmail);

                        //LocalFinanceEmail
                        cell = row.CreateCell(20);
                        cell.CellStyle = styleCell;
                        cell.SetCellValue(item.LocalFinanceEmail);

                        //LocalFinanceEmail
                        cell = row.CreateCell(21);
                        cell.CellStyle = styleCell;
                        cell.SetCellValue(item.FinanceLeaderEmail);

                        //Credit Manager
                        cell = row.CreateCell(22);
                        cell.CellStyle = styleCell;
                        cell.SetCellValue(item.CreditManagerEmail);

                        rowNo++;
                    }
                }

                //设置sheet
                helper.Save(target, true);
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }
        #endregion

    }
}
