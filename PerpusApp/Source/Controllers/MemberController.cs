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
    public class MemberController : ControllerBase
    {
        [HttpGet("GetList")]
        public async Task<IActionResult> ReadAll()
        {
            ReturnMessage response = new ReturnMessage();
            try
            {
                List<UserMember> members = await Task.Run(() => MemberCRUD.ReadAll(Startup.db_perpus_ConnStr));
                return Ok(members);
            }
            catch (Exception ex)
            {
                // return BadRequest();
                response.Error(ex);
                return Ok(response);
            }
        }

        [HttpGet("GetData/{m_u_id}")]
        public ActionResult ReadData([FromRoute] int m_u_id)
        {
            ReturnMessage response = new ReturnMessage();
            try
            {
                UserMember member = MemberCRUD.Read(Startup.db_perpus_ConnStr, m_u_id);
                if(member.Equals(null)) throw new Exception("", new Exception("Failed get data from database!"));

                return Ok(member);
            }
            catch (Exception ex)
            {
                response.Error(ex);

                return Ok(response);
            }
        }

        [HttpPost("Add")]
        public ActionResult CreateData([FromForm] UserMember m, [FromForm] IFormFile u_file, [FromForm] string creator)
        {
            ReturnMessage response = new ReturnMessage();
            try
            {
                if(string.IsNullOrEmpty(m.u_username) || string.IsNullOrEmpty(m.u_password) || string.IsNullOrEmpty(m.m_fullname) || string.IsNullOrEmpty(m.m_email) || string.IsNullOrEmpty(m.m_contact) || string.IsNullOrEmpty(m.m_address)  ||string.IsNullOrEmpty(m.m_class) || m.m_status.Equals(null)) throw new Exception("", new Exception("Data is not added. Incomplete data"));

                if(u_file != null) ImageProsessor.CheckExtension(u_file);

                if(UserCRUD.ReadRowData(Startup.db_perpus_ConnStr, m.u_username) == 1) throw new Exception("", new Exception("Username Already used!"));

                if(UserCRUD.ReadRowData(Startup.db_perpus_ConnStr, creator) != 1) throw new Exception("", new Exception("Invalid User, Access Denied!"));

                m.m_class = m.m_class;
                m.m_fullname = m.m_fullname;
                m.m_email = m.m_email;
                m.m_address = m.m_address;
                m.m_contact = m.m_contact;
                m.m_status = m.m_status;
                m.m_rec_status = 1;
                m.m_rec_createdby = creator;
                m.m_rec_created = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                Users u = new Users();
                u.u_uc_id = UserEnum.Member_user.GetHashCode();
                u.u_username = m.u_username;
                u.u_password = Crypto.Hash(m.u_password, u.u_uc_id);
                u.u_rec_status = 1;
                u.u_rec_createdby = creator;
                u.u_rec_created = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                UserPhoto up = null;
                if (u_file != null)
                {
                    up = ImageProsessor.ConvertToThumb(u_file, "Member", m.u_username);
                    up.up_rec_status = 1;
                }

                if(!MemberCRUD.CreateMemberAndUser(Startup.db_perpus_ConnStr, m, u, up)) throw new Exception("", new Exception("Data is not added in Database!"));

                response.Code = 1;
                response.Message = "Member Data Successfully added!";

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
        public ActionResult UpdateData([FromForm] UserMember m, [FromForm] IFormFile u_file, [FromForm] string updator)
        {
            ReturnMessage response = new ReturnMessage();
            try
            {
                if(string.IsNullOrEmpty(m.u_username) || string.IsNullOrEmpty(m.m_fullname) || string.IsNullOrEmpty(m.m_email) || string.IsNullOrEmpty(m.m_contact) || string.IsNullOrEmpty(m.m_address)  ||string.IsNullOrEmpty(m.m_class) || m.m_status.Equals(null)) throw new Exception("", new Exception("Data is not updated. Incomplete data"));

                if(u_file != null) ImageProsessor.CheckExtension(u_file);

                if(UserCRUD.ReadRowData(Startup.db_perpus_ConnStr, updator) != 1) throw new Exception("", new Exception("Invalid User, Access Denied!"));

                m.m_u_id = m.m_u_id;
                m.m_class = m.m_class;
                m.m_fullname = m.m_fullname;
                m.m_email = m.m_email;
                m.m_address = m.m_address;
                m.m_contact = m.m_contact;
                m.m_status = m.m_status;
                m.m_rec_updatedby = updator;
                m.m_rec_updated = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                Users u = new Users();
                u.u_id = m.m_u_id;
                u.u_username = m.u_username;
                
                u.u_password = (string.IsNullOrEmpty(m.u_password)) ? null : Crypto.Hash(m.u_password, UserEnum.Staff_user.GetHashCode());
                u.u_rec_updatedby = updator;
                u.u_rec_updated = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                UserPhoto up = null;
                if(u_file != null)
                {
                    up = ImageProsessor.ConvertToThumb(u_file, "Member", m.u_username);
                    if(UserCRUD.ReadPhoto(Startup.db_perpus_ConnStr, (int)u.u_id) != 1)
                    {
                        up.up_rec_status = 1;
                        if(!UserCRUD.CreatePhoto(Startup.db_perpus_ConnStr, up, (int)u.u_id)) throw new Exception("", new Exception("Failed add photo to database!"));
                        up = null;
                    }
                }

                if(!MemberCRUD.UpdateMemberAndUser(Startup.db_perpus_ConnStr, m, u, up)) throw new Exception("", new Exception("Data is not updated in database!"));

                response.Code = 1;
                response.Message = "Member Data has updated!";

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
    }
}