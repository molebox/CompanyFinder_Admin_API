using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CompanyDatabase.Models
{
    public class CompanyDetails
    {
        [Key]
        public int CompanyDetailsId { get; set; }

        public int CompanyId { get; set; }
        public int SkillDetailId { get; set; }

        public virtual Companies Company { get; set; }
        public virtual SkillDetail SkillDetail { get; set; }
    }
}
