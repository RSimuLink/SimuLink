using RocheSimuLink.Models;
using RocheSimuLink.Models.Law;

namespace RocheSimuLink.HL7.Law
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
            DateTimeOffset? timestamp = null)
        {
            ArgumentNullException.ThrowIfNull(sampleType);
            ArgumentNullException.ThrowIfNull(test);
            ArgumentNullException.ThrowIfNull(selectedTarget);
            ArgumentNullException.ThrowIfNull(settings);

            var when = timestamp ?? DateTimeOffset.Now;
            var statusCode = status.ToHl7Code();

            var observations = new List<ChannelResult>();
            var setId = 1;
            foreach (var target in test.Targets)
            {
                var isSelected = ReferenceEquals(target, selectedTarget);
                var channelValue = isSelected
                    ? value
                    : target.ObservationValues.FirstOrDefault() ?? string.Empty;
                var channelFlag = isSelected ? flag : ResultFlag.Normal;

                observations.Add(new ChannelResult
                {
                    SetId = setId.ToString(),
                    ValueType = "ST",
                    ObservationId = CodedElement.Parse(target.ObservationIdentifier),
                    SubId = "1",
                    Value = channelValue,
                    Interpretation = new CodedElement(channelFlag.ToHl7Code()),
                    ResultStatus = statusCode,
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
                        Observations = observations,
                    },
                },
            };
        }
    }
}
