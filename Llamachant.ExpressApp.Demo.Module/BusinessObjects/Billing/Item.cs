using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using Llamachant.ExpressApp.Demo.Module.BusinessObjects.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Llamachant.ExpressApp.Demo.Module.BusinessObjects.Billing;

[NavigationItem("Setup")]
public class Item : CustomBaseObject
{
    public virtual string Name { get; set; }

    [FieldSize(250)]
    public virtual string Description { get; set; }

    public virtual decimal DefaultPrice { get; set; }

    public virtual string SKU { get; set; }

    public virtual bool BasedOnHours { get; set; }
}