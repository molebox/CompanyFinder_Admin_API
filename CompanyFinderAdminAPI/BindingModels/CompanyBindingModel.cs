using CompanyDatabase.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CompanyFinderAdminAPI.BindingModels
{
    /// <summary>
    /// Binding model to create a layer between the raw model data and the data requested
    /// </summary>
    public class CompanyBindingModel
    {
        /// <summary>
        /// The company id
        /// </summary>
        public int CompanyId { get; set; }
        /// <summary>
        /// The company name
        /// </summary>
        [Required]
        public string CompanyName { get; set; }
        /// <summary>
        /// The contact person
        /// </summary>
        [Required]
        public string ContactPerson { get; set; }
        /// <summary>
        /// The contact email
        /// </summary>
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        /// <summary>
        /// The company biography
        /// </summary>
        [Required]
        public string Bio { get; set; }
        /// <summary>
        /// The company website address
        /// </summary>
        [Required]
        [DataType(DataType.Url)]
        public string Website { get; set; }
        /// <summary>
        /// The contact phone number
        /// </summary>
        [Required]
        [DataType(DataType.PhoneNumber)]    
        public string Phone { get; set; }
        /// <summary>
        /// Company recruitment web address
        /// </summary>
        [Display(Name = "Recruitment Address")]
        public string RecruitmentWebAddress { get; set; }

        /// <summary>
        /// Company object to hold the actual company
        /// </summary>
        public Companies Company { get; set; }
        /// <summary>
        /// List of the associated skillsets
        /// </summary>
        public List<SkillSet> SkillSetsList { get; set; }
        /// <summary>
        /// List of the associated details
        /// </summary>
        public List<SkillDetail> SkillDetailsList { get; set; }
        /// <summary>
        /// List of all the focuses
        /// </summary>
        public List<Focus> FocusList { get; set; }
        /// <summary>
        /// List of the checked nodes ids
        /// </summary>
        public List<int> CheckedRolesNodes { get; set; }
        /// <summary>
        /// List of the checked nodes ids
        /// </summary>
        public List<int> CheckedFocusNodes { get; set; }
       
    }
   
}
