using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Application.Models;

public partial class AutoGraderDBContext : DbContext
{
    public AutoGraderDBContext()
    {
    }

    public AutoGraderDBContext(DbContextOptions<AutoGraderDBContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Class> Classes { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Rule> Rules { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    public virtual DbSet<Submission> Submissions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Violation> Violations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Class>(entity =>
        {
            entity.HasKey(e => e.ClassId).HasName("PK__Classes__CB1927C07F947B2A");

            entity.Property(e => e.ClassName).HasMaxLength(100);
            entity.Property(e => e.Semester).HasMaxLength(50);

            entity.HasOne(d => d.ExaminerNavigation).WithMany(p => p.ClassExaminerNavigations)
                .HasForeignKey(d => d.Examiner)
                .HasConstraintName("FK_Classes_Examiner");

            entity.HasOne(d => d.LecturerNavigation).WithMany(p => p.ClassLecturerNavigations)
                .HasForeignKey(d => d.Lecturer)
                .HasConstraintName("FK_Classes_Lecturer");

            entity.HasMany(d => d.Students).WithMany(p => p.Classes)
                .UsingEntity<Dictionary<string, object>>(
                    "ClassStudent",
                    r => r.HasOne<Student>().WithMany()
                        .HasForeignKey("StudentId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_ClassStudents_Students"),
                    l => l.HasOne<Class>().WithMany()
                        .HasForeignKey("ClassId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_ClassStudents_Classes"),
                    j =>
                    {
                        j.HasKey("ClassId", "StudentId");
                        j.ToTable("ClassStudents");
                    });
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Roles__8AFACE1AE056E630");

            entity.Property(e => e.RoleName).HasMaxLength(100);
        });

        modelBuilder.Entity<Rule>(entity =>
        {
            entity.HasKey(e => e.RuleId).HasName("PK__Rules__110458E289F55560");

            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Pattern).HasMaxLength(255);
            entity.Property(e => e.Severity).HasMaxLength(50);
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasIndex(e => e.StudentCode, "IX_Students_StudentCode").IsUnique();

            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FullName).HasMaxLength(255);
            entity.Property(e => e.StudentCode).HasMaxLength(20);
        });

        modelBuilder.Entity<Submission>(entity =>
        {
            entity.HasKey(e => e.SubmissionId).HasName("PK__Submissi__449EE1252D440761");

            entity.Property(e => e.CheckedAt).HasColumnType("datetime");
            entity.Property(e => e.UploadedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ZipFileName).HasMaxLength(255);

            entity.HasOne(d => d.Student).WithMany(p => p.Submissions)
                .HasForeignKey(d => d.StudentId)
                .HasConstraintName("FK_Submissions_Students");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4C474DD640");

            entity.Property(e => e.Password).HasMaxLength(255);
            entity.Property(e => e.UserName).HasMaxLength(100);

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Users_Roles");
        });

        modelBuilder.Entity<Violation>(entity =>
        {
            entity.HasKey(e => e.ViolationId).HasName("PK__Violatio__18B6DC086A583DED");

            entity.Property(e => e.FilePath).HasMaxLength(500);
            entity.Property(e => e.Message).HasMaxLength(1000);

            entity.HasOne(d => d.Rule).WithMany(p => p.Violations)
                .HasForeignKey(d => d.RuleId)
                .HasConstraintName("FK__Violation__RuleI__4CA06362");

            entity.HasOne(d => d.Submission).WithMany(p => p.Violations)
                .HasForeignKey(d => d.SubmissionId)
                .HasConstraintName("FK__Violation__Submi__4BAC3F29");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
