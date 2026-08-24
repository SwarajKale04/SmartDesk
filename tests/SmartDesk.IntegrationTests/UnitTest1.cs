using Microsoft.AspNetCore.Mvc;
using SmartDesk.API.Controllers;

namespace SmartDesk.IntegrationTests;

public class SystemEndpointTests
{
    [Fact]
    public void GetInfo_ShouldExposePublicServiceMetadata()
    {
        var controller = new SystemController();

        var result = controller.GetInfo();

        var response = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(response.Value);
    }
}
