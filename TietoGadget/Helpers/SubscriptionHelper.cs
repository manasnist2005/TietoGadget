using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TietoGadget.Models;
using Tridion.OutboundEmail.ContentDelivery;
using Tridion.OutboundEmail.ContentDelivery.Profile;
using Tridion.OutboundEmail.ContentDelivery.Utilities;

using Tridion.AudienceManagement.API.Import;
using System.Configuration;
using DD4T.Factories;
using System.Xml.Linq;


namespace TietoGadget.Helpers
{
    public static class SubscriptionHelper
    {

        private static string TIETO_GADGETADDRESSBOOK = ConfigurationManager.AppSettings["Tieto_GadgetAddressBook"];
        /// <summary>
        /// Cheack for the user credential and fetches the user details
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <param name="usr"></param>
        /// <returns></returns>
        public static bool CheckUser(string email, string password, out User usr, out string contactid)
        {
            usr = new User();
            contactid = string.Empty;
            
            try
            {
                var ContactId = new string[] { email, "Website" };
                Contact contact = Contact.GetFromContactIdentificatonKeys(ContactId);                
                if (!Digests.CheckPassword(password, contact.ExtendedDetails["PASSWORD"].StringValue))
                {
                    throw new Exception();
                }
                else
                {
                    usr.Name = contact.ExtendedDetails["NAME"].StringValue;
                    usr.Surname = contact.ExtendedDetails["SURNAME"].StringValue;
                    usr.Company = contact.ExtendedDetails["COMPANY"].StringValue;
                    usr.City = contact.ExtendedDetails["CITY"].StringValue;
                    usr.State = contact.ExtendedDetails["STATE"].StringValue;
                    usr.Zip = contact.ExtendedDetails["ZIP"].StringValue;
                    usr.Birthdate = Convert.ToDateTime(contact.ExtendedDetails["BIRTH_DATE"].StringValue);
                    usr.Age = Convert.ToInt32(((DateTime.UtcNow - usr.Birthdate).TotalDays) / 365);
                    usr.Working_years = System.Decimal.Parse(contact.ExtendedDetails["WORKING_YEARS"].StringValue);
                    usr.Email = contact.ExtendedDetails["MAIL"].StringValue;
                    usr.Prefix = contact.ExtendedDetails["PREFIX"].StringValue;
                    usr.Telephone = contact.ExtendedDetails["TELEPHONE"].StringValue;
                    contactid = contact.Id.ToString();
                   
                    return true;
                }
               
            }
            catch (Com.Tridion.Marketingsolution.Profile.ContactDoesNotExistException ex)
            {
                Logger.WriteLog(Logger.LogLevel.ERROR, ex.Message); 
                return false;
            }
            catch (Exception ex)
            {
                Logger.WriteLog(Logger.LogLevel.WARN, ex.Message+" Password does not match");
                return false;
            }


        }

