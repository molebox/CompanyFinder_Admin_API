using CompanyDatabase.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompanyFinderAdminAPI.BindingModels
{
    /// <summary>
    /// Binding model for tree nodes with state set for editing company data
    /// </summary>
    public class TreeNodesWithState
    {
        /// <summary>
        /// The nodes id
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// The node name
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        /// The nodes parent id
        /// </summary>
        public int? ParentId { get; set; }
        /// <summary>
        /// Nodes Order number
        /// </summary>
        public int OrderNumber { get; set; }
        /// <summary>
        /// Nodes Children
        /// </summary>
        public virtual List<TreeNodesWithState> Children { get; set; }
        /// <summary>
        /// State of the node
        /// </summary>
        public State State { get; set; }

    }
   
}
