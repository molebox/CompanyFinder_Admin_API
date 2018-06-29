using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompanyFinderAdminAPI.BindingModels
{
    /// <summary>
    /// Binding model to create a layer between the raw model data and the data requested
    /// </summary>
    public class AddSkillsBindingModel
    {
        /// <summary>
        /// Skill id
        /// </summary>
        public int SkillId { get; set; }
        /// <summary>
        /// Skill name
        /// </summary>
        public string SkillName { get; set; }

        /// <summary>
        /// Is the skill a parent of another skill?
        /// </summary>
        public bool IsParent { get; set; }
    }
}
