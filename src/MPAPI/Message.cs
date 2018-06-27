using System;

namespace MPAPI
{
    [Serializable]
    public enum MessageLevel
    {
        /// <summary>
        /// The message is a system message.
        /// </summary>
        System,

        /// <summary>
        /// The message is a user message - it is sent directly from a worker.
        /// </summary>
        User
    }

    public sealed class Message
    {
        public Message(MessageLevel messageLevel, WorkerAddress receiverAddress, WorkerAddress senderAddress,
            int messageType, object content)
        {
            MessageLevel = messageLevel;
            ReceiverAddress = receiverAddress;
            SenderAddress = senderAddress;
            MessageType = messageType;
            Content = content;
        }

        #region Properties

        /// <summary>
        /// Gets the leve of this message - either User or System.
        /// </summary>
        public MessageLevel MessageLevel { get; }

        /// <summary>
        /// Gets the address of the receiver of this message.
        /// </summary>
        public WorkerAddress ReceiverAddress { get; }

        /// <summary>
        /// Gets the addres of the sender of this message.
        /// </summary>
        public WorkerAddress SenderAddress { get; }

        /// <summary>
        /// Gets the message type of this message.
        /// </summary>
        public int MessageType { get; }

        /// <summary>
        /// Gets the contents of this message.
        /// </summary>
        public object Content { get; }

        #endregion
    }
}