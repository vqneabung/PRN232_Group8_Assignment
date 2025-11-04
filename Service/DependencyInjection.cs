using Application.UnitOfWork;
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
        public static IServiceCollection AddDependencyInjection(this IServiceCollection services)
        {
            // 🧱 Đăng ký Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // 💼 Đăng ký các Services
            services.AddScoped<ISubmissionService, SubmissionService>();
            services.AddScoped<IRuleService, RuleService>();

            return services;
        }
    }
}