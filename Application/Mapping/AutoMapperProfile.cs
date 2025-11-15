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

            // Mapping cho Student
            CreateMap<StudentRequest, Student>();
            CreateMap<Student, StudentResponse>()
                .ForMember(dest => dest.SubmissionCount, opt => opt.MapFrom(src => src.Submissions.Count));

            // Mapping cho Class
            CreateMap<ClassRequest, Class>();
            CreateMap<Class, ClassResponse>()
                .ForMember(dest => dest.LecturerName, opt => opt.MapFrom(src => src.LecturerNavigation != null ? src.LecturerNavigation.UserName : null))
                .ForMember(dest => dest.ExaminerName, opt => opt.MapFrom(src => src.ExaminerNavigation != null ? src.ExaminerNavigation.UserName : null))
                .ForMember(dest => dest.StudentCount, opt => opt.MapFrom(src => src.Students.Count))
                .ForMember(dest => dest.Students, opt => opt.MapFrom(src => src.Students));
        }
    }
}
