using McTools.Xrm.Connection;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Threading;

namespace Javista.AttributesFactory.AppCode
{
    internal class MetadataUpsertManager
    {
        private const int AuditCellIndex = 10;
        private const int DescriptionCellIndex = 6;
        private const int DisplayNameCellIndex = 2;
        private const int EntityCellIndex = 5;
        private const int FieldTypeCellIndex = 11;
        private const int PropertiesFirstCellIndex = 12;
        private const int RequiredLevelCellIndex = 7;
        private const int SchemaNameCellIndex = 3;
        private const int SearchEnabledCellIndex = 8;
        private const int SecuredCellIndex = 9;
        private const int TypeCellIndex = 4;
        private readonly int majorVersion;
        private readonly IOrganizationService service;
        private readonly CreateSettings settings;

        public MetadataUpsertManager(CreateSettings settings, IOrganizationService service, int majorVersion)
        {
            this.service = service;
            this.settings = settings;
            this.majorVersion = majorVersion;
        }

        public List<string> GetEntities()
        {
            var entities = new List<string>();

            byte[] file = File.ReadAllBytes(settings.FilePath);
            using (MemoryStream ms = new MemoryStream(file))
            using (ExcelPackage package = new ExcelPackage(ms))
            {
                ExcelWorksheet workSheet = package.Workbook.Worksheets.First();

                int index = 0;
                for (int i = 3; i <= workSheet.Dimension.End.Row; i++)
                {
                    if (string.IsNullOrEmpty(workSheet.GetValue<string>(i, TypeCellIndex))
                    || workSheet.GetValue<string>(i, 1) == "Ignore")
                    {
                        continue;
                    }

                    index++;

                    var entity = workSheet.GetValue<string>(i, EntityCellIndex);
                    if (!entities.Contains(entity)) entities.Add(entity);
                }
            }

            return entities;
        }

