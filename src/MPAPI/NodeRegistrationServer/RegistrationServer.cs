
using System;
using System.Collections.Generic;
using System.Net;
using MPAPI.NodeHosting;
using RL_ipv4;

namespace MPAPI.NodeRegistrationServer
{
    public sealed class RegistrationServer : IRegistrationServer, IDisposable
    {
        private class ProxyEndPointMap<T>
        {
            public ProxyEndPointMap(IPEndPoint endPoint, T nodeProxy)
            {
                EndPoint = endPoint;
                Proxy = nodeProxy;
            }

            public T Proxy { get; }

            public IPEndPoint EndPoint { get; }
        }

		private readonly Dictionary<ushort, ProxyEndPointMap<IRegisteredNode>> _nodeProxies = new Dictionary<ushort, ProxyEndPointMap<IRegisteredNode>>();
		private readonly Dictionary<ushort, ProxyEndPointMap<IRegisteredNodeHost>> _hostProxies = new Dictionary<ushort, ProxyEndPointMap<IRegisteredNodeHost>>();

        #region IRegistrationServer Members

        public bool RegisterNode(IPEndPoint nodeEndPoint)
        {
            var newNode = ProxyFactory.CreateProxy<IRegisteredNode>(nodeEndPoint);

            //we need to lock all this since the generation of node ids is dependent of what is in the
            //dictionary _nodeProxies and the node will only be registered in the dictionary as the last
            //action.
            lock (_nodeProxies)
            {
                //find an id that is available
                ushort newNodeId = 0;
                while (_nodeProxies.ContainsKey(newNodeId))
                {
                    if (newNodeId == ushort.MaxValue)
                    {
                        Log.Error("Unable to register more nodes. There are already {0} nodes registered", _nodeProxies.Count);
                        return false;
                    }
                    newNodeId++;
                }

                newNode.SetId(newNodeId);
                Log.Info("Registered node. Node Id : {0} , Address : {1} , Port : {2}", newNodeId, nodeEndPoint.Address.ToString(), nodeEndPoint.Port);

                //Notify all existing nodes of this new one
                foreach (var proxyEndPointMap in _nodeProxies.Values)
                    proxyEndPointMap.Proxy.NodeRegistered(nodeEndPoint);

                _nodeProxies.Add(newNodeId, new ProxyEndPointMap<IRegisteredNode>(nodeEndPoint, newNode));
            }
            return true;
        }

        public void UnregisterNode(ushort nodeId)
        {
            lock (_nodeProxies)
            {
                if (_nodeProxies.ContainsKey(nodeId))
                {
                    var node = _nodeProxies[nodeId].Proxy;
                    _nodeProxies.Remove(nodeId);
                    foreach (var proxyEndPointMap in _nodeProxies.Values)
                        proxyEndPointMap.Proxy.NodeUnregistered(nodeId);
                    Log.Info("Unregistered node, Node Id : {0}", nodeId);
                    ((IDisposable)node).Dispose(); //not strictly necessary due to Dispose, but nice nonetheless
                }
            }
        }

        public List<IPEndPoint> GetAllNodeEndPoints()
        {
            var endPoints = new List<IPEndPoint>();
            lock (_nodeProxies)
            {
                foreach (var proxyEndPointMap in _nodeProxies.Values)
                    endPoints.Add(proxyEndPointMap.EndPoint);
            }
            return endPoints;
        }

		public bool RegisterHost(IPEndPoint hostEndPoint)
		{
			var newHost = ProxyFactory.CreateProxy<IRegisteredNodeHost>(hostEndPoint);
			lock (_hostProxies)
			{
				//find an id that is available
				ushort newHostId = 0;
				while (_hostProxies.ContainsKey(newHostId))
				{
					if (newHostId == ushort.MaxValue)
					{
						Log.Error("Unable to register more hosts. There are already {0} hosts registered", _hostProxies.Count);
						return false;
					}
					newHostId++;
				}

				newHost.SetId(newHostId);
				Log.Info("Registered host. Host Id : {0} , Address : {1} , Port : {2}", newHostId, hostEndPoint.Address.ToString(), hostEndPoint.Port);

				//Notify all existing hosts of this new one
				foreach (var proxyEndPointMap in _hostProxies.Values)
					proxyEndPointMap.Proxy.HostRegistered(hostEndPoint);

				_hostProxies.Add(newHostId, new ProxyEndPointMap<IRegisteredNodeHost>(hostEndPoint, newHost));
			}
			return true;
		}

		public void UnregisterHost(ushort hostId)
		{
			lock (_hostProxies)
			{
				if (_hostProxies.ContainsKey(hostId))
				{
					var host = _hostProxies[hostId].Proxy;
					_hostProxies.Remove(hostId);
					foreach (var proxyEndPointMap in _hostProxies.Values)
						proxyEndPointMap.Proxy.HostUnregistered(hostId);
					Log.Info("Unregistered host, Host Id : {0}", hostId);
					((IDisposable)host).Dispose(); //not strictly necessary due to Dispose, but nice nonetheless
				}
			}
		}

		public List<IPEndPoint> GetAllHostEndPoints()
		{
			var endPoints = new List<IPEndPoint>();
			lock (_hostProxies)
			{
				foreach (var proxyEndPointMap in _hostProxies.Values)
					endPoints.Add(proxyEndPointMap.EndPoint);
			}
			return endPoints;
		}
		
		#endregion

        #region IDisposable Members

        public void Dispose()
        {
            lock (_nodeProxies)
            {
				foreach (var proxyEndPointMap in _nodeProxies.Values)
					((IDisposable)proxyEndPointMap.Proxy).Dispose();
				_nodeProxies.Clear();
				foreach (var proxyEndPointMap in _hostProxies.Values)
					((IDisposable)proxyEndPointMap.Proxy).Dispose();
				_hostProxies.Clear();
			}
        }

        #endregion
    }
}
