using Microsoft.AspNetCore.SignalR;
using SematicKernelWeb.Hubs;
using SematicKernelWeb.SemanticKernel;

namespace SematicKernelWeb.Services;

public interface IBackendWorker
{
    IHubContext<ChatHub> getHubContext();
    ISemanticKernelService getISemanticKernelService();
    Task SendMessage(Guid ConversationId, string message);
    ILogger<BackendWorker> getLogger();
}