using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.Repositories;
using Intelligent.OTC.Business.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intelligent.OTC.Domain.Dtos;
using System.Data.Entity.SqlServer;
using Intelligent.OTC.Common.Utils;

namespace Intelligent.OTC.Business
{
    public class CustomerAssessmentLogService : ICustomerAssessmentLogService
    {
        public OTCRepository CommonRep { get; set; }

        #region Get All CustomerAssessment Log
        public IEnumerable<LegalEntityAssessmentDateDto> GetAllCustomerAssessmentLog(string assessmentLogDate)
        {
            List<T_CustomerAssessment_Log> assessmentLogList = new List<T_CustomerAssessment_Log>();
            IEnumerable<LegalEntityAssessmentDateDto> assessmentDateIQ = null;
            List<LegalEntityAssessmentDateDto> resultList = new List<LegalEntityAssessmentDateDto>();
            try
            {
                var assessmengLog = from tcl in CommonRep.GetDbSet<T_CustomerAssessment_Log>()
                                    select new LegalEntityAssessmentDateDto
                                    {
                                        AssessmentDate = SqlFunctions.DatePart("year", tcl.AssessmentDate).ToString() + "-" + SqlFunctions.DatePart("month", tcl.AssessmentDate).ToString()
                                        + "-" + SqlFunctions.DatePart("day", tcl.AssessmentDate).ToString(),
                                        LegalEntity = tcl.LegalEntity,
                                    };
                
                assessmentDateIQ = from tcal in assessmengLog
                                   group tcal by new { ad = tcal.AssessmentDate, le = tcal.LegalEntity } into g
                                   select new LegalEntityAssessmentDateDto
                                   {
                                       AssessmentDate = g.Key.ad,
                                       LegalEntity = g.Key.le
                                   };
                var resultIE = assessmentDateIQ.ToList().ConvertAll(p => DateTime.Parse(p.AssessmentDate)).OrderByDescending(p => p);

                DateTime d = new DateTime();
                if ((String.IsNullOrEmpty(assessmentLogDate) || !DateTime.TryParse(assessmentLogDate, out d))
                    && resultIE.Count() != 0)
                {
                    assessmentLogDate = resultIE.First().ToString("yyyy-MM-dd");
                }
                var date = DateTime.Parse(assessmentLogDate);
                assessmentLogDate = date.Year.ToString() + "-" + date.Month.ToString() + "-" + date.Day.ToString();
                assessmentDateIQ = assessmentDateIQ.Where(p => p.AssessmentDate == assessmentLogDate);

            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw;
            }
            return assessmentDateIQ;
        }

        #endregion

        /// <summary>
        /// 获取T_CustomerAssessment_Log表中的所有日期
        /// </summary>
        /// <returns></returns>
        public IEnumerable<DateTime> GetAllAssessmentDate()
        {
            List<T_CustomerAssessment_Log> assessmentLogList = new List<T_CustomerAssessment_Log>();
            IEnumerable<string> assessmentDateIQ = null;
            List<DateTime> convertList = new List<DateTime>();
            IEnumerable<DateTime> resultIE = null;
            try
            {

                var assessmengLog = from tcl in CommonRep.GetDbSet<T_CustomerAssessment_Log>()
                                    select new LegalEntityAssessmentDateDto
                                    {
                                        AssessmentDate = SqlFunctions.DatePart("year",tcl.AssessmentDate).ToString() + "-" + SqlFunctions.DatePart("month", tcl.AssessmentDate).ToString() 
                                        + "-" + SqlFunctions.DatePart("day", tcl.AssessmentDate).ToString(),
                                        LegalEntity = tcl.LegalEntity,
                                    };

                assessmentDateIQ = from T_CustomerAssessment_Log in assessmengLog
                                   group T_CustomerAssessment_Log by T_CustomerAssessment_Log.AssessmentDate into g
                                    select g.Key;
                convertList = assessmentDateIQ.ToList().ConvertAll(p => DateTime.Parse(p));
                resultIE = convertList.OrderByDescending(p => p);

            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw;
            }

            return resultIE;
        }

        //获取assessmentLog的数据条数
        public int GetAssessmentLogCount()
        {
            List<T_CustomerAssessment_Log> assessmentLogList = new List<T_CustomerAssessment_Log>();
            IEnumerable<LegalEntityAssessmentDateDto> assessmentDateIQ = null;
            List<LegalEntityAssessmentDateDto> resultList = new List<LegalEntityAssessmentDateDto>();
            try
            {

                var assessmengLog = from tcl in CommonRep.GetDbSet<T_CustomerAssessment_Log>()
                                    select new LegalEntityAssessmentDateDto
                                    {
                                        AssessmentDate = SqlFunctions.DatePart("year", tcl.AssessmentDate).ToString() + "-" + SqlFunctions.DatePart("month", tcl.AssessmentDate).ToString()
                                        + "-" + SqlFunctions.DatePart("day", tcl.AssessmentDate).ToString(),
                                        LegalEntity = tcl.LegalEntity,
                                    };

                assessmentDateIQ = from tcal in assessmengLog
                                   group tcal by new { ad = tcal.AssessmentDate, le = tcal.LegalEntity } into g
                                   select new LegalEntityAssessmentDateDto
                                   {
                                       AssessmentDate = g.Key.ad,
                                       LegalEntity = g.Key.le
                                   };

            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw;
            }

            return assessmentDateIQ.Count();
        }
        
    }
}