        /// <summary>
        /// Saves user details
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static string SaveUserDetails(User user)
        {
            
           
            try
            {              
                Contact contact = new Contact();
                contact.AddressBookId = Convert.ToInt32(TIETO_GADGETADDRESSBOOK);
                contact.EmailAddress = user.Email;                    
                contact.EmailTypes = EmailTypes.Multipart;
                contact.BounceStatus = BounceStatus.SoftBounce;                    
                contact.SubscriptionStatus = SubscriptionStatus.Subscribed;
                //By default contact is enabled
                contact.IsEnabled = true;  
                
                //Extended details
                contact.ExtendedDetails["MAIL"].Value = user.Email;
                contact.ExtendedDetails["PREFIX"].Value = user.Prefix;
                contact.ExtendedDetails["SALUTATION"].Value = user.Salutation;
                contact.ExtendedDetails["NAME"].Value = user.Name;
                contact.ExtendedDetails["SURNAME"].Value = user.Surname;
                contact.ExtendedDetails["COMPANY"].Value = user.Company;
                contact.ExtendedDetails["CITY"].Value = user.City;
                contact.ExtendedDetails["STATE"].Value = user.State;
                contact.ExtendedDetails["TELEPHONE"].Value = user.Telephone;
                contact.ExtendedDetails["ZIP"].Value = user.Zip;
                contact.ExtendedDetails["PASSWORD"].Value = Digests.DigestPassword(user.Password);
                contact.ExtendedDetails["BIRTH_DATE"].Value = Convert.ToDateTime(user.Birthdate);
                contact.ExtendedDetails["AGE"].Value = (Convert.ToInt32(((DateTime.UtcNow - user.Birthdate).TotalDays) / 365));
                contact.ExtendedDetails["WORKING_YEARS"].Value = user.Working_years;
                contact.ExtendedDetails["IDENTIFICATION_KEY"].Value = user.Email;
                contact.ExtendedDetails["IDENTIFICATION_SOURCE"].Value = "Website";
                
                //SAVE the contact to subscription 
                contact.Save("tcm:0-0-0" ?? String.Empty);
               

                return "User created";
            }
            catch (ContactAlreadyExistsException ex)
            {
                Logger.WriteLog(Logger.LogLevel.WARN, ex.Message);
                return "Email is already in use";
            }
            catch (Exception ex)
            {
                Logger.WriteLog(Logger.LogLevel.WARN, ex.Message);
                return "Error while registering";
            }

        }
        /// <summary>
        /// Update user profile
        /// </summary>
        public static string UpdateProfile(User user, String email)
        {
            try
            {
                var ContactId = new string[] { email, "website" };
                Contact contact = Contact.GetFromContactIdentificatonKeys(ContactId);
               

                contact.ExtendedDetails["PREFIX"].Value = user.Prefix;
                contact.ExtendedDetails["SALUTATION"].Value = user.Salutation;
                contact.ExtendedDetails["NAME"].Value = user.Name;
                contact.ExtendedDetails["SURNAME"].Value = user.Surname;
                contact.ExtendedDetails["COMPANY"].Value = user.Company;
                contact.ExtendedDetails["CITY"].Value = user.City;
                contact.ExtendedDetails["STATE"].Value = user.State;
                contact.ExtendedDetails["TELEPHONE"].Value = user.Telephone;
                contact.ExtendedDetails["ZIP"].Value = user.Zip;
                //contact.ExtendedDetails["PASSWORD"].Value = Digests.DigestPassword(user.Password);
                contact.ExtendedDetails["BIRTH_DATE"].Value = Convert.ToDateTime(user.Birthdate);
                contact.ExtendedDetails["AGE"].Value = (Convert.ToInt32(((DateTime.UtcNow - user.Birthdate).TotalDays) / 365));
                contact.ExtendedDetails["WORKING_YEARS"].Value = user.Working_years;
                contact.ExtendedDetails["IDENTIFICATION_KEY"].Value = email;
                contact.ExtendedDetails["IDENTIFICATION_SOURCE"].Value = "Website";
                contact.EmailTypes = EmailTypes.Html;
                contact.Save("tcm:0-0-0" ?? String.Empty);
                return "Profile Updated";
            }
            catch (Exception ex)
            {
                Logger.WriteLog(Logger.LogLevel.ERROR, ex.Message);
                return "Error while updating the profile";
            }
        }

        //public static string UpdateSubscription(List<Subscription_Category> Subscription_Categories, String email)
        //{
        //    try
        //    {
        //        var ContactId = new string[] { email, "website" };
        //        Contact contact = Contact.GetFromContactIdentificatonKeys(ContactId);

        //        foreach(Subscription_Category sub_category in Subscription_Categories)
        //        {
        //            if (sub_category.IsChecked == true)
        //            {
        //                TcmUri kwd = new TcmUri(sub_category.TcmId);
        //                contact.Keywords.Add(kwd);
        //            }
        //            else
        //            {
        //                TcmUri kwd = new TcmUri(sub_category.TcmId);
        //                contact.Keywords.Remove(kwd);
        //            }

