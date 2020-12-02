using System;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace JWT_Authentication
{
    public class Startup
    {
        private const string corspolicy_allowanyOriginMethodHeader = "corspolicy_allowanyOriginMethodHeader";

        public static string PerpusApp_service_host 
        {
            get;
            private set;
        }
        
        public static string jwt_secret_key
        {
            get;
            private set;
        }
        public static string jwt_issuer
        {
            get;
            private set;
        }
        public static string jwt_expire
        {
            get;
            private set;
        }
        public static string jwt_idle_expire
        {
            get;
            private set;
        }

        public static string jwt_mobile_secret_key
        {
            get;
            private set;
        }
        public static string jwt_mobile_issuer
        {
            get;
            private set;
        }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddNewtonsoftJson();

            try
            {
                string redis_instance_name = Configuration.GetSection("AppSettings")["redis:name"];
                string redis_config = Configuration.GetSection("AppSettings")["redis:config"];

                // Web JWT
                jwt_secret_key = Configuration.GetSection("AppSettings")["jwtAuth:jwtSecretKey"];
                jwt_issuer = Configuration.GetSection("AppSettings")["jwtAuth:jwtIssuer"];
                jwt_expire = Configuration.GetSection("AppSettings")["jwtAuth:jwtExpireMinutes"];
                jwt_idle_expire = Configuration.GetSection("AppSettings")["jwtAuth:jwtIdleExpireMinutes"];

                // Mobile JWT
                jwt_mobile_secret_key = Configuration.GetSection("AppSettings")["jwtAuth:jwtMobileSecretKey"];
                jwt_mobile_issuer = Configuration.GetSection("AppSettings")["jwtAuth:jwtMobileIssuer"];

                services.AddAuthentication(auth =>
                    auth.DefaultScheme = "JwtSchemeKey"
                )
                .AddJwtBearer("JwtSchemeKey", options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt_secret_key)),
                        ValidateIssuer = true,
                        ValidIssuer = jwt_issuer,
                        ValidateAudience = true,
                        ValidAudience = jwt_issuer,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero,
                        RequireExpirationTime = true
                    };
                })
                .AddJwtBearer("JwtMobileSchemeKey", options => 
                {
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt_mobile_secret_key)),
                        ValidateIssuer = true,
                        ValidIssuer = jwt_mobile_issuer,
                        ValidateAudience = true,
                        ValidAudience = jwt_mobile_issuer,
                        SaveSigninToken = true
                    };
                });

                services.AddSession();
                services.AddDistributedRedisCache(options => 
                {
                    options.InstanceName = redis_instance_name;
                    options.Configuration = redis_config;
                });
            }
            catch { }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSession();

            app.UseRouting();

            app.UseAuthentication();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            PerpusApp_service_host = Configuration.GetSection("AppSettings")["ExternalServices:host-PerpusApp-service"];
        }
    }
}
