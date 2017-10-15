using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Giti.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Giti.Services;
using Microsoft.AspNetCore.Http;
using System;
using Giti.Code;
using Microsoft.AspNetCore.Mvc.Razor;
using System.Globalization;
using System.Collections.Generic;
using Microsoft.AspNetCore.Localization;

namespace Giti
{
    public class Startup
    {
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
		{
            services.AddMvc().AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix);;
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
			services.AddDbContext<GitiContext>(options =>
													 options.UseNpgsql("Host=localhost;Port=5433;Username=Giti;Password=Giti;Database=Giti"));
            
			services.AddIdentity<ApplicationUser, IdentityRole>()
				.AddEntityFrameworkStores<GitiContext>()
				.AddDefaultTokenProviders();

			services.Configure<IdentityOptions>(options =>
			{
				// Password settings
				options.Password.RequireDigit = true;
				options.Password.RequiredLength = 8;
				options.Password.RequireNonAlphanumeric = false;
				options.Password.RequireUppercase = true;
				options.Password.RequireLowercase = false;
				options.Password.RequiredUniqueChars = 6;

				// Lockout settings
				options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
				options.Lockout.MaxFailedAccessAttempts = 10;
				options.Lockout.AllowedForNewUsers = true;

				// User settings
				options.User.RequireUniqueEmail = true;
			});

            services.ConfigureApplicationCookie(options =>
           {
               // Cookie settings
               options.Cookie.HttpOnly = true;
               options.Cookie.Expiration = TimeSpan.FromDays(7);
               options.LoginPath = "/Account/Login";
               options.LogoutPath = "/Account/Logout";
               options.SlidingExpiration = true;
           });

            // Add Basic Authentication
            services.AddAuthentication().AddCookie().AddBasic();

			// Add application services.
			services.AddTransient<IEmailSender, AuthMessageSender>();
			services.AddTransient<ISmsSender, AuthMessageSender>();

			// Add git services
			services.AddTransient<GitRepositoryService>();
			services.AddTransient<GitFileService>();

            //Add Localization service
            services.AddPortableObjectLocalization(options => options.ResourcesPath = "Localization");

            services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new List<CultureInfo>
                {
                    new CultureInfo("en-US"),
                    new CultureInfo("fa-IR")                
                };
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
            });
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");
			}

			app.UseStaticFiles();

            app.UseAuthentication();

            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture(Configuration["GitiSettings:DefaultCulture"])               
            });
			
			app.UseMvc(routes => RouteConfig.RegisterRoutes(routes));
		}
		
    }
}
