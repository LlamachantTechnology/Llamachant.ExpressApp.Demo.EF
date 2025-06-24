using DevExpress.Data.Filtering;
using DevExpress.ExpressApp.Filtering;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;
using Llamachant.ExpressApp.Demo.Module.BusinessObjects.Security;
using LlamachantFramework.Module.Interfaces;
using LlamachantFramework.Module.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Llamachant.ExpressApp.Demo.Module.BusinessObjects.Common;

public abstract class CustomBaseObject : BaseObject, ITrackedObject
{
    [NonCloneable]
    [ModelDefault("DisplayFormat", "{0:g}")]
    [ModelDefault("AllowEdit", "False")]
    [VisibleInDetailView(false)]
    [VisibleInListView(false)]
    [VisibleInLookupListView(false)]
    [EditorAlias("LabelPropertyEditor")]
    public virtual DateTime CreatedOn { get; set; }
    [NonCloneable]
    [ModelDefault("DisplayFormat", "{0:g}")]
    [ModelDefault("AllowEdit", "False")]
    [VisibleInDetailView(false)]
    [VisibleInListView(false)]
    [VisibleInLookupListView(false)]
    [EditorAlias("LabelPropertyEditor")]
    public virtual DateTime ModifiedOn { get; set; }
    [NonCloneable]
    [ModelDefault("DisplayFormat", "{0:g}")]
    [ModelDefault("AllowEdit", "False")]
    [VisibleInDetailView(false)]
    [VisibleInListView(false)]
    [VisibleInLookupListView(false)]
    [EditorAlias("LabelPropertyEditor")]
    public virtual DateTime DeletedOn { get; set; }

    [SearchMemberOptions(SearchMemberMode.Exclude)]
    [NonCloneable]
    [ModelDefault("AllowEdit", "False")]
    [VisibleInDetailView(false)]
    [VisibleInListView(false)]
    [VisibleInLookupListView(false)]
    [EditorAlias("LabelPropertyEditor")]
    public virtual ApplicationUser CreatedBy { get; set; }

    [SearchMemberOptions(SearchMemberMode.Exclude)]
    [NonCloneable]
    [ModelDefault("AllowEdit", "False")]
    [VisibleInDetailView(false)]
    [VisibleInListView(false)]
    [VisibleInLookupListView(false)]
    [EditorAlias("LabelPropertyEditor")]
    public virtual ApplicationUser ModifiedBy { get; set; }

    [SearchMemberOptions(SearchMemberMode.Exclude)]
    [NonCloneable]
    [ModelDefault("AllowEdit", "False")]
    [VisibleInDetailView(false)]
    [VisibleInListView(false)]
    [VisibleInLookupListView(false)]
    [EditorAlias("LabelPropertyEditor")]
    public virtual ApplicationUser DeletedBy { get; set; }

    public override void OnCreated()
    {
        base.OnCreated();
        TrackedObjectHelper.AfterConstruction(this, nameof(CreatedBy));
    }

    public override void OnSaving()
    {
        //ModifiedOn = DateTime.Now;
        //SetPropertyValueWithSecurityBypass(nameof(ModifiedBy), (ApplicationUser)GetCurrentUser());

        TrackedObjectHelper.OnSaving(this, nameof(ModifiedBy));

        if (ObjectSpace.IsObjectToDelete(this))
        {
            TrackedObjectHelper.OnDeleting(this, nameof(DeletedBy));
            //DeletedOn = DateTime.Now;
            //SetPropertyValueWithSecurityBypass(nameof(DeletedBy), (ApplicationUser)GetCurrentUser());
        }

        base.OnSaving();
    }

    public object GetCurrentUser()
    {
        return ObjectSpace.FindObject<ApplicationUser>(CriteriaOperator.Parse("ID = CurrentUserId()"));
    }
}
