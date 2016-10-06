using DD4T.Mvc.Controllers;
using TietoGadget.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TietoGadget.Helpers;
using DD4T.ContentModel;
using Tridion.ContentDelivery.Web.WAI;
using Tridion.ContentDelivery.UGC.WebService;




namespace TietoGadget.Controllers
{
    public class PageController : TridionControllerBase
    {
        public override ActionResult Page(string pageId)
        {          
            try
            {
                pageId = UriHelper.ParseUrl(pageId);
                Logger.WriteLog(Logger.LogLevel.DEBUG, "Page ID:" + pageId);
                
                
                
                string  currentPageUrl = string.Empty;
                if (pageId.Contains("/index.html"))
                  currentPageUrl = pageId.Replace("/index.html","");
                Session["currentPageUrl"] = "/" + currentPageUrl;
                
                string pageCategory = string.Empty;
                IPage pgModel = GetModelForPage(pageId, out pageCategory);

                //For Product Details
                if (Request.QueryString["q"] != null)
                {
                    string compId = Request.QueryString["q"].ToString();
                    Session["compId"] = Request.QueryString["q"].ToString();
                    Session["ItemTcmID"] = Request.QueryString["q"].ToString()+"-16";
                }
                else
                { 
                //For UGC 
                    Session["ItemTcmID"] = pgModel.Id;
                }
                //Add tracking Key
                AddTrackingKeys(pgModel.Id, this.HttpContext.ApplicationInstance.Context, pageCategory);
                
                //Fetches Product intro component list having corresponding category
                if (!string.IsNullOrEmpty(pageCategory))
                {
                    Session["productlist"] = BrokerQueryHelper.GetProductIntroComponentList(pageCategory);
                }
                
                return base.Page(pageId);
            }
            catch (Exception ex)
            {
                Logger.WriteLog(Logger.LogLevel.ERROR, ex.Message);
                Response.StatusCode = 404;
                return base.Page("system/error_404.html");                
            }
        }

        /// <summary>
        /// Gets the PageModel from the pageurl and sets the related page category
        /// </summary>
        /// <param name="PageId"></param>
        /// <param name="pageCategory"></param>
        /// <returns></returns>
        protected IPage GetModelForPage(string PageId, out string pageCategory)
        {
            IPage page = null;
            if (PageFactory != null)
            {
                if (PageFactory.TryFindPage(string.Format("/{0}", PageId), out page))
                {
                    DD4T.ContentModel.FieldSet aa = page.MetadataFields as DD4T.ContentModel.FieldSet;
                    var bb = page.MetadataFields.Values as Dictionary<string, DD4T.ContentModel.IField>.ValueCollection;
                    if (page.MetadataFields.Keys.Contains("product_category"))
                        pageCategory = page.MetadataFields["product_category"].Value;
                    else
                        pageCategory = string.Empty;
                    //var bba = ((new System.Collections.Generic.Mscorlib_DictionaryValueCollectionDebugView<string, DD4T.ContentModel.IField>(bb)).Items[0]).Value;
                    return page;
                }
               
            }
            pageCategory = string.Empty;
            return page;
        }

        /// <summary>
        /// Add tracking key entry to the broker database
        /// </summary>
        /// <param name="pageId"></param>
        /// <param name="context"></param>
        /// <param name="pageCategory"></param>
        private void AddTrackingKeys(string pageId, HttpContext context, string pageCategory)
        {
            try
            {
                var waiPage = new WAIPage(pageId, context);
                Tridion.ContentDelivery.Web.WAI.TrackingKeys trackingKey = new TrackingKeys(waiPage.User);
                Session["WAIUser"] = waiPage.User.PresentationId.ToString() + "_" + waiPage.User.Id.ToString();
                if (!string.IsNullOrEmpty(pageCategory))
                {
                    if (trackingKey.GetKey(pageCategory) < 1)
                        trackingKey.SetKey(pageCategory, 1);
                    else
                        trackingKey.IncrementKey(pageCategory);
                    trackingKey.ExecuteUpdate();
                }
            }
            catch(Exception ex)
            {
                Logger.WriteLog(Logger.LogLevel.ERROR, ex.Message);
            }


        }
    }
}
