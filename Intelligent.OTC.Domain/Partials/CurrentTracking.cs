using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Intelligent.OTC.Common;

namespace Intelligent.OTC.Domain.DataModel
{
    public partial class CurrentTracking
    {
        public int SoaId { get; set; }
        public Nullable<System.DateTime> SoaDate { get; set; }
        public int SoaStatus { get; set; }
        public int R2Id { get; set; }
        public Nullable<System.DateTime> Reminder2thDate { get; set; }
        public int Reminder2thStatus { get; set; }
        public int R3Id { get; set; }
        public Nullable<System.DateTime> Reminder3thDate { get; set; }
        public int Reminder3thStatus { get; set; }
        public int HoldId { get; set; }
        public Nullable<System.DateTime> HoldDate { get; set; }
        public int HoldStatus { get; set; }
        public Nullable<System.DateTime> CloseDate { get; set; }
        public int CloseStatus { get; set; }
        public int FirstInterval { get; set; }
        public int SecondInterval { get; set; }
        public int PaymentTat { get; set; }
        public int RiskInterval { get; set; }
        public string Desc { get; set; }
        public System.DateTime CurrentDate { get; set; }

        public System.DateTime TempDate { get; set; }
    }
}
