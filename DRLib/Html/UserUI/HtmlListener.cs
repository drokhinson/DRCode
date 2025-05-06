using System.Net;
using System.Text;
using DRLib.Html.Core;
using DRLib.Html.UserUI.Elements;
using DRLib.Html.UserUI.Events;
using Newtonsoft.Json;

namespace DRLib.Html.UserUI;

public static class HtmlListener
{
    public static void AddEvent(this Control htmlControl, TriggerCSharpEvent eventType, HtmlEventHandler action)
    {
        var namedEvent = eventType with { EventAction = action };
        htmlControl.Add(new CallCSharpJScript());
        htmlControl.AddAttribute(namedEvent);
    }
}

public class HtmlEventServer
{
    private HttpListener _listener;

    private readonly Dictionary<string, HtmlEventHandler> Events = new();
    
    public HtmlEventServer(string[] prefixes)
    {
        _listener = new HttpListener();
        foreach (string prefix in prefixes) {
            _listener.Prefixes.Add(prefix);
        }
    }

    public void LoadListeners(HtmlItem i)
    {
        var events = i.GetAttributesOfType<TriggerCSharpEvent>();
        foreach (var e in events) {
            var key = $"{i.Id}_{e.Event}";
            Events[key] = e.EventAction;
        }

        foreach (var c in i.GetAllItems())
            LoadListeners(c);
    }

    public void Start()
    {
        _listener.Start();
        Console.WriteLine("Listening...");
        while (true) {
            HttpListenerContext context = _listener.GetContext();
            ProcessRequest(context);
        }
    }

    private void ProcessRequest(HttpListenerContext context)
    {
        // Handle CORS preflight requests
        if (context.Request.HttpMethod == "OPTIONS") {
            context.Response.AddHeader("Access-Control-Allow-Origin", "*");
            context.Response.AddHeader("Access-Control-Allow-Methods", "POST, GET, OPTIONS");
            context.Response.AddHeader("Access-Control-Allow-Headers", "Content-Type");
            context.Response.StatusCode = 204;
            context.Response.Close();
            return;
        }

        // Add CORS headers to the response
        context.Response.AddHeader("Access-Control-Allow-Origin", "*");

        // Read request body
        using var reader = new StreamReader(context.Request.InputStream);
        string body = reader.ReadToEnd();
        var responseData = JsonConvert.DeserializeObject<HtmlEventArgs>(body);

        // Do something with the data
        var responseString = "OK";
        var key = $"{responseData.CallerId}_on{responseData.EventType}";
        if(Events.TryGetValue(key, out var handler))
            responseString = handler.Invoke(null, responseData);
        else {
            Console.WriteLine($"No handler found for {key}.");
            responseString = "Handler error";
        }

        // Send a response
        var response = context.Response;
        byte[] buffer = Encoding.UTF8.GetBytes(responseString);
        response.ContentLength64 = buffer.Length;
        response.OutputStream.Write(buffer, 0, buffer.Length);
        response.OutputStream.Close();
    }

    public void Stop()
    {
        _listener.Stop();
    }
}