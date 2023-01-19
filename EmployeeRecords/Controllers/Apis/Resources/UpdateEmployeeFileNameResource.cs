using System.ComponentModel.DataAnnotations;

namespace EmployeeRecords.Controllers.Apis.Resources;

public class UpdateEmployeeFileNameResource
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; }
}