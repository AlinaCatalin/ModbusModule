using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MessageQueuesController
{
    internal class PlcQueuesController
    {
        public delegate void DelegateAddBucketQueueEvent(Bucket bucket);

        public event DelegateAddBucketQueueEvent AddBucketToFinalQueueEvent;

        private ConcurrentQueue<byte[]> _plc1Queue = new();
        private ConcurrentQueue<byte[]> _plc2Queue = new();
        private ConcurrentQueue<byte[]> _plc3Queue = new();
        private ConcurrentQueue<byte[]> _plc4Queue = new();
        private ConcurrentQueue<Bucket> _type2MessagesQueue = new();

        private Timer _plcCommTimer;
        private AutoResetEvent _plcCommAutoResetEvent = new(false);

        private Thread _timerThread;

        public PlcQueuesController()
        {
            InitializePlcQueuesWithDummyData();
        }

        public void Start()
        {
            _plcCommTimer = new(HandlePlcComm, _plcCommAutoResetEvent, 0, 100);
            _timerThread = new Thread(() => { _plcCommAutoResetEvent.WaitOne(); });
            _timerThread.Start();
        }

        public void AddBucketToType2MessagesQueue(Bucket bucket) => _type2MessagesQueue.Enqueue(bucket);

        public bool AddMessageToPlcQueue(int plcId, byte[] message)
        {
            bool hasErrors = false;
            switch (plcId)
            {
                case 1:
                    _plc1Queue.Enqueue(message);
                    break;
                case 2:
                    _plc2Queue.Enqueue(message);
                    break;
                case 3:
                    _plc3Queue.Enqueue(message);
                    break;
                case 4:
                    _plc4Queue.Enqueue(message);
                    break;
                default:
                    hasErrors = true;
                    break;
            }

            return hasErrors;
        }

        private void HandlePlcComm(object? state)
        {
            // countTimeout = (countTimeout + 1) % 5;
            // 
            // Interlocked.Increment(ref countTimeout);
            // 
            // if (Interlocked.Read(ref countTimeout) > 0)
            //     Console.WriteLine("Timeout error");
            // 

#if DEBUG
            Console.WriteLine($"PlcQueuesController --- {DateTime.Now:h:mm:ss:ffff} Handling PLC Queues:");
#endif
            AddtoModbusQueue(_plc1Queue);
            AddtoModbusQueue(_plc2Queue);
            AddtoModbusQueue(_plc3Queue);
            AddtoModbusQueue(_plc4Queue);

            AddToFinalQueue(_type2MessagesQueue);

            // Console.WriteLine($"timeout timer: {Interlocked.Read(ref countTimeout)}");
            // Console.WriteLine($"final: {finalMessagesQueue.Count}");
            // Console.WriteLine($"type2: {type2MessagesQueue.Count}");
            // Console.WriteLine($"received: {receivedMessagesQueue.Count}");

            // if (!status)
            // {
            //     status = true;
            //     sendSemaphore.Release();
            // }
        }

        private void AddtoModbusQueue(ConcurrentQueue<byte[]> q)
        {
            if (q.TryDequeue(out var messagePlc))
            {
                Bucket bucketPlc = ParseMessage(messagePlc);
                if (bucketPlc.Type == 1)
                {
                    AddBucketToFinalQueueEvent.Invoke(bucketPlc);
                }
                else
                {
                    _type2MessagesQueue.Enqueue(bucketPlc);
                }
#if DEBUG
                Console.WriteLine($"\tPlcQueuesController dequeued message: {BitConverter.ToString(messagePlc)}");
#endif
                // q.Enqueue(messagePlc);
            }

        }

        private void AddToFinalQueue(ConcurrentQueue<Bucket> q)
        {
            bool status = _type2MessagesQueue.TryDequeue(out Bucket bucket);
            if (status)
            {
                AddBucketToFinalQueueEvent.Invoke(bucket);
            }
        }

        private Bucket ParseMessage(byte[] message)
        {
            Random random = new();

            Bucket bucket = new()
            {
                Tx = new byte[message.Length],
                Type = random.Next(1, 3)
            };

            bucket.Tx = message;

            return bucket;
        }

        private void InitializePlcQueuesWithDummyData()
        {
            _plc1Queue.Enqueue(new byte[] { 0xA1, 0xA1, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA });
            _plc1Queue.Enqueue(new byte[] { 0xA1, 0xA2, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA });
            _plc1Queue.Enqueue(new byte[] { 0xA1, 0xA3, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA });
            _plc1Queue.Enqueue(new byte[] { 0xA1, 0xA4, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA });
            _plc2Queue.Enqueue(new byte[] { 0xA2, 0xA1, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA });
            _plc2Queue.Enqueue(new byte[] { 0xA2, 0xA2, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA });
            _plc3Queue.Enqueue(new byte[] { 0xA3, 0xA1, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA });
            _plc3Queue.Enqueue(new byte[] { 0xA3, 0xA2, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA });
            _plc3Queue.Enqueue(new byte[] { 0xA3, 0xA3, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA });
            _plc4Queue.Enqueue(new byte[] { 0xA4, 0xA1, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA });
            _plc4Queue.Enqueue(new byte[] { 0xA4, 0xA2, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA });
            _plc4Queue.Enqueue(new byte[] { 0xA4, 0xA3, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA });
        }
    }
}
