using Cavista.CTRecruita.Data.Contexts;
using Cavista.CTRecruita.Data.Entities.Auth;
using Cavista.CTRecruita.Data.Seeder;
using Cavista.CTRecruita.Utilities.Configurations;
using Cavista.CTRecruita.Utilities.Emailer;
using Cavista.CTRecruita.Web.Extensions;
using Cavista.CTRecruita.Web.Filters;
using Hangfire;
using Hangfire.MySql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var jwtConfig = builder.Configuration.GetSection("JWT").Get<JWTConfig>();
var hangfireConfig = builder.Configuration.GetSection("HangfireAuth").Get<HangFireConfig>();
var connectionStrings = builder.Configuration.GetSection("ConnectionStrings").Get<ConnectionStrings>();
var currentAssembly = Assembly.GetExecutingAssembly().FullName;

var hangfireUsername = hangfireConfig.Username;
var hangfirePassword = hangfireConfig.Password;
var seriLogConn = builder.Configuration.GetSection("SeriLog")["ConnectionString"];
var seriLogTable = builder.Configuration.GetSection("SeriLog")["Table"];

var jsoncredential = builder.Configuration.GetSection("Google_Auth")["jsoncredential"];

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.MySQL(seriLogConn, seriLogTable)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddDbContextFactory<ApplicationContext>(options =>
{
    options.UseMySql(connectionStrings.DefaultConnection, ServerVersion.AutoDetect(connectionStrings.DefaultConnection), mySqlOptins => mySqlOptins.MigrationsAssembly(currentAssembly));
});

builder.Services.AddDbContextFactory<ApplicationReadOnlyContext>(options =>
{
    options.UseMySql(connectionStrings.ReadOnlyConnection, ServerVersion.AutoDetect(connectionStrings.ReadOnlyConnection), mySqlOptins => mySqlOptins.MigrationsAssembly(currentAssembly));
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
});

var dataProtectionKeysFolder = Path.Combine(AppContext.BaseDirectory, "keys");
builder.Services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysFolder)) 
    .SetApplicationName("CTRecruita");

builder.Services.AddIdentityCore<AppUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;
})
.AddRoles<AppRole>()
.AddUserManager<UserManager<AppUser>>()
.AddSignInManager<SignInManager<AppUser>>()
.AddEntityFrameworkStores<ApplicationContext>()
.AddTokenProvider<DataProtectorTokenProvider<AppUser>>("REFRESHTOKENPROVIDER")
.AddDefaultTokenProviders();

builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
{
    options.TokenLifespan = TimeSpan.FromHours(24);
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = true;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidAudience = jwtConfig.Issuer,
        ValidIssuer = jwtConfig.Issuer,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.SigningKey)),
        ValidateLifetime = true,
        RequireExpirationTime = !builder.Environment.IsDevelopment()
    };
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(JwtBearerEvents));
            logger.LogError("Authentication Failed", context.Exception);
            return Task.CompletedTask;
        }
    };
    options.IncludeErrorDetails = true;
});

builder.Services.AddHangfire(cfg => cfg.UseStorage(new MySqlStorage(connectionStrings.HangfireConn, new MySqlStorageOptions
{

    TransactionTimeout = TimeSpan.FromMinutes(5),
    QueuePollInterval = TimeSpan.FromSeconds(15),
    InvisibilityTimeout = TimeSpan.FromMinutes(5)
})));

builder.Services.AddHangfireServer(options =>
{
    options.WorkerCount = 3;
});
//builder.Services.AddSingleton<IInputSanitizer, StrictSanitizerService>();
//builder.Services.AddScoped<IFileService, FileService>();
//builder.Services.AddScoped<IActivityLogService, ActivityLogService>();
builder.Services.AddScoped<ISmtpClientWrapper, SmtpClientWrapper>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IBackgroundJobClient, BackgroundJobClient>();
//builder.Services.AddScoped<IBackgroundJobService, BackgroundJobService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();

    });
});
builder.Services.AddLogging();

//builder.Services.AddSerilogUi(options =>
//{
//    options.UseMySqlServer(op =>
//    {
//        op.WithConnectionString(seriLogConn);
//        op.WithTable(seriLogTable);
//    });
//    options.AddScopedBasicAuthFilter<SerilogBasicAuthenticationFilter>();
//    // You can also add authentication/authorization options here if needed
//});

builder.Services.AddControllers();

builder.Services.AddMediatR();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.CustomSchemaIds(id => id.FullName!.Replace('+', '-'));
    o.SchemaFilter<EnumSchemaFilter>();
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter your token in this field",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        BearerFormat = "JWT"
    };
    o.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, securityScheme);
    var securityRequirement = new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = JwtBearerDefaults.AuthenticationScheme
                            }
                        },
                        []
                    }
                };
    o.AddSecurityRequirement(securityRequirement);
});

var app = builder.Build();
app.UseStaticFiles();
using (var scope = app.Services.CreateScope())
{
    var contextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ApplicationContext>>();
    using var database = contextFactory.CreateDbContext();
    {
        database.Database.Migrate();
    }

    Seeder.SeedAsync(app.Services);
}
// Configure the HTTP request pipeline.
/* if (app.Environment.IsDevelopment())
 {

 }*/
app.UseCors("AllowAll");
app.UseSwagger();
app.UseSwaggerUI();
app.UseSerilogRequestLogging();
//app.UseHangfireDashboard("/jobs", new DashboardOptions
//{
//    Authorization =
//        [
//            new HangfireCustomBasicAuthenticationFilter
//                    {
//                        Pass = hangfirePassword,
//                        User = hangfireUsername
//                    }
//        ]
//});

//app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
//app.UseSerilogUi(options =>
//{
//    options.WithRoutePrefix("serilogs");
//    options.WithAuthenticationType(AuthenticationType.Basic);
//    //options.Filter

//});
app.MapControllers();
app.MapFallbackToFile("index.html");
app.Run();