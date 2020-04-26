using APBD5.Exceptions;
using APBD5.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace APBD5.Controllers
{
  
    [ApiController]
    [Route("api/students")]
    public class StudentsController : ControllerBase
    {
        private readonly IStudentsDbService _dbService;

        public StudentsController(IStudentsDbService dbService)
        {
            _dbService = dbService;
        }

        [HttpGet]
  
        public IActionResult GetStudents()
        {
            return Ok(_dbService.GetStudents());
        }

        [HttpGet("secured/{id}")]
        public IActionResult GetStudent(string id)
        {
            var student = _dbService.GetStudent(id);
            if (student == null)
                return NotFound($"No students with provided index number ({id})");
            else
                return Ok(student);
        }



    }
}