using System.Net;

namespace MPAPI
{
    /// <summary>
    /// The interface of a node as seen from the registration server
    /// </summary>
    public interface IRegisteredNode : INodeIdentity
    {
        /// <summary>
        /// Called when a new node has registered with the registration server.
        /// </summary>
        /// <param name="nodeEndPoint">The end point of the new node.</param>
        void NodeRegistered(IPEndPoint nodeEndPoint);

        /// <summary>
        /// Called when a registered node unregisters - typically when closing.
        /// </summary>
        /// <param name="nodeId">Id of the node.</param>
        void NodeUnregistered(ushort nodeId);

        /// <summary>
        /// Called by the registration server when a node has registered. The registration server is responsible
        /// for assigning id's to nodes.
        /// </summary>
        /// <param name="nodeId"></param>
        void SetId(ushort nodeId);
    }
}