using System.ComponentModel.DataAnnotations;

namespace EmployeeRecords.Controllers.SharedModels;

public class SaveEmployeeSharedModel
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; }

    public int DepartmentId { get; set; }

    [Display(Name = "Date of birth")]
    public DateOnly DateOfBirth { get; set; }

    [Required]
    [MaxLength(200)]
    public string Address { get; set; }
}