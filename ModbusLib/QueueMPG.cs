using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusLib {
    class QueueMPG {

        public Queue<PailMPG> pails = new Queue<PailMPG>();
        public Queue<Step> msgSend = new Queue<Step>();
        public Queue<Step> msgReceive = new Queue<Step>();
        public Queue<Step> msgTemp = new Queue<Step>();



    }
}
