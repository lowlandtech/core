using System.ComponentModel.DataAnnotations.Schema;

namespace LowlandTech.Plugin1.Domain.Entities;

public class Plugin1Entity
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public bool IsSaving { get; set; }

    [NotMapped]
    public bool IsSaved { get; set; }
}