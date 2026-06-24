using System.Text;

namespace RocheSimuLink.HL7.Transport
{
    /// <summary>
    /// Minimal Lower Layer Protocol (MLLP) framing constants and helpers.
    /// MLLP frames an HL7 message as: &lt;VT&gt; message &lt;FS&gt;&lt;CR&gt;.
    /// </summary>
    public static class MllpProtocol
    {
        /// <summary>Start block marker (Vertical Tab, 0x0B).</summary>
        public const byte StartBlock = 0x0B;

        /// <summary>End block marker (File Separator, 0x1C).</summary>
        public const byte EndBlock = 0x1C;

        /// <summary>Carriage return (0x0D) that terminates an MLLP frame.</summary>
        public const byte CarriageReturn = 0x0D;

        /// <summary>
        /// Wraps an HL7 message in an MLLP frame.
        /// </summary>
        public static byte[] Encode(string hl7Message, Encoding? encoding = null)
        {
            ArgumentNullException.ThrowIfNull(hl7Message);
            encoding ??= Encoding.UTF8;

            var payload = encoding.GetBytes(hl7Message);
            var frame = new byte[payload.Length + 3];
            frame[0] = StartBlock;
            Buffer.BlockCopy(payload, 0, frame, 1, payload.Length);
            frame[^2] = EndBlock;
            frame[^1] = CarriageReturn;
            return frame;
        }

        /// <summary>
        /// Extracts the HL7 payload from a complete MLLP frame.
        /// </summary>
        /// <exception cref="FormatException">Thrown when the frame is not a valid MLLP frame.</exception>
        public static string Decode(byte[] frame, Encoding? encoding = null)
        {
            ArgumentNullException.ThrowIfNull(frame);
            encoding ??= Encoding.UTF8;

            if (frame.Length < 3 || frame[0] != StartBlock ||
                frame[^2] != EndBlock || frame[^1] != CarriageReturn)
            {
                throw new FormatException("Buffer is not a complete MLLP frame.");
            }

            return encoding.GetString(frame, 1, frame.Length - 3);
        }

        /// <summary>
        /// Attempts to extract one complete MLLP frame's payload from a receive buffer.
        /// On success, returns true and reports how many bytes were consumed so the
        /// caller can advance its buffer.
        /// </summary>
        /// <param name="buffer">Bytes received so far.</param>
        /// <param name="length">Number of valid bytes in <paramref name="buffer"/>.</param>
        /// <param name="payload">The decoded HL7 message when a full frame is present.</param>
        /// <param name="bytesConsumed">Bytes consumed by the frame (including markers).</param>
        /// <param name="encoding">Text encoding (defaults to UTF-8).</param>
        public static bool TryReadFrame(
            byte[] buffer,
            int length,
            out string payload,
            out int bytesConsumed,
            Encoding? encoding = null)
        {
            ArgumentNullException.ThrowIfNull(buffer);
            encoding ??= Encoding.UTF8;
            payload = string.Empty;
            bytesConsumed = 0;

            if (length <= 0)
            {
                return false;
            }

            var start = Array.IndexOf(buffer, StartBlock, 0, length);
            if (start < 0)
            {
                return false;
            }

            for (var i = start + 1; i < length - 1; i++)
            {
                if (buffer[i] == EndBlock && buffer[i + 1] == CarriageReturn)
                {
                    var payloadLength = i - (start + 1);
                    payload = encoding.GetString(buffer, start + 1, payloadLength);

                    // Consume everything up to and including the trailing CR,
                    // discarding any stray bytes that preceded the start block.
                    bytesConsumed = i + 2;
                    return true;
                }
            }

            return false;
        }
    }
}
