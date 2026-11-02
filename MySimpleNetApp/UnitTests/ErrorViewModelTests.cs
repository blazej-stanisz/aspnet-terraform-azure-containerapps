using MySimpleNetApp.WebUI.Models;

namespace MySimpleNetApp.UnitTests;

public class ErrorViewModelTests
{
    [Fact]
    public void ShowRequestId_WhenRequestIdIsSet_ReturnsTrue()
    {
        var model = new ErrorViewModel { RequestId = "abc-123" };

        Assert.True(model.ShowRequestId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void ShowRequestId_WhenRequestIdIsMissing_ReturnsFalse(string? requestId)
    {
        var model = new ErrorViewModel { RequestId = requestId };

        Assert.False(model.ShowRequestId);
    }
}
