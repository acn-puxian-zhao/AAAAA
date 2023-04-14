using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain.Dtos
{
    public class CaBsFileDto
    {
        public string ID { get; set; }

        public string BSID { get; set; }

        public string FILETYPE { get; set; }

        public string FILE_NAME { get; set; }

        //public string PHYSICAL_PATH { get; set; }

        public string CREATE_USER { get; set; }

        public DateTime? CREATE_TIME { get; set; }

        public bool DEL_FLAG { get; set; }

        public string transactionNum { get; set; }
    }

    public class CaBsFileDtoPage
    {
        public List<CaBsFileDto> dataRows;

        public int count;
    }
}
