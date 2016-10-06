using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System;
using System.Configuration;
using Tridion.OutboundEmail.ContentDelivery;
using Tridion.OutboundEmail.ContentDelivery.Profile;
using Tridion.OutboundEmail.ContentDelivery.Utilities;

namespace TietoGadget.Controllers
{
    public class ProfileController : Controller
    {
        //
        // GET: /Profile/

        public ActionResult Index()
        {
            Contact contact = new Contact();
            contact.EmailAddress = "manas.panda@tieto.com";
            contact.ExtendedDetails["MAIL"].Value = "manas.panda@tieto.com";
            contact.ExtendedDetails["SALUTATION"].Value = "MR.";
            contact.ExtendedDetails["NAME"].Value = "Manas";
            contact.ExtendedDetails["SURNAME"].Value = "Panda";
            contact.ExtendedDetails["COMPANY"].Value = "Tieto";
            contact.ExtendedDetails["CITY"].Value = "Pune";
            contact.ExtendedDetails["STATE"].Value = "Maharashtra";
            contact.ExtendedDetails["TELEPHONE"].Value = "020-123456789";
            contact.ExtendedDetails["ZIP"].Value = "411014";
            contact.ExtendedDetails["PASSWORD"].Value = Digests.DigestPassword("abcd");
            contact.ExtendedDetails["BIRTH_DATE"].Value = "01-01-1988";
            contact.ExtendedDetails["AGE"].Value = "28";
            contact.ExtendedDetails["WORKING_YEARS"].Value = "5.9";
            contact.EmailTypes = EmailTypes.Html;
            contact.Save();
            return View();
        }

    }
}
