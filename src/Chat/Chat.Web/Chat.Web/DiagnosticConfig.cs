using System.Diagnostics.Metrics;
using System.Security.Cryptography.X509Certificates;
using Chat.Observability.Options;

namespace Chat.Web;

public static class DiagnosticConfig
{
    public static Meter Meter = new Meter("ChatApi");

    public static Counter<int> ControllerErrorCounter = Meter.CreateCounter<int>("chatapi.controller_errors", null, "Number of controller errors");
    public static Counter<int> ControllerCallCounter = Meter.CreateCounter<int>("chatapi.controller_calls", null, "Number of controller calls");

    public static void TrackControllerError(string controllerName, string actionName)
    {
        ControllerErrorCounter.Add(1, 
            new KeyValuePair<string, object?>("controller", controllerName),
            new KeyValuePair<string, object?>("action", actionName)
            );
    }

    public static void TrackControllerCall(string controllerName, string actionName)
    {
        ControllerCallCounter.Add(1, 
                   new KeyValuePair<string, object?>("controller", controllerName),
                   new KeyValuePair<string, object?>("action", actionName)
                 );
    }

}
