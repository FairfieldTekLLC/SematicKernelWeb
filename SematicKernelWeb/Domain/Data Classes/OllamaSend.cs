namespace SematicKernelWeb.Domain.Data_Classes;

public class OllamaSend
{
    public required string model { get; set; }
    public required List<Message> messages { get; set; }

    public bool stream { get; set; }

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Message
    {
        public required string role { get; set; }
        public required string content { get; set; }
    }
}