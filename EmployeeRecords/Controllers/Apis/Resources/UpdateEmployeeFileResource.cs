using System.ComponentModel.DataAnnotations;

namespace EmployeeRecords.Controllers.Apis.Resources;

public class UpdateEmployeeFileResource
{
    [Required]
    public IFormFile File { get; set; }
}