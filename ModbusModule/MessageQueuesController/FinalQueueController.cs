using System;
using System.Collections.Generic;
using System.Threading;

namespace MessageQueuesController
{
    public class FinalQueueController
    {
        private Queue<Bucket> finalMessagesQueue = new();

        public delegate void DelegateEventHandlerProcessMessage();
        public delegate void DelegateEventAddBucket(Bucket bucket);

        public event DelegateEventHandlerProcessMessage? ProcessMessage;
        public event DelegateEventAddBucket? AddBucketToProcessedBucketsList;
        public event DelegateEventAddBucket? AddBucketToType2QueueEvent;

        private Thread finalMessagesQueueThread;

        public FinalQueueController()
        {
            finalMessagesQueueThread = new(new ThreadStart(HandleFinalQueue));
        }

        public void Start()
        {
            finalMessagesQueueThread.Start();
        }

        public void AddBucket(Bucket bucket) => finalMessagesQueue.Enqueue(bucket);

        private void HandleFinalQueue()
        {
            while (true)
            {
                if (finalMessagesQueue.Count <= 0)
                {
                    ProcessMessage?.Invoke();
                    continue;
                }

                Bucket bucket = finalMessagesQueue.Dequeue();

                if (bucket.Type == 0)
                {
                    // receive Response for the bucket
                    bucket.Rx = new byte[2] { 0x01, 0x01 }; // TODO: create a method for receiving a response

                    // it's a type 1 message, and this means that we have finished the
                    // receive for this bucket and we can send it to the upper layer
                    AddBucketToProcessedBucketsList?.Invoke(bucket);
                    //ProcessMessage?.Invoke();

                    Console.WriteLine($"--- Processed bucket from Final Queue: {bucket}");

                    continue;
                }

                // it's a type 2 message, this means that we receive a Response
                // and verify if it's a different message
                bool sameMessage = bucket.VerifyMessage();

                if (sameMessage)
                {
                    // send back to Type2Queue
                    AddBucketToType2QueueEvent?.Invoke(bucket);

                    Console.WriteLine($"--- Did not receive response: {bucket}");
                }
                else
                {
                    // receive the finish message for that bucket, and send it to the upper layer
                    bucket.Rx = new byte[2] { 0x01, 0x01 };

                    AddBucketToProcessedBucketsList?.Invoke(bucket);
                    //ProcessMessage?.Invoke();

                    Console.WriteLine($"--- Processed bucket from Final Queue: {bucket}");
                }

            }
        }
    }
}
