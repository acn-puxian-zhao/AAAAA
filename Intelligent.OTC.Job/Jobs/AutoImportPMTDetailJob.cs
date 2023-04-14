using Intelligent.OTC.Business;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Utils;
using Intelligent.OTC.Domain.Dtos;
using Quartz;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Job
{
    [PersistJobDataAfterExecution]
    [DisallowConcurrentExecution] //不允许此 Job 并发执行任务（禁止新开线程执行）
    public class AutoImportPMTDetailJob : BaseJob
    {
        public CaCommonService caCommonService { get; set; }

        protected void init()
        {
            caCommonService = SpringFactory.GetObjectImpl<CaCommonService>("CaCommonService");
        }


        protected override void ExecuteInternal(IJobExecutionContext context)
        {
            try
            {
                init();
                //循环T_MailAdvisor_CAPMT表中CAProcessFlag为假且状态为Initialized的记录，进行处理
                string strPmtSql = @"SELECT * FROM T_MailAdvisor_CAPMT WHERE CAProcessFlag = 0 AND isLocked = 0 AND Status = 'Initialized'";
                Helper.Log.Info(strPmtSql);
                List<T_MailAdvisor_CAPMTDto> pmtList = SqlHelperMailAdvisor.GetList<T_MailAdvisor_CAPMTDto>(SqlHelperMailAdvisor.ExcuteTable(strPmtSql, CommandType.Text));
                foreach (T_MailAdvisor_CAPMTDto pmt in pmtList) {
                    bool lb_TotalResult = true;
                    string strTotalErrMessage = "";
                    try
                    {
                        Helper.Log.Info("******111111111111111111111");
                        SqlHelperMailAdvisor.ExcuteSql(string.Format("Update T_MailAdvisor_CAPMT set isLocked = 1 Where Id = '{0}'", pmt.Id));
                        Helper.Log.Info(string.Format("Update T_MailAdvisor_CAPMT set isLocked = 1 Where Id = '{0}'", pmt.Id));
                        string strPmtDetailSql = string.Format(@"SELECT * FROM T_MailAdvisor_CAPMT_Detail WHERE CAPMTID = '{0}' And Status = 'Initialized'", pmt.Id);
                        List<T_MailAdvisor_CAPMT_DetailDto> pmtDetailList = SqlHelperMailAdvisor.GetList<T_MailAdvisor_CAPMT_DetailDto>(SqlHelperMailAdvisor.ExcuteTable(strPmtDetailSql, CommandType.Text));
                        int intFileIndex = 0;

                        Helper.Log.Info("******222222222222");
                        foreach (T_MailAdvisor_CAPMT_DetailDto detail in pmtDetailList) {
                            intFileIndex++;
                            string strErrMsg = "";
                            try
                            {
                                //判断文件是否存在
                                Helper.Log.Info("*******************filepath:" + detail.FilePath);
                                if (File.Exists(detail.FilePath))
                                {
                                    //保存文件记录
                                    string strFileId = Guid.NewGuid().ToString();
                                    StringBuilder sqlFile = new StringBuilder();
                                    sqlFile.Append("INSERT INTO T_FILE (FILE_ID,FILE_NAME,PHYSICAL_PATH,OPERATOR,CREATE_TIME)");
                                    sqlFile.Append(" VALUES (N'" + strFileId + "',");
                                    sqlFile.Append("         N'" + detail.FileName + "',");
                                    sqlFile.Append("         N'" + detail.FilePath + "',");
                                    sqlFile.Append("         N'" + AppContext.Current.User.EID + "',GETDATE());");
                                    SqlHelper.ExcuteSql(sqlFile.ToString());
                                    FileDto fileDto = new FileDto();
                                    fileDto.FileId = strFileId;
                                    fileDto.FileName = detail.FileName;
                                    fileDto.PhysicalPath = detail.FilePath;
                                    Helper.Log.Info("******开始导入" + pmt.BusinessId);
                                    strErrMsg = caCommonService.doExportPMTDetailByFile(detail.FileName, fileDto, false, detail.FileName, pmt.BusinessId);
                                    Helper.Log.Info("******结束导入");
                                    strErrMsg = "Upload success." + strErrMsg;
                                    string strDetailResult = string.Format("Update T_MailAdvisor_CAPMT_Detail Set Status = 'Success' WHERE id = '{0}'", detail.Id);
                                    SqlHelperMailAdvisor.ExcuteSql(strDetailResult);
                                }
                                else
                                {
                                    Helper.Log.Info("******121222211");
                                    lb_TotalResult = false;
                                    SqlHelperMailAdvisor.ExcuteSql(string.Format("Update T_MailAdvisor_CAPMT_Detail Set Status = 'Failed', ErrorMessage = N'文件不存在!' WHERE id = '{0}'", detail.Id));
                                    Helper.Log.Info("Update T_MailAdvisor_CAPMT_Detail Set Status = 'Failed', ErrorMessage = N'文件不存在!', isLocked = 0 WHERE id = '{0}'", detail.Id); 
                                    if (string.IsNullOrEmpty(strTotalErrMessage))
                                    {
                                        strTotalErrMessage += "第" + intFileIndex + "个附件:" + detail.FilePath + ", 文件不存在!";
                                    }
                                    else
                                    {
                                        strTotalErrMessage += "\r\n第" + intFileIndex + "个附件:" + detail.FilePath + ", 文件不存在!";
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                Helper.Log.Info("******131313131");
                                string strMessage = e.Message;
                                if (strMessage.Length > 500) { strMessage = strMessage.Substring(0, 500); }
                                string strDetailResult = string.Format("Update T_MailAdvisor_CAPMT_Detail Set Status = 'Failed', ErrorMessage=N'{1}' WHERE id = '{0}'", detail.Id, strMessage);
                                Helper.Log.Info(strDetailResult);
                                SqlHelperMailAdvisor.ExcuteSql(strDetailResult);
                                if (string.IsNullOrEmpty(strTotalErrMessage))
                                {
                                    strTotalErrMessage += "第" + intFileIndex + "个附件:" + detail.FileName + "上传失败, " + strMessage;
                                }
                                else
                                {
                                    strTotalErrMessage += "\r\n第" + intFileIndex + "个附件:" + detail.FileName + "上传失败, " + strMessage;
                                }
                                lb_TotalResult = false;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Helper.Log.Info("******00000");
                        lb_TotalResult = false;
                        if (string.IsNullOrEmpty(strTotalErrMessage))
                        {
                            strTotalErrMessage += ex.Message;
                        }
                        else
                        {
                            strTotalErrMessage += "\r\n" + ex.Message;
                        }
                    }
                    finally
                    {
                        Helper.Log.Info("******3333333");
                        SqlHelperMailAdvisor.ExcuteSql(string.Format("Update T_MailAdvisor_CAPMT set isLocked = 0 Where Id = '{0}'", pmt.Id));
                        Helper.Log.Info(string.Format("Update T_MailAdvisor_CAPMT set isLocked = 0 Where Id = '{0}'", pmt.Id));
                    }
                    if (lb_TotalResult)
                    {
                        Helper.Log.Info("******444444");
                        //全部上传成功
                        SqlHelperMailAdvisor.ExcuteSql(string.Format("Update T_MailAdvisor_CAPMT set isLocked = 0, CAProcessFlag = 1, Status='Success' Where Id = '{0}'", pmt.Id));
                        Helper.Log.Info(string.Format(string.Format("Update T_MailAdvisor_CAPMT set isLocked = 0, CAProcessFlag = 1, Status='Success' Where Id = '{0}'", pmt.Id)));
                    }
                    else
                    {
                        Helper.Log.Info("******555555");
                        Helper.Log.Info("******555555" + strTotalErrMessage);
                        //全部上传成功
                        if (strTotalErrMessage.Length > 500) { strTotalErrMessage = strTotalErrMessage.Substring(0, 500); }
                        Helper.Log.Info(string.Format("Update T_MailAdvisor_CAPMT set isLocked = 0, CAProcessFlag = 1, Status='Failed',ErrorMessage=N'{1}' Where Id = '{0}'", pmt.Id, strTotalErrMessage));
                        strTotalErrMessage = strTotalErrMessage.Replace("'", "");
                        strTotalErrMessage = strTotalErrMessage.Replace(";", "");
                        SqlHelperMailAdvisor.ExcuteSql(string.Format("Update T_MailAdvisor_CAPMT set isLocked = 0, CAProcessFlag = 1, Status='Failed',ErrorMessage=N'{1}' Where Id = '{0}'", pmt.Id, strTotalErrMessage));
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("AutoImportPMTDetailJob Error", ex);
            }
        }
    }
}
