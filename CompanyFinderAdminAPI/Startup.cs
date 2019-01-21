using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CompanyDatabase.Models;
using CompanyFinderEmailTemplateLib.Interfaces;
using CompanyFinderEmailTemplateLib.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CompanyFinderAdminAPI
{
    /// <summary>
    /// Start up class for set up and middlewhere
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Startup  constructor
        /// </summary>
        /// <param name="configuration"></param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        /// <summary>
        /// Configuration interface object
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    builder =>
                    {
                        builder
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                    });
            });

            services.AddMvc().AddJsonOptions(options => {
                options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            });

            services.AddDbContext<CompanyDbContext>(options =>
            options.UseSqlServer(Configuration["Data:CompanyConnectionString:CompanyConnection"]));

            services.AddDbContext<AppIdentityDbContext>(options =>
            options.UseSqlServer(Configuration["Data:IdentityConnectionString:IdentityConnection"]));

            services.AddSingleton<IEmailConfiguration>(Configuration.GetSection("EmailConfiguration").Get<EmailConfiguration>());
            services.AddTransient<IEmailService, EmailService>();

            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<AppIdentityDbContext>()
                .AddDefaultTokenProviders();

        }

        private async Task CreateRoles(IServiceProvider serviceProvider)
        {
            // Add the custom roles
            var RoleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var UserManger = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            // Roles in the application
            string[] roleNames = { "Admin, TemplateAccess" };

            IdentityResult roleResult;

            foreach (var roleName in roleNames)
            {
                // Create the roles and seed them to the db
                var roleExists = await RoleManager.RoleExistsAsync(roleName);
                if (!roleExists)
                {
                    roleResult = await RoleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Create the super admin
            var powerUser = new IdentityUser
            {
                UserName = Configuration.GetSection("Data:AdminSettings")["SuperAdminName"],
            };



            string adminPassword = Configuration.GetSection("Data:AdminSettings")["SuperAdminPassword"];
            var _user = await UserManger.FindByNameAsync(Configuration.GetSection("Data:AdminSettings")["SuperAdminName"]);

            if (_user == null)
            {
                var createSuperUser = await UserManger.CreateAsync(powerUser, adminPassword);
                if (createSuperUser.Succeeded)
                {
                    // Give the new user admin role
                    await UserManger.AddToRoleAsync(powerUser, "Admin");
                }
            }

            //// Create the user
            var normalUser = new IdentityUser
            {
                UserName = Configuration.GetSection("Data:TemplateSettings")["UserName"],
            };

            string userPassword = Configuration.GetSection("Data:TemplateSettings")["UserPassword"];
            var _normalUser = await UserManger.FindByNameAsync(Configuration.GetSection("Data:TemplateSettings")["UserName"]);

            if (_normalUser == null)
            {
                var createUser = await UserManger.CreateAsync(normalUser, userPassword);
                if (createUser.Succeeded)
                {
                    // Give the new user a role
                    await UserManger.AddToRoleAsync(normalUser, "TemplateAccess");
                }
            }
        }


        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        /// <param name="serviceProvider"></param>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider serviceProvider)
        {
            try
            {   //Create Identity Database on first start up if not already exists
                using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>()
                    .CreateScope())
                {
                    serviceScope.ServiceProvider.GetService<AppIdentityDbContext>().Database.Migrate();

                }
            }
            catch (Exception ex)
            {

                Console.Write(ex);
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // ---- enable CORS for requests from anywhere ----
            app.UseCors(options => options.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod().AllowCredentials());

            app.UseMvc();

            CreateRoles(serviceProvider).Wait();
        }
    }
}
