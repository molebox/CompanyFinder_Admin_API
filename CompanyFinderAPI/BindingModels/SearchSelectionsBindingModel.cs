using CompanyDatabase.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompanyFinderAPI.BindingModels
{
    /// <summary>
    /// Binding model for the clients search query
    /// </summary>
    public class SearchSelectionsBindingModel
    {
        /// <summary>
        /// Unique node array with selected roles checkboxes
        /// </summary>
        public List<TreeNodes> UniqueNodeArray { get; set; }
        /// <summary>
        /// Unique node array with selected focus checkboxes
        /// </summary>
        public List<FocusNodes> UniqueFocusNodeArray { get; set; }
    }
}
