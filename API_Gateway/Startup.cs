using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

namespace API_Gateway
{
    public class Startup
    {
        private const string corspolicy_allowanyOriginMethodHeader = "corspolicy_allowanyOriginMethodHeader";
        private string jwtAuth_host;

        public Startup(IHostEnvironment env ,IConfiguration config)
        {
            // Configuration = configuration;
            var builder = new ConfigurationBuilder();
            builder.SetBasePath(env.ContentRootPath)
                .AddJsonFile("configuration.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables();
            Configuration = builder.Build();

            AppConfiguration = config;
        }

        public IConfiguration Configuration { get; set; }
        public IConfiguration AppConfiguration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // services.AddControllers();
            string key = AppConfiguration.GetSection("AppSettings")["jwtAuth:jwtSecretKey"];
            string issuer = AppConfiguration.GetSection("AppSettings")["jwtAuth:jwtIssuer"];

            services.AddCors(options =>
            {
                options.AddPolicy(corspolicy_allowanyOriginMethodHeader, builder =>
                {
                    builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                });
            });

            services.AddAuthentication(auth => {
                auth.DefaultScheme = "JwtSchemeKey";
            }).AddJwtBearer("JwtSchemeKey", jwt => {
                jwt.RequireHttpsMetadata = false;
                jwt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = issuer,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    RequireExpirationTime = true
                };
            });

            services.AddOcelot(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseCors(corspolicy_allowanyOriginMethodHeader);

            var configuration = new OcelotPipelineConfiguration
            {
                PreAuthorisationMiddleware = async (ctx, next) =>
                {
                    string path = ctx.Request.Path.ToString();
                    try
                    {
                        if (string.IsNullOrEmpty(path)) throw new Exception("", new Exception(HttpStatusCode.BadRequest.ToString()));

                        if (path != "/auth/login" && path != "/auth/login/resetpw")
                        {
                            IHeaderDictionary headerDict = ctx.Request.Headers;
                            HttpResponseMessage response = await CheckValidationAsync(headerDict); // Object reference not set to an instance of an object
                            if (response == null) throw new Exception();
                            if (response.StatusCode != HttpStatusCode.OK) throw new Exception();

                            string jsonDataContent = await response.Content.ReadAsStringAsync();
                            if (string.IsNullOrEmpty(jsonDataContent)) throw new Exception();

                            ResponseMessage resmsgContent = JsonSerializer.Deserialize<ResponseMessage>(
                                jsonDataContent, new JsonSerializerOptions{
                                    PropertyNameCaseInsensitive = true
                            });
                            if (resmsgContent.Code == "-1") throw new Exception(resmsgContent.Message);
                            if (resmsgContent.Code == "0") throw new Exception("", new Exception(resmsgContent.Message));

                            await next.Invoke();
                        }
                        else
                            await next.Invoke();
                    }
                    catch (Exception ex)
                    {
                        string ressCode = string.Empty;
                        string ressMsg = string.Empty;

                        if (ex.InnerException == null)
                        {
                            ressCode = "-1";
                            // ressMsg = HttpStatusCode.InternalServerError.ToString();
                            ressMsg = ex.Message;
                        }
                        else
                        {
                            ressCode = "0";
                            ressMsg = ex.InnerException.Message;
                        }

                        ctx.Response.StatusCode = 200;
                        string content = "{" +
                                            "\"Code\": \"" + ressCode + "\"" +
                                            ",\"Message\": \"" + ressMsg + "\"" +
                                         "}";
                        
                        await ctx.Response.WriteAsync(content);
                    }
                }
            };

            app.UseOcelot(configuration);

            jwtAuth_host = AppConfiguration.GetSection("AppSettings")["ExternalServices:host-jwtAuth"];

            // if (env.IsDevelopment())
            // {
            //     app.UseDeveloperExceptionPage();
            // }

            // app.UseHttpsRedirection();

            // app.UseRouting();

            // app.UseAuthorization();

            // app.UseEndpoints(endpoints =>
            // {
            //     endpoints.MapControllers();
            // });
        }

        private async Task<HttpResponseMessage> CheckValidationAsync(IHeaderDictionary headerDict)
        {
            HttpClient client = new HttpClient();
            try
            {
                client.DefaultRequestHeaders.Clear();
                if (headerDict.ContainsKey("Authorization"))
                    client.DefaultRequestHeaders.Add("Authorization", headerDict["Authorization"].ToString());
                
                if (headerDict.ContainsKey("u_id"))
                    client.DefaultRequestHeaders.Add("u_id", headerDict["u_id"].ToString());
                
                return await client.GetAsync(jwtAuth_host + "/ValidateToken");
            }
            catch
            {
                return null;
            }
        }
    }
}
