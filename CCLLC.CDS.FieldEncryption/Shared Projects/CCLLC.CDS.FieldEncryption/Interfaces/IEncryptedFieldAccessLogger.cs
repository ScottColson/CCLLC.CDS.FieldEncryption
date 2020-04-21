using System;

namespace CCLLC.CDS.FieldEncryption
{
    using CCLLC.Core;

    /// <summary>
    /// The level of access a user had to encrypted data.
    /// </summary>
    public enum AccessType { SearchHit, MaskedAccess, FullAccess}   
    
    /// <summary>
    /// Encrypted data access logging interface to capture the level of access a user had to a specific 
    /// encrypted field on a record. Using <see cref="CCLLC.Core.IRecordPointer{T}"/> rather than 
    /// <see cref="Microsoft.Xrm.Sdk.EntityReference"/> to prevent logging system from needing reference to
    /// the Microsoft.Xrm.Sdk.
    /// </summary>
    public interface IEncryptedFieldAccessLogger
    {
        void LogAccess(IRecordPointer<Guid> recordPointer, IRecordPointer<Guid> user, string fieldName, AccessType accessType);
    }
}
