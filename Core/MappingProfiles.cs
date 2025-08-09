using AutoMapper;
using CRMService.Models.Entity;
using CRMService.Models.WebHook;
using CRMService.Dto.Entity;
using CRMService.Models.Authorization;
using CRMService.Dto.Authorization;

namespace CRMService.Core
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles() 
        {
            CreateMap<CompanyCategory, CategoryDto>();
            CreateMap<CategoryDto, CompanyCategory>();
            CreateMap<Company, CompanyDto>();
            CreateMap<CompanyDto, Company>();
            CreateMap<Manufacturer, ManufacturerDto>();
            CreateMap<ManufacturerDto, Manufacturer>();
            CreateMap<Kind, KindDto>();
            CreateMap<KindDto, Kind>();
            CreateMap<KindsParameter, KindParameterDto>();
            CreateMap<KindParameterDto, KindsParameter>();
            CreateMap<Model, ModelDto>();
            CreateMap<ModelDto, Model>();
            CreateMap<Parameter, EquipmentParameterDto>();
            CreateMap<EquipmentParameterDto, Parameter>();
            CreateMap<MaintenanceEntity, MaintenanceEntityDto>();
            CreateMap<MaintenanceEntityDto, MaintenanceEntity>();
            CreateMap<EquipmentDto, Equipment>();
            CreateMap<Equipment, EquipmentDto>();
            CreateMap<Employee, EmployeeDto>();
            CreateMap<EmployeeDto, Employee>();
            CreateMap<Group, GroupDto>();
            CreateMap<StatusDto, IssueStatus>();
            CreateMap<IssueStatus, StatusDto>();
            CreateMap<Issue, IssueDto>();
            CreateMap<PriorityDto, IssuePriority>();
            CreateMap<IssuePriority, PriorityDto>();
            CreateMap<TaskTypeDto, IssueType>();
            CreateMap<IssueType, TaskTypeDto>();
            CreateMap<MaintenanceEntityWebHook, MaintenanceEntity>();
            CreateMap<User, UserDto>();
            CreateMap<UserDto, User>();
            CreateMap<Models.Authorization.Role, RoleDto>();
            CreateMap<RoleDto, Models.Authorization.Role>();
            CreateMap<Session, SessionDto>();
            CreateMap<UserRole, UserRoleDto>();
            CreateMap<BlockReason, BlockReasonDto>();
        }
    }
}
