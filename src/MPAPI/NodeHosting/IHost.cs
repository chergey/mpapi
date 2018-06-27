using System.Collections.Generic;

namespace MPAPI.NodeHosting
{
	/// <summary>
	/// The node host as seen from the HostMain subclasses
	/// </summary>
	public interface IHost : INodeHost
	{
		List<INodeHost> GetRemoteHosts();
	}
}
