using MarketUpApi.Extensions;
using MarketUpApi.Filters;
using MarketUpApi.Helpers;
using MarketUpApi.Middlewares;
using MarketUpApi.Rest;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Net;

namespace MarketUpApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {

            if (environment.IsDevelopment())
            {
                if (NetworkHelper.IsLocale())
                {
                    HttpClient.DefaultProxy = NetworkHelper.GetDefaultProxy();
                }
            }

            Configuration = configuration;
            Environment = environment;
        }

        public IConfiguration Configuration { get; }

        public IWebHostEnvironment Environment { get; }

        public void ConfigureServices(IServiceCollection services)
        {        
            services.AddHttpContextAccessor();

            //services.AddDbContext<IAppDbContext, AppDbContext>(options =>
            //{
            //    options.UseOracle(Configuration.GetConnectionString("DefaultConnection"));

            //    if (Environment.IsDevelopment())
            //    {
            //        options.EnableDetailedErrors()
            //            .EnableSensitiveDataLogging();
            //    }
            //});


            //services.AddTransient<AwpSubscriptionService>();


            services.AddControllers().AddNewtonsoftJson(options => options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder => builder.SetIsOriginAllowed(a => true)
                    .AllowAnyMethod()
                    .AllowCredentials()
                    .AllowAnyHeader());
            });

            services.AddSignalR(options =>
            {
                options.EnableDetailedErrors = true;
            });

            services.AddControllers();
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(options =>
                {
                    options.MetadataAddress = $"MarketUp/auth/realms/test/.well-known/openid-configuration";
                    options.RequireHttpsMetadata = false;
                    options.IncludeErrorDetails = true;

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateLifetime = true,
                        ValidateAudience = false,
                        ValidateIssuerSigningKey = true,
                        ValidateTokenReplay = true,
                        ValidateActor = false,
                        ValidateIssuer = true,
                        ValidIssuer = $"MarketUp/auth/realms/test"
                    };
                    options.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = context =>
                        {
                            context.NoResult();

                            var message = context.Exception.InnerException?.Message ?? context.Exception.Message;
                            var error = new ApiResponse
                            {
                                Error = message,
                                Success = false
                            };

                            context.Response.StatusCode = (int)HttpStatusCode.OK;
                            context.Response.ContentType = ApiResponseType.JsonResponse;

                            return context.Response.WriteAsync(error.ToString());
                        }
                    };
                });
            services.AddAuthorization();
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder => builder.SetIsOriginAllowed(a => true).AllowAnyMethod().AllowAnyHeader().AllowCredentials());
            });

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = $"Market API",
                    Version = $"v1"
                });
                options.OperationFilter<AuthOperationFilter>();
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 12345abcdef\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

            });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseAppException();

            app.UseCors("AllowAll");

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            if (!Environment.IsProduction())
            {
                app.UseSerilogRequestLogging();
            }
           
            app.UseMiddleware<RequestLoggingMiddleware>();
           
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.RoutePrefix = "";
                c.DocExpansion(DocExpansion.None);
                c.DisplayRequestDuration();
                c.DefaultModelRendering(ModelRendering.Example);

                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Market Api v1");
            });

        }
    }
}
