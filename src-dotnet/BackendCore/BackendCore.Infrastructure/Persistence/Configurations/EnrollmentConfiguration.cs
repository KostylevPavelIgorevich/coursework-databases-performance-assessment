using BackendCore.BackendCore.Domain.Models.AggregateSchoolClass;
using BackendCore.BackendCore.Domain.Models.AggregateStudent;
using BackendCore.BackendCore.Domain.Models.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BackendCore.BackendCore.Infrastructure.Persistence.Configurations;

public class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
{
    public void Configure(EntityTypeBuilder<Enrollment> builder)
    {
        builder.ToTable(
            "enrollments",
            tableBuilder =>
            {
                tableBuilder.HasCheckConstraint(
                    "ck_enrollments_dates",
                    "\"EndDate\" IS NULL OR \"EndDate\" >= \"StartDate\""
                );
            }
        );

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.StudentId).IsRequired();
        builder.Property(x => x.SchoolClassId).IsRequired();
        builder.Property(x => x.AcademicYearId).IsRequired();
        builder.Property(x => x.StartDate).IsRequired();
        builder.Property(x => x.EndDate);

        builder
            .HasOne<Student>()
            .WithMany()
            .HasForeignKey(x => x.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne<SchoolClass>()
            .WithMany()
            .HasForeignKey(x => x.SchoolClassId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne<AcademicYear>()
            .WithMany()
            .HasForeignKey(x => x.AcademicYearId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new
        {
            x.StudentId,
            x.AcademicYearId,
            x.StartDate,
        });
    }
}
