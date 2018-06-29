using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompanyFinderAdminAPI.BindingModels
{
    /// <summary>
    /// Binding model to create a layer between the raw model data and the data requested
    /// </summary>
    public class AddFocusBindingModel
    {
        /// <summary>
        /// Focus Id
        /// </summary>
        public int FocusId { get; set; }
        /// <summary>
        /// Focus Name
        /// </summary>
        public string FocusType { get; set; }
    }
}
