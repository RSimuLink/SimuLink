namespace RocheSimuLink.Models.Law
{
    /// <summary>
    /// A single OBX observation in a LAW result. A test typically emits several:
    /// the quantitative value, an interpretation, and supplemental data.
    /// </summary>
    public sealed class ChannelResult
    {
        /// <summary>OBX-1 set id (e.g. "1").</summary>
        public string SetId { get; set; } = "1";

        /// <summary>OBX-2 value type (e.g. "NM" numeric, "NA" numeric array, "EI", "ST").</summary>
        public string ValueType { get; set; } = "NM";

        /// <summary>OBX-3 observation identifier (e.g. HIV^HIV^99ROC).</summary>
        public CodedElement ObservationId { get; set; } = new();

        /// <summary>OBX-4 observation sub-id (e.g. "1").</summary>
        public string SubId { get; set; } = "1";

        /// <summary>OBX-5 observation value (e.g. "130").</summary>
        public string Value { get; set; } = string.Empty;

        /// <summary>OBX-6 units, a CWE with UCUM system (e.g. 10*2.{copies}/mL^^UCUM).</summary>
        public CodedElement? Units { get; set; }

        /// <summary>OBX-8 interpretation/abnormal flag, coded (e.g. VAL^^99ROC).</summary>
        public CodedElement? Interpretation { get; set; }

        /// <summary>OBX-11 result status (e.g. "F").</summary>
        public string ResultStatus { get; set; } = "F";

        /// <summary>OBX-17 observation method / instrument (e.g. c6800^Roche~c6800.504^Roche).</summary>
        public string ObservationMethod { get; set; } = string.Empty;

        /// <summary>OBX-19 analysis date/time (yyyyMMddHHmmss).</summary>
        public string AnalysisDateTime { get; set; } = string.Empty;

        /// <summary>OBX-21 result instance / equipment id (e.g. 6-504-241029-0092).</summary>
        public string EquipmentInstanceId { get; set; } = string.Empty;

        /// <summary>OBX-29 observation type (e.g. "RSLT").</summary>
        public string ObservationType { get; set; } = "RSLT";
    }
}
