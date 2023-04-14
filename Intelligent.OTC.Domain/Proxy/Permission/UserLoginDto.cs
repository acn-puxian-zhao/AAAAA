using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Intelligent.OTC.Domain.Dtos
{
    public class UserLoginDto
    {
        [Required]
        [Display(Name = "UserCode")]
        public string UserCode { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }

        public string r { get; set; }
    }
}
