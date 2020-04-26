using APBD5.Models;
using APBD5.DTOs.RequestModels;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Security.Claims;
using APBD5.Controllers;
using APBD5.DTOs.ResponseModels;
using APBD5.DTOs.Response;
using APBD5.DTOs.Request;

namespace APBD5.Services
{
    public interface IStudentsDbService
    {
       
        StudentWithStudiesRequest EnrollStudent(StudentWithStudiesRequest request);
        PromoteStudentResponse PromoteStudents(int semester, string studies);


        public IEnumerable<Student> GetStudents();
        public Student GetStudent(string IndexNumber);
        public bool CheckPassword(LoginRequest request);

        public Claim[] GetClaims(string IndexNumber);

        public void SetRefreshToken(string token, string indexNumber);
        public string CheckRefreshToken(string token);

        public void SetPassword(string password, string IndexNumber);
    }
}
