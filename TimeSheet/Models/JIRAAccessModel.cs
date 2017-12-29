using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TimeSheet.Utils;

namespace TimeSheet.Models
{
    public class JIRAAccessModel
    {
        public static string IssueQuery = string.Empty;
        public static string UserName = string.Empty;
        public static string Passoword = string.Empty;
        public JiraIssueDetailsResult GetIssueWorkLogDetailsfromJIRA(string Query)
        {
            IssueQuery = Query;
            JiraIssueDetailsResult objIssueDetails = new JiraIssueDetailsResult();            
            objIssueDetails = JIRAAccess.GetJiraIssueDetailsBasedOnQuery(Query, UserName, Passoword);
            return objIssueDetails;
        }

        public bool ValidateJIRACredentials(string Username, string password)
        {
            UserName = Username;
            Passoword = password;
            bool IsValidUser = JIRAAccess.IsValidJIRAUser(Username, password);
            return IsValidUser;
        }
        public bool UpdateSubscriptionDetails()
        {
            bool IsSubscribed = JIRAAccess.UpdateSubscription(UserName, IssueQuery);
            return IsSubscribed;
        }

    }
}