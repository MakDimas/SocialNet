using System.Text;

namespace SocialNet.Core.Models.HealthCheckModels;

public class HealthCheckResultModel
{
    public string Name { get; set; }
    public string Status { get; set; }
    public string Description { get; set; }
    public string Error { get; set; }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Name: {Name}");
        if (Description == null && Error == null)
        {
            sb.Append($"Status: {Status}");
        }
        else
        {
            sb.AppendLine($"Status: {Status}");
        }

        if (Description != null && Error != null)
        {
            sb.AppendLine($"Description: {Description}");
            sb.Append($"Error: {Error}");
        }
        else if (Description == null && Error != null)
        {
            sb.Append($"Error: {Error}");
        }
        else if (Description != null && Error == null)
        {
            sb.Append($"Description: {Description}");
        }

        return sb.ToString();
    }
}
