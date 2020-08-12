using System;
using System.Data;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using PerpusApp.Source.CRUD;
using PerpusApp.Source.Models;
using PerpusApp.Source.General;

namespace PerpusApp.Source.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        [HttpPost]
        public ActionResult Login([FromForm] string username, string password)
        {
            ReturnMessage response = new ReturnMessage();
            try
            {
                Users u = UserCRUD.Read(Startup.db_perpus_ConnStr, username);
                if(u.Equals(null)) throw new Exception("", new Exception("Login Gagal, Username Salah!"));

                if(u.u_password.Equals(password) || Crypto.Verify(password, u.u_password))
                {
                    response.Code = 1;
                    response.Message = "Login Berhasil";
                    
                    return Ok(u);
                }
            }
            catch (Exception ex)
            {
                response.Error(ex);
            }
            return Ok(response);
        }
    }
}