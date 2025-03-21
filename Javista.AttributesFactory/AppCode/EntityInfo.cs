using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Metadata.Query;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Javista.AttributesFactory.AppCode
{
    internal class EntityInfo
    {
        public EntityInfo(string name, IOrganizationService service)
        {
            Name = name;
            GetMetadata(name, service);
        }

        public List<AttributeMetadata> Attributes { get; private set; }
        public List<EntityKeyMetadata> Keys { get; private set; }
        public string Name { get; }

        private void GetMetadata(string name, IOrganizationService service)
        {
            EntityQueryExpression entityQueryExpression = new EntityQueryExpression
            {
                // Récupération de l'entité spécifiée
                Criteria = new MetadataFilterExpression
                {
                    Conditions =
                    {
                        new MetadataConditionExpression("LogicalName", MetadataConditionOperator.Equals, name)
                    }
                },
                // Sans propriétés d'entité
                Properties = new MetadataPropertiesExpression
                {
                    AllProperties = false,
                    PropertyNames = { "Attributes", "Keys" }
                },
                AttributeQuery = new AttributeQueryExpression
                {
                    // Avec uniquement les données d'OptionSet
                    Properties = new MetadataPropertiesExpression
                    {
                        AllProperties = false,
                        PropertyNames = { "AttributeType", "AttributeTypeName", "IsCustomAttribute", "LogicalName", "MetadataId", "SchemaName", "OptionSet", "Description", "DisplayName" }
                    }
                }
            };

            RetrieveMetadataChangesRequest retrieveMetadataChangesRequest = new RetrieveMetadataChangesRequest
            {
                Query = entityQueryExpression,
                ClientVersionStamp = null
            };

            var response = (RetrieveMetadataChangesResponse)service.Execute(retrieveMetadataChangesRequest);
            var emd = response.EntityMetadata.FirstOrDefault();

            if (emd == null)
            {
                throw new Exception($"Entity with logical name {name} does not exist");
            }

            Attributes = emd.Attributes.ToList();
            Keys = emd.Keys.ToList();
        }
    }
}