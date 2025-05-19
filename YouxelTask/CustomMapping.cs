using FileStorage.Application.Dtos;
using MyArchitechture.Application.Dtos;
using MyArchitechture.Domain.Entities;

namespace MyArchitechture.Api
{
	public class CustomMappingProfile : AutoMapper.Profile
	{
		public CustomMappingProfile()
		{
			//CreateMap<File, FileDto>().ReverseMap();
			CreateMap<Employee, EmployeeDto>().ReverseMap();
		}
	}
	
}
