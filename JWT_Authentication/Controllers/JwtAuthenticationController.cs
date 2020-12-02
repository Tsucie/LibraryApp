using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JWT_Authentication;

namespace JWT_Authentication.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class JwtAuthenticationController : ControllerBase
    {
        private readonly IConfiguration setting;
        private readonly IDistributedCache sessionCache;

        public JwtAuthenticationController(IConfiguration config, IDistributedCache sessionCache)
        {
            setting = config;
            this.sessionCache = sessionCache;
        }

        [HttpPost("Login")] // Has 3 awaits
        public async Task<ActionResult> AuthenticateAsync([FromBody] JObject j_obj)
        {
            ResponseMessage ressmsg = new ResponseMessage();
            try
            {
                if (j_obj["u_username"] == null || j_obj["u_password"] == null)
                    throw new Exception("", new Exception(HttpStatusCode.Unauthorized.ToString() + " : Incomplete Data!"));

                string u_username = (string)j_obj["u_username"];
                string u_password = (string)j_obj["u_password"];
                if (string.IsNullOrEmpty(u_username) || string.IsNullOrEmpty(u_password))
                    throw new Exception("", new Exception(HttpStatusCode.Unauthorized.ToString() + " : Incomplete Data!"));

                HttpResponseMessage response = null;

                // User Authentication Request
                response = await UserAuthenticationRequestAsync(u_username, u_password);
                if (response == null) throw new Exception(); // IsNull validation
                if (response.StatusCode != HttpStatusCode.OK) throw new Exception(); // HttpStatusCode Is OK validation

                string result = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(result)) throw new Exception(); // IsNullOrEmpty validation
                // resultData response from HttpResponseMessage
                dynamic resultData = JsonConvert.DeserializeObject(result);
                if (resultData == null) throw new Exception(); // IsNull validation
                string code = resultData.code.ToString();
                string message = resultData.message.ToString();
                if (code == "-1") throw new Exception(message);
                if (code == "0") throw new Exception("", new Exception(message));

                if (resultData.user == null) throw new Exception();
                dynamic user = resultData.user;
                int? homeid = null;
                if (user.home_id != null) homeid = (int)user.home_id;

                // Generate JWT Token
                string encodedJwt = await Task.Run(() =>
                    GenerateJwt(user, Startup.jwt_secret_key, Startup.jwt_issuer, Startup.jwt_expire));
                if (string.IsNullOrEmpty(encodedJwt)) throw new Exception();

                // Store to Redis
                string uid = user.u_id.ToString();
                sessionCache.SetString(uid, encodedJwt, new DistributedCacheEntryOptions() {
                    SlidingExpiration = TimeSpan.FromMinutes(double.Parse(Startup.jwt_idle_expire))
                });

                return Ok(new
                {
                    Code = code,
                    Message = message,
                    Data = new
                    {
                        u_id = (int)user.u_id,
                        u_username = user.u_username.ToString(),
                        home_id = homeid,
                        Token = encodedJwt,
                        profileUser = user.profileUser
                    }
                });
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                {
                    ressmsg.Code = "-1";
                    // ressmsg.Message = HttpStatusCode.InternalServerError.ToString();
                    ressmsg.Message = ex.Message;
                }
                else
                {
                    ressmsg.Code = "0";
                    ressmsg.Message = ex.InnerException.Message;
                }

                return Ok(ressmsg);
            }
        }

        [HttpPost("Login/Mobile")] // Has 5 awaits
        public async Task<ActionResult> MobileAuthenticateAsync([FromBody] JObject j_obj)
        {
            ResponseMessage ressmsg = new ResponseMessage();
            try
            {
                if (j_obj["u_username"] == null || j_obj["u_password"] == null)
                    throw new Exception("", new Exception(HttpStatusCode.Unauthorized.ToString() + " : Incomplete Data!"));
                
                string u_username = (string)j_obj["u_username"];
                string u_password = (string)j_obj["u_password"];
                if (string.IsNullOrEmpty(u_username) || string.IsNullOrEmpty(u_password))
                    throw new Exception("", new Exception(HttpStatusCode.Unauthorized.ToString() + " : Incomplete Data!"));

                HttpResponseMessage response = null;

                // User Authentication Request
                response = await UserAuthenticationRequestAsync(u_username, u_password);
                if (response == null) throw new Exception(); // IsNull validation
                if (response.StatusCode != HttpStatusCode.OK) throw new Exception(); // HttpStatusCode Is OK validation

                string result = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(result)) throw new Exception(); // IsNullOrEmpty validation
                // resultData response from HttpResponseMessage
                dynamic resultData = JsonConvert.DeserializeObject(result);
                if (resultData == null) throw new Exception(); // IsNull validation
                string code = resultData.code.ToString();
                string message = resultData.message.ToString();
                if (code == "-1") throw new Exception(message);
                if (code == "0") throw new Exception("", new Exception(message));

                if (resultData.user == null) throw new Exception();
                dynamic user = resultData.user;
                int? homeid = null;
                if (user.home_id != null) homeid = (int)user.home_id;

                string uid = user.u_id.ToString();

                // Check if user is logged in or not
                string sessionJwt = await Task.Run(() => sessionCache.GetString(uid));
                if (!string.IsNullOrEmpty(sessionJwt))
                    throw new Exception("", new Exception(GeneralConst.Account_Used + " Account is used in another device!"));
                
                // Generate JWT Mobile
                string encodedJwt = await Task.Run(() => GenerateJwt(user, Startup.jwt_mobile_secret_key, Startup.jwt_mobile_issuer));
                if (string.IsNullOrEmpty(encodedJwt)) throw new Exception();

                // Store to Redis
                sessionCache.SetString(uid, encodedJwt);
                if (string.IsNullOrEmpty(sessionCache.GetString(uid))) throw new Exception("", new Exception("Failed save session cache to Redis!"));

                return Ok(new
                {
                    Code = code,
                    Message = message,
                    Data = new
                    {
                        u_id = (int)user.u_id,
                        u_username = user.u_username.ToString(),
                        home_id = homeid,
                        Token = encodedJwt,
                        profileUser = user.profileUser
                    }
                });
            }
            catch (Exception ex)
            {
                 if (ex.InnerException == null)
                {
                    ressmsg.Code = "-1";
                    // ressmsg.Message = HttpStatusCode.InternalServerError.ToString();
                    ressmsg.Message = ex.Message;
                }
                else
                {
                    ressmsg.Code = "0";
                    ressmsg.Message = ex.InnerException.Message;
                }

                return Ok(ressmsg);
            }
        }

        [HttpPost("Logout")] // Has 2 awaits
        public async Task<ActionResult> LogoutAsync([FromHeader] string Authorization, [FromHeader] string u_id)
        {
            ResponseMessage ressmsg = new ResponseMessage();
            try
            {
                if (string.IsNullOrEmpty(Authorization) || string.IsNullOrEmpty(u_id))
                    throw new Exception("", new Exception(HttpStatusCode.Unauthorized.ToString()));

                string tokenJwt = await Task.Run(() => sessionCache.GetString(u_id));
                if (string.IsNullOrEmpty(tokenJwt))
                    throw new Exception("", new Exception(HttpStatusCode.Unauthorized.ToString()));

                string headerJwt = Authorization.Replace("Bearer ","");
                if (headerJwt != tokenJwt)
                    throw new Exception("", new Exception(HttpStatusCode.Unauthorized.ToString()));

                await sessionCache.RemoveAsync(u_id);

                ressmsg.Code = "1";
                ressmsg.Message = "Logged Out";
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                {
                    ressmsg.Code = "-1";
                    // ressmsg.Message = HttpStatusCode.InternalServerError.ToString();
                    ressmsg.Message = ex.Message;
                }
                else
                {
                    ressmsg.Code = "0";
                    ressmsg.Message = ex.InnerException.Message;
                }
            }

            return Ok(ressmsg);
        }

        [HttpGet("ValidateToken")] // Has 1 await
        public async Task<ActionResult> ValidateTokenAsync([FromHeader] string Authorization, [FromHeader] string u_id)
        {
            ResponseMessage ressmsg = new ResponseMessage();
            try
            {
                if (string.IsNullOrEmpty(Authorization) || string.IsNullOrEmpty(u_id))
                    throw new Exception("", new Exception(HttpStatusCode.Unauthorized.ToString()));

                string tokenJwt = await Task.Run(() => sessionCache.GetString(u_id));
                if (string.IsNullOrEmpty(tokenJwt))
                    throw new Exception("", new Exception(HttpStatusCode.Unauthorized.ToString()));

                string headerJwt = Authorization.Replace("Bearer ","");
                if (headerJwt != tokenJwt)
                    throw new Exception("", new Exception(HttpStatusCode.Unauthorized.ToString()));

                ressmsg.Code = "1";
                ressmsg.Message = "Token is Valid";
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                {
                    ressmsg.Code = "-1";
                    // ressmsg.Message = HttpStatusCode.InternalServerError.ToString();
                    ressmsg.Message = ex.Message;
                }
                else
                {
                    ressmsg.Code = "0";
                    ressmsg.Message = ex.InnerException.Message;
                }
            }
            return Ok(ressmsg);
        }

        /// <summary>
        /// Method for User Authentication with asynchronous operation
        /// </summary>
        /// <param name="u_username">Username User</param>
        /// <param name="u_password">Password User</param>
        /// <returns>Task.Result HttpResponseMessage client data</returns>
        [NonAction]
        private async Task<HttpResponseMessage> UserAuthenticationRequestAsync(string u_username, string u_password)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpRequestMessage httpreqmsg = new HttpRequestMessage();
                    httpreqmsg.Method = HttpMethod.Post;
                    httpreqmsg.RequestUri = new Uri(Startup.PerpusApp_service_host + "/User/Authenticate");
                    string jsonData = "{" +
                                            "\"u_username\":\"" + u_username + "\"" +
                                            ",\"u_password\":\"" + u_password + "\"" +
                                      "}";
                    httpreqmsg.Content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                    client.DefaultRequestHeaders.Clear();

                    return await client.SendAsync(httpreqmsg);
                }
            }
            catch { return null; }
        }

        /// <summary>
        /// Method for Generate JSON Web Token (JWT)
        /// </summary>
        /// <param name="users">users object</param>
        /// <returns>JSON Web Token string</returns>
        [NonAction]
        private string GenerateJwt(dynamic users, string jwt_key, string jwt_issuer, string jwt_expire = null)
        {
            string uid = users.u_id.ToString();
            DateTime now = DateTime.UtcNow;
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, uid),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, now.ToUniversalTime().ToString(), ClaimValueTypes.Integer64),
                new Claim(ClaimTypes.Name, users.u_username.ToString())
                // new Claim(ClaimTypes., users.u_uc_id.ToString())
            };
            
            var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwt_key));

            var jwt = new JwtSecurityToken(
                issuer: jwt_issuer,
                audience: jwt_issuer,
                claims: claims,
                notBefore: now,
                expires: (jwt_expire == null) ? (DateTime?)null : now.AddMinutes(double.Parse(jwt_expire)),
                signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
            );

            string encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            return encodedJwt;
        }
    }
}