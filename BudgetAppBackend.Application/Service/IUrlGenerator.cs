namespace BudgetAppBackend.Application.Service
{
    public interface IUrlGenerator
    {
        string GenerateUrl(string path, object routeValues);
    }
}
