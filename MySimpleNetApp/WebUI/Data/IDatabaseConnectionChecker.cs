namespace MySimpleNetApp.WebUI.Data;

public interface IDatabaseConnectionChecker
{
    Task<bool> IsConnectedAsync(CancellationToken cancellationToken = default);
}
