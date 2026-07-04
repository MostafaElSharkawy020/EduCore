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
        public DbSet<Choice> Choices { get; set; }
        public DbSet<ExamQuestion> ExamQuestions { get; set; }
        public DbSet<QuizQuestion> QuizQuestions { get; set; }
        public DbSet<StudentCourse> StudentCourses { get; set; }
        public DbSet<QuizAttempt> QuizAttempts { get; set; }
        public DbSet<ExamAttempt> ExamAttempts { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<StudentClass> StudentClasses { get; set; }

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

            // ── Attempt tables: each has two FKs, so disable cascade on the Student side ──
            modelBuilder.Entity<QuizAttempt>()
                .HasOne(a => a.Quiz).WithMany()
                .HasForeignKey(a => a.QuizID).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<QuizAttempt>()
                .HasOne(a => a.Student).WithMany()
                .HasForeignKey(a => a.StudentID).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ExamAttempt>()
                .HasOne(a => a.Exam).WithMany()
                .HasForeignKey(a => a.ExamID).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<ExamAttempt>()
                .HasOne(a => a.Student).WithMany()
                .HasForeignKey(a => a.StudentID).OnDelete(DeleteBehavior.Restrict);

            // ── Payments: money precision; only FK is Student (item is a snapshot) ──
            modelBuilder.Entity<Payment>().Property(p => p.Amount).HasPrecision(18, 2);
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Student).WithMany()
                .HasForeignKey(p => p.StudentID).OnDelete(DeleteBehavior.Restrict);

            // ── Class enrollment (à la carte): two FKs, Student side restricted ──
            modelBuilder.Entity<StudentClass>()
                .HasOne(sc => sc.Class).WithMany(c => c.StudentClasses)
                .HasForeignKey(sc => sc.ClassID).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<StudentClass>()
                .HasOne(sc => sc.Student).WithMany()
                .HasForeignKey(sc => sc.StudentID).OnDelete(DeleteBehavior.Restrict);

            // ── Unique email per account type (length-capped so the column can be indexed) ──
            modelBuilder.Entity<Teacher>().Property(t => t.Email).HasMaxLength(256);
            modelBuilder.Entity<Teacher>().HasIndex(t => t.Email).IsUnique();
            modelBuilder.Entity<Student>().Property(s => s.Email).HasMaxLength(256);
            modelBuilder.Entity<Student>().HasIndex(s => s.Email).IsUnique();

            // ── Seed a demo teacher (ID = 1) ──
            // Needed because we currently hardcode TeacherID = 1 in CoursesController.
            // TODO: remove once real teacher authentication / registration exists.
            modelBuilder.Entity<Teacher>().HasData(new Teacher
            {
                ID = 1,
                FName = "Demo",
                LName = "Teacher",
                Email = "teacher@educore.local",
                // PBKDF2 hash of "Teacher@123" (see Helpers/PasswordHasher).
                Password = "1OubmybQyMYpetU/JF2JNg==:PA3m0/NloaZJlG72BhRBEuwQJ6MTWxSx632tuVtGZ1E=",
                PhoneNumber = "0000000000",
                Biography = "Seeded demo teacher account for development."
            });
        }
    }
}
