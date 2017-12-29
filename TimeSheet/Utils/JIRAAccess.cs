using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Xml;
using System.Xml.Linq;
using TimeSheet.Models;

namespace TimeSheet.Utils
{
    public class JIRAAccess
    {
        static System.Net.Http.HttpContent _content;
        static HttpResponseMessage _response;
        public static List<JIRAIssueDetails> ResultJiraIssues;
        static string _credentialString = string.Empty;
        private static HttpClient _client;
        public static JiraIssueDetailsResult issueDetailsResult;
        static public string JiraUserName;
        static public string JiraPassword;

        /// <summary>
        /// Get issue details from JIRA by passing the jql query.
        /// </summary>
        /// <param name="pJQLQuery"></param>
        /// <param name="pUserName"></param>
        /// <param name="pPassword"></param>
        /// <returns></returns>
        static public JiraIssueDetailsResult GetJiraIssueDetailsBasedOnQuery(string pJQLQuery, string pUserName, string pPassword)
        {
            issueDetailsResult = new JiraIssueDetailsResult();
            ResultJiraIssues = new List<JIRAIssueDetails>();
            _credentialString = pUserName + ":" + pPassword;
            int JsonResultStartPosition = 0;
            int ResultMaxPosition = 1000;
            bool IsExceedMaxResult = true;
            int _fetchedRecordsCount = 0;

            for (int StartPosition = JsonResultStartPosition; IsExceedMaxResult; StartPosition = _fetchedRecordsCount)
            {
                string json = GetInputJsonString(pJQLQuery, ResultMaxPosition.ToString(), StartPosition.ToString());

                _client = new System.Net.Http.HttpClient();
                _client.DefaultRequestHeaders.ExpectContinue = false;
                _client.Timeout = TimeSpan.FromSeconds(90);
                byte[] crdential = System.Text.UTF8Encoding.UTF8.GetBytes(_credentialString);
                _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(crdential));
                _client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                _content = new StringContent(json, UTF8Encoding.UTF8, "application/json");
                try
                {
                    _response = _client.PostAsync("https://syncfusion.atlassian.net/rest/api/2/search", _content).Result;
                    _response.EnsureSuccessStatusCode();
                    if (_response.StatusCode == System.Net.HttpStatusCode.OK || _response.StatusCode == System.Net.HttpStatusCode.NoContent)
                    {
                        var Deserialize = new JavaScriptSerializer();
                        Deserialize.MaxJsonLength = Int32.MaxValue;
                        JiraFieldJson ResultJson = Deserialize.Deserialize<JiraFieldJson>(_response.Content.ReadAsStringAsync().Result);
                        ResultJiraIssues.AddRange(GetIssueDetails(ResultJson));
                        _fetchedRecordsCount = _fetchedRecordsCount + ResultJson.issues.Count();

                        if (_fetchedRecordsCount < ResultJson.total)
                        {
                            IsExceedMaxResult = true;
                        }
                        else if (_fetchedRecordsCount >= ResultJson.total)
                        {
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    issueDetailsResult.IsSucessResult = false;
                    break;
                }
            }

            if (ResultJiraIssues != null && ResultJiraIssues.Count > 0)
            {
                issueDetailsResult.TaskList.AddRange(ResultJiraIssues);
            }
            return issueDetailsResult;
        }

        public static List<JIRAIssueDetails> GetIssueDetails(JiraFieldJson pReturnJsonFile, bool IsQueryPresent = false, bool pIsProjectReleasePresent = false)
        {
            JIRAIssueDetails _issueDetails;
            JIRAIssueDetails _issueworklogDetails = new JIRAIssueDetails();
            List<JIRAIssueDetails> issueDetailsResults = new List<JIRAIssueDetails>();
            string ControlManager = string.Empty;
            foreach (var issue in pReturnJsonFile.issues)
            {
                _issueDetails = new JIRAIssueDetails();
                try
                {
                    //WorkLog
                    _issueworklogDetails.worklog = (issue.fields.worklog != null) ? issue.fields.worklog : null;

                    if (_issueworklogDetails.worklog != null && _issueworklogDetails.worklog.worklogs.Count > 0)
                    {                        
                        foreach (var work in _issueworklogDetails.worklog.worklogs)
                        {
                            _issueDetails = new JIRAIssueDetails();
                            _issueDetails.JIRAId = issue.key;
                           _issueDetails.Title = issue.fields.summary;
                            _issueDetails.projectName = issue.fields.project.name;

                        #region Engineer Id

                        if (issue.fields.customfield_11500 != null)
                        {
                        if (!string.IsNullOrEmpty(issue.fields.customfield_11500.emailAddress) && string.Equals(issue.fields.customfield_11500.emailAddress, (issue.fields.assignee != null) ? issue.fields.assignee.emailAddress : null, StringComparison.CurrentCultureIgnoreCase))
                            _issueDetails.EngineerName = issue.fields.customfield_11500.name;
                        else
                            _issueDetails.EngineerName = issue.fields.assignee.name;
                         }

                        #endregion

                                         
                            _issueDetails.workDescription = work.comment;
                            _issueDetails.WorklogAuthor = work.author.name;
                            _issueDetails.worklogDate = Convert.ToDateTime(work.created).Date;
                            _issueDetails.loghours = (!string.IsNullOrEmpty(work.timeSpent)) ? ConvertTimeSpentToHours(work.timeSpent) : 0;
                            _issueDetails.worklogId = Convert.ToInt64(work.id);

                            issueDetailsResults.Add(_issueDetails);
                        }
                        
                    }

                }

                catch (Exception ex)
                {

                }
            }

            return issueDetailsResults;
        }
        static public string GetInputJsonString(string pJQLQuery, string pMaxResults, string pStartResultPosition, bool IsQueryPresent = false, bool isAsset = false)
        {
            List<string> SubFields = new List<string>();

            SubFields.Add("summary");
            SubFields.Add("created");
            SubFields.Add("assignee");
            SubFields.Add("project");
            SubFields.Add("customfield_11500");
            SubFields.Add("worklog");
            SubFields.Add("timetracking");

            var JsonFields = new Dictionary<string, object>();
            JsonFields.Add("jql", pJQLQuery);
            JsonFields.Add("startAt", pStartResultPosition);
            JsonFields.Add("maxResults", pMaxResults);
            JsonFields.Add("fields", SubFields);

            JavaScriptSerializer Serializer = new JavaScriptSerializer();
            string Json = Serializer.Serialize((Object)JsonFields);

            return Json;
        }


        /// <summary>
        /// GetCommonDetails function return all service credential details
        /// </summary>
        static public void GetCommonDetails()
        {
            try
            {
                string XmlPath = string.Empty;
                XmlDocument XmlDoc = new XmlDocument();
                XmlPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"DependentFiles\JIRADetails.xml");
                XmlDoc.Load(XmlPath);

                #region JIRADetails

                XmlNodeList JIRAInitialDetails = XmlDoc.GetElementsByTagName("Credential");

                foreach (XmlNode Details in JIRAInitialDetails)
                {
                    JiraUserName = Details.Attributes["UserName"].Value.Trim();
                    JiraPassword = Details.Attributes["UserPassword"].Value.Trim();
                     
                    break;
                }
                #endregion JIRADetails

            }
            catch (Exception ex)
            {
               
            }
        }
        static public bool UpdateSubscription(string userName, string reportQuery)
        {
            try
            {
                string curFile = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Subscription.xml");
                if (File.Exists(curFile))
                {
                    XElement xEle = XElement.Load(curFile);
                    XElement chile = new XElement("Subscription", string.Empty);
                    xEle.Add(chile);
                    chile.SetAttributeValue("userName", userName);
                    chile.SetAttributeValue("reportQuery", reportQuery);
                    xEle.Save(AppDomain.CurrentDomain.BaseDirectory + "Subscription.xml");
                    return true;
                }
                else
                {

                    XmlDocument xmlDoc = new XmlDocument();
                    XmlNode rootNode = xmlDoc.CreateElement("subscription");
                    xmlDoc.AppendChild(rootNode);
                    xmlDoc.Save(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Subscription.xml"));
                    UpdateSubscription(userName,reportQuery);
                }
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// convert the worklog from minutes to hours.
        /// </summary>
        /// <param name="pTimeSpentDetails"></param>
        /// <returns></returns>
        public static decimal ConvertTimeSpentToHours(string pTimeSpentDetails)
        {
            string[] _timeSpentValue = pTimeSpentDetails.Split(' ');

            decimal _totalTimeSpentInHoursAndMinutes = 0;

            int _timeValue;
            decimal _timeSpentInMinutes = 0, _totalTimeValue = 0;

            foreach (var timelogvalue in _timeSpentValue)
            {
                _timeValue = 0;
                if (!string.IsNullOrEmpty(timelogvalue))
                {
                    if (timelogvalue.EndsWith("w"))
                    {
                        _timeValue = (Convert.ToInt32((timelogvalue.Substring(0, timelogvalue.Length - 1))) * 5 * 8);
                        _totalTimeValue += _timeValue;
                    }

                    else if (timelogvalue.EndsWith("d"))
                    {
                        _timeValue = (Convert.ToInt32((timelogvalue.Substring(0, timelogvalue.Length - 1))) * 8);
                        _totalTimeValue += _timeValue;
                    }

                    else if (timelogvalue.EndsWith("h"))
                    {
                        _timeValue = Convert.ToInt32((timelogvalue.Substring(0, timelogvalue.Length - 1)));
                        _totalTimeValue += _timeValue;
                    }

                    else if (timelogvalue.EndsWith("m"))
                    {
                        int s = Convert.ToInt32(timelogvalue.Substring(0, timelogvalue.Length - 1));
                        decimal f = Decimal.Divide(s, 863);

                        _timeSpentInMinutes = Math.Round((Decimal.Divide(Convert.ToInt32(timelogvalue.Substring(0, timelogvalue.Length - 1)), 60)), 2);
                    }
                }
            }

            if (_timeSpentInMinutes != 0)
                _totalTimeSpentInHoursAndMinutes = Math.Round(_totalTimeValue + _timeSpentInMinutes, 3);
            else
                _totalTimeSpentInHoursAndMinutes = _totalTimeValue;

            return _totalTimeSpentInHoursAndMinutes;
        }
        /// <summary>
        /// This function Check the user is exist in JIRA or not
        /// </summary>
        /// <param name="pUserName"></param>
        /// <param name="pPassword"></param>
        /// <returns></returns>
        static public bool IsValidJIRAUser(string pUserName, string pPassword)
        {
            _client = new HttpClient();
            bool isValidUser = false;
            _credentialString = pUserName + ":" + pPassword;
            _client.DefaultRequestHeaders.ExpectContinue = false;
            _client.Timeout = TimeSpan.FromSeconds(90);
            byte[] Crdential = UTF8Encoding.UTF8.GetBytes(_credentialString);
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(Crdential));
            _client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            try
            {
                _response = _client.GetAsync("https://syncfusion.atlassian.net/rest/api/2/user?username=" + pUserName).Result;
                _response.EnsureSuccessStatusCode();
                if (_response.StatusCode == System.Net.HttpStatusCode.OK)
                    isValidUser = true;
                else
                    isValidUser = false;
            }
            catch (Exception ex)
            {
                isValidUser = false;
            }

            return isValidUser;
        }
    }
}