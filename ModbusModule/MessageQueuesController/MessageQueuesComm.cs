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


        private AutoResetEvent plcCommAutoResetEvent = new(false);
        private Timer plcCommTimer;

        private AutoResetEvent type2QueueAutoResetEvent = new(false);
        private Timer type2QueueTimer;

        private AutoResetEvent finalQueueAutoResetEvent = new(false);
        private Timer finalQueueTimer;

        public MessageQueuesComm()
        {
            InitializePlcQueuesWithDummyData();

            plcCommTimer = new(HandlePlcComm, plcCommAutoResetEvent, 0, 100);
            type2QueueTimer = new(HandleType2Queue, type2QueueAutoResetEvent, 0, 100);
            finalQueueTimer = new(HandleFinalQueue, finalQueueAutoResetEvent, 0, 100);
        }

        public void Start()
        {
            plcCommAutoResetEvent.WaitOne();
            type2QueueAutoResetEvent.WaitOne();
            finalQueueAutoResetEvent.WaitOne();
        }

        public void Stop()
        {
            plcCommAutoResetEvent.Set();
            type2QueueAutoResetEvent.Set();
            finalQueueAutoResetEvent.Set();
        }

        private void HandlePlcComm(object? state)
        {
            AutoResetEvent autoResetEvent = (AutoResetEvent)state;

            Console.WriteLine($"{DateTime.Now:h:mm:ss:ffff} -> Handling PLC Queues:");

            if (plc1Queue.Count > 0)
            {
                byte[] messagePlc = plc1Queue.Dequeue();

                Bucket bucketPlc = ParseMessage(messagePlc);
                if (bucketPlc.Type == 0)
                {
                    finalMessagesQueue.Enqueue(bucketPlc);
                }
                else
                {
                    type2MessagesQueue.Enqueue(bucketPlc);
                }

                Console.WriteLine($"\tProcessed message: {messagePlc}");
            }
            if (plc2Queue.Count > 0)
            {
                byte[] messagePlc = plc2Queue.Dequeue();

                Bucket bucketPlc = ParseMessage(messagePlc);
                if (bucketPlc.Type == 0)
                {
                    finalMessagesQueue.Enqueue(bucketPlc);
                }
                else
                {
                    type2MessagesQueue.Enqueue(bucketPlc);
                }

                Console.WriteLine($"\tProcessed message: {messagePlc}");
            }
            if (plc3Queue.Count > 0)
            {
                byte[] messagePlc = plc3Queue.Dequeue();

                Bucket bucketPlc = ParseMessage(messagePlc);
                if (bucketPlc.Type == 0)
                {
                    finalMessagesQueue.Enqueue(bucketPlc);
                }
                else
                {
                    type2MessagesQueue.Enqueue(bucketPlc);
                }

                Console.WriteLine($"\tProcessed message: {messagePlc}");
            }
            if (plc4Queue.Count > 0)
            {
                byte[] messagePlc = plc4Queue.Dequeue();

                Bucket bucketPlc = ParseMessage(messagePlc);
                if (bucketPlc.Type == 0)
                {
                    finalMessagesQueue.Enqueue(bucketPlc);
                }
                else
                {
                    type2MessagesQueue.Enqueue(bucketPlc);
                }

                Console.WriteLine($"\tProcessed message: {messagePlc}");
            }

        }

        private void HandleType2Queue(object? state)
        {
            // TODO: should send all the elements to FinalQueue and verify if it can be processed further
            foreach (var bucket in type2MessagesQueue.AsEnumerable())
            {
                finalMessagesQueue.Enqueue(bucket);
            }
        }

        private void HandleFinalQueue(object? state)
        {
            if (finalMessagesQueue.Count <= 0)
                return;

            Bucket bucket = finalMessagesQueue.Dequeue();

            if (bucket.Type != 0)
            {
                // receive Response and verify if it's a different message
                // TODO: method to check the message
                bool sameMessage = bucket.VerifyMessage();

                if (sameMessage)
                {
                    // send back to Type2Queue
                    type2MessagesQueue.Enqueue(bucket);
                }
                else
                {
                    // receive the finish message for that bucket, and send it to the upper layer
                    // TODO: update bucket Rx with the message received
                    ProcessedBuckets.Add(bucket);
                }

                Console.WriteLine($"Processed bucket with type 2 message from Final Queue: {bucket}");
                return;
            }

            // receive Response for the bucket
            bucket.Rx = new byte[2] { 0x00, 0x00 };

            // it's a type 1 message, and this means that we have finished the receive for this bucket and we can send it to the upper layer
            ProcessedBuckets.Add(bucket);

            Console.WriteLine($"Processed bucket with type 2 message from Final Queue: {bucket}");
        }

        private Bucket ParseMessage(byte[] message)
        {
            Random random = new();

            Bucket bucket = new()
            {
                Tx = new byte[message.Length],
                Type = random.Next(2)
            };

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
