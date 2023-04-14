using Intelligent.OTC.Common;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.Repositories;
using System;
using System.Linq;

namespace Intelligent.OTC.Business
{
    public class SpecialNotesService
    {
        public SpecialNotesService() {  }
        public OTCRepository CommonRep { get; set; }

        public SpecialNote GetSpecialNotes(string strCustNum,string siteUseId) { 
        
            return CommonRep.GetQueryable<SpecialNote>().Where(o => o.CustomerNum == strCustNum 
            && o.SiteUseId == siteUseId
            && o.Deal == AppContext.Current.User.Deal).AsQueryable().FirstOrDefault();
        }

        public int AddOrUpdateByPara(string CustomerNum,string siteUseId ,string Legal, string Notes)
        {
            int result = 0;
            string Deal = AppContext.Current.User.Deal;
            var customer = CommonRep.GetDbSet<Customer>()
                .Where(o => o.Deal == Deal && o.CustomerNum == CustomerNum && o.SiteUseId == siteUseId).FirstOrDefault();
            var SpecialNote = CommonRep.GetDbSet<SpecialNote>()
                .Where(o => o.Deal == Deal && o.CustomerNum == CustomerNum && o.SiteUseId == siteUseId && o.LegalEntity == customer.Organization).FirstOrDefault();
            if (SpecialNote == null && !String.IsNullOrEmpty(Notes))
            {
                SpecialNote newSN = new SpecialNote();
                newSN.Deal = Deal;
                newSN.CustomerNum = CustomerNum;
                newSN.LegalEntity = customer.Organization;
                newSN.SpecialNotes = Notes;
                newSN.SiteUseId = siteUseId;
                newSN.CreateTime = AppContext.Current.User.Now;
                newSN.UpdateTime = AppContext.Current.User.Now;
                CommonRep.Add(newSN);
                result = 1;
            }
            else if (SpecialNote != null && !String.IsNullOrEmpty(Notes))
            {
                SpecialNote.SpecialNotes = Notes;
                SpecialNote.UpdateTime = AppContext.Current.User.Now;
                result = 1;
            }
            else if (SpecialNote != null && String.IsNullOrEmpty(Notes))
            {
                CommonRep.Remove(SpecialNote);
                result = 1;
            }
            CommonRep.Commit();
            return result;

        }


        public void AddOrUpdateByParaByArrow(string CustomerNum, string Legal, string Notes, string SiteuseId, string comment, string CommentExpirationDate)
        {
            string Deal = AppContext.Current.User.Deal;
            var SpecialNote = CommonRep.GetDbSet<SpecialNote>()
                .Where(o => o.Deal == Deal && o.CustomerNum == CustomerNum && o.LegalEntity == Legal && o.SiteUseId == SiteuseId).FirstOrDefault();
            if (SpecialNote == null)
            {
                SpecialNote newSN = new SpecialNote();
                newSN.Deal = Deal;
                newSN.CustomerNum = CustomerNum;
                newSN.LegalEntity = Legal;
                newSN.SpecialNotes = Notes;
                newSN.CreateTime = AppContext.Current.User.Now;
                newSN.UpdateTime = AppContext.Current.User.Now;
                newSN.SiteUseId = SiteuseId;
                CommonRep.Add(newSN);
            }
            else
            {
                SpecialNote.SpecialNotes = Notes;
                SpecialNote.UpdateTime = AppContext.Current.User.Now;
            }
            var customer = CommonRep.GetDbSet<Customer>()
                    .Where(o => o.Deal == Deal && o.CustomerNum == CustomerNum && o.Organization == Legal && o.SiteUseId == SiteuseId).FirstOrDefault();
            if (customer != null)
            {
                // INSERT T_Customer_ExpirationDateHis
                if (!Convert.ToDateTime(customer.CommentExpirationDate).ToString("yyyy-MM-dd").Equals(CommentExpirationDate))
                {
                    T_Customer_ExpirationDateHis cusExpDateHis = new T_Customer_ExpirationDateHis();

                    cusExpDateHis.CustomerNum = customer.CustomerNum;
                    cusExpDateHis.OldCommentExpirationDate = customer.CommentExpirationDate;
                    if (string.IsNullOrEmpty(CommentExpirationDate))
                    {
                        cusExpDateHis.NewCommentExpirationDate = null;
                    }
                    else
                    {
                        cusExpDateHis.NewCommentExpirationDate = Convert.ToDateTime(CommentExpirationDate);
                    }
                    cusExpDateHis.UserId = AppContext.Current.User.EID; //当前用户ID
                    cusExpDateHis.ChangeDate = DateTime.Now;
                    cusExpDateHis.SiteUseId = SiteuseId;
                    CommonRep.Add(cusExpDateHis);
                }

                if (string.IsNullOrEmpty(comment))
                {
                    customer.CommentLastDate = null;
                }
                else {
                    if (customer.Comment != comment)
                    {
                        customer.CommentLastDate = DateTime.Now;
                    }
                }
                customer.Comment = comment;

                if (string.IsNullOrEmpty(CommentExpirationDate) || CommentExpirationDate.ToLower() == "null")
                {
                    customer.CommentExpirationDate = null;
                }
                else
                {
                    customer.CommentExpirationDate = Convert.ToDateTime(CommentExpirationDate);
                }
            }

            CommonRep.Commit();

        }
    }

    
}
