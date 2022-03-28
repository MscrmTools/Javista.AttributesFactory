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

        public static Guid CreateEntity(string schemaName, IOrganizationService service)
        {
            CreateEntityRequest createrequest = new CreateEntityRequest
            {

                //Define the entity
                Entity = new EntityMetadata
                {
                    SchemaName = schemaName,
                    DisplayName = new Label(schemaName, 1033),
                    DisplayCollectionName = new Label(schemaName, 1033),
                    OwnershipType = OwnershipTypes.UserOwned,
                    IsActivity = false,
                },

                // Define the primary attribute for the entity
                PrimaryAttribute = new StringAttributeMetadata
                {
                    SchemaName = schemaName.Split(',')[0] + "_name",
                    RequiredLevel = new AttributeRequiredLevelManagedProperty(AttributeRequiredLevel.None),
                    MaxLength = 100,
                    FormatName = StringFormatName.Text,
                    DisplayName = new Label(schemaName, 1033)
                }

            };

            var response = (CreateEntityResponse)service.Execute(createrequest);
            return response.EntityId;
        }

        public static bool IsEntityExist(string schemaName, IOrganizationService service)
        {
            MetadataFilterExpression entityFilter = new MetadataFilterExpression();
            entityFilter.Conditions.Add(new MetadataConditionExpression("SchemaName", MetadataConditionOperator.Equals, schemaName));

            RetrieveMetadataChangesRequest req = new RetrieveMetadataChangesRequest()
            {
                Query = new EntityQueryExpression()
                {
                    Criteria = entityFilter
                }
            };

            RetrieveMetadataChangesResponse resp = service.Execute(req) as RetrieveMetadataChangesResponse;
            return resp.Results.Count == 1;
        }
    }
}