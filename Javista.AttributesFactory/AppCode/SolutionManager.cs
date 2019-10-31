using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Javista.AttributesFactory.AppCode
{
    public class SolutionInfo
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

        public SolutionInfo(string name, string friendlyName, string prefix, int optionSetPrefix, Guid id,
            string version) : this(name, friendlyName, prefix, optionSetPrefix, id)
        {
            Version = version;
        }

        public Guid Id { get; }
        public int OptionSetPrefix { get; }
        public string Prefix { get; }
        public string UniqueName { get; }
        public string Version { get; }

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
                ColumnSet = new ColumnSet("uniquename", "friendlyname", "ismanaged", "version"),
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