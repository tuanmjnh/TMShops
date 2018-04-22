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

namespace TMShops.Controllers
{
    [Filters.AuthFilters()]
    public class ProductController : BaseController
    {
        // GET: Product
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

                var rs = from d in db.Items where d.appKey == AppKey.product select d;

                if (!String.IsNullOrEmpty(searchString))
                    rs = rs.Where(d =>
                    d.idKey.Contains(searchString) ||
                    d.title.Contains(searchString));

                if (flag == 0) rs = rs.Where(d => d.flag == 0);
                else rs = rs.Where(d => d.flag == 1);

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
                    case "quantitytotal_desc":
                        rs = rs.OrderByDescending(d => d.quantityTotal);
                        break;
                    case "quantitytotal_asc":
                        rs = rs.OrderBy(d => d.quantityTotal);
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

        // GET: Product/Details/5
        public ActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var item = db.Items.Find(id);
            if (item == null)
            {
                return HttpNotFound();
            }
            return View(item);
        }

        // GET: Product/Create
        public ActionResult Create()
        {
            ViewBag.group = getGroup(AppKey.product, "0", Guid.Empty);
            return View();
        }

        // POST: Product/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "title,code_key,desc,quantity,price_old,price,flag")] Models.Item item, FormCollection collection)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (db.Items.Any(d => d.codeKey.ToUpper() == item.codeKey.ToUpper() && d.appKey == AppKey.product))
                    {
                        this.danger(TM.Common.Language.msgExistError);
                        return RedirectToAction("Create");
                    }
                    //Item
                    
                    item.id = Guid.NewGuid();
                    item.appKey = AppKey.product;
                    item.codeKey = item.codeKey.ToUpper();
                    item.quantityTotal = item.quantity;
                    item.createdBy = Common.Auth.getUser().id.ToString();
                    item.createdAt = DateTime.Now;
                    //Images
                    var images = TM.IO.UploadImages(Request.Files, DirUpload.imagesProduct);
                    item.images = images.UploadFileString();
                    db.Items.Add(item);

                    //Group Item
                    var group_item = new Models.GroupItem();
                    group_item.id = Guid.NewGuid();
                    group_item.appKey = item.appKey;
                    group_item.groupId = Guid.Parse(collection["group_id"]);
                    group_item.itemId = item.id;
                    group_item.flag = item.flag;
                    group_item.createdBy = Common.Auth.getUser().id.ToString();
                    group_item.createdAt = DateTime.Now;
                    db.GroupItems.Add(group_item);

                    db.SaveChanges();
                }
                else
                    this.danger(TM.Common.Language.msgError);
                this.success(TM.Common.Language.msgCreateSucsess);
            }
            catch (Exception ex)
            {
                this.danger(TM.Common.Language.msgCreateError);
            }
            return RedirectToAction("Create");
        }

        // GET: Product/Edit/5
        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var item = db.Items.Find(id);
            if (item == null)
            {
                return HttpNotFound();
            }
            ViewBag.group = getGroup(AppKey.product, "0", Guid.Empty);

            var Group_item = db.GroupItems.Where(d => d.itemId == item.id && d.flag > 0).FirstOrDefault();
            if (Group_item != null)
                ViewBag.Group_id = Group_item.groupId.Value;
            else
                ViewBag.Group_id = null;

            return View(item);
        }

        // POST: Product/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,title,desc,price_old,price,flag")] Models.Item item_tmp, FormCollection collection)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    //Item
                    var item = db.Items.Find(item_tmp.id);
                    item.title = item_tmp.title;
                    item.priceOld = item_tmp.priceOld;
                    item.price = item_tmp.price;
                    item.desc = item_tmp.desc;
                    item.quantity = item.quantity + long.Parse(collection["quantity_add"]);
                    item.quantityTotal = item.quantityTotal + long.Parse(collection["quantity_add"]);
                    //Images
                    var images = TM.IO.UploadImages(Request.Files, DirUpload.imagesProduct);
                    var tmp = images.UploadFileString();
                    if (tmp != null)
                        item.images = tmp;
                    item.updatedBy = Common.Auth.getUser().id.ToString();
                    item.updatedAt = DateTime.Now;
                    db.Entry(item).State = EntityState.Modified;

                    //Group Item
                    var group_item = db.GroupItems.Where(d => d.itemId == item.id && d.flag > 0).FirstOrDefault();

                    if (group_item != null)
                    {
                        group_item.groupId = Guid.Parse(collection["group_id"]);
                        db.Entry(item).State = EntityState.Modified;
                    }
                    else
                    {
                        group_item = new Models.GroupItem();
                        group_item.id = Guid.NewGuid();
                        group_item.appKey = item.appKey;
                        group_item.groupId = Guid.Parse(collection["group_id"]);
                        group_item.itemId = item.id;
                        group_item.flag = 1;
                        group_item.updatedBy = Common.Auth.getUser().id.ToString();
                        group_item.updatedAt = DateTime.Now;
                        db.GroupItems.Add(group_item);
                    }
                    db.SaveChanges();
                    this.success(TM.Common.Language.msgUpdateSucsess);
                    return RedirectToAction("Index");
                }
                else
                    this.danger(TM.Common.Language.msgError);
            }
            catch (Exception ex)
            {
                this.danger(TM.Common.Language.msgUpdateError);
            }
            return RedirectToAction("Edit");
        }

        [AllowAnonymous]
        public JsonResult check_exist_CodeKey(string code_key)
        {
            return Json(!db.Items.Any(d => d.codeKey.ToUpper() == code_key.ToUpper() && d.appKey == AppKey.product), JsonRequestBehavior.AllowGet);
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
                    var rs = db.Items.Find(tmp);
                    rs.flag = flag = rs.flag == 1 ? 0 : 1;
                }
                db.SaveChanges();
                return Json(new { success = (flag == 0 ? TM.Common.Language.msgLockSucsess : TM.Common.Language.msgRecoverSucsess) }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception) { return Json(new { danger = TM.Common.Language.msgError }, JsonRequestBehavior.AllowGet); }
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public JsonResult Delete(string[] uid)
        {
            try
            {
                //string[] id = uid.Split(',');
                foreach (var item in uid)
                {
                    var tmp = Guid.Parse(item);
                    var rs = db.Items.Find(tmp);
                    db.Items.Remove(rs);
                }
                db.SaveChanges();
                return Json(new { success = TM.Common.Language.msgDeleteSucsess }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception) { return Json(new { danger = TM.Common.Language.msgError }, JsonRequestBehavior.AllowGet); }
        }
        // GET: Product/Delete/5
        //public ActionResult Delete(Guid? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Models.item item = db.items.Find(id);
        //    if (item == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(item);
        //}

        //// POST: Product/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(Guid id)
        //{
        //    Models.item item = db.items.Find(id);
        //    db.items.Remove(item);
        //    db.SaveChanges();
        //    return RedirectToAction("Index");
        //}
    }
}
