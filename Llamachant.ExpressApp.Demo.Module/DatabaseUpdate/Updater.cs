using Aqua.EnumerableExtensions;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.EF;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Updating;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;
using DevExpress.Persistent.BaseImpl.EF.PermissionPolicy;
using DevExpress.Persistent.BaseImpl.EFCore.AuditTrail;
using Llamachant.ExpressApp.Demo.Module.BusinessObjects.Billing;
using Llamachant.ExpressApp.Demo.Module.BusinessObjects.Clients;
using Llamachant.ExpressApp.Demo.Module.BusinessObjects.Security;
using LlamachantFramework.ConditionalAppearance.Module.BusinessObjects;
using LlamachantFramework.Validation.Module.BusinessObjects;
using LlamachantFramework.Workflow.EF.BusinessObjects;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Drawing;

namespace Llamachant.ExpressApp.Demo.Module.DatabaseUpdate;

// For more typical usage scenarios, be sure to check out https://docs.devexpress.com/eXpressAppFramework/DevExpress.ExpressApp.Updating.ModuleUpdater
public class Updater : ModuleUpdater {
    public Updater(IObjectSpace objectSpace, Version currentDBVersion) :
        base(objectSpace, currentDBVersion) {
    }
    public override void UpdateDatabaseAfterUpdateSchema() {
        base.UpdateDatabaseAfterUpdateSchema();

        // If a role doesn't exist in the database, create this role
        var defaultRole = CreateDefaultRole();
        var adminRole = CreateAdminRole();

        CreateDefaultData();

        ObjectSpace.CommitChanges(); //This line persists created object(s).

        UserManager userManager = ObjectSpace.ServiceProvider.GetRequiredService<UserManager>();

        // If a user named 'User' doesn't exist in the database, create this user
        if(userManager.FindUserByName<ApplicationUser>(ObjectSpace, "User") == null) {
            // Set a password if the standard authentication type is used
            string EmptyPassword = "";
            _ = userManager.CreateUser<ApplicationUser>(ObjectSpace, "User", EmptyPassword, (user) => {
                // Add the Users role to the user
                user.Roles.Add(defaultRole);
            });
        }

        // If a user named 'Admin' doesn't exist in the database, create this user
        if(userManager.FindUserByName<ApplicationUser>(ObjectSpace, "Admin") == null) {
            // Set a password if the standard authentication type is used
            string EmptyPassword = "";
            _ = userManager.CreateUser<ApplicationUser>(ObjectSpace, "Admin", EmptyPassword, (user) => {
                // Add the Administrators role to the user
                user.Roles.Add(adminRole);
            });
        }

        ObjectSpace.CommitChanges(); //This line persists created object(s).
    }
    public override void UpdateDatabaseBeforeUpdateSchema() {
        base.UpdateDatabaseBeforeUpdateSchema();
    }
    private PermissionPolicyRole CreateAdminRole() {
        PermissionPolicyRole adminRole = ObjectSpace.FirstOrDefault<PermissionPolicyRole>(r => r.Name == "Administrators");
        if(adminRole == null) {
            adminRole = ObjectSpace.CreateObject<PermissionPolicyRole>();
            adminRole.Name = "Administrators";
            adminRole.IsAdministrative = true;
        }
        return adminRole;
    }
    private PermissionPolicyRole CreateDefaultRole() {
        PermissionPolicyRole defaultRole = ObjectSpace.FirstOrDefault<PermissionPolicyRole>(role => role.Name == "Default");
        if(defaultRole == null) {
            defaultRole = ObjectSpace.CreateObject<PermissionPolicyRole>();
            defaultRole.Name = "Default";

            defaultRole.AddObjectPermissionFromLambda<ApplicationUser>(SecurityOperations.Read, cm => cm.ID == (Guid)CurrentUserIdOperator.CurrentUserId(), SecurityPermissionState.Allow);
            defaultRole.AddNavigationPermission(@"Application/NavigationItems/Items/Default/Items/MyDetails", SecurityPermissionState.Allow);
            defaultRole.AddMemberPermissionFromLambda<ApplicationUser>(SecurityOperations.Write, "ChangePasswordOnFirstLogon", cm => cm.ID == (Guid)CurrentUserIdOperator.CurrentUserId(), SecurityPermissionState.Allow);
            defaultRole.AddMemberPermissionFromLambda<ApplicationUser>(SecurityOperations.Write, "StoredPassword", cm => cm.ID == (Guid)CurrentUserIdOperator.CurrentUserId(), SecurityPermissionState.Allow);
            defaultRole.AddTypePermissionsRecursively<PermissionPolicyRole>(SecurityOperations.Read, SecurityPermissionState.Deny);
            defaultRole.AddObjectPermission<ModelDifference>(SecurityOperations.ReadWriteAccess, "UserId = ToStr(CurrentUserId())", SecurityPermissionState.Allow);
            defaultRole.AddObjectPermission<ModelDifferenceAspect>(SecurityOperations.ReadWriteAccess, "Owner.UserId = ToStr(CurrentUserId())", SecurityPermissionState.Allow);
            defaultRole.AddTypePermissionsRecursively<ModelDifference>(SecurityOperations.Create, SecurityPermissionState.Allow);
            defaultRole.AddTypePermissionsRecursively<ModelDifferenceAspect>(SecurityOperations.Create, SecurityPermissionState.Allow);
            defaultRole.AddTypePermission<AuditDataItemPersistent>(SecurityOperations.Read, SecurityPermissionState.Deny);
            defaultRole.AddObjectPermissionFromLambda<AuditDataItemPersistent>(SecurityOperations.Read, a => a.UserObject.Key == CurrentUserIdOperator.CurrentUserId().ToString(), SecurityPermissionState.Allow);
            defaultRole.AddTypePermission<AuditEFCoreWeakReference>(SecurityOperations.Read, SecurityPermissionState.Allow);
        }
        return defaultRole;
    }



