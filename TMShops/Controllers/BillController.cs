using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TM;
using PagedList;
using System.Web.Services.Description;

namespace TMShops.Controllers
{
    [Filters.AuthFilters()]
    public class BillController : BaseController
    {
        // GET: Bill
        public ActionResult Index(int? flag, string order, string currentFilter, string searchString, int? page, string export)
        {
            try
            {
                if (searchString != null)
                {
                    page = 1;
                    searchString = searchString.Trim();
                }
                else searchString = currentFilter;
                ViewBag.order = order;
                ViewBag.currentFilter = searchString;
                ViewBag.flag = flag;

                var rs = from d in db.Bills select d;

                if (!String.IsNullOrEmpty(searchString))
                {
                    //var customers = db.customers.Where(d => d.full_name.Contains(searchString) && d.flag > 0).Select(d => d.id).ToList();
                    rs = rs.Where(d => d.codeKey.Contains(searchString) || d.customerId.Contains(searchString));
                }
                if (flag == 1) rs = rs.Where(d => d.flag == 1);
                else rs = rs.Where(d => d.flag == 0);

                switch (order)
                {
                    case "key_desc":
                        rs = rs.OrderByDescending(d => d.codeKey);
                        break;
                    case "key_asc":
                        rs = rs.OrderBy(d => d.codeKey);
                        break;
                    case "customer_desc":
                        rs = rs.OrderByDescending(d => d.customerId);
                        break;
                    case "customer_asc":
                        rs = rs.OrderBy(d => d.customerId);
                        break;
                    case "item_desc":
                        rs = rs.OrderByDescending(d => d.totalItem);
                        break;
                    case "item_asc":
                        rs = rs.OrderBy(d => d.totalItem);
                        break;
                    case "quantity_desc":
                        rs = rs.OrderByDescending(d => d.totalQuantity);
                        break;
                    case "quantity_asc":
                        rs = rs.OrderBy(d => d.totalQuantity);
                        break;
                    case "price_desc":
                        rs = rs.OrderByDescending(d => d.totalPrice);
                        break;
                    case "price_asc":
                        rs = rs.OrderBy(d => d.totalPrice);
                        break;
                    case "user_desc":
                        rs = rs.OrderByDescending(d => d.createdBy);
                        break;
                    case "user_asc":
                        rs = rs.OrderBy(d => d.createdBy);
                        break;
                    case "date_asc":
                        rs = rs.OrderBy(d => d.createdAt);
                        break;
                    default:
                        rs = rs.OrderByDescending(d => d.createdAt);
                        break;
                }

                //Export to any
                if (!String.IsNullOrEmpty(export))
                {
                    TM.Exports.ExportExcel(TM.Helper.Data.LinqToDataTable(rs.ToList()), "Danh sách sản phẩm");
                    return RedirectToAction("Index");
                }

                ViewBag.TotalRecords = rs.Count();
                int pageSize = 15;
                int pageNumber = (page ?? 1);

                return View(rs.ToPagedList(pageNumber, pageSize));
            }
            catch (Exception ex)
            {
                this.danger(TM.Common.Language.msgError);
            }
            return View();
        }

        // GET: Bill/Details/5
        public ActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var bill = db.Bills.Find(id);
            if (bill == null)
            {
                return HttpNotFound();
            }
            return View(bill);
        }

        // GET: Bill/Create
        public ActionResult Create()
        {
            //ViewBag.customer_id = new SelectList(db.customers, "id", "full_name");
            return View();
        }

        // POST: Bill/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "customer_id,total_item,total_quantity,total_price,desc,flag")] Models.Bill bill, FormCollection collection)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (bill.totalItem == 0)
                    {
                        this.danger("Vui lòng chọn ít nhất một sản phẩm");
                        return RedirectToAction("Create");
                    }
                    bill.id = Guid.NewGuid();
                    bill.codeKey = GetBillCode(Guid.NewGuid().ToString().Split('-')[4].ToUpper());
                    bill.createdBy = Common.Auth.getUser().id.ToString();
                    bill.createdAt = DateTime.Now;
                    db.Bills.Add(bill);

