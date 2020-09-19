using System;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using PerpusApp.Source.General;
using PerpusApp.Source.CRUD;
using PerpusApp.Source.Models;

namespace PerpusApp.Source.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class SiteController : ControllerBase
    {
        [HttpGet("GetList")]
        public async Task<IActionResult> ReadAll()
        {
            try
            {
                List<UserSite> siteList = await Task.Run(() => SiteCRUD.ReadAll(Startup.db_perpus_ConnStr));
                return Ok(siteList);
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        [HttpGet("GetData/{s_u_id}")]
        public ActionResult ReadData([FromRoute] int s_u_id)
        {
            ReturnMessage response = new ReturnMessage();
            try
            {
                UserSite ust = SiteCRUD.Read(Startup.db_perpus_ConnStr, s_u_id);
                if(ust.Equals(null)) throw new Exception("", new Exception("Failed get data from database!"));

                return Ok(ust);
            }
            catch (Exception ex)
            {
                response.Error(ex);

                return Ok(response);
            }
        }

        [HttpPost("Add")]
        public ActionResult CreateData([FromForm] UserSite s, [FromForm] IFormFile u_file)
        {
            ReturnMessage response = new ReturnMessage();
            try
            {
                if(string.IsNullOrEmpty(s.u_username) || string.IsNullOrEmpty(s.u_password) || string.IsNullOrEmpty(s.s_fullname) || string.IsNullOrEmpty(s.s_email) || string.IsNullOrEmpty(s.s_contact) || string.IsNullOrEmpty(s.s_address) || s.s_status.Equals(null)) throw new Exception("", new Exception("Data is not added. Incomplete data"));

                if(u_file != null) ImageProsessor.CheckExtension(u_file);

                if(UserCRUD.ReadRowData(Startup.db_perpus_ConnStr, s.u_username) == 1) throw new Exception("", new Exception("Username Already used!"));

                s.s_fullname = s.s_fullname;
                s.s_email = s.s_email;
                s.s_address = s.s_address;
                s.s_contact = s.s_contact;
                s.s_status = s.s_status;
                s.s_rec_status = 1;
                s.s_rec_createdby = "Root";
                s.s_rec_created = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                Users u = new Users();
                u.u_uc_id = UserEnum.Site_user.GetHashCode();
                u.u_username = s.u_username;
                u.u_password = Crypto.Hash(s.u_password, u.u_uc_id);
                u.u_rec_status = 1;
                u.u_rec_createdby = "Root";
                u.u_rec_created = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                UserPhoto up = null;
                if (u_file != null)
                {
                    up = ImageProsessor.ConvertToThumb(u_file, "Site", s.u_username);
                    up.up_rec_status = 1;
                }

                if(!SiteCRUD.CreateSiteAndUser(Startup.db_perpus_ConnStr, s, u, up)) throw new Exception("", new Exception("Data is not added in Database!"));

                response.Code = 1;
                response.Message = "Site Data Successfully added!";

                return Ok(response);
            }
            catch (MySqlException ex)
            {
                if(ex.Number == 1062)
                {
                    response.Code = 0;
                    response.Message = "Failed add data, duplicate data!";
                }
                else
                {
                    response.Code = -1;
                    response.Message = HttpStatusCode.InternalServerError.ToString();
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Error(ex);

                return Ok(response);
            }
        }

        [HttpPut("Edit")]
        public ActionResult UpdateData([FromForm] UserSite s, [FromForm] IFormFile u_file)
        {
            ReturnMessage response = new ReturnMessage();
            try
            {
                if(s.s_u_id.Equals(null) || string.IsNullOrEmpty(s.u_username) || string.IsNullOrEmpty(s.s_fullname) || string.IsNullOrEmpty(s.s_email) || string.IsNullOrEmpty(s.s_contact) || string.IsNullOrEmpty(s.s_address) || s.s_status.Equals(null)) throw new Exception("", new Exception("Data is not added. Incomplete data"));

                if(u_file != null) ImageProsessor.CheckExtension(u_file);

                s.s_u_id = s.s_u_id;
                s.s_fullname = s.s_fullname;
                s.s_email = s.s_email;
                s.s_contact = s.s_contact;
                s.s_address = s.s_address;
                s.s_rec_updatedby = "Root";
                s.s_rec_updated = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                Users u = new Users();
                u.u_id = s.s_u_id;
                u.u_username = s.u_username;
                u.u_password = (string.IsNullOrEmpty(s.u_password)) ? null : Crypto.Hash(s.u_password, UserEnum.Site_user.GetHashCode());
                u.u_rec_updatedby = "Root";
                u.u_rec_updated = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                UserPhoto up = null;
                if(u_file != null)
                {
                    up = ImageProsessor.ConvertToThumb(u_file, "Site", s.u_username);
                    if(UserCRUD.ReadPhoto(Startup.db_perpus_ConnStr, (int)u.u_id) != 1)
                    {
                        up.up_rec_status = 1;
                        if(!UserCRUD.CreatePhoto(Startup.db_perpus_ConnStr, up, (int)u.u_id)) throw new Exception("", new Exception("Failed add photo to database!"));
                        up = null;
                    }
                }

                if(!SiteCRUD.UpdateSiteAndUser(Startup.db_perpus_ConnStr, s, u, up)) throw new Exception("", new Exception("Data is not updated in database!"));

                response.Code = 1;
                response.Message = "Site Data has updated!";

                return Ok(response);
            }
            catch (MySqlException ex)
            {
                if(ex.Number == 1062)
                {
                    response.Code = 0;
                    response.Message = "Failed update data, duplicate data!";
                }
                else
                {
                    response.Code = -1;
                    response.Message = HttpStatusCode.InternalServerError.ToString();
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Error(ex);

                return Ok(response);
            }
        }

        [HttpDelete("Delete")]
        public ActionResult DeleteData([FromBody] UserSite s)
        {
            ReturnMessage response = new ReturnMessage();
            try
            {
                if(!SiteCRUD.DeleteSiteAndUser(Startup.db_perpus_ConnStr, (int)s.s_u_id)) throw new Exception("", new Exception("Data is not deleted in database!"));

                response.Code = 1;
                response.Message = "Site data successfully deleted!";

                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Error(ex);

                return Ok(response);
            }
        }
    }
}