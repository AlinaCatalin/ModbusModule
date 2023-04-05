using MessageQueuesController;
using ModbusLib.Interface;
using System.Collections.Generic;

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