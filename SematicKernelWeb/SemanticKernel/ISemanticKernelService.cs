using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace SematicKernelWeb.SemanticKernel;

public interface ISemanticKernelService
{
    Task<string> ImportText(string text, Guid conversationId, Guid activeDirectoryId);
    Task<string> AskAsync(string query, Guid conversationId, Guid activeDirectoryId);
    Task RemoveFile(string memoryKey, Guid activeDirectoryId);

    Task<string> ImportWebPage(string url, Guid conversationId, Guid activeDirectoryId);
    Task<string> ImportFile(string filename, Stream fileData, Guid conversationId, Guid activeDirectoryId);
    Task<List<Citation>> SearchSummariesAsync(Guid conversationId, Guid activeDirectoryId, string memoryKey);
    IChatCompletionService GetChatService();
    public Kernel GetKernel();
    Task<string> Prompt(string prompt);
}