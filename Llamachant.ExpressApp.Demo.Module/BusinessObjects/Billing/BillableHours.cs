using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base.General;
using DevExpress.Persistent.BaseImpl.EF;
using Llamachant.ExpressApp.Demo.Module.BusinessObjects.Clients;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Llamachant.ExpressApp.Demo.Module.BusinessObjects.Billing;

[DefaultProperty(nameof(BillableHoursDescription))]
public class BillableHours : Event
{
    [NotMapped]
    [ModelDefault("EditMask", "n2")]
    [ModelDefault("DisplayFormat", "{0:n2}")]
    public double Duration
    {
        get => EndOn == DateTime.MinValue || !EndOn.HasValue || !StartOn.HasValue ? 0 : EndOn.Value.Subtract(StartOn.Value).TotalHours;
        set => EndOn = StartOn.Value.AddMinutes(value);
    }

    [FieldSize(FieldSizeAttribute.Unlimited)]
    public virtual string BillableHoursDescription { get; set; }

    public virtual Client Client { get; set; }

    public virtual Invoice Invoice { get; set; }
}
