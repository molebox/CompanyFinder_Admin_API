using CompanyDatabase.Models;
using CompanyFinderEmailTemplateLib.Services;
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
    public class SendTemplateBindingModel
    {
        /// <summary>
        /// Constructor to initialize the temporary company list
        /// </summary>
        public SendTemplateBindingModel()
        {
            CompanyList = new List<TemporaryCompanyTemplate>();
        }
        /// <summary>
        /// Company Name
        /// </summary>
        public string CompanyName { get; set; }
        /// <summary>
        /// Company Email
        /// </summary>
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        /// <summary>
        /// Email object
        /// </summary>
        public EmailMessage EmailMessage { get; set; }
        /// <summary>
        /// Email Address to send to
        /// </summary>
        public EmailAddress EmailAddressToSend { get; set; }
        /// <summary>
        /// States who the email is from
        /// </summary>
        public string FromEmail { get; set; }
        /// <summary>
        /// The Email subject
        /// </summary>
        public string EmailSubject { get; set; }
        /// <summary>
        /// The Email content
        /// </summary>
        public string EmailContent { get; set; }
        /// <summary>
        /// Unique Url for template link
        /// </summary>
        public string UniqueUrl { get; set; }
        /// <summary>
        /// Template link for email
        /// </summary>
        public string Link { get; set; }

        /// <summary>
        /// A list of the companies that the template has been sent to
        /// </summary>
        public List<TemporaryCompanyTemplate> CompanyList { get; set; }
        /// <summary>
        /// Unique guid for each company template
        /// </summary>
        public Guid CompanyGuid { get; set; }
        /// <summary>
        /// Company recruitment web address
        /// </summary>
        [Display(Name = "Recruitment Address")]
        public string RecruitmentWebAddress { get; set; }
    }
}
