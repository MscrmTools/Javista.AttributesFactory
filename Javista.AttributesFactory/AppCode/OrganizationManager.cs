using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Javista.AttributesFactory.AppCode
{
    internal class OrganizationManager
    {
        public static int GetLanguageCode(IOrganizationService service)
        {
            return service.RetrieveMultiple(new QueryExpression("organization")
            {
                ColumnSet = new ColumnSet("languagecode"),
            }).Entities.First().GetAttributeValue<int>("languagecode");
        }
    }
}