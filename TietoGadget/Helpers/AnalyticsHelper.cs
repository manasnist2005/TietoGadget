using Google.Apis.Analytics.v3;
using Google.Apis.Analytics.v3.Data;
using Google.Apis.Services;
using Google.Apis.Auth.OAuth2;
using System.Security.Cryptography.X509Certificates;
using System.Collections.Generic;
using System.Linq;
using System;

public class GoogleAnalyticsData
{
    public AnalyticsService Service;
    String profileId;
    public GoogleAnalyticsData(string keyPath, string accountEmailAddress, string profileId)
    {


        var certificate = new X509Certificate2(keyPath, "notasecret", X509KeyStorageFlags.Exportable);

        var credentials = new ServiceAccountCredential(
           new ServiceAccountCredential.Initializer(accountEmailAddress)
           {
               Scopes = new[] { AnalyticsService.Scope.AnalyticsReadonly }
           }.FromCertificate(certificate));

        Service = new AnalyticsService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credentials,
            ApplicationName = "WorthlessVariable"
        });
        this.profileId = profileId;

    }

    public GaData GetAnalyticsData(string[] dimensions, string[] metrics, DateTime startDate, DateTime endDate)
    {
        GaData response = null;
        if (!profileId.Contains("ga:"))
            profileId = string.Format("ga:{0}", profileId);
        ManagementResource.AccountsResource.ListRequest AccountListRequest = Service.Management.Accounts.List();
        Accounts AccountList = AccountListRequest.Execute();
        var request = BuildAnalyticRequest(profileId, dimensions, metrics, startDate, endDate);
        request.Sort = "-ga:totalEvents";
        request.MaxResults = 5;
        response = request.Execute();
        return response;

    }
    private DataResource.GaResource.GetRequest BuildAnalyticRequest(string profileId, string[] dimensions, string[] metrics,
                                                                        DateTime startDate, DateTime endDate)
    {
        DataResource.GaResource.GetRequest request = Service.Data.Ga.Get(profileId, startDate.ToString("yyyy-MM-dd"),
                                                                            endDate.ToString("yyyy-MM-dd"), string.Join(",", metrics));
        request.Dimensions = string.Join(",", dimensions);
        return request;
    }

   

}

public static class AnalyticsHelper
{
    public static List<string> GetMostClickedContent()
    {
       // GoogleAnalyticsData ga = new GoogleAnalyticsData(@"C:\NeelNew\Tieto Project-e2f98441b191.p12", "test-analytics@tieto-project-1232.iam.gserviceaccount.com", "ga:117415479");
        GoogleAnalyticsData ga = new GoogleAnalyticsData(@"C:\NeelNew\Test Project-3d09236a0418.p12", "testing-account@earnest-math-125406.iam.gserviceaccount.com", "ga:118740657");
        string[] dimensions = { "ga:eventCategory", "ga:eventAction", "ga:eventLabel" };
        string[] metrices = { "ga:totalEvents" };
        DateTime stdate = Convert.ToDateTime("01/01/2015");
        DateTime enddate = Convert.ToDateTime("01/01/2017");
        GaData gd = ga.GetAnalyticsData(dimensions, metrices, stdate, enddate);
        
        List<string> componentIds = new List<string>();
        foreach (var content in gd.Rows)
        {
            if (content[0].ToString().StartsWith("tcm"))
                componentIds.Add(content[0]);           
        }

        return componentIds;
    }
    
}