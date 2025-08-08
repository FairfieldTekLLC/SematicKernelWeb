using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.ImageToText;
using NewsReader.Data_Classes;
using static SematicKernelWeb.SemanticKernel.SemanticKernelService;

namespace SematicKernelWeb.SemanticKernel;

public interface ISemanticKernelService
{
    public Task<string> ImportText(string text, Guid conversationId, Guid activeDirectoryId);
    public Task<string> AskAsync(string query, Guid conversationId, Guid activeDirectoryId);
    public Task RemoveFile(string memoryKey, Guid activeDirectoryId);

    public Task<string> ImportWebPage(string url, Guid conversationId, Guid activeDirectoryId);
    public Task<string> ImportFile(string filename, Stream fileData, Guid conversationId, Guid activeDirectoryId);
    public Task<List<Citation>> SearchSummariesAsync(Guid conversationId, Guid activeDirectoryId, string memoryKey);
    public IChatCompletionService GetChatService();
    public Kernel GetKernel();
    public Task<string> Prompt(string prompt);
    public Task<OllamaResult?> ProcessOllamaMsg(OllamaSend toSend, EndpointType endpointType);
}