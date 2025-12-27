using System;
using System.Collections.Generic;

namespace StudentManager
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Student Management System ===");

            using (var repository = new StudentManager.Data.StudentRepository())
            {
                bool running = true;

                while (running)
                {
                    Console.WriteLine("\n1. Add Student");
                    Console.WriteLine("2. View Students");
                    Console.WriteLine("3. Delete Student");
                    Console.WriteLine("4. Count Students");
                    Console.WriteLine("5. Exit");
                    Console.Write("Choose option: ");

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
                            CountStudents(repository);
                            break;
                        case "5":
                            running = false;
                            break;
                        default:
                            Console.WriteLine("Invalid choice!");
                            break;
                    }
                }
            }

            Console.WriteLine("Goodbye!");
        }

        static void AddStudent(StudentManager.Data.StudentRepository repository)
        {
            Console.Write("First Name: ");
            string firstName = Console.ReadLine();

            Console.Write("Last Name: ");
            string lastName = Console.ReadLine();

            Console.Write("Age: ");
            int age = int.Parse(Console.ReadLine());

            Console.Write("Student Code: ");
            string studentCode = Console.ReadLine();

            var student = new StudentManager.Models.Student
            {
                FirstName = firstName,
                LastName = lastName,
                Age = age,
                StudentCode = studentCode
            };

            repository.AddStudent(student);
            Console.WriteLine("Student added successfully!");
        }

        static void ViewStudents(StudentManager.Data.StudentRepository repository)
        {
            List<StudentManager.Models.Student> students = repository.GetAllStudents();

            if (students.Count == 0)
            {
                Console.WriteLine("No students found.");
                return;
            }

            Console.WriteLine("\nID\tName\t\tAge\tStudent Code");
            Console.WriteLine("----------------------------------------");

            foreach (var student in students)
            {
                Console.WriteLine($"{student.Id}\t{student.FirstName} {student.LastName}\t{student.Age}\t{student.StudentCode}");
            }
        }

        static void DeleteStudent(StudentManager.Data.StudentRepository repository)
        {
            Console.Write("Enter student ID to delete: ");
            int id = int.Parse(Console.ReadLine());

            bool deleted = repository.DeleteStudent(id);

            if (deleted)
            {
                Console.WriteLine("Student deleted successfully!");
            }
            else
            {
                Console.WriteLine("Student not found!");
            }
        }

        static void CountStudents(StudentManager.Data.StudentRepository repository)
        {
            int count = repository.GetStudentCount();
            Console.WriteLine($"Total students: {count}");
        }
    }
}