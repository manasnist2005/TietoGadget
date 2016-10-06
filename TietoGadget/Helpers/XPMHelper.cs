#region Namespace
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.Linq;
#endregion

namespace TietoGadget.Helpers
{

    public static class ExperienceManagerHelper
    {
        public static SiteEditSettings SiteEditSettings = new SiteEditSettings();

        /// <summary>
        /// Method to create region markup on the page
        /// </summary>
        /// <param name="regionId">ID of the region configured in region configuration</param>
        /// <returns>Region markup</returns>
        public static string GetRegionMarkUp(string regionId)
        {
            try
            {
                Logger.WriteLog(Logger.LogLevel.DEBUG, "Start GetRegionMarkUp for region: " + regionId);
                var region = SiteEditSettings.RegionXml.Elements("region").Where(e => e.Attribute("id").Value == regionId).FirstOrDefault();

                if (region != null)
                {
                    var title = region.Attribute("name").Value;
                    var minOccursElem = region.Elements("minOccurs").FirstOrDefault();
                    var maxOccursElem = region.Elements("maxOccurs").FirstOrDefault();
                    string minOccurs = "0";
                    string maxOccurs = "0";

                    if (minOccursElem != null)
                        minOccurs = minOccursElem.Value;

                    if (maxOccursElem != null)
                        maxOccurs = maxOccursElem.Value;


                    var cts = region.Element("allowedComponentTypes").Elements("componentType");

                    StringBuilder allowedCts = new StringBuilder();
                    foreach (var ct in cts)
                    {
                        var schemaName = ct.Attribute("schema").Value;
                        var templateName = ct.Attribute("template").Value;
                        string schemaUri = "";
                        string templateUri = "";
                        try
                        {
                            schemaUri = SiteEditSettings.RegionXml.Descendants("schema").Where(s => s.Attribute("name").Value.Equals(schemaName, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault().Attribute("tcmUri").Value;
                            templateUri = SiteEditSettings.RegionXml.Descendants("template").Where(s => s.Attribute("name").Value.Equals(templateName, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault().Attribute("tcmUri").Value;
                        }
                        catch (Exception)
                        {
                            throw new ApplicationException(string.Format("Schema and/or template cannot be found in the regionconfiguration. I was looking for this: Schemaname: {0}. Templatename: {1}. Are they configered in the <schemas> and/or the <templates> node?", schemaName, templateName));
                        }

                        allowedCts.Append(string.Format(allowedComponentTypesFormat, schemaUri, templateUri));
                    }

                    int min = 0;
                    int max = 0;
                    if (int.TryParse(minOccurs, out min) && int.TryParse(maxOccurs, out max))
                    {
                        if (min == 0 && max == 0)
                        {
                            //leave out the minOccurs and maxOccurs
                            return string.Format(regionFormatNoMinNoMax, title, allowedCts.ToString());
                        }
                        else if (min == 0)
                        {
                            return string.Format(regionFormatNoMin, title, allowedCts.ToString(), maxOccurs);
                        }
                        else if (max == 0)
                        {
                            return string.Format(regionFormatNoMax, title, allowedCts.ToString(), minOccurs);
                        }
                    }

                    return string.Format(regionFormat, title, allowedCts.ToString(), minOccurs, maxOccurs);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteException(Logger.LogLevel.ERROR, "There is some exception occured in GetRegionMarkUp for region: " + regionId, ex);
            }
            return "";
        }

        #region string variable
        private static string regionFormat = "<!-- Start Region: {{ \"title\": \"{0}\", " +
                "\"allowedComponentTypes\": [ " +
                "{1}" +
                " ], " +
                "\"minOccurs\": {2}, " +      // minimum amount of components in this region
                "\"maxOccurs\": {3} " +       // maximum amount of components in this region
            "}} -->";

        private static string regionFormatNoMinNoMax = "<!-- Start Region: {{ \"title\": \"{0}\", " +
                "\"allowedComponentTypes\": [ " +
                "{1}" +
                " ] " +
            "}} -->";

        private static string regionFormatNoMin = "<!-- Start Region: {{ \"title\": \"{0}\", " +
              "\"allowedComponentTypes\": [ " +
                "{1}" +
                " ], " +
                "\"maxOccurs\": {2} " +       // maximum amount of components in this region
            "}} -->";

        private static string regionFormatNoMax = "<!-- Start Region: {{ \"title\": \"{0}\", " +
               "\"allowedComponentTypes\": [ " +
                "{1}" +
                " ], " +
                "\"minOccurs\": {2} " +      // minimum amount of components in this region               
            "}} -->";

        private static string allowedComponentTypesFormat = "{{\"schema\": \"{0}\", " + // schema tcm uri
                   "\"template\": \"{1}\"}}"; // component template tcm ur
        #endregion

    }

    [Serializable]
    public class SiteEditSettings
    {
        public static string SiteEditConfigurationPath = "~/SiteEdit_config.xml";
        public static string RegionConfigurationPath = "~/RegionConfiguration.xml";
        /* Boolean indicating whether or not SiteEdit is enabled for the entire web application */
        public bool Enabled { get; set; }
        public XElement RegionXml { get; set; }

        public SiteEditSettings()
            : this(HttpContext.Current.Server.MapPath(SiteEditConfigurationPath))
        {
        }

        public SiteEditSettings(string pathToSiteEditConfiguration)
        {
            Enabled = false;
            XmlDocument seConfig = new XmlDocument();
            try
            {
                seConfig.Load(pathToSiteEditConfiguration);
                string enabled = seConfig.DocumentElement.GetAttribute("enabled");
                if ("true".Equals(enabled))
                {
                    var pathToRegionConfigXml = HttpContext.Current.Server.MapPath(RegionConfigurationPath);
                    if (File.Exists(pathToRegionConfigXml))
                    {
                        var content = File.ReadAllText(pathToRegionConfigXml, Encoding.UTF8);
                        RegionXml = XElement.Parse(content);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteException(Logger.LogLevel.ERROR, "There is some exception occured in SiteEditSettings. ", ex);
            }
        }
    }
}
 