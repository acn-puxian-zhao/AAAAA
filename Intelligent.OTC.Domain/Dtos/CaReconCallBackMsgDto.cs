using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain.Dtos
{
   public class CaReconCallBackMsgDto
    {
        public string taskId { get; set; }
        public string MENU_REGION { get; set; }
        public string LOCAL_CURRENCY { get; set; }
        public string BANK_CURRENCY { get; set; }
        public decimal? Total_AMT { get; set; }
        public decimal? BankChargeFrom { get; set; }
        public decimal? BankChargeTo { get; set; }
        public bool isJumpBankStatement { get; set; }
        public bool isJumpSiteUseId { get; set; }
        public List<CaBankStatementDto> bankList { get; set; }
        public List<CaReconMsgDetailDto> ptpList { get; set; }
        public List<CaReconMsgDetailDto> arList { get; set; }
        public string[] reconResult { get; set; }
        public string comments { get; set; }
        public int status { get; set; }

    }
}
