using iTextSharp.text.html.simpleparser;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SematicKernelWeb.ClaimsIdentities;
using SematicKernelWeb.Controllers.ViewModels;
using SematicKernelWeb.Domain.Data_Classes;
using SematicKernelWeb.Hubs;
using SematicKernelWeb.Models;
using SematicKernelWeb.SemanticKernel;
using SematicKernelWeb.SemanticKernel.Models;
using SematicKernelWeb.Services;
using System.Diagnostics;
//using SematicKernelWeb.ClaimsIdentities;
using Conversation = SematicKernelWeb.Domain.Conversation;
using ConversationType = SematicKernelWeb.Domain.ConversationType;
using Entry = SematicKernelWeb.Domain.Entry;
using Role = SematicKernelWeb.Domain.Role;

namespace SematicKernelWeb.Controllers;

public class HomeController(
    ILogger<HomeController> logger,
    ISemanticKernelService semanticKernelService,
    IHubContext<ChatHub> hubContext,
    IBackendWorker backend)
    : Controller
{
    private readonly ILogger<HomeController> _logger = logger;

    private IHubContext<ChatHub> _hubContext = hubContext;
    private IBackendWorker _backend { get; } = backend;

    private ISemanticKernelService SemanticKernelService { get; } = semanticKernelService;

    public async Task<IActionResult> ProcessPrompt(string prompt, bool isWebSearch, bool isPrompt,
        Guid conversationId, bool isUrl, bool importText, bool ask)
    {
        Conversation conversation;
        Guid conversationIdGuid = conversationId;

        if (conversationIdGuid == Guid.Empty)
        {
#pragma warning disable CS8629 // Nullable value type may be null.
            conversation = new Conversation
            {
                Title = "Test Conversation",
                CreatedAt = DateTime.Now,
                Description = "This is a test conversation",
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                OwnerId = (User.GetUserIdentity()?.Id).Value
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            };
#pragma warning restore CS8629 // Nullable value type may be null.
            conversation.Save();
        }
        else
        {
            conversation = new Conversation();
            conversation.Load(conversationIdGuid);
        }

        if (importText)
        {
            Entry userPrompt = new Entry
            {
                Type = ConversationType.ImportText,
                Text = prompt,
                Role = Role.user,
                Sequence = conversation.PromptsOrSearches.Count + 1,
                ResultText = ""
            };
            conversation.PromptsOrSearches.Add(userPrompt);
        }
        else if (isPrompt)
        {
            Entry userPrompt = new Entry
            {
                Type = ConversationType.Prompt,
                Text = prompt,
                Role = Role.user,
                Sequence = conversation.PromptsOrSearches.Count + 1,
                ResultText = ""
            };
            conversation.PromptsOrSearches.Add(userPrompt);
        }
        else if (ask)
        {
            Entry userPrompt = new Entry
            {
                Type = ConversationType.Ask,
                Text = prompt,
                Role = Role.user,
                Sequence = conversation.PromptsOrSearches.Count + 1,
                ResultText = ""
            };
            conversation.PromptsOrSearches.Add(userPrompt);
        }
        else if (isWebSearch)
        {
            Entry entry = new Entry
            {
                Type = ConversationType.WebSearch,
                Text = prompt,
                NumberOfResults = Conversation.MaxNumberOfResults,
                Role = Role.user,
                Sequence = conversation.PromptsOrSearches.Count + 1,
                FetchedDocuments = new List<HTMLDocs>()
            };
            conversation.PromptsOrSearches.Add(entry);
        }
        else if (isUrl)
        {
            Entry entry = new Entry
            {
                Type = ConversationType.UrlFetch,
                Text = prompt,
                NumberOfResults = Conversation.MaxNumberOfResults,
                Role = Role.user,
                Sequence = conversation.PromptsOrSearches.Count + 1,
                FetchedDocuments = new List<HTMLDocs>()
            };
            conversation.PromptsOrSearches.Add(entry);
        }

        conversation.Save();

        await conversation.RunConversation(SemanticKernelService, _backend);

        conversation.Save();

        return Json(new
        {
            result = "Imported Websites:",
            success = true
        });
    }

    public IActionResult Index()
    {
        List<ConversationVM> conversations = new List<ConversationVM>();

        using (NewsReaderContext ctx = new NewsReaderContext())
        {
            List<Models.Conversation> convs = ctx.Conversations.Where(x => x.Fksecurityobjectowner == User.GetId())
                .ToList();
            foreach (Models.Conversation conversation in convs)
                conversations.Add(new ConversationVM
                {
                    ParentId = conversation.Fkparentid,
                    Title = conversation.Title,
                    Description = conversation.Description,
                    Id = conversation.Pkconversationid
                });
        }

        return View(conversations);
    }

    public IActionResult UserInteraction(Guid conversationId)
    {
        if (conversationId == Guid.Empty) return RedirectToAction("Index");


        ConversationVM vm = new ConversationVM();
        using (NewsReaderContext ctx = new NewsReaderContext())
        {
            Models.Conversation? conversation =
                ctx.Conversations.FirstOrDefault(x => x.Pkconversationid == conversationId);
            if (conversation != null)
            {
                vm.Id = conversation.Pkconversationid;
                vm.Title = conversation.Title;
                vm.Description = conversation.Description;
            }
            else
            {
                vm.Id = Guid.Empty;
                vm.Title = "New Conversation";
                vm.Description = "This is a new conversation. Please add prompts or searches.";
            }

            Conversation c = new Conversation();
            c.Load(conversationId);

            vm.ConversationText = GenOutput(c);
        }

        return View(vm);
    }


    public IActionResult CreateConversationView()
    {
        return PartialView();
    }

    public string GenOutput(Conversation conversation)
    {
        //string output = $"Conversation Title: {conversation.Title}\n";
        //output += $"Created At: {conversation.CreatedAt}\n";
        //output += $"Description: {conversation.Description}\n\n";
        bool first = true;
        string output = "<div class=\"accordion accordion-flush\" id=\"accordionFlushExample\">";
        foreach (Entry item in conversation.PromptsOrSearches.OrderByDescending(x => x.Sequence))
        {
            if (item.IsHidden)
                continue;
            switch (item.Type)
            {
                case ConversationType.Ask:
                {
                    output += makeAccordian("Ask: " + item.Text, item.ResultText, item.Sequence, first);
                    break;
                }
                case ConversationType.Prompt:
                {
                    output += makeAccordian("Prompt", item.Text, item.Sequence, first);
                    break;
                }
                case ConversationType.WebSearch:
                {
                    string ss = string.Empty;

                    ss = item.ResultText + "\r\n";

                    foreach (HTMLDocs document in item.FetchedDocuments)
                        ss += $"Document Found: <a href='{document.Uri}' target='newwindow'>{document.Uri}</a>\n";

                    //ss += item.ResultText;

                    output += makeAccordian("Web Search: " + item.Text, ss, item.Sequence, first);
                    break;
                }
                case ConversationType.OllamaResult:
                {
                    output += makeAccordian("Ollama Result", item.ResultText, item.Sequence, first);
                    break;
                }
                case ConversationType.UrlFetch:
                {
                    string ss = string.Empty;
                    foreach (HTMLDocs document in item.FetchedDocuments)
                        ss += $"Document Found: <a href='{document.Uri}'  target='newwindow'>{document.Uri}</a>\n";

                    output += makeAccordian("URL Fetch: " + item.Text, ss, item.Sequence, first);


                    break;
                }
                case ConversationType.ImportText:
                {
                    output += makeAccordian("Import Text", item.Text, item.Sequence, first);

                    break;
                }
                case ConversationType.UploadFile:
                {
                    string ss = string.Empty;
                    foreach (HTMLDocs document in item.FetchedDocuments)
                        ss += "Document Found: <a href ='https://bpc.bfsaul.net/Document/Download?fileId=" +
                              document.Body + "'>" + document.Uri + "</a>\r\n";

                    output += makeAccordian("Uploaded File " + item.Text, ss, item.Sequence, first);


                    break;
                }
                case ConversationType.Comfy:
                {
                    if (item.FileData != null)
                    {
                        string base64String = Convert.ToBase64String(item.FileData);

                        output += makeAccordian("Comfy Result",
                            @"<img src=""data:image/png;base64," + base64String + @""" width=""400"" height=""400"">",
                            item.Sequence, first);
                    }

                    break;
                }
                case ConversationType.ImageToText:
                {
                    string base64String = Convert.ToBase64String(item.FileData);
                    string imageHtml = @"<div><img src=""data:image/png;base64," + base64String +
                                       @""" width=""400"" height=""400""></div><div>" + item.ResultText + "</div>";
                    output += makeAccordian("Image to Text Result", imageHtml, item.Sequence, first);

                        break;
                }
            }

            first = false;
        }

        output += "</div>";
        return output;
    }


    public string makeAccordian(string title, string body, int instance, bool show = false)
    {
        string output = @$"
    <div class=""accordion-item"">
    <h2 class=""accordion-header"">
      <button class=""accordion-button"" type=""button"" data-bs-toggle=""collapse"" data-bs-target=""#collapse{instance}"" aria-expanded=""true"" aria-controls=""collapse{instance}"">
        {title}
      </button>
    </h2>
    <div id=""collapse{instance}"" class=""accordion-collapse collapse {(show ? "show" : "")}"" data-bs-parent=""#accordionExample"">
      <div class=""accordion-body"" style='text-align: left;'>
        {body.Replace("\n", "</br>")}
      </div>
    </div></div>
  ";

        return output;
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    public IActionResult CreateConversation(string title, string description)
    {
        using (NewsReaderContext ctx = new NewsReaderContext())
        {
            Models.Conversation conversation = new Models.Conversation
            {
                Pkconversationid = Guid.NewGuid(),
                Title = title,
                Description = description,
                Fksecurityobjectowner = User.GetId(),
                Createdat = DateTime.Now
            };
            ctx.Conversations.Add(conversation);
            ctx.SaveChanges();
        }

        return Json(new
        {
            result = "Created Conversation",
            success = true
        });
    }

    public IActionResult DeleteConversation(Guid conversationId)
    {
        if (conversationId == Guid.Empty)
            return Json(new
            {
                result = "Invalid Conversation ID",
                success = false
            });
        using (NewsReaderContext ctx = new NewsReaderContext())
        {
            Models.Conversation? conversation = ctx.Conversations.Include(x => x.Entries)
                .ThenInclude(x => x.Fetcheddocs)
                .FirstOrDefault(x => x.Pkconversationid == conversationId);
            if (conversation != null)
            {
                foreach (Models.Entry entry in conversation.Entries)
                    if (entry.Fetcheddocs != null && entry.Fetcheddocs.Count > 0)
                        foreach (Fetcheddoc? doc in entry.Fetcheddocs)
                            if (!string.IsNullOrEmpty(doc.Memorykey))
                                SemanticKernelService.RemoveFile(doc.Memorykey, User.GetId()).Wait();

                ctx.Conversations.Remove(conversation);
                ctx.SaveChanges();
                return Json(new
                {
                    result = "Deleted Conversation",
                    success = true
                });
            }
        }

        return Json(new
        {
            result = "Conversation not found",
            success = false
        });
    }

    [HttpPost]
    public async Task<IActionResult> UploadFile(List<IFormFile> postedFiles, Guid conversationId)
    {
        Conversation conversation;
        Guid conversationIdGuid = conversationId;

        if (conversationIdGuid == Guid.Empty)
        {
#pragma warning disable CS8629 // Nullable value type may be null.
            conversation = new Conversation
            {
                Title = "Test Conversation",
                CreatedAt = DateTime.Now,
                Description = "This is a test conversation",
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                OwnerId = (User.GetUserIdentity()?.Id).Value
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            };
#pragma warning restore CS8629 // Nullable value type may be null.
            conversation.Save();
        }
        else
        {
            conversation = new Conversation();
            conversation.Load(conversationIdGuid);
        }

        foreach (IFormFile postedFile in postedFiles)
        {
            using Stream fs = postedFile.OpenReadStream();
            byte[] byteArray = new byte[fs.Length];
            fs.Read(byteArray, 0, (int)fs.Length);
            Entry userPrompt = new Entry
            {
                Type = ConversationType.UploadFile,
                Text = postedFile.FileName,
                FileData = byteArray,
                Role = Role.user,
                Sequence = conversation.PromptsOrSearches.Count + 1,
                ResultText = ""
            };
            conversation.PromptsOrSearches.Add(userPrompt);
        }

        await conversation.RunConversation(SemanticKernelService, _backend);
        conversation.Save();

        ConversationVM vm = new ConversationVM();
        vm.Id = conversationId;
        vm.Description = conversation.Description;
        vm.Title = conversation.Title;
        vm.ParentId = conversation.ParentId;

        vm.ConversationText = GenOutput(conversation);

        return View("UserInteraction", vm);
    }

    public async Task<IActionResult> ImageToText(List<IFormFile> postedFiles, Guid conversationId,string txtpromptimgtotext)
    {
        Conversation conversation;
        Guid conversationIdGuid = conversationId;

        if (conversationIdGuid == Guid.Empty)
        {
#pragma warning disable CS8629 // Nullable value type may be null.
            conversation = new Conversation
            {
                Title = "Test Conversation",
                CreatedAt = DateTime.Now,
                Description = "This is a test conversation",
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                OwnerId = (User.GetUserIdentity()?.Id).Value
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            };
#pragma warning restore CS8629 // Nullable value type may be null.
            conversation.Save();
        }
        else
        {
            conversation = new Conversation();
            conversation.Load(conversationIdGuid);
        }

        foreach (IFormFile postedFile in postedFiles)
        {
            using Stream fs = postedFile.OpenReadStream();
            byte[] byteArray = new byte[fs.Length];
            fs.Read(byteArray, 0, (int)fs.Length);
            Entry userPrompt = new Entry
            {
                
                Type = ConversationType.ImageToText,
                Text = txtpromptimgtotext,
                FileData = byteArray,
                Role = Role.user,
                Sequence = conversation.PromptsOrSearches.Count + 1,
                ResultText = ""
            };
            conversation.PromptsOrSearches.Add(userPrompt);
        }

        await conversation.RunConversation(SemanticKernelService, _backend);
        conversation.Save();

        ConversationVM vm = new ConversationVM();
        vm.Id = conversationId;
        vm.Description = conversation.Description;
        vm.Title = conversation.Title;
        vm.ParentId = conversation.ParentId;

        vm.ConversationText = GenOutput(conversation);

        return View("UserInteraction", vm);
    }

    public async Task<IActionResult> Status()
    {
        return PartialView();
    }
}