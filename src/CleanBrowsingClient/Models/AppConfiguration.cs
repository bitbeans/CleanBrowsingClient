using System.Collections.Generic;

namespace CleanBrowsingClient.Models
{
    public class Proxy
    {
        public string Name { get; set; }
        public string Stamp { get; set; }
    }

    public class AppConfiguration
    {
        public int LogLevel { get; set; }
        public List<Proxy> Proxies { get; set; }
    }
}
