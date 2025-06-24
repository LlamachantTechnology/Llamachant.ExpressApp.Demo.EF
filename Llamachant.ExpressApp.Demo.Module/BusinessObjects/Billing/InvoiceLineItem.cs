using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using Llamachant.ExpressApp.Demo.Module.BusinessObjects.Common;

namespace Llamachant.ExpressApp.Demo.Module.BusinessObjects.Billing;

[Appearance("DenyEditOnBasedOnHours", AppearanceItemType = "ViewItem", Enabled = false, Criteria = "Item.BasedOnHours = TRUE", TargetItems = "Quantity")]
public class InvoiceLineItem : CustomBaseObject
{
    public virtual Item Item { get; set; }


    [ModelDefault("EditMask", "n2")]
    [ModelDefault("DisplayFormat", "{0:n2}")]
    public virtual decimal Quantity { get; set; }

    public virtual decimal Price { get; set; }

    [PersistentAlias("IIF([Item.BasedOnHours], ROUND([Invoice.BillableHours].Sum([Duration] / 60), 2), [Quantity] * [Price])")]
    public decimal LineItemTotal => EvaluateAlias<decimal>();

    public virtual Invoice Invoice { get; set; }
}