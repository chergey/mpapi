using System.Net;

namespace MPAPI.NodeHosting
{
    /// <summary>
    /// The Node host as seen from the registration server.
    /// </summary>
    public interface IRegisteredNodeHost
    {
        /// <summary>
        /// Called when a new host has registered with the registration server.
        /// </summary>
        void HostRegistered(IPEndPoint hostEndPoint);

        /// <summary>
        /// Called when a host unregisters.
        /// </summary>
        /// <param name="hostId"></param>
        void HostUnregistered(ushort hostId);

        /// <summary>
        /// Called by the registration server when a node host has registered. The
        /// registration server is responsible for assigning host ids.
        /// </summary>
        /// <param name="hostId"></param>
        void SetId(ushort hostId);
    }
}