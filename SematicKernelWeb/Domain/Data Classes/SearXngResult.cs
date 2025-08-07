namespace SematicKernelWeb.Domain.Data_Classes;

public class Result
{
    public string url { get; set; }
    public string title { get; set; }
    public string content { get; set; }
    public string thumbnail { get; set; }
    public string engine { get; set; }
    public string template { get; set; }
    public List<string> parsed_url { get; set; }
    public string img_src { get; set; }
    public string priority { get; set; }
    public List<string> engines { get; set; }
    public List<int> positions { get; set; }
    public double score { get; set; }
    public string category { get; set; }
    public object publishedDate { get; set; }
}

public class SearXngResult
{
    public string query { get; set; }
    public int number_of_results { get; set; }
    public List<Result> results { get; set; }
    public List<object> answers { get; set; }
    public List<object> corrections { get; set; }
    public List<object> infoboxes { get; set; }
    public List<string> suggestions { get; set; }
    public List<List<string>> unresponsive_engines { get; set; }
}