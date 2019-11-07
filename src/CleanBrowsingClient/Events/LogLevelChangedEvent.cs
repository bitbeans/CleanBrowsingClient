using Prism.Events;
using Serilog.Events;

namespace CleanBrowsingClient.Events
{
    public class LogLevelChangedEvent : PubSubEvent<LogEventLevel> { }
}
