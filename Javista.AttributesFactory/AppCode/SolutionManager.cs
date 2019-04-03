using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Javista.AttributesFactory.AppCode
{
    internal class SolutionInfo
    {
        private readonly string friendlyName;

        public SolutionInfo(string name, string friendlyName, string prefix, int optionSetPrefix, Guid id)
        {
            UniqueName = name;
            Id = id;
            Prefix = $"{prefix}_";
            OptionSetPrefix = optionSetPrefix;
            this.friendlyName = friendlyName;
        }

        public Guid Id { get; }
        public string UniqueName { get; }

        public string Prefix { get; }

        public int OptionSetPrefix { get; }

        public override string ToString()
        {
            return friendlyName;
        }
    }

    internal class SolutionManager
    {
        public static List<SolutionInfo> GetSolutions(IOrganizationService service)
        {
            return service.RetrieveMultiple(new QueryExpression("solution")
            {
                ColumnSet = new ColumnSet("uniquename", "friendlyname"),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression("ismanaged", ConditionOperator.Equal, false),
                        new ConditionExpression("isvisible", ConditionOperator.Equal, true)
                    }
                },
                LinkEntities =
                {
                    new LinkEntity
                    {
                        LinkFromEntityName = "solution",
                        LinkFromAttributeName = "publisherid",
                        LinkToAttributeName = "publisherid",
                        LinkToEntityName = "publisher",
                        EntityAlias = "publisher",
                        Columns = new ColumnSet("customizationprefix","customizationoptionvalueprefix")
                    }
                }
            }).Entities.Select(e => new SolutionInfo(e.GetAttributeValue<string>("uniquename"),
                e.GetAttributeValue<string>("friendlyname"),
                e.GetAttributeValue<AliasedValue>("publisher.customizationprefix").Value.ToString(),
                (int)e.GetAttributeValue<AliasedValue>("publisher.customizationoptionvalueprefix").Value,
                e.Id)).ToList();
        }
    }
}