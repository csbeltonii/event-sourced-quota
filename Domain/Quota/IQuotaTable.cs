namespace Domain.Quota;

public interface IQuotaTable
{
    public string QuotaTableName { get; set; }
    public string ProjectId { get; set; }
}