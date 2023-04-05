using ModbusLib.Interface;
using System.Collections.Generic;

namespace MessageQueuesController
{
    public class MessageQueuesFacade
    {
        private PlcQueuesController _plcQueuesController;
        private MessageProcessor _messageProcessor;
        private MessageQueuesSender _messageQueuesSender;

        public MessageQueuesFacade()
        {
            _plcQueuesController = new PlcQueuesController();
            _messageProcessor = new MessageProcessor();
            List <IObserver> observers = new() { _messageProcessor };
            _messageQueuesSender = new MessageQueuesSender(observers, 30);

            _plcQueuesController.AddBucketToFinalQueueEvent += _messageQueuesSender.AddBucket;
            _messageProcessor.AddBucketToType2QueueEvent += _plcQueuesController.AddBucketToType2MessagesQueue;
            _messageProcessor.GetCurrentBucketInProcessingEvent += _messageQueuesSender.GetCurrentBucket;
            _messageProcessor.MessageForCurrentBucketReceivedEvent += _messageQueuesSender.ReleaseSendSemaphore;
        }

        public void Start()
        {
            _plcQueuesController.Start();
            _messageQueuesSender.Start();
            _messageProcessor.Start();
        }


    }
}
