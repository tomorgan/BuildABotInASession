using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bot_Application9.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            //work out what the user said
            var uri = "https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/b7b57375-1f93-484d-8319-2a227eada9c3?subscription-key=92928868a7ea4d2c82de85b2b53f7286&verbose=true&timezoneOffset=0&q=" + activity.Text;
            WebRequest req = WebRequest.Create(uri);
            WebResponse response = req.GetResponse();
            var intent = ConvertJsonToIntentResponse(response);
            string entity = "";
            if (intent.Entities.Count > 0)
            {
                entity = intent.Entities.First();
            }

            var sb = new StringBuilder();
            var sessions = new SessionListAPI();
                     

            if (intent.Intent == "NextSession")
            {
                var sessionList = sessions.GetNextSession(entity);
               
                foreach (var session in sessionList)
                {
                    sb.Append($"The next session in the {session.Track} is {session.Title} with {session.Author} in {session.Location}. ");
                }
            }
            else if (intent.Intent == "SessionsToday")
            {
                if (entity == "")
                {
                    entity = "all";
                }
                var todaysSessions = sessions.GetTodaysSession(entity);
                foreach (var session in todaysSessions)
                {
                    sb.Append($"At {session.StartTime.ToShortTimeString()} it's {session.Title} with {session.Author}. ");
                }
            }
            else
            {
                sb.Append("I don't know what you're talking about");
            }



            // return our reply to the user
            await context.PostAsync(sb.ToString());

            context.Wait(MessageReceivedAsync);
        }

        private IntentResponse ConvertJsonToIntentResponse(WebResponse queryResponse)
        {
            using (var reader = new StreamReader(queryResponse.GetResponseStream()))
            {
                JToken token = JObject.Parse(reader.ReadToEnd());
                var topScoringIntent = token.SelectToken("topScoringIntent");
                if (Convert.ToDouble(topScoringIntent.SelectToken("score")) > 0.6)
                {
                    var response = new IntentResponse()
                    {
                        IntentMatched = true,
                        Intent = topScoringIntent.SelectToken("intent").ToString()
                    };
                    response.Entities = new List<string>();
                    foreach (var entity in token.SelectTokens("entities"))
                    {
                        if (entity.Count() > 0)
                        {
                           // if (Convert.ToDouble(entity.First.SelectToken("score")) > 0.5)
                            //{
                                response.Entities.Add(entity.First.SelectToken("entity").ToString());
                            //}
                        }
                    }
                    return response;
                }
                else
                {
                    return new IntentResponse() { IntentMatched = false, Intent = "", Entities = new List<string>() };
                }
            }
        }

        public class IntentResponse
        {
            public bool IntentMatched { get; set; }
            public string Intent { get; set; }
            public List<string> Entities { get; set; }
        }

    }

}
