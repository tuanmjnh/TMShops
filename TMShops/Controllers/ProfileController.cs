using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TM;

namespace TMShops.Controllers
{
    [Filters.AuthFilters()]
    public class ProfileController : BaseController
    {
        // GET: Profile
        public ActionResult Index()
        {
            //ViewBag.address = db.users.SingleOrDefault(u => u.id == id).address;
            return View(db.Users.SingleOrDefault(u => u.id == Common.Auth.getUser().id));
        }
        [HttpPost]
        public ActionResult Index(FormCollection collection)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var rs = db.Users.SingleOrDefault(u => u.id == Common.Auth.getUser().id);
                    rs.address = collection["address"].ToString(); ;
                    rs.fullName = collection["full_name"].ToString();
                    db.SaveChanges();
                    ModelState.Clear();
                    this.success("Cập nhật thông tin thành công");
                    return Redirect("Profile");
                }
            }
            catch { }
            this.danger("Cập nhật thông tin thất bại");
            return View();
        }
        public ActionResult ChangePassword()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ChangePassword(FormCollection collection)
        {

            try
            {
                if (ModelState.IsValid)
                {
                    string password = TM.Encrypt.CryptoMD5TM(collection["oldpassword"].ToString() + Common.Auth.getUser().salt);
                    var rs = db.Users.SingleOrDefault(u => u.id == Common.Auth.getUser().id && u.password == password);

                    if (rs != null)
                    {
                        password = TM.Encrypt.CryptoMD5TM(collection["password"].ToString() + Common.Auth.getUser().salt);
                        rs.password = password;
                        db.SaveChanges();
                        ModelState.Clear();
                        this.success("Thay đổi mật khẩu thành công");
                    }
                    else
                    {
                        ModelState.Clear();
                        this.danger("Mật khẩu hiện tại không đúng");
                    }

                }
            }
            catch (Exception) { this.danger("Cập nhật thông tin thất bại"); }
            return RedirectToAction("ChangePassword");
        }
        public ActionResult Setting()
        {
            //if (id == null)
            //    return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            //if (data == null)
            //    return HttpNotFound();
            var module_key = Common.Auth.getUser().id.ToString();
            var setting = db.Settings.Where(s => s.moduleKey.Equals(module_key) && s.subKey.Equals(TM.Common.Config.sub_key.form_use_label));
            if (setting.Count() > 0)
                return View(setting.FirstOrDefault());
            else
                return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Setting(Guid? id, FormCollection collection)
        {

            try
            {
                if (id != null)
                {
                    var settings = db.Settings.Find(id);
                    settings.value = collection[TM.Common.Config.sub_key.form_use_label];
                }
                else
                {
                    var settings = new Models.Setting();
                    settings.id = Guid.NewGuid();
                    settings.moduleKey = Common.Auth.getUser().id.ToString();
                    settings.subKey = TM.Common.Config.sub_key.form_use_label;
                    settings.value = collection[TM.Common.Config.sub_key.form_use_label];
                    db.Settings.Add(settings);
                }
                db.SaveChanges();
                this.success("Cập nhật thông tin thành công");
            }
            catch (Exception ex) { this.danger("Cập nhật thông tin thất bại"); }
            return RedirectToAction("Setting");
        }
    }
}