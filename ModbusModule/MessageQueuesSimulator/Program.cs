using MessageQueuesController;

namespace MessageQueuesSimulator
{
    class Program 
    {
        static void Main(string[] args) 
        {
            MessageQueuesFacade facade = new MessageQueuesFacade();

            facade.Start();
        }
    }
}