    private void CreateDefaultData()
    {
        var client = ObjectSpace.FindObject<Client>(null);
        if (client == null)
        {
            CreateValidationRules();
            CreateAppearanceRules();
            CreateClients();
            CreateWorkflowDefinition();
        }
    }

    private void CreateValidationRules()
    {
        var fnr = ObjectSpace.CreateObject<ValidationRuleData>();
        fnr.Name = "Contact-FirstName";
        fnr.RuleType = LlamachantFramework.Validation.Enums.ValidationRuleType.RuleRequiredField;
        fnr.TargetObjectType = typeof(Contact);
        fnr.TargetProperty = nameof(Contact.FirstName);
        fnr.ResultType = DevExpress.Persistent.Validation.ValidationResultType.Error;
        fnr.CustomMessageTemplate = "First name is required (This is from a database validation rule!)";

        var lnr = ObjectSpace.CreateObject<ValidationRuleData>();
        lnr.Name = "Contact-LastName";
        lnr.RuleType = LlamachantFramework.Validation.Enums.ValidationRuleType.RuleRequiredField;
        lnr.TargetObjectType = typeof(Contact);
        lnr.TargetProperty = nameof(Contact.LastName);
        lnr.ResultType = DevExpress.Persistent.Validation.ValidationResultType.Error;
        lnr.CustomMessageTemplate = "Last name is required (This is from a database validation rule!)";
    }

    private void CreateAppearanceRules()
    {
        var rule = ObjectSpace.CreateObject<AppearanceRuleData>();
        rule.Name = "Client-Name Missing";
        rule.DataType = typeof(Client);
        rule.TargetItems = nameof(Client.Name);
        rule.Context = "DetailView";
        rule.Criteria = $"ISNULLOREMPTY([{nameof(Client.Name)}])";
        rule.AppearanceItemType = DevExpress.ExpressApp.ConditionalAppearance.AppearanceItemType.ViewItem;
        rule.BackColor = Color.Salmon;
        rule.FontColor = Color.Black;
    }

