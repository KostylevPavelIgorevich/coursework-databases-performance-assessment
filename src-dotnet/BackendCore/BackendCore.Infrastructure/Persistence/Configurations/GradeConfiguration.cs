using BackendCore.BackendCore.Domain.Models.AggregateLesson;
using BackendCore.BackendCore.Domain.Models.AggregateStudent;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BackendCore.BackendCore.Infrastructure.Persistence.Configurations;

public class GradeConfiguration : IEntityTypeConfiguration<Grade>
{
    public void Configure(EntityTypeBuilder<Grade> builder)
    {
        builder.ToTable(
            "grades",
            tableBuilder =>
            {
                tableBuilder.HasCheckConstraint(
                    "ck_grades_value",
                    "\"Value\" >= 1 AND \"Value\" <= 5"
                );
            }
        );

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.LessonId).IsRequired();
        builder.Property(x => x.StudentId).IsRequired();
        builder.Property(x => x.GradeTypeId).IsRequired();
        builder.Property(x => x.Value).IsRequired();
        builder.Property(x => x.Comment).HasMaxLength(1000);

        builder
            .HasOne<Lesson>()
            .WithMany()
            .HasForeignKey(x => x.LessonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne<Student>()
            .WithMany()
            .HasForeignKey(x => x.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne<GradeType>()
            .WithMany()
            .HasForeignKey(x => x.GradeTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasIndex(x => new
            {
                x.LessonId,
                x.StudentId,
                x.GradeTypeId,
            })
            .IsUnique();
    }
}
