using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CompanyDatabase.Models;
using CompanyFinderAPI.BindingModels;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CompanyFinderAPI.Controllers
{
    /// <summary>
    /// Competence controller for handling requests from the main website client
    /// </summary>
    //[EnableCors("SiteCorsPolicy")]
    [Produces("application/json")]
    [Route("api/Competence")]
    public class CompetenceController : Controller
    {
        private CompanyDbContext _context;
        private UserManager<IdentityUser> userManger;
        private SignInManager<IdentityUser> signInManager;

        /// <summary>
        /// Constructor, initializes db context
        /// </summary>
        /// <param name="context"></param>
        /// <param name="userMgr"></param>
        /// <param name="signInMgr"></param>
        public CompetenceController(CompanyDbContext context, UserManager<IdentityUser> userMgr, SignInManager<IdentityUser> signInMgr)
        {
            _context = context;
            userManger = userMgr;
            signInManager = signInMgr;
        }

        /// <summary>
        /// Get the home page info -- api/GetHomePage
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("~/api/gethomepage")]
        public IActionResult GetHomePage()
        {
            var tagline = ApiGetHomePageInfo();
            var homepageBindingModel = new HomePageBindingModel();

            if (tagline != null)
            {
                homepageBindingModel.Tagline = tagline.TagLine;
                var result = new ObjectResult(homepageBindingModel);
                return result;
            }
            else if (tagline == null)
            {
                homepageBindingModel.Tagline = "Default Tagline - Change me";
                var homepage = new HomePage
                {
                    TagLine = homepageBindingModel.Tagline
                };
                _context.HomePage.Add(homepage);
                _context.SaveChanges();
                var result = new ObjectResult(homepageBindingModel);
                return result;
            }

            return NotFound();
        }
        private HomePage ApiGetHomePageInfo() => _context.HomePage.FirstOrDefault();

        /// <summary>
        /// Post to login to the main app, checkes the users login details against the database, if ok returns status code 200
        /// </summary>
        /// <param name="homePageViewModel"></param>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("~/api/homepagelogin")]
        public async Task<IActionResult> HomePageLogin([FromBody]HomePageBindingModel homePageViewModel, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                IdentityUser user = await userManger.FindByNameAsync(homePageViewModel.SearchUsername);

                if (user != null)
                {
                    await signInManager.SignOutAsync();
                    if ((await signInManager.PasswordSignInAsync(user, homePageViewModel.SearchPassword, false, false)).Succeeded)
                    {
                        return Ok(user);
                    }
                    else
                    {
                        return Unauthorized();
                    }

                }
            }

            return Unauthorized();

        }

        /// <summary>
        /// Gets the tree nodes from the database
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("~/api/gettreenodes")]
        public IActionResult GetTreeNodes()
        {
            var treeNodes = _context.TreeNodes.ToList();

            var result = new ObjectResult(treeNodes);
            return result;
        }


        /// <summary>
        /// Gets the focus nodes from the database
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("~/api/getfocusnodes")]
        public IActionResult GetFocusNodes()
        {
            var focusNodes = _context.FocusNodes.ToList();
            var result = new ObjectResult(focusNodes);
            return result;
        }



        private IEnumerable<Companies> GetAllCompanies() => _context.Companies;

        /// <summary>
        /// Post for the search selections. The client sends the users selections via ajax and the method checks the db and returns the matching companies
        /// </summary>
        /// <param name="searchSelections"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("~/api/search")]
        public IActionResult SearchResults([FromBody]SearchSelectionsBindingModel searchSelections)
        {
            List<TreeNodes> selectedSkills = searchSelections.UniqueNodeArray;
            List<FocusNodes> selectedFocuses = searchSelections.UniqueFocusNodeArray;

            if (ModelState.IsValid)
            {
                ////Initialize the companies in the tables
                var allCompanies = GetAllCompanies();
                var allCompsInCompanySkillsTable = new List<CompanySkills>();
                var allCompsInCompanyDetailsTable = new List<CompanyDetails>();
                var allCompsInCompanyFocusTable = new List<CompanyFocus>();
                foreach (var company in allCompanies)
                {
                    allCompsInCompanySkillsTable.AddRange(company.CompanySkills);
                    allCompsInCompanyDetailsTable.AddRange(company.CompanyDetails);
                    allCompsInCompanyFocusTable.AddRange(company.CompanyFocuses);
                }

                var tempCompList = new List<Companies>();
                var tempFocusMatch = new List<Companies>();

                var focuses = new List<int>();
                var skills = new List<int>();
                var details = new List<int>();

                foreach (var focusName in _context.Focus)
                {
                    foreach (var selectedFocusName in selectedFocuses)
                    {
                        if (selectedFocusName.Name == focusName.FocusType)
                        {
                            focuses.Add(focusName.FocusId);
                        }
                    }
                }
                foreach (var skillName in _context.SkillSet)
                {
                    foreach (var selectedSkillName in selectedSkills)
                    {
                        if (selectedSkillName.Name == skillName.SkillName)
                        {
                            skills.Add(skillName.SkillId);
                        }
                    }
                }
                foreach (var detailName in _context.SkillDetail)
                {
                    foreach (var selectedDetailName in selectedSkills)
                    {
                        if (selectedDetailName.Name == detailName.DetailName)
                        {
                            details.Add(detailName.SkillDetailId);
                        }
                    }
                }

                foreach (var company in allCompanies)
                {
                    foreach (var focus in _context.CompanyFocus.Where(c => c.CompanyId == company.CompanyId))
                    {
                        foreach (var selectedFocus in focuses.Where(f => f == focus.FocusId))
                        {
                            tempFocusMatch.Add(company);
                        }
                    }
                }

                List<Companies> uniqueFocusMatch = tempFocusMatch.Distinct().ToList();

                foreach (var comp in uniqueFocusMatch)
                {
                    var foundCompSkills = _context.CompanySkills.Where(c => c.CompanyId == comp.CompanyId).ToList();

                    foreach (var compSkill in foundCompSkills)
                    {
                        foreach (var skill in skills.Where(s => s == compSkill.SkillId))
                        {
                            if (compSkill.Company.SkillList == null)
                            {
                                compSkill.Company.SkillList = new List<SkillSet>();
                            }
                            compSkill.Company.SkillList.Add(compSkill.SkillSet);
                            tempCompList.Add(compSkill.Company);
                        }
                    }
                }

                foreach (var comp in uniqueFocusMatch)
                {
                    var foundCompDetails = _context.CompanyDetails.Where(c => c.CompanyId == comp.CompanyId).ToList();


                    foreach (var compDetail in foundCompDetails)
                    {

                        foreach (var detail in details.Where(s => s == compDetail.SkillDetailId))
                        {
                            if (compDetail.Company.DetailList == null)
                            {
                                compDetail.Company.DetailList = new List<SkillDetail>();
                            }
                            compDetail.Company.DetailList.Add(compDetail.SkillDetail);
                            tempCompList.Add(compDetail.Company);
                        }
                    }
                }

                var sortedList = from company in tempCompList
                                 group company by company into match
                                 orderby match.Count() descending
                                 select match.Key;


                List<Companies> matches = sortedList.Distinct().ToList();

                var result = new ObjectResult(matches);
                return result;
            }
            else
            {
                return NotFound();
            }

        }

    }
}
