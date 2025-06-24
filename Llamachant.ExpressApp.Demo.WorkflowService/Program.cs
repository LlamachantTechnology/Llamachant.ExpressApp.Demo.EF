using DevExpress.ExpressApp;
using DevExpress.ExpressApp.AspNetCore.DesignTime;
using DevExpress.ExpressApp.Security;
using Llamachant.ExpressApp.Demo.Win;
using Llamachant.ExpressApp.Demo.WorkflowService.Utils;
using Llamachant.ExpressApp.Workflow.Service.Controllers;
using LlamachantFramework.Workflow.Service;
using LlamaLogger.Core.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Configuration;

//This will create a workflow service using your XAF application and start processing workflow definitions.
//This can be a console application, windows service, background service, Azure function, etc.
//https://www.llamachant.com/docs/Llamachant.ExpressApp.Workflow.Service


//This example has 5 steps:
//1. Create an XAF Application
//2. Configure the security and options for your application and then log in
//3. Initialize the Workflow Service
//4. Run the workflow
//5. Optional: Create a queue to run the workflow again on an interval

try
{

    #region "1. Create an XAF Application"
    string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
    //var xafApplication = CreateWinApplication(connectionString);  //Uncomment if you want to use the Win application instead of the Blazor application
    var xafApplication = CreateBlazorApplication(connectionString); //Uncomment if you want to use the Blazor application instead of the Win application
    #endregion

    #region "2. Configure the security and options for your application and then log in"
    ((SecurityStrategyComplex)xafApplication.Security).Authentication = new WorkflowSecurity("Admin"); //TIP: Create a Workflow Service User instead of Admin

    xafApplication.Setup();
    xafApplication.Security.Logon(xafApplication.ObjectSpaceProvider.CreateUpdatingObjectSpace(false));
    #endregion

    #region "3. Initialize the Workflow Service"
    //The LlamaLoggerConsoleLogger is a custom ILogController implementation used to log exceptions to Llama Logger (www.llamalogger.com).
    //You can create your own logger that implements the ILogger interface or use the LlamachantFramework.Workflow.Logging.FileSystemLogger or LlamachantFramework.Workflow.Logging.ConsoleLogger.
    var logger = new LlamaLoggerConsoleLogger("APIKey", "VersionNumber");
    WorkflowService.Instance.Initialize(xafApplication, logger);

    //Optional: Provide a custom email service (Default uses SMTP)
    WorkflowService.Instance.EmailService = new CustomEmailService(logger);
    #endregion

    #region "4. Run the workflow"
    WorkflowService.Instance.RunWorkflow();
    #endregion

    #region "5. Optional: Create a queue to run the workflow again on an interval"
    WorkflowQueueController queue = new WorkflowQueueController();
    queue.SetInterval(30); //Runs the workflow every 30 seconds
    queue.Start();
    #endregion


    Console.WriteLine($"The sample workflow service is running. It will run every 30 seconds.");
}
catch (Exception ex)
{
    Console.WriteLine(ex.GetFullExceptionText());
    Console.WriteLine();

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("An error occurred. If this is the first time you've run this application, the database may not exist. Log into the Win or Blazor application first and then run this service again.");
}

Console.ReadLine();



XafApplication CreateWinApplication(string connectionstring)
{
    var winApplication = ApplicationBuilder.BuildApplication(connectionstring);
    winApplication.SplashScreen = null;

    return winApplication;
}

XafApplication CreateBlazorApplication(string connectionstring)
{
    var hostBuilder = Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder => {
                webBuilder.UseStartup<Llamachant.ExpressApp.Demo.Blazor.Server.Startup>();
            });

    var blazorApplication = DesignTimeApplicationFactoryHelper.Create(hostBuilder);

    blazorApplication.ConnectionString = connectionstring;

    return blazorApplication;
}