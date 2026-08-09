using DllNetwork.Settings;
using System.Text.Json.Serialization;

namespace NetworkTest.Json;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(NetworkSettings))]
internal partial class SourceGenerationContext : JsonSerializerContext;
