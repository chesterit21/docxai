using Api.DataAccess;
using Api.Domain.Attributes;
using Api.Domain.Converters;
using Api.Domain.EntityRequests.Dms;
using Api.Domain.Formatters;
using Api.Domain.Middlewares;
using Api.Domain.Miscellaneous;
using Api.Extensions;
using Api.Extensions.DI;
using Api.Services;
using Docubase.api.Filters;
using Docubase.api.Middleware;
using Docubase.api.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using Syncfusion.EJ2.SpellChecker;
using Syncfusion.Licensing;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;//ILogger

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

//var encword = "P@ssw0rd#2026".Encrypt();
//var makelicense = new RequestLicenseInfo
//{
//	UserCount = 3,
//	ValidityPeriod = DateTimeOffset.Now.AddYears(1000).ToUnixTimeSeconds()
//};
//var encrypted = Encryption.Encrypt(System.Text.Json.JsonSerializer.Serialize(makelicense));

var env = builder.Environment;

configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

string? path = configuration.GetValue<string>("WordEj2EditorParameter:SpellcheckDictionaryPath");
string? jsonFileName = configuration.GetValue<string>("WordEj2EditorParameter:SpellcheckJsonFilename");

// Set default path if null or empty
path = string.IsNullOrEmpty(path) ? Path.Combine(env.ContentRootPath, "App_Data") : Path.Combine(env.ContentRootPath, path);
// Set default json file name if null or empty
jsonFileName = string.IsNullOrEmpty(jsonFileName) ? Path.Combine(path, "spellcheck.json") : Path.Combine(path, jsonFileName);
// Initialize spell checker dictionaries if file exists
if (File.Exists(jsonFileName))
{
	string jsonImport = File.ReadAllText(jsonFileName);
	var spellChecks = JsonConvert.DeserializeObject<List<DictionaryData>>(jsonImport);
	var spellDictCollection = new List<DictionaryData>();
	string personalDictPath = null;
	if (spellChecks != null)
	{
		foreach (var spellCheck in spellChecks)
		{
			spellDictCollection.Add(new DictionaryData(
				spellCheck.LanguadeID,
				Path.Combine(path, spellCheck.DictionaryPath),
				Path.Combine(path, spellCheck.AffixPath)
			));
			personalDictPath = Path.Combine(path, spellCheck.PersonalDictPath);
		}
	}
	SpellChecker.InitializeDictionaries(spellDictCollection, personalDictPath, 3);
}

// Register LoginSettings
builder.Services.Configure<AppSettings.LoginData>(
	builder.Configuration.GetSection("Login"));

// Register ConnectionStringSettings
builder.Services.Configure<AppSettings.ConnectionStringProperty>(
	builder.Configuration.GetSection("ConnectionString"));

//services.AddDistributedMemoryCache();
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
services.AddLicenseManagement();

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
	//string feOrigin = configuration["ApplicationUrl"].ToString();
	options.AddPolicy("cors", policy =>
	{
		policy.AllowAnyMethod()
			   .AllowAnyHeader()
			   .AllowCredentials()
			   .SetIsOriginAllowed(hostName => true); //.AllowAnyOrigin()
	});
});


services.AddMemoryCache();
services.AddEndpointsApiExplorer();
services.Configure<GzipCompressionProviderOptions>(options => options.Level = System.IO.Compression.CompressionLevel.Optimal);
services.AddResponseCompression();


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
// Apply migrations at startup
//using (var scope = app.Services.CreateScope())
//{
//	var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();
//	dbContext.Database.Migrate(); // This applies any pending migrations
//}


//Register Syncfusion license
string licenseKey = "Ngo9BigBOggjHTQxAR8/V1JGaF5cXGpCf0x0RHxbf1x2ZFRMY1tbRn5PMyBoS35Rc0RhW31ccXFXQ2lUUEVyVEFf";
SyncfusionLicenseProvider.RegisterLicense(licenseKey);
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



app.UseRouting();
app.UseCors("cors");

app.UseMiddleware<SetupModeMiddleware>();

app.UseAuthentication();
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

app.UseTokenBlacklist();//extension method utk blacklist token ketika logout
app.UseResponseCompression();

////Check for file existence
//string flagFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "seed.flag");

//if (!File.Exists(flagFilePath))
//{
//	//Run seeder
//	//using var scope = app.Services.CreateScope();
//	//var seeder = scope.ServiceProvider.GetRequiredService<ISeeder>();
//	//await seeder.SeedAsync();

//	//await Seeder.SeedItAsync(app.Services);
//	_ = Task.Run(() => Seeder.SeedItAsync(app.Services));

//	//Create flag file to prevent reseeding
//	File.WriteAllText(flagFilePath, "Seeded on " + DateTime.Now);
//}

//Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)
//string baseDir = AppContext.BaseDirectory;
//string baseDir = AppDomain.CurrentDomain.BaseDirectory;
//var uploadPath = Path.Combine(baseDir, "upload");

var setting = AppSettings.Read();
string uploadPath = setting.ApplicationInfoData.AppUploadFolder;
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

string userPpFolder = "user_pp";
string userPpPath = Path.Combine(AppContext.BaseDirectory, userPpFolder);
if (!Directory.Exists(userPpPath))
{
	Directory.CreateDirectory(userPpPath);
}

app.UseStaticFiles(new StaticFileOptions
{
	FileProvider = new PhysicalFileProvider(userPpPath),
	RequestPath = "/profilep"
});

//var externalFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "C:\\MySite\\ExternalFolder");
//app.UseStaticFiles(new StaticFileOptions
//{
//	FileProvider = new PhysicalFileProvider(externalFolderPath),
//	RequestPath = "/external" // This will be the URL path
//});

//app.MapHub<MessageHub>("/ws", options =>
//{
//    //options.Transports = HttpTransportType.WebSockets | HttpTransportType.LongPolling;
//    options.TransportMaxBufferSize = (1024 * 1024) * 5;
//});
app.Run();
