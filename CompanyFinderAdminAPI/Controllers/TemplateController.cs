using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CompanyDatabase.Models;
using CompanyFinderAdminAPI.BindingModels;
using CompanyFinderAdminAPI.Extension_Methods;
using CompanyFinderEmailTemplateLib.Interfaces;
using CompanyFinderEmailTemplateLib.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MimeKit;
using Newtonsoft.Json.Linq;

namespace CompanyFinderAdminAPI.Controllers
{
    /// <summary>
    /// Template controller for handling template requests
    /// </summary>
    [Produces("application/json")]
    [Route("api/Template")]
    [Authorize]
    public class TemplateController : Controller
    {
        private CompanyDbContext _context;
        private IEmailService _emailService;
        private UserManager<IdentityUser> _userManger;
        private SignInManager<IdentityUser> _signInManager;
        private IConfiguration _config;

        /// <summary>
        /// Constructor, initializes the db context
        /// </summary>
        /// <param name="context"></param>
        /// <param name="emailService"></param>
        /// <param name="userMgr"></param>
        /// <param name="signInMgr"></param>
        /// <param name="Configuration"></param>
        public TemplateController(CompanyDbContext context, IEmailService emailService, UserManager<IdentityUser> userMgr, SignInManager<IdentityUser> signInMgr, IConfiguration Configuration)
        {
            _context = context;
            _emailService = emailService;
            _userManger = userMgr;
            _signInManager = signInMgr;
            _config = Configuration;
        }

    
        private string SenderAddress
        {
            get
            {
                return _config["EmailConfiguration:SenderAddress"];
            }
        }
        private string SenderName
        {
            get
            {
                return _config["EmailConfiguration:SenderName"];
            }
        }


        private IEnumerable<Companies> GetAllCompanies() => _context.Companies;


        private IEnumerable<Focus> GetAllFocuses() => _context.Focus.ToList();

        private IEnumerable<SkillDetail> GetAllSkillDetails() => _context.SkillDetail.ToList();


        private IEnumerable<SkillSet> GetAllSkills() => _context.SkillSet.ToList();

        private IEnumerable<CompanySkills> GetCompanySkills() => _context.CompanySkills;

        private Companies GetCompById(int id) => GetAllCompanies().FirstOrDefault(comp => comp.CompanyId == id);


        private IEnumerable<TemporaryCompanyTemplate> GetAllCompaniesFromTempTable() => _context.TemporaryCompanyTemplate;
        private IEnumerable<EmailTemplate> GetEmailTemplateInfo => _context.EmailTemplates.ToList();

        /// <summary>
        /// POST to check the login, if successful then access is gained to the company template 
        /// </summary>
        /// <param name="templateLoginBindingModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("~/api/template/templatelogin")]
        [AllowAnonymous]
        public async Task<IActionResult> TemplateLogin([FromForm]TemplateLoginBindingModel templateLoginBindingModel)
        {
            if (ModelState.IsValid)
            {
                IdentityUser user = await _userManger.FindByNameAsync(templateLoginBindingModel.CompanyAccessName);

                if (user != null)
                {
                    await _signInManager.SignOutAsync();
                    if ((await _signInManager.PasswordSignInAsync(user, templateLoginBindingModel.CompanyAccessPassword, false, false)).Succeeded)
                    {
                        return Ok(templateLoginBindingModel);
                    }

                }
            }
            return Unauthorized();
        }

        private IEnumerable<TreeNodes> ApiGetAllTreeNodes() => _context.TreeNodes.ToList();

        private IEnumerable<FocusNodes> ApiGetAllFocusNodes() => _context.FocusNodes.ToList();

