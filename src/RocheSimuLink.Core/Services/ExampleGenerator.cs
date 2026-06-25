using System.Globalization;
using System.Text;
using RocheSimuLink.HL7.Law;
using RocheSimuLink.HL7.Models;
using RocheSimuLink.Models;
using RocheSimuLink.Models.Law;
using RocheSimuLink.Models.Workflows;

namespace RocheSimuLink.Services
{
    /// <summary>
    /// Produces the LAB-27/28/29 HL7 example messages from the values typed in
    /// the main UI, without needing a LIS connection. It reuses the same x800DM
    /// IHE-LAW builders used on the wire, so the generated examples match what
    /// the simulator would actually send/receive. Control ids are linked across
    /// the conversation (each response acknowledges its request).
    /// </summary>
    public sealed class ExampleGenerator
    {
        private readonly ConnectionSettings _settings;

        public ExampleGenerator(ConnectionSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        /// <summary>
        /// Builds the selected workflows. Returns them in canonical conversation
        /// order regardless of the order requested. A timestamp can be supplied
        /// for deterministic output (tests); otherwise "now" is used.
        /// </summary>
        public IReadOnlyList<GeneratedMessage> Generate(
            ExampleGeneratorInput input,
            IEnumerable<ExampleWorkflow> selected,
            DateTimeOffset? timestamp = null)
        {
            ArgumentNullException.ThrowIfNull(input);
            ArgumentNullException.ThrowIfNull(selected);

            var wanted = new HashSet<ExampleWorkflow>(selected);
            var when = timestamp ?? DateTimeOffset.Now;

            // Identities linking the conversation together.
            var instrument = _settings.SendingApplication;   // e.g. SimuLink / X800DM
            var host = _settings.ReceivingApplication;        // e.g. LIS / Host
            var sampleId = input.SampleId;
            var queryTag = Guid.NewGuid().ToString();
            var qbpId = Guid.NewGuid().ToString();
            var omlId = Guid.NewGuid().ToString();
            var oulId = Guid.NewGuid().ToString();

            var results = new List<GeneratedMessage>();

            void Add(ExampleWorkflow wf, Hl7Message msg)
            {
                var info = ExampleWorkflowInfo.For(wf);
                results.Add(new GeneratedMessage(
                    wf, info.Label, info.MessageType, info.Direction, msg.RawMessage));
            }

            if (wanted.Contains(ExampleWorkflow.Lab27WorkOrderRequest))
            {
                Add(ExampleWorkflow.Lab27WorkOrderRequest, LawQbpQ11Builder.Build(new WorkOrderQuery
                {
                    Header = Header(instrument, host, when, qbpId),
                    QueryTag = queryTag,
                    SampleId = sampleId,
                }));
            }

            if (wanted.Contains(ExampleWorkflow.Lab27RequestAcknowledge))
            {
                Add(ExampleWorkflow.Lab27RequestAcknowledge, LawRspK11Builder.Build(new WorkOrderQueryResponse
                {
                    Header = Header(host, instrument, when, Guid.NewGuid().ToString()),
                    AcknowledgmentCode = "AA",
                    AcknowledgedControlId = qbpId,
                    QueryTag = queryTag,
                    QueryResponseStatus = "OK",
                    EchoedQueryTag = queryTag,
                    SampleId = sampleId,
                }));
            }

            if (wanted.Contains(ExampleWorkflow.Lab28TestOrderSubmission))
            {
                Add(ExampleWorkflow.Lab28TestOrderSubmission, LawOmlO33Builder.Build(new TestOrderMessage
                {
                    Header = Header(host, instrument, when, omlId),
                    Specimen = Specimen(input, "P"),
                    Orders =
                    {
                        new TestOrder
                        {
                            OrderControl = "NW",
                            TransactionDateTime = when.ToString("yyyyMMddHHmmss"),
                            PlacerOrderNumber = "1",
                            TestCode = CodedElement.Parse(input.Test.UniversalServiceIdentifier),
                            ConsumptionVolume = ConsumptionVolume(input.SampleVolume),
                        },
                    },
                }));
            }

            if (wanted.Contains(ExampleWorkflow.Lab28TestOrderResponse))
            {
                Add(ExampleWorkflow.Lab28TestOrderResponse, LawOrlO34Builder.Build(new TestOrderResponse
                {
                    Header = Header(instrument, host, when, Guid.NewGuid().ToString()),
                    AcknowledgmentCode = "AA",
                    AcknowledgedControlId = omlId,
                    Specimen = Specimen(input, "P"),
                    Orders =
                    {
                        new TestOrderResponseItem
                        {
                            OrderControl = "OK",
                            PlacerOrderNumber = "1",
                            OrderStatus = "SC",
                        },
                    },
                }));
            }

            if (wanted.Contains(ExampleWorkflow.Lab29TestResult))
            {
                var result = ResultMessage(input, when, "P");
                result.MessageControlId = oulId;
                Add(ExampleWorkflow.Lab29TestResult, LawOulR22Builder.Build(result));
            }

            if (wanted.Contains(ExampleWorkflow.Lab29ResultAccepted))
            {
                Add(ExampleWorkflow.Lab29ResultAccepted, LawAckR22Builder.Build(new ResultAcknowledgment
                {
                    Header = Header(host, instrument, when, Guid.NewGuid().ToString()),
                    AcknowledgmentCode = "AA",
                    AcknowledgedControlId = oulId,
                }));
            }

            if (wanted.Contains(ExampleWorkflow.ControlTestResult))
            {
                // Same as the result message but with a QC specimen role ("Q").
                var control = ResultMessage(input, when, "Q");
                Add(ExampleWorkflow.ControlTestResult, LawOulR22Builder.Build(control));
            }

            return results;
        }

        /// <summary>
        /// Renders the generated messages as a single text document with a
        /// labelled header per message, suitable for the on-screen view, the
        /// clipboard, and file export.
        /// </summary>
        public static string Render(IEnumerable<GeneratedMessage> messages)
        {
            ArgumentNullException.ThrowIfNull(messages);

            var sb = new StringBuilder();
            foreach (var m in messages)
            {
                if (sb.Length > 0)
                {
                    sb.AppendLine().AppendLine();
                }

                sb.AppendLine($"===== {m.Label}  [{m.MessageType}]  ({m.Direction}) =====");
                // Present each HL7 segment on its own line for readability.
                foreach (var segment in m.RawMessage.Split('\r'))
                {
                    sb.AppendLine(segment);
                }
            }

            return sb.ToString().TrimEnd();
        }

        private LawMessageHeader Header(
            string sending, string receiving, DateTimeOffset when, string controlId) => new()
            {
                SendingApplication = sending,
                ReceivingApplication = receiving,
                MessageDateTime = FormatTimestamp(when),
                MessageControlId = controlId,
            };

        private static Specimen Specimen(ExampleGeneratorInput input, string role) => new()
        {
            SampleId = input.SampleId,
            Namespace = "ROCHE",
            SpecimenType = new CodedElement(input.SampleType.Hl7Code),
            Role = role,
        };

        private LawResultMessage ResultMessage(
            ExampleGeneratorInput input, DateTimeOffset when, string role)
        {
            var target = input.Target ?? input.Test.Targets.FirstOrDefault() ?? new Target();
            var message = LawResultMessageFactory.Create(
                input.SampleId,
                input.SampleType,
                input.Test,
                target,
                input.ResultValue,
                input.ResultFlag,
                input.ResultStatus,
                _settings,
                when);

            message.Specimen.Role = role;
            foreach (var test in message.Tests)
            {
                if (string.IsNullOrEmpty(test.ConsumptionVolume))
                {
                    test.ConsumptionVolume = ConsumptionVolume(input.SampleVolume);
                }
            }

            return message;
        }

        private static string FormatTimestamp(DateTimeOffset when) =>
            when.ToString("yyyyMMddHHmmsszzz", CultureInfo.InvariantCulture).Replace(":", string.Empty);

        /// <summary>
        /// Converts a UI sample-volume label (e.g. "500 uL", "200uL") into the
        /// HL7 UCUM consumption form "500^uL&amp;&amp;UCUM". Returns empty when no
        /// numeric volume can be read, so the TCD volume field is simply omitted.
        /// </summary>
        private static string ConsumptionVolume(string volume)
        {
            if (string.IsNullOrWhiteSpace(volume))
            {
                return string.Empty;
            }

            var digits = new string(volume.TakeWhile(c => char.IsDigit(c) || c == '.').ToArray());
            if (digits.Length == 0)
            {
                // Fall back to the first run of digits anywhere in the label.
                digits = new string(volume.Where(char.IsDigit).ToArray());
            }

            return digits.Length == 0 ? string.Empty : $"{digits}^uL&&UCUM";
        }
    }
}
