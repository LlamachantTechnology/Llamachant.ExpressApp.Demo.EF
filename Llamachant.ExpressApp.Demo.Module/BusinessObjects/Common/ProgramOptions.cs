using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using LlamachantFramework.Module.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Llamachant.ExpressApp.Demo.Module.BusinessObjects.Common;

[ImageName("ProgramOptions")]
[SingletonBO]
[DefaultClassOptions]
[Appearance("ProgramOptions-FileStorage-Azure", $"[{nameof(FileStorageType)}] = 1", TargetItems = nameof(FileStorageRootFolder), Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "DetailView")]
[Appearance("ProgramOptions-FileStorage-FileSystem", $"[{nameof(FileStorageType)}] = 0", TargetItems = $"{nameof(AzureBlobStorageContainer)},{nameof(AzureBlobStorageCS)}", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "DetailView")]
public class ProgramOptions : CustomBaseObject
{
    public static ProgramOptions GetInstance(IObjectSpace space)
    {
        var options = space.FindObject<ProgramOptions>(null, true);
        if (options == null)
            options = space.CreateObject<ProgramOptions>();

        return options;
    }

    [RuleRequiredField]
    public virtual string CompanyName { get; set; }

    public virtual string Address { get; set; }

    public virtual string PhoneNumber { get; set; }

    public virtual string Website { get; set; }

    public virtual string EmailAddress { get; set; }

    [ImmediatePostData]
    public virtual FileStorageType FileStorageType { get; set; }

    [ModelDefault("RowCount", "1")]
    [FieldSize(512)]
    public virtual string AzureBlobStorageCS { get; set; }

    public virtual string AzureBlobStorageContainer { get; set; }

    [ModelDefault("RowCount", "1")]
    [FieldSize(512)]
    public virtual string FileStorageRootFolder { get; set; }
}

public enum FileStorageType
{
    FileSystem = 0, Azure = 1, AWS = 2, GoogleCloud = 3, Database = 4, Other = 100
}
