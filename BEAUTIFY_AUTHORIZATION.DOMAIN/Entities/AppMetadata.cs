using System.Text.Json.Serialization;

namespace BEAUTIFY_AUTHORIZATION.DOMAIN.Entities;
public class AppMetadata
{
    public string Provider { get; set; }
    public List<string> Providers { get; set; }
}

public class UserMetadata
{
    [JsonPropertyName("avatar_url")] public string AvatarUrl { get; set; }
    [JsonPropertyName("email")] public string Email { get; set; }
    [JsonPropertyName("email_verified")] public bool EmailVerified { get; set; }
    [JsonPropertyName("full_name")] public string FullName { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; }
    [JsonPropertyName("picture")] public string Picture { get; set; }

    [JsonPropertyName("preferred_username")]
    public string PreferredUsername { get; set; }
}