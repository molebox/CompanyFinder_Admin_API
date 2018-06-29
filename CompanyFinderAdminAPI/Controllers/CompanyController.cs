using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CompanyDatabase.Models;
using CompanyFinderAdminAPI.BindingModels;
using CompanyFinderAdminAPI.Extension_Methods;
using CompanyFinderEmailTemplateLib.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CompanyFinderAdminAPI.Controllers
{
    /// <summary>
    /// Company controller for CRUD operations against the db. 
    /// The client will sends requests to this controller and the controller will in turn talk to the db and get the requested data
    /// </summary>
    [Produces("application/json")]
    [Route("api/Company")]
    //[Authorize]
    public class CompanyController : Controller
    {
        private CompanyDbContext _context;
        private UserManager<IdentityUser> _userManger;
        private SignInManager<IdentityUser> _signInManager;

        /// <summary>
        /// Constructor to initialize the db context, email service, identity
        /// </summary>
        /// <param name="context"></param>
        /// <param name="userMgr"></param>
        /// <param name="signInMgr"></param>
        public CompanyController(CompanyDbContext context, UserManager<IdentityUser> userMgr, SignInManager<IdentityUser> signInMgr)
        {
            _context = context;
            _userManger = userMgr;
            _signInManager = signInMgr;
        }

        /// <summary>
        /// Gets the tree nodes from the database
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("~/api/company/gettreenodes")]
        public IActionResult GetTreeNodes()
        {
            var treeNodes = _context.TreeNodes.ToList();

            var result = new ObjectResult(treeNodes);
            return result;
        }

        private IEnumerable<TreeNodes> ApiGetAllTreeNodes() => _context.TreeNodes.ToList();

        private IEnumerable<FocusNodes> ApiGetAllFocusNodes() => _context.FocusNodes.ToList();

        private List<TreeNodes> SaveAllNodes(List<TreeNodes> nodes, int parentId)
        {
            return nodes.Where(l => l.ParentId == parentId).OrderBy(l => l.OrderNumber)
                 .Select(l => new TreeNodes
                 {
                     Name = l.Name,
                     Id = l.Id,
                     ParentId = l.ParentId,
                     OrderNumber = l.OrderNumber,
                     Children = SaveAllNodes(nodes, l.Id)
                 }).ToList();
        }
        private List<TreeNodesWithState> SaveAllNodesWithState(List<TreeNodesWithState> nodes, int parentId)
        {
            return nodes.Where(l => l.ParentId == parentId).OrderBy(l => l.OrderNumber)
                 .Select(l => new TreeNodesWithState
                 {
                     Id = l.Id,
                     Text = l.Text,
                     ParentId = l.ParentId,
                     OrderNumber = l.OrderNumber,
                     State = l.State,
                     Children = SaveAllNodesWithState(nodes, l.Id)
                 }).ToList();
        }
        private List<FocusNodesWithState> SaveAllFocusNodesWithState(List<FocusNodesWithState> nodes, int parentId)
        {
            return nodes.Where(l => l.ParentId == parentId).OrderBy(l => l.OrderNumber)
                 .Select(l => new FocusNodesWithState
                 {
                     Id = l.Id,
                     Text = l.Text,
                     ParentId = l.ParentId,
                     OrderNumber = l.OrderNumber,
                     State = l.State,
                     Children = SaveAllFocusNodesWithState(nodes, l.Id)
                 }).ToList();
        }
        private List<FocusNodes> SaveAllFocusNodes(List<FocusNodes> nodes, int parentId)
        {
            return nodes.Where(l => l.ParentId == parentId).OrderBy(l => l.OrderNumber)
                 .Select(l => new FocusNodes
                 {
                     Name = l.Name,
                     Id = l.Id,
                     ParentId = l.ParentId,
                     OrderNumber = l.OrderNumber,
                     Children = SaveAllFocusNodes(nodes, l.Id)
                 }).ToList();
        }

        /// <summary>
        /// Saves the tree view to the database
        /// </summary>
        /// <param name="saveTree"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("~/api/company/savetree")]
        public IActionResult SaveTree([FromBody]SaveTreeBindingModel saveTree)
        {
            var backupData = _context.TreeNodes.ToList();
            var compSkillBackup = _context.CompanySkills.ToList();
            var compDetailsBackup = _context.CompanyDetails.ToList();
            var compFocusBackup = _context.CompanyFocus.ToList();

            if (saveTree.TreeNodesList.Count == 0 || saveTree.TreeNodesList == null)
            {
                return NotFound();
            }
            else if (saveTree.FocusNodesList.Count == 0 || saveTree.FocusNodesList == null)
            {
                return NotFound();
            }
            else
            {
                try
                {
                    //Clear the tables and save
                    _context.TreeNodes.Clear();
                    _context.FocusNodes.Clear();
                    _context.Focus.Clear();
                    _context.SkillSet.Clear();
                    _context.SkillDetail.Clear();

                    _context.SaveChanges();
                }
                catch (DbUpdateException ex)
                {
                    Console.WriteLine(ex.Message);
                    return NotFound();
                }

                try
                {
                    //Get the skills and details by their order number. Get the root node by its id and get all the children
                    var newSkills = saveTree.TreeNodesList.Where(s => s.OrderNumber == 2).ToList();
                    var newDetails = saveTree.TreeNodesList.Where(d => d.OrderNumber == 3).ToList();
                    var root = saveTree.TreeNodesList.Where(r => r.Id == 1).ToList();
                    var rootChildren = saveTree.TreeNodesList.Where(r => r.Children != null).ToList();
                    //Get the focuses by their order number. Get the root node by its id and get all the children
                    var newFocuses = saveTree.FocusNodesList.Where(f => f.OrderNumber == 2).ToList();
                    var focusRoot = saveTree.FocusNodesList.Where(f => f.Id == 1).ToList();
                    var focusChildren = saveTree.FocusNodesList.Where(f => f.Children != null).ToList();

                    if (newSkills.Count > 0)
                    {
                        foreach (var item in newSkills)
                        {
                            var newSkill = new SkillSet //Create the skillset using the node information
                            {
                                SkillId = item.Id,
                                SkillName = item.Name
                            };
                            _context.SkillSet.Add(newSkill);
                        }
                    }

                    if (newDetails.Count > 0)
                    {
                        foreach (var item in newDetails)
                        {
                            var newDetail = new SkillDetail //Create the skill detail using the node information
                            {
                                SkillDetailId = item.Id,
                                DetailName = item.Name
                            };
                            _context.SkillDetail.Add(newDetail);
                        }
                    }
                    //Get all the children
                    var nodeLists = root.Where(l => l.ParentId == null)
                        .Select(l => new TreeNodes
                        {
                            Name = l.Name,
                            Id = l.Id,
                            ParentId = l.ParentId,
                            OrderNumber = l.OrderNumber,
                            Children = SaveAllNodes(rootChildren, l.Id)
                        }).ToList();

                    if (nodeLists != null)
                    {
                        foreach (var item in nodeLists)
                        {
                            _context.TreeNodes.Add(item);
                        }
                    }

                    if (newFocuses.Count > 0)
                    {
                        foreach (var item in newFocuses)
                        {
                            var newFocus = new Focus //Create the focus using the node information
                            {
                                FocusId = item.Id,
                                FocusType = item.Name
                            };
                            _context.Focus.Add(newFocus);
                        }
                    }
                    //Get all the children
                    var nodeFocusLists = focusRoot.Where(l => l.ParentId == null)
                        .Select(l => new FocusNodes
                        {
                            Name = l.Name,
                            Id = l.Id,
                            ParentId = l.ParentId,
                            OrderNumber = l.OrderNumber,
                            Children = SaveAllFocusNodes(focusChildren, l.Id)
                        }).ToList();

                    if (nodeFocusLists != null)
                    {
                        foreach (var item in nodeFocusLists)
                        {
                            _context.FocusNodes.Add(item);
                        }
                    }
                    _context.SaveChanges();

                    //Reloads the junction tables - runs a check to see that it doesnt load items that have been deleted from another table
                    var currentSkillSets = _context.SkillSet.ToList();
                    var currentSkillDetails = _context.SkillDetail.ToList();
                    var currentFocus = _context.Focus.ToList();

                    foreach (var item in compSkillBackup)
                    {
                        foreach (var skill in currentSkillSets)
                        {
                            if (item.SkillId == skill.SkillId)
                            {
                                var compSkill = new CompanySkills
                                {
                                    CompanyId = item.CompanyId,
                                    SkillId = item.SkillId,
                                };
                                _context.CompanySkills.Add(compSkill);
                            }
                        }
                    }

                    foreach (var item in compDetailsBackup)
                    {
                        foreach (var detail in currentSkillDetails)
                        {
                            if (item.SkillDetailId == detail.SkillDetailId)
                            {
                                var compDetail = new CompanyDetails
                                {
                                    CompanyId = item.CompanyId,
                                    SkillDetailId = item.SkillDetailId,
                                };
                                _context.CompanyDetails.Add(compDetail);
                            }
                        }
                    }

                    foreach (var item in compFocusBackup)
                    {
                        foreach (var focus in currentFocus)
                        {
                            if (item.FocusId == focus.FocusId)
                            {
                                var compFocus = new CompanyFocus
                                {
                                    CompanyId = item.CompanyId,
                                    FocusId = item.FocusId,
                                };
                                _context.CompanyFocus.Add(compFocus);
                            }
                        }
                    }

                    //If all went well then return a 200 OK and save everything
                    _context.SaveChanges();
                    var result = new ObjectResult(saveTree);
                    return result;

                }
                catch (DbUpdateException ex)
                {
                    //If something went wrong then print the error to the console, load the backup data back into the databse and return a 404
                    Console.WriteLine(ex.Message);
                    _context.SaveChanges();
                    return NotFound();
                }
            }
        }

        /// <summary>
        /// Gets the tree nodes from the database
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("~/api/company/getfocusnodes")]
        public IActionResult GetFocusNodes()
        {
            var focusNodes = _context.FocusNodes.ToList();

            var result = new ObjectResult(focusNodes);
            return result;
        }

        /// <summary>
        /// Gets all the companies from the db -- api/GetAllCompanies
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("~/api/company/getallcompanies")]
        public IActionResult GetAllCompanies()
        {
            var allCompanies = _context.Companies.ToList();

            if (!allCompanies.Any())
            {
                return NotFound();
            }
            else
            {
                var result = new ObjectResult(allCompanies);
                return result;
            }
        }

        private IEnumerable<Companies> ApiGetAllCompanies() => _context.Companies.ToList();

        /// <summary>
        /// Get all the focuses from the db -- api/GetAllFocuses
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("~/api/company/getallfocus")]
        public IActionResult GetAllFocuses()
        {
            var allFocuses = _context.Focus.ToList();

            if (!allFocuses.Any())
            {
                return NotFound();
            }
            else
            {
                var result = new ObjectResult(allFocuses);
                return result;
            }
        }
        private IEnumerable<Focus> ApiGetAllFocus() => _context.Focus.ToList();

        /// <summary>
        /// Get all the skill details from db -- api/GetAllSkillDetails
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("~/api/company/getalldetails")]
        public IActionResult GetAllSkillDetails()
        {
            var allDetails = _context.SkillDetail.ToList();

            if (!allDetails.Any())
            {
                return NotFound();
            }
            else
            {
                var result = new ObjectResult(allDetails);
                return result;
            }
        }
        private IEnumerable<SkillDetail> ApiGetAllDetails() => _context.SkillDetail.ToList();

        /// <summary>
        /// Get all the skills from the db -- api/GetAllSkills
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("~/api/company/getallskills")]
        public IActionResult GetAllSkills()
        {
            var allSkills = _context.SkillSet.ToList();

            if (!allSkills.Any())
            {
                return NotFound();
            }
            else
            {
                var result = new ObjectResult(allSkills);
                return result;
            }
        }
        /// <summary>
        /// Get all the skills from the db -- api/GetAllSkills
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("~/api/company/getskillbyid/{id}")]
        public IActionResult GetSkillById(int id)
        {
            var foundSkill = _context.SkillSet.Where(s => s.SkillId == id);
            var tempSkill = new SkillSet();

            foreach (var item in foundSkill)
            {
                tempSkill = item;
            }

            if (foundSkill == null)
            {
                return NotFound();
            }
            else
            {
                var result = new ObjectResult(tempSkill);
                return result;
            }
        }
        private IEnumerable<SkillSet> ApiGetAllSkills() => _context.SkillSet.ToList();

        /// <summary>
        /// Get the company via its id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("~/api/company/getcompanybyid/{id}")]
        public IActionResult GetCompById(int id)
        {
            var foundComp = ApiGetAllCompanies().FirstOrDefault(comp => comp.CompanyId == id);
            if (foundComp == null)
            {
                return NotFound();
            }
            else
            {
                var result = new ObjectResult(foundComp);
                return result;
            }

        }

        /// <summary>
        /// Creates a node in the database. If successful then returns true, if fail then false -- api/AddNewNode
        /// 
        /// Using JObject ensures that however the data is posted, 
        /// we are able to serialize it to the related class and can be assigned a specific data type with the ToObject method which requires the datatype to serialize to.
        /// 
        /// </summary>
        /// <param name="treeNodes"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("~/api/company/addnewnode")]
        public async Task<IActionResult> AddNewNode([FromBody]CreateTreeNodesBindingModel treeNodes)
        {
            //AddSkillsBindingModel skill = value.ToObject<AddSkillsBindingModel>();
            var newDetail = new SkillDetail();
            var newNode = new TreeNodes();
            var newSkill = new SkillSet();
            var newFocusNode = new FocusNodes();


            if (treeNodes.TreeNodes.OrderNumber == 1)
            {
                int newId = 0;
                int newNodeId = 1;

                var getAllSkills = ApiGetAllSkills();

                if (getAllSkills.Count() <= 0)
                {
                    newId = 1;
                }
                else
                {
                    // Get the last antered Id from the db table and +1 ready to assign to new skill
                    var lastId = ApiGetAllSkills().OrderByDescending(i => i.SkillId).First();
                    newId = lastId.SkillId;
                    newId++;
                }
                var lastNodeId = ApiGetAllTreeNodes().OrderByDescending(i => i.Id).First();
                newNodeId = lastNodeId.Id;
                newNodeId++;

                newSkill.SkillId = newId;
                newSkill.SkillName = treeNodes.TreeNodes.Name;


                newNode.Id = newNodeId;
                newNode.Name = treeNodes.TreeNodes.Name;
                newNode.OrderNumber = treeNodes.TreeNodes.OrderNumber;
                newNode.ParentId = treeNodes.TreeNodes.ParentId;

                var treeNodeList = ApiGetAllTreeNodes().ToList();
                treeNodes.TreeNodesList = treeNodeList;

                if (ModelState.IsValid)
                {
                    _context.Add(newNode);
                    _context.Add(newSkill);

                    await _context.SaveChangesAsync();
                    return Ok();

                }
            }
            else if (treeNodes.TreeNodes.OrderNumber == 2)
            {
                int newId = 0;
                int newNodeId = 1;

                var getAllDetails = ApiGetAllDetails();

                if (getAllDetails.Count() <= 0)
                {
                    newId = 1;
                }
                else
                {
                    // Get the last antered Id from the db table and +1 ready to assign to new skill
                    var lastId = ApiGetAllDetails().OrderByDescending(i => i.SkillDetailId).First();
                    newId = lastId.SkillDetailId;
                    newId++;
                }
                var lastNodeId = ApiGetAllTreeNodes().OrderByDescending(i => i.Id).First();
                newNodeId = lastNodeId.Id;
                newNodeId++;

                newDetail.SkillDetailId = newId;
                newDetail.DetailName = treeNodes.TreeNodes.Name;

                newNode.Id = newNodeId;
                newNode.Name = treeNodes.TreeNodes.Name;
                newNode.OrderNumber = treeNodes.TreeNodes.OrderNumber;
                newNode.ParentId = treeNodes.TreeNodes.ParentId;

                var treeNodeList = ApiGetAllTreeNodes().ToList();
                treeNodes.TreeNodesList = treeNodeList;

                if (ModelState.IsValid)
                {
                    _context.Add(newNode);
                    _context.Add(newDetail);
                    //_context.Add(newFocusNode);


                    await _context.SaveChangesAsync();


                    return Ok();
                }
            }



            return NotFound();
        }


        /// <summary>
        /// Post method that checks the login for access to the search page. If login is good then the user is redirected to the search oage, if the login
        /// is bad an error message is displayed
        /// </summary>
        /// <param name="admin"></param>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("~/api/company/adminlogin")]
        [AllowAnonymous]
        public async Task<IActionResult> AdminLogin(AdminLoginBindingModel admin, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                IdentityUser administrator = await _userManger.FindByNameAsync(admin.AdminName);

                if (administrator != null)
                {
                    await _signInManager.SignOutAsync();
                    if ((await _signInManager.PasswordSignInAsync(administrator, admin.AdminPassword, false, false)).Succeeded)
                    {
                        return Ok(admin);
                    }

                }
            }
            return Unauthorized();
        }

        /// <summary>
        /// Get 1 company from the db based on the id recieved -- api/GetCompany
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("~/api/company/getcompany/{id}")]
        public IActionResult GetCompany(int id)
        {
            var company = ApiGetAllCompanies().FirstOrDefault(comp => comp.CompanyId == id);

            if (company != null)
            {
                company.CompanySkills = _context.CompanySkills
               .Where(c => c.CompanyId == id)
               .ToList();

                company.CompanyDetails = _context.CompanyDetails
                    .Where(c => c.CompanyId == id)
                    .ToList();

                company.CompanyFocuses = _context.CompanyFocus
                    .Where(f => f.CompanyId == id)
                    .ToList();

                var result = new ObjectResult(company);
                return result;

            }
            return NotFound();
        }

        /// <summary>
        /// Get 1 focus from the db based on the id recieved -- api/GetFocus
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("~/api/company/getfocus/{id}")]
        public Focus GetFocus(int id) => ApiGetAllFocus().FirstOrDefault(f => f.FocusId == id);


        /// <summary>
        /// Get 1 skill from the db based on the id recieved -- api/GetSkill
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("~/api/company/getskill/{id}")]
        public SkillSet GetSkill(int id) => ApiGetAllSkills().FirstOrDefault(skill => skill.SkillId == id);

        /// <summary>
        /// Get 1 detail from the db based on the id recieved -- api/GetDetail
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("~/api/company/getdetail/{id}")]
        public SkillDetail GetDetail(int id) => ApiGetAllDetails().FirstOrDefault(detail => detail.SkillDetailId == id);


        private HomePage ApiGetHomePageInfo() => _context.HomePage.FirstOrDefault();

        /// <summary>
        /// Get tree nodes for the edit company 
        /// </summary>
        /// <returns></returns>
        [HttpGet("{id}")]
        [Route("~/api/company/editcompanytreenodes/{id}")]
        public async Task<IActionResult> GetEditCompanyTreeNodes(int id)
        {
            var company = await _context.Companies.SingleOrDefaultAsync(m => m.CompanyId == id);

            var compToEditSkills = _context.CompanySkills
               .Where(c => c.CompanyId == id)
               .ToList();

            var compToEditDetails = _context.CompanyDetails
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
            var nodeLists = tempTreeNodeList.Where(l => l.ParentId == null)
                .Select(l => new TreeNodesWithState
                {
                    Id = l.Id,
                    Text = l.Text,
                    ParentId = l.ParentId,
                    OrderNumber = l.OrderNumber,
                    State = l.State,
                    Children = SaveAllNodesWithState(tempTreeNodeList, l.Id)
                }).ToList();

            // Return the node list model as json 
            var result = new ObjectResult(nodeLists);
            return result;
        }
        /// <summary>
        /// Get tree nodes for the template
        /// </summary>
        /// <returns></returns>
        [HttpGet("{id}")]
        [Route("~/api/company/edittemplatetreenodes/{id}")]
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
            var nodeLists = tempTreeNodeList.Where(l => l.ParentId == null)
                .Select(l => new TreeNodesWithState
                {
                    Id = l.Id,
                    Text = l.Text,
                    ParentId = l.ParentId,
                    OrderNumber = l.OrderNumber,
                    State = l.State,
                    Children = SaveAllNodesWithState(tempTreeNodeList, l.Id)
                }).ToList();

            // Return the node list model as json 
            var result = new ObjectResult(nodeLists);
            return result;
        }

        /// <summary>
        /// Get tree nodes for the edit company 
        /// </summary>
        /// <returns></returns>
        [HttpGet("{id}")]
        [Route("~/api/company/editcompanyfocusnodes/{id}")]
        public async Task<IActionResult> GetEditCompanyFocusNodes(int id)
        {
            var company = await _context.Companies.SingleOrDefaultAsync(m => m.CompanyId == id);

            var compToEditFocuses = _context.CompanyFocus
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

            //Create the tree structure from the skill and detail nodes
            var nodeLists = tempFocusNodeList.Where(l => l.ParentId == null)
                .Select(l => new FocusNodesWithState
                {
                    Id = l.Id,
                    Text = l.Text,
                    ParentId = l.ParentId,
                    OrderNumber = l.OrderNumber,
                    State = l.State,
                    Children = SaveAllFocusNodesWithState(tempFocusNodeList, l.Id)
                }).ToList();

            // Return the node list model as json 
            var result = new ObjectResult(nodeLists);
            return result;

        }
        /// <summary>
        /// Get focus tree nodes for the template
        /// </summary>
        /// <returns></returns>
        [HttpGet("{id}")]
        [Route("~/api/company/edittemplatefocusnodes/{id}")]
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

            //Create the tree structure from the skill and detail nodes
            var nodeLists = tempFocusNodeList.Where(l => l.ParentId == null)
                .Select(l => new FocusNodesWithState
                {
                    Id = l.Id,
                    Text = l.Text,
                    ParentId = l.ParentId,
                    OrderNumber = l.OrderNumber,
                    State = l.State,
                    Children = SaveAllFocusNodesWithState(tempFocusNodeList, l.Id)
                }).ToList();

            // Return the node list model as json 
            var result = new ObjectResult(nodeLists);
            return result;

        }

        /// <summary>
        /// Get the company data ready for editing -- api/EditCompany
        /// </summary>
        /// <returns></returns>
        [HttpGet("{id}")]
        [Route("~/api/company/editcompany/{id}")]
        public async Task<IActionResult> EditCompany(int id)
        {
            //Get the company via its id
            var company = await _context.Companies.SingleOrDefaultAsync(m => m.CompanyId == id);

            var companySkills = _context.CompanySkills.Where(c => c.CompanyId == id).ToList();
            var companyDetails = _context.CompanyDetails.Where(c => c.CompanyId == id).ToList();
            var companyFocus = _context.CompanyFocus.Where(c => c.CompanyId == id).ToList();



            //Create the binding model from the data
            var companyToEdit = new CompanyBindingModel
            {
                CompanyId = id,
                CompanyName = company.CompanyName,
                ContactPerson = company.ContactPerson,
                Email = company.Email,
                Phone = company.Phone,
                Website = company.Website,
                RecruitmentWebAddress = company.RecruitmentWebAddress,
                Bio = company.Bio,
            };

            // Return the company binding model as json 
            var result = new ObjectResult(companyToEdit);
            return result;
        }

        /// <summary>
        /// Put to edit the company in the db. Returns true if update to db is successful otherwise returns false -- api/EditCompany
        /// 
        /// Using JObject ensures that however the data is posted, 
        /// we are able to serialize it to the related class and can be assigned a specific data type with the ToObject method which requires the datatype to serialize to.
        /// 
        /// </summary>
        /// <param name="company"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("~/api/company/editcompany")]
        public async Task<IActionResult> EditCompany([FromBody]CompanyBindingModel company)
        {
            var skillSets = ApiGetAllSkills();
            var skillDetails = ApiGetAllDetails();
            var focuses = ApiGetAllFocus();

            // Remove the company from the companySkills table via the id
            var removeCompSkills = _context.CompanySkills.Where(c => c.CompanyId == company.CompanyId);
            _context.CompanySkills.RemoveRange(removeCompSkills);

            // Remove the company from the companySkills table via the id
            var removeCompDetails = _context.CompanyDetails.Where(c => c.CompanyId == company.CompanyId);
            _context.CompanyDetails.RemoveRange(removeCompDetails);

            // Remove the company from the companyFocus table via the id
            var removeCompFocus = _context.CompanyFocus.Where(c => c.CompanyId == company.CompanyId);
            _context.CompanyFocus.RemoveRange(removeCompFocus);

            // Find the company in the companies table via the id
            var foundCompany = await _context.Companies.SingleOrDefaultAsync(m => m.CompanyId == company.CompanyId);
            // Assign the company its new properties taken from the viewmodel
            foundCompany.CompanyId = company.CompanyId;
            foundCompany.CompanyName = company.CompanyName;
            foundCompany.ContactPerson = company.ContactPerson;
            foundCompany.Email = company.Email;
            foundCompany.Phone = company.Phone;
            foundCompany.Bio = company.Bio;
            foundCompany.Website = company.Website;
            foundCompany.CompanySkills = new List<CompanySkills>();
            foundCompany.CompanyDetails = new List<CompanyDetails>();
            foundCompany.CompanyFocuses = new List<CompanyFocus>();

            try
            {

                if (company.CheckedFocusNodes.Count != 0)
                {
                    //Match the ids to the focuses in the database and add the focus objects to a new list
                    foreach (var focus in focuses)
                    {
                        foreach (var node_id in company.CheckedFocusNodes)
                        {
                            if (node_id == focus.FocusId)
                            {
                                var compFocus = new CompanyFocus
                                {
                                    CompanyId = foundCompany.CompanyId,
                                    FocusId = focus.FocusId
                                };
                                foundCompany.CompanyFocuses.Add(compFocus);
                            }
                        }
                    }
                }

                //Check the list, if its NOT null then take the values and asign them to the company focus table
                if (company.CheckedRolesNodes != null)
                {
                    //Match the ids to the skills in the database and add the skill objects to a new list
                    foreach (var skill in skillSets)
                    {
                        foreach (var node_id in company.CheckedRolesNodes)
                        {
                            if (node_id == skill.SkillId)
                            {
                                var compSkill = new CompanySkills
                                {
                                    CompanyId = foundCompany.CompanyId,
                                    SkillId = skill.SkillId
                                };
                                foundCompany.CompanySkills.Add(compSkill);
                            }
                        }
                    }

                    //Match the ids to the details in the database and add the detail objects to a new list
                    foreach (var detail in skillDetails)
                    {
                        foreach (var node_id in company.CheckedRolesNodes)
                        {
                            if (node_id == detail.SkillDetailId)
                            {
                                var compDetail = new CompanyDetails
                                {
                                    CompanyId = foundCompany.CompanyId,
                                    SkillDetailId = detail.SkillDetailId
                                };
                                foundCompany.CompanyDetails.Add(compDetail);
                            }
                        }
                    }
                }


                if (company.CompanyId != company.CompanyId)
                {
                    return NotFound();
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        _context.Update(foundCompany);
                        await _context.SaveChangesAsync();
                        return Ok();

                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!CompanyExists(company.CompanyId))
                        {
                            return NotFound();
                        }
                    }

                }
                return NotFound();

            }
            catch (Exception e)
            {
                Console.Write(e.Message);
                return NotFound();
            }
        }

        /// <summary>
        /// Get the database information for the skills, details and focuses to be used when creating a new company -- api/company/getnewcompanyinfo
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("~/api/company/getnewcompanyinfo")]
        public IActionResult GetInfoForCreatingNewCompany()
        {
            var companyBindingModel = new CompanyBindingModel
            {
                FocusList = ApiGetAllFocus().ToList(),
                SkillSetsList = ApiGetAllSkills().ToList(),
                SkillDetailsList = ApiGetAllDetails().ToList()
            };

            var result = new ObjectResult(companyBindingModel);
            return result;
        }

        /// <summary>
        /// Creates a company in the database. If successful then returns true, if fail then false -- api/CreateCompany
        /// 
        /// Using JObject ensures that however the data is posted, 
        /// we are able to serialize it to the related class and can be assigned a specific data type with the ToObject method which requires the datatype to serialize to.
        /// 
        /// </summary>
        /// <param name="company"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("~/api/company/createcompany")]
        public async Task<IActionResult> CreateCompany([FromBody]CompanyBindingModel company)
        {
            //CompanyBindingModel company = value.ToObject<CompanyBindingModel>();

            var skillSets = ApiGetAllSkills();
            var skillDetails = ApiGetAllDetails();
            var focuses = ApiGetAllFocus();
            var skillsFromClient = new List<SkillSet>();
            var detailsFromClient = new List<SkillDetail>();
            var focusFromClient = new List<Focus>();

            if (company.CheckedRolesNodes.Count != 0)
            {
                //Match the ids to the skills in the database and add the skill objects to a new list
                foreach (var skill in skillSets)
                {
                    foreach (var node_id in company.CheckedRolesNodes)
                    {
                        if (node_id == skill.SkillId)
                        {
                            skillsFromClient.Add(skill);
                        }
                    }
                }
                //Match the ids to the details in the database and add the detail objects to a new list
                foreach (var detail in skillDetails)
                {
                    foreach (var node_id in company.CheckedRolesNodes)
                    {
                        if (node_id == detail.SkillDetailId)
                        {
                            detailsFromClient.Add(detail);
                        }
                    }
                }
            }
            if (company.CheckedFocusNodes.Count != 0)
            {
                //Match the ids to the focus in the database and add the focus objects to a new list
                foreach (var focus in focuses)
                {
                    foreach (var node_id in company.CheckedFocusNodes)
                    {
                        if (node_id == focus.FocusId)
                        {
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

            var newCompany = new Companies
            {
                CompanyId = newId,
                CompanyName = company.CompanyName,
                ContactPerson = company.ContactPerson,
                Email = company.Email,
                Phone = company.Phone,
                Bio = company.Bio,
                Website = company.Website,
                RecruitmentWebAddress = company.RecruitmentWebAddress
            };
            if (focusFromClient != null)
            {
                //var selectedFocuses = company.FocusList.Where(s => s.IsSelected).ToList();

                // Create the new rows for the new companies companySkills
                foreach (var selectedFocus in focusFromClient)
                {
                    var compFocus = new CompanyFocus
                    {
                        CompanyId = newCompany.CompanyId,
                        FocusId = selectedFocus.FocusId
                    };
                    newCompany.CompanyFocuses.Add(compFocus);
                }
            }
            if (skillsFromClient != null)
            {
                //var selectedSkills = company.SkillSetsList.Where(s => s.IsSelected).ToList();

                // Create the new rows for the new companies companySkills
                foreach (var selectedSkill in skillsFromClient)
                {
                    var compSkill = new CompanySkills
                    {
                        CompanyId = newCompany.CompanyId,
                        SkillId = selectedSkill.SkillId
                    };
                    newCompany.CompanySkills.Add(compSkill);
                }
            }
            if (detailsFromClient != null)
            {
                //var selectedDetails = company.SkillDetailsList.Where(d => d.IsSelected).ToList();

                // Create the new rows for the new companies companyDetails
                foreach (var selectedDetail in detailsFromClient)
                {
                    var compDetail = new CompanyDetails
                    {
                        CompanyId = newCompany.CompanyId,
                        SkillDetailId = selectedDetail.SkillDetailId
                    };
                    newCompany.CompanyDetails.Add(compDetail);
                }
            }
            if (skillsFromClient == null && detailsFromClient == null)
            {
                if (ModelState.IsValid)
                {

                    try
                    {
                        _context.Add(newCompany);
                        await _context.SaveChangesAsync();

                        return Ok();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        return NotFound();
                    }

                }
            }
            else
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        _context.Add(newCompany);
                        await _context.SaveChangesAsync();

                        return Ok();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        return NotFound();
                    }
                }

            }

            return NotFound();
        }

        /// <summary>
        /// Creates a skill in the database. If successful then returns true, if fail then false -- api/AddSkill
        /// 
        /// Using JObject ensures that however the data is posted, 
        /// we are able to serialize it to the related class and can be assigned a specific data type with the ToObject method which requires the datatype to serialize to.
        /// 
        /// </summary>
        /// <param name="skill"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("~/api/company/addskill")]
        public async Task<IActionResult> AddSkill([FromBody]AddSkillsBindingModel skill)
        {
            //AddSkillsBindingModel skill = value.ToObject<AddSkillsBindingModel>();

            int newId = 0;

            var getAllSkills = ApiGetAllSkills();

            if (getAllSkills.Count() <= 0)
            {
                newId = 1;
            }
            else
            {
                // Get the last antered Id from the db table and +1 ready to assign to new skill
                var lastId = ApiGetAllSkills().OrderByDescending(i => i.SkillId).First();
                newId = lastId.SkillId;
                newId++;
            }

            var newSkill = new SkillSet
            {
                SkillId = newId,
                SkillName = skill.SkillName
            };

            if (ModelState.IsValid)
            {
                _context.Add(newSkill);
                await _context.SaveChangesAsync();
                return Ok();
            }
            return NotFound();
        }

        /// <summary>
        /// Creates a detail in the database. If successful then returns true, if fail then false -- api/AddDetail
        /// 
        /// Using JObject ensures that however the data is posted, 
        /// we are able to serialize it to the related class and can be assigned a specific data type with the ToObject method which requires the datatype to serialize to.
        /// 
        /// </summary>
        /// <param name="detail"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("~/api/company/adddetail")]
        public async Task<IActionResult> AddDetail([FromBody]AddDetailsBindingModel detail)
        {
            //AddDetailsBindingModel detail = value.ToObject<AddDetailsBindingModel>();

            int newId = 0;

            var getAllDetails = ApiGetAllDetails();

            if (getAllDetails.Count() <= 0)
            {
                newId = 1;
            }
            else
            {
                // Get the last antered Id from the db table and +1 ready to assign to new skill
                var lastId = ApiGetAllDetails().OrderByDescending(i => i.SkillDetailId).First();
                newId = lastId.SkillDetailId;
                newId++;
            }

            var newSkill = new SkillDetail
            {
                SkillDetailId = newId,
                DetailName = detail.DetailName
            };

            if (ModelState.IsValid)
            {
                _context.Add(newSkill);
                var result = await _context.SaveChangesAsync();
                return Ok();

            }
            return NotFound();
        }

        /// <summary>
        /// Creates a focus in the database. If successful then returns true, if fail then false -- api/AddFocus
        /// 
        /// Using JObject ensures that however the data is posted, 
        /// we are able to serialize it to the related class and can be assigned a specific data type with the ToObject method which requires the datatype to serialize to.
        /// 
        /// </summary>
        /// <param name="focus"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("~/api/company/addfocus")]
        public async Task<IActionResult> AddFocus([FromBody]AddFocusBindingModel focus)
        {
            //AddFocusBindingModel focus = value.ToObject<AddFocusBindingModel>();

            if (ModelState.IsValid)
            {
                int newFocusId = 0;
                int newFocusNodeId = 0;

                var getAllFocuses = ApiGetAllFocus();

                var getAllFocusNodes = ApiGetAllFocusNodes();

                if (getAllFocuses.Count() <= 0)
                {
                    newFocusId = 1;
                }
                else
                {
                    // Get the last entered Id from the db table and +1 ready to assign to new id
                    var lastId = ApiGetAllFocus().OrderByDescending(i => i.FocusId).First();
                    newFocusId = lastId.FocusId;
                    newFocusId++;
                }
                if (getAllFocusNodes.Count() <= 0)
                {
                    newFocusNodeId = 1;
                }
                else
                {
                    // Get the last entered Id from the db table and +1 ready to assign to new id
                    var lastId = ApiGetAllFocusNodes().OrderByDescending(i => i.Id).First();
                    newFocusNodeId = lastId.Id;
                    newFocusNodeId++;
                }

                var newFocus = new Focus
                {
                    FocusId = newFocusId,
                    FocusType = focus.FocusType
                };
                var newFocusNode = new FocusNodes
                {
                    Id = newFocusNodeId,
                    Name = newFocus.FocusType,
                    OrderNumber = 1,
                    ParentId = 1
                };


                _context.Add(newFocus);
                _context.Add(newFocusNode);
                await _context.SaveChangesAsync();
                var result = new ObjectResult(focus);
                return result;
            }
            return NotFound();
        }

        /// <summary>
        /// Put to edit the focus in the db. Returns true if update to db is successful otherwise returns false -- api/EditFocus
        /// 
        /// Using JObject ensures that however the data is posted, 
        /// we are able to serialize it to the related class and can be assigned a specific data type with the ToObject method which requires the datatype to serialize to.
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="focus"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("~/api/company/editfocus/{id}")]
        public async Task<IActionResult> EditFocus(int id, [FromBody]AddFocusBindingModel focus)
        {
            //AddFocusBindingModel focus = value.ToObject<AddFocusBindingModel>();

            var findFocus = GetFocus(id);
            findFocus.FocusType = focus.FocusType;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(findFocus);
                    await _context.SaveChangesAsync();
                    return Ok();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (findFocus == null)
                    {
                        return NotFound();
                    }

                }
            }
            return NotFound();
        }

        /// <summary>
        /// Put to edit the skill in the db. Returns true if update to db is successful otherwise returns false -- api/EditSkill
        /// 
        /// Using JObject ensures that however the data is posted, 
        /// we are able to serialize it to the related class and can be assigned a specific data type with the ToObject method which requires the datatype to serialize to.
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="skill"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("~/api/company/editskill/{id}")]
        public async Task<IActionResult> EditSkill(int id, [FromBody] AddSkillsBindingModel skill)
        {
            //AddSkillsBindingModel skill = value.ToObject<AddSkillsBindingModel>();

            var findSkill = GetSkill(id);
            findSkill.SkillName = skill.SkillName;


            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(findSkill);
                    await _context.SaveChangesAsync();
                    return Ok();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (findSkill == null)
                    {
                        return NotFound();
                    }

                }
            }
            return NotFound();
        }

        /// <summary>
        /// Put to edit the detail in the db. Returns true if update to db is successful otherwise returns false -- api/EditDetail
        /// 
        /// Using JObject ensures that however the data is posted, 
        /// we are able to serialize it to the related class and can be assigned a specific data type with the ToObject method which requires the datatype to serialize to.
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="detail"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("~/api/company/editdetail/{id}")]
        public async Task<IActionResult> EditDetail(int id, [FromBody]AddDetailsBindingModel detail)
        {
            //AddDetailsBindingModel detail = value.ToObject<AddDetailsBindingModel>();

            var foundDetail = GetDetail(id);
            foundDetail.DetailName = detail.DetailName;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(foundDetail);
                    await _context.SaveChangesAsync();
                    return Ok();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (foundDetail == null)
                    {
                        return NotFound();
                    }

                }
            }
            return NotFound();
        }

        /// <summary>
        /// Delete the skill in the db. Returns true if update to db is successful otherwise returns false -- api/DeleteSkill
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("~/api/company/deleteskill/{id}")]
        public async Task<IActionResult> DeleteSkill(int id)
        {
            var skill = GetSkill(id);

            TreeNodes temp = new TreeNodes();

            var treeNodesList = _context.TreeNodes.ToList();

            try
            {
                var tempSkills = _context.CompanySkills.Where(s => s.SkillId == skill.SkillId).ToList();

                // get the matching skill in the tree nodes table
                var skillNode = from s in treeNodesList
                                .Where(s => s.Name == skill.SkillName)
                                select s;

                // If not null then assign the skill as a temp node
                if (skillNode != null)
                {
                    foreach (var item in skillNode)
                    {
                        temp = item;
                    }
                }

                //var nodesToRemove = RemoveRolesChildren(_context.TreeNodes.ToList(), temp.Id);

                //foreach (var item in nodesToRemove)
                //{
                //    treeNodesList.Remove(item);

                //    foreach (var childNode in _context.SkillDetail.Where(d => d.DetailName == item.Name))
                //    {
                //        _context.Remove(childNode);
                //    }
                //}


                // remove the skills from the company skills table
                foreach (var s in tempSkills)
                {
                    _context.CompanySkills.RemoveRange(s);
                }
                // remove the skill from the skill set table
                _context.SkillSet.Remove(skill);
                await _context.SaveChangesAsync();
                var result = new ObjectResult(skill);
                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return NotFound();
            }
        }

        private List<TreeNodes> RemoveRolesChildren(List<TreeNodes> nodes, int parentId)
        {
            return nodes.Where(l => l.ParentId == parentId).OrderBy(l => l.OrderNumber)
                .Select(l => new TreeNodes
                {
                    Name = l.Name,
                    Id = l.Id,
                    ParentId = l.ParentId,
                    Children = RemoveRolesChildren(nodes, l.Id)
                }).ToList();
        }

        /// <summary>
        /// Delete the detail in the db. Returns true if update to db is successful otherwise returns false -- api/DeleteDetail
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("~/api/company/deletedetail/{id}")]
        public async Task<IActionResult> DeleteDetail(int id)
        {
            var detail = GetDetail(id);

            try
            {
                var tempDetails = _context.CompanyDetails.Where(s => s.SkillDetailId == detail.SkillDetailId).ToList();

                foreach (var d in tempDetails)
                {
                    _context.CompanyDetails.RemoveRange(d);
                }

                _context.SkillDetail.Remove(detail);
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return NotFound();
            }
        }

        /// <summary>
        /// Delete the focus in the db. Returns true if update to db is successful otherwise returns false -- api/DeleteFocus
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("~/api/company/deletefocus/{id}")]
        public async Task<IActionResult> DeleteFocus(int id)
        {
            var focus = GetFocus(id);

            try
            {
                var tempFocuses = _context.CompanyFocus.Where(s => s.FocusId == focus.FocusId).ToList();

                foreach (var f in tempFocuses)
                {
                    _context.CompanyFocus.RemoveRange(f);
                }

                _context.Focus.Remove(focus);
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return NotFound();
            }
        }

        /// <summary>
        /// Delete the company in the db. Returns true if update to db is successful otherwise returns false -- api/DeleteCompany
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("~/api/company/deletecompany/{id}")]
        public async Task<IActionResult> DeleteCompany(int id)
        {
            var comp = ApiGetAllCompanies().FirstOrDefault(c => c.CompanyId == id);

            try
            {
                // Delete the child entities first
                var tempCompSkills = _context.CompanySkills.Where(c => c.CompanyId == comp.CompanyId).ToList();
                var tempCompDetails = _context.CompanyDetails.Where(c => c.CompanyId == comp.CompanyId).ToList();
                var tempCompFocuses = _context.CompanyFocus.Where(c => c.CompanyId == comp.CompanyId).ToList();

                foreach (var skill in tempCompSkills)
                {
                    _context.CompanySkills.RemoveRange(skill);
                }
                foreach (var detail in tempCompDetails)
                {
                    _context.CompanyDetails.RemoveRange(detail);
                }
                foreach (var focus in tempCompFocuses)
                {
                    _context.CompanyFocus.RemoveRange(focus);
                }

                // Then remove the company from the companies table
                _context.Companies.Remove(comp);
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return NotFound();
            }
        }

        /// <summary>
        /// Edit the home page -- api/GetHomePage
        /// 
        /// Using JObject ensures that however the data is posted, 
        /// we are able to serialize it to the related class and can be assigned a specific data type with the ToObject method which requires the datatype to serialize to.
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="home"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("~/api/company/edithomepage/{id}")]
        public async Task<IActionResult> EditHomepge(int id, [FromBody] HomePageBindingModel home)
        {
            //HomePageBindingModel home = value.ToObject<HomePageBindingModel>();

            if (ModelState.IsValid)
            {
                var tag = _context.HomePage.FirstOrDefault();
                if (tag != null)
                {
                    _context.HomePage.Remove(tag);
                }

                // Create new home page object and add the new tagline
                var homePageInfo = new HomePage
                {
                    TagLine = home.Tagline
                };

                // Save the new tagline to the database and return to the view
                _context.HomePage.Add(homePageInfo);
                await _context.SaveChangesAsync();
                return Ok();
            }
            return NotFound();
        }

        private bool CompanyExists(int id)
        {
            return ApiGetAllCompanies().Any(e => e.CompanyId == id);
        }

        /// <summary>
        /// GET for the add skills and details admin view. The skills and details are loaded via the repo from the db and shown in a table format.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("~/api/company/indexskillsdetailsfocus")]
        public IActionResult IndexSkillsDetailsFocus()
        {
            var sd = new SkillDetailFocusBindingModel()
            {
                SkillSets = ApiGetAllSkills(),
                SkillDetails = ApiGetAllDetails(),
                Focuses = ApiGetAllFocus()
            };

            var result = new ObjectResult(sd);
            return result;
        }
    }
}
