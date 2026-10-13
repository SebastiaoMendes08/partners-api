using Partners.Application.Partners;

namespace Partners.UnitTests;

public class VersionTokenTests
{
    [Fact]
    public void Encoded_version_can_be_decoded()
    {
        byte[] version = [0, 0, 0, 0, 0, 0, 7, 209];

        var token = VersionToken.Encode(version);

        Assert.True(VersionToken.TryDecode(token, out var decoded));
        Assert.Equal(version, decoded);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-base64!")]
    [InlineData("AAAA")]
    [InlineData("AAAAAAAAAAAAAAAA")]
    public void Malformed_token_is_rejected(string? token)
    {
        Assert.False(VersionToken.TryDecode(token, out _));
    }
}
