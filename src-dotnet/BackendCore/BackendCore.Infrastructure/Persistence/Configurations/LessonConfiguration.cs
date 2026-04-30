using BackendCore.BackendCore.Domain.Models.AggregateLesson;
using BackendCore.BackendCore.Domain.Models.AggregateSchoolClass;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BackendCore.BackendCore.Infrastructure.Persistence.Configurations;

public class LessonConfiguration : IEntityTypeConfiguration<Lesson>
{
    public void Configure(EntityTypeBuilder<Lesson> builder)
    {
        builder.ToTable("lessons");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.ScheduleSlotId).IsRequired();
        builder.Property(x => x.Date).IsRequired();
        builder.Property(x => x.Topic).IsRequired().HasMaxLength(300);

        builder
            .HasOne<ScheduleSlot>()
            .WithMany()
            .HasForeignKey(x => x.ScheduleSlotId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.ScheduleSlotId, x.Date }).IsUnique();
    }
}
