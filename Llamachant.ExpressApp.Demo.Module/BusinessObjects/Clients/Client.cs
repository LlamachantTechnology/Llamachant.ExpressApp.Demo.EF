using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using Llamachant.ExpressApp.Demo.Module.BusinessObjects.Billing;
using Llamachant.ExpressApp.Demo.Module.BusinessObjects.Common;
using Llamachant.ExpressApp.Demo.Module.BusinessObjects.Security;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Llamachant.ExpressApp.Demo.Module.BusinessObjects.Clients;

[ImageName("Client")]
[DefaultClassOptions]
public class Client : CustomBaseObject
{
    public virtual string Name { get; set; }

    [FieldSize(250)]
    public virtual string Address { get; set; }

    public virtual string EmailAddress { get; set; }

    public virtual string PhoneNumber { get; set; }

    public virtual string Website { get; set; }

    public virtual TaxRate TaxRate { get; set; }

    [EditorAlias("LabelPropertyEditor")]
    [PersistentAlias($"ISNULL([Invoices][].Sum({nameof(Invoice.Total)}), 0) - ISNULL([Payments][].Sum({nameof(Payment.PaymentAmount)}), 0)")]
    public virtual decimal Balance => EvaluateAlias<decimal>();

    [Aggregated]
    public virtual IList<Contact> Contacts { get; set; } = new ObservableCollection<Contact>();

    [Aggregated]
    public virtual IList<BillableHours> BillableHours { get; set; } = new ObservableCollection<BillableHours>();

    [Aggregated]
    public virtual IList<Invoice> Invoices { get; set; } = new ObservableCollection<Invoice>();

    [Aggregated]
    public virtual IList<Payment> Payments { get; set; } = new ObservableCollection<Payment>();

    [Aggregated]
    public virtual IList<ClientDocument> Documents { get; set; } = new ObservableCollection<ClientDocument>();


    public virtual IList<ApplicationUser> Users { get; set; } = new ObservableCollection<ApplicationUser>();
}
