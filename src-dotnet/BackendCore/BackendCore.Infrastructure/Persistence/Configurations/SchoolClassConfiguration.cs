using BackendCore.BackendCore.Domain.Models.AggregateSchoolClass;
using BackendCore.BackendCore.Domain.Models.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BackendCore.BackendCore.Infrastructure.Persistence.Configurations;

public class SchoolClassConfiguration : IEntityTypeConfiguration<SchoolClass>
{
    public void Configure(EntityTypeBuilder<SchoolClass> builder)
    {
        builder.ToTable(
            "school_classes",
            tableBuilder =>
            {
                tableBuilder.HasCheckConstraint(
                    "ck_school_classes_grade",
                    "\"Grade\" >= 1 AND \"Grade\" <= 11"
                );
            }
        );

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.Grade).IsRequired();
        builder.Property(x => x.Letter).IsRequired().HasMaxLength(5);
        builder.Property(x => x.AcademicYearId).IsRequired();

        builder
            .HasOne<AcademicYear>()
            .WithMany()
            .HasForeignKey(x => x.AcademicYearId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasIndex(x => new
            {
                x.Grade,
                x.Letter,
                x.AcademicYearId,
            })
            .IsUnique();
    }
}
