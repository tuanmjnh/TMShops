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
    public class CategoryController : BaseController
    {
        // GET: Category
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

                var rs = getGroup(AppKey.product, "0", Guid.Empty).AsQueryable();

                if (!String.IsNullOrEmpty(searchString))
                    rs = rs.Where(d =>
                    d.title.Contains(searchString));

                if (flag == 0) rs = rs.Where(d => d.flag == 0);
                else rs = rs.Where(d => d.flag == 1);

                //switch (order)
                //{
                //case "value_desc":
                //    rs = rs.OrderByDescending(d => d.title);
                //    break;
                //default:
                //    rs = rs.OrderBy(d => d.title);
                //    break;
                //case "id_asc":
                //    rs = rs.OrderByDescending(d => d.id);
                //    break;
                //default:
                //    rs = rs.OrderBy(d => d.id);
                //    break;
                //}

                //Export to any
                if (!String.IsNullOrEmpty(export))
                {
                    TM.Exports.ExportExcel(TM.Helper.Data.LinqToDataTable(rs.ToList()), "Danh sách danh mục");
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

        // GET: Category/Details/5
        public ActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var group = db.Groups.Find(id);
            if (group == null)
            {
                return HttpNotFound();
            }
            return View(group);
        }

        // GET: Category/Create
        public ActionResult Create()
        {
            ViewBag.group = getGroup(AppKey.product, "0", Guid.Empty);
            return View();
        }

        // POST: Category/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "title,parent_id,desc,flag")] Models.Group group)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    //ViewBag.group
                    group.id = Guid.NewGuid();

                    //Parents_ID
                    group.parentsId = ",";
                    group.levels = 0;
                    if (group.parentId != "0")
                    {
                        var parent_id = Guid.Parse(group.parentId);
                        var tmp = db.Groups.Where(d => d.id == parent_id && d.flag > 0).FirstOrDefault();
                        group.parentsId = tmp.parentsId + parent_id + ",";
                        group.levels = tmp.levels + 1;
                    }

                    group.appKey = AppKey.product;
                    group.createdBy = Common.Auth.getUser().id.ToString();
                    group.createdAt = DateTime.Now;

                    db.Groups.Add(group);
                    db.SaveChanges();
                    this.success(TM.Common.Language.msgCreateSucsess);
                }
                else this.danger(TM.Common.Language.msgError);
            }
            catch (Exception ex)
            {
                this.danger(TM.Common.Language.msgCreateError);
            }
            return RedirectToAction("Create");
        }

        // GET: Category/Edit/5
        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var group = db.Groups.Find(id);
            if (group == null)
            {
                return HttpNotFound();
            }
            ViewBag.group = getGroup(AppKey.product, "0", id.Value);
            return View(group);
        }

        // POST: Category/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,title,parent_id,desc,flag")] Models.Group group_tmp)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var group = db.Groups.Find(group_tmp.id);

                    //Parents_ID
                    if (group_tmp.parentId != "0")
                    {
                        var parent_id = Guid.Parse(group_tmp.parentId);
                        var tmp = db.Groups.Where(d => d.id == parent_id && d.flag > 0).FirstOrDefault();
                        group.parentsId = tmp.parentsId + parent_id + ",";
                        group.levels = tmp.levels + 1;
                    }

                    group.parentId = group_tmp.parentId;
                    group.title = group_tmp.title;
                    group.desc = group_tmp.desc;
                    group.flag = group_tmp.flag;
                    group.updatedBy = Common.Auth.getUser().id.ToString();
                    group.updatedAt = DateTime.Now;

                    db.Entry(group).State = EntityState.Modified;
                    db.SaveChanges();
                    this.success(TM.Common.Language.msgCreateSucsess);
                    return RedirectToAction("Index");
                }
                else this.danger(TM.Common.Language.msgError);
            }
            catch (Exception ex)
            {
                this.danger(TM.Common.Language.msgCreateError);
            }
            return RedirectToAction("Edit");
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
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public JsonResult Delete(string uid)
        {
            try
            {
                string[] id = uid.Split(',');
                foreach (var item in id)
                {
                    var tmp = Guid.Parse(item);
                    var rs = db.Groups.Find(tmp);
                    db.Groups.Remove(rs);
                }
                db.SaveChanges();
                return Json(new { success = TM.Common.Language.msgDeleteSucsess }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception) { return Json(new { danger = TM.Common.Language.msgError }, JsonRequestBehavior.AllowGet); }
        }
        // GET: Category/Delete/5
        //public ActionResult Delete(Guid? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Models.group group = db.groups.Find(id);
        //    if (group == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(group);
        //}

        //// POST: Category/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(Guid id)
        //{
        //    Models.group group = db.groups.Find(id);
        //    db.groups.Remove(group);
        //    db.SaveChanges();
        //    return RedirectToAction("Index");
        //}

    }
}
