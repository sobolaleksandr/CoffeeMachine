namespace CoffeeMachine.Web
{

    using CoffeMachine.Data;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger(options => options.SerializeAsV2 = true);
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
                    options.RoutePrefix = string.Empty;
                });
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            //services
            //    .AddDbContext<CoffeeMachineContext>(builder => builder.UseInMemoryDatabase("UnitOfWork"))
            //    .AddScoped<IUnitOfWork, UnitOfWork<CoffeeMachineContext>>();

            var connection = Configuration.GetConnectionString("DefaultConnection");
            services
                .AddDbContext<CoffeeMachineContext>(options => options.UseSqlServer(connection))
                .AddScoped<IUnitOfWork, UnitOfWork<CoffeeMachineContext>>();
            services.AddSwaggerGen();
            services.AddMvc();
        }
    }
}