using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.DomainModel;
using Intelligent.OTC.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Business.Interfaces
{
    public interface ICaReconService
    {
        string getReconIdByBsId(string bsId);

        string getReconIdByArId(string arId);

        void deleteReconGroupByReconId(string reconId);
        
        void createReconGroupByBSId(string bsId, string taskId, string strPmtId);

        CaReconTaskDto getCaReconTaskById(string id);
        void createReconGroupByRecon(List<string> bsIds, List<string> arIds, string comments, string menuregion, string taskId, string createUser);
        CaReconGroupDtoPage getReconGroupList(string taskId);
        CaBankStatementDtoPage getUnmatchBankList(string taskId);
        CaARDtoPage getArListByCustomerNum(CaCustomerInputrDto[] customerList);
        string groupExport(CaCustomerInputrDto[] customerList);
        void unGroupReconGroupByReconId(string reconId);
        void checkCloseReconGroupByReconId(string reconId);
        void createReconGroup(string taskId, List<string> bsIds, List<string> arIds);
        CaReconGroupDtoPage getReconGroupListByBSIds(string bsIds);
        CaBankStatementDtoPage getUnmatchBankListByBSIds(string bsIds);
        HttpResponseMessage exporReconResultByTaskId(string taskId);
        string exporReconResultByBsIds(string bsIds);
        CaReconTaskDto getCaReconTaskByTaskId(string taskId);
        CaReconGroupDtoPage getReconGroupMultipleResultListByBSIds(string bsIds);
        void changeToMatch(string bsId);
    }

}
