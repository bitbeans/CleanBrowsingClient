using System.Collections.Generic;
using System.Net.NetworkInformation;

namespace CleanBrowsingClient.Models
{
    public class LocalNetworkInterface
    {
        private string _name;
        private NetworkInterfaceType _type;
        private OperationalStatus _operationalStatus;
        private string _description;
        private List<DnsServer> _dns;
        private bool _ipv6Support;
        private bool _ipv4Support;
        private bool _useDnsCrypt;
        private bool _isChangeable;

        public LocalNetworkInterface()
        {
            Dns = new List<DnsServer>();
            _isChangeable = true;
        }

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
            }
        }

        /// <summary>
        /// The status of the network card (up/down)
        /// </summary>
        public OperationalStatus OperationalStatus
        {
            get => _operationalStatus;
            set
            {
                _operationalStatus = value;
            }
        }

        public string Description
        {
            get => _description;
            set
            {
                _description = value;
            }
        }

        public NetworkInterfaceType Type
        {
            get => _type;
            set
            {
                _type = value;
            }
        }

        public List<DnsServer> Dns
        {
            get => _dns;
            set
            {
                _dns = value;
            }
        }

        public bool Ipv6Support
        {
            get => _ipv6Support;
            set
            {
                _ipv6Support = value;
            }
        }

        public bool Ipv4Support
        {
            get => _ipv4Support;
            set
            {
                _ipv4Support = value;
            }
        }

        public bool IsChangeable
        {
            get => _isChangeable;
            set
            {
                _isChangeable = value;
            }
        }

        public bool UseDnsCrypt
        {
            get => _useDnsCrypt;
            set
            {
                _useDnsCrypt = value;
            }
        }
    }
}
