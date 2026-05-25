using System.Text.Json.Serialization;

namespace DllNetwork.Json;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(BroadcastJson))]
[JsonSerializable(typeof(List<BroadcastJson>))]
internal partial class SourceGenerationContext : JsonSerializerContext;
