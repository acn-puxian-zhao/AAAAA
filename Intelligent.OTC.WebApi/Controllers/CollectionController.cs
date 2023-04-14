using ICSharpCode.SharpZipLib.Zip;
using Intelligent.OTC.Business;
using Intelligent.OTC.Business.Collection;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Attr;
using Intelligent.OTC.Common.Exceptions;
using Intelligent.OTC.Common.Utils;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.WebApi.Core;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Web;
using System.Web.Http;

namespace Intelligent.OTC.WebApi.Controllers
{
    [UserAuthorizeFilter(actionSet: "dataprepare")]
    public class CollectionController : ApiController
    {
        public const string strArchiveAccountKey = "ArchiveAccountLevelPath";//ArchiveAccount路径的config保存名
        public const string strArchiveInvoiceKey = "ArchiveInvoiceLevelPath";//ArchiveInvoice路径的config保存名
        public const string strArchiveOneYearSalesKey = "ArchiveOneYearSalesPath";//ArchiveOneYearSales路径的config保存名
        public const string strArchiveInvoiceDetailKey = "ArchiveInvoiceDetailPath";
        public const string strArchiveVATKey = "ArchiveVATPath";
        [HttpGet]
        [PagingQueryable]
        public IEnumerable<CustomerAging> Get()
        {
            ICustomerService service = SpringFactory.GetObjectImpl<ICustomerService>("CustomerService");
            return service.GetCustomerAging();
        }

