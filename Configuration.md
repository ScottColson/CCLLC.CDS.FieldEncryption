# Configuration

Specific configuration instructions for CDS Field Encryption is contained in 
on or more XML Data Resource records that contain information about the system
defaults as well as entity and field level configuration.

To provide the best long-term maintainability of a system that relies on encryption,
configuration should be segregated into separate XML data resource per entity or at
a minimum per solution domain. 

Each XML data configuration must have a _configuration_ root element. Each _configuration_ 
root can have zero or more _setting_ and _entity_ elements.

###### Setting Elements

Setting implement simple name/value pair inputs such as 
```xml
<setting name="settingName" value="settingValue"/>
```
that are used to override configuration defaults for the following global settings:

1. Default Unmask Trigger Attribute (_unmaskTriggerAttribute_) - the attribute name used to tell the system that the operation 
wants a field to be decrypted. This default value is used if a specific unmask attribute is
not defined as part of the field definition. If not provided, the system uses "ccllc_decrypthis" 
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

###### Entity Elements

TBD



