using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Com.Tridion.Storage.Ugc;
using Tridion.ContentDelivery.UGC.Web.Model;
using Tridion.ContentDelivery.UGC.WebService;
using System.Web.Mvc;
using Tridion.OutboundEmail.ContentDelivery;
using Tridion.OutboundEmail.ContentDelivery.Profile;
using System.Xml;
using Tridion.ContentDelivery.AmbientData;



namespace TietoGadget.Helpers
{
    
    public static class UGCHelper
    {
        private static Uri UserClaim = new Uri("taf:claim:contentdelivery:webservice:user");

        public static List<Comment> GetComments(string tcmId, out int total, out double avgRating)
        {
            List<Comment> lstComment = new List<Comment>();
            try
            {
                if (tcmId != string.Empty)
                {
                    lstComment = CommentsRetriever.RetrieveComments(tcmId, true, true, true, true, false, false, false, 100, 0);
                    avgRating = (ItemStatsRetriever.RetrieveItemStats(tcmId)).AverageRating;
                    total = lstComment.Count;
                    return lstComment.ToList();
                }
                else
                {
                    total = 0;
                    avgRating = 0.0;
                    return lstComment;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                total = 0;
                avgRating = 0.0;
                return lstComment;
            }
        }       
        public static string PostComment(string tcmID, string userId, string email, string content)
        {
            try
            {
                if (email != "")
                {
                    var ContactId = new string[] { email, "website" };
                    Contact contact = Contact.GetFromContactIdentificatonKeys(ContactId);
                    SyncUGCUserWithContact(contact, contact.ExtendedDetails["NAME"].Value.ToString(), email);
                    WebServiceHelper.PostComment(tcmID, contact.ExtendedDetails["NAME"].Value.ToString(), email, content);
                }
                else
                {
                    WebServiceHelper.PostComment(tcmID, userId, email, content);
                }
                return "Comment posted";
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                return "Error while execution";
            }
        }
        public static string EditComment(string tcmID, long commentId, string content)
        {
            try
            {
                WebServiceHelper.EditComment(tcmID, commentId, content);
                return "Comment edited";
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                return "Error while execution";
            }
        }
        public static string RemoveComment(long commentId)
        {
            try
            {
                WebServiceHelper.RemoveComment(commentId);
                return "Comment removed";
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                return "Error while execution";
            }
        }
        public static string PostRating(string tcmID, int rating)
        {
            try
            {
                WebServiceHelper.PostRating(tcmID, rating);                
                return "Rating posted";
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                return "Error while execution";
            }
        }
        public static string CommentVoteUp(long commetId)
        {
            try
            {
                WebServiceHelper.VoteCommentUp(commetId);
                return "Vote up for the comment";
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                return "Error while execution";
            }
        }
        public static string CommentVoteDown(long commetId)
        {
            try
            {
                WebServiceHelper.VoteCommentDown(commetId);
                return "Vote down for the comment";
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                return "Error while execution";
            }
        }

       
        private static void SyncUGCUserWithContact(Contact contact, string name, string email)
        {
            // Modify to fit AM identification fields
            string externalId = "{\"IDENTIFICATION_KEY\":\"" + contact.ExtendedDetails["IDENTIFICATION_KEY"].StringValue + "\",\"IDENTIFICATION_SOURCE\":\"" + contact.ExtendedDetails["IDENTIFICATION_SOURCE"].StringValue + "\"}";

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            nsmgr.AddNamespace("m", "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata");
            nsmgr.AddNamespace("d", "http://schemas.microsoft.com/ado/2007/08/dataservices");

            XmlDocument users = new XmlDocument();
            WebServiceClient webServiceClient = new WebServiceClient("application/atom+xml");
            string ugcResponse = webServiceClient.DownloadString("/Users?$filter=ExternalId eq '" + externalId + "'");
            users.LoadXml(ugcResponse);

            XmlNodeList responseUsers = users.GetElementsByTagName("properties", "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata");
            XmlNode firstUser = null;
            if (responseUsers.Count > 0) {
                firstUser = responseUsers.Item(0);
            }
            
            if (firstUser != null)
            {
                // Get first 'Contact-linked' UGC user.
                string userId = firstUser.SelectSingleNode("d:Id", nsmgr).InnerText;
                AmbientDataContext.CurrentClaimStore.Put(UserClaim, userId);

                //
                // INSERT CODE HERE if remaining users need to be aggregated.
                //

            }
            else
            {
                // Else: Current user claim is leading.
                if (AmbientDataContext.CurrentClaimStore.Contains(UserClaim))
                {
                    XmlDocument existingUser = new XmlDocument();
                    string ugcUser = webServiceClient.DownloadString("/Users(Id=" + AmbientDataContext.CurrentClaimStore.Get<string>(UserClaim) + ")");
                    existingUser.LoadXml(ugcUser);

                    XmlNodeList userElementList = existingUser.GetElementsByTagName("properties", "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata");
                    XmlNode userElement = null;
                    if (userElementList.Count > 0) {
                        userElement = userElementList.Item(0);
                    }
                    
                    webServiceClient = new WebServiceClient();
                    if (userElement != null)
                    {
                        // Link current tracked UGC user to Contact.
                        string existingUserId = userElement.SelectSingleNode("d:Id", nsmgr).InnerText;
                        User user = new User(
                            existingUserId,
                            userElement.SelectSingleNode("d:Name", nsmgr).InnerText,
                            userElement.SelectSingleNode("d:EmailAddress", nsmgr).InnerText,
                            externalId);

                        webServiceClient.UploadString("/Users(Id=" + existingUserId + ")", "PUT", "{d:" + user.ToJSON() + "}");
                    }
                    else
                    {
                        // Create new 'Contact-linked' UGC user.
                        User user = new User(
                            AmbientDataContext.CurrentClaimStore.Get<string>(UserClaim),
                            name,
                            email,
                            externalId);

                        webServiceClient.UploadString("/Users", "POST", "{d:" + user.ToJSON() + "}");
                    }
                }
            }
        }
       /* 
        public void test()
        {
            WebServiceClient websvcclient = new WebServiceClient();
           
        }
        */

    }
}