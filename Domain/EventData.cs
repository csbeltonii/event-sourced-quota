using static System.Text.Json.JsonNamingPolicy;
using System.Text.Json.Serialization;
using Domain.Interfaces;

namespace Domain;

public class EventData<TEvent> : Entity where TEvent : IDomainEvent
{
    public TEvent Value { get; set; }

    [JsonConstructor]
    private EventData() : base(string.Empty) { }

    public EventData(TEvent value, string userId) : base(userId)
    {
        Value = value;
        Id = Value.Id;
    }

    public override string DocumentType => DocumentTypes.EventData;
    public string Type => CamelCase.ConvertName(typeof(TEvent).Name);

    public static implicit operator TEvent(EventData<TEvent> eventData) => eventData.Value!;
}