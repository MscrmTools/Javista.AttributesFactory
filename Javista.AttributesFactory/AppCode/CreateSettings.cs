using Microsoft.Xrm.Sdk.Metadata;
using System.Collections.Generic;

namespace Javista.AttributesFactory.AppCode
{
    public class CreateSettings
    {
        public bool AddLookupSuffix { get; internal set; }
        public bool AddOptionSetSuffix { get; internal set; }
        public string FilePath { get; set; }
        public int LanguageCode { get; set; }
        public SolutionInfo Solution { get; set; }
        public int ThrottleInSeconds { get; internal set; }
        public List<EntityKeyMetadata> KeysToDelete { get; internal set; }
    }
}