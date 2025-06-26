using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using Llamachant.ExpressApp.Demo.Module.BusinessObjects.Clients;
using Llamachant.ExpressApp.Demo.Module.BusinessObjects.Common;
using System.Collections.ObjectModel;
using System.ComponentModel;
using static DevExpress.DataProcessing.InMemoryDataProcessor.AddSurrogateOperationAlgorithm;

namespace Llamachant.ExpressApp.Demo.Module.BusinessObjects.Billing;

[ImageName("BO_Invoice")]
[VisibleInReports(true)]
[VisibleInDashboards(true)]
public class Invoice : CustomBaseObject
{
    public override void OnCreated()
    {
        base.OnCreated();


    }

    public virtual Client Client { get; set; }

    public virtual DateTime InvoiceDate { get; set; }

    [FieldSize(FieldSizeAttribute.Unlimited)]
    public virtual string InvoiceNotes { get; set; }

    [PersistentAlias($"[InvoiceLineItems][].Sum([{nameof(InvoiceLineItem.LineItemTotal)}])")]
    public decimal SubTotal => EvaluateAlias<decimal>();

    [PersistentAlias("[SubTotal] * IIF(ISNULL(Client.TaxRate), 1, 1 + Client.TaxRate.Rate)")]
    public decimal Total => EvaluateAlias<decimal>();

    [Aggregated]
    public virtual IList<InvoiceLineItem> InvoiceLineItems { get; set; } = new ObservableCollection<InvoiceLineItem>();

    [Aggregated]
    //[DataSourceCriteria("Client = '@This.Client' AND ISNULL(Invoice)")]
    public virtual IList<BillableHours> BillableHours { get; set; } = new ObservableCollection<BillableHours>();


}
