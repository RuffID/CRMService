using Serilog.Core;
using Serilog.Events;

namespace CRMService.Core
{
    public class SimpleClassNameEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            if (logEvent.Properties.TryGetValue("SourceContext", out LogEventPropertyValue? sourceContextValue)
                && sourceContextValue is ScalarValue scalar && scalar.Value is string fullName)
            {
                string simpleName = fullName.Split('.').Last();
                logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("SourceContext", simpleName));
            }
        }
    }
}
