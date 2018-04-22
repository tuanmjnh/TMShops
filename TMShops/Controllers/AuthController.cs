using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TM;

namespace TMShops.Controllers
{
    public class AuthController : BaseController
    {
        // GET: Auth
        public ActionResult Index()
        {
            if (Connection() == null)
                this.danger("Không thể kết nối đến CSDL!");
            if (Common.Auth.isAuth())
                return Redirect(TM.Url.BaseUrl);
            return View();
        }
        [HttpPost]
        public ActionResult Index(FormCollection collection)
        {
            try
            {
                string username = collection["username"].ToString();
                string password = collection["password"].ToString();
                //AuthStatic
                if (Common.AuthStatic.isAuthStatic(username, password))
                {
                    Common.Auth.setAuth(Common.AuthStatic.userStatic());
                    return Redirect(TM.Url.RedirectContinue());
                }

                //AuthDB
                var tmp = db.Users.Where(u => u.username == username);
                if (tmp.FirstOrDefault() == null)
                {
                    this.danger("Sai tên tài khoản hoặc mật khẩu!");
                    return View();
                }

                password = TM.Encrypt.CryptoMD5TM(password + tmp.FirstOrDefault().salt);
                tmp = db.Users.Where(u => u.username == username && u.password == password);
                if (tmp.SingleOrDefault() == null)
                {
                    this.danger("Sai tên tài khoản hoặc mật khẩu!");
                    return View();
                }

                var rs = tmp.Where(u => u.flag > 0).FirstOrDefault();
                if (rs == null)
                {
                    this.danger("Tài khoản đã bị khóa. Vui lòng liên hệ admin!");
                    return View();
                }
                Common.Auth.setAuth(rs);
                return Redirect(TM.Url.RedirectContinue());
                //return Redirect(TM.Url.BaseUrl);
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
            {
                Exception raise = dbEx;
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        string message = string.Format("{0}:{1}",
                            validationErrors.Entry.Entity.ToString(),
                            validationError.ErrorMessage);
                        // raise a new exception nesting  
                        // the current instance as InnerException  
                        raise = new InvalidOperationException(message, raise);
                    }
                }
                throw raise;
            }
            return View();
        }
        [Filters.AuthFilters]
        [HttpGet]
        public ActionResult logout()
        {
            Common.Auth.logout();
            return Redirect(System.Web.HttpContext.Current.Request.UrlReferrer.ToString());//System.Web.HttpContext.Current.Request.UrlReferrer
        }
        [Filters.AuthFilters]
        public ActionResult ChangePassword(Guid id)
        {

            return View(db.Users.SingleOrDefault(u => u.id == id));
        }
        [Filters.AuthFilters]
        [HttpPost]
        public ActionResult ChangePassword(Guid id, string password)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var rs = db.Users.Find(id);
                    rs.password = TM.Encrypt.CryptoMD5TM(password + rs.salt);
                    db.SaveChanges();
                    ModelState.Clear();
                    this.success("Cập nhật mật khẩu thành công");
                    return RedirectToAction("Index");
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
            {
                Exception raise = dbEx;
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        string message = string.Format("{0}:{1}",
                            validationErrors.Entry.Entity.ToString(),
                            validationError.ErrorMessage);
                        // raise a new exception nesting  
                        // the current instance as InnerException  
                        raise = new InvalidOperationException(message, raise);
                    }
                }
                throw raise;
            }
            return View();
        }
    }
}