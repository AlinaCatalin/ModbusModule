using ModbusLib.Interface;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace MessageQueuesController
{
    public class MessageProcessor : IObserver
    {
        public delegate void DelegateAddBucketQueueEvent(Bucket bucket);
        public delegate Bucket DelegateGetBucketEvent();
        public delegate void DelegateNotiifyEvent();

        public event DelegateAddBucketQueueEvent AddBucketToType2QueueEvent;
        public event DelegateGetBucketEvent GetCurrentBucketInProcessingEvent;
        public event DelegateNotiifyEvent MessageForCurrentBucketReceivedEvent;

        private ConcurrentQueue<Bucket> _receivedMessagesQueue = new();

        private Thread _processMessageThread;

        public MessageProcessor()
        {
            _processMessageThread = new Thread(ProcessMessage);
        }

        public void Start() => _processMessageThread.Start();

        private void ProcessMessage()
        {
            while (true)
            {
                var status = _receivedMessagesQueue.TryDequeue(out Bucket localBucket);
                if (!status)
                    continue;
                
                // verify if the bucket has finished the reception of Rx message
                // if the reception has been finished, move it to the outer layer of the application
                // otherwise, move it back to Type2MessagesQueue from PlcQueuesController

                if (localBucket.VerifyMessage())
                {
                    // received response for bucket, finished work with it
                    // TODO: send bucket to outer layer
#if DEBUG
                    Console.WriteLine($"MessageProcessor --- Finished work with bucket: {localBucket}");
#endif
                }
                else
                {
                    // did not receive response for bucket, send it back to type2MessagesQueue
                    AddBucketToType2QueueEvent.Invoke(localBucket);
#if DEBUG
                    Console.WriteLine($"MessageProcessor --- Readded bucket to Type2Queue: {localBucket}");
#endif
                }
            }
        }

        public void Notify(byte[] message)
        {
#if DEBUG
            Console.WriteLine($"MessageProcessor --- Data received: {BitConverter.ToString(message)}");
#endif

            // complete the Rx field of the currentBucket in process and add it to receivedMessagesQueue
            Bucket currentBucket = GetCurrentBucketInProcessingEvent.Invoke();
            currentBucket.Rx = new byte[message.Length];
            currentBucket.Rx = message;

            _receivedMessagesQueue.Enqueue(currentBucket);

            // release the semaphore on the MessageSender
            MessageForCurrentBucketReceivedEvent.Invoke();
        }
    }
}
