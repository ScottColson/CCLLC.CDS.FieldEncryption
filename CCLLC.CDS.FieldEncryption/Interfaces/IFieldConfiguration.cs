using System;
using System.Collections.Generic;
using System.Text;

namespace CCLLC.CDS.FieldEncryption
{
    public interface IFieldConfiguration
    {
        /// <summary>
        /// The logical name of the field that requires encryption. 
        /// </summary>
        string FieldName { get; }

        string UnmaskTriggerAttributeName { get; }

        /// <summary>
        /// Override default field preparation prior to encryption for this field.
        /// </summary>
        bool DisableFieldPrep { get; }

        /// <summary>
        /// Override the default field global preparation pattern for this field. When
        /// null or empty, the <see cref="IServiceConfiguration.GlobalFieldPrepPattern"/> is
        /// used.
        /// </summary>
        string FieldPrepPattern { get; }

        
        string FullyMaskedFormat { get; }

        string[] PartiallyMaskedViewRoles { get; }
        string PartiallyMaskedFormat { get; }
        string PartiallyMaskedPattern { get; }

        string[] UnmaskedViewRoles { get; }
        string UnmaskedFormat { get; }
        string UnmaskedPattern { get;  }


        
        
        
        string MatchPattern { get;  }
        string InvalidMatchMessage { get;  }
        string[] SearchRoles { get;  }
        bool AllowInactiveRecordSearch { get;  }
        

    }
}
