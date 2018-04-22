using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using PagedList;
using TM;
using Dapper;

namespace TMShops.Controllers
{
    [Filters.AuthFilters()]
    public class CustomerController : BaseController
    {
        // GET: Customer
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

                var rs = from d in db.Customers select d;

                if (!String.IsNullOrEmpty(searchString))
                    rs = rs.Where(d =>
                    d.fullName.Contains(searchString) ||
                    d.facebook.Contains(searchString) ||
                    d.cardId.Contains(searchString) ||
                    d.mobile.Contains(searchString));

                if (flag == 0) rs = rs.Where(d => d.flag == 0);
                else rs = rs.Where(d => d.flag == 1);

                switch (order)
                {
                    case "card_desc":
                        rs = rs.OrderByDescending(d => d.cardId);
                        break;
                    case "card_asc":
                        rs = rs.OrderBy(d => d.cardId);
                        break;
                    case "dateofbirth_desc":
                        rs = rs.OrderByDescending(d => d.dateOfBirth);
                        break;
                    case "dateofbirth_asc":
                        rs = rs.OrderBy(d => d.dateOfBirth);
                        break;
                    case "sdt_desc":
                        rs = rs.OrderByDescending(d => d.mobile);
                        break;
                    case "sdt_asc":
                        rs = rs.OrderBy(d => d.mobile);
                        break;
                    case "fb_desc":
                        rs = rs.OrderByDescending(d => d.facebook);
                        break;
                    case "fb_asc":
                        rs = rs.OrderBy(d => d.facebook);
                        break;
                    case "fullName_desc":
                        rs = rs.OrderByDescending(d => d.fullName);
                        break;
                    default:
                        rs = rs.OrderBy(d => d.fullName);
                        break;
                        //case "id_asc":
                        //    rs = rs.OrderByDescending(d => d.id);
                        //    break;
                        //default:
                        //    rs = rs.OrderBy(d => d.id);
                        //    break;
                }

                //Export to any
                //if (!String.IsNullOrEmpty(export))
                //{
                //    Export(getDataTable(rs), export);
                //    return RedirectToAction("Index");
                //}

                ViewBag.TotalRecords = rs.Count();
                int pageSize = 15;
                int pageNumber = (page ?? 1);

