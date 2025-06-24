using DevExpress.ExpressApp;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using LlamachantFramework.FileAttachments.AzureBlobStorage;
using LlamachantFramework.Module.Interfaces;
using LlamachantFramework.Module.Utils.FileAttachments;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Llamachant.ExpressApp.Demo.Module.BusinessObjects.Common;

public abstract class CustomFileAttachment : CustomBaseObject, IFileAttachment
{
    [NotMapped]
    public IFileData FileData { get => this; set { } }

    public virtual int Size { get; set; }
    public virtual string StorageLocation { get; set; }
    public virtual string FileName { get; set; }

    public string CalculateStorageLocation()
    {
        var options = ProgramOptions.GetInstance(ObjectSpace);

        if (options.FileStorageType == FileStorageType.FileSystem) //File System Path
            return Path.Combine(options.FileStorageRootFolder, @"TestAttachments\MyFiles\");
        else //Azure Blob Storage
            return "TestAttachments/MyFiles/"; //Azure Blob Storage (Requires LlamachantFramework.FileAttachments.AzureBlobStorage Package)
    }

    public void Clear()
    {
        GetProcessor().Clear(this);
    }

    public void LoadFromStream(string fileName, Stream stream)
    {
        GetProcessor().LoadFromStream(fileName, stream, this);
    }

    public void SaveToStream(Stream stream)
    {
        GetProcessor().SaveToStream(stream, this);
    }

    private FileAttachmentProcessorBase GetProcessor()
    {
        var options = ProgramOptions.GetInstance(ObjectSpace);

        if (options.FileStorageType == FileStorageType.FileSystem)
        {
            if (String.IsNullOrWhiteSpace(options.FileStorageRootFolder))
                throw new UserFriendlyException("The File Storage Root Folder has not been configured in program options.");

            return new FileStorageFileAttachmentProcessor() { DeleteOnClear = true, OverwriteExistingFiles = false };
        }
        else
        {
            //Azure Blob Storage (Requires LlamachantFramework.FileAttachments.AzureBlobStorage Package)
            string blobStorageCS = options.AzureBlobStorageCS;
            string containerName = options.AzureBlobStorageContainer;

            if (String.IsNullOrWhiteSpace(blobStorageCS) || String.IsNullOrEmpty(containerName))
                throw new UserFriendlyException("The Azure Blob Storage connection string or container name has not been configured in program options.");

            return new AzureBlobStorageFileAttachmentProcessor(blobStorageCS, containerName) { DeleteOnClear = false, OverwriteExistingFiles = false, PublicAccessType = Azure.Storage.Blobs.Models.PublicAccessType.None };
        }
    }
}
