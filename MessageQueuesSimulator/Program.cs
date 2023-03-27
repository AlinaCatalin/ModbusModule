using MessageQueuesController;

namespace MessageQueuesSimulator {
    class Program {
        static void Main(string[] args) {
            MessageQueuesComm messageQueuesComm = new();

            messageQueuesComm.Start();
        }
    }
}