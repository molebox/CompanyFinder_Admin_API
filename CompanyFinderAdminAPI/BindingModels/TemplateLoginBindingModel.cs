using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CompanyFinderAdminAPI.BindingModels
{
    /// <summary>
    /// Binding model for the template login
    /// </summary>
    public class TemplateLoginBindingModel
    {
        /// <summary>
        /// The admin id
        /// </summary>
        public int TemplateAccessID { get; set; }
        /// <summary>
        /// The Admin username
        /// </summary>
        [Required]
        [Display(Name = "Username")]
        public string CompanyAccessName { get; set; }
        /// <summary>
        /// The admin password
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string CompanyAccessPassword { get; set; }
    }
}
