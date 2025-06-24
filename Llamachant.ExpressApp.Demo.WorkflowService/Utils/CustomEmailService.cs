using DevExpress.ExpressApp;
using LlamachantFramework.Workflow.Interfaces;
using LlamachantFramework.Workflow.Logging;
using LlamachantFramework.Workflow.Utils;

namespace Llamachant.ExpressApp.Demo.WorkflowService.Utils;

public class CustomEmailService : WorkflowEmailService
{
    ILogController logController;
    public CustomEmailService(ILogController logcontroller)
    {
        logController = logcontroller;        
    }
    public override void SendEmail(IObjectSpace space, string emailto, string subject, string body, byte[] attachmentdata, string reportname, IWorkflowInstance instance)
    {
        logController.LogRequired($"Since this is a fake email service, here's what would be sent:\r\n\tTo: {emailto}\r\n\tSubject: {subject}\r\n\tAttachment: {(attachmentdata == null || attachmentdata.Length == 0 ? "No" : "Yes")}\r\n\tReport: {(reportname ?? "None")}");

        //This is where you would send the email through your preferred provider.
        //base.SendEmail(space, emailto, subject, body, attachmentdata, reportname, instance);
    }
}
