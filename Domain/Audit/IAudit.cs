namespace Domain.Audit;

public interface IAudit
{
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; }
    public DateTime LastUpdated { get; set; }
    public string UpdatedBy { get; set; }
    public int SchemaVersion { get; set; }
}