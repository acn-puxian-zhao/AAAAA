using Intelligent.OTC.Business.Interfaces;
using Intelligent.OTC.Domain.Dtos;
using Intelligent.OTC.Domain.Repositories;
using System.Collections.Generic;
using System;
using Intelligent.OTC.Domain.DomainModel;
using System.Linq;
using Intelligent.OTC.Common.Utils;
using Intelligent.OTC.Common;
using Intelligent.OTC.Domain.DataModel;
using System.Net.Http;
using System.Web;
using System.Configuration;
using System.IO;
using Intelligent.OTC.Common.Exceptions;
using System.Net.Http.Headers;
using System.Net;

namespace Intelligent.OTC.Business 
{
    public class CaCustomerForwardService : ICaCustomerForwardService
    {

        public OTCRepository CommonRep { get; set; }



        public CAForwarderListDtoPage getForwarder(int page, int pageSize, string legalEntity, string customerNum, string forwardNum, string forwardName)
        {
            CAForwarderListDtoPage result = new CAForwarderListDtoPage();

            if (string.IsNullOrEmpty(legalEntity) || legalEntity == "undefined")
            {
                legalEntity = "";
            }
            if (string.IsNullOrEmpty(customerNum) || customerNum == "undefined")
            {
                customerNum = "";
            }
            if (string.IsNullOrEmpty(forwardNum) || forwardNum == "undefined")
            {
                forwardNum = "";
            } 
            if (string.IsNullOrEmpty(forwardName) || forwardName == "undefined")
            {
                forwardName = "";
            }

            string sql = string.Format(@"select id                    as Id,
                                               LegalEntity            as legalEntity,
                                               CUSTOMER_NUM           as CustomerNum,
                                               FORWARD_NUM            as ForwardNum,
                                               FORWARD_NAME           as ForwardName,
                                               CUSTOMER_NAME          as customerName,
                                               LOCALIZE_CUSTOMER_NAME as localizeCustomerName,
                                               FORWARD_GROUP as ForwardGroup
                                        from (
                                                 select ROW_NUMBER() over (order by cm.CREATE_Date desc) as RowNumber,
                                                       cm.*,
                                                       vc.CUSTOMER_NAME,
                                                       vc.LOCALIZE_CUSTOMER_NAME
                                                from T_CA_ForwarderList cm with (nolock) 
                                                         left join (select distinct CUSTOMER_NUM, CUSTOMER_NAME, LOCALIZE_CUSTOMER_NAME from V_CA_Customer with (nolock)) vc
                                                                    on cm.CUSTOMER_NUM = vc.CUSTOMER_NUM
                                                where Status = 1
                                                AND ((cm.LegalEntity like '%{2}%') OR '' = '{2}')
                                                AND ((cm.CUSTOMER_NUM like '%{3}%') OR '' = '{3}')
                                                AND ((cm.FORWARD_NUM like '%{4}%') OR '' = '{4}')
                                                AND ((cm.FORWARD_NAME like '%{5}%') OR '' = '{5}')
                                             ) as a
                                        WHERE RowNumber BETWEEN {0} AND {1}
                                        ORDER BY LegalEntity, FORWARD_NAME, CUSTOMER_NUM", page == 1 ? 0:pageSize*(page-1)+1, pageSize*page, legalEntity, customerNum, forwardNum, forwardName);


            List<CAForwarderListDto> dto = CommonRep.ExecuteSqlQuery<CAForwarderListDto>(sql).ToList();

            string sql1 = string.Format(@"select count(1) as count from T_CA_ForwarderList with (nolock)  where Status = 1
                                                AND((LegalEntity like '%{0}%') OR '' = '{0}')
                                                AND((CUSTOMER_NUM like '%{1}%') OR '' = '{1}')
                                                AND((FORWARD_NUM like '%{2}%') OR '' = '{2}')
                                                AND((FORWARD_NAME like '%{3}%') OR '' = '{3}')", legalEntity, customerNum, forwardNum, forwardName);

            result.list = dto;
            result.listCount = SqlHelper.ExcuteScalar<int>(sql1);

            return result;
        }

        public CACustomerMappingDto getCustomerName(string customerNum)
        {
            string sql = string.Format(@"select top 1
                                       CUSTOMER_NAME          as CustomerName,
                                       LOCALIZE_CUSTOMER_NAME as LocalizeCustomerName
                                    from V_CA_Customer with (nolock)
                                    where CUSTOMER_NUM = '{0}'", customerNum);


            List<CACustomerMappingDto> list = SqlHelper.GetList<CACustomerMappingDto>(SqlHelper.ExcuteTable(sql, System.Data.CommandType.Text, null));
            if (list != null && list.Count > 0)
            {
                return list[0];
            }

            return null;
      
        }


        public int AddOrUpdate(CAForwarderListDto model)
        {
            try
            {
                string sql = string.Format(@"select *
                                    from T_CA_ForwarderList with (nolock) 
                                    where id = '{0}'", model.Id);


            List<CAForwarderListDto> list = SqlHelper.GetList<CAForwarderListDto>(SqlHelper.ExcuteTable(sql, System.Data.CommandType.Text, null));

                string updateSql = "";
                if (list != null && list.Count > 0)
                {
                     updateSql = string.Format(@"update T_CA_ForwarderList set  LegalEntity = N'{0}' ,CUSTOMER_NUM = N'{1}' ,FORWARD_NUM = N'{2}',FORWARD_NAME = N'{3}',FORWARD_GROUP = N'{6}',modify_user= N'{4}',modify_date=getdate() where ID = '{5}'",
                        model.LegalEntity, model.CustomerNum, model.ForwardNum,model.ForwardName.Replace("'", "''"), AppContext.Current.User.EID, model.Id,model.ForwardGroup);
                }
                else {
                    updateSql = string.Format(@"insert into T_CA_ForwarderList(id, LegalEntity, customer_num, FORWARD_NUM,FORWARD_NAME, status, create_user, create_date,
                                              modify_user, modify_date,FORWARD_GROUP)
                                              values (newid(), N'{0}', N'{1}', N'{2}',N'{3}', 1, N'{4}', getdate(), N'{5}', getdate(),N'{6}')",
                        model.LegalEntity, model.CustomerNum, model.ForwardNum,model.ForwardName.Replace("'","''"), AppContext.Current.User.EID, AppContext.Current.User.EID,model.ForwardGroup);
                }

                SqlHelper.ExcuteSql(updateSql);

                return 1;
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                return -1;
            }
        }

        public void Remove(string id)
        {
            try
            {
                string sql = string.Format(@"delete from T_CA_ForwarderList
                                           where id = '{0}'", id);

                SqlHelper.ExcuteSql(sql);
      
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
               
            }
        }

        public HttpResponseMessage exporAll()
        {
            string templateFile = "";
            string fileName = "";
            string tmpFile = "";

            try
            {
                //模板文件  
                templateFile = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings["ExportForwarderListTemplate"].ToString());
                fileName = "ForwarderList_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".xlsx";
                tmpFile = Path.Combine(Path.GetTempPath(), fileName);

                List<CAForwarderListDto> customerList = GetForwarderListAll();
                this.SetData(templateFile, tmpFile, customerList);

                HttpResponseMessage response = new HttpResponseMessage();
                response.StatusCode = HttpStatusCode.OK;
                MemoryStream fileStream = new MemoryStream();
                if (File.Exists(tmpFile))
                {
                    using (FileStream fs = File.OpenRead(tmpFile))
                    {
                        fs.CopyTo(fileStream);
                    }
                }
                else
                {
                    throw new OTCServiceException("Get file failed because file not exist with physical path: " + tmpFile);
                }
                Stream ms = fileStream;
                ms.Position = 0;
                response.Content = new StreamContent(ms);
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = fileName
                };
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                response.Content.Headers.ContentLength = ms.Length;

                return response;
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }


        private void SetData(string templateFileName, string tmpFile, List<CAForwarderListDto> list)
        {
            int rowNo = 1;

            try
            {
                NpoiHelper helper = new NpoiHelper(templateFileName);
                helper.Save(tmpFile, true);
                helper = new NpoiHelper(tmpFile);
                string sheetName = "";

                foreach (string sheet in helper.Sheets)
                {
                    sheetName = sheet;
                    break;
                }

                //设置sheet
                helper.ActiveSheetName = sheetName;

                //设置Excel的内容信息
                foreach (var lst in list)
                {
                    helper.SetData(rowNo, 0, lst.LegalEntity);
                    helper.SetData(rowNo, 1, lst.CustomerNum);
                    helper.SetData(rowNo, 2, lst.CustomerName);
                    helper.SetData(rowNo, 3, lst.LocalizeCustomerName);
                    helper.SetData(rowNo, 4, lst.ForwardNum);
                    helper.SetData(rowNo, 5, lst.ForwardName);

                    rowNo++;
                }

                helper.Save(tmpFile, true);
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }

        private List<CAForwarderListDto> GetForwarderListAll()
        {           
            string sql = @"select cm.id               as Id,
                                  cm.LegalEntity       as legalEntity,
                                  cm.CUSTOMER_NUM     as CustomerNum,
                                  cm.FORWARD_NUM            as ForwardNum,
                                  cm.FORWARD_NAME           as ForwardName,
                                  vc.CUSTOMER_NAME    as customerName,
                                  vc.LOCALIZE_CUSTOMER_NAME as localizeCustomerName
                           from T_CA_ForwarderList cm with (nolock)
                           left join V_CA_Customer vc  with (nolock) on cm.CUSTOMER_NUM = vc.CUSTOMER_NUM
                           where Status = 1";


            return CommonRep.ExecuteSqlQuery<CAForwarderListDto>(sql).ToList();
        }
    }
}
