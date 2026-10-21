using AutoMapper;
using ProjectTracker.API.DTOs;
using ProjectTracker.API.Models;

namespace ProjectTracker.API.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Project, ProjectReadDto>();
            CreateMap<ProjectCreateDto, Project>();

            CreateMap<TaskItem, TaskItemReadDto>();
            CreateMap<TaskItemCreateDto, TaskItem>();
        }
    }
}
