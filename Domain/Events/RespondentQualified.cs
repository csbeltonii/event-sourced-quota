namespace Domain.Events;

public record RespondentQualified(string QuotaName, string CellName, string RespondentId)
    : QuotaEvent(QuotaName, CellName, RespondentId);