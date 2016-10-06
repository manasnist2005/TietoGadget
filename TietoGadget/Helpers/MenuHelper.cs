using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DD4T.Factories;
using System.Xml;
using System.Xml.Linq;
using System.Collections;

namespace TietoGadget.Helpers
{
    public class MenuHelper
    {
        public static List<sitemap> GetPageNavigationModel()
        {           
            List<sitemap> lstSitemap = new List<sitemap>();
            PageFactory pg = new PageFactory();
            //Read the page contents as string
            string sitemapxml = pg.FindPageContent("/system/sitemap/sitemap.xml");
                     
            XElement xe = XElement.Parse(sitemapxml);

            var x = from url in xe.Elements("url")
                    where url.Attribute("type").Value == "SG"
                    //select loc.Element("loc").Value;
                    select new sitemap
                    {
                        loc = url.Element("loc").Value,
                        title = url.Element("title").Value
                    };

            sitemap sitemaphome = new sitemap();
            sitemaphome.Title = "Home";
            sitemaphome.Location = "/";
            lstSitemap.Add(sitemaphome);

            foreach (var stmap in x.ToList())
            {
                sitemap sitemap = new sitemap();
                sitemap.Title = stmap.title;
                sitemap.Location = stmap.loc;
                lstSitemap.Add(sitemap);
            }
            return lstSitemap;

        }

        public static List<sitemap> GetFooterNavigationModel()
        {
            List<sitemap> lstvav = new List<sitemap>();
            PageFactory pg = new PageFactory();
            string footerxml = pg.FindPageContent("/system/navigation.xml");
            XElement xe = XElement.Parse(footerxml);
            var x = from url in xe.Elements("url")
                    where url.Attribute("type").Value == "SG"
                    //select loc.Element("loc").Value;
                    select new sitemap
                    {
                        loc = url.Element("loc").Value,
                        title = url.Element("title").Value
                    };

            foreach (var stmap in x.ToList())
            {
                sitemap sitemap = new sitemap();
                sitemap.Title = stmap.title;
                sitemap.Location = stmap.loc;
                lstvav.Add(sitemap);
            }
            return lstvav;

        }
    }

    public class sitemap
    {
        public string loc;
        public string title;

        public string Location
        {
            get { return loc; }
            set { loc = value; }
        }
        public string Title
        {
            get { return title; }
            set { title = value; }
        }


    }

}