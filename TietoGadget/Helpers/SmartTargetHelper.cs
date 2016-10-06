#region Namespace
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Xml;
using System.Xml.Serialization;

using DD4T.ContentModel;
using DD4T.ContentModel.Contracts.Providers;
using DD4T.ContentModel.Factories;
using DD4T.Factories;
using DD4T.Mvc.Html;
using DD4T.Utils;
using Tridion.ContentDelivery.AmbientData;
using Tridion.ContentDelivery.DynamicContent;
using Tridion.ContentDelivery.DynamicContent.Query;
using Tridion.ContentDelivery.Meta;
using Tridion.SmartTarget.Query;
using Tridion.SmartTarget.Query.Builder;
using Tridion.SmartTarget.Utils;
#endregion

namespace TietoGadget.Helpers
{

    public static class SmartTargetHelper
    {

        private static string DEFAULTAGEPROMOTIONCP = ConfigurationManager.AppSettings["DefaultAgePromotioncp"];
        private static string DEFAULTBROWSERPROMOTIONCP = ConfigurationManager.AppSettings["DefaultBrwoserPromotioncp"];
        private static string DEFAULTSEASONPROMOTIONCP = ConfigurationManager.AppSettings["DefaultSeasonPromotioncp"];
        private static string DEFAULTAGEPROMOTIONPAGE = ConfigurationManager.AppSettings["DefaultAgePromotionpage"];
        private static string DEFAULTBROWSERPROMOTIONPAGE = ConfigurationManager.AppSettings["DefaultBrwoserPromotionpage"];
        private static string DEFAULTSEASONPROMOTIONPAGE = ConfigurationManager.AppSettings["DefaultSeasonPromotionpage"];


