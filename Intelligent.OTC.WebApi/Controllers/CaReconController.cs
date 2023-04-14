using Intelligent.OTC.Business.Interfaces;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Exceptions;
using Intelligent.OTC.Common.Utils;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.DomainModel;
using Intelligent.OTC.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;

namespace Intelligent.OTC.WebApi.Controllers
{

    public class CaReconController : ApiController
    {
        [HttpGet]
        [Route("api/caReconController/getReconGroupList")]
        public CaReconGroupDtoPage getReconGroupList(string taskId)
        {
            ICaReconService service = SpringFactory.GetObjectImpl<ICaReconService>("CaReconService");
            var res = service.getReconGroupList(taskId);
            return res;
        }

        [HttpGet]
        [Route("api/caReconController/getUnmatchBankList")]
        public CaBankStatementDtoPage getUnmatchBankList(string taskId)
        {
            ICaReconService service = SpringFactory.GetObjectImpl<ICaReconService>("CaReconService");
            var res = service.getUnmatchBankList(taskId);
            return res;
        }

        [HttpPost]
        [Route("api/caReconController/getReconGroupListByBSIds")]
        public CaReconGroupDtoPage getReconGroupListByBSIds(CaReconAdjustmentDto bsIds)
        {
            ICaReconService service = SpringFactory.GetObjectImpl<ICaReconService>("CaReconService");
            var res = service.getReconGroupListByBSIds(bsIds.bsIds);
            return res;
        }

        [HttpPost]
        [Route("api/caReconController/getReconGroupMultipleResultListByBSIds")]
        public CaReconGroupDtoPage getReconGroupMultipleResultListByBSIds(CaReconAdjustmentDto bsIds)
        {
            ICaReconService service = SpringFactory.GetObjectImpl<ICaReconService>("CaReconService");
            var res = service.getReconGroupMultipleResultListByBSIds(bsIds.bsIds);
            return res;
        }

        [HttpPost]
        [Route("api/caReconController/getUnmatchBankListByBSIds")]
        public CaBankStatementDtoPage getUnmatchBankListByBSIds(CaReconAdjustmentDto bsIds)
        {
            ICaReconService service = SpringFactory.GetObjectImpl<ICaReconService>("CaReconService");
            var res = service.getUnmatchBankListByBSIds(bsIds.bsIds);
            return res;
        }

        [HttpPost]
        [Route("api/caReconController/getArListByCustomerNum")]
        public CaARDtoPage getArListByCustomerNum(CaCustomerInputrDto[] customerList)
        {
            ICaReconService service = SpringFactory.GetObjectImpl<ICaReconService>("CaReconService");
            var res = service.getArListByCustomerNum(customerList);
            return res;
        }

        [HttpPost]
        [Route("api/caReconController/groupExport")]
        public string groupExport(CaCustomerInputrDto[] customerList)
        {
            ICaReconService service = SpringFactory.GetObjectImpl<ICaReconService>("CaReconService");
            var res = service.groupExport(customerList);
            return res;
        }

        [HttpPost]
        [Route("api/caReconController/ungroup")]
        public void ungroup(string[] reconIds)
        {
            ICaReconService service = SpringFactory.GetObjectImpl<ICaReconService>("CaReconService");
            // 判断是否存在已经close的数据，若存在则抛出异常
            foreach (var reconId in reconIds)
            {
                service.checkCloseReconGroupByReconId(reconId);
            }

            foreach (var reconId in reconIds)
            {
                service.unGroupReconGroupByReconId(reconId);
            }
            
        }

        [HttpPost]
        [Route("api/caReconController/group")]
        public void group(CaGroupDto caGroupDto)
        {
            ICaReconService service = SpringFactory.GetObjectImpl<ICaReconService>("CaReconService");
            // 根据bankId删除reconGroup
            List<string> bankIdList = new List<string>(caGroupDto.bankIds);
            List<string> arIdList = new List<string>(caGroupDto.arIds);
            foreach (var bsId in bankIdList)
            {
                string reconId = service.getReconIdByBsId(bsId);
                service.deleteReconGroupByReconId(reconId);
            }
            // 生成新recon
            service.createReconGroup(caGroupDto.taskId, bankIdList, arIdList);
        }

        [HttpGet]
        [Route("api/caReconController/exporReconResultByReconId")]
        public HttpResponseMessage exporReconResultByReconId(string taskId)
        {
            ICaReconService service = SpringFactory.GetObjectImpl<ICaReconService>("CaReconService");
            return service.exporReconResultByTaskId(taskId);
        }

        [HttpPost]
        [Route("api/caReconController/exporReconResultByBsIds")]
        public string exporReconResultByBsIds(CaReconAdjustmentDto bsIds)
        {
            ICaReconService service = SpringFactory.GetObjectImpl<ICaReconService>("CaReconService");
            var res = service.exporReconResultByBsIds(bsIds.bsIds);
            return res;
        }

        [HttpPost]
        [Route("api/caReconController/changeToMatch")]
        public void changeToMatch(CaReconAdjustmentDto bsIds)
        {
            ICaReconService service = SpringFactory.GetObjectImpl<ICaReconService>("CaReconService");
            service.changeToMatch(bsIds.bsIds);
        }
    }
}
