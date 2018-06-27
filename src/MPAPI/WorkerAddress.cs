

using System;

namespace MPAPI
{
    [Serializable]
    public sealed class WorkerAddress
    {
        private readonly ushort _nodeId;
        private readonly ushort _workerId;

        /// <summary>
        /// Gets the node id part of the address.
        /// </summary>
        public ushort NodeId => _nodeId;

        /// <summary>
        /// Gets the worker id part of the address.
        /// </summary>
        public ushort WorkerId => _workerId;

        public static bool IsBroadcastAddress(ushort nodeId, ushort workerId) => nodeId == ushort.MaxValue && workerId == ushort.MaxValue;

        public static bool IsBroadcastAddress(WorkerAddress address) => address.NodeId == ushort.MaxValue && address.WorkerId == ushort.MaxValue;

        public WorkerAddress(ushort nodeId, ushort workerId)
        {
            _nodeId = nodeId;
            _workerId = workerId;
        }

        public static bool operator ==(WorkerAddress addr1, WorkerAddress addr2)
        {
            if ((object)addr1 == null && (object)addr2 == null)
                return true;
            if ((object)addr1 != null && (object)addr2 != null)
                return (addr1.NodeId == addr2.NodeId) && (addr1.WorkerId == addr2.WorkerId);
            return false;
        }

        public static bool operator !=(WorkerAddress addr1, WorkerAddress addr2)
        {

            if ((object)addr1 == null && (object)addr2 == null)
                return false;
            if ((object)addr1 != null && (object)addr2 != null)
                return (addr1.NodeId != addr2.NodeId) || (addr1.WorkerId != addr2.WorkerId);
            return true;
        }

        public override bool Equals(object obj)
        {
            if (obj is WorkerAddress)
                return this == (WorkerAddress)obj;
            return false;
        }

        public override int GetHashCode() => base.GetHashCode();

        public override string ToString()
        {
            return $"{WorkerId}@{NodeId}";
        }
    }
}
