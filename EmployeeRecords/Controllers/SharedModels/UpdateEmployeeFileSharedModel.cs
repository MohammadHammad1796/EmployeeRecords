using System.ComponentModel.DataAnnotations;

namespace EmployeeRecords.Controllers.SharedModels;

public class UpdateEmployeeFileSharedModel
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; }

    public IFormFile? File { get; set; }
}