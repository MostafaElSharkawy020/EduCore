using EduCore.Models;
using Microsoft.EntityFrameworkCore;

namespace EduCore.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // One DbSet per model => one table each
        public DbSet<Course> Courses { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Assistant> Assistants { get; set; }
        public DbSet<TeacherAssistant> TeacherAssistants { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Card> Cards { get; set; }
        public DbSet<Video> Videos { get; set; }
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<Exam> Exams { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<ExamQuestion> ExamQuestions { get; set; }
        public DbSet<QuizQuestion> QuizQuestions { get; set; }
        public DbSet<StudentCourse> StudentCourses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ── Decimal precision for money columns ──
            modelBuilder.Entity<Course>()
                .Property(c => c.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Class>()
                .Property(c => c.Price)
                .HasPrecision(18, 2);

            // ── Junction tables: disable cascade on one FK each ──
            // (two cascading FKs on one table => SQL Server "multiple cascade paths" error)

            modelBuilder.Entity<StudentCourse>()
                .HasOne(sc => sc.Course)
                .WithMany(c => c.StudentCourses)
                .HasForeignKey(sc => sc.CourseID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ExamQuestion>()
                .HasOne(eq => eq.Question)
                .WithMany(q => q.ExamQuestions)
                .HasForeignKey(eq => eq.QuestionID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<QuizQuestion>()
                .HasOne(qq => qq.Question)
                .WithMany(q => q.QuizQuestions)
                .HasForeignKey(qq => qq.QuestionID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TeacherAssistant>()
                .HasOne(ta => ta.Assistant)
                .WithMany(a => a.TeacherAssistants)
                .HasForeignKey(ta => ta.AssistantID)
                .OnDelete(DeleteBehavior.Restrict);

            // ── Seed a demo teacher (ID = 1) ──
            // Needed because we currently hardcode TeacherID = 1 in CoursesController.
            // TODO: remove once real teacher authentication / registration exists.
            modelBuilder.Entity<Teacher>().HasData(new Teacher
            {
                ID = 1,
                FName = "Demo",
                LName = "Teacher",
                Email = "teacher@educore.local",
                Password = "password",
                PhoneNumber = "0000000000",
                Biography = "Seeded demo teacher account for development."
            });
        }
    }
}
