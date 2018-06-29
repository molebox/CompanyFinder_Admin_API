using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompanyFinderAdminAPI.BindingModels
{
    /// <summary>
    /// Binding model to create a layer between the raw model data and the data requested
    /// </summary>
    public class AddDetailsBindingModel
    {
        /// <summary>
        /// The detail id
        /// </summary>
        public int SkillDetailId { get; set; }
        /// <summary>
        /// The detail name
        /// </summary>
        public string DetailName { get; set; }
    }
}
