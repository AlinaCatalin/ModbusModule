using MessageQueuesController;

namespace MessageQueuesSimulator
{
    internal class Program
    {
        static void Main(string[] args)
        {
            MessageQueuesComm messageQueuesComm = new();

            messageQueuesComm.Start();
        }
    }
}