        /// <summary>
        /// Get tree nodes for the edit company 
        /// </summary>
        /// <returns></returns>
        [HttpGet("{id}")]
        [Route("~/api/template/editcompanytreenodes/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetEditCompanyTreeNodes(int id)
        {
            var company = await _context.TemporaryCompanyTemplate.SingleOrDefaultAsync(m => m.CompanyId == id);

            var compToEditSkills = _context.TemporaryCompanySkills
               .Where(c => c.CompanyId == id)
               .ToList();

            var compToEditDetails = _context.TemporaryCompanyDetails
                .Where(c => c.CompanyId == id)
                .ToList();

            var treeNodes = ApiGetAllTreeNodes();
            var tempTreeNodeList = new List<TreeNodesWithState>();

            foreach (var item in treeNodes)
            {
                var newNode = new TreeNodesWithState
                {
                    Id = item.Id,
                    Text = item.Name,
                    OrderNumber = item.OrderNumber,
                    ParentId = item.ParentId
                };
                tempTreeNodeList.Add(newNode);
            }

            //Loop through the companies saved skills and add the treenode via the id to the list to send back
            foreach (var db_node in tempTreeNodeList)
            {
                db_node.State = new State();

                foreach (var skill in compToEditSkills)
                {
                    if (db_node.Id == skill.SkillId)
                    {
                        db_node.State.Selected = true;
                    }
                }
            }
            //Loop through the companies saved details and add the treenode via the id to the list to send back
            foreach (var db_node in tempTreeNodeList)
            {
                db_node.State = new State();

                foreach (var detail in compToEditDetails)
                {
                    if (db_node.Id == detail.SkillDetailId)
                    {
                        db_node.State.Selected = true;
                    }
                }
            }

            //Create the tree structure from the skill and detail nodes
            List<TreeNodesWithState> nodeLists = StaticTreeNodeManipulation.CreateTreeWithState(tempTreeNodeList);

            // Return the node list model as json 
            var result = new ObjectResult(nodeLists);
            return result;
        }
        /// <summary>
        /// Get tree nodes for the edit company 
        /// </summary>
        /// <returns></returns>
        [HttpGet("{id}")]
        [Route("~/api/template/edittemplatetreenodes/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetEditTemplateTreeNodes(int id)
        {
            var company = await _context.TemporaryCompanyTemplate.SingleOrDefaultAsync(m => m.CompanyId == id);

            var compToEditSkills = _context.TemporaryCompanySkills
               .Where(c => c.CompanyId == id)
               .ToList();

            var compToEditDetails = _context.TemporaryCompanyDetails
                .Where(c => c.CompanyId == id)
                .ToList();

            var treeNodes = ApiGetAllTreeNodes();
            var tempTreeNodeList = new List<TreeNodesWithState>();

            foreach (var item in treeNodes)
            {
                var newNode = new TreeNodesWithState
                {
                    Id = item.Id,
                    Text = item.Name,
                    OrderNumber = item.OrderNumber,
                    ParentId = item.ParentId
                };
                tempTreeNodeList.Add(newNode);
            }

            //Loop through the companies saved skills and add the treenode via the id to the list to send back
            foreach (var db_node in tempTreeNodeList)
            {
                db_node.State = new State();

                foreach (var skill in compToEditSkills)
                {
                    if (db_node.Id == skill.SkillId)
                    {
                        db_node.State.Selected = true;
                    }
                }
            }
            //Loop through the companies saved details and add the treenode via the id to the list to send back
            foreach (var db_node in tempTreeNodeList)
            {
                db_node.State = new State();

                foreach (var detail in compToEditDetails)
                {
                    if (db_node.Id == detail.SkillDetailId)
                    {
                        db_node.State.Selected = true;
                    }
                }
            }

            //Create the tree structure from the skill and detail nodes
            List<TreeNodesWithState> nodeLists = StaticTreeNodeManipulation.CreateTreeWithState(tempTreeNodeList);

            // Return the node list model as json 
            var result = new ObjectResult(nodeLists);
            return result;
        }


        /// <summary>
        /// Get tree nodes for the edit company 
        /// </summary>
        /// <returns></returns>
        [HttpGet("{id}")]
        [Route("~/api/template/editcompanyfocusnodes/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetEditCompanyFocusNodes(int id)
        {
            var company = await _context.TemporaryCompanyTemplate.SingleOrDefaultAsync(m => m.CompanyId == id);

            var compToEditFocuses = _context.TemporaryCompanyFocus
                .Where(f => f.CompanyId == id)
                .ToList();

            var focusNodes = ApiGetAllFocusNodes();
            var tempFocusNodeList = new List<FocusNodesWithState>();

            foreach (var item in focusNodes)
            {
                var newNode = new FocusNodesWithState
                {
                    Id = item.Id,
                    Text = item.Name,
                    OrderNumber = item.OrderNumber,
                    ParentId = item.ParentId
                };
                tempFocusNodeList.Add(newNode);
            }

            //Loop through the companies saved focuses and add the focus node via the id to the list to send back
            foreach (var db_node in tempFocusNodeList)
            {
                db_node.State = new State();

                foreach (var focus in compToEditFocuses)
                {
                    if (db_node.Id == focus.FocusId)
                    {
                        db_node.State.Selected = true;
                    }
                }
            }

            //Create the tree structure from the focus nodes
            List<FocusNodesWithState> nodeLists = StaticTreeNodeManipulation.CreateFocusTreeWithState(tempFocusNodeList);

            // Return the node list model as json 
            var result = new ObjectResult(nodeLists);
            return result;

        }

        /// <summary>
        /// Get tree nodes for the edit company 
        /// </summary>
        /// <returns></returns>
        [HttpGet("{id}")]
        [Route("~/api/template/edittemplatefocusnodes/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetEditTemplateFocusNodes(int id)
        {
            var company = await _context.TemporaryCompanyTemplate.SingleOrDefaultAsync(m => m.CompanyId == id);

            var compToEditFocuses = _context.TemporaryCompanyFocus
                .Where(f => f.CompanyId == id)
                .ToList();

            var focusNodes = ApiGetAllFocusNodes();
            var tempFocusNodeList = new List<FocusNodesWithState>();

            foreach (var item in focusNodes)
            {
                var newNode = new FocusNodesWithState
                {
                    Id = item.Id,
                    Text = item.Name,
                    OrderNumber = item.OrderNumber,
                    ParentId = item.ParentId
                };
                tempFocusNodeList.Add(newNode);
            }

            //Loop through the companies saved focuses and add the focus node via the id to the list to send back
            foreach (var db_node in tempFocusNodeList)
            {
                db_node.State = new State();

                foreach (var focus in compToEditFocuses)
                {
                    if (db_node.Id == focus.FocusId)
                    {
                        db_node.State.Selected = true;
                    }
                }
            }

            //Create the tree structure from the focus nodes
            List<FocusNodesWithState> nodeLists = StaticTreeNodeManipulation.CreateFocusTreeWithState(tempFocusNodeList);

            // Return the node list model as json 
            var result = new ObjectResult(nodeLists);
            return result;

        }


        /// <summary>
        /// Get the template by the company id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        [Route("~/api/template/gettemplate/{id}")]
        public IActionResult GetTemplate(int id)
        {
            var company = _context.TemporaryCompanyTemplate.SingleOrDefault(m => m.CompanyId == id);

            var compToEditSkills = _context.TemporaryCompanySkills
                .Where(c => c.CompanyId == id)
                .ToList();

            var compToEditDetails = _context.TemporaryCompanyDetails
                .Where(c => c.CompanyId == id)
                .ToList();

            var compToEditFocuses = _context.TemporaryCompanyFocus
                .Where(f => f.CompanyId == id)
                .ToList();

            var companyToEdit = new CompanyTemplateBindingModel
            {
                CompanyId = company.CompanyId,
                CompanyName = company.CompanyName,
                ContactPerson = company.ContactPerson,
                Email = company.Email,
                Phone = company.Phone,
                Website = company.Website,
                Bio = company.Bio,
                UniqueUrl = company.CompanyGuid,
                RecruitmentWebAddress = company.RecruitmentWebAddress,
                OtherNotes = company.OtherNotes
            };

            if (compToEditFocuses == null)
            {
                var focuses = GetAllFocuses().ToList();
                companyToEdit.FocusList = new List<Focus>();

                //set the selected focuses
                foreach (var compFocus in compToEditFocuses)
                {
                    var chosenFocus = _context.Focus.First(d => d.FocusId == compFocus.FocusId);

                    foreach (var focus in focuses)
                    {
                        if (focus.FocusId == chosenFocus.FocusId)
                        {
                            focus.IsSelected = true;
                        }
                    }
                }

                foreach (var focus in focuses)
                {
                    companyToEdit.FocusList.Add(focus);
                }
            }

            if (compToEditSkills == null)
            {
                var details = GetAllSkillDetails().ToList();
                companyToEdit.SkillDetailsList = new List<SkillDetail>();

                //set the selected details
                foreach (var detail in compToEditDetails)
                {
                    var chosenDetail = _context.SkillDetail.First(d => d.SkillDetailId == detail.SkillDetailId);

                    foreach (var skillDetail in details)
                    {
                        if (skillDetail.SkillDetailId == chosenDetail.SkillDetailId)
                        {
                            skillDetail.IsSelected = true;
                        }
                    }
                }

                foreach (var detail in details)
                {
                    companyToEdit.SkillDetailsList.Add(detail);
                }
            }
            else if (compToEditDetails == null)
            {
                var skills = GetAllSkills().ToList();
                companyToEdit.SkillSetsList = new List<SkillSet>();

                //set the selected skills
                foreach (var skill in compToEditSkills)
                {
                    var chosenSkill = _context.SkillSet.First(s => s.SkillId == skill.SkillId);

                    foreach (var skillSet in skills)
                    {
                        if (skillSet.SkillId == chosenSkill.SkillId)
                        {
                            skillSet.IsSelected = true;
                        }
                    }
                }
                foreach (var skill in skills)
                {
                    companyToEdit.SkillSetsList.Add(skill);
                }
            }
            else
            {
                //Get the skills lists from the db
                var skills =  GetAllSkills().ToList();
                var details = GetAllSkillDetails().ToList();
                var focuses = GetAllFocuses().ToList();

                companyToEdit.SkillSetsList = new List<SkillSet>();
                companyToEdit.SkillDetailsList = new List<SkillDetail>();
                companyToEdit.FocusList = new List<Focus>();


                //set the selected focuses
                foreach (var focus in compToEditFocuses)
                {
                    var chosenFocus = _context.Focus.First(s => s.FocusId == focus.FocusId);

                    foreach (var compFocus in focuses)
                    {
                        if (compFocus.FocusId == chosenFocus.FocusId)
                        {
                            compFocus.IsSelected = true;
                        }
                    }
                }
                //set the selected skills
                foreach (var skill in compToEditSkills)
                {
                    var chosenSkill = _context.SkillSet.First(s => s.SkillId == skill.SkillId);

                    foreach (var skillSet in skills)
                    {
                        if (skillSet.SkillId == chosenSkill.SkillId)
                        {
                            skillSet.IsSelected = true;
                        }
                    }
                }
                //set the selected details
                foreach (var detail in compToEditDetails)
                {
                    var chosenDetail = _context.SkillDetail.First(d => d.SkillDetailId == detail.SkillDetailId);

                    foreach (var skillDetail in details)
                    {
                        if (skillDetail.SkillDetailId == chosenDetail.SkillDetailId)
                        {
                            skillDetail.IsSelected = true;
                        }
                    }
                }

                //set the view model lists to reflect the currently selected skills
                foreach (var skill in skills)
                {
                    companyToEdit.SkillSetsList.Add(skill);
                }
                foreach (var detail in details)
                {
                    companyToEdit.SkillDetailsList.Add(detail);
                }
                foreach (var focus in focuses)
                {
                    companyToEdit.FocusList.Add(focus);
                }
            }
            var result = new ObjectResult(companyToEdit);
            return result;
        }

        /// <summary>
        ///  POST once template is approved the changes are pushed to the real db and the temp data deleted from the temp tables
        /// </summary>
        /// <param name="template"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [Route("~/api/template/posttemplate")]
        public async Task<IActionResult> GetTemplate([FromBody]CompanyTemplateBindingModel template)
        {
            //CompanyTemplateBindingModel template = value.ToObject<CompanyTemplateBindingModel>();

            int newId = 0;

            if (!GetAllCompanies().Any())
            {
                newId = 1;
            }
            else
            {
                // Get the last entered Id from the db table and +1 ready to assign to new companyToEdit
                var lastId = GetAllCompanies().OrderByDescending(i => i.CompanyId).First();
                newId = lastId.CompanyId;
                newId++;
            }


            var newCompany = new Companies
            {
                CompanyId = newId,
                CompanyName = template.CompanyName,
                ContactPerson = template.ContactPerson,
                Email = template.Email,
                Phone = template.Phone,
                Bio = template.Bio,
                Website = template.Website,
                RecruitmentWebAddress = template.RecruitmentWebAddress
            };

            if (template.CheckedFocusNodes.Count() >= 1)
            {
                var selectedFocuses = ApiGetAllFocus().ToList();

                foreach (var focus in selectedFocuses)
                {                 
                    // Create the new rows for the new companies companySkills
                    foreach (var selectedFocus in template.CheckedFocusNodes)
                    {
                        if(focus.FocusId == selectedFocus)
                        {
                            var compFocus = new CompanyFocus
                            {
                                CompanyId = newCompany.CompanyId,
                                FocusId = selectedFocus
                            };
                            newCompany.CompanyFocuses.Add(compFocus);
                        }                   
                    }
                }             
            }

            if (template.CheckedRolesNodes.Count() >= 1)
            {
                var selectedSkills = ApiGetAllSkills().ToList();
                var selectedDetails = ApiGetAllDetails().ToList();

                foreach (var skill in selectedSkills)
                {
                    // Create the new rows for the new companies companySkills
                    foreach (var selectedSkill in template.CheckedRolesNodes)
                    {
                        if(skill.SkillId == selectedSkill)
                        {
                            var compSkill = new CompanySkills
                            {
                                CompanyId = newCompany.CompanyId,
                                SkillId = selectedSkill
                            };
                            newCompany.CompanySkills.Add(compSkill);
                        }                      
                    }
                }

                foreach (var detail in selectedDetails)
                {
                    foreach (var selectedDetail in template.CheckedRolesNodes)
                    {
                        if(detail.SkillDetailId == selectedDetail)
                        {
                            var compDetail = new CompanyDetails
                            {
                                CompanyId = newCompany.CompanyId,
                                SkillDetailId = selectedDetail
                            };
                            newCompany.CompanyDetails.Add(compDetail);
                        }                 
                    }
                }               
            }

                if (ModelState.IsValid)
                {
                    var foundCompany = _context.TemporaryCompanyTemplate.SingleOrDefault(c => c.CompanyId == template.CompanyId);

                    // Remove the company from the companySkills table via the id
                    var removeCompSkills = _context.TemporaryCompanySkills.Where(c => c.Company.CompanyId == template.CompanyId);

                    _context.TemporaryCompanySkills.RemoveRange(removeCompSkills);

                    // Remove the company from the companySkills table via the id
                    var removeCompDetails = _context.TemporaryCompanyDetails.Where(c => c.Company.CompanyId == template.CompanyId);

                    _context.TemporaryCompanyDetails.RemoveRange(removeCompDetails);

                    // Remove the company from the companyFocus table via the id
                    var removeCompFocus = _context.TemporaryCompanyFocus.Where(c => c.Company.CompanyId == template.CompanyId);

                    _context.TemporaryCompanyFocus.RemoveRange(removeCompFocus);

                    // Delete the company from the temp table before filling back in with the submitted form details
                    _context.TemporaryCompanyTemplate.Remove(foundCompany);

                    _context.SaveChanges();

                    await CreateCompany(newCompany);

                    var result = new ObjectResult(template);
                    return result;
                }

            
            return NotFound();
        }

        /// <summary>
        /// Unlock the template for the company to edit
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [AllowAnonymous]
        [Route("~/api/template/unlocktemplate/{id}")]
        public IActionResult UnlockTemplate(int id)
        {
            var company = _context.TemporaryCompanyTemplate.SingleOrDefault(m => m.CompanyId == id);

            if (company != null)
            {
                company.Locked = false;

                _context.Update(company);

                _context.SaveChanges();

                return Ok();
            }
            return NotFound();
        }
        /// <summary>
        /// Unlock the template for the company to edit
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [AllowAnonymous]
        [Route("~/api/template/removecompanyfromtemptable/{id}")]
        public IActionResult RemoveCompanyFromTempTable(int id)
        {
            var company = _context.TemporaryCompanyTemplate.SingleOrDefault(m => m.CompanyId == id);

            if (company != null)
            {
                // Remove the company from the companySkills table via the id
                var removeCompSkills = _context.TemporaryCompanySkills.Where(c => c.Company.CompanyGuid == company.CompanyGuid);

                _context.TemporaryCompanySkills.RemoveRange(removeCompSkills);

                // Remove the company from the companySkills table via the id
                var removeCompDetails = _context.TemporaryCompanyDetails.Where(c => c.Company.CompanyGuid == company.CompanyGuid);

                _context.TemporaryCompanyDetails.RemoveRange(removeCompDetails);

                // Remove the company from the companyFocus table via the id
                var removeCompFocus = _context.TemporaryCompanyFocus.Where(c => c.Company.CompanyGuid == company.CompanyGuid);

                _context.TemporaryCompanyFocus.RemoveRange(removeCompFocus);

                // Delete the company from the temp table
                _context.TemporaryCompanyTemplate.Remove(company);

                _context.SaveChanges();

                return Ok();
            }
            return NotFound();
        }

        /// <summary>
        /// Get a template for editing based on the companies unique guid 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [AllowAnonymous]
        [Route("~/api/template/companytemplate/{id}")]
        public async Task<IActionResult> CompanyTemplate(Guid id)
        {
            foreach (var comp in GetAllCompaniesFromTempTable())
            {
                if (comp.CompanyGuid == id && comp.Locked == true)
                {
                    var companyLocked = new CompanyTemplateBindingModel
                    {
                        CompanyId = comp.CompanyId,
                        CompanyName = comp.CompanyName,
                        ContactPerson = comp.ContactPerson,
                        Email = comp.Email,
                        Bio = comp.Bio,
                        Website = comp.Website,
                        RecruitmentWebAddress = comp.RecruitmentWebAddress,
                        Phone = comp.Phone,
                        OtherNotes = comp.OtherNotes,
                        UniqueUrl = comp.CompanyGuid,
                        Locked = true
                    };

                    var resultLocked = new ObjectResult(companyLocked);
                    return resultLocked;
                }
            }

            var company = await _context.TemporaryCompanyTemplate.SingleOrDefaultAsync(m => m.CompanyGuid == id);

            var compToEditSkills = _context.TemporaryCompanySkills
                .Where(c => c.Company.CompanyGuid == id)
                .ToList();

            var compToEditDetails = _context.TemporaryCompanyDetails
                .Where(c => c.Company.CompanyGuid == id)
                .ToList();

            var compToEditFocuses = _context.TemporaryCompanyFocus
                .Where(f => f.Company.CompanyGuid == id)
                .ToList();

            var companyToEdit = new CompanyTemplateBindingModel
            {
                CompanyId = company.CompanyId,
                CompanyName = company.CompanyName,
                ContactPerson = company.ContactPerson,
                Email = company.Email,
                Phone = company.Phone,
                Website = company.Website,
                Bio = company.Bio,
                UniqueUrl = id,
                RecruitmentWebAddress = company.RecruitmentWebAddress,
                OtherNotes = company.OtherNotes
            };

            if (compToEditFocuses == null)
            {
                var focuses = GetAllFocuses().ToList();
                companyToEdit.FocusList = new List<Focus>();

                //set the selected focuses
                foreach (var compFocus in compToEditFocuses)
                {
                    var chosenFocus = _context.Focus.First(d => d.FocusId == compFocus.FocusId);

                    foreach (var focus in focuses)
                    {
                        if (focus.FocusId == chosenFocus.FocusId)
                        {
                            focus.IsSelected = true;
                        }
                    }
                }

                foreach (var focus in focuses)
                {
                    companyToEdit.FocusList.Add(focus);
                }
            }

            if (compToEditSkills == null)
            {
                var details = GetAllSkillDetails().ToList();
                companyToEdit.SkillDetailsList = new List<SkillDetail>();

                //set the selected details
                foreach (var detail in compToEditDetails)
                {
                    var chosenDetail = _context.SkillDetail.First(d => d.SkillDetailId == detail.SkillDetailId);

                    foreach (var skillDetail in details)
                    {
                        if (skillDetail.SkillDetailId == chosenDetail.SkillDetailId)
                        {
                            skillDetail.IsSelected = true;
                        }
                    }
                }

                foreach (var detail in details)
                {
                    companyToEdit.SkillDetailsList.Add(detail);
                }
            }
            else if (compToEditDetails == null)
            {
                var skills = GetAllSkills().ToList();
                companyToEdit.SkillSetsList = new List<SkillSet>();

                //set the selected skills
                foreach (var skill in compToEditSkills)
                {
                    var chosenSkill = _context.SkillSet.First(s => s.SkillId == skill.SkillId);

                    foreach (var skillSet in skills)
                    {
                        if (skillSet.SkillId == chosenSkill.SkillId)
                        {
                            skillSet.IsSelected = true;
                        }
                    }
                }
                foreach (var skill in skills)
                {
                    companyToEdit.SkillSetsList.Add(skill);
                }
            }
            else
            {
                //Get the skills lists from the db
                var skills = GetAllSkills().ToList();
                var details = GetAllSkillDetails().ToList();
                var focuses = GetAllFocuses().ToList();

                companyToEdit.SkillSetsList = new List<SkillSet>();
                companyToEdit.SkillDetailsList = new List<SkillDetail>();
                companyToEdit.FocusList = new List<Focus>();


                //set the selected focuses
                foreach (var focus in compToEditFocuses)
                {
                    var chosenFocus = _context.Focus.First(s => s.FocusId == focus.FocusId);

                    foreach (var compFocus in focuses)
                    {
                        if (compFocus.FocusId == chosenFocus.FocusId)
                        {
                            compFocus.IsSelected = true;
                        }
                    }
                }
                //set the selected skills
                foreach (var skill in compToEditSkills)
                {
                    var chosenSkill = _context.SkillSet.First(s => s.SkillId == skill.SkillId);

                    foreach (var skillSet in skills)
                    {
                        if (skillSet.SkillId == chosenSkill.SkillId)
                        {
                            skillSet.IsSelected = true;
                        }
                    }
                }
                //set the selected details
                foreach (var detail in compToEditDetails)
                {
                    var chosenDetail = _context.SkillDetail.First(d => d.SkillDetailId == detail.SkillDetailId);

                    foreach (var skillDetail in details)
                    {
                        if (skillDetail.SkillDetailId == chosenDetail.SkillDetailId)
                        {
                            skillDetail.IsSelected = true;
                        }
                    }
                }

                //set the view model lists to reflect the currently selected skills
                foreach (var skill in skills)
                {
                    companyToEdit.SkillSetsList.Add(skill);
                }
                foreach (var detail in details)
                {
                    companyToEdit.SkillDetailsList.Add(detail);
                }
                foreach (var focus in focuses)
                {
                    companyToEdit.FocusList.Add(focus);
                }
            }


            var result = new ObjectResult(companyToEdit);
            return result;
        }

        private IEnumerable<EmailTemplate> ApiGetEmailTemplateData() => _context.EmailTemplates.ToList();
        private IEnumerable<SkillDetail> ApiGetAllDetails() => _context.SkillDetail.ToList();
        private IEnumerable<Focus> ApiGetAllFocus() => _context.Focus.ToList();
        private IEnumerable<Companies> ApiGetAllCompanies() => _context.Companies.ToList();
        private IEnumerable<SkillSet> ApiGetAllSkills() => _context.SkillSet.ToList();

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="template"></param>
        ///// <returns></returns>
        //[HttpPost]
        //[AllowAnonymous]
        //[Route("~/api/template/submittedtemplate")]
        //public async Task<IActionResult> SubmittedCompanyTemplate([FromBody]CompanyTemplateBindingModel template)
        //{

        //    var skillSets = ApiGetAllSkills();
        //    var skillDetails = ApiGetAllDetails();
        //    var focuses = ApiGetAllFocus();
        //    var skillsFromClient = new List<SkillSet>();
        //    var detailsFromClient = new List<SkillDetail>();
        //    var focusFromClient = new List<Focus>();

        //    var sent_At_DateTime_Now = DateTime.Now;
        //    var companyTemplate = new TemporaryCompanyTemplate();

        //    if (template.CheckedRolesNodes.Count != 0)
        //    {
        //        //Match the ids to the skills in the database and add the skill objects to a new list
        //        foreach (var skill in skillSets)
        //        {
        //            foreach (var node_id in template.CheckedRolesNodes)
        //            {
        //                if (node_id == skill.SkillId)
        //                {
        //                    skill.IsSelected = true;
        //                    skillsFromClient.Add(skill);
        //                }
        //            }
        //        }
        //        //Match the ids to the details in the database and add the detail objects to a new list
        //        foreach (var detail in skillDetails)
        //        {
        //            foreach (var node_id in template.CheckedRolesNodes)
        //            {
        //                if (node_id == detail.SkillDetailId)
        //                {
        //                    detail.IsSelected = true;
        //                    detailsFromClient.Add(detail);
        //                }
        //            }
        //        }
        //    }
        //    if (template.CheckedFocusNodes.Count != 0)
        //    {
        //        //Match the ids to the focus in the database and add the focus objects to a new list
        //        foreach (var focus in focuses)
        //        {
        //            foreach (var node_id in template.CheckedFocusNodes)
        //            {
        //                if (node_id == focus.FocusId)
        //                {
        //                    focus.IsSelected = true;
        //                    focusFromClient.Add(focus);
        //                }
        //            }
        //        }
        //    }

        //    int newId = 0;

        //    if (!ApiGetAllCompanies().Any())
        //    {
        //        newId = 1;
        //    }
        //    else
        //    {
        //        // Get the last antered Id from the db table and +1 ready to assign to new companyToEdit
        //        var lastId = ApiGetAllCompanies().OrderByDescending(i => i.CompanyId).First();
        //        newId = lastId.CompanyId;
        //        newId++;
        //    }

        //    //var foundCompany = _context.TemporaryCompanyTemplate.SingleOrDefault(c => c.CompanyGuid == template.UniqueUrl);


        //        companyTemplate.CompanyId = newId;
        //        companyTemplate.CompanyName = template.CompanyName;
        //        companyTemplate.ContactPerson = template.ContactPerson;
        //        companyTemplate.Email = template.Email;
        //        companyTemplate.RecruitmentWebAddress = template.RecruitmentWebAddress;
        //        companyTemplate.Phone = template.Phone;
        //        companyTemplate.Website = template.Website;
        //        companyTemplate.Bio = template.Bio;
        //        companyTemplate.CompanyGuid = template.UniqueUrl;
        //        companyTemplate.Locked = true;
        //        companyTemplate.OtherNotes = template.OtherNotes;
        //        companyTemplate.Sent_at = sent_At_DateTime_Now;
        //        companyTemplate.Received_at = DateTime.Now;


        //    //// Remove the company from the companySkills table via the id
        //    //var removeCompSkills = _context.TemporaryCompanySkills.Where(c => c.Company.CompanyGuid == template.CompanyGuid);

        //    //_context.TemporaryCompanySkills.RemoveRange(removeCompSkills);

        //    //// Remove the company from the companySkills table via the id
        //    //var removeCompDetails = _context.TemporaryCompanyDetails.Where(c => c.Company.CompanyGuid == template.CompanyGuid);

        //    //_context.TemporaryCompanyDetails.RemoveRange(removeCompDetails);

        //    //// Remove the company from the companyFocus table via the id
        //    //var removeCompFocus = _context.TemporaryCompanyFocus.Where(c => c.Company.CompanyGuid == template.CompanyGuid);

        //    //_context.TemporaryCompanyFocus.RemoveRange(removeCompFocus);

        //    //// Delete the company from the temp table before filling back in with the submitted form details
        //    //_context.TemporaryCompanyTemplate.Remove(foundCompany);

        //    //_context.SaveChanges();

        //    try
        //    {
        //        if (template.CheckedFocusNodes != null)
        //        {
        //            //var selectedFocuses = focusFromClient.Where(s => s.IsSelected).ToList();

        //            // Create the new rows for the new companies companyFocus
        //            foreach (var selectedFocus in focusFromClient)
        //            {
        //                var compFocus = new TemporaryCompanyFocus
        //                {
        //                    CompanyId = companyTemplate.CompanyId,
        //                    FocusId = selectedFocus.FocusId
        //                };
        //                companyTemplate.TemporaryCompanyFocuses.Add(compFocus);
        //                //_context.TemporaryCompanyFocus.Add(compFocus);
        //            }
        //        }
        //        if (template.CheckedRolesNodes != null)
        //        {
        //            //var selectedSkills = skillsFromClient.Where(s => s.IsSelected).ToList();

        //            // Create the new rows for the new companies companySkills
        //            foreach (var selectedSkill in skillsFromClient)
        //            {
        //                var compSkill = new TemporaryCompanySkills
        //                {
        //                    CompanyId = companyTemplate.CompanyId,
        //                    SkillId = selectedSkill.SkillId
        //                };
        //                companyTemplate.TemporaryCompanySkills.Add(compSkill);
        //                //_context.TemporaryCompanySkills.Add(compSkill);
        //            }

        //            //var selectedDetails = detailsFromClient.Where(d => d.IsSelected).ToList();

        //            // Create the new rows for the new companies companyDetails
        //            foreach (var selectedDetail in detailsFromClient)
        //            {
        //                var compDetail = new TemporaryCompanyDetails
        //                {
        //                    CompanyId = companyTemplate.CompanyId,
        //                    SkillDetailId = selectedDetail.SkillDetailId
        //                };
        //                companyTemplate.TemporaryCompanyDetails.Add(compDetail);
        //                //_context.TemporaryCompanyDetails.Add(compDetail);
        //            }
        //        }
        //        _context.SaveChanges();





        //        //if (template.SkillDetailsList == null && template.SkillSetsList == null)
        //        //{
        //        //    if (ModelState.IsValid)
        //        //    {
        //        //        await SaveCompanyDataToTemporaryTable(companyTemplate);

        //        //        //SendTemplateSummary(companyTemplate);
        //        //        var result = new ObjectResult(template);
        //        //        return result;
        //        //    }
        //        //}
        //        //else
        //        //{
        //            if (ModelState.IsValid)
        //            {
        //                await SaveCompanyDataToTemporaryTable(companyTemplate);

        //                //SendTemplateSummary(companyTemplate);
        //                var result = new ObjectResult(template);
        //                return result;
        //            }

        //        //}
        //    }
        //    catch (Exception e)
        //    {

        //        Console.WriteLine(e);
        //        return NotFound(e);
        //    }


        //    return NotFound();

        //}


        /// <summary>
        /// 
        /// </summary>
        /// <param name="template"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [Route("~/api/template/submittedtemplate")]
        public async Task<IActionResult> SubmittedCompanyTemplate([FromBody]CompanyTemplateBindingModel template)
        {

            var skillSets = ApiGetAllSkills();
            var skillDetails = ApiGetAllDetails();
            var focuses = ApiGetAllFocus();
            var skillsFromClient = new List<SkillSet>();
            var detailsFromClient = new List<SkillDetail>();
            var focusFromClient = new List<Focus>();

            if (template.CheckedRolesNodes.Count != 0)
            {
                //Match the ids to the skills in the database and add the skill objects to a new list
                foreach (var skill in skillSets)
                {
                    foreach (var node_id in template.CheckedRolesNodes)
                    {
                        if (node_id == skill.SkillId)
                        {
                            skill.IsSelected = true;
                            skillsFromClient.Add(skill);
                        }
                    }
                }
                //Match the ids to the details in the database and add the detail objects to a new list
                foreach (var detail in skillDetails)
                {
                    foreach (var node_id in template.CheckedRolesNodes)
                    {
                        if (node_id == detail.SkillDetailId)
                        {
                            detail.IsSelected = true;
                            detailsFromClient.Add(detail);
                        }
                    }
                }
            }
            if (template.CheckedFocusNodes.Count != 0)
            {
                //Match the ids to the focus in the database and add the focus objects to a new list
                foreach (var focus in focuses)
                {
                    foreach (var node_id in template.CheckedFocusNodes)
                    {
                        if (node_id == focus.FocusId)
                        {
                            focus.IsSelected = true;
                            focusFromClient.Add(focus);
                        }
                    }
                }
            }

            int newId = 0;

            if (!ApiGetAllCompanies().Any())
            {
                newId = 1;
            }
            else
            {
                // Get the last antered Id from the db table and +1 ready to assign to new companyToEdit
                var lastId = ApiGetAllCompanies().OrderByDescending(i => i.CompanyId).First();
                newId = lastId.CompanyId;
                newId++;
            }

            var foundCompany = _context.TemporaryCompanyTemplate.SingleOrDefault(c => c.CompanyGuid == template.UniqueUrl);



            var companyTemplate = new TemporaryCompanyTemplate
            {
                CompanyId = foundCompany.CompanyId,
                CompanyName = template.CompanyName,
                ContactPerson = template.ContactPerson,
                Email = template.Email,
                RecruitmentWebAddress = template.RecruitmentWebAddress,
                Phone = template.Phone,
                Website = template.Website,
                Bio = template.Bio,
                CompanyGuid = template.UniqueUrl,
                Locked = true,
                OtherNotes = template.OtherNotes,
                Sent_at = foundCompany.Sent_at,
                Received_at = DateTime.Now
            };

            // Remove the company from the companySkills table via the id
            var removeCompSkills = _context.TemporaryCompanySkills.Where(c => c.Company.CompanyGuid == foundCompany.CompanyGuid);

            _context.TemporaryCompanySkills.RemoveRange(removeCompSkills);

            // Remove the company from the companySkills table via the id
            var removeCompDetails = _context.TemporaryCompanyDetails.Where(c => c.Company.CompanyGuid == foundCompany.CompanyGuid);

            _context.TemporaryCompanyDetails.RemoveRange(removeCompDetails);

            // Remove the company from the companyFocus table via the id
            var removeCompFocus = _context.TemporaryCompanyFocus.Where(c => c.Company.CompanyGuid == foundCompany.CompanyGuid);

            _context.TemporaryCompanyFocus.RemoveRange(removeCompFocus);

            // Delete the company from the temp table before filling back in with the submitted form details
            _context.TemporaryCompanyTemplate.Remove(foundCompany);

            _context.SaveChanges();

            if (template.CheckedFocusNodes != null)
            {
                var selectedFocuses = focusFromClient.Where(s => s.IsSelected).ToList();

                // Create the new rows for the new companies companyFocus
                foreach (var selectedFocus in selectedFocuses)
                {
                    var compFocus = new TemporaryCompanyFocus
                    {
                        CompanyId = companyTemplate.CompanyId,
                        FocusId = selectedFocus.FocusId
                    };
                    companyTemplate.TemporaryCompanyFocuses.Add(compFocus);
                }
            }
            if (template.CheckedRolesNodes != null)
            {
                var selectedSkills = skillsFromClient.Where(s => s.IsSelected).ToList();

                // Create the new rows for the new companies companySkills
                foreach (var selectedSkill in selectedSkills)
                {
                    var compSkill = new TemporaryCompanySkills
                    {
                        CompanyId = companyTemplate.CompanyId,
                        SkillId = selectedSkill.SkillId
                    };
                    companyTemplate.TemporaryCompanySkills.Add(compSkill);
                }
            }
            if (template.CheckedRolesNodes != null)
            {
                var selectedDetails = detailsFromClient.Where(d => d.IsSelected).ToList();

                // Create the new rows for the new companies companyDetails
                foreach (var selectedDetail in selectedDetails)
                {
                    var compDetail = new TemporaryCompanyDetails
                    {
                        CompanyId = companyTemplate.CompanyId,
                        SkillDetailId = selectedDetail.SkillDetailId
                    };
                    companyTemplate.TemporaryCompanyDetails.Add(compDetail);
                }
            }
            if (template.SkillDetailsList == null && template.SkillSetsList == null)
            {
                if (ModelState.IsValid)
                {
                    await SaveCompanyDataToTemporaryTable(companyTemplate);

                    //SendTemplateSummary(companyTemplate);
                    var result = new ObjectResult(template);
                    return result;
                }
            }
            else
            {
                if (ModelState.IsValid)
                {
                    await SaveCompanyDataToTemporaryTable(companyTemplate);

                    //SendTemplateSummary(companyTemplate);
                    var result = new ObjectResult(template);
                    return result;
                }

            }
            return NotFound();

        }

        /// <summary>
        /// Saves the template as a draft in the temporary template table
        /// </summary>
        /// <param name="template"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [Route("~/api/template/drafttemplate")]
        public async Task<IActionResult> DraftTemplate([FromBody]CompanyTemplateBindingModel template)
        {
            //CompanyTemplateBindingModel template = value.ToObject<CompanyTemplateBindingModel>();

            int newId = FindCompanyIdFromTempCompanyTable();

            var foundCompany = _context.TemporaryCompanyTemplate.SingleOrDefault(c => c.CompanyGuid == template.UniqueUrl);

            var companyTemplate = new TemporaryCompanyTemplate
            {
                CompanyId = foundCompany.CompanyId,
                CompanyName = template.CompanyName,
                ContactPerson = template.ContactPerson,
                Email = template.Email,
                RecruitmentWebAddress = template.RecruitmentWebAddress,
                Phone = template.Phone,
                Website = template.Website,
                Bio = template.Bio,
                CompanyGuid = template.UniqueUrl,
                Locked = false,
                OtherNotes = template.OtherNotes,
                Sent_at = foundCompany.Sent_at
            };

            // Remove the company from the companySkills table via the id
            var removeCompSkills = _context.TemporaryCompanySkills.Where(c => c.Company.CompanyGuid == foundCompany.CompanyGuid);

            _context.TemporaryCompanySkills.RemoveRange(removeCompSkills);

            // Remove the company from the companySkills table via the id
            var removeCompDetails = _context.TemporaryCompanyDetails.Where(c => c.Company.CompanyGuid == foundCompany.CompanyGuid);

            _context.TemporaryCompanyDetails.RemoveRange(removeCompDetails);

            // Remove the company from the companyFocus table via the id
            var removeCompFocus = _context.TemporaryCompanyFocus.Where(c => c.Company.CompanyGuid == foundCompany.CompanyGuid);

            _context.TemporaryCompanyFocus.RemoveRange(removeCompFocus);

            // Delete the company from the temp table before filling back in with the submitted form details
            _context.TemporaryCompanyTemplate.Remove(foundCompany);

            _context.SaveChanges();

            if (template.FocusList != null)
            {
                var selectedFocuses = template.FocusList.Where(s => s.IsSelected).ToList();

                // Create the new rows for the new companies companyFocus
                foreach (var selectedFocus in selectedFocuses)
                {
                    var compFocus = new TemporaryCompanyFocus
                    {
                        CompanyId = companyTemplate.CompanyId,
                        FocusId = selectedFocus.FocusId
                    };
                    companyTemplate.TemporaryCompanyFocuses.Add(compFocus);
                }
            }
            if (template.SkillSetsList != null)
            {
                var selectedSkills = template.SkillSetsList.Where(s => s.IsSelected).ToList();

                // Create the new rows for the new companies companySkills
                foreach (var selectedSkill in selectedSkills)
                {
                    var compSkill = new TemporaryCompanySkills
                    {
                        CompanyId = companyTemplate.CompanyId,
                        SkillId = selectedSkill.SkillId
                    };
                    companyTemplate.TemporaryCompanySkills.Add(compSkill);
                }
            }
            if (template.SkillDetailsList != null)
            {
                var selectedDetails = template.SkillDetailsList.Where(d => d.IsSelected).ToList();

                // Create the new rows for the new companies companyDetails
                foreach (var selectedDetail in selectedDetails)
                {
                    var compDetail = new TemporaryCompanyDetails
                    {
                        CompanyId = companyTemplate.CompanyId,
                        SkillDetailId = selectedDetail.SkillDetailId
                    };
                    companyTemplate.TemporaryCompanyDetails.Add(compDetail);
                }
            }
            if (template.SkillDetailsList == null && template.SkillSetsList == null)
            {
                if (ModelState.IsValid)
                {
                    await SaveCompanyDataToTemporaryTable(companyTemplate);
                    var result = new ObjectResult(template);
                    return result;
                }
            }
            else
            {
                if (ModelState.IsValid)
                {
                    await SaveCompanyDataToTemporaryTable(companyTemplate);
                    var result = new ObjectResult(template);
                    return result;
                }

            }
            return NotFound();
        }

        /// <summary>
        /// Gets the companmy templates that are currently stored in the temporary company template table
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        [Route("~/api/template/gettemplatestatus")]
        public IActionResult GetTemplateStatus()
        {
            var template = new SendTemplateBindingModel();
            // Get the current companies stored in the temp comp table

            if (!GetAllCompaniesFromTempTable().Any())
            {
                var emailInfo = _context.EmailTemplates.ToList();

                foreach (var item in emailInfo)
                {
                    template.EmailSubject = item.EmailSubject;
                    template.EmailContent = item.EmailContent;
                }
                var result = new ObjectResult(template);
                return result;
            }
            else
            {
                foreach (var comp in GetAllCompaniesFromTempTable())
                {
                    template.CompanyList.Add(comp);
                }

                var emailInfo = _context.EmailTemplates.ToList();

                foreach (var item in emailInfo)
                {
                    template.EmailSubject = item.EmailSubject;
                    template.EmailContent = item.EmailContent;
                }

                var result = new ObjectResult(template);
                return result;
            }

        }
        /// <summary>
        /// POST saves the company to the temp company table once email is sent with link to template
        /// </summary>
        /// <param name="sendTemplate"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [Route("~/api/template/savetemplatestatus")]
        public async Task<IActionResult> SaveTemplateStatus([FromBody]SendTemplateBindingModel sendTemplate)
        {
            sendTemplate.CompanyList.Clear();

            if (ModelState.IsValid)
            {
                var email = new EmailAddress();
                var emailToSend = new EmailMessage();

                email.Address = sendTemplate.Email;

                var fromEmail = new EmailAddress
                {
                    Name = SenderName,
                    Address = SenderAddress
                };

                emailToSend.ToAddresses.Add(email);
                emailToSend.FromAddresses.Add(fromEmail);
                emailToSend.Content = sendTemplate.EmailContent + sendTemplate.Link;
                emailToSend.Subject = sendTemplate.EmailSubject;

                int emailId;

                if (!ApiGetAllCompanies().Any())
                {
                    emailId = 1;
                }
                else
                {
                    // Get the last antered Id from the db table and +1 ready to assign to new companyToEdit
                    var lastId = ApiGetEmailTemplateData().OrderByDescending(i => i.Id).FirstOrDefault();
                    emailId = lastId.Id;
                    emailId++;
                }

                try
                {
                    _context.EmailTemplates.Clear();
                    _context.SaveChanges();

                    var emailTemplate = new EmailTemplate
                    {
                        Id = emailId,
                        EmailSubject = sendTemplate.EmailSubject,
                        EmailContent = sendTemplate.EmailContent
                    };

                    _context.Add(emailTemplate);
                    _context.SaveChanges();

                    var emailResult = await _emailService.Send(emailToSend);

                    if (emailResult)
                    {
                        int newId = 0;

                        if (!GetAllCompaniesFromTempTable().Any())
                        {
                            newId = 1;
                        }
                        else
                        {
                            // Get the last entered Id from the db table and +1 ready to assign to new companyToEdit
                            var lastId = GetAllCompaniesFromTempTable().OrderByDescending(i => i.CompanyId).First();
                            newId = lastId.CompanyId;
                            newId++;
                        }
                        // Create the temp company data
                        var companyTemplateDetails = new TemporaryCompanyTemplate
                        {
                            CompanyId = newId,
                            CompanyName = sendTemplate.CompanyName,
                            Email = sendTemplate.Email,
                            Sent_at = DateTime.Now,
                            CompanyGuid = sendTemplate.CompanyGuid,
                            RecruitmentWebAddress = sendTemplate.RecruitmentWebAddress
                        };

                        // Save the company to the temp table in the db.
                        await SaveCompanyDataToTemporaryTable(companyTemplateDetails);
                        // Add the companies from the temp table to the list to show in the view
                        foreach (var comp in GetAllCompaniesFromTempTable())
                        {
                            sendTemplate.CompanyList.Add(comp);
                        }

                        var result = new ObjectResult(sendTemplate);
                        return result;
                    }
                }
                catch (Exception e)
                {
                    return NotFound(e);
                }

               
            }
            return NotFound();
        }

        /// <summary>
        /// A Summary of the companies submitted information, to be sent as an email after submition of form.
        /// </summary>
        /// <param name="template"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [Route("~/api/template/sendtemplatesummary")]
        public async void SendTemplateSummary([FromBody]CompanyTemplateBindingModel template)
        {
            var email = new EmailAddress();
            var emailToSend = new EmailMessage();

            email.Address = template.Email;

            var fromEmail = new EmailAddress
            {
                Name = SenderName,
                Address = SenderAddress
            };

            emailToSend.ToAddresses.Add(email);
            emailToSend.FromAddresses.Add(fromEmail);

            emailToSend.Subject = "A summary of your companies template";

            var comp = new CompanyTemplateBindingModel()
            {
                CompanyId = template.CompanyId,
                CompanyName = template.CompanyName,
                ContactPerson = template.ContactPerson,
                Email = template.Email,
                Phone = template.Phone,
                RecruitmentWebAddress = template.RecruitmentWebAddress,
                UniqueUrl = template.UniqueUrl,
                Website = template.Website,
                Bio = template.Bio,
                OtherNotes = template.OtherNotes,

            };
           

            var builder = new BodyBuilder
            {

                HtmlBody = string.Format(@"<section>" +
                "  <div class=" + "standard - container" + ">" +
                " <h3 class=" + "text-center" + ">Thank you for submitting your companies information.</h3>" +
                " <h4 class=" + "text-center" + ">Here is a summary of your submitted info:</h4>" +

                "<ul>" +
                "<li>" + "Company name: " + comp.CompanyName + "</li>" +
                "<li>" + "Contact person: " + comp.ContactPerson + "</li>" +
                "<li>" + "Recruitment contact email: " + comp.Email + "</li>" +
                "<li>" + "Phone: " + comp.Phone + "</li>" +
                "<li>" + "Recuitment website address: " + comp.RecruitmentWebAddress + "</li>" +
                "<li>" + "Company website: " + comp.Website + "</li>" +
                "<li>" + "Other notes: " + comp.OtherNotes + "</li>" +
                "</ul>" +

                    " <h5 class=" + "text-center" + ">If you would like to make any changes please contact admin.</h5>" +
                "</section>"),
            };

            emailToSend.Content = builder.HtmlBody;

            await _emailService.Send(emailToSend);
        }

        private async Task SaveCompanyDataToTemporaryTable(TemporaryCompanyTemplate temporaryCompany)
        {
            try
            {
                _context.Add(temporaryCompany);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private int FindCompanyIdFromTempCompanyTable()
        {
            int newId = 0;

            if (!GetAllCompaniesFromTempTable().Any())
            {
                newId = 1;
            }
            else
            {
                // Get the last antered Id from the db table and +1 ready to assign to new companyToEdit
                var lastId = GetAllCompaniesFromTempTable().OrderByDescending(i => i.CompanyId).First();
                newId = lastId.CompanyId;
                newId++;
            }

            return newId;
        }

        private async Task CreateCompany(Companies compToCreate)
        {
            try
            {
                _context.Add(compToCreate);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }
    }
}
