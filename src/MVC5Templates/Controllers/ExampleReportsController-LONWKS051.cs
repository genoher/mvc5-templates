using DuetGroup.WebsiteUtils.Controllers;
using DuetGroup.WebsiteUtils.Filters;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using DuetGroup.Database.Rpt;

namespace DuetGroup.Reports.Website.Controllers
{
    public class ReportsController : DuetControllerBase
    {
        private RptContext _db;

        public ReportsController()
        {
            _db = new RptContext();
            _db.Configuration.ProxyCreationEnabled = false;
        }

        // GET: Reports
		public ActionResult Index()
        {
			var reports = _db.Reports
                             .Include(r => r.Connection)
							 .Include(r => r.SqlType)
							 .ToList();

			return View(reports);
        }

        [Route("Reports/{reportName}/ViewReport")]
        public ActionResult ViewReport(string reportName)
        {
            var rpt = _db.Reports
                         .Include(i => i.SqlType)
                         .Include(i => i.ReportParameters)
                         .Where(w => w.Name == reportName)
                         .First();


            var vm = new ViewReportViewModel
            {
                Report = rpt,
                RestUrl = this.Url.Content("~/") + "rptapi/" + rpt.Name
            };

            return View(vm);
        }

        // GET: Reports/Details/5
		public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var report = _db.Reports.Include(r => r.Connection)
								.Include(r => r.SqlType)
								.SingleOrDefault(w => w.ReportId == id);

            if (report == null)
                return HttpNotFound();

            return View(report);
        }

        // GET: Reports/Create
        public ActionResult Create()
        {
			var vm = new ReportViewModel();
			vm = RefreshViewModel(vm);

			return View(vm);
        }

        // POST: Reports/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
		public ActionResult Create(ReportViewModel reportViewModel)
        {
            if (ModelState.IsValid)
            {
                reportViewModel.Report.CleanName();

				using(var db = new RptContext())
				{
					db.Reports.Add(reportViewModel.Report);
					db.SaveChanges();
				}
                return RedirectToAction("Index");
            }

			reportViewModel = RefreshViewModel(reportViewModel);
			return View(reportViewModel);
        }

        // GET: Reports/Edit/5
		public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var report = _db.Reports.Find(id);
            if (report == null)
                return HttpNotFound();

			var reportViewModel = new ReportViewModel();
            reportViewModel.Report = report;
			reportViewModel = RefreshViewModel(reportViewModel);

			return View(reportViewModel);
        }

        // POST: Reports/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
		public ActionResult Edit(ReportViewModel reportViewModel)
        {
            if (ModelState.IsValid)
            {
                reportViewModel.Report.CleanName();

				using(var db = new RptContext())
				{
					db.Entry(reportViewModel.Report).State = EntityState.Modified;
					db.SaveChanges();
				}
                return RedirectToAction("Index");
            }

			reportViewModel = RefreshViewModel(reportViewModel);
			return View(reportViewModel);
        }

        // GET: Reports/Delete/5
		public ActionResult Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var report = _db.Reports.Include(r => r.Connection)
								.Include(r => r.SqlType)
								.SingleOrDefault(w => w.ReportId == id);

            if (report == null)
                return HttpNotFound();

            return View(report);
        }

        // POST: Reports/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
		public ActionResult DeleteConfirmed(int id)
        {
			using(var db = new RptContext())
			{
				var report = db.Reports.Find(id);
				db.Reports.Remove(report);
				db.SaveChanges();
			}

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }
		
		private ReportViewModel RefreshViewModel(ReportViewModel reportViewModel)
		{
			reportViewModel.Connections = _db.Connections.OrderBy(ob => ob.Name).ToList();
			reportViewModel.SqlTypes = _db.SqlTypes.OrderBy(ob => ob.Name).ToList();

			return reportViewModel;
		}
	}
}
