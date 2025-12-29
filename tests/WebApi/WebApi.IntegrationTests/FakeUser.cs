namespace Dilcore.WebApi.IntegrationTests;

public class FakeUser
{
    public string UserId { get; set; } = "test-user-id";
    public string Name { get; set; } = "TestUser";
    public string Email { get; set; } = "test@example.com";
    public string TenantId { get; set; } = "test-tenant";
    public bool IsAuthenticated { get; set; } = true;
}