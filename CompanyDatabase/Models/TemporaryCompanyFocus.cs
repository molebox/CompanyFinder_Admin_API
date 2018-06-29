using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CompanyDatabase.Models
{
    public class TemporaryCompanyFocus
    {
        [Key]
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public int FocusId { get; set; }

        public TemporaryCompanyTemplate Company { get; set; }
        public Focus Focus { get; set; }
    }
}
