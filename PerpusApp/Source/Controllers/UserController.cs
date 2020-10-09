using System;
using System.Data;
using System.Linq;
using System.Net;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using PerpusApp.Source.CRUD;
using PerpusApp.Source.Models;
using PerpusApp.Source.General;

namespace PerpusApp.Source.Controller
{
    [Route("PerpusApp/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        [HttpPost("Authenticate")]
        public ActionResult Authentication([FromBody] Users u)
        {
            ReturnMessage response = new ReturnMessage();
            try
            {
                if (string.IsNullOrEmpty(u.u_username) || string.IsNullOrEmpty(u.u_password)) throw new Exception("", new Exception("Incomplete Data"));

                u = UserCRUD.Authenticate(Startup.db_perpus_ConnStr, u.u_username, u.u_password);
                if (u == null) throw new Exception("", new Exception("Wrong Password!"));

                dynamic userProfile = null;
                string sid = null;
                string stfid = null;
                string mid = null;
                int? homeid = null;

                if (u.u_uc_id == UserEnum.Site_user.GetHashCode())
                {
                    userProfile = SiteCRUD.Read(Startup.db_perpus_ConnStr, (int)u.u_id);
                    sid = userProfile.s_id.ToString();
                }
                if (u.u_uc_id == UserEnum.Staff_user.GetHashCode())
                {
                    userProfile = StaffCRUD.Read(Startup.db_perpus_ConnStr, (int)u.u_id);
                    stfid = userProfile.stf_id.ToString();
                }
                if (u.u_uc_id == UserEnum.Member_user.GetHashCode())
                {
                    userProfile = MemberCRUD.Read(Startup.db_perpus_ConnStr, (int)u.u_id);
                    mid = userProfile.m_id.ToString();
                }

                if (!string.IsNullOrEmpty(sid)) homeid = int.Parse(sid);
                if (!string.IsNullOrEmpty(stfid)) homeid = int.Parse(stfid);
                if (!string.IsNullOrEmpty(mid)) homeid = int.Parse(mid);

                response.Code = 1;
                response.Message = "User is Authenticated";

                return Ok(
                    new
                    {
                        Code = response.Code,
                        Message = response.Message,
                        User = new
                        {
                            u_id = u.u_id,
                            u_username = u.u_username,
                            u_uc_id = u.u_uc_id,
                            up_photo = userProfile.up_photo,
                            home_id = homeid,
                            profileUser = userProfile
                        }
                    }
                );
            }
            catch (Exception ex)
            {
                response.Error(ex);
            }
            return Ok(response);
        }

        [HttpPut("ChangePassword")]
        public ActionResult<ReturnMessage> ChangeUserPassword([FromBody] Users u_obj)
        {
            ReturnMessage response = new ReturnMessage();
            Random rand = new Random();
            try
            {
                if (u_obj.u_id.Equals(null) || string.IsNullOrEmpty(u_obj.u_password) || string.IsNullOrEmpty(u_obj.u_new_password) || string.IsNullOrEmpty(u_obj.u_new_confirm_password)) throw new Exception("", new Exception("Incomplete Data!"));

                if (u_obj.u_new_password != u_obj.u_new_confirm_password) throw new Exception("", new Exception("New Password and confirm password are not matched!"));

                if (u_obj.u_password == u_obj.u_new_password) throw new Exception("", new Exception("New Password cannot be same with Old Password!, Try " + u_obj.u_new_password + rand.Next(1000, 9999).ToString()));

                Users u = UserCRUD.Authenticate(Startup.db_perpus_ConnStr, u_obj.u_id, u_obj.u_password);
                if (u == null) throw new Exception("", new Exception("Invalid Password!"));
                int uid = (int)u.u_id;
                int ucid = (int)u.u_uc_id;

                u = new Users();
                u.u_password = Crypto.Hash(u_obj.u_new_password, ucid);
                u.u_rec_updatedby = (User.Identity.Name != null) ? User.Identity.Name : "system";
                u.u_rec_updated = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                if (UserCRUD.Update(Startup.db_perpus_ConnStr, uid, u) == 0) throw new Exception();

                response.Code = 1;
                response.Message = "Password has changed successfully!";
            }
            catch (MySqlException sqlEx)
            {
                response.Code = -1;
                // response.Message = HttpStatusCode.InternalServerError.ToString();
                response.Message = "Error Code (" + sqlEx.Code.ToString() + "): " + sqlEx.Message;
            }
            catch (Exception ex)
            {
                response.Error(ex);
            }

            return Ok(new
            {
                Code = response.Code.ToString(),
                Message = response.Message
            });
        }

        [HttpPut("ResetPassword")]
        public ActionResult<ReturnMessage> ResetUserPassword([FromBody] Users u_obj)
        {
            ReturnMessage response = new ReturnMessage();

            try
            {
                if (string.IsNullOrEmpty(u_obj.u_username) || string.IsNullOrEmpty(u_obj.u_new_password) || string.IsNullOrEmpty(u_obj.u_new_confirm_password)) throw new Exception("", new Exception("Incomplete data"));

                if (u_obj.u_new_password != u_obj.u_new_confirm_password) throw new Exception("", new Exception("New password are not matched!"));

                dynamic userProfile = null;
                string userEmail = null;
                string userContact = null;

                // Getting Account data
                Users u = UserCRUD.Read(Startup.db_perpus_ConnStr, u_obj.u_username);
                if (u == null) throw new Exception("", new Exception("User doesn't exist!"));
                int uid = (int)u.u_id;
                int ucid = (int)u.u_uc_id;
                // Getting UserProfile detail
                if (ucid == UserEnum.Site_user.GetHashCode())
                {
                    userProfile = SiteCRUD.Read(Startup.db_perpus_ConnStr, uid);
                    if (userProfile == null) throw new Exception();
                    userEmail = userProfile.s_email;
                    userContact = userProfile.s_contact;
                }
                if (ucid == UserEnum.Staff_user.GetHashCode())
                {
                    userProfile = StaffCRUD.Read(Startup.db_perpus_ConnStr, uid);
                    if (userProfile == null) throw new Exception();
                    userEmail = userProfile.stf_email;
                    userContact = userProfile.stf_contact;
                }
                if (ucid == UserEnum.Member_user.GetHashCode())
                {
                    userProfile = MemberCRUD.Read(Startup.db_perpus_ConnStr, uid);
                    if (userProfile == null) throw new Exception();
                    userEmail = userProfile.m_email;
                    userContact = userProfile.m_contact;
                }

                // Verification start >>
                int verifScore = 0;
                if (u_obj.email != null && userEmail != null)
                    if (u_obj.email == userEmail) verifScore += 1;

                if (u_obj.contact != null && userContact != null)
                    if (u_obj.contact == userContact) verifScore += 1;
                
                if (verifScore != 2) throw new Exception("", new Exception("Account verified failed, please enter the right email and contact number!"));
                // Verification end <<

                // Update User password
                u = new Users();
                u.u_password = Crypto.Hash(u_obj.u_new_password, ucid);
                u.u_rec_updatedby = (User.Identity.Name != null) ? User.Identity.Name : "system";
                u.u_rec_updated = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                if (UserCRUD.Update(Startup.db_perpus_ConnStr, uid, u) == 0) throw new Exception();

                response.Code = 1;
                response.Message = "Successfully reset " + u_obj.u_username + " Password to " + u_obj.u_new_password;
            }
            catch (MySqlException sqlEx)
            {
                response.Code = -1;
                // response.Message = HttpStatusCode.InternalServerError.ToString();
                response.Message = "Error Code (" + sqlEx.Code.ToString() + "): " + sqlEx.Message;
            }
            catch (Exception ex)
            {
                response.Error(ex);
            }

            return Ok(response);
        }
    }
}