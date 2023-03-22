using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace MessageQueuesController
{
    public class MessageQueuesComm
    {
        public List<Bucket> ProcessedBuckets { get; private set; } = new();

        private Queue<byte[]> plc1Queue = new();
        private Queue<byte[]> plc2Queue = new();
        private Queue<byte[]> plc3Queue = new();
        private Queue<byte[]> plc4Queue = new();

        private Queue<Bucket> type2MessagesQueue = new();
        private Queue<Bucket> finalMessagesQueue = new();

        private object type2MessagesQueueLock = new();
        private object finalMessagesQueueLock = new();

        private AutoResetEvent plcCommAutoResetEvent = new(false);
        private Timer plcCommTimer;

        private Thread plcCommTimerThread;
        private Thread type2MessagesQueueThread;
        private Thread finalMessagesQueueThread;

        public MessageQueuesComm()
        {
            InitializePlcQueuesWithDummyData();

            plcCommTimer = new(HandlePlcComm, plcCommAutoResetEvent, 0, 100);

            type2MessagesQueueThread = new(new ThreadStart(HandleType2Queue));

            finalMessagesQueueThread = new(new ThreadStart(HandleFinalQueue));

            plcCommTimerThread = new(new ThreadStart(() => plcCommAutoResetEvent.WaitOne()));
        }

        public void Start()
        {
            plcCommTimerThread.Start();
            type2MessagesQueueThread.Start();
            finalMessagesQueueThread.Start();
        }

        public void Stop()
        {
            plcCommAutoResetEvent.Set();
        }

        private void HandlePlcComm(object? state)
        {
            AutoResetEvent autoResetEvent = (AutoResetEvent)state;

            Console.WriteLine($"{DateTime.Now:h:mm:ss:ffff} -> Handling PLC Queues:");

            ProcessPlcQueue(plc1Queue);

            ProcessPlcQueue(plc2Queue);

            ProcessPlcQueue(plc3Queue);

            ProcessPlcQueue(plc4Queue);

        }

        private void HandleType2Queue()
        {
            while(true)
            {
                lock (type2MessagesQueueLock)
                {
                    // TODO: should send the last element to FinalQueue
                    // and verify if it can be processed further
                    if (type2MessagesQueue.Count <= 0)
                        continue;

                    Bucket bucket = type2MessagesQueue.Dequeue();

                    lock (finalMessagesQueueLock)
                    {
                        finalMessagesQueue.Enqueue(bucket);
                    }
                }

                Thread.Sleep(20);
            }
        }

        private void HandleFinalQueue()
        {
            while (true)
            {
                lock (finalMessagesQueueLock)
                {
                    if (finalMessagesQueue.Count <= 0)
                        continue;

                    Bucket bucket = finalMessagesQueue.Dequeue();

                    if (bucket.Type == 0)
                    {
                        // receive Response for the bucket
                        bucket.Rx = new byte[2] { 0x01, 0x01 }; // TODO: create a method for receiving a response

                        // it's a type 1 message, and this means that we have finished the
                        // receive for this bucket and we can send it to the upper layer
                        ProcessedBuckets.Add(bucket);

                        Console.WriteLine($"--- Processed bucket from Final Queue: {bucket}");

                        continue;
                    }

                    // it's a type 2 message, this means that we receive a Response
                    // and verify if it's a different message
                    bool sameMessage = bucket.VerifyMessage();

                    if (sameMessage)
                    {
                        // send back to Type2Queue
                        lock (type2MessagesQueueLock)
                        {
                            type2MessagesQueue.Enqueue(bucket);
                        }
                        Console.WriteLine($"--- Did not receive response: {bucket}");
                    }
                    else
                    {
                        // receive the finish message for that bucket, and send it to the upper layer
                        bucket.Rx = new byte[2] { 0x01, 0x01 };
                        ProcessedBuckets.Add(bucket);
                        Console.WriteLine($"--- Processed bucket from Final Queue: {bucket}");
                    }

                }

                Thread.Sleep(10);
            }
        }

        private void ProcessPlcQueue(Queue<byte[]> plcQueue)
        {  
            if (plcQueue.Count > 0)
            {
                byte[] messagePlc = plcQueue.Dequeue();

                Bucket bucketPlc = ParseMessage(messagePlc);
                if (bucketPlc.Type == 0)
                {
                    lock (finalMessagesQueueLock)
                    {
                        finalMessagesQueue.Enqueue(bucketPlc);
                    }
                }
                else
                {
                    lock (type2MessagesQueueLock)
                    {
                        type2MessagesQueue.Enqueue(bucketPlc);
                    }
                }

                Console.WriteLine($"\tProcessed message: {messagePlc.ConvertToString()}");
            }
        }

        private Bucket ParseMessage(byte[] message)
        {
            Random random = new();

            Bucket bucket = new()
            {
                Tx = new byte[message.Length],
                Type = random.Next(2)
            };

            bucket.Tx = message;

            return bucket;
        }

        private void InitializePlcQueuesWithDummyData()
        {
            plc1Queue.Enqueue(new byte[] { 0x01, 0x05, 0x00, 0x10, 0xFF, 0x00, 0x8D, 0xD8 });
            plc1Queue.Enqueue(new byte[] { 0x01, 0x05, 0x10, 0x10, 0xFF, 0x00, 0x8D, 0xD8 });
            plc1Queue.Enqueue(new byte[] { 0x01, 0x05, 0x11, 0x00, 0xFF, 0x00, 0x8D, 0xD8 });
            plc1Queue.Enqueue(new byte[] { 0x01, 0x05, 0xA0, 0xFF, 0xFF, 0x00, 0x8D, 0xD8 });

            plc2Queue.Enqueue(new byte[] { 0x02, 0x03, 0x00, 0x00, 0x00, 0x01, 0x85, 0xE8 });
            plc2Queue.Enqueue(new byte[] { 0x02, 0x03, 0x00, 0x00, 0x00, 0x01, 0x90, 0xB5 });

            plc3Queue.Enqueue(new byte[] { 0x03, 0x06, 0x00, 0x05, 0x11, 0x22, 0x14, 0x60 });
            plc3Queue.Enqueue(new byte[] { 0x03, 0x06, 0x00, 0x05, 0x11, 0x22, 0x15, 0x60 });
            plc3Queue.Enqueue(new byte[] { 0x03, 0x06, 0x00, 0x05, 0x11, 0x22, 0x16, 0x60 });

            plc4Queue.Enqueue(new byte[] { 0x04, 0x05, 0xA0, 0xFF, 0xFF, 0x00, 0x8D, 0xD8 });
            plc4Queue.Enqueue(new byte[] { 0x04, 0x03, 0x00, 0x00, 0x00, 0x01, 0x90, 0xB5 });
            plc4Queue.Enqueue(new byte[] { 0x04, 0x06, 0x00, 0x05, 0x11, 0x22, 0x16, 0x60 });
        }
    }
}
