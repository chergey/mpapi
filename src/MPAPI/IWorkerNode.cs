using System.Collections.Generic;
using System.Net;

namespace MPAPI
{
    /// <summary>
    /// The interface of a node as seen from a worker
    /// </summary>
    public interface IWorkerNode : INodeIdentity
    {
        /// <summary>
        /// Get a list of ids for all other registered nodes.
        /// </summary>
        /// <returns></returns>
        List<ushort> GetRemoteNodeIds();

        /// <summary>
        /// Gets the number of processors/cores available to the node local.
        /// </summary>
        /// <returns></returns>
        int GetProcessorCount();

        /// <summary>
        /// Gets the number of workers currently running on the local node.
        /// </summary>
        /// <returns></returns>
        int GetWorkerCount();

        /// <summary>
        /// Gets the number of processors/cores available to the node - local or remote - with the specified id.
        /// </summary>
        /// <param name="nodeId">Id of the node.</param>
        /// <returns></returns>
        int GetProcessorCount(ushort nodeId);

        /// <summary>
        /// Gets the number of workers currently running on the specified node.
        /// </summary>
        /// <param name="nodeId">The node id</param>
        /// <returns></returns>
        int GetWorkerCount(ushort nodeId);

        IPEndPoint GetIpEndPoint(ushort nodeId);
    }
}
