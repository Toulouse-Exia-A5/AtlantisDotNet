﻿using Atlantis.RawMetrics.DAL;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebLogic.Messaging;

namespace DeviceMessagingService
{
    public partial class DeviceMessaging : ServiceBase
    {

        static AutoResetEvent autoEvent = new AutoResetEvent(false);

        
        private string topicHost;
        private string topicPort;
        private string connectionFactoryName;
        private string topicName;

        private bool stop;

        EventLog log;

        HttpClient httpClient;

        public DeviceMessaging()
        {
            InitializeComponent();
            log = new EventLog();
            if (!System.Diagnostics.EventLog.SourceExists(ConfigurationManager.AppSettings["EventLogSource"]))
            {
                System.Diagnostics.EventLog.CreateEventSource(
                    ConfigurationManager.AppSettings["EventLogSource"], ConfigurationManager.AppSettings["EventLogName"]);
            }
            log.Source = ConfigurationManager.AppSettings["EventLogSource"];
            log.Log = ConfigurationManager.AppSettings["EventLogName"];

            topicHost = ConfigurationManager.AppSettings["topicHost"];
            topicPort = ConfigurationManager.AppSettings["topicPort"];
            connectionFactoryName = ConfigurationManager.AppSettings["connectionFactoryName"];
            topicName = ConfigurationManager.AppSettings["topicName"];

            IDictionary<string, Object> paramMap = new Dictionary<string, Object>();

            paramMap[Constants.Context.PROVIDER_URL] =
              "t3://" + topicHost + ":" + topicPort;

            IContext context = ContextFactory.CreateContext(paramMap);


            IConnectionFactory cf = context.LookupConnectionFactory(connectionFactoryName);

            IQueue queue = (IQueue)context.LookupDestination(topicName);

            IConnection connection = cf.CreateConnection();

            connection.ClientID = "MetricService";

            connection.Start();
            ISession consumerSession = connection.CreateSession(
                                Constants.SessionMode.AUTO_ACKNOWLEDGE);

            IMessageConsumer consumer = consumerSession.CreateConsumer(queue);

            consumer.Message += new MessageEventHandler(OnMessage);



            autoEvent.WaitOne();

            httpClient = new HttpClient();
            httpClient.BaseAddress=new Uri(ConfigurationManager.AppSettings["DeviceMessagingIP"]);
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

        }

        private void OnMessage(IMessageConsumer sender, MessageEventArgs args)
        {
            log.WriteEntry("Message received :" + args.Message.ToString());

            httpClient.PostAsync(httpClient.BaseAddress, new StringContent(args.Message.ToString())).Wait();
        }

        protected override void OnStart(string[] args)
        {
            log.WriteEntry("Service started");

        }

        protected override void OnStop()
        {
            log.WriteEntry("Service stopped");
            autoEvent.Set();
        }

    }
}
