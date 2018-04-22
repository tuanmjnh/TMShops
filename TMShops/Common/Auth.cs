using System;
namespace TMShops.Common
{
    public class Auth
    {
        public const string AuthSession = "AuthApp";
        public static void setAuth(Models.User account)
        {
            try { System.Web.HttpContext.Current.Session[AuthSession] = account; }
            catch { return; }
        }
        public static Models.User getUser()
        {
            try
            {
                var auth = (Models.User)System.Web.HttpContext.Current.Session[AuthSession];
                return auth;
            }
            catch { return null; }
        }
        public static bool isAuth()
        {
            if (System.Web.HttpContext.Current.Session[AuthSession] != null)
                return true;
            return false;
        }
        public static bool inRoles(string[] r)
        {
            if (r.Contains(getUser().roles))
                return true;
            return false;
        }
        public static void logout()
        {
            try { System.Web.HttpContext.Current.Session[AuthSession] = null; }
            catch { return; }
        }
    }
    public class AuthStatic
    {
        public static Models.User userStatic()
        {
            var user = new Models.User();
            user.id = Guid.NewGuid();
            user.username = "tuanmjnh";
            user.password = "aa2de065c899d53d7031b0975c56062f";//"Tuanmjnh@tm"; //"fc44d0279133a2f46178134ce9bf2167";//tuanmjnh@123
            user.salt = "1c114c58-69d9-41e6-bd3e-363906e04e50";
            user.fullName = "SuperAdmin";
            user.mobile = "0123456789";
            user.email = "SuperAdmin@SuperAdmin.com";
            user.address = "SuperAdmin";
            user.roles = roles.superadmin;
            user.orders = 1;
            user.createdBy = user.id.ToString();
            user.createdAt = DateTime.Now;
            user.updatedBy = user.id.ToString();
            user.updatedAt = DateTime.Now;
            user.lastLogin = DateTime.Now;
            user.staffId = Guid.NewGuid();
            user.flag = 1;
            user.extras = "";
            return user;
        }
        public static bool isAuthStatic(string username, string password)
        {
            if (userStatic().username == username && userStatic().password == TM.Encrypt.CryptoMD5TM(password + userStatic().salt))
                return true;
            return false;
        }
    }
    public class roles
    {
        public const string superadmin = "187eb627-0a7b-44a8-83c4-ceb4829709a3";
        public const string admin = "ee82e7f1-592c-4f5c-95fa-7cad30b14a2d";
        public const string mod = "238391cd-990d-4f3b-8d0c-0300416f9263";
        public const string director = "121ab8e5-1ad2-4b78-8ff2-4d953c9b71a8";
        public const string leader = "d0443498-09c4-4267-a7c9-2a20dde8e925";
        public const string staff = "dc67601d-ad74-4813-8293-8d4a07db1d31";
    }
}
