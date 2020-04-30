# CCLLC.CDS.FieldEncryption

Provides a set if services that can be used within a CDS Plugin to implement field level encryption 
and decryption of CDS entity data. See the [sample plugin code](https://github.com/ScottColson/CCLLC.CDS.FieldEncryption/blob/master/CCLLC.CDS.FieldEncryption.Sample/FieldEncryptionPlugin.cs)
for an example plugin implementation.

#### Features

1. Uses AES256 (aka Rijndael encryption) to encrypt and decrypt data using an encryption key stored as an Azure Key Vault Secret.
2. Configurable to support multiple entities and multiple fields within each entity.
3. Supports user triggered record query and quick find against decrypted field data using a special search trigger character.
4. Encrypts data in configured fields prior to writing the field data to the CDS data store.
5. Optional data format cleaning prior to field data encryption and reformatting prior to output to provide improved searchability when conducting an encrypted data search
6. Role based decryption authorization with optional partial masking of field data prior to display.
7. Supports implicit or explicit field decryption as part of a Retrieve or RetrieveMultiple operation.
8. Optional logging of decrypted data access by system users.

#### Key Concepts

1. Encryption is limited to fields that represent string data. Encrypted strings are larger than their corresponding clear text so field sizes should be increased.
2. The key used to encrypt and decrypt field level data is stored as an Azure Key Vault Secret which is retrieved and cached for improved performance.
3. Configuration information is required for each field that the system will manage. All configuration data is stored as XML in one or more CDS XML data resource records.
4. Explicit decryption is dependent on a trigger attribute that added to the column set of a Retrieve or RetrieveMultiple request. This trigger attribute should not be an attribute used for any actual data storage.
5. Decryption only occurs if the user is authorized to view the data as clear text or a partially masked result or as clear text.
6. Users can search for records with an encrypted value by preceding their search text with a # character. This character is configurable.

##### Azure Key Vault

The Field Encryption Service uses an Azure Key Vault to store the encryption key so that
control of the key can be segregated from administration of the CDS application that uses
field encryption. 

To connect to the Azure Key Vault, the service requires the following information:
- Azure Tenant Id
- Client/Application Id
- Client/Application Secret.
- The Azure Key Vault Name.

The Application Id and secret are created through Azure AD App registrations. Note that 
this registered app must granted the right to List and Get secrets in the vault.

This access data is stored in the following CDS Environment Variables:

- CCLLC.Azure.Secrets.TenantId
- CCLLC.Azure.Secrets.ClientId
- CCLLC.Azure.Secrets.ClientSecret
- CCLLC.Azure.Secrets.VaultName

##### Role Based Access

By default, all users are allowed to view decrypted data and to search against encrypted
fields. 

However, security role assignments can be used to restrict user access for viewing and
for searching at the field level. The ability to view data can be set to either view
the clear text data or with some additional configuration, users can be restricted
to seeing a partially masked version of the clear text data. If a user is granted access
to both clear text and partially masked data, then clear text data will be displayed.

Additionally, the required security roles to search against an encrypted field can be
set at a field level.

See [full documentation on configuration](Configuration.md) for additional information
on configuring role based access.

##### Explicit Decryption

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

##### Encrypted Search

The ability to search for a record based on the value in an encrypted field is made possible
by the use of an encryption algorithm that is predictive, i.e. given the same key and the same
clear text, the algorithm will always generate the same encrypted value. 

The Field Encryption Service uses this predictive nature to generate a special search query
when it determines that the user/developer is searching against an encrypted string.

The service makes this determination be examining the criteria of the QueryExpression passed
to the CDS platform. If the QueryExperession contains a condition with a _Like_
operator AND the value associated with that condition starts with encrypted search trigger 
(# by default) and ends with a wildcard (%) character, then the service interprets the 
QueryExpression as a request for encrypted search and it replaces the QueryExpression criteria
with a new criteria based on conditions where fields that are configured for encryption are
_Equal_ to the encrypted search value.

The mechanism that creates the new filter criteria will only search against fields where
the user has the required search role. 

##### Configuration    

CDS field encryption is configured using XML data resources with configuration 
information similar to the following example that directs the service to manage encryption for
the ccllc_driverlicensenumber field of the contact entity.
```xml
<configuration>
    <entity recordType="contact">
        <field fieldName="ccllc_driverlicensenumber"/>
    </entity>
</configuration>
```
This simplified configuration example relies on default values and does not implement role based
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

##### Dependencies

- [CCLLC.CDS.Sdk](https://scottcolson.github.io/CCLLCCodeLibraries/CCLLC.CDS.Sdk.html)
- [CCLLC.Azure.Secrets](https://scottcolson.github.io/CCLLC.Azure.Secrets/)

##### Nuget Packages

- [Assembly - CCLLC.CDS.FieldEncryption](https://www.nuget.org/packages/CCLLC.CDS.FieldEncryption/)
- [Source Code - CCLLC.CDS.FieldEncryption.Sources](https://www.nuget.org/packages/CCLLC.CDS.FieldEncryption.Sources/)



