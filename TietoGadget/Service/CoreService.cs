using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TietoGadget.CoreService2013;

namespace TietoGadget.Service
{
    public class CoreService : IDisposable
    {
        private readonly SessionAwareCoreServiceClient _client;
        public CoreService()
        {
            _client = new SessionAwareCoreServiceClient("wsHttp");
        _client.Impersonate("SP15DEV//Administrator");
        }
        public SessionAwareCoreServiceClient GetClient
           {
            get
            {
            return _client;
            }
            }

        public UserData GetCurrentUser()
        {
            return _client.GetCurrentUser();
        }
        public void SaveApplicationData(string subjectId, ApplicationData[] applicationData)
        {
            _client.SaveApplicationData(subjectId, applicationData);
        }
        public ApplicationData ReadApplicationData(string subjectId, string applicationId)
        {
            return _client.ReadApplicationData(subjectId, applicationId);
        }
 }
}