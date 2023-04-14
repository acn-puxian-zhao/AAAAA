using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain.Dtos
{
   public class CaReconMsgDto
    {
        public string taskId { get; set; }
        public string REGION { get; set; }
        public string FUNC_CURRENCY { get; set; }
        public string BANK_CURRENCY { get; set; }
        public decimal? Total_AMT { get; set; }
        public decimal? BankChargeFrom { get; set; }
        public decimal? BankChargeTo { get; set; }
        public bool isJumpBankStatement { get; set; }
        public bool isJumpSiteUseId { get; set; }
        public List<CaBankStatementDto> bankList { get; set; }
        public List<CaReconMsgDetailDto> pmtList { get; set; }
        public List<CaReconMsgDetailDto> ptpList { get; set; }
        public List<CaReconMsgDetailDto> arList { get; set; }
        public List<CaReconMsgDetailDto> funcArList { get; set; }

    }

    public class CaReconMsgResultDto : CaReconMsgDto
    {
        public string status { get; set; }
        public reconResultDto[] reconResult { get; set; }

    }

    public class reconResultDto
    {
        public string KEY { get; set; }
        public reconResultGroupDto[] GROUP { get; set; }
    }

    public class reconResultGroupDto
    {
        public string INV { get; set; }
        public decimal? AMOUNT { get; set; }
        public decimal? FUNCAMOUNT { get; set; }
    }
}
