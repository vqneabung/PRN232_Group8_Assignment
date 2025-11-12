using Application.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public static class SeedData
    {
        public static async Task SeedStudentsAsync(AutoGraderDBContext context)
        {
            if (!await context.Students.AnyAsync())
            {
                var students = new List<Student>
                {
                    new Student
                    {
                        StudentCode = "SE183208",
                        FullName = "Nguyen Van A",
                        Email = "anvn@example.com"
                    },
                    new Student
                    {
                        StudentCode = "SE183209",
                        FullName = "Le Thi B",
                        Email = "blt@example.com"
                    },
                    new Student
                    {
                        StudentCode = "SE183210",
                        FullName = "Tran Van C",
                        Email = "ctv@example.com"
                    }
                };

                context.Students.AddRange(students);
                await context.SaveChangesAsync();
            }

            // Seed default project convention if not exists
            if (!await context.ProjectConventions.AnyAsync())
            {
                var convention = new ProjectConvention
                {
                    ExpectedSolutionPrefix = "PRN232_SU25_",
                    ExpectedSolutionSuffix = ".api.sln",
                    AdditionalRules = "Solution name must contain student code from database",
                    CreatedAt = DateTime.Now
                };

                context.ProjectConventions.Add(convention);
                await context.SaveChangesAsync();
            }
        }
    }
}