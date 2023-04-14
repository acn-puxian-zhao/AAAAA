using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Intelligent.OTC.Domain.Repositories;
using Intelligent.OTC.Common.UnitOfWork;
using Intelligent.OTC.Domain.DataModel;
using System.IO;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Utils;
using Intelligent.OTC.Domain;

namespace Intelligent.OTC.Business
{
    public class CollectorSignatureService
    {
        public OTCRepository CommonRep { get; set; }
        public CollectorSignature GetCollectSignture(string langulage)
        {
            return CommonRep.GetQueryable<CollectorSignature>().Where(o => o.Collector == AppContext.Current.User.EID &&
                                   o.Deal==AppContext.Current.User.Deal && o.LANGUAGE == langulage).FirstOrDefault();
        }

        public string SaveOrUpdateSign(List<string> signature)
        {
            try
            {
                CollectorSignature collect=GetCollectSignture(signature[1]);
                if (collect == null)
                {
                    CollectorSignature newsign = new CollectorSignature();
                    newsign.Deal = AppContext.Current.User.Deal;
                    newsign.Signature = signature[0];
                    newsign.LANGUAGE = signature[1];
                    newsign.Collector = AppContext.Current.User.EID;
                    newsign.Operator = AppContext.Current.User.EID; ;
                    newsign.CreateTime = AppContext.Current.User.Now;
                    CommonRep.Add(newsign);
                }
                else 
                {
                    CollectorSignature collect2 = new CollectorSignature();
                    collect2.Id = collect.Id;
                    collect2.UpdateTime = AppContext.Current.User.Now;
                    collect2.Signature = signature[0];
                    collect2.LANGUAGE = signature[1];
                    collect2.Operator = AppContext.Current.User.EID;
                    CollectorSignature old = CommonRep.FindBy<CollectorSignature>(collect2.Id);
                    ObjectHelper.CopyObjectWithUnNeed(collect2, old, new string[] { "Id", "Collector","Deal","Language"});
                }                   
                CommonRep.Commit();
                return "Update Success!";
            }
            catch (Exception ex)
            {
                Helper.Log.Info(ex);
                throw new Exception(ex.Message);
            }
        }

        public CollectorSignature GetNewSignaTuture()
        {
            CollectorSignature collect = new CollectorSignature();
            return collect;
        }


    }
}
