
using System;

using System.IdentityModel.Tokens.Jwt;

using System.Security.Claims;
using System.Text;
using APBD5.DTOs.Request;
using APBD5.DTOs.RequestModels;
using APBD5.Helpers;
using APBD5.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace APBD5.Controllers
{
    [Route("api/enrollments")]
    [ApiController]
    public class EnrollmentsController : ControllerBase
    {
        private IStudentsDbService _service;
        private IConfiguration Configuration;

        public EnrollmentsController(IStudentsDbService service, IConfiguration configuration)
        {
            _service = service;
            Configuration = configuration;
        }

        [HttpPost]
        [Authorize(Roles = "Employee")]
        public IActionResult EnrollStudent(StudentWithStudiesRequest request)
        {
            try
            {
                return Ok(_service.EnrollStudent(request));

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("login")]
        public IActionResult Login(LoginRequest loginRequest)
        {
            if (!_service.CheckPassword(loginRequest))
                return Forbid("Bearer");

            var claims = _service.GetClaims(loginRequest.Login);
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "Gakko",
                audience: "Students",
                claims: claims,
                expires: DateTime.Now.AddMinutes(5),
                signingCredentials: creds
                );
            var refreshToken = Guid.NewGuid();
            _service.SetRefreshToken(refreshToken.ToString(), loginRequest.Login);
            return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token), refreshToken });
        }

        [HttpPost("refresh-token/{token}")]
        public IActionResult RefreshToken(string token)
        {
            var user = _service.CheckRefreshToken(token);
            if (user == null)
                return Forbid("Bearer");

            var claims = _service.GetClaims(user);
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var newToken = new JwtSecurityToken(
                issuer: "Gakko",
                audience: "Students",
                claims: claims,
                expires: DateTime.Now.AddMinutes(5),
                signingCredentials: creds
                );
            var refreshToken = Guid.NewGuid();
            _service.SetRefreshToken(refreshToken.ToString(), user);
            return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(newToken), refreshToken });

        }

    }
}