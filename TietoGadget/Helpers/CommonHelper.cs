using System;
using System.Web.Mvc;
using DD4T.ContentModel;
using DD4T.ContentModel.Factories;
using DD4T.Factories;
using System.Collections.Generic;

namespace TietoGadget.Helpers
{
    public static class CommonHelper
    {
        /// <summary>
        /// Returns component for specified component ID and CT
        /// </summary>
        /// <param name="componentId"></param>
        /// <param name="componentCTId"></param>
        /// <returns></returns>
        public static Component GetComponent(string componentId, string componentCTId)
        {
            IComponentFactory cf = DependencyResolver.Current.GetService<IComponentFactory>();
            IComponent comp = cf.GetComponent(componentId, componentCTId);
            return (Component)comp;
        }

        /// <summary>
        /// Returns component for specified component ID 
        /// </summary>
        /// <param name="componentId"></param>
        /// <param name="componentCTId"></param>
        /// <returns></returns>
        public static Component GetComponent(string componentId)
        {
            IComponentFactory cf = DependencyResolver.Current.GetService<IComponentFactory>();
            IComponent comp = cf.GetComponent(componentId);
            return (Component)comp;
        }

        /// <summary>
        /// The Value method accepts a key field to check against the field object. If the key is present then its value is returned
        /// otherwise the empty string is returned. 
        /// The Benefit of this method is reducing view logic key checking.
        /// </summary>
        /// <param name="field">Tridion Field</param>
        /// <param name="key">Tridion Field Key</param>
        /// <returns>String Value of field[key]</returns>
        public static string Value(IFieldSet field, string key)
        {
            if (!String.IsNullOrEmpty(key))
            {
                if (field.ContainsKey(key))
                {
                    return field[key].Value;
                }
            }
            return String.Empty;
        }
                
        /// <summary>
        /// Returns the resolved url from a component tcm ID
        /// </summary>
        /// <param name="componentId"></param>
        /// <returns></returns>
        public static string GetResolvedUrl(string componentId)
        {
            LinkFactory dd4tLinkFactory = new LinkFactory();
            string url = dd4tLinkFactory.ResolveLink(componentId);
            return url;
        }

        /// <summary>
        /// Read the carousel component values from Fieldset and sets the values in the variables
        /// </summary>
        /// <param name="field"></param>
        /// <param name="imgurl"></param>
        /// <param name="title"></param>
        /// <param name="header"></param>
        /// <param name="description"></param>
        /// <param name="link"></param>
        public static void GetCarouselComponentValues(DD4T.ContentModel.IFieldSet field, out string imgurl, out string title, 
                                                               out string header, out string description, out string link)
        {

            header = description = imgurl =title= string.Empty;
            link = "#";
            if (field.Keys.Contains("image"))
            { 
                imgurl = field["image"].LinkedComponentValues[0].Multimedia.Url;
                title = @field["image"].LinkedComponentValues[0].Title;
            }
            if (field.Keys.Contains("header")) 
                header = field["header"].Value;
            if(field.Keys.Contains("description")) 
                description = field["description"].Value;
            if (field.Keys.Contains("link")) 
                link = field["link"].Value;           
            
        }

        public static void GetPageNavigationModel()
        {
            PageFactory pg = new PageFactory();
            
        }

        //For dummy
        public static List<string> wrapComponents(List<string> componentIds)
        {
            componentIds.Clear();
            componentIds.Add("tcm:2033-4623");
            componentIds.Add("tcm:2033-4781");
            componentIds.Add("tcm:2033-4691");
            componentIds.Add("tcm:2033-4638");
            return componentIds;
        }

    }
}