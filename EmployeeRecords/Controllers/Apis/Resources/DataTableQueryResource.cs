using System.ComponentModel.DataAnnotations;

namespace EmployeeRecords.Controllers.Apis.Resources;

public class DataTableQueryResource
{
    [Required]
    public SortResource Sort { get; set; }

    [Required]
    public PageResource Page { get; set; }

    [MaxLength(50)]
    public string? SearchQuery { get; set; }

    public bool WithTotal { get; set; }
}