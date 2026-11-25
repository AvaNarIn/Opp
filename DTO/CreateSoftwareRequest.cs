using System.ComponentModel.DataAnnotations;

public class CreateSoftwareRequest
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = "";

    [Required]
    [StringLength(100)]
    public string Version { get; set; } = "";

    [StringLength(1000)]
    public string Link { get; set; } = "";
}