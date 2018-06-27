using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using MPAPI.NodeRegistrationServer;
using RL_ipv4;

namespace MPAPI.NodeHosting
{
    public class NodeHost : INodeHost, IHost, IRegisteredNodeHost, IDisposable
    {
        private const string AppDir = "apps";

        private ServiceHost _host;
        private IRegistrationServer _registrationServerProxy;
        private ushort _id;
        private readonly object _idLock = new object();
        private readonly Dictionary<ushort, INodeHost> _remoteHosts = new Dictionary<ushort, INodeHost>();
        private HostMain _hostMain;
        private readonly Dictionary<string, AppDomain> _runningNodes = new Dictionary<string, AppDomain>();

        public bool Open(string registrationServerNameOrAddress, int registrationServerPort, int listenerPort)
        {
            if (_host == null)
            {
                try
                {
                    //Open a connection to the registration server
                    var addressList = Dns.GetHostEntry(registrationServerNameOrAddress).AddressList;
                    IPAddress address = null;
                    foreach (var a in addressList)
                        if (a.AddressFamily == AddressFamily.InterNetwork)
                        {
                            address = a;
                            break;
                        }

                    var registrationServerEndPoint = new IPEndPoint(address, registrationServerPort);
                    _registrationServerProxy =
                        ProxyFactory.CreateProxy<IRegistrationServer>(registrationServerEndPoint);

                    //get node hosts that are already registered
                    _remoteHosts.Clear();
                    var remoteHostEndPoints = _registrationServerProxy.GetAllHostEndPoints();
                    foreach (var remoteHostEndPoint in remoteHostEndPoints)
                    {
                        INodeHost remoteHost = ProxyFactory.CreateProxy<INodeHost>(remoteHostEndPoint);
                        _remoteHosts.Add(remoteHost.GetId(), remoteHost);
                    }

                    //open the host that contains this node
                    _host = new ServiceHost(this, listenerPort);
                    _host.Open();

                    //Register the host with the registration server. This causes all other registered nodes to connect to this one
                    _registrationServerProxy.RegisterHost(_host.EndPoint);
                    return true;
                }
                catch (Exception ex)
                {
                    string msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                    Log.Error("Could not connect to registration server. Message reads : {0}", msg);
                }
            }

            return false;
        }

        public void Open<THostMain>(string registrationServerNameOrAddress, int registrationServerPort,
            int listenerPort) where THostMain : HostMain
        {
            Open(registrationServerNameOrAddress, registrationServerPort, listenerPort);
            _hostMain = Activator.CreateInstance<HostMain>();
            _hostMain._host = this;
            _hostMain.Main();
        }

        public void Open(Type hostType, string registrationServerNameOrAddress, int registrationServerPort,
            int listenerPort)
        {
            Open(registrationServerNameOrAddress, registrationServerPort, listenerPort);
            _hostMain = (HostMain) Activator.CreateInstance(hostType);
            _hostMain._host = this;
            _hostMain.Main();
        }

        public List<INodeHost> GetRemoteHosts()
        {
            var remoteHosts = new List<INodeHost>();
            foreach (var rh in _remoteHosts.Values)
                remoteHosts.Add(rh);
            return remoteHosts;
        }

        #region INodeHost

        public string StopNode(string nodeName)
        {
            lock (_runningNodes)
            {
                if (_runningNodes.ContainsKey(nodeName))
                {
                    string msg = null;
                    try
                    {
                        var appDomain = _runningNodes[nodeName];
                        AppDomain.Unload(appDomain);
                        _runningNodes.Remove(nodeName);
                    }
                    catch (ArgumentNullException)
                    {
                        msg = "ArgumentNullException caught";
                    }
                    catch (CannotUnloadAppDomainException ex)
                    {
                        msg = $"CannotUnloadAppDomainException caught. Message: {ex.Message}";
                    }
                    catch (Exception ex)
                    {
                        msg = $"Generic Exception caught. Message: {ex.Message}";
                    }

                    if (msg != null)
                    {
                        var message = $"Cannot stop node '{nodeName}'. Reason: {msg}";
                        Log.Error(message);
                        return message;
                    }

                    return null;
                }
                else
                {
                    var msg = $"Cannot stop node '{nodeName}' since it is not running";
                    Log.Error(msg);
                    return msg;
                }
            }
        }

