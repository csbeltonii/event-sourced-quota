namespace Domain.Events;

public record RespondentCompletedSurvey(string QuotaName, string CellName, string RespondentId) 
    : QuotaEvent(QuotaName, CellName, RespondentId);