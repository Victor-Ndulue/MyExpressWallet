using Application.Repositories.CommonRepo;
using Application.UnitOfWork.Implementations;
using Application.UnitOfWork.Interfaces;
using Microsoft.EntityFrameworkCore;
using Services.Implementations.ServiceCommon;
using Services.Implementations.ServiceEntities;
using Services.Interfaces.IServiceCommon;
using Services.Interfaces.IServiceEntities;
using Services.LoggerService.Implementation;
using Services.LoggerService.Interface;

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

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<ITransactionServices, TransactionServices>();
            services.AddScoped<IUserServices, UserServices>();
            services.AddAutoMapper(typeof(Services.MapInitializers.MappingProfile));
            services.AddSingleton<ILoggerManager, LoggerManager>();
            services.AddDbContext<AppDbContext>(opt => opt.UseSqlServer(config.GetConnectionString("SqlConnection")), ServiceLifetime.Scoped);
        }
    }
}
