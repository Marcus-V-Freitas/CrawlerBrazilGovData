using Amazon.SQS.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AWSHelpers.SQS.Interfaces
{
    public interface ISQSHelper
    {
        Task<ListQueuesResponse> ShowQueues(string listQueueName = "");

        Task<CreateQueueResponse> CreateQueue(string queueName, string deadLetterQueueUrl = null, string maxReceiveCount = null, string receiveWaitTime = null);

        Task<string> GetQueueArn(string queueName);

        Task<GetQueueAttributesResponse> UpdateAttribute(string queueName, string attribute, string value);

        Task<GetQueueAttributesResponse> ShowAllAttributes(string queueName);

        Task<DeleteQueueResponse> DeleteQueue(string queueName);

        Task<SendMessageResponse> SendMessage(string queueName, string messageBody);

        Task<SendMessageBatchResponse> SendMessageBatch(string queueName, List<SendMessageBatchRequestEntry> messages);

        Task<PurgeQueueResponse> DeleteAllMessages(string queueName);

        Task<DeleteMessageResponse> DeleteMessage(Message message, string queueName);

        Task<ReceiveMessageResponse> GetMessage(string queueName, int waitTime = 0);

        Task<string> GetQueueUrl(string queueName);

        Task<List<T>> ExtractAndParserListSQSMessages<T>(string queueName);

        Task<List<T>> ExtractAndParserSQSMessages<T>(string queueName);
    }
}