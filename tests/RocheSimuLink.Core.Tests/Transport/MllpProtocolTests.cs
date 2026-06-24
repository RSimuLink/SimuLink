using System.Text;
using RocheSimuLink.HL7.Transport;
using Xunit;

namespace RocheSimuLink.Core.Tests.Transport;

public class MllpProtocolTests
{
    [Fact]
    public void Encode_WrapsPayloadWithMarkers()
    {
        var frame = MllpProtocol.Encode("HELLO");

        Assert.Equal(MllpProtocol.StartBlock, frame[0]);
        Assert.Equal(MllpProtocol.EndBlock, frame[^2]);
        Assert.Equal(MllpProtocol.CarriageReturn, frame[^1]);
        Assert.Equal("HELLO", Encoding.UTF8.GetString(frame, 1, frame.Length - 3));
    }

    [Fact]
    public void EncodeDecode_RoundTrips()
    {
        const string msg = "MSH|^~\\&|A|B|C|D";

        var decoded = MllpProtocol.Decode(MllpProtocol.Encode(msg));

        Assert.Equal(msg, decoded);
    }

    [Fact]
    public void Decode_RejectsUnframedBuffer()
    {
        var notAFrame = Encoding.UTF8.GetBytes("HELLO");

        Assert.Throws<FormatException>(() => MllpProtocol.Decode(notAFrame));
    }

    [Fact]
    public void TryReadFrame_ReturnsFalseWhenIncomplete()
    {
        var partial = new byte[] { MllpProtocol.StartBlock, (byte)'H', (byte)'I' };

        var ok = MllpProtocol.TryReadFrame(partial, partial.Length, out _, out _);

        Assert.False(ok);
    }

    [Fact]
    public void TryReadFrame_ExtractsPayloadAndConsumesFrame()
    {
        var frame = MllpProtocol.Encode("ABC");

        var ok = MllpProtocol.TryReadFrame(frame, frame.Length, out var payload, out var consumed);

        Assert.True(ok);
        Assert.Equal("ABC", payload);
        Assert.Equal(frame.Length, consumed);
    }

    [Fact]
    public void TryReadFrame_LeavesTrailingBytesForNextFrame()
    {
        var first = MllpProtocol.Encode("ONE");
        var second = MllpProtocol.Encode("TWO");
        var combined = first.Concat(second).ToArray();

        var ok = MllpProtocol.TryReadFrame(combined, combined.Length, out var payload, out var consumed);

        Assert.True(ok);
        Assert.Equal("ONE", payload);
        Assert.Equal(first.Length, consumed);

        // Remaining bytes should be exactly the second frame.
        var remaining = combined.Skip(consumed).ToArray();
        Assert.Equal("TWO", MllpProtocol.Decode(remaining));
    }
}
