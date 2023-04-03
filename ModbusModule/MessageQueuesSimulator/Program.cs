using MessageQueuesController;
using ModbusLib.Interface;
using System.Collections.Generic;

namespace MessageQueuesSimulator
{
    class Program 
    {
        static void Main(string[] args) 
        {
            PlcQueuesController plcQueuesController = new();

            MessageProcessor messageProcessor = new();

            List<IObserver> observers = new() { messageProcessor };

            MessageQueuesSender messageQueuesSender = new(observers);

            plcQueuesController.AddBucketToFinalQueueEvent += messageQueuesSender.AddBucket;

            messageProcessor.AddBucketToType2QueueEvent += plcQueuesController.AddBucketToType2MessagesQueue;
            messageProcessor.GetCurrentBucketInProcessingEvent += messageQueuesSender.GetCurrentBucket;
            messageProcessor.MessageForCurrentBucketReceivedEvent += messageQueuesSender.ReleaseSendSemaphore;

            plcQueuesController.Start();
            messageQueuesSender.Start();
            messageProcessor.Start();
        }
    }
}