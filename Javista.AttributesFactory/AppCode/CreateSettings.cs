namespace Javista.AttributesFactory.AppCode
{
    internal class CreateSettings
    {
        public bool AddLookupSuffix { get; internal set; }
        public bool AddOptionSetSuffix { get; internal set; }
        public string FilePath { get; set; }
        public int LanguageCode { get; set; }
        public SolutionInfo Solution { get; set; }
        public int ThrottleInSeconds { get; internal set; }
    }
}