using Application.Enities;
using Application.Models;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Application.Mapping
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // Mapping cho Rule
            CreateMap<RuleRequest, Rule>();
            CreateMap<Rule, RuleResponse>();

            // Mapping cho ProjectConvention
            CreateMap<ProjectConventionRequest, ProjectConvention>();
            CreateMap<ProjectConvention, ProjectConventionResponse>();

            // Mapping cho Student
            CreateMap<StudentRequest, Student>();
            CreateMap<Student, StudentResponse>()
                .ForMember(dest => dest.SubmissionCount, opt => opt.MapFrom(src => src.Submissions.Count));
        }
    }
}
