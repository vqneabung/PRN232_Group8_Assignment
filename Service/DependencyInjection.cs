using Application.Helper;
using Application.UnitOfWork;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Service.IPRN232Service;
using Service.PRN232Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDependencyInjection(this IServiceCollection services, IConfiguration? configuration = null)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            var plagiarismServiceUrl = configuration?["PlagiarismService:BaseUrl"] ?? "http://localhost:5001";
            services.AddSingleton(sp => new PlagiarismCheckClient(plagiarismServiceUrl));

            services.AddScoped<ISubmissionService, SubmissionService>();
            services.AddScoped<IRuleService, RuleService>();
            services.AddScoped<IStudentService, StudentService>();
            services.AddScoped<IPlagiarismService, PlagiarismService>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IClassService, ClassService>();
            services.AddScoped<IUserService, UserService>();

            return services;
        }
    }
}