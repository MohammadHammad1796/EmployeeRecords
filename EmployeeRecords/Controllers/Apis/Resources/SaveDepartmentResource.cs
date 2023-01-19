using System.ComponentModel.DataAnnotations;

namespace EmployeeRecords.Controllers.Apis.Resources;

public class SaveDepartmentResource
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; }
}