using BackendCore.BackendCore.Domain.Models.AggregateStudent;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BackendCore.BackendCore.Infrastructure.Persistence.Configurations;

public class StudentStatusConfiguration : IEntityTypeConfiguration<StudentStatus>
{
    public void Configure(EntityTypeBuilder<StudentStatus> builder)
    {
        builder.ToTable("student_statuses");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Description).HasMaxLength(500);

        builder.HasIndex(x => x.Name).IsUnique();
    }
}
