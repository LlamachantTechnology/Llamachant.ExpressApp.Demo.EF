using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using Llamachant.ExpressApp.Demo.Module.BusinessObjects.Common;

namespace Llamachant.ExpressApp.Demo.Module.BusinessObjects.Billing;

[NavigationItem("Setup")]
public class TaxRate : CustomBaseObject
{

    public virtual string Name { get; set; }

    [ModelDefault("EditMask", "P")]
    [ModelDefault("DisplayFormat", "{0:P}")]
    public virtual decimal Rate { get; set; }

}