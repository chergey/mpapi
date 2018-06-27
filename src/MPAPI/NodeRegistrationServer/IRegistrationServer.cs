using System.Collections.Generic;
using System.Net;

namespace MPAPI.NodeRegistrationServer
{
    public interface IRegistrationServer
    {
        bool RegisterNode(IPEndPoint nodeEndPoint);
        void UnregisterNode(ushort nodeId);
        List<IPEndPoint> GetAllNodeEndPoints();

        bool RegisterHost(IPEndPoint hostEndPoint);
        void UnregisterHost(ushort hostId);
        List<IPEndPoint> GetAllHostEndPoints();
    }
}