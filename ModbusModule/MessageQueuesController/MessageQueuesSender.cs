using ModbusLib;
using ModbusLib.Interface;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;

namespace MessageQueuesController
{
    internal class MessageQueuesSender 
    {
        private KoberSerial _koberSerial;
        
        private ConcurrentQueue<Bucket> _finalMessagesQueue = new();

        private Bucket _currentBucket;

        private Thread _sendMessageThread;

        private Semaphore _sendMessageSemaphore = new(0, 1);
        private int _sendSemaphoreTimeout;

        public MessageQueuesSender(List<IObserver> observers, int sendSempahoreTimeoutMiliseconds) 
        {
            _sendSemaphoreTimeout = sendSempahoreTimeoutMiliseconds;

            SerialPort serialPort = new("COM7") 
            { 
                BaudRate = 115200,
                DataBits = 8,
                Parity = Parity.Even,
                StopBits = StopBits.One,
                Handshake = Handshake.None
            };

            _koberSerial = new KoberSerial(serialPort);

            foreach (var observer in observers)
            {
                _koberSerial.AddObserver(observer);
            }

            _sendMessageThread = new Thread(SendMessage);
        }

        public void Start() => _sendMessageThread.Start();

        public void ReleaseSendSemaphore() => _sendMessageSemaphore.Release();

        public void AddBucket(Bucket bucket) => _finalMessagesQueue.Enqueue(bucket);

        public Bucket GetCurrentBucket() => _currentBucket;

        private void SendMessage()
        {
            while (true)
            {
                bool status = _finalMessagesQueue.TryDequeue(out Bucket bucket);
                if (!status)
                    continue;

                _currentBucket = bucket;

                _koberSerial.Write(_currentBucket.Tx);
#if DEBUG
                Console.WriteLine($"MessageQueuesSender --- Bucket sent to serial: {_currentBucket}");
#endif
                // if the thread succesfuly sends a message, then the thread must wait for the respone
                bool hasResumed = _sendMessageSemaphore.WaitOne(_sendSemaphoreTimeout);
                if (hasResumed)
                {
                    // define a mechanism to automatically resume the queues
                }
                else
                {
                    // message was sent but no response was received in the specified time
                    // freeze the current status of the queues

                }
            }
        }
    }
}
