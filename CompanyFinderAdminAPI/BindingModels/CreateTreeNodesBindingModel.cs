using CompanyDatabase.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompanyFinderAdminAPI.BindingModels
{
    /// <summary>
    /// Binding model to hold thenew tree nodes
    /// </summary>
    public class CreateTreeNodesBindingModel
    {
        /// <summary>
        /// List of tree nodes
        /// </summary>
        public List<TreeNodes> TreeNodesList { get; set; }
        /// <summary>
        /// The new tree node
        /// </summary>
        public TreeNodes TreeNodes { get; set; }

        /// <summary>
        /// List of focus nodes
        /// </summary>
        public List<FocusNodes> FocusNodesList { get; set; }
        /// <summary>
        /// The new focus node
        /// </summary>
        public FocusNodes FocusNodes { get; set; }

    }
}
