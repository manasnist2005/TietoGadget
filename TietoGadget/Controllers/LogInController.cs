using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using TietoGadget.Helpers;
using TietoGadget.Models;
using Tridion.ContentDelivery.UGC.WebService;


namespace TietoGadget.Controllers
{
    public class LogInController : Controller
    {
        //
        // GET: /LogIn/
       [HttpGet]
        public ActionResult Index()
        {
            MenuHelper.GetPageNavigationModel();
            Session["UserName"] = null;
            Session["contactid"] = null;
            Session["User"] = null; 
           Session["ItemTcmID"] = null;
           Session.Clear();
            return View();
        }
        [HttpPost]
        public ActionResult Index(User usr)
        {
            if((!string.IsNullOrEmpty(usr.Email)) && (!string.IsNullOrEmpty(usr.Password)))
            {
                string contactid = string.Empty;
                if (SubscriptionHelper.CheckUser(usr.Email, usr.Password, out usr, out contactid))
                {
                    Session["UserName"] = usr.Name;
                    Session["contactid"] = contactid;
                    Session["User"] = usr;
                    #region Add cookie for custom cartridge
                    /* 
                     * 
                    HttpCookie objCookie = new HttpCookie("UserAge");
                    Response.Cookies.Clear();
                    objCookie.Value = usr.Age.ToString();
                    Response.Cookies.Add(objCookie);
                    DateTime dtExpiry = DateTime.Now.AddMinutes(10);
                    Response.Cookies["UserAge"].Expires = dtExpiry;
                     */
                #endregion
                    if(!string.IsNullOrEmpty(contactid))
                        HttpContext.Session.Add("taf:claim:audiencemanager:contact:id", contactid);

                    FormsAuthentication.SetAuthCookie(usr.Email, true);
                    
                    
                    return Redirect("/Index.html");
                }                    
                else
                {
                    ViewBag.LoginMsg = "Email and password do not match";
                    return View("Index"); 
                }
               
            }
            else
            {
                return View("Index");
            }

        }
        [HttpPost]
        public ActionResult Register(User usr)
        {
            if (ModelState.IsValid)
            {
                ViewBag.Message = SubscriptionHelper.SaveUserDetails(usr);
                return View("Index");
            }
            else
            {               
                return View("Index");
            }
               
        }

        [HttpGet]
        public ActionResult UpdateProfile()
        {
            User usr = new User();
            if (Session["User"] != null)
            {
                usr = Session["User"] as User;
                return View("UpdateProfile", usr);
            }
            else
                return View("Index");

        }

        [HttpPost]
        public ActionResult UpdateProfile(User usr)
        {
            User sessionusr = Session["User"] as User;
            TryUpdateModel(usr);
            ModelState.Remove("Email");
            ModelState.Remove("Password");
            if (ModelState.IsValid)
            {
                ViewBag.UpdateMessage = SubscriptionHelper.UpdateProfile(usr, sessionusr.Email);
                return Redirect("/Login.html");
            }
            else
            {
                return View();
            }

        }

        [HttpGet]
        public ActionResult Subscribe()
        {
            User usr = new User();
            if (Session["User"] != null)
            {
                usr = Session["User"] as User;
                usr.UserPreferences = SubscriptionHelper.CheckSubscription(usr);
                return View("Subscribe", usr);
            }
            else
                return View("Index");




        }

        [HttpPost]
        public ActionResult Subscribe(User usr)
        {
            User sessionusr = Session["User"] as User;
            ViewBag.SubscriptionMessage = SubscriptionHelper.UpdateSubscription(usr.UserPreferences, sessionusr.Email);
            return Redirect("/Index.html");

        }

        #region UGC Function calls
        [HttpGet]
        public JsonResult GetComments(string pageid)
        {
            int total; double avgRating;
            if (Session["ItemTcmID"] != null)
                pageid = Session["ItemTcmID"].ToString();
            var records = UGCHelper.GetComments(pageid, out total, out avgRating);            
            return Json(new { records, total, pageid ,avgRating}, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public string SaveComment(string content)
        {
            if (Session["User"] == null )
                return UGCHelper.PostComment(Session["ItemTcmID"].ToString(), "Anonymous", "", content);
            else
            {
                TietoGadget.Models.User user = Session["User"] as User;
                return UGCHelper.PostComment(Session["ItemTcmID"].ToString(), user.Name + " " + user.Surname, user.Email, content);   
            }
        }
        [HttpPost]
        public string EditComment(string tcmID,long commentId,string content)
        { 
            return UGCHelper.EditComment(tcmID, commentId, content);
        }

        [HttpPost]
        public string RemoveComment(long id)
        {
            return UGCHelper.RemoveComment(id);
        }
        [HttpPost]
        public string VoteCommentUp(long id)
        {
            return UGCHelper.CommentVoteUp(id);
        }
        [HttpPost]
        public string VoteCommentDown(long id)
        {
            return UGCHelper.CommentVoteDown(id);
        }
        [HttpPost]
        public string PostRating(string tcmId,int rating)
        {
            return UGCHelper.PostRating(tcmId, rating);
        }

        #endregion
    }
}