        public void Process(BackgroundWorker worker, ConnectionDetail detail)
        {
            var eiCache = new List<EntityInfo>();

            byte[] file = File.ReadAllBytes(settings.FilePath);
            using (MemoryStream ms = new MemoryStream(file))
            using (ExcelPackage package = new ExcelPackage(ms))
            {
                ExcelWorksheet workSheet = package.Workbook.Worksheets.First();
                int percent = 0;
                int index = 0;
                for (int i = 3; i <= workSheet.Dimension.End.Row; i++)
                {
                    if (string.IsNullOrEmpty(workSheet.GetValue<string>(i, TypeCellIndex))
                    || workSheet.GetValue<string>(i, 1) == "Ignore")
                    {
                        continue;
                    }

                    if (worker.CancellationPending)
                    {
                        return;
                    }

                    index++;

                    var info = new ProcessResult
                    {
                        DisplayName = workSheet.GetValue<string>(i, DisplayNameCellIndex),
                        Attribute = workSheet.GetValue<string>(i, SchemaNameCellIndex),
                        Type = workSheet.GetValue<string>(i, TypeCellIndex),
                        Entity = workSheet.GetValue<string>(i, EntityCellIndex).ToLower(),
                        Processing = true,
                    };

                    if ((info.Type == "Customer" || info.Type == "Lookup") && settings.AddLookupSuffix && !info.Attribute.EndsWith("Id"))
                    {
                        info.Attribute = $"{info.Attribute}Id";
                    }
                    if ((info.Type == "Choice" || info.Type == "Choices") && settings.AddOptionSetSuffix && !info.Attribute.ToLower().EndsWith("code"))
                    {
                        info.Attribute = $"{info.Attribute}Code";
                    }

                    worker.ReportProgress(percent, info);
                    percent = index * 100 / (workSheet.Dimension.End.Row - 2);

                    try
                    {
                        AttributeMetadata amd = null;
                        var fakeAmd = GetFakeAmd(info, workSheet, i);

                        // Check validity for an Update
                        var ei = eiCache.FirstOrDefault(e => e.Name == info.Entity);
                        if (ei == null)
                        {
                            ei = new EntityInfo(info.Entity, service);
                            eiCache.Add(ei);
                        }

                        var existingAttribute = ei.Attributes.FirstOrDefault(a => a.LogicalName == info.Attribute.ToLower());
                        if (existingAttribute != null)
                        {
                            fakeAmd.MetadataId = existingAttribute.MetadataId;
                        }

                        var type = workSheet.GetValue<string>(i, TypeCellIndex);
                        switch (type)
                        {
                            case "Single line of text":

                                if (!string.IsNullOrEmpty(workSheet.GetValue<string>(i, PropertiesFirstCellIndex + 2)) && detail.OrganizationMajorVersion < 9)
                                {
                                    throw new Exception(
                                        "Autonumber attributes can only be created in a version 9 or above of Microsoft Dynamics 365");
                                }

                                amd = CreateStringAttribute(workSheet, i, PropertiesFirstCellIndex);
                                break;

                            case "Choice":
                            case "OptionSet":
                                amd = CreateOptionsetAttribute(workSheet, i, PropertiesFirstCellIndex + 4, false, info.DisplayName, info.Attribute, info.Entity, fakeAmd.Description?.LocalizedLabels[0]?.Label, (existingAttribute as PicklistAttributeMetadata)?.OptionSet, existingAttribute != null);
                                break;

                            case "Multiselect OptionSet":
                            case "Choices":

                                if (detail.OrganizationMajorVersion < 9)
                                {
                                    throw new Exception(
                                        "Choices can only be created in a version 9 or above of Microsoft Dynamics 365");
                                }

                                amd = CreateOptionsetAttribute(workSheet, i, PropertiesFirstCellIndex + 4, true, info.DisplayName, info.Attribute, info.Entity, fakeAmd.Description?.LocalizedLabels[0]?.Label, (existingAttribute as PicklistAttributeMetadata)?.OptionSet, existingAttribute != null);
                                break;

                            case "Two options":
                                amd = CreateBooleanAttribute(workSheet, i, PropertiesFirstCellIndex + 8);
                                break;

                            case "Whole number":
                                amd = CreateNumberAttribute(workSheet, i, PropertiesFirstCellIndex + 11);
                                break;

                            case "Float number":
                                amd = CreateFloatAttribute(workSheet, i, PropertiesFirstCellIndex + 15);
                                break;

                            case "Decimal number":
                                amd = CreateDecimalAttribute(workSheet, i, PropertiesFirstCellIndex + 19);
                                break;

                            case "Money":
                                amd = CreateMoneyAttribute(workSheet, i, PropertiesFirstCellIndex + 23);
                                break;

                            case "Multiple lines of text":
                                amd = CreateMemoAttribute(workSheet, i, PropertiesFirstCellIndex);
                                break;

                            case "Date and time":
                                amd = CreateDateTimeAttribute(workSheet, i, PropertiesFirstCellIndex + 27);
                                break;

                            case "Lookup":
                                amd = CreateLookupAttribute(workSheet, i, PropertiesFirstCellIndex + 30, fakeAmd, info, !fakeAmd.MetadataId.HasValue);
                                break;

                            case "Customer":
                                amd = CreateCustomerAttribute(workSheet, i, PropertiesFirstCellIndex + 31, fakeAmd, existingAttribute, info, !fakeAmd.MetadataId.HasValue);
                                break;

                            case "File":
                                amd = CreateFileAttribute(workSheet, i, PropertiesFirstCellIndex + 45);
                                break;

                            case "Image":
                                amd = CreateImageAttribute(workSheet, i, PropertiesFirstCellIndex + 47);
                                break;
                        }

                        if (amd == null)
                        {
                            info.Success = true;
                            info.Processing = false;
                            worker.ReportProgress(percent, info);
                            continue;
                        }

                        if (existingAttribute != null)
                        {
                            if (existingAttribute.GetType() != amd.GetType())
                            {
                                throw new Exception(
                                    @"Attribute in Excel file is not of same type as existing attribute in organization");
                            }
                        }

                        amd.DisplayName = fakeAmd.DisplayName;
                        amd.SchemaName = fakeAmd.SchemaName;
                        amd.LogicalName = fakeAmd.LogicalName;
                        amd.IsValidForAdvancedFind = fakeAmd.IsValidForAdvancedFind;
                        amd.IsSecured = fakeAmd.IsSecured;
                        amd.IsAuditEnabled = fakeAmd.IsAuditEnabled;
                        amd.SourceType = fakeAmd.SourceType;
                        amd.MetadataId = fakeAmd.MetadataId;
                        amd.RequiredLevel = fakeAmd.RequiredLevel;
                        if (fakeAmd.Description != null)
                        {
                            amd.Description = fakeAmd.Description;
                        }

                        info.Attribute = amd.SchemaName;

                        OrganizationRequest request;
                        if (amd.MetadataId.HasValue)
                        {
                            request = new UpdateAttributeRequest
                            {
                                Attribute = amd,
                                EntityName = info.Entity,
                                SolutionUniqueName = settings.Solution.UniqueName,
                                MergeLabels = true
                            };

                            info.IsCreate = false;
                        }
                        else
                        {
                            request = new CreateAttributeRequest
                            {
                                Attribute = amd,
                                EntityName = info.Entity,
                                SolutionUniqueName = settings.Solution.UniqueName
                            };

                            info.IsCreate = true;
                        }

                        try
                        {
                            service.Execute(request);
                            info.Success = true;
                            info.Processing = false;
                            worker.ReportProgress(percent, info);
                        }
                        catch (FaultException<OrganizationServiceFault> error)
                        {
                            // Special handle for file attribute as they are not returned by the query
                            if (info.IsCreate && error.Detail.ErrorCode == -2147192813)
                            {
                                request = new UpdateAttributeRequest
                                {
                                    Attribute = amd,
                                    EntityName = info.Entity,
                                    SolutionUniqueName = settings.Solution.UniqueName,
                                    MergeLabels = true
                                };

                                info.IsCreate = false;

                                service.Execute(request);
                                info.Success = true;
                                info.Processing = false;
                                worker.ReportProgress(percent, info);
                            }
                            else
                            {
                                info.Success = false;
                                info.Processing = false;
                                info.Message = error.Message;
                                worker.ReportProgress(percent, info);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        info.Success = false;
                        info.Processing = false;
                        info.Message = e.Message;
                        worker.ReportProgress(percent, info);
                    }
                    finally
                    {
                        if (settings.ThrottleInSeconds != 0)
                        {
                            Thread.Sleep(settings.ThrottleInSeconds * 1000);
                        }
                    }
                }
            }
        }

        private void ApplyOptionsUpdate(OptionSetMetadata eomd, OptionSetMetadata omd, string entity = null, string attribute = null)
        {
            if (eomd == null)
            {
                return;
            }

            var newOptions = omd.Options.Where(o => !eomd.Options.Select(o2 => o2.Value ?? -1).Contains(o.Value ?? 0));
            foreach (var newOption in newOptions)
            {
                service.Execute(new InsertOptionValueRequest
                {
                    EntityLogicalName = entity,
                    AttributeLogicalName = attribute?.ToLower(),
                    Label = newOption.Label,
                    OptionSetName = entity == null ? omd.Name : null,
                    Value = newOption.Value,
                    SolutionUniqueName = settings.Solution.UniqueName
                });
            }

            var existingOptions =
                omd.Options.Where(o => eomd.Options.Select(o2 => o2.Value ?? -1).Contains(o.Value ?? 0));
            foreach (var existingOption in existingOptions)
            {
                service.Execute(new UpdateOptionValueRequest
                {
                    EntityLogicalName = entity,
                    AttributeLogicalName = attribute?.ToLower(),
                    Label = existingOption.Label,
                    OptionSetName = entity == null ? omd.Name : null,
                    Value = existingOption.Value ?? 0,
                    SolutionUniqueName = settings.Solution.UniqueName
                });
            }

            var oldOptions = eomd.Options.Where(o => !omd.Options.Select(o2 => o2.Value ?? -1)
                .Contains(o.Value ?? 0));
            foreach (var oldOption in oldOptions)
            {
                service.Execute(new DeleteOptionValueRequest
                {
                    EntityLogicalName = entity,
                    AttributeLogicalName = attribute?.ToLower(),
                    OptionSetName = entity == null ? omd.Name : null,
                    Value = oldOption.Value ?? 0,
                    SolutionUniqueName = settings.Solution.UniqueName
                });
            }

            service.Execute(new OrderOptionRequest
            {
                EntityLogicalName = entity,
                AttributeLogicalName = attribute?.ToLower(),
                OptionSetName = entity == null ? omd.Name : null,
                Values = omd.Options.Select(o => o.Value ?? 0).ToArray(),
                SolutionUniqueName = settings.Solution.UniqueName
            });
        }

        private AttributeMetadata CreateBooleanAttribute(ExcelWorksheet sheet, int rowIndex, int startCell)
        {
            var amd = new BooleanAttributeMetadata
            {
                OptionSet = new BooleanOptionSetMetadata(),
            };
            var stringValues = sheet.GetValue<string>(rowIndex, startCell);
            if (string.IsNullOrEmpty(stringValues))
            {
                throw new Exception("Two options values cannot be null");
            }

            foreach (var optionRow in stringValues.Split('\n'))
            {
                var parts = optionRow.Split(':');
                if (parts[0] == "0")
                {
                    amd.OptionSet.FalseOption = new OptionMetadata(new Label(parts[1], settings.LanguageCode), int.Parse(parts[0]));
                }
                else if (parts[0] == "1")
                {
                    amd.OptionSet.TrueOption = new OptionMetadata(new Label(parts[1], settings.LanguageCode), int.Parse(parts[0]));
                }
            }

            if (!string.IsNullOrEmpty(sheet.GetValue<string>(rowIndex, startCell + 1)))
            {
                amd.DefaultValue = sheet.GetValue<string>(rowIndex, startCell + 1) == "True";
            }

            return amd;
        }

        private AttributeMetadata CreateCustomerAttribute(ExcelWorksheet sheet, int rowIndex, int startCell, AttributeMetadata fakeAmd, AttributeMetadata existingAttribute, ProcessResult info, bool isCreate)
        {
            var amc = new AssociatedMenuConfiguration
            {
                Order = sheet.GetValue<int>(rowIndex, startCell + 5),
                Group = new AssociatedMenuGroup?()
            };

            switch (sheet.GetValue<string>(rowIndex, startCell + 4))
            {
                case "Details":
                    amc.Group = AssociatedMenuGroup.Details;
                    break;

                case "Sales":
                    amc.Group = AssociatedMenuGroup.Sales;
                    break;

                case "Service":
                    amc.Group = AssociatedMenuGroup.Service;
                    break;

                case "Marketing":
                    amc.Group = AssociatedMenuGroup.Marketing;
                    break;
            }

            switch (sheet.GetValue<string>(rowIndex, startCell + 2))
            {
                case "Do not display":
                    amc.Behavior = AssociatedMenuBehavior.DoNotDisplay;
                    break;

                case "Custom label":
                    amc.Behavior = AssociatedMenuBehavior.UseLabel;

                    if (!string.IsNullOrEmpty(sheet.GetValue<string>(rowIndex, startCell + 3)))
                    {
                        amc.Label = new Label(sheet.GetValue<string>(rowIndex, startCell + 3), settings.LanguageCode);
                    }
                    else
                    {
                        throw new Exception("If display behavior is \"Custom Label\", the label must be specified");
                    }

                    break;

                default:
                    amc.Behavior = AssociatedMenuBehavior.UseCollectionName;
                    break;
            }

            var cc = new CascadeConfiguration();
            switch (sheet.GetValue<string>(rowIndex, startCell + 6))
            {
                case "Parental":
                    cc.Assign = CascadeType.Cascade;
                    cc.Delete = CascadeType.Cascade;
                    cc.Merge = CascadeType.Cascade;
                    cc.Reparent = CascadeType.Cascade;
                    cc.RollupView = CascadeType.NoCascade;
                    cc.Share = CascadeType.Cascade;
                    cc.Unshare = CascadeType.Cascade;
                    break;

                case "Referential, restrict delete":
                    cc.Assign = CascadeType.NoCascade;
                    cc.Delete = CascadeType.Restrict;
                    cc.Merge = CascadeType.NoCascade;
                    cc.Reparent = CascadeType.NoCascade;
                    cc.RollupView = CascadeType.NoCascade;
                    cc.Share = CascadeType.NoCascade;
                    cc.Unshare = CascadeType.NoCascade;
                    break;

                case "Custom":
                    cc.RollupView = CascadeType.NoCascade;
                    cc.Assign = GetCascade(sheet.GetValue<string>(rowIndex, startCell + 7));
                    cc.Share = GetCascade(sheet.GetValue<string>(rowIndex, startCell + 8));
                    cc.Unshare = GetCascade(sheet.GetValue<string>(rowIndex, startCell + 9));
                    cc.Reparent = GetCascade(sheet.GetValue<string>(rowIndex, startCell + 10));
                    cc.Delete = GetCascade(sheet.GetValue<string>(rowIndex, startCell + 11));
                    cc.Merge = GetCascade(sheet.GetValue<string>(rowIndex, startCell + 12));
                    break;

                default:
                    cc.Assign = CascadeType.NoCascade;
                    cc.Delete = CascadeType.RemoveLink;
                    cc.Merge = CascadeType.NoCascade;
                    cc.Reparent = CascadeType.NoCascade;
                    cc.RollupView = CascadeType.NoCascade;
                    cc.Share = CascadeType.NoCascade;
                    cc.Unshare = CascadeType.NoCascade;
                    break;
            }

            AttributeMetadata lookup;

            if (existingAttribute != null)
            {
                lookup = existingAttribute;
                lookup.DisplayName = fakeAmd.DisplayName;
                lookup.IsValidForAdvancedFind = fakeAmd.IsValidForAdvancedFind;
                lookup.RequiredLevel = fakeAmd.RequiredLevel;
                lookup.IsSecured = fakeAmd.IsSecured;
                lookup.IsAuditEnabled = fakeAmd.IsAuditEnabled;
            }
            else
            {
                lookup = new LookupAttributeMetadata
                {
                    DisplayName = fakeAmd.DisplayName,
                    SchemaName = fakeAmd.SchemaName,
                    LogicalName = fakeAmd.LogicalName,
                    IsValidForAdvancedFind = fakeAmd.IsValidForAdvancedFind,
                    IsSecured = fakeAmd.IsSecured,
                    RequiredLevel = fakeAmd.RequiredLevel,
                    IsAuditEnabled = fakeAmd.IsAuditEnabled,
                    Targets = new[] { "account", "contact" }
                };
            }

            if (fakeAmd.Description != null)
            {
                lookup.Description = fakeAmd.Description;
            }

            if (settings.AddLookupSuffix && !lookup.SchemaName.ToLower().EndsWith("id"))
            {
                lookup.SchemaName = $"{lookup.SchemaName}Id";
                lookup.LogicalName = lookup.SchemaName.ToLower();
            }

            var accountRelationship = new OneToManyRelationshipMetadata
            {
                IsValidForAdvancedFind = sheet.GetValue<string>(rowIndex, startCell) == "Yes",
                SchemaName = $"{settings.Solution.Prefix}{info.Entity}_account_{lookup.SchemaName}",
                AssociatedMenuConfiguration = amc,
                CascadeConfiguration = cc,
                IsHierarchical = false,
                ReferencedEntity = "account",
                ReferencingEntity = info.Entity,
                SecurityTypes = SecurityTypes.Append,
            };

            if (accountRelationship.AssociatedMenuConfiguration.Label?.LocalizedLabels?.FirstOrDefault()?.Label
                    .IndexOf("\n", StringComparison.Ordinal) > 0)
            {
                accountRelationship.AssociatedMenuConfiguration.Label.LocalizedLabels[0].Label =
                    accountRelationship.AssociatedMenuConfiguration.Label.LocalizedLabels[0].Label.Split('\n')[0];
            }

            var contactRelationship = new OneToManyRelationshipMetadata
            {
                IsValidForAdvancedFind = sheet.GetValue<string>(rowIndex, startCell) == "Yes",
                SchemaName = $"{settings.Solution.Prefix}{info.Entity}_contact_{lookup.SchemaName}",
                AssociatedMenuConfiguration = amc,
                CascadeConfiguration = cc,
                IsHierarchical = false,
                ReferencedEntity = "contact",
                ReferencingEntity = info.Entity,
                SecurityTypes = SecurityTypes.Append,
            };

            if (contactRelationship.AssociatedMenuConfiguration.Label?.LocalizedLabels?.FirstOrDefault()?.Label
                    .IndexOf("\n", StringComparison.Ordinal) > 0)
            {
                contactRelationship.AssociatedMenuConfiguration.Label.LocalizedLabels[0].Label =
                    contactRelationship.AssociatedMenuConfiguration.Label.LocalizedLabels[0].Label.Split('\n')[1];
            }

            if (isCreate)
            {
                service.Execute(new CreateCustomerRelationshipsRequest
                {
                    OneToManyRelationships = new[] { accountRelationship, contactRelationship },
                    Lookup = (LookupAttributeMetadata)lookup,
                    SolutionUniqueName = settings.Solution.UniqueName
                });

                info.IsCreate = true;

                return null;
            }

            service.Execute(new UpdateRelationshipRequest
            {
                Relationship = accountRelationship,
                SolutionUniqueName = settings.Solution.UniqueName,
                MergeLabels = true
            });

            service.Execute(new UpdateRelationshipRequest
            {
                Relationship = contactRelationship,
                SolutionUniqueName = settings.Solution.UniqueName,
                MergeLabels = true
            });

            return lookup;
        }

        private AttributeMetadata CreateDateTimeAttribute(ExcelWorksheet sheet, int rowIndex, int startCell)
        {
            var amd = new DateTimeAttributeMetadata();

            switch (sheet.GetValue<string>(rowIndex, startCell))
            {
                case "User local time":
                    amd.DateTimeBehavior = DateTimeBehavior.UserLocal;
                    break;

                case "Date only":
                    amd.DateTimeBehavior = DateTimeBehavior.DateOnly;
                    break;

                case "Timezone independent":
                    amd.DateTimeBehavior = DateTimeBehavior.TimeZoneIndependent;
                    break;
            }

            switch (sheet.GetValue<string>(rowIndex, startCell + 1))
            {
                case "Date only":
                    amd.Format = DateTimeFormat.DateOnly;
                    break;

                case "Date and time":
                    amd.Format = DateTimeFormat.DateAndTime;
                    break;
            }

            return amd;
        }

        private AttributeMetadata CreateDecimalAttribute(ExcelWorksheet sheet, int rowIndex, int startCell)
        {
            var damd = new DecimalAttributeMetadata
            {
                Precision = sheet.GetValue<int>(rowIndex, startCell),
                MinValue = sheet.GetValue<decimal>(rowIndex, startCell + 1),
                MaxValue = sheet.GetValue<decimal>(rowIndex, startCell + 2)
            };

            return damd;
        }

        private AttributeMetadata CreateFileAttribute(ExcelWorksheet sheet, int rowIndex, int startCell)
        {
            var famd = new FileAttributeMetadata
            {
                MaxSizeInKB = sheet.GetValue<int>(rowIndex, startCell),
            };

            return famd;
        }

        private AttributeMetadata CreateFloatAttribute(ExcelWorksheet sheet, int rowIndex, int startCell)
        {
            var famd = new DoubleAttributeMetadata
            {
                Precision = sheet.GetValue<int>(rowIndex, startCell),
                MinValue = sheet.GetValue<double>(rowIndex, startCell + 1),
                MaxValue = sheet.GetValue<double>(rowIndex, startCell + 2)
            };

            return famd;
        }

        private AttributeMetadata CreateImageAttribute(ExcelWorksheet sheet, int rowIndex, int startCell)
        {
            var iamd = new ImageAttributeMetadata
            {
                MaxSizeInKB = sheet.GetValue<int>(rowIndex, startCell),
                CanStoreFullImage = sheet.GetValue<string>(rowIndex, startCell + 1) == "True",
                IsPrimaryImage = sheet.GetValue<string>(rowIndex, startCell + 2) == "True"
            };

            return iamd;
        }

        private AttributeMetadata CreateLookupAttribute(ExcelWorksheet sheet, int rowIndex, int startCell, AttributeMetadata fakeAmd, ProcessResult info, bool isCreate)
        {
            var amc = new AssociatedMenuConfiguration
            {
                Order = sheet.GetValue<int>(rowIndex, startCell + 6),
                Group = new AssociatedMenuGroup?()
            };

            switch (sheet.GetValue<string>(rowIndex, startCell + 5))
            {
                case "Details":
                    amc.Group = AssociatedMenuGroup.Details;
                    break;

                case "Sales":
                    amc.Group = AssociatedMenuGroup.Sales;
                    break;

                case "Service":
                    amc.Group = AssociatedMenuGroup.Service;
                    break;

                case "Marketing":
                    amc.Group = AssociatedMenuGroup.Marketing;
                    break;
            }

            switch (sheet.GetValue<string>(rowIndex, startCell + 3))
            {
                case "Do not display":
                    amc.Behavior = AssociatedMenuBehavior.DoNotDisplay;
                    break;

                case "Custom label":
                    amc.Behavior = AssociatedMenuBehavior.UseLabel;

                    if (!string.IsNullOrEmpty(sheet.GetValue<string>(rowIndex, startCell + 4)))
                    {
                        amc.Label = new Label(sheet.GetValue<string>(rowIndex, startCell + 4), settings.LanguageCode);
                    }
                    else
                    {
                        throw new Exception("If display behavior is \"Custom Label\", the label must be specified");
                    }

                    break;

                default:
                    amc.Behavior = AssociatedMenuBehavior.UseCollectionName;
                    break;
            }

            var cc = new CascadeConfiguration();
            switch (sheet.GetValue<string>(rowIndex, startCell + 7))
            {
                case "Parental":
                    cc.Assign = CascadeType.Cascade;
                    cc.Delete = CascadeType.Cascade;
                    cc.Merge = CascadeType.Cascade;
                    cc.Reparent = CascadeType.Cascade;
                    cc.RollupView = CascadeType.NoCascade;
                    cc.Share = CascadeType.Cascade;
                    cc.Unshare = CascadeType.Cascade;
                    break;

                case "Referential, restrict delete":
                    cc.Assign = CascadeType.NoCascade;
                    cc.Delete = CascadeType.Restrict;
                    cc.Merge = CascadeType.NoCascade;
                    cc.Reparent = CascadeType.NoCascade;
                    cc.RollupView = CascadeType.NoCascade;
                    cc.Share = CascadeType.NoCascade;
                    cc.Unshare = CascadeType.NoCascade;
                    break;

                case "Custom":
                    cc.RollupView = CascadeType.NoCascade;
                    cc.Assign = GetCascade(sheet.GetValue<string>(rowIndex, startCell + 8));
                    cc.Share = GetCascade(sheet.GetValue<string>(rowIndex, startCell + 9));
                    cc.Unshare = GetCascade(sheet.GetValue<string>(rowIndex, startCell + 10));
                    cc.Reparent = GetCascade(sheet.GetValue<string>(rowIndex, startCell + 11));
                    cc.Delete = GetCascade(sheet.GetValue<string>(rowIndex, startCell + 12));
                    cc.Merge = GetCascade(sheet.GetValue<string>(rowIndex, startCell + 13));
                    break;

                default:
                    cc.Assign = CascadeType.NoCascade;
                    cc.Delete = CascadeType.RemoveLink;
                    cc.Merge = CascadeType.NoCascade;
                    cc.Reparent = CascadeType.NoCascade;
                    cc.RollupView = CascadeType.NoCascade;
                    cc.Share = CascadeType.NoCascade;
                    cc.Unshare = CascadeType.NoCascade;
                    break;
            }

            var lookup = new LookupAttributeMetadata
            {
                DisplayName = fakeAmd.DisplayName,
                SchemaName = fakeAmd.SchemaName,
                LogicalName = fakeAmd.LogicalName,
                IsValidForAdvancedFind = fakeAmd.IsValidForAdvancedFind,
                RequiredLevel = fakeAmd.RequiredLevel,
                IsSecured = fakeAmd.IsSecured,
                IsAuditEnabled = fakeAmd.IsAuditEnabled,
                Targets = new[] { sheet.GetValue<string>(rowIndex, startCell).ToLower() }
            };

            if (settings.AddLookupSuffix && !lookup.SchemaName.ToLower().EndsWith("id"))
            {
                lookup.SchemaName = $"{lookup.SchemaName}Id";
                lookup.LogicalName = lookup.SchemaName.ToLower();
            }

            if (fakeAmd.Description != null)
            {
                lookup.Description = fakeAmd.Description;
            }

            var relationship = new OneToManyRelationshipMetadata
            {
                IsValidForAdvancedFind = sheet.GetValue<string>(rowIndex, startCell + 1) == "Yes",
                SchemaName =
                    $"{settings.Solution.Prefix}{info.Entity}_{sheet.GetValue<string>(rowIndex, startCell)}_{lookup.SchemaName}",
                AssociatedMenuConfiguration = amc,
                CascadeConfiguration = cc,
                IsHierarchical = sheet.GetValue<string>(rowIndex, startCell + 2) == "Yes",
                ReferencedEntity = sheet.GetValue<string>(rowIndex, startCell),
                ReferencingEntity = info.Entity,
                SecurityTypes = SecurityTypes.Append,
            };

            if (relationship.SchemaName.Length > 100)
            {
                relationship.SchemaName = relationship.SchemaName.Substring(0, 100);
            }

            if (isCreate)
            {
                service.Execute(new CreateOneToManyRequest
                {
                    OneToManyRelationship = relationship,
                    Lookup = lookup,
                    SolutionUniqueName = settings.Solution.UniqueName
                });

                info.IsCreate = true;

                return null;
            }

            service.Execute(new UpdateRelationshipRequest
            {
                Relationship = relationship,
                SolutionUniqueName = settings.Solution.UniqueName,
                MergeLabels = true
            });

            return lookup;
        }

        private AttributeMetadata CreateMemoAttribute(ExcelWorksheet sheet, int rowIndex, int startCell)
        {
            var mamd = new MemoAttributeMetadata
            {
                MaxLength = sheet.GetValue<int>(rowIndex, startCell)
            };

            return mamd;
        }

        private AttributeMetadata CreateMoneyAttribute(ExcelWorksheet sheet, int rowIndex, int startCell)
        {
            var mamd = new MoneyAttributeMetadata
            {
                Precision = sheet.GetValue<int>(rowIndex, startCell),
                MinValue = sheet.GetValue<double>(rowIndex, startCell + 1),
                MaxValue = sheet.GetValue<double>(rowIndex, startCell + 2)
            };

            return mamd;
        }

        private AttributeMetadata CreateNumberAttribute(ExcelWorksheet sheet, int rowIndex, int startCell)
        {
            var namd = new IntegerAttributeMetadata
            {
                MinValue = sheet.GetValue<int>(rowIndex, startCell + 1),
                MaxValue = sheet.GetValue<int>(rowIndex, startCell + 2)
            };

            switch (sheet.GetValue<string>(rowIndex, startCell))
            {
                default:
                    namd.Format = IntegerFormat.None;
                    break;

                case "Duration":
                    namd.Format = IntegerFormat.Duration;
                    break;

                case "Timezone":
                    namd.Format = IntegerFormat.TimeZone;
                    break;

                case "Language code":
                    namd.Format = IntegerFormat.Language;
                    break;

                case "Locale":
                    namd.Format = IntegerFormat.Locale;
                    break;
            }

            return namd;
        }

        private AttributeMetadata CreateOptionsetAttribute(ExcelWorksheet sheet, int rowIndex, int startCell, bool isMultiSelect, string displayName, string schemaName, string entity, string description, OptionSetMetadata eomd, bool existingAttribute)
        {
            bool isGlobal = false;
            string globalOptionSetName = "";

            var globalValue = sheet.GetValue<string>(rowIndex, startCell + 1);
            if (globalValue != "Yes" && globalValue != "No")
            {
                if (!string.IsNullOrEmpty(globalValue))
                {
                    isGlobal = true;
                    globalOptionSetName = globalValue.ToLower();
                }
            }
            else
            {
                throw new Exception("Choice(s) processing has changed. Please replace Yes/No value by the name of the global choice or empty for local choice");
            }

            var omd = new OptionSetMetadata
            {
                IsGlobal = isGlobal,
                DisplayName = new Label(displayName, settings.LanguageCode),
                OptionSetType = OptionSetType.Picklist,
                Name = schemaName//.ToLower()
            };

            if (!string.IsNullOrEmpty(description))
            {
                omd.Description = new Label(description, settings.LanguageCode);
            }

            string optionsString = sheet.GetValue<string>(rowIndex, startCell);
            if (optionsString.Length == 0)
            {
                throw new Exception("OptionSet values cannot be null");
            }

            OptionMetadataCollection omc = new OptionMetadataCollection();

            foreach (var optionRow in optionsString.Split('\n'))
            {
                var parts = optionRow.Split(':');

                if (parts.Length != 2)
                {
                    continue;
                }

                var index = int.Parse(parts[0]);

                var om = new OptionMetadata(new Label(parts[1], settings.LanguageCode), index);

                if (parts.Length > 2)
                {
                    om.Description = new Label(parts[2], settings.LanguageCode);
                }

                if (majorVersion >= 9)
                {
                    if (parts.Length > 3)
                    {
                        om.ExternalValue = parts[3];
                    }
                }

                omc.Add(om);
            }

            if (settings.AddOptionSetSuffix && !omd.Name.ToLower().EndsWith("code"))
            {
                omd.Name = $"{omd.Name}Code";
            }

            if (isGlobal)
            {
                if (existingAttribute)
                {
                    omd.Name = eomd.Name;
                }
                else
                {
                    omd.Name = omd.Name.ToLower();
                    if (settings.AddOptionSetSuffix && !omd.Name.EndsWith("code"))
                    {
                        omd.Name = $"{omd.Name}code";
                    }
                }

                omd.Options.Clear();
                omd.Options.AddRange(omc);

                if (eomd == null)
                {
                    service.Execute(new CreateOptionSetRequest
                    {
                        OptionSet = omd,
                        SolutionUniqueName = settings.Solution.UniqueName
                    });
                }
                else
                {
                    service.Execute(new UpdateOptionSetRequest
                    {
                        OptionSet = omd,
                        SolutionUniqueName = settings.Solution.UniqueName,
                        MergeLabels = true
                    });

                    ApplyOptionsUpdate(eomd, omd);
                }
            }
            else
            {
                omd.Options.Clear();
                omd.Options.AddRange(omc);
                ApplyOptionsUpdate(eomd, omd, entity, schemaName);
            }

            if (isMultiSelect)
            {
                var amd = new MultiSelectPicklistAttributeMetadata
                {
                    OptionSet = omd
                };

                if (!string.IsNullOrEmpty(sheet.GetValue<string>(rowIndex, startCell + 2)))
                {
                    amd.DefaultFormValue = sheet.GetValue<int>(rowIndex, startCell + 2);
                }

                return amd;
            }
            else
            {
                var omd2 = new OptionSetMetadata();
                if (isGlobal)
                {
                    omd2.IsGlobal = true;
                    omd2.DisplayName = omd.DisplayName;
                    omd2.OptionSetType = omd.OptionSetType;
                    omd2.Name = omd.Name.ToLower();
                    omd2.Description = omd.Description;
                }
                else
                {
                    omd2 = omd;
                }

                var amd = new PicklistAttributeMetadata
                {
                    OptionSet = omd2
                };

                if (!string.IsNullOrEmpty(sheet.GetValue<string>(rowIndex, startCell + 2)))
                {
                    int defaultValue = sheet.GetValue<int>(rowIndex, startCell + 2);

                    if (defaultValue != -1)
                    {
                        if (omd.Options.Any(o => o.Value == defaultValue))
                        {
                            amd.DefaultFormValue = defaultValue;
                        }
                        else
                        {
                            defaultValue = int.Parse($"{settings.Solution.OptionSetPrefix}{defaultValue.ToString().PadLeft(4, '0')}");
                            if (omd.Options.Any(o => o.Value == defaultValue))
                            {
                                amd.DefaultFormValue = defaultValue;
                            }
                        }
                    }
                }

                return amd;
            }
        }

        private AttributeMetadata CreateStringAttribute(ExcelWorksheet sheet, int rowIndex, int startCell)
        {
            var samd = new StringAttributeMetadata
            {
                MaxLength = sheet.GetValue<int>(rowIndex, startCell),
                AutoNumberFormat = sheet.GetValue<string>(rowIndex, startCell + 2)
            };

            switch (sheet.GetValue<string>(rowIndex, startCell + 1))
            {
                case "Email":
                    samd.Format = StringFormat.Email;
                    break;

                default:
                    samd.Format = StringFormat.Text;
                    break;

                case "Text area":
                    samd.Format = StringFormat.TextArea;
                    break;

                case "URL":
                    samd.Format = StringFormat.Url;
                    break;

                case "Ticker symbol":
                    samd.Format = StringFormat.TickerSymbol;
                    break;

                case "Phone":
                    samd.Format = StringFormat.Phone;
                    break;
            }

            return samd;
        }

        private CascadeType? GetCascade(string value)
        {
            switch (value)
            {
                case "None":
                    return CascadeType.NoCascade;

                case "Active":
                    return CascadeType.Active;

                case "Owner":
                    return CascadeType.UserOwned;

                case "Restrict":
                    return CascadeType.Restrict;

                case "Remove link":
                    return CascadeType.RemoveLink;

                default:
                    return CascadeType.Cascade;
            }
        }

        private AttributeMetadata GetFakeAmd(ProcessResult info, ExcelWorksheet workSheet, int i)
        {
            AttributeMetadata fakeAmd = new AttributeMetadata
            {
                DisplayName = new Label(info.DisplayName, settings.LanguageCode),
                SchemaName = info.Attribute,
                LogicalName = info.Attribute.ToLower(),
                IsValidForAdvancedFind = new BooleanManagedProperty(workSheet.GetValue<string>(i, SearchEnabledCellIndex) == "Yes"),
                IsSecured = workSheet.GetValue<string>(i, SecuredCellIndex) == "Yes",
                IsAuditEnabled = new BooleanManagedProperty(workSheet.GetValue<string>(i, AuditCellIndex) == "Yes")
            };
            if (!string.IsNullOrEmpty(workSheet.GetValue<string>(i, DescriptionCellIndex)))
            {
                fakeAmd.Description = new Label(workSheet.GetValue<string>(i, DescriptionCellIndex), settings.LanguageCode);
            }
            switch (workSheet.GetValue<string>(i, RequiredLevelCellIndex))
            {
                case "Optional":
                    fakeAmd.RequiredLevel = new AttributeRequiredLevelManagedProperty(AttributeRequiredLevel.None);
                    break;

                case "Business required":
                    fakeAmd.RequiredLevel = new AttributeRequiredLevelManagedProperty(AttributeRequiredLevel.Recommended);
                    break;

                case "System required":
                    fakeAmd.RequiredLevel =
                        new AttributeRequiredLevelManagedProperty(AttributeRequiredLevel.ApplicationRequired);
                    break;
            }
            switch (workSheet.GetValue<string>(i, FieldTypeCellIndex))
            {
                case "Simple":
                    fakeAmd.SourceType = 0;
                    break;

                case "Calculated":
                    fakeAmd.SourceType = 1;
                    break;

                case "Rollup":
                    fakeAmd.SourceType = 2;
                    break;
            }
            return fakeAmd;
        }
    }
}