using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CompanyDatabase.Models
{
    public class TemporaryCompanySkills
    {
        [Key]
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public int SkillId { get; set; }


        public TemporaryCompanyTemplate Company { get; set; }

        public SkillSet SkillSet { get; set; }
    }
}
