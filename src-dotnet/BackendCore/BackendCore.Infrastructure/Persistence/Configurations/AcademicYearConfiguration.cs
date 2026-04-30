using BackendCore.BackendCore.Domain.Models.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BackendCore.BackendCore.Infrastructure.Persistence.Configurations;

public class AcademicYearConfiguration : IEntityTypeConfiguration<AcademicYear>
{
    public void Configure(EntityTypeBuilder<AcademicYear> builder)
    {
        builder.ToTable(
            "academic_years",
            tableBuilder =>
            {
                tableBuilder.HasCheckConstraint(
                    "ck_academic_years_dates",
                    "\"EndDate\" > \"StartDate\""
                );
            }
        );

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.StartDate).IsRequired();
        builder.Property(x => x.EndDate).IsRequired();
        builder.Property(x => x.IsCurrent).IsRequired();

        builder.HasIndex(x => new { x.StartDate, x.EndDate }).IsUnique();
    }
}
