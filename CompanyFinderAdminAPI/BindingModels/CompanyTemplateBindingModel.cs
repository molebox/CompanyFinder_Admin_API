using CompanyDatabase.Models;
using CompanyFinderEmailTemplateLib.Services;
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
    public class CompanyTemplateBindingModel
    {
        /// <summary>
        /// Company Id
        /// </summary>
        public int CompanyId { get; set; }
        /// <summary>
        /// Company Name
        /// </summary>
        public string CompanyName { get; set; }
        /// <summary>
        /// Contact Person
        /// </summary>
        public string ContactPerson { get; set; }
        /// <summary>
        /// Company Email
        /// </summary>
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        /// <summary>
        /// Company website
        /// </summary>
        public string Website { get; set; }
        /// <summary>
        /// Company Recruitment website address
        /// </summary>
        public string RecruitmentWebAddress { get; set; }
        /// <summary>
        /// Company Biography
        /// </summary>
        public string Bio { get; set; }
        /// <summary>
        /// Company Phone Number
        /// </summary>
        [DataType(DataType.PhoneNumber)]
        public string Phone { get; set; }
        /// <summary>
        /// Any other information the company wantes to give
        /// </summary>
        public string OtherNotes { get; set; }

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
        /// Unique guid for each company template
        /// </summary>
        public Guid UniqueUrl { get; set; }

        /// <summary>
        /// Is the template locked
        /// </summary>
        public bool Locked { get; set; }
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

