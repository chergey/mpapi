/*****************************************************************
 * MPAPI - Message Passing API
 * A framework for writing parallel and distributed applications
 * 
 * Author   : Frank Thomsen
 * Web      : http://sector0.dk
 * Contact  : mpapi@sector0.dk
 * License  : New BSD licence
 * 
 * Copyright (c) 2012, Frank Thomsen
 * 
 * Feel free to contact me with bugs and ideas.
 *****************************************************************/

using System.Collections.Generic;

namespace MPAPI.NodeHosting
{
	public interface INodeHost : INodeIdentity
	{
		string StopNode(string nodeName);
		string StartNode(string nodeName, string mainAssemblyName);
		bool IsRunning(string nodeName);
		string UploadFile(string nodeName, string fileName, byte[] fileBytes);
		string DeleteNode(string nodeName);
		List<string> GetInstalledNodes();
		List<string> GetRunningNodes();
	}
}
