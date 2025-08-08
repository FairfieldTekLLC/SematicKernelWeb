using Microsoft.KernelMemory;
using Microsoft.KernelMemory.Context;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.ImageToText;
using Newtonsoft.Json;
using System.Diagnostics;
using NewsReader.Data_Classes;
using SematicKernelWeb.Classes;
using SematicKernelWeb.Domain.Data_Classes;

using System.Net.Http.Headers;

namespace SematicKernelWeb.SemanticKernel;

public class SemanticKernelService(Kernel kernel, IKernelMemory memory) : ISemanticKernelService
{
    public async Task<string> ImportText(string text, Guid conversationId, Guid activeDirectoryId)
    {
        TagCollection collection = new TagCollection();
        collection.Add("conversationId", conversationId.ToString());
        return await memory.ImportTextAsync(text, null, collection, activeDirectoryId.ToString());
    }


    public async Task<string> AskAsync(string query, Guid conversationId, Guid activeDirectoryId)
    {
        var answer = await memory.AskAsync(
            query,
            filter: MemoryFilters.ByTag("conversationId", conversationId.ToString()),
            index: activeDirectoryId.ToString());
        return answer.Result;
    }

    public async Task<string> Prompt(string prompt)
    {
        PromptExecutionSettings settings = new() { FunctionChoiceBehavior = FunctionChoiceBehavior.Required() };
        FunctionResult t = kernel.InvokePromptAsync(prompt, new KernelArguments(settings)).Result;
        Debug.WriteLine(t);
        return t.ToString();
    }


    public async Task RemoveFile(string memoryKey, Guid activeDirectoryId)
    {
        await memory.DeleteDocumentAsync(memoryKey, activeDirectoryId.ToString());
    }

    public async Task<string> ImportWebPage(string url, Guid conversationId, Guid activeDirectoryId)
    {
        try
        {
            RequestContext context = new RequestContext();
            context.SetArg("custom_summary_prompt_str",
                "Super extra summarize, use short sentences, no list, no new lines.  Content: {{$input}}. Summary: ");

            TagCollection collection = new TagCollection();
            collection.Add("conversationId", conversationId.ToString());
            return await memory.ImportWebPageAsync(url, null, collection, activeDirectoryId.ToString(),
                Constants.PipelineOnlySummary, context);
        }
        catch (Exception)
        {
            return "";
        }
    }

    public async Task<string> ImportFile(string filename, Stream fileData, Guid conversationId, Guid activeDirectoryId)
    {
        try
        {
            TagCollection collection = new TagCollection();
            collection.Add("conversationId", conversationId.ToString());
            return await memory.ImportDocumentAsync(fileData, filename, null, collection, activeDirectoryId.ToString());
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return string.Empty;
        }
    }

    public async Task<List<Citation>> SearchSummariesAsync(Guid conversationId, Guid activeDirectoryId,
        string memoryKey)
    {
        List<Citation> result = await memory.SearchSummariesAsync(activeDirectoryId.ToString(),
            MemoryFilters.ByTag("conversationId", conversationId.ToString()).ByDocument(memoryKey));

        return result;
    }

    public IChatCompletionService GetChatService()
    {
        return kernel.GetRequiredService<IChatCompletionService>();
    }


    public Kernel GetKernel()
    {
        return kernel;
    }

    public class OllamaSend
    {
        public string model { get; set; } = "";
        public bool stream { get; set; } = false;

        public string prompt { get; set; }
        public string suffix { get; set; } = "";
        public List<string> images { get; set; }=new List<string>();


    }

    public enum EndpointType
    {
        chat,
        generate
    }

    public async Task<OllamaResult?> ProcessOllamaMsg(OllamaSend toSend, EndpointType endpointType)
    {




        HttpClient client = new HttpClient();
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, Config.Instance.OllamaServerUrl +"/api/generate");
        string json = JsonConvert.SerializeObject(toSend);
        request.Content = new StringContent(json);
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
        HttpResponseMessage response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        string responseBody = await response.Content.ReadAsStringAsync();
        OllamaResult? ollamaResult = JsonConvert.DeserializeObject<OllamaResult>(responseBody);
        return ollamaResult;

    }
}