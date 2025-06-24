using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Security;
using Llamachant.ExpressApp.Demo.Module.BusinessObjects.Security;

namespace Llamachant.ExpressApp.Demo.WorkflowService.Utils;


/// <summary>
/// This class lets us log in without a password for the workflow service
/// </summary>
public class WorkflowSecurity : AuthenticationStandard
{
    private string workflowUsername;
    public WorkflowSecurity(string username)
    {
        workflowUsername = username;
    }

    public override object Authenticate(IObjectSpace objectSpace)
    {
        return objectSpace.FindObject<ApplicationUser>(CriteriaOperator.FromLambda<ApplicationUser>(x => x.UserName == workflowUsername));
    }
}