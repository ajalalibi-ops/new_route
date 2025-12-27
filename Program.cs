using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;

namespace StudentManager
{
    // Student Model
    public class Student
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public int Age { get; set; }
        public string StudentCode { get; set; } = "";

        public string FullName
        {
            get { return FirstName + " " + LastName; }
        }
    }

    // Database Repository
    public class StudentRepository : IDisposable
    {
        private SqliteConnection _connection;

        public StudentRepository()
        {
            _connection = new SqliteConnection("Data Source=students.db");
            _connection.Open();
            CreateTable();
        }

        private void CreateTable()
        {
            var command = _connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Students (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    FirstName TEXT NOT NULL,
                    LastName TEXT NOT NULL,
                    Age INTEGER NOT NULL,
                    StudentCode TEXT NOT NULL
                )";
            command.ExecuteNonQuery();
        }

        public void AddStudent(Student student)
        {
            var command = _connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Students (FirstName, LastName, Age, StudentCode)
                VALUES ($firstName, $lastName, $age, $studentCode)";

            command.Parameters.AddWithValue("$firstName", student.FirstName);
            command.Parameters.AddWithValue("$lastName", student.LastName);
            command.Parameters.AddWithValue("$age", student.Age);
            command.Parameters.AddWithValue("$studentCode", student.StudentCode);

            command.ExecuteNonQuery();
        }

        public List<Student> GetAllStudents()
        {
            var students = new List<Student>();

            var command = _connection.CreateCommand();
            command.CommandText = "SELECT * FROM Students ORDER BY Id";

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var student = new Student
                    {
                        Id = reader.GetInt32(0),
                        FirstName = reader.GetString(1),
                        LastName = reader.GetString(2),
                        Age = reader.GetInt32(3),
                        StudentCode = reader.GetString(4)
                    };
                    students.Add(student);
                }
            }

            return students;
        }

        public bool DeleteStudent(int id)
        {
            var command = _connection.CreateCommand();
            command.CommandText = "DELETE FROM Students WHERE Id = $id";
            command.Parameters.AddWithValue("$id", id);

            int rowsAffected = command.ExecuteNonQuery();
            return rowsAffected > 0;
        }

        public int GetStudentCount()
        {
            var command = _connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM Students";
            var result = command.ExecuteScalar();
            return Convert.ToInt32(result);
        }

        public void Dispose()
        {
            if (_connection != null)
            {
                _connection.Close();
                _connection.Dispose();
            }
        }
    }

    // Main Program
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== STUDENT MANAGEMENT SYSTEM ===");

            using (var repository = new StudentRepository())
            {
                bool exit = false;

                while (!exit)
                {
                    Console.WriteLine("\nMAIN MENU:");
                    Console.WriteLine("1. Add New Student");
                    Console.WriteLine("2. View All Students");
                    Console.WriteLine("3. Delete Student");
                    Console.WriteLine("4. Show Student Count");
                    Console.WriteLine("5. Exit");
                    Console.Write("\nEnter your choice (1-5): ");

                    string choice = Console.ReadLine();

                    switch (choice)
                    {
                        case "1":
                            AddStudent(repository);
                            break;
                        case "2":
                            ViewStudents(repository);
                            break;
                        case "3":
                            DeleteStudent(repository);
                            break;
                        case "4":
                            ShowCount(repository);
                            break;
                        case "5":
                            exit = true;
                            Console.WriteLine("\nGoodbye!");
                            break;
                        default:
                            Console.WriteLine("\nInvalid choice! Please try again.");
                            break;
                    }

                    if (!exit && choice != "4")
                    {
                        Console.WriteLine("\nPress Enter to continue...");
                        Console.ReadLine();
                    }
                }
            }
        }

        static void AddStudent(StudentRepository repository)
        {
            Console.Clear();
            Console.WriteLine("=== ADD NEW STUDENT ===\n");

            try
            {
                var student = new Student();

                Console.Write("Enter First Name: ");
                student.FirstName = Console.ReadLine() ?? "";

                Console.Write("Enter Last Name: ");
                student.LastName = Console.ReadLine() ?? "";

                Console.Write("Enter Age: ");
                if (!int.TryParse(Console.ReadLine(), out int age) || age < 15 || age > 60)
                {
                    Console.WriteLine("\nError: Age must be between 15 and 60.");
                    return;
                }
                student.Age = age;

                Console.Write("Enter Student Code: ");
                student.StudentCode = Console.ReadLine() ?? "";

                // Validate input
                if (string.IsNullOrWhiteSpace(student.FirstName) ||
                    string.IsNullOrWhiteSpace(student.LastName) ||
                    string.IsNullOrWhiteSpace(student.StudentCode))
                {
                    Console.WriteLine("\nError: All fields are required!");
                    return;
                }

                repository.AddStudent(student);
                Console.WriteLine($"\nSuccess: Student '{student.FullName}' has been added.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError: {ex.Message}");
            }
        }

        static void ViewStudents(StudentRepository repository)
        {
            Console.Clear();
            Console.WriteLine("=== ALL STUDENTS ===\n");

            var students = repository.GetAllStudents();

            if (students.Count == 0)
            {
                Console.WriteLine("No students found in the database.");
                return;
            }

            Console.WriteLine("ID\tName\t\tAge\tStudent Code");
            Console.WriteLine("----------------------------------------");

            foreach (var student in students)
            {
                Console.WriteLine($"{student.Id}\t{student.FirstName} {student.LastName}\t{student.Age}\t{student.StudentCode}");
            }

            Console.WriteLine($"\nTotal: {students.Count} student(s)");
        }

        static void DeleteStudent(StudentRepository repository)
        {
            Console.Clear();
            Console.WriteLine("=== DELETE STUDENT ===\n");

            // First show all students
            var students = repository.GetAllStudents();

            if (students.Count == 0)
            {
                Console.WriteLine("No students to delete.");
                return;
            }

            Console.WriteLine("Current Students:\n");
            foreach (var student in students)
            {
                Console.WriteLine($"{student.Id}. {student.FullName} - {student.StudentCode}");
            }

            Console.Write("\nEnter the ID of the student to delete: ");

            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("\nError: Invalid ID format.");
                return;
            }

            // Find student to show name before deleting
            var studentToDelete = students.Find(s => s.Id == id);

            if (studentToDelete == null)
            {
                Console.WriteLine("\nError: Student with this ID does not exist.");
                return;
            }

            Console.Write($"\nAre you sure you want to delete '{studentToDelete.FullName}'? (yes/no): ");
            string confirm = Console.ReadLine()?.ToLower() ?? "";

            if (confirm == "yes" || confirm == "y")
            {
                bool success = repository.DeleteStudent(id);

                if (success)
                {
                    Console.WriteLine($"\nSuccess: Student '{studentToDelete.FullName}' has been deleted.");
                }
                else
                {
                    Console.WriteLine("\nError: Failed to delete student.");
                }
            }
            else
            {
                Console.WriteLine("\nDelete operation cancelled.");
            }
        }

        static void ShowCount(StudentRepository repository)
        {
            Console.Clear();
            Console.WriteLine("=== STUDENT COUNT ===\n");

            int count = repository.GetStudentCount();
            Console.WriteLine($"Total number of students: {count}");
        }
    }
}