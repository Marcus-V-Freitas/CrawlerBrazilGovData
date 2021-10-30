using Amazon.SQS;
using Amazon.SQS.Model;
using AWSHelpers.SQS.Interfaces;
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

        public SQSHelper(IAmazonSQS amazonSQS)
        {
            _amazonSQS = amazonSQS;
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
                    HttpStatusCode=HttpStatusCode.OK                    
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

            return await _amazonSQS.CreateQueueAsync(new CreateQueueRequest { QueueName = queueName, Attributes = attrs });
        }

        public async Task<string> GetQueueArn(string queueUrl)
        {
            GetQueueAttributesResponse responseGetAtt = await _amazonSQS.GetQueueAttributesAsync(queueUrl, new List<string> { QueueAttributeName.QueueArn });
            return responseGetAtt.QueueARN;
        }

        public async Task<ListQueuesResponse> ShowQueues(string listQueueName = "")
        {
            return await _amazonSQS.ListQueuesAsync(listQueueName);
        }

        public async Task<GetQueueAttributesResponse> UpdateAttribute(string queueUrl, string attribute, string value)
        {
            if (ValidAttribute(attribute))
            {
                await _amazonSQS.SetQueueAttributesAsync(queueUrl, new Dictionary<string, string> { { attribute, value } });
            }
            return await ShowAllAttributes(queueUrl);
        }

        public async Task<GetQueueAttributesResponse> ShowAllAttributes(string queueUrl)
        {
            return await _amazonSQS.GetQueueAttributesAsync(queueUrl, new List<string> { QueueAttributeName.All });
        }

        public async Task<DeleteQueueResponse> DeleteQueue(string queueUrl)
        {
            return await _amazonSQS.DeleteQueueAsync(queueUrl);
        }

        public async Task<SendMessageResponse> SendMessage(string queueUrl, string messageBody)
        {
            return await _amazonSQS.SendMessageAsync(queueUrl, messageBody);
        }

        public async Task<SendMessageBatchResponse> SendMessageBatch(string queueUrl, List<SendMessageBatchRequestEntry> messages)
        {
            return await _amazonSQS.SendMessageBatchAsync(queueUrl, messages);
        }

        public async Task<PurgeQueueResponse> DeleteAllMessages(string queueUrl)
        {
            return await _amazonSQS.PurgeQueueAsync(queueUrl);
        }

        public async Task<DeleteMessageResponse> DeleteMessage(Message message, string queueUrl)
        {
            return await _amazonSQS.DeleteMessageAsync(queueUrl, message.ReceiptHandle);
        }

        public async Task<ReceiveMessageResponse> GetMessage(string queueUrl, int waitTime = 0)
        {
            return await _amazonSQS.ReceiveMessageAsync(new ReceiveMessageRequest
            {
                QueueUrl = queueUrl,
                MaxNumberOfMessages = MaxMessages,
                WaitTimeSeconds = waitTime
                // (Could also request attributes, set visibility timeout, etc.)
            });
        }
    }
}