using System.Net.Http;
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Amazon.Lambda.Core;
using Newtonsoft.Json;
using M2Mqtt.Net;
using M2Mqtt;
// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Hausautomation
{
    public class Function
    {

        public const string INVOCATION_NAME = "Hausautomation";

        public List<FactResource> GetResources()
        {
            List<FactResource> resources = new List<FactResource>();
            FactResource enUSResource = new FactResource("en-US");
            enUSResource.SkillName = "American Science Facts";
            enUSResource.GetFactMessage = "Here's your science fact: ";
            enUSResource.HelpMessage = "You can say tell me a science fact, or, you can say exit... What can I help you with?";
            enUSResource.HelpReprompt = "You can say tell me a science fact to start";
            enUSResource.StopMessage = "Goodbye!";
            enUSResource.Facts.Add("A year on Mercury is just 88 days long.");
            enUSResource.Facts.Add("Despite being farther from the Sun, Venus experiences higher temperatures than Mercury.");
            enUSResource.Facts.Add("Venus rotates counter-clockwise, possibly because of a collision in the past with an asteroid.");
            enUSResource.Facts.Add("On Mars, the Sun appears about half the size as it does on Earth.");
            enUSResource.Facts.Add("Earth is the only planet not named after a god.");
            enUSResource.Facts.Add("Jupiter has the shortest day of all the planets.");
            enUSResource.Facts.Add("The Milky Way galaxy will collide with the Andromeda Galaxy in about 5 billion years.");
            enUSResource.Facts.Add("The Sun contains 99.86% of the mass in the Solar System.");
            enUSResource.Facts.Add("The Sun is an almost perfect sphere.");
            enUSResource.Facts.Add("A total solar eclipse can happen once every 1 to 2 years. This makes them a rare event.");
            enUSResource.Facts.Add("Saturn radiates two and a half times more energy into space than it receives from the sun.");
            enUSResource.Facts.Add("The temperature inside the Sun can reach 15 million degrees Celsius.");
            enUSResource.Facts.Add("The Moon is moving approximately 3.8 cm away from our planet every year.");

            resources.Add(enUSResource);
            return resources;
        }
        public SkillResponse FunctionHandler(SkillRequest input, ILambdaContext context)
        {
            SkillResponse response = new SkillResponse();
            response.Response = new ResponseBody();
            response.Response.ShouldEndSession = false;
            IOutputSpeech innerResponse = null;

            var log = context.Logger;
            var allResources = GetResources();
            var resource = allResources.FirstOrDefault();

            if (input.GetRequestType() == typeof(LaunchRequest))
            {
                log.LogLine($"Default LaunchRequest made: 'Alexa, open Science Facts");
                innerResponse = new PlainTextOutputSpeech();
                (innerResponse as PlainTextOutputSpeech).Text = "Guten Tag";

            }
            else if (input.GetRequestType() == typeof(IntentRequest))
            {
                var intentRequest = (IntentRequest)input.Request;

                switch (intentRequest.Intent.Name)
                {
                    case "AMAZON.CancelIntent":
                        log.LogLine($"AMAZON.CancelIntent: send StopMessage");
                        innerResponse = new PlainTextOutputSpeech();
                        (innerResponse as PlainTextOutputSpeech).Text = resource.StopMessage;
                        response.Response.ShouldEndSession = true;
                        break;
                    case "AMAZON.StopIntent":
                        log.LogLine($"AMAZON.StopIntent: send StopMessage");
                        innerResponse = new PlainTextOutputSpeech();
                        (innerResponse as PlainTextOutputSpeech).Text = resource.StopMessage;
                        response.Response.ShouldEndSession = true;
                        break;
                    case "AMAZON.HelpIntent":
                        log.LogLine($"AMAZON.HelpIntent: send HelpMessage");
                        innerResponse = new PlainTextOutputSpeech();
                        (innerResponse as PlainTextOutputSpeech).Text = resource.HelpMessage;
                        break;
                    case "Lampe":
                        log.LogLine($"GetFactIntent sent: send new fact");
                        innerResponse = new PlainTextOutputSpeech();
                        (innerResponse as PlainTextOutputSpeech).Text = MQTTClient();
                        break;
                    case "GetNewFactIntent":
                        log.LogLine($"GetFactIntent sent: send new fact");
                        innerResponse = new PlainTextOutputSpeech();
                        //(innerResponse as PlainTextOutputSpeech).Text = emitNewFact(resource, false);
                        break;
                    default:
                        log.LogLine($"Unknown intent: " + intentRequest.Intent.Name);
                        innerResponse = new PlainTextOutputSpeech();
                        (innerResponse as PlainTextOutputSpeech).Text = resource.HelpReprompt;
                        break;
                }                
            }
            response.Response.OutputSpeech = innerResponse;
            response.Version = "1.0";
            log.LogLine($"Skill Response Object...");
            log.LogLine(JsonConvert.SerializeObject(response));
            return response;
        }

        public string MQTTClient()
        {
            MqttClient client = new MqttClient("broker.hivemq.com");
            string clientId = Guid.NewGuid().ToString();
            client.Connect(clientId);
            string strValue = "test123";
            client.Publish("hausautomation/in/", Encoding.UTF8.GetBytes(strValue));
            //client.Disconnect();
            return "Test erfolgreich";
        }

        private SkillResponse MakeSkillResponse(string outputSpeech,
            bool shouldEndSession,
            string repromptText = "Just say, tell me about Canada to learn more. To exit, say, exit.")
        {
            var response = new ResponseBody
            {
                ShouldEndSession = shouldEndSession,
                OutputSpeech = new PlainTextOutputSpeech { Text = outputSpeech }
            };

            if (repromptText != null)
            {
                response.Reprompt = new Reprompt() { OutputSpeech = new PlainTextOutputSpeech() { Text = repromptText } };
            }

            var skillResponse = new SkillResponse
            {
                Response = response,
                Version = "1.0"
            };
            return skillResponse;
        }
    }
    public class FactResource
    {
        public FactResource(string language)
        {
            this.Language = language;
            this.Facts = new List<string>();
        }

        public string Language { get; set; }
        public string SkillName { get; set; }
        public List<string> Facts { get; set; }
        public string GetFactMessage { get; set; }
        public string HelpMessage { get; set; }
        public string HelpReprompt { get; set; }
        public string StopMessage { get; set; }
    }
}