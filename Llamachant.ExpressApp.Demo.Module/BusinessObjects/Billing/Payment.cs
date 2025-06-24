using DevExpress.Persistent.Base;
using Llamachant.ExpressApp.Demo.Module.BusinessObjects.Clients;
using Llamachant.ExpressApp.Demo.Module.BusinessObjects.Common;

namespace Llamachant.ExpressApp.Demo.Module.BusinessObjects.Billing;

[VisibleInReports(true)]
[VisibleInDashboards(true)]
public class Payment : CustomBaseObject
{
    public override void OnCreated()
    {
        base.OnCreated();

        PaymentDate = DateTime.Today;
    }


    public virtual DateTime PaymentDate { get; set; }

    public virtual PaymentMethod PaymentMethod { get; set; }

    public virtual string TransactionID { get; set; }

    public virtual decimal PaymentAmount { get; set; }

    public virtual Client Client { get; set; }

}