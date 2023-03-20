using System.Text;

namespace MessageQueuesController
{
    public class Bucket
    {
        public byte[] Tx { get; set; }
        public byte[] Rx { get; set; }
        public int Type { get; set; }

        public override string ToString()
        {
            StringBuilder TxSb = new();
            for (int i = 0; i < Tx.Length; i++)
            {
                TxSb.Append($"0x{Tx[i]} ");
            }
            StringBuilder RxSb = new();
            for (int i = 0; i < Rx.Length; i++)
            {
                RxSb.Append($"0x{Rx[i]} ");
            }
            return $"Tx: {TxSb} | Rx: {RxSb} | Type: {Type}";
        }

        public bool VerifyMessage()
        {
            Random random = new();
            return random.Next(2) == 0 ? true : false;
        }
    }
}