    private void CreateClients()
    {
        List<string> names = new List<string>()
        {
            "SnailRocket Inc.",
            "JellyButton Studios",
            "Fluffbyte",
            "PeachPylon LLC",
            "Cranky Narwhal Co."
        };

        foreach (string name in names)
        {
            var client = ObjectSpace.CreateObject<Client>();
            client.Name = name;

            var count = Random.Shared.Next(0, 5);
            for (int i = 0; i < count; i++)
                CreateContacts(client);

            count = Random.Shared.Next(0, 5);
            CreateBillableHours(client);

            if (Random.Shared.Next(0, 10) % 2 == 0)
                CreateInvoices(client);
        }
    }

    private void CreateContacts(Client client)
    {
        var names = new List<(string FirstName, string LastName)>
        {
            ("Alex", "Carter"),
            ("Jordan", "Henderson"),
            ("Taylor", "Reynolds"),
            ("Morgan", "Manning"),
            ("Cameron", "Griffin"),
            ("Avery", "Lawson"),
            ("Blake", "Foster"),
            ("Sydney", "Bennett"),
            ("Quinn", "Chambers"),
            ("Riley", "Dawson")
        };

        var contact = ObjectSpace.CreateObject<Contact>();
        contact.FirstName = names[Random.Shared.Next(0, names.Count - 1)].FirstName;
        contact.LastName = names[Random.Shared.Next(0, names.Count - 1)].LastName;
        contact.Client = client;
    }

    private void CreateBillableHours(Client client)
    {
        var hours = ObjectSpace.CreateObject<BillableHours>();
        hours.Client = client;
        hours.StartOn = DateTime.Today.AddDays(Random.Shared.Next(0, 7) * -1).AddHours(Random.Shared.Next(8, 18));
        hours.Duration = (double)Random.Shared.Next(1, 8) * 15.0 / 60.0;
        hours.BillableHoursDescription = "Hours Include: \r\n - Topic 1\r\n - Topic 2";
    }

    private void CreateInvoices(Client client)
    {
        var invoice = ObjectSpace.CreateObject<Invoice>();
        invoice.Client = client;
        ObjectSpace.GetObjects<BillableHours>(CriteriaOperator.FromLambda<BillableHours>(x => x.Client == client), true).ForEach(x => invoice.BillableHours.Add(x));

        var lineitem = ObjectSpace.CreateObject<InvoiceLineItem>();
        lineitem.Invoice = invoice;

        if (invoice.BillableHours.Count > 0)
        {
            lineitem.Item = FindOrCreateInvoiceItem("Hourly Consulting / Development / Training", true);
            lineitem.Price = 50;
        }
        else
        {
            lineitem.Item = FindOrCreateInvoiceItem("Llama Logger Subscription", false);
            lineitem.Price = 10;
        }
    }

    private Item FindOrCreateInvoiceItem(string name, bool basedonhours)
    {
        var item = ObjectSpace.FindObject<Item>(CriteriaOperator.FromLambda<Item>(x => x.Name == name), true);
        if (item == null)
        {
            item = ObjectSpace.CreateObject<Item>();
            item.Name = name;
            item.BasedOnHours = basedonhours;
        }
        return item;
    }

    private void CreateWorkflowDefinition()
    {
        var definition = ObjectSpace.CreateObject<WorkflowDefinition>();
        definition.Name = "Welcome Email (Will run one time per Client record)";
        definition.ObjectSpecific = true;
        definition.SendAnEmail = true;
        definition.TargetObjectType = typeof(Client);
        definition.Criteria = "Contains([EmailAddress], '@')";
        definition.EmailPropertyPath = nameof(Client.EmailAddress);
        definition.Frequency = LlamachantFramework.Workflow.Enums.WorkflowFrequency.OneTime;
        definition.EmailTemplate = CreateWorkflowEmailTemplate();
        definition.Active = true;
    }

