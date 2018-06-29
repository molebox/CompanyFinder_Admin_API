using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompanyFinderAdminAPI.BindingModels
{
    /// <summary>
    /// Binding model to create a layer between the raw model data and the data requested
    /// </summary>
    public class HomePageBindingModel
    {
        /// <summary>
        /// Tagline Id
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Tagline for the home page
        /// </summary>
        public string Tagline { get; set; }
    }
}
