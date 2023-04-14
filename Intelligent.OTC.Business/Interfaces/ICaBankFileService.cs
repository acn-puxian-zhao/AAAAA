using Intelligent.OTC.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Intelligent.OTC.Business.Interfaces
{
    public interface ICaBankFileService
    {
        string saveBSFile(HttpPostedFile file, CaBankStatementDto bank);

        List<CaBsFileWithPathDto> GetFilesByBankId(string bankId);

        CaBsFileDtoPage GetFileList(string transactionNum, string fileName, string fileType, string valueDateF, string valueDateT, int page, int pageSize);

        void deleteFileById(string fileId);
    }
}
