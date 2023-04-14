using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Business.Interfaces
{
    public interface ICustomerAssessmentLogService
    {
        #region Get All CustomerAssessment Log
        IEnumerable<LegalEntityAssessmentDateDto> GetAllCustomerAssessmentLog(string assessmentLogDate);
        #endregion

        /// <summary>
        /// 获取T_CustomerAssessment_Log表中的所有日期
        /// </summary>
        /// <returns></returns>
        IEnumerable<DateTime> GetAllAssessmentDate();

        int GetAssessmentLogCount();
    }
}
