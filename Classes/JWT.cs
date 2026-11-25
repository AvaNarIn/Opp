public class JwtSettings
{
    public string Key { get; set; } = "";
    public string Algorithm { get; set; } = "";
    public int AccessTokenMinutes { get; set; }
}