using Microsoft.Xrm.Sdk;
using System;

namespace Javista.AttributesFactory.AppCode
{
    internal class MdAInfo
    {
        private Entity e;

        public MdAInfo(Entity e)
        {
            this.e = e;

            if (e.GetAttributeValue<string>("name").Length > (MaxLengthValue?.Length ?? 0))
            {
                MaxLengthValue = e.GetAttributeValue<string>("name");
            }
        }

        public static string MaxLengthValue { get; set; }
        public Guid Id => e.Id;

        public override string ToString()
        {
            return e.GetAttributeValue<string>("name");
        }
    }
}