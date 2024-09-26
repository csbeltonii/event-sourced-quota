using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Domain.Quota;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TargetType
{
    [EnumMember(Value = "Infinite")]
    Strict,

    [EnumMember(Value = "Infinite")]
    Infinite
}