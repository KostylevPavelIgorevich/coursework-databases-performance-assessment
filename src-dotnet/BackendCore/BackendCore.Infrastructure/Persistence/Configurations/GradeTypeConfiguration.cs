using BackendCore.BackendCore.Domain.Models.AggregateLesson;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BackendCore.BackendCore.Infrastructure.Persistence.Configurations;

public class GradeTypeConfiguration : IEntityTypeConfiguration<GradeType>
{
    public void Configure(EntityTypeBuilder<GradeType> builder)
    {
        builder.ToTable("grade_types");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Description).HasMaxLength(500);

        builder.HasIndex(x => x.Name).IsUnique();
    }
}
