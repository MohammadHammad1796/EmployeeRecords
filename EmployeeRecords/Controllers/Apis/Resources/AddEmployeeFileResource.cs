using System.ComponentModel.DataAnnotations;

namespace EmployeeRecords.Controllers.Apis.Resources;

public class AddEmployeeFileResource
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; }

    [Required]
    public IFormFile File { get; set; }
}