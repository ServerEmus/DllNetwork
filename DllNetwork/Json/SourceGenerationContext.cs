using System.Text.Json.Serialization;

namespace DllNetwork.Json;

/// <summary>
/// Represent a source generated context for the <see cref="DllNetwork.Json.BroadcastAccount"/>.
/// </summary>
[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(BroadcastAccount))]
[JsonSerializable(typeof(List<BroadcastAccount>))]
internal partial class SourceGenerationContext : JsonSerializerContext;
