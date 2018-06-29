using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CompanyDatabase.Models
{
    public class CompanyFocus
    {
        [Key]
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public int FocusId { get; set; }

        public Companies Company { get; set; }
        public Focus Focus { get; set; }
    }
}
