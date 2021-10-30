using Amazon.SQS.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AWSHelpers.SQS.Interfaces
{
    public interface ISQSHelper
    {
        Task<ListQueuesResponse> ShowQueues(string listQueueName = "");

        Task<CreateQueueResponse> CreateQueue(string queueName, string deadLetterQueueUrl = null, string maxReceiveCount = null, string receiveWaitTime = null);

        Task<string> GetQueueArn(string queueUrl);

        Task<GetQueueAttributesResponse> UpdateAttribute(string queueUrl, string attribute, string value);

        Task<GetQueueAttributesResponse> ShowAllAttributes(string queueUrl);

        Task<DeleteQueueResponse> DeleteQueue(string queueUrl);

        Task<SendMessageResponse> SendMessage(string queueUrl, string messageBody);

        Task<SendMessageBatchResponse> SendMessageBatch(string queueUrl, List<SendMessageBatchRequestEntry> messages);

        Task<PurgeQueueResponse> DeleteAllMessages(string queueUrl);

        Task<DeleteMessageResponse> DeleteMessage(Message message, string queueUrl);

        Task<ReceiveMessageResponse> GetMessage(string queueUrl, int waitTime = 0);
    }
}