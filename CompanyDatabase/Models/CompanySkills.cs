using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CompanyDatabase.Models
{
    public class CompanySkills
    {
        [Key]
        public int CompSkillId { get; set; }
        public int CompanyId { get; set; }
        public int SkillId { get; set; }


        public Companies Company { get; set; }

        public SkillSet SkillSet { get; set; }
    }
}
