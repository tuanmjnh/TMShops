using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Dapper;

namespace TMShops.Controllers
{
    public class BaseController : Controller
    {
        public Models.TMShopsContext db = new Models.TMShopsContext();
        public static System.Data.SqlClient.SqlConnection Connection()
        {
            try
            {
                return TM.SQLDB.Connection("MainContext");
            }
            catch (Exception) { return null; }
        }
        public static void ConnectionString()
        {
            TM.SQLDB.ConstantConnectionString = "MainContext";
        }
        //public static List<Models.setting> AllSetting { get; set; }
        public static List<dynamic> AllSetting { get; set; }
        bool setting = setSetting();
        public static List<dynamic> Settings(string module_key)
        {
            try
            {
                //return AllSetting.Where(s => s.module_key.Equals("module_key")).ToList();
                return Connection().Query("SELECT * FROM settings WHERE module_key=@module_key",
                    new { module_key = module_key }).ToList();
            }
            catch (Exception) { return null; }
        }
        public static List<dynamic> Settings(string module_key, string sub_key)
        {
            try
            {
                //return AllSetting.Where(s => s.module_key.Equals(module_key) && s.sub_key.Equals(sub_key)).ToList();
                return Connection().Query("SELECT * FROM settings WHERE module_key=@module_key AND sub_key=@sub_key",
                new { module_key = module_key, sub_key = sub_key }).ToList();
            }
            catch (Exception) { return null; }
        }
        public static dynamic Setting(string module_key, string sub_key, string value)
        {
            try
            {
                //return AllSetting.Where(s => s.module_key.Equals(module_key) && s.sub_key.Equals(sub_key) && s.value.Equals(value)).FirstOrDefault();
                return Connection().Query("SELECT * FROM settings WHERE module_key=@module_key AND sub_key=@sub_key AND value=@value",
                new { module_key = module_key, sub_key = sub_key, value = value }).First();
            }
            catch (Exception) { return null; }
        }
        public static string Value(string module_key, string sub_key)
        {
            try
            {
                return Settings(module_key, sub_key).First().value;
            }
            catch (Exception) { return null; }
        }
        public static string SubValue(string module_key, string sub_key, string value)
        {
            try
            {
                return Setting(module_key, sub_key, value).sub_value;
            }
            catch (Exception) { return null; }
        }
        public static bool setSetting()
        {
            try
            {
                if (AllSetting == null) LoadSetting();
                return true;
            }
            catch (Exception) { return false; }
        }
        public static void LoadSetting()
        {
            try
            {
                AllSetting = Connection().Query("SELECT * FROM settings").ToList();
            }
            catch (Exception) { }
            //using (var dbs = new Models.PortalContext())
            //{
            //    AllSetting = dbs.settings.ToList();
            //}
        }
        //public static string Root = SettingKey("host").value;

        //public static List<Models.setting> Setting()
        //{
        //    using (var dbs = new Models.PortalContext())
        //    {
        //        return dbs.settings.ToList();
        //    }
        //}
        //public static Models.setting SettingKey(string app_key)
        //{
        //    return Setting().Where(s => s.app_key == app_key).FirstOrDefault();
        //}
        public static string getMonthYear(string str)
        {
            try
            {
                return str[0].ToString() + str[1].ToString() + "/20" + str[2].ToString() + str[3].ToString();
            }
            catch (Exception) { return str; }
        }
        public static string getMonthDayYear(string str)
        {
            try
            {
                return str[0].ToString() + str[1].ToString() + "/1/20" + str[2].ToString() + str[3].ToString();
            }
            catch (Exception) { return str; }
        }
        public static string getYearMonth(string str)
        {
            try
            {
                return str[4].ToString() + str[5].ToString() + "/" + str[0].ToString() + str[1].ToString() + str[2].ToString() + str[3].ToString();
            }
            catch (Exception) { return str; }
        }
        public static List<Models.Group> getGroup(string AppKey, string parent, Guid id)
        {
            try
            {
                var list = new List<Models.Group>();
                var db = new Models.TMShopsContext();
                var rs = new List<Models.Group>();

                if (id == Guid.Empty)
                    rs = db.Groups.Where(d => d.appKey == AppKey && d.parentId == parent && d.flag > 0).ToList();
                else
                    rs = db.Groups.Where(d => d.id != id && d.appKey == AppKey && d.parentId == parent && d.flag > 0).ToList();

                foreach (var item in rs)
                {
                    list.Add(item);
                    list.AddRange(getGroup(AppKey, item.id.ToString(), id));
                }
                return list;
            }
            catch (Exception) { throw; }
        }
        public static string GetUser(string id)
        {
            try
            {
                var db = new Models.TMShopsContext();
                var rs = db.Users.Find(Guid.Parse(id));
                if (rs != null)
                    if (!String.IsNullOrEmpty(rs.fullName))
                        return rs.fullName;
                    else return rs.username;
                else return TM.Common.Language.emptyvl;
            }
            catch (Exception) { return TM.Common.Language.emptyvl; }

        }
        public static string GetCustomerName(string id)
        {
            try
            {
                var db = new Models.TMShopsContext();
                var rs = db.Customers.Find(Guid.Parse(id));
                if (rs != null)
                    return rs.fullName;
                else
                    return TM.Common.Language.emptyvl;
            }
            catch (Exception) { return TM.Common.Language.emptyvl; }
        }
        protected override void Dispose(bool disposing)
        {
            //if (RoleManager != null) RoleManager.Dispose();
            //if (UserManager != null) UserManager.Dispose();
            if (db != null) db.Dispose();
            base.Dispose(disposing);
        }
    }
    public class DirUpload
    {
        public const string images = "Uploads/Images/";
        public const string imagesProduct = images + "Product/";
        public const string imagesCustomer = images + "Customer/";
    }
    public class AppKey
    {
        public const string customer = "customer";
        public const string product = "product";
        public const string personInfo = "personInfo";
        public const string cardManager = "cardManager";
        public const string cardPerson = "cardPerson";

        public const string TypePayDay = "day";
        public const string TypePayMonth = "month";
        public const string TypePayMonths = "months";
        public const string TypePayYear = "year";
        public static Dictionary<string, string> TypePayTime()
        {
            var rs = new Dictionary<string, string>();
            rs.Add(TypePayDay, "Ngày");
            rs.Add(TypePayMonth, "Tháng");
            rs.Add(TypePayMonths, "Quý");
            rs.Add(TypePayYear, "Năm");
            return rs;
        }
    }
}