using DevExpress.Persistent.Base;
using Llamachant.ExpressApp.Demo.Module.BusinessObjects.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Llamachant.ExpressApp.Demo.Module.BusinessObjects.Clients;

[ImageName("Action_Document_Object_Inplace")]
public class ClientDocument : CustomFileAttachment
{
    public virtual Client Client { get; set; }
}
