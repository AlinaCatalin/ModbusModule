using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ModbusLib {
    class QueueController {

        public static Thread threadAllMsg = new Thread(RunThreadAllMsg);
        public static Thread threadWaitMsg = new Thread(RunThreadWaitMsg);

        public static Queue<int> allMessage = new Queue<int>();
        public static Queue<int> waitingMessage = new Queue<int>();
        public static Queue<int> respMessage = new Queue<int>();

        public static bool flag = true;
        public static int msg = 0;
     
        public static int timeAllMsg = 50;
        public static int timeWaitMsg = 500;
        

        public static void InsertInAllMessage(int msg) {
            allMessage.Enqueue(msg);
        }
        public static void InsertInWaitingMessage(int msg) {
            waitingMessage.Enqueue(msg);
        }

        public static void InsertInRespMessage(int msg) {
            respMessage.Enqueue(msg);
        }

        public static int GetFromAllMessage() {
            if(allMessage.Count > 0)
                return allMessage.Dequeue();
            return -1;
        }
        public static int GetFromWaitingMessage() {
            if (waitingMessage.Count > 0)
                return waitingMessage.Dequeue();
            return -1;
        }

        public static int GetFromRespMessage() {
            if (respMessage.Count > 0)
                return respMessage.Dequeue();
            return -1;
        }

        public static void RunThreadAllMsg() {
            if (flag) {
                msg = GetFromAllMessage();
                //send msg
                Thread.Sleep(timeAllMsg);
            } else {
                msg = GetFromWaitingMessage();

            }
        }
        
        public static void RunThreadWaitMsg() {

        }

        public static void StartControlQueue() {
            threadAllMsg.Start();
            threadWaitMsg.Start();
        }
    }
}