                    //Sub_Bill
                    var listItem = collection["listItem[]"].Split(',');
                    foreach (var item in listItem)
                    {
                        var i = item.Split('|');
                        var sub_bill = new Models.SubBill();
                        sub_bill.id = Guid.NewGuid();
                        sub_bill.idKey = bill.id;
                        sub_bill.codeKey = bill.codeKey;
                        sub_bill.itemId = Guid.Parse(i[0]);
                        sub_bill.title = i[1];
                        sub_bill.quantity = Int32.Parse(i[2]);
                        sub_bill.priceOld = decimal.Parse(i[3]);
                        sub_bill.price = decimal.Parse(i[4]);
                        sub_bill.totalPrice = sub_bill.quantity * sub_bill.price;
                        sub_bill.flag = 1;

                        //Update product quantity in store
                        var items = db.Items.Find(sub_bill.itemId);
                        if (items.quantity < sub_bill.quantity)
                        {
                            this.danger("Sản phẩm Không đủ số lượng trong kho!");
                            return RedirectToAction("Create");
                        }
                        items.quantity = items.quantity - sub_bill.quantity;
                        db.Entry(items).State = EntityState.Modified;
                        db.SubBills.Add(sub_bill);
                    }
                    db.SaveChanges();
                    this.success(TM.Common.Language.msgCreateSucsess);
                    return RedirectToAction("Create");
                }
            }
            catch (Exception)
            {
                this.danger(TM.Common.Language.msgCreateError);
            }
            return View(bill);
        }

        // GET: Bill/Edit/5
        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var bill = db.Bills.Find(id);
            if (bill == null)
            {
                return HttpNotFound();
            }
            //ViewBag.customer_id = new SelectList(db.customers, "id", "full_name", bill.customer_id);
            ViewBag.sub_bill = db.SubBills.Where(d => d.idKey == bill.id && d.flag > 0).ToList();
            return View(bill);
        }

        // POST: Bill/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,desc,flag")] Models.Bill bill_tmp)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var bill = db.Bills.Find(bill_tmp.id);
                    bill.flag = bill_tmp.flag;
                    bill.desc = bill_tmp.desc;
                    db.Entry(bill).State = EntityState.Modified;
                    db.SaveChanges();
                    this.success(TM.Common.Language.msgUpdateSucsess);
                    return RedirectToAction("Index");
                }
                else
                    this.danger(TM.Common.Language.msgError);
            }
            catch (Exception)
            {
                this.danger(TM.Common.Language.msgUpdateError);
            }
            return RedirectToAction("Edit");
        }

        // GET: Bill/Delete/5
        //public ActionResult Delete(Guid? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Models.bill bill = db.bills.Find(id);
        //    if (bill == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(bill);
        //}

        //// POST: Bill/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(Guid id)
        //{
        //    Models.bill bill = db.bills.Find(id);
        //    db.bills.Remove(bill);
        //    db.SaveChanges();
        //    return RedirectToAction("Index");
        //}
        private string GetBillCode(string code)
        {
            var check = db.Bills.Any(d => d.codeKey == code);
            if (check)
                return GetBillCode(Guid.NewGuid().ToString().Split('-')[4].ToUpper());
            else return code;
        }
        [HttpGet]
        public JsonResult update_flag(string uid)
        {
            try
            {
                string[] id = uid.Split(',');
                var flag = 0;
                foreach (var item in id)
                {
                    var tmp = Guid.Parse(item);
                    var rs = db.Groups.Find(tmp);
                    rs.flag = flag = rs.flag == 1 ? 0 : 1;
                }
                db.SaveChanges();
                return Json(new { success = (flag == 0 ? TM.Common.Language.msgLockSucsess : TM.Common.Language.msgRecoverSucsess) }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception) { return Json(new { danger = TM.Common.Language.msgError }, JsonRequestBehavior.AllowGet); }
        }
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public JsonResult Delete(string uid)
        //{
        //    try
        //    {
        //        string[] id = uid.Split(',');
        //        foreach (var item in id)
        //        {
        //            var tmp = Guid.Parse(item);
        //            var rs = db.groups.Find(tmp);
        //            db.groups.Remove(rs);
        //        }
        //        db.SaveChanges();
        //        return Json(new { success = VinaphoneCommon.Language.msgDeleteSucsess }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception) { return Json(new { danger = VinaphoneCommon.Language.msgError }, JsonRequestBehavior.AllowGet); }
        //}
        [HttpGet]
        public JsonResult getCustomer(string term)
        {
            try
            {
                var data = db.Customers.Where(d => d.fullName.Contains(term) && d.flag > 0).Select(d => d.fullName).Take(5).ToList();
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception) { return Json(new { danger = TM.Common.Language.msgError }, JsonRequestBehavior.AllowGet); }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult getQuantity(Guid? id, long quantity)
        {
            try
            {
                var item = db.Items.Find(id);
                if (item != null)
                {
                    if (item.quantity >= quantity)
                        return Json(new { success = TM.Common.Language.msgSucsess, data = quantity }, JsonRequestBehavior.AllowGet);
                    else
                        return Json(new { success = TM.Common.Language.msgSucsess, data = item.quantity }, JsonRequestBehavior.AllowGet);
                }
                else
                    return Json(new { danger = TM.Common.Language.msgError }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception) { return Json(new { danger = TM.Common.Language.msgError }, JsonRequestBehavior.AllowGet); }
        }
        public ActionResult ProductList(int? flag, string order, string currentFilter, string searchString, int? page, string export)
        {
            try
            {
                if (searchString != null)
                {
                    page = 1;
                    searchString = searchString.Trim();
                }
                else searchString = currentFilter;
                ViewBag.order = order;
                ViewBag.currentFilter = searchString;
                ViewBag.flag = flag;

                var rs = from d in db.Items where d.appKey == AppKey.product && d.quantity > 0 && d.flag > 0 select d;

                if (!String.IsNullOrEmpty(searchString))
                    rs = rs.Where(d =>
                    d.idKey.Contains(searchString) ||
                    d.title.Contains(searchString));

                //if (flag == 0) rs = rs.Where(d => d.flag == 0);
                //else rs = rs.Where(d => d.flag == 1);

                switch (order)
                {
                    case "key_desc":
                        rs = rs.OrderByDescending(d => d.codeKey);
                        break;
                    case "key_asc":
                        rs = rs.OrderBy(d => d.codeKey);
                        break;
                    case "quantity_desc":
                        rs = rs.OrderByDescending(d => d.quantity);
                        break;
                    case "quantity_asc":
                        rs = rs.OrderBy(d => d.quantity);
                        break;
                    case "priceOld_desc":
                        rs = rs.OrderByDescending(d => d.priceOld);
                        break;
                    case "priceOld_asc":
                        rs = rs.OrderBy(d => d.priceOld);
                        break;
                    case "price_desc":
                        rs = rs.OrderByDescending(d => d.price);
                        break;
                    case "price_asc":
                        rs = rs.OrderBy(d => d.price);
                        break;
                    case "title_desc":
                        rs = rs.OrderByDescending(d => d.title);
                        break;
                    default:
                        rs = rs.OrderBy(d => d.title);
                        break;
                        //case "id_asc":
                        //    rs = rs.OrderByDescending(d => d.id);
                        //    break;
                        //default:
                        //    rs = rs.OrderBy(d => d.id);
                        //    break;
                }

                //Export to any
                if (!String.IsNullOrEmpty(export))
                {
                    TM.Exports.ExportExcel(TM.Helper.Data.LinqToDataTable(rs.ToList()), "Danh sách sản phẩm");
                    return RedirectToAction("Index");
                }

                ViewBag.TotalRecords = rs.Count();
                int pageSize = 10;
                int pageNumber = (page ?? 1);

                IEnumerable<Models.Item> item = db.Items;
                return PartialView(rs.ToPagedList(pageNumber, pageSize));
            }
            catch (Exception ex)
            {
                this.danger(TM.Common.Language.msgError);
            }
            return PartialView();
        }
    }
}