        /// <summary>
        /// Renders each Smart Target region into a list which can be traversed by the caller.
        /// </summary>
        /// <param name="helper">The helper to extend</param>
        /// <param name="regionName">the name of the region to render</param>
        /// <param name="viewName">the name of the component view</param>
        /// <param name="maxItems">the number to limit on</param>
        /// <param name="startIndex">the item index at which to start rendering</param>
        /// <returns>a list of rendered items for a given region across all promotions</returns>
        public static MvcHtmlString RenderSmartTargetRegionItemsUsingView(this HtmlHelper helper, string regionName, string viewName, string pageuri, int maxItems = 0, int startIndex = 0)
        {
            string publicationId = ConfigurationManager.AppSettings["DD4T.PublicationId"];

            // First query Fredhopper for the targeted component IDs
            ClaimStore claimStore = AmbientDataContext.CurrentClaimStore;

            string query = AmbientDataHelper.GetTriggers(claimStore);
            var queryBuilder = new QueryBuilder();
            queryBuilder.Parse(query);
            if (maxItems > 0)
            {
                queryBuilder.MaxItems = maxItems;
            }

            queryBuilder.StartIndex = startIndex;

            var pubIdUri = new Tridion.SmartTarget.Utils.TcmUri("tcm:0-2033-1");
            Tridion.SmartTarget.Query.Builder.PublicationCriteria pubCriteria
                = new Tridion.SmartTarget.Query.Builder.PublicationCriteria(pubIdUri);
            queryBuilder.AddCriteria(pubCriteria);
            RegionCriteria regionCriteria = new RegionCriteria(regionName);

            var pageIdUri = new Tridion.SmartTarget.Utils.TcmUri(pageuri);
            Tridion.SmartTarget.Query.Builder.PageCriteria pgcriteria = new PageCriteria(pageIdUri);
            queryBuilder.AddCriteria(regionCriteria);
            queryBuilder.AddCriteria(pgcriteria);

            List<string> componentIds = new List<string>();
            ResultSet fredHopperResultset = queryBuilder.Execute();
            //string promotionMarkup="<!-- StartPromotion Region: { \"RegionID\": \"Main Promotion\" } -->";
            StringBuilder promotionXPMMarkUp = new StringBuilder();
            promotionXPMMarkUp.Append(string.Empty);
            promotionXPMMarkUp.Append("<span>\n");
            promotionXPMMarkUp.Append(ResultSet.GetExperienceManagerMarkup(regionName, 10, fredHopperResultset.Promotions));
            var renderedRegionItemsList = new List<MvcHtmlString>();
            foreach (Promotion p in fredHopperResultset.Promotions)
            {
                foreach (Item i in p.Items)
                {
                    componentIds.Add(i.ComponentUriAsString + "|" + i.TemplateUriAsString);
                    i.Visible = true;
                    p.Visible = true;

                }

                foreach (string s in componentIds)
                {
                    promotionXPMMarkUp.Append("<span>\n");
                    promotionXPMMarkUp.Append("<!-- StartPromotion Region: { \"PromotionID\": \" " + p.PromotionId + " \" , \"RegionID\": \" " + regionName + "\" } -->\n");
                    string[] compPresIds = s.Split(new char[] { '|' });
                    string compId = compPresIds[0], templateId = compPresIds[1];
                    // We now have the Model (i.e. the Component), but we need to call the View, which is the title of the CT.
                    // The issue is that the Broker API does not expose (nor store) the title of CTs.  So the only way to get this
                    // is to grab it from DD4T's rendered Component Presentation XML.
                    IComponent comp = null;
                    ComponentFactory cf = new ComponentFactory();
                    cf.TryGetComponent(compId, out comp, "");
                    var renderedCp = helper.Partial(viewName, comp);
                    promotionXPMMarkUp.Append("<span>\n");
                    promotionXPMMarkUp.Append(renderedCp);
                    promotionXPMMarkUp.Append("\n</span>\n");
                    promotionXPMMarkUp.Append("<!-- End Promotion -->\n");
                    promotionXPMMarkUp.Append("</span>\n");
                    renderedRegionItemsList.Add(renderedCp);
                    //renderedRegionItemsList.Add(promotionXPMMarkUp);


                }
            }
            if (componentIds.Count() == 0)
            {
                if (pageuri == DEFAULTAGEPROMOTIONPAGE)
                    componentIds.Add(DEFAULTAGEPROMOTIONCP);
                else if (pageuri == DEFAULTBROWSERPROMOTIONPAGE)
                    componentIds.Add(DEFAULTBROWSERPROMOTIONCP);
                else if (pageuri == DEFAULTSEASONPROMOTIONPAGE)
                    componentIds.Add(DEFAULTSEASONPROMOTIONCP);
                foreach (string s in componentIds)
                {
                    string[] compPresIds = s.Split(new char[] { '|' });
                    string compId = compPresIds[0], templateId = compPresIds[1];
                    IComponent comp = null;
                    ComponentFactory cf = new ComponentFactory();
                    cf.TryGetComponent(compId, out comp, "");
                    var renderedCp = helper.Partial(viewName, comp);
                    renderedRegionItemsList.Add(renderedCp);
                }
            }


            // Next, query the standard Tridion Broker to get the components out.
            // This is because we should use the master source of published content.
            // Using the CP source that has been published to Fredhopper (see API  or service response).
            // is not recommended, so we use the master source of published content, i.e. the Tridion Broker.        


            //if (fredHopperResultset.IsEmpty == "true")
            //{
            //    var renderedCp = helper.Partial(viewName, comp);
            //    renderedRegionItemsList.Add(renderedCp);

            //}


            promotionXPMMarkUp.Append("<!-- End Query-->\n");
            promotionXPMMarkUp.Append("</span>");
            //promotionXPMMarkUp.Append("<!-- End SiteEdit Promotional Region -->");
            return MvcHtmlString.Create(promotionXPMMarkUp.ToString());
        }

        /// <summary>
        /// renders the smart target region
        /// </summary>
        /// <param name="helper">the html helper to extend</param>
        /// <param name="regionName">the name of the region</param>
        /// <param name="componentViewName">the name of the component view</param>
        /// <param name="maxItems"">the maximum number of results to return</param>
        /// <param name="startIndex">which returned item to start the result set on</param>
        /// <returns>returns a concatenated string of all Smart-Targeted component presentations</returns>
        public static MvcHtmlString RenderSmartTargetRegionUsingView(this HtmlHelper helper, string regionName, string componentViewName, string pageuri, int maxItems = 0, int startIndex = 0)
        {
            MvcHtmlString renderedRegionItems = RenderSmartTargetRegionItemsUsingView(helper, regionName, componentViewName, pageuri, maxItems, startIndex);


            return renderedRegionItems;
        }
    }
}