        [HttpPost]
        public String UploadFile(string levelFlag)
        {
            lock (strArchiveAccountKey)
            {
                // perform save file to local path
                HttpFileCollection files = HttpContext.Current.Request.Files;
                string msg = string.Empty;
                string archivePath = string.Empty;
                string archiveFileName = string.Empty;
                string archiveZIPFileName = string.Empty;
                FileType fileT = FileType.Account;
                FileUploadHistory accFileName = new FileUploadHistory();
                FileUploadHistory invFileName = new FileUploadHistory();
                FileUploadHistory invDetailFileName = new FileUploadHistory();
                FileUploadHistory vatFileName = new FileUploadHistory();
                //string strCode;
                string strMessage = string.Empty;
                FileService service = SpringFactory.GetObjectImpl<FileService>("FileService");
                bool isSaved = false;
                
                try
                {
                    if (files.Count > 0)
                    {
                        CustomerService custService = SpringFactory.GetObjectImpl<CustomerService>("CustomerService");
                        int fileId = 0;
                        if (levelFlag == "0")
                        {
                            fileT = FileType.Account;
                            archivePath = ConfigurationManager.AppSettings[strArchiveAccountKey].ToString();
                        }
                        else if (levelFlag == "1")
                        {
                            fileT = FileType.Invoice;
                            archivePath = ConfigurationManager.AppSettings[strArchiveInvoiceKey].ToString();
                        }                    
                        else if (levelFlag == "2")
                        {
                            fileT = FileType.InvoiceDetail;
                            archivePath = ConfigurationManager.AppSettings[strArchiveInvoiceDetailKey].ToString();
                        }
                        else if (levelFlag == "3")
                        {
                            fileT = FileType.VAT;
                            archivePath = ConfigurationManager.AppSettings[strArchiveVATKey].ToString();
                        }

                        archivePath = archivePath + DateTime.Now.Year.ToString() + "\\" + DateTime.Now.Month.ToString();

                        //arrow-此时archivePath（文件夹名字）为C盘+Account/invoice/oneyrarsales+年+//月
                        if (Directory.Exists(archivePath) == false)
                        {
                            Directory.CreateDirectory(archivePath);
                        }

                        archiveFileName = archivePath + "\\" + DateTime.Now.ToString("yyyy-MM-dd") + "-" + fileT.ToString() +
                                "-" + AppContext.Current.User.EID + "-" + DateTime.Now.ToString("HHmmssf");

                        //arrow- 如果是压缩包情况下
                        if (files[0].ContentType == "application/x-gzip" || files[0].ContentType == "application/x-tar")
                        {
                            string strFileName = files[0].FileName;
                            string strExtension = Path.GetExtension(files[0].FileName).ToUpper();
                            if (strExtension == ".GZ" || strExtension == ".TAR")
                            {                    
                                strFileName = archivePath + "\\" + files[0].FileName;
                                files[0].SaveAs(strFileName);

                                string strExtendName = "";
                                //如果有第二节扩展名，则获得第二节扩展名的文件名（.tar.gz）
                                string strInnerFileName = Path.GetFileNameWithoutExtension(files[0].FileName);
                                string strInnerExtension = Path.GetExtension(strInnerFileName).ToUpper();
                                if (strInnerExtension == ".TAR")
                                {
                                    strFileName = ungzip(strFileName, archiveFileName + strInnerExtension.ToLower(), true);
                                    strExtension = strInnerExtension;
                                }
                                //读取压缩包中的文件(如果是2次压缩，读取的已经是内层的文件了),判断是什么格式的内部文件
                                strExtendName = ".csv";
                                archiveFileName = archiveFileName + strExtendName;
                                if (strExtension.ToLower() == ".zip")
                                {
                                    using (ZipArchive zipArchive = System.IO.Compression.ZipFile.Open(strFileName, ZipArchiveMode.Read))
                                    {
                                        foreach (ZipArchiveEntry entry in zipArchive.Entries)
                                        {
                                            string strExtend = getFileExtendName(entry.Name).ToUpper();
                                            if (strExtend.ToUpper() == ".XLSX" || strExtend.ToUpper() == ".XLS" || strExtend.ToUpper() == ".CSV")
                                            {
                                                strExtendName = strExtend.ToLower();
                                                break;
                                            }
                                        }
                                    }
                                    unZip(strFileName, archiveFileName);
                                }
                                if (strExtension.ToLower() == ".gz")
                                {
                                    ungzip(strFileName, archiveFileName, true);
                                }
                                if (strExtension.ToLower() == ".tar")
                                {
                                    using (FileStream fr = new FileStream(strFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                                    {
                                        ICSharpCode.SharpZipLib.Tar.TarInputStream s = new ICSharpCode.SharpZipLib.Tar.TarInputStream(fr);
                                        ICSharpCode.SharpZipLib.Tar.TarEntry theEntry;
                                        while ((theEntry = s.GetNextEntry()) != null)
                                        {
                                            string fileName = Path.GetFileName(theEntry.Name);
                                            if (fileName != String.Empty)
                                            {
                                                string strExtend = getFileExtendName(fileName);
                                                if (strExtend.ToUpper() == ".XLSX" || strExtend.ToUpper() == ".XLS" || strExtend.ToUpper() == ".CSV")
                                                {
                                                    strExtendName = strExtend.ToLower();
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                    unTar(strFileName, archiveFileName, true);
                                }
                                isSaved = true;
                            }
                        }
                        else
                        {
                            string strFileName = files[0].FileName;
                            string strExtension = Path.GetExtension(files[0].FileName).ToUpper();
                            archiveFileName = archiveFileName + strExtension;
                        }
                 
                        //arrow-1不是zip的需要先放到C盘目录下+2Update to processflag-cancel（FileType，EID，Untreated）+3Insert the upload file record(Untreated)
                        custService.UploadFile(isSaved,files[0], archiveFileName, fileT,  ref fileId, true);

                        if (fileId!=0)
                        {
                            strMessage = fileId.ToString();
                        }
                        else
                        {
                            strMessage = "";
                        }                    
                    }

                    return strMessage;
                }
                catch (AgingImportException ex)
                {
                    Helper.Log.Error(ex.Message, ex);
                    //将DB中的未处理状态变成cancel状态
                    service.UpdateAllProcessFlagCancel();
                    throw new OTCServiceException(ex.Message);
                }
                catch (Exception ex)
                {
                    Helper.Log.Error(ex.Message, ex);
                    //将DB中的未处理状态变成cancel状态
                    service.UpdateAllProcessFlagCancel();
                    throw new OTCServiceException("Uploaded file error!");
                }
            }
        }

        public string getFileExtendName(string strFileName)
        {
            string strExtendName = "";
            if (Path.GetExtension(strFileName).ToUpper() == ".XLSX")
            {
                strExtendName = ".xlsx";
            }
            else if (Path.GetExtension(strFileName).ToUpper() == ".XLS")
            {
                strExtendName = ".xls";
            }
            else if (Path.GetExtension(strFileName).ToUpper() == ".CSV")
            {
                strExtendName = ".csv";
            }
            return strExtendName;
        }

        public void unZip(string zipfile, string archiveFileName)
        {
            if (!File.Exists(zipfile))
            {
                Helper.Log.Error(string.Format("Cannot find file '{0}'", zipfile), null);
                return;
            }
            using (ZipInputStream s = new ZipInputStream(File.OpenRead(zipfile)))
            {

                ZipEntry theEntry;
                while ((theEntry = s.GetNextEntry()) != null)
                {

                    Helper.Log.Info(theEntry.Name);

                    string directoryName = Path.GetDirectoryName(theEntry.Name);
                    string fileName = Path.GetFileName(theEntry.Name);

                    // create directory
                    if (directoryName.Length > 0)
                    {
                        Directory.CreateDirectory(directoryName);
                    }

                    if (fileName != String.Empty)
                    {
                        using (FileStream streamWriter = File.Create(archiveFileName))
                        {

                            int size = 2048;
                            byte[] data = new byte[2048];
                            while (true)
                            {
                                size = s.Read(data, 0, data.Length);
                                if (size > 0)
                                {
                                    streamWriter.Write(data, 0, size);
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
        //ADD 解压gz
        public string ungzip(string path, string decomPath, bool overwrite)
        {
            //for overwriting purposes
            if (File.Exists(decomPath))
            {
                if (overwrite)
                {
                    File.Delete(decomPath);
                }
                else
                {
                    string message = "The decompressed path you specified already exists and cannot be overwritten.";
                    Exception ex = new IOException(message);
                    Helper.Log.Error(message, ex);
                    throw ex;
                }
            }
            //create our file streams
            GZipStream stream = new GZipStream(new FileStream(path, FileMode.Open, FileAccess.ReadWrite), CompressionMode.Decompress);
            FileStream decompressedFile = new FileStream(decomPath, FileMode.OpenOrCreate, FileAccess.Write);
            //data represents a byte from the compressed file
            //it's set through each iteration of the while loop
            int data;
            while ((data = stream.ReadByte()) != -1) //iterates over the data of the compressed file and writes the decompressed data
            {
                decompressedFile.WriteByte((byte)data);
            }
            //close our file streams 
            decompressedFile.Close();
            stream.Close();
            return decomPath;
        }
        //END 解压gz

        /// <summary>
        /// 解压.tar
        /// </summary>
        /// <param name="path"></param>
        /// <param name="decomPath"></param>
        /// <param name="overwrite"></param>
        /// <returns></returns>
        public void unTar(string path, string decomPath, bool overwrite)
        {
            //for overwriting purposes
            if (File.Exists(decomPath))
            {
                if (overwrite)
                {
                    File.Delete(decomPath);
                }
                else
                {
                    string message = "The decompressed path you specified already exists and cannot be overwritten.";
                    Exception ex = new IOException(message);
                    Helper.Log.Error(message, ex);
                    throw ex;
                }
            }
            FileStream fr = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            ICSharpCode.SharpZipLib.Tar.TarInputStream s = new ICSharpCode.SharpZipLib.Tar.TarInputStream(fr);
            ICSharpCode.SharpZipLib.Tar.TarEntry theEntry;
            while ((theEntry = s.GetNextEntry()) != null)
            {
                string directoryName = Path.GetDirectoryName(theEntry.Name);
                string fileName = Path.GetFileName(theEntry.Name);

                if (fileName != String.Empty)
                {
                    FileStream streamWriter = File.Create(decomPath);
                    int size = 2048;
                    byte[] data = new byte[2048];
                    while (true)
                    {
                        size = s.Read(data, 0, data.Length);
                        if (size > 0)
                        {
                            streamWriter.Write(data, 0, size);
                        }
                        else
                        {
                            break;
                        }
                    }
                    streamWriter.Close();
                }
            }
            s.Close();
            fr.Close();
        }

        [HttpPost]
        public void Submit([FromBody]List<string> agingIds)
        {
            //CustomerService custService = SpringFactory.GetObjectImpl<CustomerService>("CustomerService");
            //custService.SubmitInitialAging(agingIds);
        }

        [HttpPost]
        [Route("api/collection/ProcessDealCollection")]
        public void ProcessDealCollection()
        {
            CollectionService custService = SpringFactory.GetObjectImpl<CollectionService>("CollectionService");
            custService.ProcessDealCollection(AppContext.Current.User.Deal);
        }

    }
}