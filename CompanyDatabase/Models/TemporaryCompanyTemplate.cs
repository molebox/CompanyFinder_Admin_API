using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace CompanyDatabase.Models
{
    public class TemporaryCompanyTemplate
    {
        public TemporaryCompanyTemplate()
        {
            TemporaryCompanyDetails = new HashSet<TemporaryCompanyDetails>();
            TemporaryCompanyFocuses = new HashSet<TemporaryCompanyFocus>();
            TemporaryCompanySkills = new HashSet<TemporaryCompanySkills>();
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
        [Display(Name = "Recruitment Address")]
        public string RecruitmentWebAddress { get; set; }
        public string Phone { get; set; }
        public string OtherNotes { get; set; }

        public Guid CompanyGuid { get; set; }

        public DateTime Sent_at { get; set; }
        public DateTime? Created_at { get; set; }

        public DateTime? Received_at { get; set; }

        public bool Locked { get; set; }

        public ICollection<TemporaryCompanySkills> TemporaryCompanySkills { get; set; }
        public ICollection<TemporaryCompanyDetails> TemporaryCompanyDetails { get; set; }
        public ICollection<TemporaryCompanyFocus> TemporaryCompanyFocuses { get; set; }

    }
}