        //        }

        //        contact.SubscriptionStatus = SubscriptionStatus.OptedIn;                
        //        contact.Save("tcm:0-0-0" ?? String.Empty);
        //        return "You have opted for these categories";
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.WriteLog(Logger.LogLevel.ERROR, ex.Message);
        //        return "Error while subscription";
        //    }
        //}

        public static List<Subscription_Category> GetCategories()
        {

            //List<Subscription_Category> subscriptions = new List<Subscription_Category>();
            //Subscription_Category sc1 = new Subscription_Category() { IsChecked = false, TcmId = "tcm:2033-5925-1024", Category = "Camera" };
            //Subscription_Category sc2 = new Subscription_Category() { IsChecked = false, TcmId = "tcm:2033-5929-1024", Category = "Desktop" };
            //Subscription_Category sc3 = new Subscription_Category() { IsChecked = false, TcmId = "tcm:2033-5927-1024", Category = "Laptop" };
            //Subscription_Category sc4 = new Subscription_Category() { IsChecked = false, TcmId = "tcm:2033-5926-1024", Category = "Mobile" };
            //Subscription_Category sc5 = new Subscription_Category() { IsChecked = false, TcmId = "tcm:2033-5928-1024", Category = "TV" };
            //subscriptions.Add(sc1);
            //subscriptions.Add(sc2);
            //subscriptions.Add(sc3);
            //subscriptions.Add(sc4);
            //subscriptions.Add(sc5);

            PageFactory pg = new PageFactory();
            string keywordset = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" + pg.FindPageContent("/system/keywords.xml").Replace("tcm:Item", "Item");
            XElement xe = XElement.Parse(keywordset);
            var x = from Keywords in xe.Elements("Keywords").Elements("Item")
                    where Keywords.Attribute("CategoryTitle").Value == "Subscription Category"

                    select new Subscription_Category
                    {
                        TcmId = Keywords.Attribute("ID").Value,
                        Category = Keywords.Attribute("Title").Value,
                        IsChecked = false
                    };


            List<Subscription_Category> subscriptions = x.ToList();
            return subscriptions;
           
        }

        public static List<Subscription_Category> CheckSubscription(User usr)
        {
            List<Subscription_Category> Categories = GetCategories();

            try
            {
                var ContactId = new string[] { usr.Email, "Website" };
                Contact contact = Contact.GetFromContactIdentificatonKeys(ContactId);
                foreach (var s in Categories)
                {
                    if (contact.Keywords.Contains(s.TcmId))
                    {
                        s.IsChecked = true;
                    }
                }
                return Categories;


            }

            catch (Exception ex)
            {
                Logger.WriteLog(Logger.LogLevel.WARN, ex.Message + " Error on Checking Subscription");
                return Categories;
            }

        }
        public static string UpdateSubscription(List<Subscription_Category> Subscription_Categories, String email)
        {
            try
            {
                var ContactId = new string[] { email, "website" };
                Contact contact = Contact.GetFromContactIdentificatonKeys(ContactId);

                foreach (Subscription_Category sub_category in Subscription_Categories)
                {
                    if (sub_category.IsChecked == true)
                    {
                        TcmUri kwd = new TcmUri(sub_category.TcmId);
                        contact.Keywords.Add(kwd);
                    }
                    else
                    {
                        TcmUri kwd = new TcmUri(sub_category.TcmId);
                        contact.Keywords.Remove(kwd);
                    }

                }
                if ( contact.Keywords.Count != 0)
                    contact.SubscriptionStatus = SubscriptionStatus.OptedIn;
                else
                    contact.SubscriptionStatus = SubscriptionStatus.Subscribed;
                contact.Save("tcm:0-0-0" ?? String.Empty);
                return "You are opted for these categories";
            }
            catch (Exception ex)
            {
                Logger.WriteLog(Logger.LogLevel.ERROR, ex.Message);
                return "Error while subscription";
            }
        }
    }
}