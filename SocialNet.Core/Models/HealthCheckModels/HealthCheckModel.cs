using System.Text;

namespace SocialNet.Core.Models.HealthCheckModels;

public class HealthCheckModel
{
    public string Status { get; set; }
    public IEnumerable<HealthCheckResultModel> Results { get; set; }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"\nStatus: {Status}");
        sb.AppendLine($"Results:");
        foreach (var result in Results)
        {
            sb.Append(result.ToString());
        }

        return sb.ToString();
    }
}
