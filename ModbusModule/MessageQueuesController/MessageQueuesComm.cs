using System.Collections.Generic;

namespace MessageQueuesController
{
    public class MessageQueuesComm
    {
        public List<Bucket> ProcessedBuckets { get; private set; } = new();

        private FinalQueueController finalQueueController;
        private Type2MessageQueueController type2MessageQueueController;
        private PlcQueuesController plcQueuesController;

        public MessageQueuesComm()
        {
            finalQueueController = new FinalQueueController();
            type2MessageQueueController = new Type2MessageQueueController();
            plcQueuesController = new PlcQueuesController();

            finalQueueController.AddBucketToType2QueueEvent += type2MessageQueueController.AddBucket;
            finalQueueController.AddBucketToProcessedBucketsList += FinishBucketProcess;
            finalQueueController.ProcessMessage += plcQueuesController.HandlePlcComm;
            finalQueueController.ProcessMessage += type2MessageQueueController.HandleType2Queue;

            type2MessageQueueController.AddBucketToFinalQueueEvent += finalQueueController.AddBucket;
            plcQueuesController.AddBucketToFinalQueueEvent += finalQueueController.AddBucket;
            plcQueuesController.AddBucketToType2QueueEvent += type2MessageQueueController.AddBucket;
        }

        private void FinishBucketProcess(Bucket bucket)
        {
            ProcessedBuckets.Add(bucket);
        }

        public void Start()
        {
            finalQueueController.Start();
        }


        
        
    }
}
