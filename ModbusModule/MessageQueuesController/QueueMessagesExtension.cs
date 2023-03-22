using System.Text;

namespace MessageQueuesController
{
    public static class QueueMessagesExtension
    {
        public static string ConvertToString(this byte[] bytes)
        {
            StringBuilder sb = new();
            for (int i = 0; i < bytes.Length; i++)
            {
                sb.Append(bytes[i]);
            }

            return sb.ToString();
        }
    }
}