                return View(rs.ToPagedList(pageNumber, pageSize));
            }
            catch (Exception ex)
            {
                this.danger(ex.Message);
            }
            return View();
        }

        // GET: Customer/Details/5
        public ActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var customer = db.Customers.Find(id);
            if (customer == null)
            {
                return HttpNotFound();
            }
            return View(customer);
        }

        // GET: Customer/Create
        public ActionResult Create()
        {
            ViewBag.PersonInfo = getPersonInfo();
            ViewBag.Card = db.Items.Where(d => d.appKey == AppKey.cardManager && d.flag > 0).OrderBy(d => d.price).ToList();
            return View();
        }

        // POST: Customer/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "full_name,date_of_birth,mobile,address,facebook,email,card_id,desc,flag")] Models.Customer customer, FormCollection collection)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    customer.id = Guid.NewGuid();
                    if (customer.dateOfBirth != null)
                        customer.dateOfBirth = customer.dateOfBirth.ToString();
                    //Images
                    var images = TM.IO.UploadImages(Request.Files, DirUpload.imagesCustomer, 3);
                    customer.images = images.UploadFileString();
                    customer.createdBy = Common.Auth.getUser().id.ToString();
                    customer.createdAt = DateTime.Now;
                    db.Customers.Add(customer);

                    var PersonInfo = db.SubItems.Where(d => d.appKey == AppKey.personInfo && d.flag > 0).OrderBy(d => d.value).ToList();
                    foreach (var item in PersonInfo)
                    {
                        var sub_item = new Models.SubItem();
                        var sub_value = collection[item.id.ToString() + "[]"].Split(',');

                        sub_item.id = Guid.NewGuid();
                        sub_item.appKey = AppKey.customer;
                        sub_item.idKey = customer.id;
                        sub_item.mainKey = item.id.ToString();
                        sub_item.value = item.value;
                        if (sub_value.Length > 0)
                            sub_item.subValue = sub_value[0];
                        if (sub_value.Length > 1)
                            sub_item.extras = sub_value[1];
                        sub_item.createdBy = Common.Auth.getUser().id.ToString();
                        sub_item.createdAt = DateTime.Now;
                        sub_item.flag = 1;
                        db.SubItems.Add(sub_item);
                    }

                    //Card
                    var cardType = db.Items.Find(Guid.Parse(collection["code_key"]));
                    var card = new Models.Item();
                    card.id = Guid.NewGuid();
                    card.codeKey = collection["code_key"];
                    card.quantity = long.Parse(collection["quantity"]);
                    card.title = collection["totalPay"];
                    card.desc = collection["desc_card"];
                    card.price = decimal.Parse(collection["price"]);
                    card.priceOld = decimal.Parse(collection["price_old"]);
                    card.flag = 1;
                    card = AddCardExtend(card, cardType, customer.id.ToString(), collection["startDate"], collection["quantity"]);
                    db.Items.Add(card);

                    //card.id = Guid.NewGuid();
                    //card.app_key = AppKey.cardPerson;
                    //card.id_key = customer.id + "," + card.id_key;
                    //card.code_key = customer.card_id;
                    //card.title = collection["startDate"] + "," + collection["endDate"];
                    //card.desc = collection["totalPay"];
                    //card.price_old = decimal.Parse(collection["totalNotPay"]);
                    //card.price = decimal.Parse(collection["totalPaid"]);
                    //card.flag = customer.flag;
                    //card.created_by = VinaphoneCommon.Auth.id().ToString();
                    //card.created_at = DateTime.Now;
                    //db.items.Add(card);


                    db.SaveChanges();
                    this.success(TM.Common.Language.msgCreateSucsess);
                    return RedirectToAction("Create");
                }
                else
                    this.danger(TM.Common.Language.msgError);
            }
            catch (Exception ex)
            {
                this.danger(TM.Common.Language.msgCreateError);
            }
            ViewBag.PersonInfo = getPersonInfo();
            return View(customer);
        }

        // GET: Customer/Edit/5
        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var customer = db.Customers.Find(id);
            if (customer == null)
            {
                return HttpNotFound();
            }
            //
            ViewBag.PersonInfo = getPersonInfo();
            ViewBag.Card = db.Items.Where(d => d.appKey == AppKey.cardManager && d.flag > 0).OrderBy(d => d.price).ToList();

            //
            //ViewBag.CardExtendedList = Connection().Query("select top 15 *,(select title from item where id=i.code_key) as card_name from item i where app_key=@app_key AND id_key=@id_key order by created_at desc",
            //    new { app_key = AppKey.cardPerson, id_key = id.ToString() }).ToList();

            dynamic CardExtendedList = (from d in db.Items
                                        where d.appKey == AppKey.cardPerson && d.idKey == id.ToString()
                                        orderby d.createdAt descending
                                        select new
                                        {
                                            id = d.id,
                                            appKey = d.appKey,
                                            idkey = d.idKey,
                                            codeKey = d.codeKey,
                                            title = d.title,
                                            desc = d.desc,
                                            quantity = d.quantity,
                                            quantityTotal = d.quantityTotal,
                                            priceOld = d.priceOld,
                                            price = d.price,
                                            startedat = d.startedAt,
                                            endedat = d.endedAt,
                                            createdby = d.createdBy,
                                            createdat = d.createdAt,
                                            updatedby = d.updatedBy,
                                            updatedat = d.updatedAt,
                                            flag = d.flag,
                                            extras = d.extras,
                                            cardName = db.Items.Where(di => di.id.ToString() == d.codeKey).FirstOrDefault().title
                                        }).AsEnumerable().Select(d => d.ToExpando());
            ViewBag.CardExtendedList = CardExtendedList;
            //
            var CardExtended = GetCardExtended(id.ToString(), AppKey.TypePayMonth);
            if (CardExtended != null)
            {
                ViewBag.CardExtended = "<div class=\"alert alert-success\" role=\"alert\">Thẻ <b>" + customer.cardId + "</b> còn <b>" + (CardExtended.endedAt.Value - DateTime.Now).Days +
                    "</b> ngày (Ngày bắt đầu <b>" + CardExtended.startedAt.Value.ToString("dd/MM/yyyy") + "</b> - Ngày kết thúc <b>" + CardExtended.endedAt.Value.ToString("dd/MM/yyyy") + "</b>)</div>";
            }
            else
            {
                CardExtended = GetCardExtended(id.ToString(), AppKey.TypePayDay);
                if (CardExtended != null)
                    ViewBag.CardExtended = "<div class=\"alert alert-success\" role=\"alert\">Thẻ <b>" + customer.cardId + "</b> còn <b>" + (CardExtended.quantity - CardExtended.quantityTotal) + "</b> Lần</div>";
                else
                    ViewBag.CardExtended = "<div class=\"alert alert-danger\" role=\"alert\">Thẻ <b>" + customer.cardId + "</b> đã hết hạn!</div>";
            }

            return View(customer);
        }

        // POST: Customer/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,full_name,date_of_birth,mobile,address,facebook,email,desc,flag")] Models.Customer customer_tmp, FormCollection collection)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var customer = db.Customers.Find(customer_tmp.id);
                    if (customer_tmp.dateOfBirth != null)
                        customer.dateOfBirth = customer_tmp.dateOfBirth.ToString();
                    customer.fullName = customer_tmp.fullName;
                    customer.mobile = customer_tmp.mobile;
                    customer.address = customer_tmp.address;
                    customer.facebook = customer_tmp.facebook;
                    customer.email = customer_tmp.email;
                    //Images
                    var images = TM.IO.UploadImages(Request.Files, DirUpload.imagesCustomer, 3);
                    var tmp = images.UploadFileString();
                    if (tmp != null)
                        customer.images = tmp;

                    customer.desc = customer_tmp.desc;
                    customer.flag = customer_tmp.flag;
                    db.Entry(customer).State = EntityState.Modified;

                    var customerPersonInfo = db.SubItems.Where(d => d.idKey == customer_tmp.id).ToList();
                    foreach (var item in customerPersonInfo)
                        db.SubItems.Remove(item);

                    var PersonInfo = db.SubItems.Where(d => d.appKey == AppKey.personInfo && d.flag > 0).OrderBy(d => d.value).ToList();
                    foreach (var item in PersonInfo)
                    {
                        var sub_item = new Models.SubItem();
                        var sub_value = collection[item.id.ToString() + "[]"].Split(',');

                        sub_item.id = Guid.NewGuid();
                        sub_item.appKey = AppKey.customer;
                        sub_item.idKey = customer.id;
                        sub_item.mainKey = item.id.ToString();
                        sub_item.value = item.value;
                        if (sub_value.Length > 0)
                            sub_item.subValue = sub_value[0];
                        if (sub_value.Length > 1)
                            sub_item.extras = sub_value[1];
                        sub_item.createdBy = Common.Auth.getUser().id.ToString();
                        sub_item.createdAt = DateTime.Now;
                        sub_item.flag = 1;
                        db.SubItems.Add(sub_item);
                    }

                    db.SaveChanges();
                    this.success(TM.Common.Language.msgUpdateSucsess);
                    return RedirectToAction("Edit");
                }
                else
                    this.danger(TM.Common.Language.msgError);
            }
            catch (Exception ex)
            {
                this.danger(TM.Common.Language.msgUpdateError);
            }
            ViewBag.PersonInfo = getPersonInfo();

            return View(customer_tmp);
        }
        // POST: Customer/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CardExtend([Bind(Include = "id_key,code_key,title,desc,quantity,price_old,price,started_at,ended_at,flag")] Models.Item item, FormCollection collection)
        {
            var customer_id = collection["id"];
            var Url = "Edit/" + customer_id;
            try
            {
                if (GetCardExtended(customer_id, AppKey.TypePayMonth) != null)
                {
                    this.danger("Thẻ chưa hết hạn!");
                    return RedirectToAction(Url);

                }
                else
                {
                    if (GetCardExtended(customer_id, AppKey.TypePayDay) != null)
                    {
                        this.danger("Thẻ chưa hết hạn!");
                        return RedirectToAction(Url);
                    }
                }
                if (ModelState.IsValid)
                {
                    var cardType = db.Items.Find(Guid.Parse(item.codeKey));
                    if (cardType == null)
                    {
                        this.danger("Không tìm thấy loại thẻ. Vui lòng thử lại");
                        return RedirectToAction(Url);
                    }
                    item = AddCardExtend(item, cardType, customer_id, collection["startDate"], item.quantity.ToString());
                    db.Items.Add(item);
                    db.SaveChanges();
                    this.success("Gia hạn thẻ thành công!");
                    return RedirectToAction(Url);
                }
                else
                    this.danger(TM.Common.Language.msgError);
            }
            catch (Exception ex)
            {
                this.danger(TM.Common.Language.msgUpdateError);
            }
            return RedirectToAction(Url);
        }
        private Models.Item AddCardExtend(Models.Item item, Models.Item cardType, string customer_id, string startDate, string quantity)
        {
            try
            {
                Connection().Execute("update item set flag=0 where id_key=@id_key", new { id_key = customer_id });
                var started_at = DateTime.ParseExact(startDate, "dd/MM/yyyy", null);
                var ended_at = new DateTime();

                if (cardType.codeKey == AppKey.TypePayDay)
                {
                    ended_at = started_at.AddDays(double.Parse(quantity));
                    item.extras = AppKey.TypePayDay;
                }
                else if (cardType.codeKey == AppKey.TypePayMonth)
                {
                    ended_at = started_at.AddMonths(Int32.Parse(quantity));
                    item.extras = AppKey.TypePayMonth;
                }
                else if (cardType.codeKey == AppKey.TypePayMonths)
                {
                    ended_at = started_at.AddMonths(Int32.Parse(quantity) * 3);
                    item.extras = AppKey.TypePayMonths;
                }
                else
                {
                    ended_at = started_at.AddYears(Int32.Parse(quantity));
                    item.extras = AppKey.TypePayYear;
                }

                item.id = Guid.NewGuid();
                item.appKey = AppKey.cardPerson;
                item.idKey = customer_id;
                item.quantityTotal = 0;
                item.startedAt = started_at;
                item.endedAt = ended_at.AddSeconds(86399);
                item.flag = 1;
                item.createdBy = Common.Auth.getUser().id.ToString();
                item.createdAt = DateTime.Now;
            }
            catch (Exception) { throw; }
            return item;
        }
        public Models.Item GetCardExtended(string customer_id, string type)
        {
            try
            {
                if (type == AppKey.TypePayDay)
                    return db.Items.Where(d => d.idKey == customer_id && d.quantityTotal < d.quantity && d.extras == AppKey.TypePayDay).FirstOrDefault();
                else
                    return db.Items.Where(d => d.idKey == customer_id && d.endedAt >= DateTime.Now && d.extras != AppKey.TypePayDay).FirstOrDefault();
            }
            catch (Exception)
            {
                throw;
            }
        }
        // GET: Customer/Delete/5
        //public ActionResult Delete(Guid? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Models.customer customer = db.customers.Find(id);
        //    if (customer == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(customer);
        //}

        //// POST: Customer/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(Guid id)
        //{
        //    Models.customer customer = db.customers.Find(id);
        //    db.customers.Remove(customer);
        //    db.SaveChanges();
        //    return RedirectToAction("Index");
        //}
        [AllowAnonymous]
        public JsonResult check_exist_code_key(string code_key)
        {
            return Json(!db.Items.Any(d => d.codeKey.ToUpper() == code_key.ToUpper() && d.appKey == AppKey.cardPerson), JsonRequestBehavior.AllowGet);
        }
        [AllowAnonymous]
        public JsonResult check_exist_card_id(string card_id)
        {
            return Json(!db.Customers.Any(d => d.cardId.ToUpper() == card_id.ToUpper()), JsonRequestBehavior.AllowGet);
        }
        private List<Models.SubItem> getPersonInfo()
        {
            try
            {
                return db.SubItems.Where(d => d.appKey == AppKey.personInfo && d.flag > 0).OrderBy(d => d.value).ToList();
            }
            catch (Exception) { throw; }
        }
        private int GetCardID(int card)
        {
            try
            {
                var check = db.Customers.Any(d => d.cardId == card.ToString());
                if (check)
                {
                    Random rand = new Random();
                    var tmp = rand.Next(100000, 999999);
                    return GetCardID(tmp);
                }
                else return card;
            }
            catch (Exception) { throw; }
        }
        [AllowAnonymous]
        public JsonResult getCardID()
        {
            try
            {
                Random rand = new Random();
                var tmp = rand.Next(100000, 999999);
                return Json(new { success = "Xử lý thành công", data = GetCardID(tmp) }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message }, JsonRequestBehavior.AllowGet); }//"Xử lý lỗi vui lòng thử lại!" 
        }
        [HttpGet]
        public JsonResult removePriceOld(Guid? uid)
        {
            try
            {
                var item = db.Items.Find(uid);
                item.price = item.price + item.priceOld;
                item.priceOld = 0;
                db.Entry(item).State = EntityState.Modified;
                db.SaveChanges();
                return Json(new { success = "Xóa nợ thành công" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception) { return Json(new { danger = TM.Common.Language.msgError }, JsonRequestBehavior.AllowGet); }
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
                    var rs = db.Customers.Find(tmp);
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
                foreach (var item in uid)
                {
                    var tmp = Guid.Parse(item);
                    var rs = db.Customers.Find(tmp);
                    db.Customers.Remove(rs);
                    var sub_item = db.SubItems.Where(d => d.idKey == rs.id);
                    foreach (var i in sub_item)
                        db.SubItems.Remove(i);
                }
                db.SaveChanges();
                return Json(new { success = TM.Common.Language.msgDeleteSucsess }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception) { return Json(new { danger = TM.Common.Language.msgError }, JsonRequestBehavior.AllowGet); }
        }
    }
}
