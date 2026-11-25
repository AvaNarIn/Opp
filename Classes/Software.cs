namespace Bugtrack.Api.Classes;

public class Software
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string Version { get; set; } = "";

    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public string Link { get; set; } = "";

    public void ChangeName(string name)
    {
        Name = name;
    }

    public void ChangeVersion(string version)
    {
        Version = version;
    }
}