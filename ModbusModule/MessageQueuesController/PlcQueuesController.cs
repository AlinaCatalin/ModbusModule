using System;
using System.Collections.Generic;

namespace MessageQueuesController
{
    public class PlcQueuesController
    {
        private Queue<byte[]> plc1Queue = new();
        private Queue<byte[]> plc2Queue = new();
        private Queue<byte[]> plc3Queue = new();
        private Queue<byte[]> plc4Queue = new();


        public delegate void DelegateEventAddBucket(Bucket bucket);

        public event DelegateEventAddBucket? AddBucketToFinalQueueEvent;
        public event DelegateEventAddBucket? AddBucketToType2QueueEvent;

        public PlcQueuesController()
        {
            InitializePlcQueuesWithDummyData();
        }

        public void HandlePlcComm()
        {
            Console.WriteLine($"{DateTime.Now:h:mm:ss:ffff} -> Handling PLC Queues:");

            ProcessPlcQueue(plc1Queue);
            ProcessPlcQueue(plc2Queue);
            ProcessPlcQueue(plc3Queue);
            ProcessPlcQueue(plc4Queue);
        }

        private void ProcessPlcQueue(Queue<byte[]> plcQueue)
        {
            if (plcQueue.Count > 0)
            {
                byte[] messagePlc = plcQueue.Dequeue();

                Bucket bucketPlc = ParseMessage(messagePlc);
                if (bucketPlc.Type == 0)
                {
                    AddBucketToFinalQueueEvent?.Invoke(bucketPlc);
                }
                else
                {
                    AddBucketToType2QueueEvent?.Invoke(bucketPlc);
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
