using BackendCore.BackendCore.Domain.Models.AggregateStudent;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BackendCore.BackendCore.Infrastructure.Persistence.Configurations;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.ToTable("students");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.LastName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.MiddleName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.BirthDate).IsRequired();
        builder.Property(x => x.StudentStatusId).IsRequired();

        builder
            .HasOne<StudentStatus>()
            .WithMany()
            .HasForeignKey(x => x.StudentStatusId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