    private WorkflowEmailTemplate CreateWorkflowEmailTemplate()
    {
        var template = ObjectSpace.CreateObject<WorkflowEmailTemplate>();
        template.Name = "Client Welcome Email Template";
        template.TargetObjectType = typeof(Client);
        template.Subject = "Welcome {{Name}}";
        template.Body = @"<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Transitional//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"">  <html xmlns=""http://www.w3.org/1999/xhtml"">   <head>    <meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8"" /><title>    </title>   </head>   <body>    <p style=""text-align:left;text-indent:0pt;margin:0pt 0pt 0pt 0pt;""><span style=""color:#000000;background-color:transparent;font-family:Calibri;font-size:11pt;font-weight:normal;font-style:normal;"">&lt;!DOCTYPE html&gt;</span></p><p style=""text-align:left;text-indent:0pt;margin:0pt 0pt 0pt 0pt;""><span style=""color:#000000;background-color:transparent;font-family:Calibri;font-size:11pt;font-weight:normal;font-style:normal;"">&lt;html lang=&quot;en&quot; xmlns=&quot;http://www.w3.org/1999/xhtml&quot;&gt;</span></p><p style=""text-align:left;text-indent:0pt;margin:0pt 0pt 0pt 0pt;""><span style=""color:#000000;background-color:transparent;font-family:Calibri;font-size:11pt;font-weight:normal;font-style:normal;"">&lt;head&gt;</span></p><p style=""text-align:left;text-indent:0pt;margin:0pt 0pt 0pt 0pt;""><span style=""color:#000000;background-color:transparent;font-family:Calibri;font-size:11pt;font-weight:normal;font-style:normal;"">&nbsp; &nbsp;&nbsp;&lt;meta charset=&quot;utf-8&quot; /&gt;</span></p><p style=""text-align:left;text-indent:0pt;margin:0pt 0pt 0pt 0pt;""><span style=""color:#000000;background-color:transparent;font-family:Calibri;font-size:11pt;font-weight:normal;font-style:normal;"">&nbsp; &nbsp;&nbsp;&lt;title&gt;&lt;/title&gt;</span></p><p style=""text-align:left;text-indent:0pt;margin:0pt 0pt 0pt 0pt;""><span style=""color:#000000;background-color:transparent;font-family:Calibri;font-size:11pt;font-weight:normal;font-style:normal;"">&nbsp; &nbsp;&nbsp;&lt;style&gt;</span></p><p style=""text-align:left;text-indent:0pt;margin:0pt 0pt 0pt 0pt;""><span style=""color:#000000;background-color:transparent;font-family:Calibri;font-size:11pt;font-weight:normal;font-style:normal;"">&nbsp; &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;a {</span></p><p style=""text-align:left;text-indent:0pt;margin:0pt 0pt 0pt 0pt;""><span style=""color:#000000;background-color:transparent;font-family:Calibri;font-size:11pt;font-weight:normal;font-style:normal;"">&nbsp; &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;text-decoration: none;</span></p><p style=""text-align:left;text-indent:0pt;margin:0pt 0pt 0pt 0pt;""><span style=""color:#000000;background-color:transparent;font-family:Calibri;font-size:11pt;font-weight:normal;font-style:normal;"">&nbsp; &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;}</span></p><p style=""text-align:left;text-indent:0pt;margin:0pt 0pt 0pt 0pt;""><span style=""color:#000000;background-color:transparent;font-family:Calibri;font-size:11pt;font-weight:normal;font-style:normal;"">&nbsp;</span></p><p style=""text-align:left;text-indent:0pt;margin:0pt 0pt 0pt 0pt;""><span style=""color:#000000;background-color:transparent;font-family:Calibri;font-size:11pt;font-weight:normal;font-style:normal;"">&nbsp; &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;td {</span></p><p style=""text-align:left;text-indent:0pt;margin:0pt 0pt 0pt 0pt;""><span style=""color:#000000;background-color:transparent;font-family:Calibri;font-size:11pt;font-weight:normal;font-style:normal;"">&nbsp; &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;padding-bottom: 10px;</span></p><p style=""text-align:left;text-indent:0pt;margin:0pt 0pt 0pt 0pt;""><span style=""color:#000000;background-color:transparent;font-family:Calibri;font-size:11pt;font-weight:normal;font-style:normal;"">&nbsp; &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;min-width: 115px;</span></p><p style=""text-align:left;text-indent:0pt;margin:0pt 0pt 0pt 0pt;""><span style=""color:#000000;background-color:transparent;font-family:Calibri;font-size:11pt;font-weight:normal;font-style:normal;"">&nbsp; &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;}</span></p><p style=""text-align:left;text-indent:0pt;margin:0pt 0pt 0pt 0pt;""><span style=""color:#000000;background-color:transparent;font-family:Calibri;font-size:11pt;font-weight:normal;font-style:normal;"">&nbsp;</span></p><p style=""text-align:left;text-indent:0pt;margin:0pt 0pt 0pt 0pt;""><span style=""color:#000000;background-color:transparent;font-family:Calibri;font-size:11pt;font-weight:normal;font-style:normal;"">&nbsp; &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;hr {</span></p><p style=""text-align:left;text-indent:0pt;margin:0pt 0pt 0pt 0pt;""><span style=""color:#000000;background-color:transparent;font-family:Calibri;font-size:11pt;font-weight:normal;font-style:normal;"">&nbsp; &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;width: 95%;</span></p><p style=""text-align:left;text-indent:0pt;margin:0pt 0pt 0pt 0pt;""><span style=""color:#000000;background-color:transparent;font-family:Calibri;font-size:11pt;font-weight:normal;font-style:normal;"">&nbsp; &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;margin: auto;</span></p><p style=""text-align:left;text-indent:0pt;margin:0pt 0pt 0pt 0pt;""><span style=""color:#000000;background-color:transparent;font-family:Calibri;font-size:11pt;font-weight:normal;font-style:normal;"">&nbsp; &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;border-top: solid 1px rgb(220, 220, 220);</span></p><p style=""text-align:left;text-indent:0pt;margin:0pt 0pt 0pt 0pt;""><span style=""color:#000000;background-color:transparent;font-family:Calibri;font-size:11pt;font-weight:normal;font-style:normal;"">&nbsp; &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;}</span></p><p style=""text-align:left;text-indent:0pt;margin:0pt 0pt 0pt 0pt;""><span style=""color:#000000;background-color:transparent;font-family:Calibri;font-size:11pt;font-weight:normal;font-style:normal;"">&nbsp;</span></p><p style=""text-align:left;text-indent:0pt;margin:0pt 0pt 0pt 0pt;""><span style=""color:#000000;background-color:transparent;font-family:Calibri;font-size:11pt;font-weight:normal;font-style:normal;"">&nbsp; &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;.cellheader {</span></p><p style=""text-align:left;text-indent:0pt;margin:0pt 0pt 0pt 0pt;""><span style=""color:#000000;background-color:transparent;font-family:Calibri;font-size:11pt;font-weight:normal;font-style:normal;"">&nbsp; &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;font-weight: bold;</span></p><p style=""text-align:left;text-indent:0pt;margin:0pt 0pt 0pt 0pt;""><span style=""color:#000000;background-color:transparent;font-family:Calibri;font-size:11pt;font-weight:normal;font-style:normal;"">&nbsp; &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;}</span></p><p style=""text-align:left;text-indent:0pt;margin:0pt 0pt 0pt 0pt;""><span style=""color:#000000;background-color:transparent;font-family:Calibri;font-size:11pt;font-weight:normal;font-style:normal;"">&nbsp; &nbsp;&nbsp;&lt;/style&gt;</span></p><p style=""text-align:left;text-indent:0pt;margin:0pt 0pt 0pt 0pt;""><span style=""color:#000000;background-color:transparent;font-family:Calibri;font-size:11pt;font-weight:normal;font-style:normal;"">&lt;/head&gt;</span></p><p style=""text-align:left;text-indent:0pt;margin:0pt 0pt 0pt 0pt;""><span style=""color:#000000;background-color:transparent;font-family:Calibri;font-size:11pt;font-weight:normal;font-style:normal;"">&lt;body style=&quot;font-family: sans-serif; font-size: 14px;&quot;&gt;</span></p><p style=""text-align:left;text-indent:0pt;margin:0pt 0pt 0pt 0pt;""><span style=""color:#000000;background-color:transparent;font-family:Calibri;font-size:11pt;font-weight:normal;font-style:normal;"">&nbsp; &nbsp;&nbsp;&lt;div style=&quot;background-color: rgb(245, 245, 245); border: solid 1px rgb(200, 200, 200); border-radius: 10px; padding-bottom: 10px;&quot;&gt;</span></p><p style=""text-align:left;text-indent:0pt;margin:0pt 0pt 0pt 0pt;""><span style=""color:#000000;background-color:transparent;font-family:Calibri;font-size:11pt;font-weight:normal;font-style:normal;"">&nbsp; &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&lt;div style=&quot;padding: 12px; padding-top: 0px; margin-bottom: 8px;&quot;&gt;</span></p><p style=""text-align:left;text-indent:0pt;margin:0pt 0pt 0pt 0pt;""><span style=""color:#000000;background-color:transparent;font-family:Calibri;font-size:11pt;font-weight:normal;font-style:normal;"">&nbsp; &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&lt;h2 style=&quot;font-size: 22px; font-weight: 400;&quot;&gt;Welcome to the Llamachant Framework Modules!&lt;/h2&gt;</span></p><p style=""text-align:left;text-indent:0pt;margin:0pt 0pt 0pt 0pt;""><span style=""color:#000000;background-color:transparent;font-family:Calibri;font-size:11pt;font-weight:normal;font-style:normal;"">&nbsp; &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&lt;a href=&quot;https://www.llamachant.com&quot;&gt;</span></p><p style=""text-align:left;text-indent:0pt;margin:0pt 0pt 0pt 0pt;""><span style=""color:#000000;background-color:transparent;font-family:Calibri;font-size:11pt;font-weight:normal;font-style:normal;"">&nbsp; &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&lt;span style=&quot;background-color: #ea9937; padding: 7px; border-radius: 5px; width: 130px; color: white; text-align: center; font-size: 13px;&quot;&gt; Visit Our Website&lt;/span&gt;</span></p><p style=""text-align:left;text-indent:0pt;margin:0pt 0pt 0pt 0pt;""><span style=""color:#000000;background-color:transparent;font-family:Calibri;font-size:11pt;font-weight:normal;font-style:normal;"">&nbsp; &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&lt;/a&gt;</span></p><p style=""text-align:left;text-indent:0pt;margin:0pt 0pt 0pt 0pt;""><span style=""color:#000000;background-color:transparent;font-family:Calibri;font-size:11pt;font-weight:normal;font-style:normal;"">&nbsp; &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&lt;/div&gt;</span></p><p style=""text-align:left;text-indent:0pt;margin:0pt 0pt 0pt 0pt;""><span style=""color:#000000;background-color:transparent;font-family:Calibri;font-size:11pt;font-weight:normal;font-style:normal;"">&nbsp; &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&lt;hr /&gt;</span></p><p style=""text-align:left;text-indent:0pt;margin:0pt 0pt 0pt 0pt;""><span style=""color:#000000;background-color:transparent;font-family:Calibri;font-size:11pt;font-weight:normal;font-style:normal;"">&nbsp; &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&lt;div style=&quot;color: rgb(60, 60, 60); padding: 10px; margin-top: 4px;&quot;&gt;</span></p><p style=""text-align:left;text-indent:0pt;margin:0pt 0pt 0pt 0pt;""><span style=""color:#000000;background-color:transparent;font-family:Calibri;font-size:11pt;font-weight:normal;font-style:normal;"">&nbsp; &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&lt;table&gt;</span></p><p style=""text-align:left;text-indent:0pt;margin:0pt 0pt 0pt 0pt;""><span style=""color:#000000;background-color:transparent;font-family:Calibri;font-size:11pt;font-weight:normal;font-style:normal;"">&nbsp; &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&lt;tr&gt;</span></p><p style=""text-align:left;text-indent:0pt;margin:0pt 0pt 0pt 0pt;""><span style=""color:#000000;background-color:transparent;font-family:Calibri;font-size:11pt;font-weight:normal;font-style:normal;"">&nbsp; &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&lt;td class=&quot;cellheader&quot;&gt;Client Name&lt;/td&gt;</span></p><p style=""text-align:left;text-indent:0pt;margin:0pt 0pt 0pt 0pt;""><span style=""color:#000000;background-color:transparent;font-family:Calibri;font-size:11pt;font-weight:normal;font-style:normal;"">&nbsp; &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&lt;td&gt;{{Name}}&lt;/td&gt;</span></p><p style=""text-align:left;text-indent:0pt;margin:0pt 0pt 0pt 0pt;""><span style=""color:#000000;background-color:transparent;font-family:Calibri;font-size:11pt;font-weight:normal;font-style:normal;"">&nbsp; &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&lt;/tr&gt;</span></p><p style=""text-align:left;text-indent:0pt;margin:0pt 0pt 0pt 0pt;""><span style=""color:#000000;background-color:transparent;font-family:Calibri;font-size:11pt;font-weight:normal;font-style:normal;"">&nbsp; &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&lt;/table&gt;</span></p><p style=""text-align:left;text-indent:0pt;margin:0pt 0pt 0pt 0pt;""><span style=""color:#000000;background-color:transparent;font-family:Calibri;font-size:11pt;font-weight:normal;font-style:normal;"">&nbsp; &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&lt;/div&gt;</span></p><p style=""text-align:left;text-indent:0pt;margin:0pt 0pt 0pt 0pt;""><span style=""color:#000000;background-color:transparent;font-family:Calibri;font-size:11pt;font-weight:normal;font-style:normal;"">&nbsp; &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&lt;div style=&quot;margin: 5px; border: solid 1px #cfcfcf;&quot;&gt;</span></p><p style=""text-align:left;text-indent:0pt;margin:0pt 0pt 0pt 0pt;""><span style=""color:#000000;background-color:transparent;font-family:Calibri;font-size:11pt;font-weight:normal;font-style:normal;"">&nbsp; &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&lt;/div&gt;</span></p><p style=""text-align:left;text-indent:0pt;margin:0pt 0pt 0pt 0pt;""><span style=""color:#000000;background-color:transparent;font-family:Calibri;font-size:11pt;font-weight:normal;font-style:normal;"">&nbsp; &nbsp;&nbsp;&lt;/div&gt;</span></p><p style=""text-align:left;text-indent:0pt;margin:0pt 0pt 0pt 0pt;""><span style=""color:#000000;background-color:transparent;font-family:Calibri;font-size:11pt;font-weight:normal;font-style:normal;"">&lt;/body&gt;</span></p><p style=""text-align:left;text-indent:0pt;margin:0pt 0pt 0pt 0pt;""><span style=""color:#000000;background-color:transparent;font-family:Calibri;font-size:11pt;font-weight:normal;font-style:normal;"">&lt;/html&gt;</span></p></body>  </html>  ";

        return template;
    }
}
