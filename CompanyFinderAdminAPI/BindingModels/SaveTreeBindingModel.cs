using CompanyDatabase.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompanyFinderAdminAPI.BindingModels
{
    /// <summary>
    /// Binding model to save the tree from the client
    /// </summary>
    public class SaveTreeBindingModel
    {
        /// <summary>
        /// List to hold the treenodes
        /// </summary>
        public List<TreeNodes> TreeNodesList { get; set; }
        /// <summary>
        /// List to hold the treenodes
        /// </summary>
        public List<FocusNodes> FocusNodesList { get; set; }
    }
}
