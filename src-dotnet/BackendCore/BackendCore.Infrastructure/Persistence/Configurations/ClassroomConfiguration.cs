using BackendCore.BackendCore.Domain.Models.AggregateSchoolClass;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BackendCore.BackendCore.Infrastructure.Persistence.Configurations;

public class ClassroomConfiguration : IEntityTypeConfiguration<Classroom>
{
    public void Configure(EntityTypeBuilder<Classroom> builder)
    {
        builder.ToTable("classrooms");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.Number).IsRequired().HasMaxLength(20);
        builder.Property(x => x.Building).HasMaxLength(100);

        builder.HasIndex(x => x.Number).IsUnique();
    }
}
