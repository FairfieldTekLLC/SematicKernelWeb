using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.AspNetCore.SignalR;
using SematicKernelWeb.Hubs;
using SematicKernelWeb.SemanticKernel;

namespace SematicKernelWeb.Services;

public class BackendWorker : BackgroundService, IBackendWorker
{
    private static readonly ConcurrentQueue<Message> _messageQueue = new();
    private readonly IHubContext<ChatHub> _hubContext;
    private readonly ILogger<BackendWorker> _logger;
    private readonly ISemanticKernelService _semanticKernelService;
    private readonly Thread worker;
    private bool _ShutDown;

    public BackendWorker(ISemanticKernelService semanticKernelService, IHubContext<ChatHub> hubContext,
        ILogger<BackendWorker> logger)
    {
        _logger = logger;
        _hubContext = hubContext;
        _semanticKernelService = semanticKernelService;
        worker = new Thread(() =>
        {
            while (!_ShutDown)
            {
                if (_messageQueue.Count > 0)
                    if (_messageQueue.TryDequeue(out var message))
                    {
                        if (message == null)
                            continue;

                        Debug.WriteLine(
                            "-------------------------------------------------------------------------------------------------------------------------");
                        Debug.WriteLine("Sending Message Conversation Id: " + message.ConversationId + " Message: " +
                                        message.Text);
                        Debug.WriteLine(
                            "-------------------------------------------------------------------------------------------------------------------------");

                        _hubContext.Clients.All
                            .SendAsync("ReceiveMessage", message.ConversationId.ToString(), message.Text)
                            .ConfigureAwait(false).GetAwaiter().GetResult();
                    }

                Thread.Sleep(2000); // Sleep for a second before checking the queue again
            }
        });

        worker.Start();
    }

    public ILogger<BackendWorker> getLogger()
    {
        return _logger;
    }

    public IHubContext<ChatHub> getHubContext()
    {
        return _hubContext;
    }

    public async Task SendMessage(Guid ConversationId, string message)
    {
        await _hubContext.Clients.All.SendAsync("ReceiveMessage", ConversationId.ToString(), message);

        _messageQueue.Enqueue(new Message
        {
            ConversationId = ConversationId,
            Text = message
        });
    }

    public ISemanticKernelService getISemanticKernelService()
    {
        return _semanticKernelService;
    }


    public void Dispose()
    {
        _ShutDown = true;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Debug.WriteLine("help");
        return Task.CompletedTask;
    }

    private class Message
    {
        public Guid ConversationId { get; set; }
        public string Text { get; set; }
    }
}