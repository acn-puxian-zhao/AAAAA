using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;
using Intelligent.OTC.Business;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Attr;
using Intelligent.OTC.Common.Exceptions;
using Intelligent.OTC.Common.Utils;
using Intelligent.OTC.Domain.DataModel;

namespace Intelligent.OTC.WebApi.Controllers
{
    [UserAuthorizeFilter(actionSet: "common")]
    public class AppFilesController : ApiController
    {
        [HttpGet]
        public HttpResponseMessage GetFile(string fileId)
        {
            FileService fs = SpringFactory.GetObjectImpl<FileService>("FileService");
            AppFile f = fs.GetAppFile(fileId);
            if (f != null)
            {
                try
                {
                    HttpResponseMessage response = new HttpResponseMessage();
                    response.StatusCode = HttpStatusCode.OK;
                    Stream ms = f.GetFileStream();
                    ms.Position = 0;
                    response.Content = new StreamContent(ms);
                    response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                    {
                        FileName = f.FileName
                    };
                    string ct = string.Empty;
                    if (!string.IsNullOrWhiteSpace(f.ContentType))
                    {
                        ct = f.ContentType;
                    }
                    else if (Path.GetExtension(f.FileName).ToLower() == "xls" || Path.GetExtension(f.FileName).ToLower() == "xlsx")
                    {
                        ct = "application/vnd.ms-excel";
                    }
                    else
                    {
                        ct = "application/octet-stream";
                    }
                    response.Content.Headers.ContentType = new MediaTypeHeaderValue(ct);
                    response.Content.Headers.ContentLength = ms.Length;
                    return response;
                }
                catch
                {
                    
                    throw;
                }
            }
            else
            {
                throw new OTCServiceException("The given file was not found. ", HttpStatusCode.NotFound);
            }
        }

        [HttpPost]
        public void DeleteFile(int id)
        {
            FileService fs = SpringFactory.GetObjectImpl<FileService>("FileService");
            fs.DeleteAppFile(id);
        }

        [HttpGet]
        public List<AppFile> GetFiles(string fileIds)
        {
            AssertUtils.ArgumentHasText(fileIds, "File Ids");
            FileService fs = SpringFactory.GetObjectImpl<FileService>("FileService");
            return fs.GetAppFiles(fileIds.Split(',').ToList());
        }

        /// <summary>
        /// Upload attachments
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public List<AppFile> UploadFiles()
        {
            HttpFileCollection files = HttpContext.Current.Request.Files;
            List<AppFile> appFiles = new List<AppFile>();

            for (int i = 0; i < files.Count; i++)
            {
                HttpPostedFile file = files[i];

                FileService fs = SpringFactory.GetObjectImpl<FileService>("FileService");
                appFiles.Add(fs.AddAppFile(file.FileName, file.InputStream, FileType.MailAttachment));
            }

            return appFiles;
        }
    }
}
