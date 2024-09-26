namespace Domain.Audit;

public class SystemInformation(string userId) : IAudit
{
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = userId;
    public DateTime LastUpdated { get; set; } = DateTime.Now;
    public string UpdatedBy { get; set; } = userId;
    public int SchemaVersion { get; set; }

    public void UpdateSystemInformation(string updatedBy)
    {
        UpdatedBy = updatedBy;
        LastUpdated = DateTime.UtcNow;
    }
}