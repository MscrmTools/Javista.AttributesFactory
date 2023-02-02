namespace Javista.AttributesFactory.AppCode
{
    internal class ProcessResult
    {
        public string Attribute { get; set; }
        public string DisplayName { get; set; }
        public string Entity { get; set; }
        public string Message { get; set; }
        public bool Processing { get; set; }
        public bool Success { get; set; }
        public string Type { get; set; }
        public bool IsCreate { get; set; }
        public bool IsDelete { get; internal set; }
    }
}