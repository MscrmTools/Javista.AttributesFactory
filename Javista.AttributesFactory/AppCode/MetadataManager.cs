using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Metadata.Query;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Javista.AttributesFactory.AppCode
{
    public class MetadataManager
    {
        public static List<EntityMetadata> GetEntities(Guid solutionid, IOrganizationService service)
        {
            var components = service.RetrieveMultiple(new QueryExpression("solutioncomponent")
            {
                ColumnSet = new ColumnSet("objectid"),
                NoLock = true,
                Criteria = new FilterExpression
                {
                    Conditions =
                        {
                            new ConditionExpression("solutionid", ConditionOperator.Equal,solutionid),
                            new ConditionExpression("componenttype", ConditionOperator.Equal, 1)
                        }
                }
            }).Entities;

            var list = components.Select(component => component.GetAttributeValue<Guid>("objectid"))
                .ToList();

            if (list.Count > 0)
            {
                EntityQueryExpression entityQueryExpression = new EntityQueryExpression
                {
                    Criteria = new MetadataFilterExpression(LogicalOperator.Or),
                    Properties = new MetadataPropertiesExpression
                    {
                        AllProperties = false,
                        PropertyNames = { "DisplayName", "LogicalName" }
                    }
                };

                list.ForEach(id =>
                {
                    entityQueryExpression.Criteria.Conditions.Add(new MetadataConditionExpression("MetadataId", MetadataConditionOperator.Equals, id));
                });

                RetrieveMetadataChangesRequest retrieveMetadataChangesRequest = new RetrieveMetadataChangesRequest
                {
                    Query = entityQueryExpression,
                    ClientVersionStamp = null
                };

                var response = (RetrieveMetadataChangesResponse)service.Execute(retrieveMetadataChangesRequest);

                return response.EntityMetadata.ToList();
            }

            return new List<EntityMetadata>();
        }
    }
}