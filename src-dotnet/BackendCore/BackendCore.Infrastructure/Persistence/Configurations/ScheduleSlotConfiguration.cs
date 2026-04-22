using BackendCore.BackendCore.Domain.Models.AggregateSchoolClass;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BackendCore.BackendCore.Infrastructure.Persistence.Configurations;

public class ScheduleSlotConfiguration : IEntityTypeConfiguration<ScheduleSlot>
{
    public void Configure(EntityTypeBuilder<ScheduleSlot> builder)
    {
        builder.ToTable(
            "schedule_slots",
            tableBuilder =>
            {
                tableBuilder.HasCheckConstraint(
                    "ck_schedule_slots_lesson_number",
                    "\"LessonNumber\" >= 1 AND \"LessonNumber\" <= 10"
                );
                tableBuilder.HasCheckConstraint(
                    "ck_schedule_slots_day_of_week",
                    "\"DayOfWeek\" >= 1 AND \"DayOfWeek\" <= 5"
                );
            }
        );

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.SchoolClassId).IsRequired();
        builder.Property(x => x.DayOfWeek).IsRequired();
        builder.Property(x => x.LessonNumber).IsRequired();
        builder.Property(x => x.TeachingAssignmentId).IsRequired();
        builder.Property(x => x.ClassroomId).IsRequired();

        builder
            .HasOne<SchoolClass>()
            .WithMany()
            .HasForeignKey(x => x.SchoolClassId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne<TeachingAssignment>()
            .WithMany()
            .HasForeignKey(x => x.TeachingAssignmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne<Classroom>()
            .WithMany()
            .HasForeignKey(x => x.ClassroomId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasIndex(x => new
            {
                x.SchoolClassId,
                x.DayOfWeek,
                x.LessonNumber,
            })
            .IsUnique();
    }
}
