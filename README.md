# CCLLC.CDS.FieldEncryption

Provides a set if services for implementing field level encryption and decryption
of CDS entity data.

Specific service methods are called via plugin event handlers that fire on Create and Update events
to encrypt data and upon Retrieve and RetrieveMultiple events to decrypt data. Decryption
is a two step process with operations in the PreOp stage that determine what fields
will be decrypted and if masking will be required and operations in the PostOp
stage that actually decrypt retrieved field data.

An additional service can be executed during the PreOp stage of a RetrieveMultiple event
to evaluate if the user is requesting a search of encrypted data and modify the query
criteria search all encrypted fields for the particular search value.

CDS field encryption is configured using XML data resources with configuration 
information similar to the following example that directs the service to manage encryption for
the ccllc_driverlicensenumber field of the contact entity.
```XML

<configuration>
    <entity recordType="contact">
        <field fieldName="ccllc_driverlicensenumber"/>
    </entity>
</configuration>

```
This is a simplified example that relies on default values and does not implement role based
access. See the [full documentation on configuration](\Configuration.md) for additional options and deployment
considerations.

See the [sample plugin code] to see how to implement the Field Encryption Service in a plugin.

###### Dependencies

- [CCLLC.CDS.Sdk]
- [CCLLC.Azure.Secrets]

###### Nuget Packages

- TBD


