using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace Bot_Application9
{
    public class SessionListAPI
    {
        private string nextSessionURL = "http://ucdaysessionapi.azurewebsites.net/api/onnext";
        private string todaySessionsURL = "http://ucdaysessionapi.azurewebsites.net/api/today";

        public IEnumerable<Session> GetNextSession(string track)
        {
            return GetSessions(nextSessionURL, track);
        }

        public IEnumerable<Session> GetTodaysSession(string track)
        {
            return GetSessions(todaySessionsURL, track);
        }

        private IEnumerable<Session> GetSessions(string baseUrl, string track)
        {
            var url = baseUrl;
            if (!String.IsNullOrEmpty(track)) url = url + "?track=" + track;
            WebRequest request = WebRequest.Create(url);
            WebResponse response = request.GetResponse();
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                var json = reader.ReadToEnd();
                var sessions = JsonConvert.DeserializeObject<IEnumerable<Session>>(json);
                return sessions;
            }
        }


    }

    public class Session
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public DateTime StartTime { get; set; }
        public int LengthMins { get; set; }
        public string Location { get; set; }
        public string Track { get; set; }

    }
}