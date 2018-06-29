using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompanyFinderAdminAPI.BindingModels
{
    /// <summary>
    /// Focus nodes view model with state
    /// </summary>
    public class FocusNodesWithState
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
        public virtual List<FocusNodesWithState> Children { get; set; }
        /// <summary>
        /// State of the node
        /// </summary>
        public State State { get; set; }
    }
}
