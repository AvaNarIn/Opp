using System.ComponentModel.DataAnnotations;

public class CreateProjectRequest
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = "";

    [StringLength(2000)]
    public string Description { get; set; } = "";

    [StringLength(1000)]
    public string Link { get; set; } = "";
}