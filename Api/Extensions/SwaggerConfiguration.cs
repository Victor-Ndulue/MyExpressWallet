using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace Api.Extensions
{
    public static class SwaggerConfiguration
    {
        public static void ConfigureSwagger(SwaggerGenOptions options/* this IServiceCollection services*/)
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

            /* c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory,
        $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"));*/



            //// Set the path to your XML documentation file
            //var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            //string xmlPathString = servi "/Presentation/MyExpressWallet.xml";
            //var xmlPath = Path.GetFullPath(xmlPathString);
            //options.IncludeXmlComments(xmlPath);

        }
    }
}
