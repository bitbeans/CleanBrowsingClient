using System.Net.NetworkInformation;

namespace CleanBrowsingClient.Models
{
    public class DnsServer
    {
        private NetworkInterfaceComponent _type;
        private string _address;

        public NetworkInterfaceComponent Type
        {
            get => _type;
            set
            {
                _type = value;
            }
        }

        public string Address
        {
            get => _address;
            set
            {
                _address = value;
            }
        }
    }
}
