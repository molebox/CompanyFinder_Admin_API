using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompanyFinderAdminAPI.BindingModels
{
    /// <summary>
    /// The state of the tree node
    /// </summary>
    public class State
    {
        /// <summary>
        /// Is the node already checked
        /// </summary>
        public bool Selected { get; set; }
    }
}
