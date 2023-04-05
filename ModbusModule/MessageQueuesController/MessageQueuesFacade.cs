using ModbusLib.Interface;
using System.Collections.Generic;
using System.Diagnostics;
using System;

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
            Process currentProcess = Process.GetCurrentProcess();
            ProcessThreadCollection appThreads = currentProcess.Threads;
            int initialThreadsCount = appThreads.Count;
            Console.WriteLine($"initial threads: {initialThreadsCount}");
            for (int i = 0; i < appThreads.Count; i++)
            {
                Console.WriteLine($"\tThread[{i}]: {appThreads[i].Id}");
            }

            _plcQueuesController.Start();
            _messageQueuesSender.Start();
            _messageProcessor.Start();

            currentProcess.Refresh();
            appThreads = currentProcess.Threads;
            int finalThreadsCount = appThreads.Count;
            Console.WriteLine($"final threads: {finalThreadsCount}");
            for (int i = 0; i < appThreads.Count; i++)
            {
                Console.WriteLine($"\tThread[{i}]: {appThreads[i].Id}");
            }

            for (int i = 0; i < finalThreadsCount - initialThreadsCount; i++)
            {
                currentProcess.Threads[i + initialThreadsCount].ProcessorAffinity = (IntPtr)(1 << i);
                Console.WriteLine($"thread {i + initialThreadsCount} set to run on core: {i}");
            }
        }


    }
}
