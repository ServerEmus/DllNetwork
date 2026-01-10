using MemoryPack;
using MemoryPack.Internal;

namespace DllNetwork.Formatters;

[Preserve]
public class INetworkPacketFormatter : MemoryPackFormatter<INetworkPacket>
{
    public static INetworkPacketFormatter Instance { get; } = new();
    public Dictionary<Type, ushort> TypeToTag = [];
    public ushort LastTag => (ushort)(TypeToTag.Count == 0 ? 0 : TypeToTag.Last().Value);
    public ushort LastAvailable => (ushort)(TypeToTag.Count == 0 ? 0 : TypeToTag.Last().Value + 1);

    public void AddToTag<Type>()
    {
        TypeToTag.Add(typeof(Type), LastAvailable);
    }

    public void AddToTag(Type type)
    {
        TypeToTag.Add(type, LastAvailable);
    }

    public override void Deserialize(ref MemoryPackReader reader, scoped ref INetworkPacket? value)
    {
        if (!reader.TryReadUnionHeader(out ushort tag))
            value = default;
        else
        {
            var type = TypeToTag.FirstOrDefault(x => x.Value == tag);
            if (type.Value != tag)
                MemoryPackSerializationException.ThrowInvalidTag(tag, typeof(INetworkPacket));
            else
            {
                var oValue = (object?)value;
                reader.GetFormatter(type.Key).Deserialize(ref reader, ref oValue);
                value = (INetworkPacket?)oValue;
            }
        }
    }

    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref INetworkPacket? value)
    {
        if (value == null)
        {
            writer.WriteNullUnionHeader();
            return;
        }

        Type type = value.GetType();
        if (!TypeToTag.TryGetValue(type, out ushort tag))
        {
            var AssignableFrom = TypeToTag.Keys.Where(x => x.IsAssignableFrom(type)).ToList();
            foreach (var assignType in AssignableFrom)
            {
                if (TypeToTag.TryGetValue(assignType, out tag))
                {
                    type = assignType;
                    goto WRITE;
                }
            }
            var AssignableTo = TypeToTag.Keys.Where(x => x.IsAssignableTo(type)).ToList();
            foreach (var assignType in AssignableTo)
            {
                if (TypeToTag.TryGetValue(assignType, out tag))
                {
                    type = assignType;
                    goto WRITE;
                }
            }
            MemoryPackSerializationException.ThrowNotFoundInUnionType(value.GetType(), typeof(INetworkPacket));
        }
    WRITE:
        writer.WriteUnionHeader(tag);
        var formatter = writer.GetFormatter(type);
        var oValue = (object?)value;
        formatter.Serialize(ref writer, ref oValue);
    }
}