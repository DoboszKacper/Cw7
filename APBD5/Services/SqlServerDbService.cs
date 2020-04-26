using APBD5.Helpers;
using APBD5.Models;
using APBD5.DTOs.RequestModels;
using System;
using System.Data;
using System.Collections.Generic;
using System.Data.SqlClient;
using APBD5.DTOs.Response;
using APBD5.DTOs.Request;
using System.Security.Claims;
using APBD5.Password;

namespace APBD5.Services
{
    public class SqlServerDbService : IStudentsDbService
    {
        private const string ConnStr = "Data Source=db-mssql;Initial Catalog=kacpe;Integrated Security=True";

        public IEnumerable<Student> GetStudents()
        {
            var list = new List<Student>();
            using (var con = new SqlConnection(ConnStr))
            {
                using var cmd = new SqlCommand
                {
                    Connection = con,
                    CommandText = @"SELECT s.IndexNumber, s.FirstName, s.LastName, s.BirthDate, s.IdEnrollment 
                                    FROM Student s;"
                };

                con.Open();
                using var dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    var student = new Student
                    {
                        IndexNumber = dr["IndexNumber"].ToString(),
                        FirstName = dr["FirstName"].ToString(),
                        LastName = dr["LastName"].ToString(),
                        BirthDate = DateTime.Parse(dr["BirthDate"].ToString())
                        
                    };
                    list.Add(student);
                }
            }
            return list;
        }
        public Student GetStudent(string indexNumber)
        {
            using var con = new SqlConnection(ConnStr);
            using var cmd = new SqlCommand
            {
                Connection = con,
                CommandText = @"SELECT s.IndexNumber,
                                       s.FirstName,
                                       s.LastName,
                                       s.BirthDate,
                                       s.IdEnrollment 
                                FROM Student s
                                WHERE s.IndexNumber = @indexNumber;"
            };

            cmd.Parameters.AddWithValue("indexNumber", indexNumber);

            con.Open();
            using var dr = cmd.ExecuteReader();
            if (dr.Read())
            {
                return new Student
                {
                    IndexNumber = dr["IndexNumber"].ToString(),
                    FirstName = dr["FirstName"].ToString(),
                    LastName = dr["LastName"].ToString(),
                    BirthDate = DateTime.Parse(dr["BirthDate"].ToString())
                
                };
            }
            else
                return null;
        }
        public void CreateStudent(string indexNumber, string firstName, string lastName, DateTime birthDate, int idEnrollment, SqlConnection sqlConnection = null, SqlTransaction transaction = null)
        {
            using var cmd = new SqlCommand
            {
                CommandText = @"INSERT INTO Student(IndexNumber, FirstName, LastName, BirthDate, IdEnrollment)
                                VALUES (@IndexNumber, @FirstName, @LastName, @BirthDate, @IdEnrollment);"
            };
            cmd.Parameters.AddWithValue("IndexNumber", indexNumber);
            cmd.Parameters.AddWithValue("FirstName", firstName);
            cmd.Parameters.AddWithValue("LastName", lastName);
            cmd.Parameters.AddWithValue("BirthDate", birthDate);
            cmd.Parameters.AddWithValue("IdEnrollment", idEnrollment);

            if (sqlConnection == null)
            {
                using var con = new SqlConnection(ConnStr);
                con.Open();
                cmd.Connection = con;
                cmd.ExecuteNonQuery();
            }
            else
            {
                cmd.Connection = sqlConnection;
                cmd.Transaction = transaction;
                cmd.ExecuteNonQuery();
            }
        }
        public bool CheckIfEnrollmentExists(string studies, int semester)
        {
            using var con = new SqlConnection(ConnStr);
            using var cmd = new SqlCommand
            {
                Connection = con,
                CommandText = @"SELECT e.IdEnrollment
                                FROM Enrollment e JOIN Studies s ON e.IdStudy = s.IdStudy
                                WHERE s.Name = @Name AND e.Semester = @Semester;"
            };
            cmd.Parameters.AddWithValue("Name", studies);
            cmd.Parameters.AddWithValue("Semester", semester);

            con.Open();
            using var dr = cmd.ExecuteReader();
            return dr.Read();
        }

