using System;
using System.Threading;

namespace PlcMessagesProducer
{
    public class PlcMessageProducer
    {
        public delegate bool DelegateAddPlcMessage(byte[] message, int plcId);
        
        public event DelegateAddPlcMessage AddPlcMessageEvent;

        private Thread _plcMessagesProducerThread;

        public PlcMessageProducer()
        {
            _plcMessagesProducerThread = new Thread(ProducePlcMessage);
        }

        public void Start() => _plcMessagesProducerThread.Start();

        private void ProducePlcMessage()
        {
            Random rand = new();

            while (true)
            {
                byte[] message = new byte[8];
                rand.NextBytes(message);

                int plcId = rand.Next(1, 5);

                AddPlcMessageEvent.Invoke(message, plcId);

                Console.WriteLine($"\t\t ----- PlcMessageProducer produced for plc{plcId} message: {BitConverter.ToString(message)}");

                int waitTime = rand.Next(10, 200);
                
                Thread.Sleep(waitTime);
            }
        }
    }
}
