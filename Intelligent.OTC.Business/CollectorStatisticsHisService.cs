using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Business
{
    public class CollectorStatisticsHisService
    {
        public OTCRepository CommonRep { get; set; }

        /// <summary>
        /// get all CustomerAgingStaging Data from Db
        /// </summary>
        /// <returns></returns>
        public CollectorStatisticsGraphDto GetCustomerContactCount(DateTime start, DateTime end, string type, string collector)
        {
            start = Convert.ToDateTime(start.ToString("yyyy-MM") + "-01");
            end = end.AddMonths(1).AddDays(-1);

            CollectorStatisticsGraphDto dtoReturn = new CollectorStatisticsGraphDto();
            string strTitle = "";
            IQueryable<Object> legend;
            IQueryable<Object> xAxis;
            IQueryable<Object> series;
            if (collector == "ALL")
            {
                //检索所有Collector的某项数据
                strTitle = "Collector Statistic - " + type + " 分析";

                var legendGroup = from csh in CommonRep.GetQueryable<T_COLLECTOR_STATISTICS_HIS>()
                          where csh.STATISTICS_DATE >= start && csh.STATISTICS_DATE <= end
                          && csh.STATISTICS_TYPE == type 
                          group csh by csh.STATISTICS_COLLECTOR into g
                          select new {g.Key};
                legend = from l in legendGroup
                                  join d in CommonRep.GetQueryable<SysTypeDetail>()
                                  on new { collector = l.Key } equals new { collector = d.DetailName }
                         where d.TypeCode == "045"
                         orderby d.Seq
                         select new { l.Key};

                var xAxisGroup = from csh in CommonRep.GetQueryable<T_COLLECTOR_STATISTICS_HIS>()
                                  where csh.STATISTICS_DATE >= start && csh.STATISTICS_DATE <= end
                                  && csh.STATISTICS_TYPE == type
                                  group csh by csh.STATISTICS_DATE into g
                                  select new { g.Key };
                xAxis = xAxisGroup;

                var seriesData = from csh in CommonRep.GetQueryable<T_COLLECTOR_STATISTICS_HIS>()
                          where csh.STATISTICS_DATE >= start && csh.STATISTICS_DATE <= end
                          && csh.STATISTICS_TYPE == type
                          select new {sdate = csh.STATISTICS_DATE, sname = csh.STATISTICS_COLLECTOR, sValue = csh.STATISTICS_VALUE };
                series = seriesData.OrderBy(o=>o.sname).ThenBy(o=>o.sdate);
            }
            else
            {
                //检索单人Collector的各项数据
                strTitle = "Collector Statistic - " + collector + " 分析";

                var legendGroup = from csh in CommonRep.GetQueryable<T_COLLECTOR_STATISTICS_HIS>()
                          where csh.STATISTICS_DATE >= start && csh.STATISTICS_DATE <= end
                          && csh.STATISTICS_COLLECTOR == collector
                        group csh by csh.STATISTICS_TYPE into g
                        select new { g.Key };
                legend = from l in legendGroup
                         join d in CommonRep.GetQueryable<SysTypeDetail>()
                         on new { collector = l.Key } equals new { collector = d.DetailName }
                         where d.TypeCode == "046"
                         orderby d.Seq
                         select new { l.Key };

                var xAxisGroup = from csh in CommonRep.GetQueryable<T_COLLECTOR_STATISTICS_HIS>()
                                  where csh.STATISTICS_DATE >= start && csh.STATISTICS_DATE <= end
                                  && csh.STATISTICS_COLLECTOR == collector
                                  group csh by csh.STATISTICS_DATE into g
                                  select new { g.Key };
                xAxis = xAxisGroup;

                var seriesData = from csh in CommonRep.GetQueryable<T_COLLECTOR_STATISTICS_HIS>()
                          where csh.STATISTICS_DATE >= start && csh.STATISTICS_DATE <= end
                          && csh.STATISTICS_COLLECTOR == collector
                          select new { sdate = csh.STATISTICS_DATE, sname = csh.STATISTICS_TYPE, sValue = csh.STATISTICS_VALUE };
                series = seriesData.OrderBy(o => o.sname).ThenBy(o => o.sdate); 
            }

            dtoReturn.title = strTitle;
            dtoReturn.legend = legend;
            dtoReturn.xAxis = xAxis;
            dtoReturn.series = series;

            return dtoReturn;
        }
    }
}