        public PromoteStudentResponse PromoteStudents(int semester, string studies)
        {
            using (var con = new SqlConnection(ConnStr))
            using (var com = new SqlCommand())
            {
                com.Connection = con;
                con.Open();

                var tran = con.BeginTransaction();
                com.Transaction = tran;

                try
                {
                    com.CommandText = "SELECT * FROM Enrollment join Studies on Studies.IdStudy = Enrollment.IdStudy where Name=@Name AND semester=@SemesterPar ";
                    com.Parameters.AddWithValue("Name", studies);
                    com.Parameters.AddWithValue("SemesterPar", semester);

                    using (var dr = com.ExecuteReader())
                    {
                        if (!dr.Read())
                            throw new ArgumentException("Nie znaleziono wpisu o podanej wartości");
                    }

                    com.CommandText = "EXEC promoteStudents @Studies=@Name, @semester=@SemesterPar";
                    com.ExecuteNonQuery();
                    tran.Commit();

                    return new PromoteStudentResponse
                    {
                        Semester = semester + 1,
                        Studies = studies

                    };
                }
                catch (SqlException ex)
                {
                    tran.Rollback();
                    throw new ArgumentException(ex.Message);
                }

            }
        }

        public bool CheckPassword(LoginRequest request)
        {
            using (var con = new SqlConnection(ConnStr))
            using (var com = new SqlCommand())
            {
                com.Connection = con;
                con.Open();

                com.CommandText = "SELECT Password,Salt FROM Student WHERE IndexNumber=@number";
                com.Parameters.AddWithValue("number", request.Login);

                using var dr = com.ExecuteReader();

                if (dr.Read())
                {
                    return SecurePassword.Validate(request.Password, dr["Salt"].ToString(), dr["Password"].ToString());
                }
                return false; 


            }
        }

        public Claim[] GetClaims(string IndexNumber)
        {
            using (var con = new SqlConnection(ConnStr))
            using (var com = new SqlCommand())
            {
                com.Connection = con;
                con.Open();

                com.CommandText = "select Student.IndexNumber,FirstName,LastName,Role" +
                    " from Student_Roles Join Roles on Student_Roles.IdRole = Roles.IdRole join Student on Student.IndexNumber = Student_Roles.IndexNumber" +
                    " where Student.IndexNumber=@Index;";
                com.Parameters.AddWithValue("Index", IndexNumber);

                var dr = com.ExecuteReader();

                if (dr.Read())
                {
                   
                    var claimList = new List<Claim>();
                    claimList.Add(new Claim(ClaimTypes.NameIdentifier, dr["IndexNumber"].ToString()));
                    claimList.Add(new Claim(ClaimTypes.Name, dr["FirstName"].ToString() + " " + dr["LastName"].ToString()));
                    claimList.Add(new Claim(ClaimTypes.Role, dr["Role"].ToString()));

                    while (dr.Read())
                    {
                        claimList.Add(new Claim(ClaimTypes.Role, dr["Role"].ToString()));
                    }
                    return claimList.ToArray();
                }
                else return null;



            }
        }

        public void SetRefreshToken(string token, string IndexNumber)
        {
            using (var con = new SqlConnection(ConnStr))
            using (var com = new SqlCommand())
            {
                com.Connection = con;
                con.Open();

                com.CommandText = "UPDATE Student SET RefreshToken=@token, TokenExpirationDate=@expires WHERE IndexNumber=@IndexNumber";
                com.Parameters.AddWithValue("token", token);
                com.Parameters.AddWithValue("expires", DateTime.Now.AddDays(2));
                com.Parameters.AddWithValue("IndexNumber", IndexNumber);

                var dr = com.ExecuteNonQuery();


            }
        }

        public string CheckRefreshToken(string token)
        {
            using (var con = new SqlConnection(ConnStr))
            using (var com = new SqlCommand())
            {
                com.Connection = con;
                con.Open();

                com.CommandText = "SELECT IndexNumber FROM STUDENT WHERE RefreshToken=@token AND TokenExpirationDate > @expires";
                com.Parameters.AddWithValue("token", token);
                com.Parameters.AddWithValue("expires", DateTime.Now);

                using var dr = com.ExecuteReader();

                if (dr.Read())
                    return dr["IndexNumber"].ToString();
                else
                    return null;


            }
        }

        public void SetPassword(string password, string IndexNumber)
        {
            using (var con = new SqlConnection(ConnStr))
            using (var com = new SqlCommand())
            {
                com.Connection = con;
                con.Open();

                com.CommandText = "Update Student set Password=@Password, Salt=@Salt WHERE IndexNumber=@IndexNumber";
                var salt = SecurePassword.CreateSalt();
                var hashedPassword = SecurePassword.Create(password, salt);
                com.Parameters.AddWithValue("Password", hashedPassword);
                com.Parameters.AddWithValue("Salt", salt);
                com.Parameters.AddWithValue("IndexNumber", IndexNumber);

                var dr = com.ExecuteNonQuery();

            }
        }

        public StudentWithStudiesRequest EnrollStudent(StudentWithStudiesRequest request)
        {
            throw new NotImplementedException();
        }
    }
  }
