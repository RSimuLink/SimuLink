using System.Text.RegularExpressions;
using RocheLIT.Models;
using RocheLIT.Models.Law;
using RocheLIT.Models.Ucap;

namespace RocheLIT.HL7.Law
{
    /// <summary>Builds UCAP-specific OUL^R22 result messages.</summary>
    public static partial class LawUcapResultMessageFactory
    {
        [GeneratedRegex("^[A-Za-z0-9]{1,14}$")]
        private static partial Regex TestNameSuffixRegex();

        [GeneratedRegex("^[0-9]{7}$")]
        private static partial Regex UniversalServiceIdRegex();

        [GeneratedRegex("^[A-Za-z0-9]{1,15}$")]
        private static partial Regex TargetNameRegex();

        public static LawResultMessage Create(
            UcapResultRequest request,
            ConnectionSettings settings,
            DateTimeOffset? timestamp = null)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(settings);
            Validate(request);

            var when = timestamp ?? DateTimeOffset.Now;
            var responsibleObserver = string.IsNullOrWhiteSpace(settings.SendingApplication)
                ? string.Empty
                : $"{settings.SendingApplication}SYSTEM";

            var testCode = new CodedElement(request.UniversalServiceId.Trim(), "UCAP", "99ROC");
            var observations = request.Targets
                .Select((target, index) => BuildObservation(target, index + 1, when, responsibleObserver))
                .ToList();

            return new LawResultMessage
            {
                SendingApplication = settings.SendingApplication,
                ReceivingApplication = settings.ReceivingApplication,
                MessageDateTime = when.ToString("yyyyMMddHHmmsszzz").Replace(":", string.Empty),
                MessageControlId = Guid.NewGuid().ToString(),
                Specimen = new Specimen
                {
                    SampleId = request.SampleId.Trim(),
                    Namespace = "ROCHE",
                    SpecimenType = CodedElement.Parse(
                        request.SampleType.SpecimenCode.Length > 0
                            ? request.SampleType.SpecimenCode
                            : request.SampleType.Hl7Code),
                    Role = "P",
                    CarrierId = request.RackId.Trim(),
                    CarrierPosition = request.CarrierPosition.Trim(),
                },
                Tests =
                {
                    new LawTestResult
                    {
                        SetId = "1",
                        TestCode = testCode,
                        OrderControl = "SC",
                        OrderStatus = "CM",
                        ConsumptionVolume =
                            LawResultMessageFactory.FormatConsumptionVolume(request.SampleVolume),
                        Observations = observations,
                    },
                },
            };
        }

        private static void Validate(UcapResultRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.SampleId))
            {
                throw new ArgumentException("Sample ID is required.", nameof(request));
            }

            if (!TestNameSuffixRegex().IsMatch(request.TestNameSuffix.Trim()))
            {
                throw new ArgumentException(
                    "UCAP test type must contain 1 to 14 alphanumeric characters after U_.",
                    nameof(request));
            }

            if (!UniversalServiceIdRegex().IsMatch(request.UniversalServiceId.Trim()))
            {
                throw new ArgumentException("UCAP USID must contain exactly 7 digits.", nameof(request));
            }

            if (string.IsNullOrWhiteSpace(request.SampleType.SpecimenCode) &&
                string.IsNullOrWhiteSpace(request.SampleType.Hl7Code))
            {
                throw new ArgumentException("UCAP sample type is required.", nameof(request));
            }

            if (string.IsNullOrWhiteSpace(request.SampleVolume))
            {
                throw new ArgumentException("UCAP sample volume is required.", nameof(request));
            }

            if (request.Targets.Count is < 1 or > 4)
            {
                throw new ArgumentException("UCAP requires 1 to 4 targets.", nameof(request));
            }

            foreach (var target in request.Targets)
            {
                if (!TargetNameRegex().IsMatch(target.TargetName.Trim()))
                {
                    throw new ArgumentException(
                        "Each UCAP target name must contain 1 to 15 alphanumeric characters.",
                        nameof(request));
                }

                _ = InterpretationCode(target.ResultValue);
            }
        }

        private static ChannelResult BuildObservation(
            UcapTargetResult target,
            int setId,
            DateTimeOffset when,
            string responsibleObserver)
        {
            var name = target.TargetName.Trim();
            return new ChannelResult
            {
                SetId = setId.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ValueType = "ST",
                ObservationId = new CodedElement(name, name, "99ROC"),
                SubId = "1",
                Value = DisplayResult(target.ResultValue),
                Interpretation = new CodedElement(InterpretationCode(target.ResultValue), "", "99ROC"),
                Status = "F",
                ResponsibleObserver = responsibleObserver,
                ObservationMethod = "c6800^Roche~c6800.504^Roche",
                AnalysisDateTime = when.ToString("yyyyMMddHHmmss"),
                ObservationType = "RSLT",
            };
        }

        private static string DisplayResult(string value)
        {
            var normalized = value.Trim();
            if (string.Equals(normalized, "RR", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(normalized, "Reactive", StringComparison.OrdinalIgnoreCase))
            {
                return "Reactive";
            }

            if (string.Equals(normalized, "NR", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(normalized, "Non-Reactive", StringComparison.OrdinalIgnoreCase))
            {
                return "Non-Reactive";
            }

            throw new ArgumentException("UCAP result must be Reactive or Non-Reactive.");
        }

        private static string InterpretationCode(string value)
        {
            var normalized = value.Trim();
            if (string.Equals(normalized, "RR", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(normalized, "Reactive", StringComparison.OrdinalIgnoreCase))
            {
                return "RR";
            }

            if (string.Equals(normalized, "NR", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(normalized, "Non-Reactive", StringComparison.OrdinalIgnoreCase))
            {
                return "NR";
            }

            throw new ArgumentException("UCAP result must be Reactive or Non-Reactive.");
        }
    }
}
