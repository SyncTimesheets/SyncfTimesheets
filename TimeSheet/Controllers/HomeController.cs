using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TimeSheet.Models;

namespace TimeSheet.Controllers
{
    public class HomeController : Controller
    {

        public JIRAAccessModel jiraAccessModel = new TimeSheet.Models.JIRAAccessModel();
   
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Timesheet()
        {            
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        
        [HttpPost]
        [ActionName("FilterDateRange")]
        public JsonResult FilterDateRange(string query)
        {
            var result = new JiraIssueDetailsResult();
            var jiraAccessModel = new TimeSheet.Models.JIRAAccessModel();
            result = jiraAccessModel.GetIssueWorkLogDetailsfromJIRA(query);
            return Json(result);
        }
        public ActionResult GetReportBasedonQuery(DateTime startdate, DateTime enddate, string worklogauthor)
        {
            var result = new JiraIssueDetailsResult();
            var jiraAccessModel = new TimeSheet.Models.JIRAAccessModel();
            string query = "(worklogDate >=" + startdate.ToString("yyyy-MM-dd") + " and worklogDate <=" + enddate.ToString("yyyy-MM-dd") + " and worklogAuthor in ('" + worklogauthor + "'))";
            result = jiraAccessModel.GetIssueWorkLogDetailsfromJIRA(query);
            ViewBag.Result = result;
            return View("~/Views/Home/Timesheet.cshtml");
        }

        [HttpPost]
        public bool checkCrendintail(string UserName, string Password) {
            var loginResult = jiraAccessModel.ValidateJIRACredentials(UserName, Password);           
            return loginResult;            
        }

        [HttpPost]
        public ActionResult Logout()
        {
            return RedirectToAction("Index", "Home");
        }
        [HttpPost]
        public bool subscribe() {
            var result = jiraAccessModel.UpdateSubscriptionDetails();
            return result;
        }
    }
}