using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace CompanyDatabase.Models
{
    public class SkillDetail
    {
        public SkillDetail()
        {

        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SkillDetailId { get; set; }
        public string DetailName { get; set; }


        public virtual ICollection<Companies> Companies { get; set; }

        [NotMapped]
        public bool IsSelected { get; set; }

    }
}
