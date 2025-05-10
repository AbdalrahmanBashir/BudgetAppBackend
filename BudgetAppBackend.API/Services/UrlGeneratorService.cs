using System.Text;
using BudgetAppBackend.Application.Service;

namespace BudgetAppBackend.API.Services
{
    public class UrlGeneratorService : IUrlGenerator
    {
        private readonly string _baseUrl = "https://localhost:7105/";

        public string GenerateUrl(string path, object routeValues)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException(nameof(path), "Path cannot be null or empty.");
            }

            var urlBuilder = new StringBuilder(_baseUrl.TrimEnd('/'));
            urlBuilder.Append("/");
            urlBuilder.Append(path.TrimStart('/'));

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            string userId = null;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
            var queryParams = new List<string>();

            foreach (var property in routeValues.GetType().GetProperties())
            {
                var value = property.GetValue(routeValues);
                if (value != null)
                {
                    if (property.Name == "UserId")
                    {
                        userId = Uri.EscapeDataString(value.ToString());
                    }
                    else
                    {
                        queryParams.Add($"{Uri.EscapeDataString(property.Name)}={Uri.EscapeDataString(value.ToString())}");
                    }
                }
            }

            if (!string.IsNullOrEmpty(userId))
            {
                urlBuilder.Append($"/user/{userId}/list-of-transactions");
            }

            if (queryParams.Any())
            {
                urlBuilder.Append("?");
                urlBuilder.Append(string.Join("&", queryParams));
            }

            return urlBuilder.ToString();
        }
    }
}
