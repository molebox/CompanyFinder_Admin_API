using CompanyDatabase.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompanyFinderAdminAPI.BindingModels
{
    /// <summary>
    /// Binding model to create a layer between the raw model data and the data requested
    /// </summary>
    public class SkillDetailFocusBindingModel
    {
        /// <summary>
        /// IEnummeration of the skill sets
        /// </summary>
        public IEnumerable<SkillSet> SkillSets { get; set; }
        /// <summary>
        /// IEnummeration of the details
        /// </summary>
        public IEnumerable<SkillDetail> SkillDetails { get; set; }
        /// <summary>
        /// IEnummeration of the focuses
        /// </summary>
        public IEnumerable<Focus> Focuses { get; set; }
    }
}
