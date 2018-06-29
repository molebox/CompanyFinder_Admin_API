using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace CompanyDatabase.Models
{
   public class Companies
    {
        public Companies()
        {
            CompanySkills = new HashSet<CompanySkills>();
            CompanyDetails = new HashSet<CompanyDetails>();
            CompanyFocuses = new HashSet<CompanyFocus>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CompanyId { get; set; }
        [Display(Name = "Company Name")]
        public string CompanyName { get; set; }
        [Display(Name = "Contact Person")]
        public string ContactPerson { get; set; }
        public string Email { get; set; }
        [Display(Name = "Biography")]
        public string Bio { get; set; }
        public string Website { get; set; }
        public string Phone { get; set; }

        [Display(Name = "Recruitment Address")]
        public string RecruitmentWebAddress { get; set; }

        public ICollection<CompanySkills> CompanySkills { get; set; }
        public ICollection<CompanyDetails> CompanyDetails { get; set; }
        public ICollection<CompanyFocus> CompanyFocuses { get; set; }

        [NotMapped]
        public List<SkillSet> SkillList { get; set; }
        [NotMapped]
        public List<SkillDetail> DetailList { get; set; }

        [NotMapped]
        public string TemplateStatus { get; set; }
    }
}
