using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.SignalR;
using Microsoft.SemanticKernel;
using SematicKernelWeb.Classes;
using SematicKernelWeb.Domain;
using SematicKernelWeb.Domain.Data_Classes;
using SematicKernelWeb.Helpers;
using SematicKernelWeb.Hubs;
using SematicKernelWeb.Services;

namespace SematicKernelWeb.SemanticKernel.Extensions;

public class InternetSearchPlugin
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public InternetSearchPlugin(IBackendWorker backend, IServiceScopeFactory scopeFactory)
    {
        _backend = backend;
        _serviceScopeFactory = scopeFactory;
    }

    private IBackendWorker _backend { get; }

    [KernelFunction("search_internet")]
    [Description("Search the internet for a subject.")]
    public async Task<string> SearchTheInternet(
        [Description("The conversation Id")] Guid conversationId,
        [Description("Owner Id")] Guid ownerId,
        [Description("for")] string searchString)
    {
        try
        {
            _backend.getLogger()
                .LogInformation(
                    "Internet Search Plugin called with search string: \"{SearchString}\" Conversation Id : {ConversationId} Owner Id: {OwnerId}",
                    searchString, conversationId, ownerId);
            Conversation conversation;
            Guid conversationIdGuid = conversationId;

            if (conversationIdGuid == Guid.Empty) return "";
            conversation = new Conversation();
            conversation.Load(conversationIdGuid);

            Entry entry = new Entry
            {
                Type = ConversationType.WebSearch,
                Text = searchString,
                NumberOfResults = Conversation.MaxNumberOfResults,
                Role = Role.user,
                Sequence = conversation.PromptsOrSearches.Count + 1,
                FetchedDocuments = new List<HTMLDocs>()
            };


            string content =
                await ImportWebSearch(conversationId, ownerId, entry, _backend.getISemanticKernelService());

            entry.ResultText = content;


            conversation.PromptsOrSearches.Add(entry);
            conversation.Save();
            _backend.getLogger()
                .LogInformation("Internet Search Plugin completed for conversation Id: {ConversationId}",
                    conversationId);
            return content;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return "";
        }
    }


    private bool LoadSite(string url)
    {
        return Config.Instance.ignoreSites.All(site =>
            !url.Contains(site, StringComparison.InvariantCultureIgnoreCase));
    }


    public async Task<string> ImportWebSearch(Guid conversationId, Guid ownerId, Entry item,
        ISemanticKernelService kernal)
    {
        using IServiceScope scope = _serviceScopeFactory.CreateScope();

        IHubContext<ChatHub> hubContext = scope.ServiceProvider.GetService<IHubContext<ChatHub>>();
        StringBuilder resultText = new StringBuilder();
        if (item.FetchedDocuments.Count == 0)
        {
            await _backend.SendMessage(conversationId, "(Plugin) Performing Web Search for: " + item.Text);


            string searchString = item.Text ?? "";
            if (searchString.Contains(" today ", StringComparison.InvariantCultureIgnoreCase))
                searchString = searchString.Replace(" today ", DateTime.Now.ToString("D"));
            if (searchString.Contains(" today?", StringComparison.InvariantCultureIgnoreCase))
                searchString = searchString.Replace(" today?", DateTime.Now.ToString("D"));
            if (searchString.Contains(" today.", StringComparison.InvariantCultureIgnoreCase))
                searchString = searchString.Replace(" today.", DateTime.Now.ToString("D"));
            if (searchString.Contains(" today!", StringComparison.InvariantCultureIgnoreCase))
                searchString = searchString.Replace(" today!", DateTime.Now.ToString("D"));
            if (searchString.Contains(" today", StringComparison.InvariantCultureIgnoreCase))
                searchString = searchString.Replace(" today", DateTime.Now.ToString("D"));

            if (searchString.Contains(" today's", StringComparison.InvariantCultureIgnoreCase))
                searchString = searchString.Replace(" today's", DateTime.Now.ToString("D"));


            List<Result> SearchResults =
                new List<string> { searchString }.QuerySearchEngineForUrls(30);

            List<string> urls = new List<string>();


            int counter = 0;
            foreach (Result result in SearchResults)
            {
                if (LoadSite(result.url))
                {
                    urls.Add(result.url);
                    resultText.AppendLine("Title: " + result.title);
                    resultText.AppendLine("Url: <a href='" + result.url + "'>" + result.url +"</a>");
                    resultText.AppendLine("Content: " + result.content);
                    resultText.AppendLine("--------------------------------------------------");
                    counter++;
                }

                if (counter > item.NumberOfResults)
                    break;
            }

            ConcurrentBag<HTMLDocs> hEntries = new ConcurrentBag<HTMLDocs>();

            Parallel.ForEach(urls, new ParallelOptions { MaxDegreeOfParallelism = 8 }, url =>
            {
                try
                {
                    _backend.SendMessage(conversationId, "(Plugin) Started Processing url: " + url).ConfigureAwait(true)
                        .GetAwaiter().GetResult();

                    Debug.WriteLine("Processing url: " + url);
                    HTMLDocs doc = new HTMLDocs
                    {
                        Uri = url,
                        Body = "",
                        Summary = "",
                        MemoryKey = kernal.ImportWebPage(url, conversationId, ownerId).Result
                    };

                    Debug.WriteLine("Finished loading file! " + url);
                    _backend.SendMessage(conversationId, "(Plugin) Finished Processing url: " + url)
                        .ConfigureAwait(true).GetAwaiter().GetResult();

                    hEntries.Add(doc);
                }
                catch (Exception e)
                {
                    _backend.getLogger().LogError(e, "Error processing URL: {Url}", url);
                    try
                    {
                        string content = url.FetchUrlAsContent().Result;
                        if (string.IsNullOrEmpty(content) || content.Length < 500)
                        {
                            Console.WriteLine("Failed to fetch content for URL: " + url);
                            return;
                        }

                        HTMLDocs doc = new HTMLDocs
                        {
                            Uri = url,
                            Body = "",
                            MemoryKey = content,
                            Summary = ""
                        };
                        hEntries.Add(doc);
                    }
                    catch (Exception exception)
                    {
                        _backend.getLogger().LogError(exception, "Error fetching URL content: {Url}", url);
                    }
                }
            });

            item.FetchedDocuments = hEntries.Where(x => x.MemoryKey != string.Empty).ToList();
            _backend.SendMessage(conversationId, "(Plugin) Finished").ConfigureAwait(true).GetAwaiter().GetResult();
        }

        return resultText.ToString();
    }
}