using BackendCore.BackendCore.Domain.Models.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BackendCore.BackendCore.Infrastructure.Persistence.Configurations;

public class TeacherConfiguration : IEntityTypeConfiguration<Teacher>
{
    public void Configure(EntityTypeBuilder<Teacher> builder)
    {
        builder.ToTable("teachers");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.LastName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.MiddleName).IsRequired().HasMaxLength(100);

        builder.HasIndex(x => new
        {
            x.LastName,
            x.FirstName,
            x.MiddleName,
        });
    }
}
