using DevExpress.Persistent.Base;
using Llamachant.ExpressApp.Demo.Module.BusinessObjects.Common;
using System.ComponentModel;

namespace Llamachant.ExpressApp.Demo.Module.BusinessObjects.Clients;

[DefaultClassOptions]
[DefaultProperty(nameof(FirstName))]
public class Contact : CustomBaseObject
{


    public virtual string FirstName { get; set; }

    public virtual string LastName { get; set; }

    public virtual string EmailAddress { get; set; }

    public virtual string PhoneNumber { get; set; }

    public virtual Client Client { get; set; }

}