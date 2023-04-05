using MessageQueuesController;
using PlcMessagesProducer;

namespace MessageQueuesSimulator
{
    class Program 
    {
        static void Main(string[] args) 
        {
            MessageQueuesFacade facade = new MessageQueuesFacade();

            PlcMessageProducer plcMessageProducer = new();
            plcMessageProducer.AddPlcMessageEvent += facade.AddMessageToPlcQueues;
            plcMessageProducer.Start();

            facade.Start();
        }

        

    }
}