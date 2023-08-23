using Application.Repositories.CommonRepo;
using Microsoft.EntityFrameworkCore;

namespace Api.Extensions
{
    public static class ServiceExtensions
    {
        public static void ConfigureServiceInjection(this IServiceCollection services, IConfiguration config) 
        {
            services.AddCors(opts =>
            {
                opts.AddPolicy("CorsPolicy", builder =>
                {
                    builder.AllowAnyHeader();
                    builder.AllowAnyMethod();
                    builder.AllowAnyOrigin();
                });
            });

            services.AddDbContext<AppDbContext>(opt => opt.UseSqlServer(config.GetConnectionString("SqlConnection"), 
                b => b.MigrationsAssembly("Application")));
        }
    }
}
