using RocheSimuLink.HL7.Builders;
using RocheSimuLink.Models;
using Xunit;

namespace RocheSimuLink.Core.Tests.Builders;

public class SegmentBuilderTests
{
    [Fact]
    public void Msh_StartsWithFieldSeparatorAndEncodingChars()
    {
        var msh = MshBuilder.Build("OUL^R22");

        Assert.StartsWith("MSH|^~\\&|", msh);
    }

    [Fact]
    public void Msh_ContainsMessageTypeAndVersion()
    {
        var msh = MshBuilder.Build("OUL^R22");
        var fields = msh.Split('|');

        Assert.Equal("OUL^R22", fields[8]);
        Assert.Equal("2.5.1", fields[11]);
    }

    [Fact]
    public void Pid_PlacesPatientIdInAssigningAuthority()
    {
        var pid = PidBuilder.Build("SID123");

        Assert.Equal("PID|1||SID123^^^SimuLink||Doe^John", pid);
    }

    [Fact]
    public void Orc_AcknowledgesOrder()
    {
        var orc = OrcBuilder.Build("ORD-7");

        Assert.Equal("ORC|OK|ORD-7", orc);
    }

    [Fact]
    public void Obr_IncludesUniversalServiceIdentifier()
    {
        var obr = ObrBuilder.Build("SID123", "GLU^Glucose^L");

        Assert.Equal("OBR|1|SID123||GLU^Glucose^L", obr);
    }

    [Fact]
    public void Spm_IncludesSpecimenCode()
    {
        var spm = SpmBuilder.Build("SID123", "SER");

        Assert.Equal("SPM|1|SID123||SER", spm);
    }

    [Fact]
    public void Obx_PlacesValueAndInterpretation()
    {
        var target = new Target { ObservationIdentifier = "GLU^Glucose^L" };

        var obx = ObxBuilder.Build(target, "140", "H");
        var fields = obx.Split('|');

        Assert.Equal("140", fields[5]); // OBX-5 value
        Assert.Equal("H", fields[8]);   // OBX-8 abnormal flag
        Assert.Equal("F", fields[11]);  // OBX-11 default status (Final)
    }
}
