﻿using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.ApplicationBuilder;
using DevExpress.ExpressApp.Blazor.ApplicationBuilder;
using DevExpress.ExpressApp.Blazor.Services;
using DevExpress.Persistent.Base;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.EntityFrameworkCore;
using Llamachant.ExpressApp.Demo.Blazor.Server.Services;
using DevExpress.Persistent.BaseImpl.EF.PermissionPolicy;
using Llamachant.ExpressApp.Demo.Module.BusinessObjects.Security;

namespace Llamachant.ExpressApp.Demo.Blazor.Server;

public class Startup {
    public Startup(IConfiguration configuration) {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    public void ConfigureServices(IServiceCollection services) {
        services.AddSingleton(typeof(Microsoft.AspNetCore.SignalR.HubConnectionHandler<>), typeof(ProxyHubConnectionHandler<>));

        services.AddRazorPages();
        services.AddServerSideBlazor();
        services.AddHttpContextAccessor();
        services.AddScoped<CircuitHandler, CircuitHandlerProxy>();
        services.AddXaf(Configuration, builder => {
            builder.UseApplication<DemoBlazorApplication>();
            builder.Modules
                .AddAuditTrailEFCore()
                .AddCloning()
                .AddConditionalAppearance()
                .AddDashboards(options => {
                    options.DashboardDataType = typeof(DevExpress.Persistent.BaseImpl.EF.DashboardData);
                })
                .AddFileAttachments()
                .AddOffice()
                .AddReports(options => {
                    options.EnableInplaceReports = true;
                    options.ReportDataType = typeof(DevExpress.Persistent.BaseImpl.EF.ReportDataV2);
                    options.ReportStoreMode = DevExpress.ExpressApp.ReportsV2.ReportStoreModes.XML;
                })
                .AddScheduler()
                .AddValidation(options => {
                    options.AllowValidationDetailsAccess = false;
                })
                .AddViewVariants()
                .Add<Llamachant.ExpressApp.Demo.Module.DemoModule>()
                .Add<DemoBlazorModule>()


                //Add Llamachant Framework Modules
                .AddLlamachantFrameworkBlazor()
                .AddLlamachantFrameworkWizardModuleBlazor()
                .AddLlamachantFrameworkDragDropBlazorModule(options =>
                {
                    options.UseNestedFrameTemplate = true;
                    options.UseDetailFormTemplate = true;
                });


            builder.ObjectSpaceProviders
                .AddSecuredEFCore(options => options.PreFetchReferenceProperties())
                    .WithAuditedDbContext(contexts => {
                        contexts.Configure<Llamachant.ExpressApp.Demo.Module.BusinessObjects.DemoEFCoreDbContext, Llamachant.ExpressApp.Demo.Module.BusinessObjects.DemoAuditingDbContext>(
                            (serviceProvider, businessObjectDbContextOptions) => {
                                // Uncomment this code to use an in-memory database. This database is recreated each time the server starts. With the in-memory database, you don't need to make a migration when the data model is changed.
                                // Do not use this code in production environment to avoid data loss.
                                // We recommend that you refer to the following help topic before you use an in-memory database: https://docs.microsoft.com/en-us/ef/core/testing/in-memory
                                //businessObjectDbContextOptions.UseInMemoryDatabase("InMemory");
                                string connectionString = null;
                                if(Configuration.GetConnectionString("ConnectionString") != null) {
                                    connectionString = Configuration.GetConnectionString("ConnectionString");
                                }
#if EASYTEST
                                if(Configuration.GetConnectionString("EasyTestConnectionString") != null) {
                                    connectionString = Configuration.GetConnectionString("EasyTestConnectionString");
                                }
#endif
                                ArgumentNullException.ThrowIfNull(connectionString);
                                businessObjectDbContextOptions.UseSqlServer(connectionString);
                                businessObjectDbContextOptions.UseChangeTrackingProxies();
                                businessObjectDbContextOptions.UseObjectSpaceLinkProxies();
                                businessObjectDbContextOptions.UseLazyLoadingProxies();
                            },
                            (serviceProvider, auditHistoryDbContextOptions) => {
                                string connectionString = null;
                                if(Configuration.GetConnectionString("ConnectionString") != null) {
                                    connectionString = Configuration.GetConnectionString("ConnectionString");
                                }
#if EASYTEST
                                if(Configuration.GetConnectionString("EasyTestConnectionString") != null) {
                                    connectionString = Configuration.GetConnectionString("EasyTestConnectionString");
                                }
#endif
                                ArgumentNullException.ThrowIfNull(connectionString);
                                auditHistoryDbContextOptions.UseSqlServer(connectionString);
                                auditHistoryDbContextOptions.UseChangeTrackingProxies();
                                auditHistoryDbContextOptions.UseObjectSpaceLinkProxies();
                                auditHistoryDbContextOptions.UseLazyLoadingProxies();
                            });
                    })
                .AddNonPersistent();
            builder.Security
                .UseIntegratedMode(options => {
                    options.Lockout.Enabled = true;

                    options.RoleType = typeof(PermissionPolicyRole);
                    // ApplicationUser descends from PermissionPolicyUser and supports the OAuth authentication. For more information, refer to the following topic: https://docs.devexpress.com/eXpressAppFramework/402197
                    // If your application uses PermissionPolicyUser or a custom user type, set the UserType property as follows:
                    options.UserType = typeof(ApplicationUser);
                    // ApplicationUserLoginInfo is only necessary for applications that use the ApplicationUser user type.
                    // If you use PermissionPolicyUser or a custom user type, comment out the following line:
                    options.UserLoginInfoType = typeof(ApplicationUserLoginInfo);
                    options.Events.OnSecurityStrategyCreated += securityStrategy => {
                        // Use the 'PermissionsReloadMode.NoCache' option to load the most recent permissions from the database once
                        // for every DbContext instance when secured data is accessed through this instance for the first time.
                        // Use the 'PermissionsReloadMode.CacheOnFirstAccess' option to reduce the number of database queries.
                        // In this case, permission requests are loaded and cached when secured data is accessed for the first time
                        // and used until the current user logs out.
                        // See the following article for more details: https://docs.devexpress.com/eXpressAppFramework/DevExpress.ExpressApp.Security.SecurityStrategy.PermissionsReloadMode.
                        ((SecurityStrategy)securityStrategy).PermissionsReloadMode = PermissionsReloadMode.NoCache;
                    };
                })
                .AddPasswordAuthentication(options => {
                    options.IsSupportChangePassword = true;
                });
        });
        var authentication = services.AddAuthentication(options => {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        });
        authentication.AddCookie(options => {
            options.LoginPath = "/LoginPage";
        });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
        if(env.IsDevelopment()) {
            app.UseDeveloperExceptionPage();
        }
        else {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. To change this for production scenarios, see: https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }
        app.UseHttpsRedirection();
        app.UseRequestLocalization();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseXaf();
        app.UseEndpoints(endpoints => {
            endpoints.MapXafEndpoints();
            endpoints.MapBlazorHub();
            endpoints.MapFallbackToPage("/_Host");
            endpoints.MapControllers();
        });
    }
}
