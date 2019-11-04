using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Api
{
    public class Acount
    {
        public int IdAcount{get;set;}
        public string UsrName{get;set;}
        public string PassWord{get;set;}
        public string Email{get;set;}
        //public bool Admin{get;set;}
        public string Roles{get;set;}
    }

    [Route("[controller]")]
    public class AcountController : Controller
    {
        Db db;
        IConfiguration config;
        public AcountController(Db dbi, IConfiguration _config)
        {
            db = dbi;
            config = _config;
        }
        
        [HttpPost]
        public IActionResult CreateUser(string user, string pass, string repeatPass, string email, string name)
        {
            if(pass != repeatPass)
            {
                return BadRequest("no coinciden las contraseñas");
            }
            var newUser = new Acount();
            
            //newUser.Name = name;
            newUser.UsrName = user;
            newUser.PassWord = Hash(pass);
            //newUser.Email = email;
            db.Acount.Add(newUser);
            db.SaveChanges();
            return Ok();
        }
        [HttpPost("login")]
        public IActionResult GetSession(string username, string password)
        {
            var usr = db.Acount.FirstOrDefault(u => u.UsrName == username && u.PassWord == Hash(password));
            if(usr == null)
            {
                return BadRequest("Usuario o contraseña invalidos");
            }
            var claims = new[]
            {
                new Claim("Nombre", username),
                //new Claim("Nombre de la app", "fullhousecrm"),
                //new Claim("Admin", usr.Admin.ToString()),
                new Claim("IdAcount", usr.IdAcount.ToString()),
                new Claim("UserName", usr.UsrName)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.GetValue<string>("ApiKey")));
            var credenciales = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiration = DateTime.UtcNow.AddHours(12);
            JwtSecurityToken token = new JwtSecurityToken(
                issuer : "localhost",
                audience: "localhost",
                claims : claims,
                expires: expiration,
                signingCredentials: credenciales
            );
            return Ok(new{
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiration = expiration
            });

        }

        [HttpGet("user")]
        public object ClaimsUser()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity.Claims.Count() > 0)
            {
                return new{
                   userName = identity.Claims.ToList()[0].Value,
                   rolUsuario = identity.Claims.ToList()[1].Value == "True" ? "Admin" : "Usuario"
                } ;
            }else
            {
                return BadRequest("No estás autenticado");
            }
        }
        static string Hash(string randomString)
        {
            var crypt = new System.Security.Cryptography.SHA256Managed();
            var hash = new System.Text.StringBuilder();
            byte[] crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(randomString));
            foreach (byte theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }
            return hash.ToString();
        }        
    }
}