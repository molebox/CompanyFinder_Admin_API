using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CompanyFinderAdminAPI.BindingModels
{
    /// <summary>
    /// Binding model to create a layer between the raw model data and the data requested
    /// </summary>
    public class AdminLoginBindingModel
    {
        /// <summary>
        /// The admin id
        /// </summary>
        public int AdminID { get; set; }
        /// <summary>
        /// The Admin username
        /// </summary>
        [Required]
        public string AdminName { get; set; }
        /// <summary>
        /// The admin password
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        public string AdminPassword { get; set; }
    }
}
