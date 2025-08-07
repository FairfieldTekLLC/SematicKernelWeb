using System.ComponentModel;
using System.Net;
using Microsoft.SemanticKernel;
using SematicKernelWeb.Classes;
using SematicKernelWeb.Domain;
using SematicKernelWeb.Domain.Data_Classes;
using SematicKernelWeb.Services;

namespace SematicKernelWeb.SemanticKernel.Extensions;

public class ComfyPlugin
{
    private readonly IServiceScopeFactory _serviceScopeFactory;


    public ComfyPlugin(IBackendWorker backend, IServiceScopeFactory scopeFactory)
    {
        _backend = backend;
        _serviceScopeFactory = scopeFactory;
    }

    private IBackendWorker _backend { get; }


    private string GeneratePrompt(string text, string fileName)
    {
        return @"
        {
            ""3"": {
                ""class_type"": ""KSampler"",
                ""inputs"": {
                    ""cfg"": 8,
                    ""denoise"": 1,
                    ""latent_image"": [
                        ""5"",
                        0
                    ],
                    ""model"": [
                        ""4"",
                        0
                    ],
                    ""negative"": [
                        ""7"",
                        0
                    ],
                    ""positive"": [
                        ""6"",
                        0
                    ],
                    ""sampler_name"": ""euler"",
                    ""scheduler"": ""normal"",
                    ""seed"": 8566257,
                    ""steps"": 20
                }
            },
            ""4"": {
                ""class_type"": ""CheckpointLoaderSimple"",
                ""inputs"": {
                    ""ckpt_name"": ""v1-5-pruned-emaonly.safetensors""
                }
            },
            ""5"": {
                ""class_type"": ""EmptyLatentImage"",
                ""inputs"": {
                    ""batch_size"": 1,
                    ""height"": 512,
                    ""width"": 512
                }
            },
            ""6"": {
                ""class_type"": ""CLIPTextEncode"",
                ""inputs"": {
                    ""clip"": [
                        ""4"",
                        1
                    ],
                    ""text"": """ + text + @"""
                }
            },
            ""7"": {
                ""class_type"": ""CLIPTextEncode"",
                ""inputs"": {
                    ""clip"": [
                        ""4"",
                        1
                    ],
                    ""text"": ""bad hands""
                }
            },
            ""8"": {
                ""class_type"": ""VAEDecode"",
                ""inputs"": {
                    ""samples"": [
                        ""3"",
                        0
                    ],
                    ""vae"": [
                        ""4"",
                        2
                    ]
                }
            },
            ""9"": {
                ""class_type"": ""SaveImage"",
                ""inputs"": {
                    ""filename_prefix"": """ + fileName + @""",
                    ""images"": [
                        ""8"",
                        0
                    ]
                }
            }
        }";
    }


    private void QueuePrompt(string prompt)
    {
        try
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(Config.Instance.ComfyUrl);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write("{\"prompt\": " + prompt + "}");
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                Console.WriteLine(result);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }


    [KernelFunction("comfy-plugin")]
    [Description("draw me a picture of")]
    public async Task<string> LoadTheUrl(
        [Description("The conversation Id")] Guid conversationId,
        [Description("Owner Id")] Guid ownerId,
        [Description("of")] string imageDescription)
    {
        _backend.getLogger()
            .LogInformation(
                "Comfy Plugin called with image description: \"{ImageDescription}\" Conversation Id : {ConversationId} Owner Id: {OwnerId}",
                imageDescription, conversationId, ownerId);
        Guid g = Guid.NewGuid();

        string prompt = GeneratePrompt(imageDescription, g.ToString());
        _backend.getLogger().LogInformation("Generated prompt: {Prompt}", prompt);
        QueuePrompt(prompt);

        Conversation conversation;
        Guid conversationIdGuid = conversationId;

        if (conversationIdGuid == Guid.Empty) return "";
        conversation = new Conversation();
        conversation.Load(conversationIdGuid);
        _backend.getLogger().LogInformation("Conversation loaded with Id: {ConversationId}", conversationId);
        Entry entry = new Entry
        {
            Type = ConversationType.Comfy,
            Text = imageDescription,
            NumberOfResults = Conversation.MaxNumberOfResults,
            Role = Role.user,
            Sequence = conversation.PromptsOrSearches.Count + 1,
            FetchedDocuments = new List<HTMLDocs>()
        };

        string fileName = Config.Instance.ComfyOutPutFolder + g + "_00001_.png";


        int counter = 0;
        while (!File.Exists(fileName))
        {
            Thread.Sleep(1000);
            counter++;
            if (counter > 60) // wait for 60 seconds max
            {
                _backend.getLogger().LogWarning("Comfy generation timed out for conversation Id: {ConversationId}",
                    conversationId);
                await _backend.SendMessage(conversationId, "(Plugin) Comfy generation timed out.");
                return "timeout";
            }
        }

        counter = 0;
        if (File.Exists(fileName))
            while (true)
            {
                counter++;

                try
                {
                    var data = File.ReadAllBytes(fileName);
                    entry.FileData = data;
                    File.Delete(fileName);
                    break;
                }
                catch (Exception e)
                {
                    Thread.Sleep(1000);

                }
                if (counter > 60) // wait for 60 seconds max
                {
                    _backend.getLogger().LogWarning("Comfy generation timed out for conversation Id: {ConversationId}",
                        conversationId);
                    await _backend.SendMessage(conversationId, "(Plugin) Comfy generation timed out.");
                    return "timeout";
                }
            }

        conversation.PromptsOrSearches.Add(entry);
        conversation.Save();
        await _backend.SendMessage(conversationId, "(Plugin) plugin finished.");
        _backend.getLogger().LogInformation("Comfy generation completed for conversation Id: {ConversationId}",
            conversationId);

        await _backend.SendMessage(conversationId, "(Plugin) Performing Comfy generation of: " + imageDescription);
        return "ok";
    }
}