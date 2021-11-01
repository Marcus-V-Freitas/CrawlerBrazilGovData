using Amazon.SQS;
using Amazon.SQS.Model;
using AWSHelpers.SQS.Interfaces;
using Core.Cache.Interfaces;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace AWSHelpers.SQS.Implementation
{
    public class SQSHelper : ISQSHelper
    {
        private readonly int MaxMessages = 10;
        private readonly IAmazonSQS _amazonSQS;
        private readonly ICacheProvider _cache;

        public SQSHelper(IAmazonSQS amazonSQS, ICacheProvider cache)
        {
            _amazonSQS = amazonSQS;
            _cache = cache;
        }

        private static bool ValidAttribute(string attribute)
        {
            var attOk = false;
            var qAttNameType = typeof(QueueAttributeName);
            List<string> qAttNamefields = new List<string>();
            foreach (var field in qAttNameType.GetFields())
            {
                qAttNamefields.Add(field.Name);
            }
            foreach (var name in qAttNamefields)
            {
                if (attribute == name)
                {
                    attOk = true;
                    break;
                }
            }
            return attOk;
        }

        public async Task<CreateQueueResponse> CreateQueue(string queueName, string deadLetterQueueUrl = null, string maxReceiveCount = null, string receiveWaitTime = null)
        {
            ListQueuesResponse queues = await ShowQueues(queueName);

            if (queues.QueueUrls.Any())
            {
                return new CreateQueueResponse()
                {
                    QueueUrl = queues.QueueUrls.FirstOrDefault(),
                    HttpStatusCode = HttpStatusCode.OK
                };
            }

            var attrs = new Dictionary<string, string>();

            // If a dead-letter queue is given, create a message queue
            if (!string.IsNullOrEmpty(deadLetterQueueUrl))
            {
                attrs.Add(QueueAttributeName.ReceiveMessageWaitTimeSeconds, receiveWaitTime);
                attrs.Add(QueueAttributeName.RedrivePolicy,
                  $"{{\"deadLetterTargetArn\":\"{await GetQueueArn(deadLetterQueueUrl)}\"," +
                  $"\"maxReceiveCount\":\"{maxReceiveCount}\"}}");
                // Add other attributes for the message queue such as VisibilityTimeout
            }
            // If no dead-letter queue is given, create one of those instead
            //else
            //{
            //  // Add attributes for the dead-letter queue as needed
            //  attrs.Add();
            //}

            var queueReponse = await _amazonSQS.CreateQueueAsync(new CreateQueueRequest { QueueName = queueName, Attributes = attrs });

            if (queueReponse.HttpStatusCode == HttpStatusCode.OK)
            {
                await _cache.GetOrCreateAsync(queueName, queueReponse.QueueUrl);
            }

            return queueReponse;
        }

        public async Task<string> GetQueueArn(string queueName)
        {
            GetQueueAttributesResponse responseGetAtt = await _amazonSQS.GetQueueAttributesAsync(queueName, new List<string> { QueueAttributeName.QueueArn });
            return responseGetAtt.QueueARN;
        }

        public async Task<ListQueuesResponse> ShowQueues(string listQueueName = "")
        {
            return await _amazonSQS.ListQueuesAsync(listQueueName);
        }

        public async Task<GetQueueAttributesResponse> UpdateAttribute(string queueName, string attribute, string value)
        {
            string queueUrl = await GetQueueUrl(queueName);
            if (ValidAttribute(attribute) && !string.IsNullOrEmpty(queueUrl))
            {
                await _amazonSQS.SetQueueAttributesAsync(queueUrl, new Dictionary<string, string> { { attribute, value } });
            }
            return await ShowAllAttributes(queueName);
        }

        public async Task<GetQueueAttributesResponse> ShowAllAttributes(string queueName)
        {
            string queueUrl = await GetQueueUrl(queueName);
            if (!string.IsNullOrEmpty(queueUrl))
            {
                return await _amazonSQS.GetQueueAttributesAsync(queueUrl, new List<string> { QueueAttributeName.All });
            }
            return null;
        }

        public async Task<DeleteQueueResponse> DeleteQueue(string queueName)
        {
            string queueUrl = await GetQueueUrl(queueName);
            if (!string.IsNullOrEmpty(queueUrl))
            {
                return await _amazonSQS.DeleteQueueAsync(queueUrl);
            }
            return null;
        }

        public async Task<SendMessageResponse> SendMessage(string queueName, string messageBody)
        {
            string queueUrl = await GetQueueUrl(queueName);
            if (!string.IsNullOrEmpty(queueUrl))
            {
                return await _amazonSQS.SendMessageAsync(queueUrl, messageBody);
            }
            return null;
        }

        public async Task<SendMessageBatchResponse> SendMessageBatch(string queueName, List<SendMessageBatchRequestEntry> messages)
        {
            string queueUrl = await GetQueueUrl(queueName);
            if (!string.IsNullOrEmpty(queueUrl))
            {
                return await _amazonSQS.SendMessageBatchAsync(queueUrl, messages);
            }
            return null;
        }

        public async Task<PurgeQueueResponse> DeleteAllMessages(string queueName)
        {
            string queueUrl = await GetQueueUrl(queueName);
            if (!string.IsNullOrEmpty(queueUrl))
            {
                return await _amazonSQS.PurgeQueueAsync(queueUrl);
            }
            return null;
        }

        public async Task<DeleteMessageResponse> DeleteMessage(Message message, string queueName)
        {
            string queueUrl = await GetQueueUrl(queueName);
            if (!string.IsNullOrEmpty(queueUrl))
            {
                return await _amazonSQS.DeleteMessageAsync(queueUrl, message.ReceiptHandle);
            }
            return null;
        }

        public async Task<ReceiveMessageResponse> GetMessage(string queueName, int waitTime = 0)
        {
            string queueUrl = await GetQueueUrl(queueName);

            if (!string.IsNullOrEmpty(queueUrl))
            {
                return await _amazonSQS.ReceiveMessageAsync(new ReceiveMessageRequest
                {
                    QueueUrl = queueUrl,
                    MaxNumberOfMessages = MaxMessages,
                    WaitTimeSeconds = waitTime,
                    // (Could also request attributes, set visibility timeout, etc.)
                });
            }
            return null;
        }

        public async Task<List<T>> ExtractAndParserListSQSMessages<T>(string queueName, bool deleteAllMessages = false, bool deleteQueue = false)
        {
            List<T> listMessages = new();
            var messages = await GetMessage(queueName);

            if (messages != null && messages.HttpStatusCode == HttpStatusCode.OK && messages.Messages.Any())
            {
                var jsonMessages = messages.Messages.Select(x => x.Body).ToList();
                foreach (var jsonMessage in jsonMessages)
                {
                    List<T> messagesObj = JsonConvert.DeserializeObject<List<T>>(jsonMessage);
                    if (messagesObj != null && messagesObj.Any())
                    {
                        listMessages.AddRange(messagesObj);
                    }
                }

                if (deleteAllMessages)
                {
                    await DeleteAllMessages(queueName);
                }

                if (deleteQueue)
                {
                    await DeleteQueue(queueName);
                }
            }
            return listMessages;
        }

        public async Task<List<T>> ExtractAndParserSQSMessages<T>(string queueName, bool deleteAllMessages = false, bool deleteQueue = false)
        {
            List<T> listMessages = new();
            var messages = await GetMessage(queueName);

            if (messages.HttpStatusCode == HttpStatusCode.OK && messages.Messages.Any())
            {
                var jsonMessages = messages.Messages.Select(x => x.Body).ToList();
                foreach (var jsonMessage in jsonMessages)
                {
                    T messageObj = JsonConvert.DeserializeObject<T>(jsonMessage);
                    if (messageObj != null)
                    {
                        listMessages.Add(messageObj);
                    }
                }

                if (deleteAllMessages)
                {
                    await DeleteAllMessages(queueName);
                }

                if (deleteQueue)
                {
                    await DeleteQueue(queueName);
                }
            }
            return listMessages;
        }

        public async Task<string> GetQueueUrl(string queueName)
        {
            string queueUrl = await _cache.GetOrCreateAsync(queueName);

            if (string.IsNullOrEmpty(queueUrl))
            {
                ListQueuesResponse queues = await ShowQueues(queueName);

                if (queues != null && queues.QueueUrls.Any())
                {
                    queueUrl = queues.QueueUrls.FirstOrDefault();
                    await _cache.GetOrCreateAsync(queueName, queueUrl);
                }
            }
            return queueUrl;
        }
    }
}