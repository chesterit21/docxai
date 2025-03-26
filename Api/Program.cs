using Api.Domain.Attributes;
using Api.Domain.Converters;
using Api.Domain.Formatters;
using Api.Domain.Middlewares;
using Api.Domain.Miscellaneous;
using Docubase.api.Filters;
using Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

services.AddMemoryCache();
services.AddRouting(options => options.LowercaseUrls = true);
services.AddAuthorization();
services.AddHttpContextAccessor();
services.AddEndpointsApiExplorer();
services.AddMvc(options =>
{
    options.Filters.Add<ExceptionFilter>();
    //options.Filters.Add<AccessFilter>();
    options.Filters.Add(new AuthorizeFilter(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build()));
    options.InputFormatters.Insert(0, new HttpRawJsonBodyInputFormatter());
});

services.AddControllers(options =>
{
    options.Conventions.Add(new ControllerDocumentationConvention());
})
    .AddJsonOptions(options =>
    {
        //DONT SHOW NULL RESPONSE
        //options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.Converters.Add(new DateFormatConverter());
        options.JsonSerializerOptions.Converters.Add(new DateFormatConverterNullable());
        options.JsonSerializerOptions.Converters.Add(new JsonDoubleConverter());
        options.JsonSerializerOptions.AllowTrailingCommas = true;
        options.JsonSerializerOptions.ReadCommentHandling = System.Text.Json.JsonCommentHandling.Skip;
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    }
    );

services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        var key = configuration["Authentication:SymmetricSecurityKey"];
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ClockSkew = TimeSpan.Zero,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "shuba.co.id",
            ValidAudience = "shuba.co.id",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
        };
    });

services.AddCors(options =>
{
    options.AddPolicy("cors", policy =>
    {
        policy.AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials()
              .SetIsOriginAllowed(hostName => true); //.AllowAnyOrigin()
    });
});

services.AddEndpointsApiExplorer();
services.AddSwaggerGen(c =>
{
    c.TagActionsBy(api =>
    {
        if (api.ActionDescriptor is ControllerActionDescriptor actionDescriptor)
        {
            //var group = actionDescriptor.ControllerTypeInfo.GetCustomAttributes(typeof(GroupTagAttribute), true)
            //    .Cast<GroupTagAttribute>().FirstOrDefault();

            //return group != null
            //    ? [group.Name]
            //    : [actionDescriptor.ControllerName];

            var group = actionDescriptor.ControllerTypeInfo.GetCustomAttributes(typeof(DisplayNameAttribute), true)
            .Cast<DisplayNameAttribute>().FirstOrDefault();

            return group != null
                ? [group.DisplayName]
                : [actionDescriptor.ControllerName];
        }

        throw new NullReferenceException("Couldn't find the group name");
    });

    c.EnableAnnotations();
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "DMS", Version = "v1" });
    c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());

    // Configure bearer token in swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please insert a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()//new string[] { }
                    }
                });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath);
});

services.RegisterApplicationDependencies(builder.Configuration);


var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.DisplayRequestDuration();
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Docubase API by Shuba Solution v1");
        options.RoutePrefix = "swagger";
        options.DocumentTitle = "Docubase API by Shuba Solution";
        options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
    });
}

app.UseCors("cors");
app.UseAuthentication();
app.UseRouting();
app.UseAuthorization();
app.Use(next => context =>
{
    context.Request.EnableBuffering();
    return next(context);
});
app.UseMiddleware<UnauthorizedMessageMiddleware>();

app.MapControllers();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

Seeder.SeedIt(app.Services);

var uploadPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "upload");
if (!Directory.Exists(uploadPath))
{
    //builder.Environment.ContentRootPath;
    //EQUALS TO
    //Path.GetDirectoryName(Assembly.GetEntryAssembly().Location.Substring(0, Assembly.GetEntryAssembly().Location.IndexOf("bin\\")));

    Directory.CreateDirectory(uploadPath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadPath),
    RequestPath = "/resource"
});
//app.MapHub<MessageHub>("/ws", options =>
//{
//    //options.Transports = HttpTransportType.WebSockets | HttpTransportType.LongPolling;
//    options.TransportMaxBufferSize = (1024 * 1024) * 5;
//});
app.Run();
