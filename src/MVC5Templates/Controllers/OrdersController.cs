using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MVC5Templates.Models;

namespace MVC5Templates.Controllers
{
    public class OrdersController : Controller
    {
        private NorthWndContext _db;

        public OrdersController()
        {
            _db = new NorthWndContext();
            _db.Configuration.ProxyCreationEnabled = false;
        }

        // GET: Orders
		public ActionResult Index()
        {
			var orders = _db.Orders.Include(o => o.Customer)
								.Include(o => o.Employee)
								.Include(o => o.Shipper)
								.ToList();

			return View(orders);
        }

        // GET: Orders/Details/5
		public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var order = _db.Orders.Include(o => o.Customer)
								.Include(o => o.Employee)
								.Include(o => o.Shipper)
								.SingleOrDefault(w => w.OrderID == id);

            if (order == null)
                return HttpNotFound();

            return View(order);
        }

        // GET: Orders/Create
        public ActionResult Create()
        {
			var vm = new MVC5Templates.Models.OrderViewModel();
			vm = RefreshViewModel(vm);

			return View(vm);
        }

        // POST: Orders/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
		public ActionResult Create(MVC5Templates.Models.OrderViewModel orderViewModel)
        {
            if (ModelState.IsValid)
            {
				using(var db = new NorthWndContext())
				{
					db.Orders.Add(orderViewModel.Order);
					db.SaveChanges();
				}
                return RedirectToAction("Index");
            }

			orderViewModel = RefreshViewModel(orderViewModel);
			return View(orderViewModel);
        }

        // GET: Orders/Edit/5
		public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var order = _db.Orders.Find(id);
            if (order == null)
                return HttpNotFound();

			var orderViewModel = new MVC5Templates.Models.OrderViewModel();
			orderViewModel.Order = order;
			orderViewModel = RefreshViewModel(orderViewModel);

			return View(orderViewModel);
        }

        // POST: Orders/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
		public ActionResult Edit(MVC5Templates.Models.OrderViewModel orderViewModel)
        {
            if (ModelState.IsValid)
            {
				using(var db = new NorthWndContext())
				{
					db.Entry(orderViewModel.Order).State = EntityState.Modified;
					db.SaveChanges();
				}
                return RedirectToAction("Index");
            }

			orderViewModel = RefreshViewModel(orderViewModel);
			return View(orderViewModel);
        }

        // GET: Orders/Delete/5
		public ActionResult Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var order = _db.Orders.Include(o => o.Customer)
								.Include(o => o.Employee)
								.Include(o => o.Shipper)
								.SingleOrDefault(w => w.OrderID == id);

            if (order == null)
                return HttpNotFound();

            return View(order);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
		public ActionResult DeleteConfirmed(int id)
        {
			using(var db = new NorthWndContext())
			{
				var order = db.Orders.Find(id);
				db.Orders.Remove(order);
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
		
		private MVC5Templates.Models.OrderViewModel RefreshViewModel(OrderViewModel orderViewModel)
		{
			orderViewModel.Customers = _db.Customers.OrderBy(ob => ob.CompanyName).ToList();
			orderViewModel.Employees = _db.Employees.OrderBy(ob => ob.LastName).ToList();
			orderViewModel.Shippers = _db.Shippers.OrderBy(ob => ob.CompanyName).ToList();

			return orderViewModel;
		}
	}
}
