# Llamachant.ExpressApp.Demo (EF)
##### A DevExpress XAF sample with the Llamachant.ExpressApp modules included

## Getting Started
In order to run this sample application, you must ensure the following:
1. You have a valid DevExpress Universal license (www.devexpress.com)
2. You get your DevExpress NuGet feed URL (https://nuget.devexpress.com)
3. You either create a DevExpressNuGet environment variable on your machine OR you replace the %DevExpressNuGet% variable with your feed URL manually in the nuget.config file
4. Specify the DevExpress version you want to use in Directory.Packages.props
5. Update your connection string in:
   * appsettings.json (Llamachant.ExpressApp.Demo.Blazor.Server)
   * App.config (Llamachant.ExpressApp.Demo.Win)
   * App.config (Llamachant.ExpressApp.Demo.WorkflowService)

### Creating an Environment Variable
* Windows key search > Environment Variables
* Will open a System Properties popup
* Click 'Environment Variables...' at the bottom
* Add new user variable
* Variable Name: DevExpressNuGet
* Variable Value copy url from: https://nuget.devexpress.com

> [!NOTE]  
> If you have a project open in visual studio when adding or changing this environment variable, you will need to close and reopen visual studio for it to work.


### Specify a DevExpress Version
* With the solution open, expand the Solution Items folder in Visual Studio
* Edit the Directory.Packages.props file to replace the DevExpressMajor version tag (Example: 25.1.3)


### Select projects to run
We suggest running either the Blazor application or the Win application along with the WorkflowService project

> [!TIP]
> The WorkflowService project is used to process workflow definitions in the application. In this sample, a new email is sent to any client that has an email address (No actual email is sent but the details are visible in the console).