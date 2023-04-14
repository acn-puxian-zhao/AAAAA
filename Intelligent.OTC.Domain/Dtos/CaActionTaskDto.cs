using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain.Dtos
{
    public class CaActionTaskDto
    {
        public string MENUREGION { get; set; }
        public string MENUREGIONNAME { get; set; }
        public string FACTREGION { get; set; }
        public string TRANSACTION_NUMBER { get; set; }
        public DateTime? VALUE_DATE { get; set; }
        public string CURRENCY { get; set; }
        public decimal? TRANSACTION_AMOUNT { get; set; }
        public decimal? CURRENT_AMOUNT { get; set; }
        public string Description { get; set; }
        public string MATCH_STATUS { get; set; }
        public string MATCH_STATUS_NAME { get; set; }
        public DateTime? UPLOADTIME { get; set; }
        public string UPLOADFILEID { get; set; }
        public string UPLOADFILENAME { get; set; }
        public string UPLOADFILEPATH { get; set; }
        public int HASUPLOADFILE { get; set; }
        public DateTime? IDENTIFY_TIME { get; set; }
        public string IDENTIFY_TASKID { get; set; }
        public DateTime? ADVISOR_TIME { get; set; }
        public string ADVISOR_TASKID { get; set; }
        public string ADVISOR_MailId { get; set; }
        public DateTime? ADVISOR_MailDate { get; set; }
        public int HASADVISORMAIL { get; set; }
        public DateTime? RECON_TIME { get; set; }
        public string RECON_TASKID { get; set; }
        public DateTime? Adjustment_time { get; set; }
        public DateTime? APPLY_TIME { get; set; }

        public string POSTFILENAME { get; set; }
        public string POSTFILEPATH { get; set; }
        public int haspostfile { get; set; }

        public DateTime? CLEARING_TIME { get; set; }
        public string CLEARFILENAME { get; set; }
        public string CLEARFILEPATH { get; set; }
        public int hasclearfile { get; set; }

        public string CREATE_USER { get; set; }

        public string LegalEntity { get; set; }

        public DateTime? PMTMAIL_DATE { get; set; }

        public string PMTMAIL_MESSAGEID  { get; set; }
        public string REF1  { get; set; }

        
    }

    public class CaActionTaskPage
    {
        public List<CaActionTaskDto> dataRows;

        public int count;
    }
}
