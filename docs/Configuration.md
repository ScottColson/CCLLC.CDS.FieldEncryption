# Configuration

Specific configuration instructions for CDS Field Encryption is contained in 
on or more XML Data Resource records that contain information about the system
defaults as well as entity and field level configuration.

To provide the best long-term maintainability of a system that relies on encryption,
configuration should be segregated into separate XML data resource per entity or at
a minimum per solution domain. 

Each XML data configuration must have a _configuration_ root element. Each _configuration_ 
root can have zero or more _setting_ and _entity_ elements.

##### Setting Elements

Setting elements are parented by the _configuration_ element and implement simple name/value pairs 
```xml
<setting name="settingName" value="settingValue"/>
```
used to override configuration defaults for the following global settings:

1. Default Unmask Trigger Attribute (_unmaskTriggerAttribute_) - the attribute name used to tell the system that the operation 
wants a field to be decrypted. This global default value is used if a specific unmask attribute is
not defined as part of the entity or field definitions. If not provided, the system uses "ccllc_decrypthis" 
as the default trigger attribute name._

2. Default Field Prep Pattern (_fieldPrepPattern_) - a RegEx pattern used to remove any unwanted
formatting characters from a value before it is encrypted. This default value is used if a specific
value is not defined as part of the field definition. If not provided the system defaults to not
removing any formatting fields prior to encryption.

3. Search Trigger (_searchTrigger_) - a character used as the starting character of a search string,
usually as part of a quick find, to tell the system that the user is searching encrypted fields
for a match. If not provided the system defaults to using the '#' character for the search trigger.

If multiple setting elements with the same name are included as part of the set
of XML data resources, then the last XML data resource processed will determine the
setting value.

##### Entity Elements

Entity elements are parented by the _configuration_ element and are used to group encrypted field configuration 
information for a given entity
```xml
<entity recordName="contact"></entity>
```
where the schema name of the target entity is defined using the _recordName_ attribute. The _entity_ 
element also supports an optional _unmaskTriggerAttribute_ attribute that is used to define a 
default unmask attribute for the entity that is different than the global default value.

##### Field Elements

Field elements are parented by an _entity_ element and define which fields in a given entity that the
CDS Field Encryption Service manages. The _field_ element requires a _fieldName_ attribute with

```xml
<field fieldName="ccllc_driverlicensenumber"
       unmaskTriggerAttribute="ccllc_somefield"
       disableFieldPrep="false"
       fieldPrepPattern="[^a-zA-Z0-9]"
       matchPattern="*"
       invalidMatchMessage="Invalid DLN Number"
       fullyMaskedFormat="*********"
       partiallyMaskedViewRoles="*"
       partiallyMaskedPattern="(\S{6})(\S{3})"
       partiallyMaskedFormat="*******-$2"
       unaskedViewRoles="DLN_Operator,DLN_Supervisor"
       unaskedPattern="(\S{2})(\S{4})(\S{3})"
       unaskedFormat="$1-$2-$3"
       searchRoles="*"
       inactiveRecordSearch="false" />
``` 

the schema name of the field that is being managed by the CDS Field Encryption Service. The following additional
attributes can be set in the _field_ element to refine how the service operates against that field:

- _unmaskTriggerAttribute_ can be used to set a specific unmasking trigger field for the field that is being encrypted. When omitted, the trigger for the _entity_ element or the global trigger are used.
- _disableFieldPrep_ can be set to **true** if data in this field should not be processed for removal of formatting characters.
- _fieldPrepPattern_ can be set to override the global default prep pattern for this field.
- _matchPattern_ can be set to require the clear text field value to match a specific RegEx pattern prior to Encryption. This check is completed after any prep that removes formatting characters. This value also eliminates a field from encrypted search if the clear text search value does not match the RegEx pattern.
- _invalidMatchMessage_ can be used to create a specific error message when the clear text value does not match.
- _fullyMaskedFormat_ can be used to set the returned when a user does not have permission to see the decrypted value or when decryption was not explicitly requested. If omitted, the default of ********* is displayed.
- _partiallyMaskedViewRoles_ can be set to a comma delimited list of security roles that are allowed to view the decrypted field data in a partially masked format. Defaults to a wildcard * character if omitted.
- _partiallyMaskedPattern_ is used to define a RegEx pattern that parses the clear text value into groups for output formatting based on the pattern defined for _partiallyMaskedFormat_. If left blank no formatting will be applied the output will be decrypted clear text.
- _partiallyMaskedFormat_ is used to define the format of the partially masked output text. If left blank no formatting will be applied the output will be decrypted clear text. 
- _unmaskedViewRoles_ can be set to a comma delimited list of security roles that are allowed to view the decrypted field data as clear text. Defaults to a wildcard * character if omitted.
- _unaskedPattern_ is used to define a RegEx pattern that parses the clear text value into groups for output formatting based on the pattern defined for _unmaskedFormat_. If left blank no formatting will be applied the output will be decrypted clear text.
- _unmaskedFormat_ is used to define the formating for the clear text output. If left blank no formatting will be applied the output will be decrypted clear text without formatting. 
- _searchRoles_ can be used to limit users that can search against the encrypted field to only those specified in the comma delimited list of security roles.
- _inactiveRecordSearch_ can be set to true to include inactive records in the result of an encrypted field search against this field. Note that this is a field level option so some fields can include inactive records and others cannot.

