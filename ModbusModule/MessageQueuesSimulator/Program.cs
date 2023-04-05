using MessageQueuesController;
using ModbusLib.Interface;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace MessageQueuesSimulator
{
    class Program 
    {
        static void Main(string[] args) 
        {
            Process currentProcess = Process.GetCurrentProcess();
            ProcessThreadCollection appThreads = currentProcess.Threads;

            int initialThreadsCount = appThreads.Count;
            System.Console.WriteLine($"initial threads: {initialThreadsCount}");
            

            PlcQueuesController plcQueuesController = new();

            MessageProcessor messageProcessor = new();

            List<IObserver> observers = new() { messageProcessor };

            MessageQueuesSender messageQueuesSender = new(observers);

            plcQueuesController.AddBucketToFinalQueueEvent += messageQueuesSender.AddBucket;

            messageProcessor.AddBucketToType2QueueEvent += plcQueuesController.AddBucketToType2MessagesQueue;
            messageProcessor.GetCurrentBucketInProcessingEvent += messageQueuesSender.GetCurrentBucket;
            messageProcessor.MessageForCurrentBucketReceivedEvent += messageQueuesSender.ReleaseSendSemaphore;

            currentProcess.Refresh();
            appThreads = currentProcess.Threads;
            int beforeStartThreadsCount = appThreads.Count;
            System.Console.WriteLine($"before start threads: {beforeStartThreadsCount}");

            plcQueuesController.Start();
            messageQueuesSender.Start();
            messageProcessor.Start();


            // currentProcess.Refresh();
            // appThreads = currentProcess.Threads;
            // int beforeTestThreadsCount = appThreads.Count;
            // System.Console.WriteLine($"before test threads: {beforeTestThreadsCount}");
            // 
            // Thread thread1 = new Thread(() => { Thread.Sleep(1000000); });
            // Thread thread2 = new Thread(() => { Thread.Sleep(1000000); });
            // Thread thread3 = new Thread(() => { Thread.Sleep(1000000); });
            // Thread thread4 = new Thread(() => { Thread.Sleep(1000000); });
            // Thread thread5 = new Thread(() => { Thread.Sleep(1000000); });
            // 
            // thread1.Start();
            // thread2.Start();
            // thread3.Start();


            currentProcess.Refresh();
            appThreads = currentProcess.Threads;
            int finalThreadsCount = appThreads.Count;
            System.Console.WriteLine($"final threads: {finalThreadsCount}");

            // for (int i = 0; i < finalThreadsCount - beforeStartThreadsCount; i++)
            // {
            //     currentProcess.Threads[i + beforeStartThreadsCount].ProcessorAffinity = (IntPtr)(1 << 4 << i);
            //     Console.WriteLine($"thread {i + beforeStartThreadsCount} set to run on core: {i}");
            // }


        }
    }
}