        public string StartNode(string nodeName, string mainAssemblyName)
        {
            lock (_runningNodes)
            {
                if (_runningNodes.ContainsKey(nodeName))
                {
                    var msg = $"Node '{nodeName}' is already running";
                    Log.Error(msg);
                    return msg;
                }

                try
                {
                    var adSetup = new AppDomainSetup();
                    var appDomainBasePath = Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppDir),
                        nodeName);
                    adSetup.ApplicationBase = appDomainBasePath;
                    adSetup.ConfigurationFile = Path.Combine(appDomainBasePath, mainAssemblyName + ".config");
                    var appDomain = AppDomain.CreateDomain(nodeName, null, adSetup);
                    appDomain.SetData("hostEndPoint", this._host.EndPoint);
                    Thread t = new Thread(() =>
                        appDomain.ExecuteAssembly(Path.Combine(appDomainBasePath, mainAssemblyName),
                            new[] {"-hostId:" + this._id.ToString(), "-nodeName:" + nodeName}))
                    {
                        IsBackground = true
                    };
                    t.Start();
                    _runningNodes.Add(nodeName, appDomain);
                    return null;
                }
                catch (Exception ex)
                {
                    var msg =
                        $"Cannot start node '{nodeName}'. Exception of type {ex.GetType().FullName} caught. Message: {ex.Message}";
                    Log.Error(msg);
                    return msg;
                }
            }
        }

        public bool IsRunning(string nodeName)
        {
            lock (_runningNodes)
            {
                if (_runningNodes.ContainsKey(nodeName))
                    return true;
                return false;
            }
        }

        public string UploadFile(string nodeName, string fileName, byte[] fileBytes)
        {
            lock (_runningNodes)
            {
                if (_runningNodes.ContainsKey(nodeName))
                {
                    var msg = $"Node '{nodeName}' is already running";
                    Log.Error(msg);
                    return msg;
                }

                try
                {
                    var appBasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppDir);
                    if (!Directory.Exists(appBasePath))
                        Directory.CreateDirectory(appBasePath);
                    var appDomainBasePath = Path.Combine(appBasePath, nodeName);
                    if (!Directory.Exists(appDomainBasePath))
                        Directory.CreateDirectory(appDomainBasePath);
                    File.WriteAllBytes(Path.Combine(appDomainBasePath, fileName), fileBytes);
                    return null;
                }
                catch (Exception ex)
                {
                    var msg = string.Format(
                        "Cannot upload file {3} to node '{0}'. Exception of type {1} caught. Message: {2}", nodeName,
                        ex.GetType().FullName, ex.Message, fileName);
                    Log.Error(msg);
                    return msg;
                }
            }
        }

        public string DeleteNode(string nodeName)
        {
            lock (_runningNodes)
            {
                if (_runningNodes.ContainsKey(nodeName))
                {
                    var msg = $"Cannot delete running node '{nodeName}'";
                    Log.Error(msg);
                    return msg;
                }

                try
                {
                    var appDomainBasePath = Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppDir),
                        nodeName);
                    if (Directory.Exists(appDomainBasePath))
                        Directory.Delete(appDomainBasePath, true);
                    return null;
                }
                catch (Exception ex)
                {
                    var msg =
                        $"Cannot delete node '{nodeName}'. Exception of type {ex.GetType().FullName} caught. Message: {ex.Message}";
                    Log.Error(msg);
                    return msg;
                }
            }
        }

        public List<string> GetInstalledNodes()
        {
            var appBasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppDir);
            return new List<string>(Directory.GetDirectories(appBasePath));
        }

        public List<string> GetRunningNodes()
        {
            var runningNodeNames = new List<string>();
            lock (_runningNodes)
            {
                foreach (var name in _runningNodes.Keys)
                    runningNodeNames.Add(name);
            }

            return runningNodeNames;
        }

        #endregion

        #region IRegisteredNodeHost

        public void HostRegistered(IPEndPoint hostEndPoint)
        {
            lock (_remoteHosts)
            {
                var host = ProxyFactory.CreateProxy<INodeHost>(hostEndPoint);
                var hostId = host.GetId();
                _remoteHosts.Add(hostId, host);
                _hostMain?.OnRemoteHostRegistered(hostId);
            }
        }

        public void HostUnregistered(ushort hostId)
        {
            lock (_remoteHosts)
            {
                if (_remoteHosts.ContainsKey(hostId))
                {
                    var host = _remoteHosts[hostId];
                    _remoteHosts.Remove(hostId);
                    ((IDisposable) host).Dispose();
                }
            }

            _hostMain?.OnRemoteHostUnregistered(hostId);
        }

        public void SetId(ushort hostId)
        {
            lock (_idLock)
            {
                _id = hostId;
            }
        }

        #endregion

        #region INodeIdentity

        public ushort GetId() => _id;

        #endregion

        #region IDisposable

        public void Dispose()
        {
            //stop all running nodes
            lock (_runningNodes)
            {
                var runningNodeNames = new List<string>();
                foreach (var name in _runningNodes.Keys)
                    runningNodeNames.Add(name);
                foreach (var nodeName in runningNodeNames)
                    StopNode(nodeName);
            }

            //disconnect from the registration server
            if (_registrationServerProxy != null)
            {
                _registrationServerProxy.UnregisterHost(_id);
                ((IDisposable) _registrationServerProxy).Dispose();
            }

            //kill the host
            _host?.Dispose();
            _host = null;
        }

        #endregion
    }
}