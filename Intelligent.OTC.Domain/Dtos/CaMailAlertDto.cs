using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain.Dtos
{
    public class CaMailAlertDto
    {
        public string ID { get; set; }
        public string EID { get; set; }
        public string BSID { get; set; }
        public string TransNumber { get; set; }
        public string AlertType { get; set; }
        public string LegalEntity { get; set; }
        public string CustomerNum { get; set; }
        public decimal Amount { get; set; }
        public string TOTITLE { get; set; }
        public string CCTITLE { get; set; }
        public string STATUS { get; set; }
        public string MessageId { get; set; }
        public string Comment { get; set; }
        public DateTime? CreateTime { get; set; }
        public DateTime? SendTime { get; set; }
        public string businessId { get; set; }
        public string strCreateTime { get; set; }
        public bool ISLOCKED { get; set; }
        public string SiteUseId { get; set; }
        public string mailto { get; set; }
        public string mailcc { get; set; }
        public string subject { get; set; }
        public string IndexFile { get; set; }
    }

    public class CaMailAlertDtoPage
    {
        public List<CaMailAlertDto> dataRows;

        public int count;
    }

}
