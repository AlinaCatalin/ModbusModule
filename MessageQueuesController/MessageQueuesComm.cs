using ModbusLib.Interface;
using ModbusLib;

using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.IO.Ports;
using System;

namespace MessageQueuesController {
    public class MessageQueuesComm : IObserver{

        private Timer plcCommTimer;

        private KoberSerial kober;

        private bool status = false;
        private Bucket currentBucket;
        private int countTimeout = -1;

        private Queue<byte[]> plc1Queue = new();
        private Queue<byte[]> plc2Queue = new();
        private Queue<byte[]> plc3Queue = new();
        private Queue<byte[]> plc4Queue = new();

        private static SerialPort serialPort;

        private readonly Semaphore sendSemaphore;
        private readonly Semaphore recvSemaphore;

        private Queue<Bucket> proccesRecvMsg = new();
        private Queue<Bucket> type2MessagesQueue = new();
        private Queue<Bucket> finalMessagesQueue = new();
        public List<Bucket> ProcessedBuckets{ get; set;}

        private AutoResetEvent plcCommAutoResetEvent = new(false);

        public MessageQueuesComm() {

            serialPort = new("COM4") { 
                BaudRate = 115200,
                DataBits = 8,
                Parity = Parity.Even,
                StopBits = StopBits.One,
                Handshake = Handshake.None
            };

            kober = new (serialPort);
            kober.AddObserver(this);

            InitializePlcQueuesWithDummyData();
            sendSemaphore = new(0, 1);
            recvSemaphore = new(0, 1);

            plcCommTimer = new(HandlePlcComm, plcCommAutoResetEvent, 0, 100);

            CreateThreads();
        }

        private void CreateThreads() {
            //thread to send msg
            new Thread(() => {
                while (true) {
                    sendSemaphore.WaitOne();
                    currentBucket = finalMessagesQueue.Dequeue();
                    kober.Write(currentBucket.Tx);
                    Debug.WriteLine("COnsum mesaje");
                    countTimeout = -1;
                }
            }).Start();

            //thread to receive msg
            new Thread(() => {
                while (true) {
                    recvSemaphore.WaitOne();
                    var local = proccesRecvMsg.Dequeue();
                    // decode msg and remove or add to type2queue
                    Decode(local);
                    Debug.WriteLine("Consum raspuns");
                }
            }).Start();
        }
        public void Start() {
            plcCommAutoResetEvent.WaitOne();
        }

        public void Stop() {
            plcCommAutoResetEvent.Set();
        }

        private void AddtoModbusQueue(Queue<byte[]> q) {
            if (q.Count > 0) {
                byte[] messagePlc = q.Dequeue();

                Bucket bucketPlc = ParseMessage(messagePlc);
                if (bucketPlc.Type == 0) {
                    finalMessagesQueue.Enqueue(bucketPlc);
                } else {
                    type2MessagesQueue.Enqueue(bucketPlc);
                }

                Console.WriteLine($"\tProcessed message: {messagePlc}");
            }
        }

        private void AddToFinalQueue(Queue<Bucket> q) {
            foreach (var bucket in q) {
                finalMessagesQueue.Enqueue(bucket);
            }
        }
        private void HandlePlcComm(object? state) {
            countTimeout = (countTimeout + 1) % 5;

            if (countTimeout > 0)
                Console.WriteLine("Timeout error");
            
            Console.WriteLine($"{DateTime.Now:h:mm:ss:ffff} -> Handling PLC Queues:");

            AddtoModbusQueue(plc1Queue);
            AddtoModbusQueue(plc2Queue);
            AddtoModbusQueue(plc3Queue);
            AddtoModbusQueue(plc4Queue);

            AddToFinalQueue(type2MessagesQueue);

            if (!status) {
                status = true;
                sendSemaphore.Release();
            }

        }

        private Bucket ParseMessage(byte[] message) {
            Random random = new();

            Bucket bucket = new() {
                Tx = new byte[message.Length],
                Type = random.Next(1, 2)
            };

            return bucket;
        }

        private void InitializePlcQueuesWithDummyData() {
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

            ProcessedBuckets = new();
        }

        public void Notify() {
            byte[] data = kober.GetRecv();
            currentBucket.Rx = data;
            proccesRecvMsg.Enqueue(currentBucket);
            Debug.WriteLine(System.Text.Encoding.UTF8.GetString(data));
            recvSemaphore.Release();
            sendSemaphore.Release();
        }

        public void Decode(Bucket b) {
            if(b.Rx.Length > 0) {
                type2MessagesQueue.Enqueue(b);
            }
        }
    }
}
