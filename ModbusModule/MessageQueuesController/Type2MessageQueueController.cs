using System.Collections.Generic;

namespace MessageQueuesController
{
    public class Type2MessageQueueController
    {
        private Queue<Bucket> type2MessagesQueue = new();

        public delegate void DelegateEventAddBucket(Bucket bucket);

        public event DelegateEventAddBucket? AddBucketToFinalQueueEvent;

        public void HandleType2Queue()
        {
            // Send the last element to FinalQueue and verify if it can be processed further
            if (type2MessagesQueue.Count <= 0)
                return;

            Bucket bucket = type2MessagesQueue.Dequeue();

            AddBucketToFinalQueueEvent?.Invoke(bucket);
        }

        public void AddBucket(Bucket bucket) => type2MessagesQueue.Enqueue(bucket);

    }
}
