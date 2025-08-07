namespace SematicKernelWeb.Domain.Data_Classes;

public class HTMLDocs
{
    public Guid Id { get; set; }
    public string? MemoryKey { get; set; }
    public string? Uri { get; set; }
    public string? Body { get; set; }
    public string? Summary { get; set; }
}