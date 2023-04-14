using Intelligent.OTC.Common;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.Repositories;
using System;
using System.Linq;

namespace Intelligent.OTC.Business
{
    public class WorkflowHistoryService
    {
        #region Parameters
        public OTCRepository CommonRep { get; set; }
        public XcceleratorRepository XRep { get; set; }
        public string CurrentDeal
        {
            get
            {
                return AppContext.Current.User.Deal.ToString();
            }
        }
        public string CurrentUser
        {
            get
            {
                return AppContext.Current.User.EID.ToString();
            }
        }
        public DateTime CurrentTime
        {
            get
            {
                return AppContext.Current.User.Now;
            }
        }
        public string CurrentOper
        {
            get
            {
                return AppContext.Current.User.Id.ToString();
            }

        }
        #endregion

        public void AddOne(CollectorAlert Alert) {
            var WfHistoryCount = CommonRep.GetDbSet<WorkflowHistory>().Where(o => o.TaskId == Alert.TaskId).Count();
            if (WfHistoryCount == 0)
            {
                WorkflowHistory Wfh = new WorkflowHistory();
                Wfh.Deal = Alert.Deal;
                Wfh.Eid = Alert.Eid;
                Wfh.AlertId = Alert.Id;
                Wfh.TaskId = Alert.TaskId;
                Wfh.ProcessId = Alert.ProcessId;
                Wfh.CauseObjectNumber = Alert.CauseObjectNumber;
                Wfh.Type = Alert.AlertType;
                CommonRep.Add(Wfh);
                CommonRep.Commit();
            }
        }
    }
}
