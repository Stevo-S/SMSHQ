using System.Web.Mvc;
using Hangfire;

namespace MessageSender.Controllers
{
    public class JobsController : Controller
    {
        // GET: Jobs
        public ActionResult Index()
        {
            var monitor = JobStorage.Current.GetMonitoringApi();
            var jobs = monitor.ProcessingJobs(0, (int)monitor.ProcessingCount());
            return View(jobs);
        }
    }
}