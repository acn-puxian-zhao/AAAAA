using System;
using System.Collections.Generic;

namespace Intelligent.OTC.Domain.Dtos
{
   public class CaStatusReportDto
    {
        public int  Total { get; set; }
        public int UnknowCount { get; set; }
        public string UnknowPercent { get; set; }
        public int UnMatchCount { get; set; }
        public string UnMatchPercent { get; set; }
        public int MatchCount { get; set; }
        public string MatchPercent { get; set; }
        public string  CreateDate { get; set; }
        public string LegalEntity { get; set; }
    }

    public class CaStatusReportProcessDto
    {
        public int count { get; set; }
        public string status { get; set; }
        public string date { get; set; }
        public string LegalEntity { get; set; }
    }
}
