using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TimeSheet.Models
{
    public class JIRAObjects
    {
    }
    public class JiraIssueDetailsResult
    {
        public List<JIRAIssueDetails> TaskList = new List<JIRAIssueDetails>();
        public bool IsSucessResult = true;
        public string ExceptionMessage = string.Empty;
    }
    public class JIRAIssueDetails
    {
        public string JIRAId { get; set; }
        public string Title { get; set; }
        public string projectName { get; set; }
        public string EngineerName { get; set; }
        public string WorklogAuthor { get; set; }
        public decimal loghours { get; set; }
        public DateTime worklogDate { get; set; }
        public string workDescription { get; set; }
        public Worklog worklog { get; set; }
        public long worklogId { get; set; }
    }
    public class Worklog
    {
        public int startAt { get; set; }
        public int maxResults { get; set; }
        public int total { get; set; }
        public List<worklogs> worklogs { get; set; }
    }

    public class worklogs
    {
        public string comment { get; set; }
        public string timeSpent { get; set; }
        public JIRA_User author { get; set; }
        public JIRA_User updateAuthor { get; set; }
        public object created { get; set; }
        public object updated { get; set; }
        public object started { get; set; }
        public string id { get; set; }
    }
    public class JiraFieldJson
    {
        public List<JiraFieldIssue> issues;
        public Int32 maxResults;
        public Int32 startAt;
        public Int32 total;
    }
    public class JiraFieldIssue
    {
        public string key;
        public JiraIssuesField fields;
    }
    public class Issuetype
    {
        public string self { get; set; }
        public int id { get; set; }
        public string description { get; set; }
        public string iconUrl { get; set; }
        public string name { get; set; }
        public bool subtask { get; set; }
    }
    public class JiraIssuesField
    {
        public string key { get; set; }
        public string summary { get; set; }
        public string description { get; set; }
        public Projects project { get; set; }
        public JIRA_User assignee { get; set; }
        public JIRA_User customfield_11500 { get; set; }
        public object timespent { get; set; }
        public Worklog worklog { get; set; }

    }
    public class Projects
    {
        public string name { get; set; }
    }

    public class JIRA_User
    {
        public string name { get; set; }
        public string emailAddress { get; set; }
        public string displayName { get; set; }
    }
}