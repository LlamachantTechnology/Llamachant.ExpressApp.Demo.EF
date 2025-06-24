using DevExpress.Persistent.Base;
using Llamachant.ExpressApp.Demo.Module.BusinessObjects.Common;

namespace Llamachant.ExpressApp.Demo.Module.BusinessObjects.Billing;

[NavigationItem("Setup")]
public class PaymentMethod : CustomBaseObject
{
    public virtual string Name { get; set; }

}