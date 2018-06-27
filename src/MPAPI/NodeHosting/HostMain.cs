namespace MPAPI.NodeHosting
{
    public abstract class HostMain
    {
        internal IHost _host;
        protected IHost Host => _host;

        public abstract void Main();

        public virtual void OnRemoteHostRegistered(ushort hostId)
        {
        }

        public virtual void OnRemoteHostUnregistered(ushort hostId)
        {
        }
    }
}