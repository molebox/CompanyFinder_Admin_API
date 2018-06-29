using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CompanyFinderAPI.BindingModels
{
    /// <summary>
    /// Binding model for the homepage
    /// </summary>
    public class HomePageBindingModel
    {
        /// <summary>
        /// Tagline for the home page
        /// </summary>
        public string Tagline { get; set; }

        /// <summary>
        /// Username to access the search
        /// </summary>
        [Required]
        [Display(Name = "Username")]
        public string SearchUsername { get; set; }
        /// <summary>
        /// Password to access the search
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string SearchPassword { get; set; }

        /// <summary>
        /// The return url
        /// </summary>
        public string ReturnUrl { get; set; } = "/";

       
    }
}
