using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Api.Extensions
{
    public static class SwaggerConfiguration
    {
        public static void ConfigureSwagger(SwaggerGenOptions options)
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "Your API", Version = "v1" });

            // Configure Swagger to include the authorization token field
            var securityScheme = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Description = "JWT Authorization header using the Bearer scheme",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            };

            options.AddSecurityDefinition("Bearer", securityScheme);

            var securityRequirement = new OpenApiSecurityRequirement
            {
                { securityScheme, new[] { "Bearer" } }
            };

            options.AddSecurityRequirement(securityRequirement);

            string path = @"C:\Users\victor.ndulue\OneDrive - Africa Prudential\source\repos\MyExpressWallet\Presentation\Controllers";
            var xmlFile = "MyExpressWalletDocumentation.xml";
            var xmlPath = Path.Combine(path, xmlFile);
            var documentationPath = Path.GetFullPath(xmlPath);

            options.IncludeXmlComments(documentationPath);
        }
    }
}