using DD4T.ContentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using Tridion.ContentDelivery.Web.WAI;

namespace TietoGadget.Helpers
{
    public static class ProfilingHelper
    {
        public static void AddTrackingKey(User usr, string keyword)
        { 
 
            Tridion.ContentDelivery.Web.WAI.TrackingKeys trackingKey = new TrackingKeys(usr);
            if (trackingKey.GetKey(keyword)== -1)
                trackingKey.SetKey(keyword, 1);
            else
                trackingKey.IncrementKey(keyword);                      
           
         
        }

        
    }
}