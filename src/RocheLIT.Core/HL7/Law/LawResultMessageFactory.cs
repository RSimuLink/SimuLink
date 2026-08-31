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
            ConnectionSettings settings,
            DateTimeOffset? timestamp = null,
            string sampleVolume = "",
            string rackId = "",
            string carrierPosition = "",
            bool includeInventory = false,
            bool includeCtValues = false,
            IReadOnlyDictionary<string, string>? targetResults = null)
        {
            ArgumentNullException.ThrowIfNull(sampleType);
            ArgumentNullException.ThrowIfNull(test);
            ArgumentNullException.ThrowIfNull(selectedTarget);
            ArgumentNullException.ThrowIfNull(settings);

            var when = timestamp ?? DateTimeOffset.Now;
            const string statusCode = "F";
            var responsibleObserver = string.IsNullOrWhiteSpace(settings.SendingApplication)
                ? string.Empty
                : $"{settings.SendingApplication}SYSTEM";

            var observations = new List<ChannelResult>();
            var setId = 1;
            foreach (var target in test.Targets)
            {
                var isSelected = ReferenceEquals(target, selectedTarget);
                var rawValue = TryGetTargetResult(targetResults, target, out var targetValue)
                    ? targetValue
                    : isSelected
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
                    Status = statusCode,
                    ResponsibleObserver = responsibleObserver,
                    ObservationMethod = "c6800^Roche~c6800.504^Roche",
                    AnalysisDateTime = when.ToString("yyyyMMddHHmmss"),
                    ObservationType = "RSLT",
                });

                setId++;
            }

            var message = new LawResultMessage
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
                    CarrierId = rackId.Trim(),
                    CarrierPosition = carrierPosition.Trim(),
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

            foreach (var testResult in message.Tests)
            {
                if (includeInventory)
                {
                    testResult.Reagents.AddRange(DefaultInventory());
                }

                if (includeCtValues)
                {
                    testResult.Observations.Add(BuildCtValuesObservation(testResult, when));
                }
            }

            return message;
        }

        public static LawResultMessage CreateControl(
            TestType test,
            ControlResult control,
            ConnectionSettings settings,
            DateTimeOffset? timestamp = null,
            string sampleId = "",
            bool includeInventory = true)
        {
            ArgumentNullException.ThrowIfNull(test);
            ArgumentNullException.ThrowIfNull(control);
            ArgumentNullException.ThrowIfNull(settings);

            var when = timestamp ?? DateTimeOffset.Now;
            var controlSampleId = string.IsNullOrWhiteSpace(sampleId)
                ? GenerateControlSampleId()
                : sampleId.Trim();
            var responsibleObserver = string.IsNullOrWhiteSpace(settings.SendingApplication)
                ? string.Empty
                : $"{settings.SendingApplication}SYSTEM";

            var observations = new List<ChannelResult>();
            var setId = 1;
            var targets = test.Targets.Count > 0
                ? test.Targets
                : new List<Target> { new() };
            foreach (var target in targets)
            {
                observations.Add(BuildControlObservation(
                    target,
                    control,
                    setId.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    when,
                    responsibleObserver));
                setId++;
            }

            var testResult = new LawTestResult
            {
                SetId = "1",
                TestCode = CodedElement.Parse(test.UniversalServiceIdentifier),
                OrderControl = "SC",
                OrderStatus = "CM",
                EmitTcdWhenEmpty = true,
                Observations = observations,
            };

            if (includeInventory)
            {
                testResult.Reagents.AddRange(DefaultInventory());
            }

            // The HIM requires the supplemental OBX to be sent for control
            // results even when the primary result is invalid.
            testResult.Observations.Add(BuildCtValuesObservation(testResult, when));

            return new LawResultMessage
            {
                SendingApplication = settings.SendingApplication,
                ReceivingApplication = settings.ReceivingApplication,
                MessageDateTime = when.ToString("yyyyMMddHHmmsszzz").Replace(":", string.Empty),
                MessageControlId = Guid.NewGuid().ToString(),
                Specimen = new Specimen
                {
                    SampleId = controlSampleId,
                    Namespace = "ROCHE",
                    SpecimenType = new CodedElement(),
                    Role = "Q",
                },
                ContainerInventories =
                {
                    new ReagentInventory
                    {
                        SubstanceId = new CodedElement(control.Name, "", "99ROC"),
                        Status = new CodedElement("OK", "", "HL70383"),
                        SubstanceType = new CodedElement("CO", "", "HL70384"),
                        ExpiryDateTime = "20250930235959+0200",
                        LotNumber = ControlLot(control),
                    },
                },
                Tests = { testResult },
            };
        }

        public static IReadOnlyList<ControlResult> ControlResultsFor(TestType test)
        {
            ArgumentNullException.ThrowIfNull(test);

            if (test.ControlResults.Count > 0)
            {
                return test.ControlResults;
            }

            return new[]
            {
                new ControlResult { Name = $"{test.Name} (+) C", IsPositive = true },
                new ControlResult { Name = "(-) C", IsPositive = false },
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

        private static bool TryGetTargetResult(
            IReadOnlyDictionary<string, string>? targetResults,
            Target target,
            out string value)
        {
            value = string.Empty;
            if (targetResults is null || targetResults.Count == 0)
            {
                return false;
            }

            if (targetResults.TryGetValue(target.ObservationIdentifier, out var exactValue) ||
                targetResults.TryGetValue(target.Name, out exactValue))
            {
                value = exactValue ?? string.Empty;
                return true;
            }

            var identifier = CodedElement.Parse(target.ObservationIdentifier).Identifier;
            if (identifier.Length > 0 && targetResults.TryGetValue(identifier, out var identifierValue))
            {
                value = identifierValue ?? string.Empty;
                return true;
            }

            return false;
        }

        private sealed record ResolvedResult(string Value, CodedElement Interpretation);

        private static string GenerateControlSampleId()
        {
            Span<byte> bytes = stackalloc byte[20];
            System.Security.Cryptography.RandomNumberGenerator.Fill(bytes);

            var chars = new char[21];
            chars[0] = 'C';
            for (var i = 0; i < bytes.Length; i++)
            {
                chars[i + 1] = (char)('0' + (bytes[i] % 10));
            }

            return new string(chars);
        }

        private static string ControlLot(ControlResult control)
        {
            if (!control.IsPositive)
            {
                return "K12238";
            }

            return control.Name.Contains("Malaria", StringComparison.OrdinalIgnoreCase)
                ? "M06991"
                : "K15555";
        }

        private static ChannelResult BuildControlObservation(
            Target target,
            ControlResult control,
            string setId,
            DateTimeOffset when,
            string responsibleObserver)
        {
            var observation = CodedElement.Parse(target.ObservationIdentifier);
            if (string.IsNullOrWhiteSpace(observation.Identifier))
            {
                observation = new CodedElement(target.Name, target.Name, "99ROC");
            }

            var numeric = ControlNumericValue(control.Name);
            return new ChannelResult
            {
                SetId = setId,
                ValueType = numeric.ValueType,
                ObservationId = observation,
                SubId = "1",
                Value = numeric.Value,
                Units = numeric.Units,
                Interpretation = new CodedElement("VAL", "", "99ROC"),
                Status = "F",
                ResponsibleObserver = responsibleObserver,
                ObservationMethod = "c6800^Roche~c6800.2567^Roche",
                AnalysisDateTime = when.ToString("yyyyMMddHHmmss"),
                EquipmentInstanceId = "6-2567-250313-0039",
                ObservationType = "RSLT",
            };
        }

        private static (string ValueType, string Value, CodedElement? Units) ControlNumericValue(
            string controlName)
        {
            if (controlName.Contains(" H ", StringComparison.OrdinalIgnoreCase))
            {
                return ("NM", "281", new CodedElement("10*3.{copies}/mL", "", "UCUM"));
            }

            if (controlName.Contains(" L ", StringComparison.OrdinalIgnoreCase))
            {
                return ("NM", "630", new CodedElement("10*0.{copies}/mL", "", "UCUM"));
            }

            return ("ST", "Valid", null);
        }

        private static IEnumerable<ReagentInventory> DefaultInventory()
        {
            yield return Inventory("Wash reagent", "LI", "20260228225959+0100", "M03540");
            yield return Inventory("Lysis reagent", "LI", "20260430215959+0200", "M05831");
            yield return Inventory("MGP cassette", "SC", "20251130225959+0100", "K23431");
            yield return Inventory("Reagent cassette", "MR", "20260131225959+0100", "M08263");
            yield return Inventory("Diluent", "DI", "20260331215959+0200", "M05812");
            yield return Inventory("Amplification plate", "SC", "20260531215959+0200", "040");
            yield return Inventory("Processing plate", "SC", "20260331215959+0200", "073");
        }

        private static ReagentInventory Inventory(
            string substance, string substanceType, string expiryDateTime, string lotNumber) => new()
            {
                SubstanceId = new CodedElement(substance, "", "99ROC"),
                Status = new CodedElement("OK", "", "HL70383"),
                SubstanceType = new CodedElement(substanceType, "", "HL70384"),
                ExpiryDateTime = expiryDateTime,
                LotNumber = lotNumber,
            };

        private static ChannelResult BuildCtValuesObservation(LawTestResult test, DateTimeOffset when)
        {
            var primary = test.Observations.FirstOrDefault();
            return new ChannelResult
            {
                SetId = (test.Observations.Count + 1).ToString(
                    System.Globalization.CultureInfo.InvariantCulture),
                ValueType = "NA",
                ObservationId = new CodedElement(
                    primary?.ObservationId.Identifier ?? "WNV",
                    primary?.ObservationId.Text ?? "WNV",
                    "99ROC^S_OTHER^Other Supplemental^IHELAW"),
                SubId = primary?.SubId ?? "1",
                Value = "37.04^36.32",
                Interpretation = new CodedElement(
                    primary?.Interpretation?.Identifier ?? "RR",
                    "",
                    primary?.Interpretation?.CodingSystem ?? "99ROC"),
                Status = primary?.Status ?? "F",
                ResponsibleObserver = primary?.ResponsibleObserver ?? "X800DMSYSTEM",
                ObservationMethod = primary?.ObservationMethod ?? "c6800^Roche~c6800.504^Roche",
                AnalysisDateTime = primary?.AnalysisDateTime ?? when.ToString("yyyyMMddHHmmss"),
                EquipmentInstanceId = primary?.EquipmentInstanceId ?? string.Empty,
                ObservationType = "RSLT",
            };
        }
    }
}
