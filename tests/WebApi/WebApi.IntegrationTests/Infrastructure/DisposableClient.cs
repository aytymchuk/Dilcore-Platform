namespace Dilcore.WebApi.IntegrationTests.Infrastructure;

internal sealed class DisposableClient<T>(T client, HttpClient httpClient) : IDisposableClient<T>
{
    public T Client => client;
    public void Dispose() => httpClient.Dispose();
}
