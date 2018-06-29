using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace CompanyDatabase.Models
{
    public class SkillSet
    {
        public SkillSet()
        {

        }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SkillId { get; set; }
        public string SkillName { get; set; }

        public virtual ICollection<Companies> Companies { get; set; }

        [NotMapped]
        public bool IsSelected { get; set; }
    }
}
