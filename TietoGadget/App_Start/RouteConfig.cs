using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace TietoGadget
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            ///Route for SDL Tridion UI 2011 (Aka !SiteEdit)
            routes.MapRoute(
                "SiteEditBlankPage",
                "se_blank.html",
                new { controller = "Empty", action = "Index" });

            routes.MapRoute(
                name: "LogIn",
                url: "LogIn.html",
                defaults: new { controller = "LogIn", action = "Index", id = UrlParameter.Optional }
            );
            routes.MapRoute(
               name: "Update",
               url: "LogIn/UpdateProfile.html",
               defaults: new { controller = "LogIn", action = "UpdateProfile", id = UrlParameter.Optional }
           );
            routes.MapRoute(
              name: "Subscribe",
              url: "LogIn/Subscribe.html",
              defaults: new { controller = "LogIn", action = "Subscribe", id = UrlParameter.Optional }
          );
            routes.MapRoute(
                name: "Register",
                url: "LogIn/Register",
                defaults: new { controller = "LogIn", action = "Register", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Get Comments",
                url: "UGC/GetComments",
                defaults: new { controller = "LogIn", action = "GetComments", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                name: "Save Comments",
                url: "UGC/SaveComment",
                defaults: new { controller = "LogIn", action = "SaveComment", id = UrlParameter.Optional }
            );
            routes.MapRoute(
               name: "Edit Comments",
               url: "UGC/EditComment",
               defaults: new { controller = "LogIn", action = "EditComment", id = UrlParameter.Optional }
           );
            routes.MapRoute(
               name: "Remove Comments",
               url: "UGC/RemoveComment",
               defaults: new { controller = "LogIn", action = "RemoveComment", id = UrlParameter.Optional }
           );
            routes.MapRoute(
               name: "Vote Comment Up",
               url: "UGC/VoteCommentUp",
               defaults: new { controller = "LogIn", action = "VoteCommentUp", id = UrlParameter.Optional }
           );
            routes.MapRoute(
               name: "Vote Comment down",
               url: "UGC/VoteCommentDown",
               defaults: new { controller = "LogIn", action = "VoteCommentDown", id = UrlParameter.Optional }
           );
            routes.MapRoute(
              name: "Post Rating",
              url: "UGC/PostRating",
              defaults: new { controller = "LogIn", action = "PostRating", id = UrlParameter.Optional }
          );
            //Tridion page route
            routes.MapRoute(
               "TridionPage",
               "{*PageId}",
               new { controller = "Page", action = "Page" }, // Parameter defaults
               new { pageId = @"^(.*)?$" } // Parameter constraints
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );

            
        }
    }
}