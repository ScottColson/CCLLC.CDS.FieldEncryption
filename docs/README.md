# CCLLC.CDS.FieldEncryption

Provides a set if services that can be used within a CDS Plugin to implement field level encryption 
and decryption of CDS entity data. See the [sample plugin code](https://github.com/ScottColson/CCLLC.CDS.FieldEncryption/blob/master/CCLLC.CDS.FieldEncryption.Sample/FieldEncryptionPlugin.cs)
for an example plugin implementation.

##### Features

1. Uses the Rijndael algorithm to encrypt and decrypt data using an encryption key stored as an Azure Key Vault Secret.
2. Configurable to support multiple entities and multiple fields within each entity.
3. Supports user triggered record query and quick find against decrypted field data using a special search trigger character.
4. Encrypts data in configured fields prior to writing the field data to the CDS data store.
5. Optional data format cleaning prior to field data encryption and reformatting prior to output to provide improved searchability when conducting an encrypted data search
6. Role based decryption authorization with optional partial masking of field data prior to display.
7. Supports implicit or explicit field decryption as part of a Retrieve or RetrieveMultiple operation.
8. Optional logging of decrypted data access by system users.

##### Key Concepts

###### Explicit Decryption

The Encrypted Field Service defaults to requiring the user/developer to explicitly direct it to decrypt data
as part of a Retrieve or RetrieveMultiple operation. Decryption is explicitly directed by including a field 
defined on the entity that is used as an "unmasking attribute" or flag. These unmasking attributes can be
of any type but since they are not expected to actually contain data using a Boolean/Two Option field is 
the most efficient option.

This unmasking attribute can be defined in the system configuration at a global, entity, or field level. If
this attribute is included in the column set of the Retrieve or RetrieveMultiple message, and the user has 
an appropriate role, then the service will decrypt and format the encrypted field data as part of the 
operation. Formatting may optionally include partial data masking depending on the users security roles.

For basic form or view use, decryption can be explicitly directed by adding the trigger attribute field to 
the form or to the view column list. However, keep in mind that the trigger attribute field will not be 
returned in the query result so the form field or view field will always be blank.

To provide the highest level of protection for the encrypted field data, consider adding a secondary retrieve
operation launched via a scripted retrieve based on user action such that the default form action is to not 
decrypt the data and show it as a masked field with an option for the user to click something to decrypt and
view the data.

Implicit decryption can be configured at the global, entity or field level by setting the _unmaskTriggerAttribute_ 
to a wild card (*) value. Implicit decryption results in a decryption of field data on every Retrieve or
RetrieveMultiple operation as long as the users roles allow it. This mode therefore has more impact on system
performance and offers less protection for the encrypted data because it will be displayed as decrypted at
all times.

###### Encrypted Search

TBD

###### Configuration    

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
access. See the [full documentation on configuration](Configuration.md) for additional options and deployment
considerations.

All XML configuration data resource records are named with a common "virtual directory" prefix where the 
CDS Field Encryption Service will look for them and load them. For example the following data resource
records could be used to define global settings and configuration for contacts and accounts.

- ccllc_/configuration/encryptedfields/settings.xml
- ccllc_/configuration/encryptedfields/entities/account.xml
- ccllc_/configuration/encryptedfields/entities/contact.xml

To override the default prefix path of *ccllc_/configuration/encryptedfields/* set a different path 
value in the CDS Environment Variables with a key name of *CCLLC.EncryptedFields.DataPath*.

By default the configuration data loaded by the service will be cached for a given plugin for 15 minutes. This
cache time can be modified by setting a numeric value representing the number of seconds to cache in 
the CDS Environment Variable key name *CCLLC.EncryptedFields.CacheTimeout*. 



###### Dependencies

- [CCLLC.CDS.Sdk](https://scottcolson.github.io/CCLLCCodeLibraries/CCLLC.CDS.Sdk.html)
- [CCLLC.Azure.Secrets](https://scottcolson.github.io/CCLLC.Azure.Secrets/)

###### Nuget Packages

- [Assembly - CCLLC.CDS.FieldEncryption](https://www.nuget.org/packages/CCLLC.CDS.FieldEncryption/)
- [Source Code - CCLLC.CDS.FieldEncryption.Sources](https://www.nuget.org/packages/CCLLC.CDS.FieldEncryption.Sources/)



