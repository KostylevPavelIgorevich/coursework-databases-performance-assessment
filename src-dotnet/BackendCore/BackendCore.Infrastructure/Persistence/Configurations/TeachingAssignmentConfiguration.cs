using BackendCore.BackendCore.Domain.Models.AggregateSchoolClass;
using BackendCore.BackendCore.Domain.Models.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BackendCore.BackendCore.Infrastructure.Persistence.Configurations;

public class TeachingAssignmentConfiguration : IEntityTypeConfiguration<TeachingAssignment>
{
    public void Configure(EntityTypeBuilder<TeachingAssignment> builder)
    {
        builder.ToTable("teaching_assignments");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.SchoolClassId).IsRequired();
        builder.Property(x => x.TeacherId).IsRequired();
        builder.Property(x => x.SubjectId).IsRequired();
        builder.Property(x => x.AcademicYearId).IsRequired();

        builder
            .HasOne<SchoolClass>()
            .WithMany()
            .HasForeignKey(x => x.SchoolClassId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne<Teacher>()
            .WithMany()
            .HasForeignKey(x => x.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne<Subject>()
            .WithMany()
            .HasForeignKey(x => x.SubjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne<AcademicYear>()
            .WithMany()
            .HasForeignKey(x => x.AcademicYearId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasIndex(x => new
            {
                x.SchoolClassId,
                x.SubjectId,
                x.AcademicYearId,
            })
            .IsUnique();
    }
}
