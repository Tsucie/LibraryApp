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
    public class StaffController : ControllerBase
    {
        [HttpGet("GetList")]
        public async Task<IActionResult> ReadAll()
        {
            ReturnMessage ress = new ReturnMessage();
            try
            {
                List<UserStaff> staffList = await Task.Run(() => StaffCRUD.ReadAll(Startup.db_perpus_ConnStr));
                return Ok(staffList);
            }
            catch (Exception ex)
            {
                // return BadRequest();
                ress.Error(ex);
                return Ok(ress);
            }
        }

        [HttpGet("GetData/{stf_u_id}")]
        public ActionResult ReadData([FromRoute] int stf_u_id)
        {
            ReturnMessage response = new ReturnMessage();
            try
            {
                UserStaff stf = StaffCRUD.Read(Startup.db_perpus_ConnStr, stf_u_id);
                if(stf.Equals(null)) throw new Exception("", new Exception("Failed get data from database!"));

                return Ok(stf);
            }
            catch (Exception ex)
            {
                response.Error(ex);

                return Ok(response);
            }
        }

        [HttpPost("Add")]
        public ActionResult CreateData([FromForm] UserStaff s, [FromForm] IFormFile u_file, [FromForm] int s_u_id)
        {
            ReturnMessage response = new ReturnMessage();
            try
            {
                if(string.IsNullOrEmpty(s.u_username) || string.IsNullOrEmpty(s.u_password) || s.stf_sc_id.Equals(null) || string.IsNullOrEmpty(s.stf_fullname) || string.IsNullOrEmpty(s.stf_email) || string.IsNullOrEmpty(s.stf_contact) || string.IsNullOrEmpty(s.stf_address) || string.IsNullOrEmpty(s.stf_shift) || s.stf_status.Equals(null)) throw new Exception("", new Exception("Data is not added. Incomplete data"));

                if(u_file != null) ImageProsessor.CheckExtension(u_file);

                if(UserCRUD.ReadRowData(Startup.db_perpus_ConnStr, s.u_username) == 1) throw new Exception("", new Exception("Username Already used!"));

                UserSite site = SiteCRUD.Read(Startup.db_perpus_ConnStr, s_u_id);
                if(site.Equals(null)) throw new Exception("", new Exception("Invalid User, Access Denied!"));

                s.stf_sc_id = s.stf_sc_id;
                s.stf_fullname = s.stf_fullname;
                s.stf_email = s.stf_email;
                s.stf_address = s.stf_address;
                s.stf_contact = s.stf_contact;
                s.stf_shift = s.stf_shift;
                s.stf_status = s.stf_status;
                s.stf_rec_status = 1;
                s.stf_rec_createdby = site.u_username;
                s.stf_rec_created = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                Users u = new Users();
                u.u_uc_id = UserEnum.Staff_user.GetHashCode();
                u.u_username = s.u_username;
                u.u_password = Crypto.Hash(s.u_password, u.u_uc_id);
                u.u_rec_status = 1;
                u.u_rec_createdby = site.u_username;
                u.u_rec_created = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                UserPhoto up = null;
                if (u_file != null)
                {
                    up = ImageProsessor.ConvertToThumb(u_file, "Staff", s.u_username);
                    up.up_rec_status = 1;
                }

                if(!StaffCRUD.CreateStaffAndUser(Startup.db_perpus_ConnStr, s, u, up)) throw new Exception("", new Exception("Data is not added in Database!"));

                response.Code = 1;
                response.Message = "Staff Data Successfully added!";

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
                    response.Message = ex.Message; //HttpStatusCode.InternalServerError.ToString();
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
        public ActionResult UpdateData([FromForm] UserStaff s, [FromForm] IFormFile u_file, [FromForm] int s_u_id)
        {
            ReturnMessage response = new ReturnMessage();
            try
            {
                if(s.stf_u_id.Equals(null) || s.stf_sc_id.Equals(null) || string.IsNullOrEmpty(s.u_username) || string.IsNullOrEmpty(s.stf_fullname) || string.IsNullOrEmpty(s.stf_email) || string.IsNullOrEmpty(s.stf_contact) || string.IsNullOrEmpty(s.stf_address) || string.IsNullOrEmpty(s.stf_shift) || s.stf_status.Equals(null)) throw new Exception("", new Exception("Data is not added. Incomplete data"));

                if(u_file != null) ImageProsessor.CheckExtension(u_file);

                UserSite site = SiteCRUD.Read(Startup.db_perpus_ConnStr, s_u_id);
                if(site.Equals(null)) throw new Exception("", new Exception("Invalid User, Access Denied!"));

                s.stf_u_id = s.stf_u_id;
                s.stf_sc_id = s.stf_sc_id;
                s.stf_fullname = s.stf_fullname;
                s.stf_email = s.stf_email;
                s.stf_contact = s.stf_contact;
                s.stf_address = s.stf_address;
                s.stf_rec_updatedby = site.u_username;
                s.stf_rec_updated = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                Users u = new Users();
                u.u_id = s.stf_u_id;
                u.u_username = s.u_username;
                u.u_password = (string.IsNullOrEmpty(s.u_password)) ? null : Crypto.Hash(s.u_password, UserEnum.Staff_user.GetHashCode());
                u.u_rec_updatedby = site.u_username;
                u.u_rec_updated = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                UserPhoto up = null;
                if(u_file != null)
                {
                    up = ImageProsessor.ConvertToThumb(u_file, "Staff", s.u_username);
                    if(UserCRUD.ReadPhoto(Startup.db_perpus_ConnStr, (int)u.u_id) != 1)
                    {
                        up.up_rec_status = 1;
                        if(!UserCRUD.CreatePhoto(Startup.db_perpus_ConnStr, up, (int)u.u_id)) throw new Exception("", new Exception("Failed add photo to database!"));
                        up = null;
                    }
                }

                if(!StaffCRUD.UpdateStaffAndUser(Startup.db_perpus_ConnStr, s, u, up)) throw new Exception("", new Exception("Data is not updated in database!"));

                response.Code = 1;
                response.Message = "Staff Data has updated!";

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
                    response.Message = ex.Message;//HttpStatusCode.InternalServerError.ToString();
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
        public ActionResult DeleteData([FromBody] UserStaff s)
        {
            ReturnMessage response = new ReturnMessage();
            try
            {
                if(!StaffCRUD.DeleteStaffAndUser(Startup.db_perpus_ConnStr, (int)s.stf_u_id)) throw new Exception("", new Exception("Data is not deleted in database!"));

                response.Code = 1;
                response.Message = "Staff data successfully deleted!";

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