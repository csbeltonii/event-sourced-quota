namespace Domain.Events;

public record RespondentDisqualified(string QuotaName, string CellName, string RespondentId)
    : QuotaEvent(QuotaName, CellName, RespondentId);