using System.ComponentModel.DataAnnotations;

namespace EmployeeRecords.Controllers.SharedModels;

public class SaveDepartmentSharedModel
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; }
}