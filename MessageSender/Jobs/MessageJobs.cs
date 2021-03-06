﻿using Hangfire;
using MessageSender.Hubs;
using MessageSender.Models;
using MessageSender.SMS;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace MessageSender.Jobs
{
    public class MessageJobs
    {
        private static int jobCount = 0;
        private static IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<JobsHub>();

        [DisableConcurrentExecution(3600)]
        [AutomaticRetry(Attempts = 3)]
        public static void SendScheduledMessages()
        {
            int id = ++jobCount;
            int messages = 1000;
            float progressPercentage = 0;
            for (int sent = 1; sent <= messages; sent++)
            {
                Thread.Sleep(100);
                progressPercentage = (sent / messages) * 100;
                hubContext.Clients.All.updateProgress(id, sent, messages);
            }

            hubContext.Clients.All.removeJob(id);
        }

        [DisableConcurrentExecution(3600)]
        [AutomaticRetry(Attempts = 3)]
        public static void SendBatchMessage(int messageId)
        {
            int id = ++jobCount;
            int progressCount = 0;
            List<BatchMessageRecipient> recipients = new List<BatchMessageRecipient>();
            string message = "";
            string sender = "";
            string serviceId = "";
            int batchId = 0;

            using (var db = new ApplicationDbContext())
            {
                var batchMessage = db.BatchMessages.Find(messageId);

                if (batchMessage != null)
                {
                    // if recipients were uploaded using spreadsheet file
                    if (batchMessage.Recipients.Any())
                    {
                        recipients = batchMessage.Recipients.ToList();
                    }
                    else // recipients already existed in the database as subscribers
                    {
                        var subscribers = db.Subscribers.Where(s => s.isActive && s.ServiceId.Equals(batchMessage.ServiceId)).ToList();
                        recipients = (from r in subscribers
                                         select new BatchMessageRecipient
                                         {
                                             Destination = r.PhoneNumber,
                                             MessageId = batchMessage.Id,
                                             Timestamp = DateTime.Now
                                         }).ToList();
                    }
                    message = batchMessage.MessageContent;
                    sender = batchMessage.Sender;
                    batchId = batchMessage.Id;
                    serviceId = batchMessage.ServiceId;
                }
            }

            var totalMessages = recipients.Count();
            var bundleCount = SMSConfiguration.GetGroupMessageLimit();
            int iterations = (totalMessages / bundleCount) + 1;
            for (int i = 0; i < iterations; i++)
            {
                var massMessage = new MassMessage()
                {
                    Text = message,
                    Sender = sender,
                    Destinations = recipients.Skip(i * bundleCount).Take(bundleCount).Select(r => r.Destination).ToList(),
                    TimeStamp = DateTime.Now.ToString("yyyyMMddHHmmss"),
                    ServiceId = serviceId,
                    Correlator = batchId.ToString("D8") + "s" + i.ToString()
                };

                massMessage.Send();
                progressCount += massMessage.Destinations.Count();
                hubContext.Clients.All.updateProgress(id, progressCount, totalMessages);
            }

            hubContext.Clients.All.removeJob(id);
        }

        
        public static void SendSubscriptionWelcomeMessage(int outboundMessageId)
        {
            using (var db = new ApplicationDbContext())
            {
                var message = db.OutboundMessages.Find(outboundMessageId);

                if (message != null)
                {
                    var subscriptionResponse = new SMSMessage
                    {
                        Correlator = message.Id.ToString("D12"),
                        Destination = message.Destination,
                        Sender = message.Sender,
                        ServiceId = message.ServiceId,
                        Text = message.Text
                    };

                    subscriptionResponse.Send();
                }
            }
        }
    }

}