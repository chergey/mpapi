using System;
using System.Collections.Generic;
using System.Net;

namespace MPAPI
{
    /// <summary>
    /// The interface of a node as seen from other nodes
    /// </summary>
    public interface INode : INodeIdentity
    {
        /// <summary>
        /// Puts a message in the mail box of the receiver.
        /// </summary>
        void PutMessage(MessageLevel messageLevel, ushort receiverNodeId, ushort receiverWorkerId, ushort senderNodeId,
            ushort senderWorkerId, int messageType, object content);

        /// <summary>
        /// Gets a list of worker ids running on the node.
        /// </summary>
        /// <returns></returns>
        List<ushort> GetWorkerIds();

        /// <summary>
        /// Spawns a new worker.
        /// </summary>
        /// <param name="workerType">The type of worker to spawn.</param>
        /// <param name="workerId">If successfull, contains the id of the new worker.</param>
        /// <returns>True if the spawn was successfull, otherwise false.</returns>
        bool Spawn(Type workerType, out ushort workerId);

        /// <summary>
        /// Spawns a new worker.
        /// </summary>
        /// <param name="workerTypeName">The fully qualified name of the type of worker.</param>
        /// <param name="workerId">If successfull, contains the id of the new worker.</param>
        /// <returns>True if the spawn was successfull, otherwise false.</returns>
        bool Spawn(string workerTypeName, out ushort workerId);

        /// <summary>
        /// Gets the number of processors/processing cores available to the node.
        /// </summary>
        /// <returns></returns>
        int GetProcessorCount();

        /// <summary>
        /// Gets the number of workers currently running on the node.
        /// </summary>
        /// <returns></returns>
        int GetWorkerCount();

        void Monitor(WorkerAddress monitor, WorkerAddress monitoree);

        IPEndPoint GetIpEndPoint();
    }
}