using SematicKernelWeb.Domain.Data_Classes;

namespace SematicKernelWeb.Domain;

public class Entry
{
    public Guid Id { get; set; }
    public int Sequence { get; set; }
    public ConversationType Type { get; set; }
    public Role Role { get; set; } = Role.user;
    public string Text { get; set; } = string.Empty;

    // public string? SearchString { get; set; }
    public int NumberOfResults { get; set; }

    public List<HTMLDocs> FetchedDocuments { get; set; } = new();

    public string? ResultText { get; set; }

    public byte[] FileData { get; set; } = null;

    public bool IsHidden { get; set; } = false;
}