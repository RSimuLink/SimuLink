using RocheLIT.Models;
using RocheLIT.Models.Law;

namespace RocheLIT.HL7.Law
{
    /// <summary>
    /// Projects the UI's flat result selection (a test, its targets, a chosen
    /// value/flag/status) into the rich <see cref="LawResultMessage"/> consumed
    /// by <see cref="LawOulR22Builder"/>.
    ///
    /// Every target in the test becomes its own OBX channel: the selected target
    /// carries the user-entered value and flag, while the remaining targets fall
    /// back to their first configured observation value with a normal flag. This
    /// keeps the message multi-channel even though the UI edits one target at a
    /// time.
    /// </summary>
    public static class LawResultMessageFactory
    {
        public static LawResultMessage Create(
            string sampleId,
            SampleType sampleType,
            TestType test,
            Target selectedTarget,
            string value,
            ResultFlag flag,
            ResultStatus status,
            ConnectionSettings settings,
            DateTimeOffset? timestamp = null,
            string sampleVolume = "")
        {
            ArgumentNullException.ThrowIfNull(sampleType);
            ArgumentNullException.ThrowIfNull(test);
            ArgumentNullException.ThrowIfNull(selectedTarget);
            ArgumentNullException.ThrowIfNull(settings);

            var when = timestamp ?? DateTimeOffset.Now;
            var statusCode = status.ToHl7Code();
            var responsibleObserver = string.IsNullOrWhiteSpace(settings.SendingApplication)
                ? string.Empty
                : $"{settings.SendingApplication}SYSTEM";

            var observations = new List<ChannelResult>();
            var setId = 1;
            foreach (var target in test.Targets)
            {
                var isSelected = ReferenceEquals(target, selectedTarget);
                var rawValue = isSelected
                    ? value
                    : target.ObservationValues.FirstOrDefault() ?? string.Empty;
                var result = ResolveTargetResult(target, rawValue, isSelected ? flag : ResultFlag.Normal);

                observations.Add(new ChannelResult
                {
                    SetId = setId.ToString(),
                    ValueType = "ST",
                    ObservationId = CodedElement.Parse(target.ObservationIdentifier),
                    SubId = "1",
                    Value = result.Value,
                    Interpretation = result.Interpretation,
                    ResultStatus = statusCode,
                    ResponsibleObserver = responsibleObserver,
                    ObservationMethod = "c6800^Roche~c6800.504^Roche",
                    AnalysisDateTime = when.ToString("yyyyMMddHHmmss"),
                    ObservationType = "RSLT",
                });

                setId++;
            }

            return new LawResultMessage
            {
                SendingApplication = settings.SendingApplication,
                ReceivingApplication = settings.ReceivingApplication,
                MessageDateTime = when.ToString("yyyyMMddHHmmsszzz").Replace(":", string.Empty),
                MessageControlId = Guid.NewGuid().ToString(),
                Specimen = new Specimen
                {
                    SampleId = sampleId,
                    SpecimenType = CodedElement.Parse(
                        sampleType.SpecimenCode.Length > 0 ? sampleType.SpecimenCode : sampleType.Hl7Code),
                    Role = "P",
                },
                Tests =
                {
                    new LawTestResult
                    {
                        SetId = "1",
                        TestCode = CodedElement.Parse(test.UniversalServiceIdentifier),
                        OrderControl = "SC",
                        OrderStatus = "CM",
                        ConsumptionVolume = FormatConsumptionVolume(sampleVolume),
                        Observations = observations,
                    },
                },
            };
        }

        public static string FormatConsumptionVolume(string volume)
        {
            if (string.IsNullOrWhiteSpace(volume))
            {
                return string.Empty;
            }

            var digits = new string(volume.TakeWhile(c => char.IsDigit(c) || c == '.').ToArray());
            if (digits.Length == 0)
            {
                digits = new string(volume.Where(char.IsDigit).ToArray());
            }

            return digits.Length == 0 ? string.Empty : $"{digits}^uL&&UCUM";
        }

        private static ResolvedResult ResolveTargetResult(
            Target target, string rawValue, ResultFlag fallbackFlag)
        {
            var value = rawValue.Trim();
            if (value.Length == 0)
            {
                return new ResolvedResult(string.Empty, new CodedElement(fallbackFlag.ToHl7Code()));
            }

            var codeIndex = IndexOf(target.ObservationValues, value);
            if (codeIndex >= 0 && codeIndex < target.InterpretationCodes.Count)
            {
                var displayValue = target.InterpretationCodes[codeIndex];
                return new ResolvedResult(displayValue, new CodedElement(value, "", "99ROC"));
            }

            var textIndex = IndexOf(target.InterpretationCodes, value);
            if (textIndex >= 0 && textIndex < target.ObservationValues.Count)
            {
                return new ResolvedResult(
                    value, new CodedElement(target.ObservationValues[textIndex], "", "99ROC"));
            }

            return new ResolvedResult(value, new CodedElement(fallbackFlag.ToHl7Code()));
        }

        private static int IndexOf(IReadOnlyList<string> values, string value)
        {
            for (var i = 0; i < values.Count; i++)
            {
                if (string.Equals(values[i], value, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            return -1;
        }

        private sealed record ResolvedResult(string Value, CodedElement Interpretation);
    }
}
