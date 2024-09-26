namespace Data.Models;

public class ChangeFeedProcessingSettings
{
    public string SourceContainer { get; set; }
    public string LeaseContainer { get; set; }
    public string DatabaseName { get; set; }
}