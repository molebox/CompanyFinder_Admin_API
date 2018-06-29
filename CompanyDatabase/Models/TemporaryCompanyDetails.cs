using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CompanyDatabase.Models
{
    public class TemporaryCompanyDetails
    {
        [Key]
        public int Id { get; set; }

        public int CompanyId { get; set; }
        public int SkillDetailId { get; set; }

        public virtual TemporaryCompanyTemplate Company { get; set; }
        public virtual SkillDetail SkillDetail { get; set; }
    }
}
