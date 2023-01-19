using AutoMapper;
using EmployeeRecords.Controllers.Apis.Resources;
using EmployeeRecords.Core.Models;
using File = EmployeeRecords.Core.Models.File;

namespace EmployeeRecords.Mapping;

public class ApplicationMappingProfile : Profile
{
    public ApplicationMappingProfile()
    {
        CreateMap<Employee, EmployeeResource>()
            .ForMember(r => r.DateOfBirth, opt => opt.MapFrom(e => DateOnly.FromDateTime(e.DateOfBirth)));
        CreateMap<File, FileResource>();

        CreateMap<SaveEmployeeResource, Employee>()
            .ForMember(e => e.DateOfBirth, opt => opt.MapFrom(r => new DateTime(r.DateOfBirth.Year, r.DateOfBirth.Month, r.DateOfBirth.Day)));
        CreateMap<SaveDepartmentResource, Department>();
    }
}