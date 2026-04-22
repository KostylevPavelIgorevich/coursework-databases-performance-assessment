using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BackendCore.BackendCore.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "academic_years",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    IsCurrent = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_academic_years", x => x.Id);
                    table.CheckConstraint("ck_academic_years_dates", "\"EndDate\" > \"StartDate\"");
                });

            migrationBuilder.CreateTable(
                name: "classrooms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Building = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_classrooms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "grade_types",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_grade_types", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "student_statuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_student_statuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "subjects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    ShortName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subjects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "teachers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MiddleName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_teachers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "periods",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AcademicYearId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_periods", x => x.Id);
                    table.CheckConstraint("ck_periods_dates", "\"EndDate\" > \"StartDate\"");
                    table.CheckConstraint("ck_periods_order", "\"Order\" >= 1");
                    table.ForeignKey(
                        name: "FK_periods_academic_years_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "academic_years",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "school_classes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Grade = table.Column<int>(type: "integer", nullable: false),
                    Letter = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    AcademicYearId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_school_classes", x => x.Id);
                    table.CheckConstraint("ck_school_classes_grade", "\"Grade\" >= 1 AND \"Grade\" <= 11");
                    table.ForeignKey(
                        name: "FK_school_classes_academic_years_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "academic_years",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "students",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MiddleName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    BirthDate = table.Column<DateOnly>(type: "date", nullable: false),
                    StudentStatusId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_students", x => x.Id);
                    table.ForeignKey(
                        name: "FK_students_student_statuses_StudentStatusId",
                        column: x => x.StudentStatusId,
                        principalTable: "student_statuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "teaching_assignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SchoolClassId = table.Column<int>(type: "integer", nullable: false),
                    TeacherId = table.Column<int>(type: "integer", nullable: false),
                    SubjectId = table.Column<int>(type: "integer", nullable: false),
                    AcademicYearId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_teaching_assignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_teaching_assignments_academic_years_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "academic_years",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_teaching_assignments_school_classes_SchoolClassId",
                        column: x => x.SchoolClassId,
                        principalTable: "school_classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_teaching_assignments_subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_teaching_assignments_teachers_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "teachers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "enrollments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StudentId = table.Column<int>(type: "integer", nullable: false),
                    SchoolClassId = table.Column<int>(type: "integer", nullable: false),
                    AcademicYearId = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_enrollments", x => x.Id);
                    table.CheckConstraint("ck_enrollments_dates", "\"EndDate\" IS NULL OR \"EndDate\" >= \"StartDate\"");
                    table.ForeignKey(
                        name: "FK_enrollments_academic_years_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "academic_years",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_enrollments_school_classes_SchoolClassId",
                        column: x => x.SchoolClassId,
                        principalTable: "school_classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_enrollments_students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "schedule_slots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SchoolClassId = table.Column<int>(type: "integer", nullable: false),
                    DayOfWeek = table.Column<int>(type: "integer", nullable: false),
                    LessonNumber = table.Column<int>(type: "integer", nullable: false),
                    TeachingAssignmentId = table.Column<int>(type: "integer", nullable: false),
                    ClassroomId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_schedule_slots", x => x.Id);
                    table.CheckConstraint("ck_schedule_slots_day_of_week", "\"DayOfWeek\" >= 1 AND \"DayOfWeek\" <= 5");
                    table.CheckConstraint("ck_schedule_slots_lesson_number", "\"LessonNumber\" >= 1 AND \"LessonNumber\" <= 10");
                    table.ForeignKey(
                        name: "FK_schedule_slots_classrooms_ClassroomId",
                        column: x => x.ClassroomId,
                        principalTable: "classrooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_schedule_slots_school_classes_SchoolClassId",
                        column: x => x.SchoolClassId,
                        principalTable: "school_classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_schedule_slots_teaching_assignments_TeachingAssignmentId",
                        column: x => x.TeachingAssignmentId,
                        principalTable: "teaching_assignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "lessons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ScheduleSlotId = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Topic = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lessons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_lessons_schedule_slots_ScheduleSlotId",
                        column: x => x.ScheduleSlotId,
                        principalTable: "schedule_slots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "grades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LessonId = table.Column<int>(type: "integer", nullable: false),
                    StudentId = table.Column<int>(type: "integer", nullable: false),
                    GradeTypeId = table.Column<int>(type: "integer", nullable: false),
                    Value = table.Column<int>(type: "integer", nullable: false),
                    Comment = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_grades", x => x.Id);
                    table.CheckConstraint("ck_grades_value", "\"Value\" >= 1 AND \"Value\" <= 5");
                    table.ForeignKey(
                        name: "FK_grades_grade_types_GradeTypeId",
                        column: x => x.GradeTypeId,
                        principalTable: "grade_types",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_grades_lessons_LessonId",
                        column: x => x.LessonId,
                        principalTable: "lessons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_grades_students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_academic_years_StartDate_EndDate",
                table: "academic_years",
                columns: new[] { "StartDate", "EndDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_classrooms_Number",
                table: "classrooms",
                column: "Number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_enrollments_AcademicYearId",
                table: "enrollments",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_enrollments_SchoolClassId",
                table: "enrollments",
                column: "SchoolClassId");

            migrationBuilder.CreateIndex(
                name: "IX_enrollments_StudentId_AcademicYearId_StartDate",
                table: "enrollments",
                columns: new[] { "StudentId", "AcademicYearId", "StartDate" });

            migrationBuilder.CreateIndex(
                name: "IX_grade_types_Name",
                table: "grade_types",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_grades_GradeTypeId",
                table: "grades",
                column: "GradeTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_grades_LessonId_StudentId_GradeTypeId",
                table: "grades",
                columns: new[] { "LessonId", "StudentId", "GradeTypeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_grades_StudentId",
                table: "grades",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_lessons_ScheduleSlotId_Date",
                table: "lessons",
                columns: new[] { "ScheduleSlotId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_periods_AcademicYearId_Order",
                table: "periods",
                columns: new[] { "AcademicYearId", "Order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_schedule_slots_ClassroomId",
                table: "schedule_slots",
                column: "ClassroomId");

            migrationBuilder.CreateIndex(
                name: "IX_schedule_slots_SchoolClassId_DayOfWeek_LessonNumber",
                table: "schedule_slots",
                columns: new[] { "SchoolClassId", "DayOfWeek", "LessonNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_schedule_slots_TeachingAssignmentId",
                table: "schedule_slots",
                column: "TeachingAssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_school_classes_AcademicYearId",
                table: "school_classes",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_school_classes_Grade_Letter_AcademicYearId",
                table: "school_classes",
                columns: new[] { "Grade", "Letter", "AcademicYearId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_student_statuses_Name",
                table: "student_statuses",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_students_StudentStatusId",
                table: "students",
                column: "StudentStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_subjects_Name",
                table: "subjects",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_teachers_LastName_FirstName_MiddleName",
                table: "teachers",
                columns: new[] { "LastName", "FirstName", "MiddleName" });

            migrationBuilder.CreateIndex(
                name: "IX_teaching_assignments_AcademicYearId",
                table: "teaching_assignments",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_teaching_assignments_SchoolClassId_SubjectId_AcademicYearId",
                table: "teaching_assignments",
                columns: new[] { "SchoolClassId", "SubjectId", "AcademicYearId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_teaching_assignments_SubjectId",
                table: "teaching_assignments",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_teaching_assignments_TeacherId",
                table: "teaching_assignments",
                column: "TeacherId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "enrollments");

            migrationBuilder.DropTable(
                name: "grades");

            migrationBuilder.DropTable(
                name: "periods");

            migrationBuilder.DropTable(
                name: "grade_types");

            migrationBuilder.DropTable(
                name: "lessons");

            migrationBuilder.DropTable(
                name: "students");

            migrationBuilder.DropTable(
                name: "schedule_slots");

            migrationBuilder.DropTable(
                name: "student_statuses");

            migrationBuilder.DropTable(
                name: "classrooms");

            migrationBuilder.DropTable(
                name: "teaching_assignments");

            migrationBuilder.DropTable(
                name: "school_classes");

            migrationBuilder.DropTable(
                name: "subjects");

            migrationBuilder.DropTable(
                name: "teachers");

            migrationBuilder.DropTable(
                name: "academic_years");
        }
    }
}
