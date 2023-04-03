﻿using System;
using System.Text;

namespace MessageQueuesController {
    public class Bucket {
        public byte[] Tx { get; set; }
        public byte[] Rx { get; set; }
        public int Type { get; set; }

        public bool VerifyMessage() {
            Random random = new();
            return random.Next(2) == 0 ? true : false;
        }

        public override string ToString()
        {
            return $"Bucket: Type:{Type} Tx: {BitConverter.ToString(Tx)}";
        }
    }
}
