using System.Linq;
using System.Web;
using System.Web.Mvc;
using Course3_2.Models;
using Course3_2.Service;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using MicroServiceCore.Model;

namespace Course3_2.Controllers
{
    public class HomeController : Controller
    {
        private UserManager<ApplicationUser> _userManager;
        private ApplicationDbContext applicationDbContext = new ApplicationDbContext();
        private UserManager<ApplicationUser> Manager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            set
            {
                _userManager = value;
            }
        }

        private ApiGatewayService apiGateway;

        public HomeController()
        {
            apiGateway = new ApiGatewayService();
            apiGateway.Initialize();
        }

        public HomeController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
            apiGateway = new ApiGatewayService();
            apiGateway.Initialize();
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult AddMail()
        {
            if (User.Identity.IsAuthenticated)
            {
                return View("AddMailView");
            }

            return View("Index");
        }

        [HttpPost]
        public ActionResult AddMail(EmailModel model)
        {
            if (ModelState.IsValid)
            {
                //model.User = Manager.FindById(User.Identity.GetUserId());
                applicationDbContext.EmailModels.Add(model);
                applicationDbContext.SaveChanges();

                
            }
            return View("Index");
        }

        public ActionResult GetMailForAllAccounts()
        {
            if (User.Identity.IsAuthenticated)
            {

                ApplicationUser user = Manager.FindById(User.Identity.GetUserId());

                var emails = applicationDbContext.EmailModels.Where(x => x.User.Id == user.Id);
                var messages = new System.Collections.Generic.List<EmailMessage>();
                foreach (var mail in emails)
                {
                    var addressModel = new EmailAddressModel
                    {
                        Email = mail.Email,
                        Password = mail.Password,
                        PopPort = 995
                    };

                    var mList = apiGateway.GetMessagesPop3(addressModel);
                    messages.AddRange(mList);
                }
                
                return View("AllMail",messages);
            }

            return View("Index");
        }
    }
}