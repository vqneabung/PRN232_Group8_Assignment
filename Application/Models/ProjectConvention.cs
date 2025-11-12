using System;
using System.ComponentModel.DataAnnotations;

namespace Application.Models;

public partial class ProjectConvention
{
    public int Id { get; set; }

    [MaxLength(100)]
    public string ExpectedSolutionPrefix { get; set; } = string.Empty;

    [MaxLength(100)]
    public string ExpectedSolutionSuffix { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? AdditionalRules { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}