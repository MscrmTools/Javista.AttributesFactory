using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Metadata.Query;
using OfficeOpenXml;
using OfficeOpenXml.DataValidation;
using OfficeOpenXml.DataValidation.Contracts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace Javista.AttributesFactory.AppCode
{
    internal class MetadataDocManager
    {
        private Control parentControl;

        public MetadataDocManager(IOrganizationService service, Control parentControl)
        {
            Service = service;

            this.parentControl = parentControl;
        }

        public IOrganizationService Service { get; set; }

        public void GenerateDocumentation(List<EntityMetadata> emds, out string filePath, BackgroundWorker worker)
        {
            filePath = "";

            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "Javista.AttributesFactory.Template.Attributes_Template.xlsx";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    MessageBox.Show(parentControl, @"There was an error trying to retrieve the Excel template",
                        @"Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                using (ExcelPackage package = new ExcelPackage(stream))
                {
                    ExcelWorksheet sheet = package.Workbook.Worksheets.First();

                    int line = 3;
                    int startCell = 2;

                    foreach (var emd in emds)
                    {
                        var fullEmd = ((RetrieveEntityResponse)Service.Execute(new RetrieveEntityRequest
                        {
                            EntityFilters = EntityFilters.Attributes | EntityFilters.Relationships,
                            LogicalName = emd.LogicalName
                        })).EntityMetadata;

                        foreach (var amd in fullEmd.Attributes)
                        {
                            if (!(amd.IsCustomAttribute ?? false))
                            {
                                continue;
                            }

                            if (amd.AttributeType.Value == AttributeTypeCode.Virtual
                                || amd.AttributeOf != null)
                            {
                                continue;
                            }

                            sheet.InsertRow(line, 1);
                            sheet.Cells[line + 1, 1, line + 1, 56].Copy(sheet.Cells[line, 1]);

                            for (int i = 1; i <= 56; i++)
                            {
                                var sourceRange = sheet.Cells[line + 1, i].Address;
                                var sourceValidation = sheet.DataValidations[sourceRange];
                                if (sourceValidation != null)
                                {
                                    //Test for each type
                                    if (sourceValidation.ValidationType.Type == eDataValidationType.List)
                                    {
                                        var destRange = sheet.Cells[line, i].Address;
                                        var destCell = sheet.Cells[destRange];
                                        var destVal = sheet.DataValidations.AddListValidation(destCell.Address);
                                        destVal.Formula.ExcelFormula = ((IExcelDataValidationList)sourceValidation).Formula.ExcelFormula;
                                    }
                                }
                            }

                            sheet.Cells[line, 1].Value = "Process";
                            sheet.Cells[line, 2].Value = amd.DisplayName?.UserLocalizedLabel?.Label ?? "N/A";
                            sheet.Cells[line, 3].Value = amd.SchemaName;
                            sheet.Cells[line, 4].Value = amd.AttributeTypeName.Value;
                            sheet.Cells[line, 5].Value = amd.EntityLogicalName;
                            sheet.Cells[line, 6].Value = amd.Description?.UserLocalizedLabel?.Label;
                            sheet.Cells[line, 7].Value = GetRequiredLevelString(amd.RequiredLevel.Value);
                            sheet.Cells[line, 8].Value = amd.IsValidForAdvancedFind.Value ? "Yes" : "No";
                            sheet.Cells[line, 9].Value = amd.IsSecured ?? false ? "Yes" : "No";
                            sheet.Cells[line, 10].Value = amd.IsAuditEnabled.Value ? "Yes" : "No";
                            sheet.Cells[line, 11].Value = GetSourceTypeString(amd.SourceType ?? 0);

                            ProcessDetails(amd, fullEmd, sheet, line);
                        }

                        line++;
                    }

                    filePath = "c:\\temp\\md.xlsx";

                    package.SaveAs(new FileInfo(filePath));
                }
            }
        }

        public List<EntityMetadata> GetEntities()
        {
            EntityQueryExpression entityQueryExpression = new EntityQueryExpression()
            {
                Properties = new MetadataPropertiesExpression
                {
                    AllProperties = false,
                    PropertyNames = { "LogicalName", "DisplayName", "SchemaName" }
                }
            };

            RetrieveMetadataChangesRequest retrieveMetadataChangesRequest = new RetrieveMetadataChangesRequest
            {
                Query = entityQueryExpression,
                ClientVersionStamp = null
            };

            var response = (RetrieveMetadataChangesResponse)Service.Execute(retrieveMetadataChangesRequest);

            return response.EntityMetadata.ToList();
        }

        private string GetOptionSets(OptionSetMetadata optionSet)
        {
            var sb = new StringBuilder();
            foreach (var option in optionSet.Options)
            {
                sb.AppendLine($"{option.Value ?? -1}:{option.Label?.UserLocalizedLabel?.Label}");
            }

            return sb.ToString();
        }

        private object GetRequiredLevelString(AttributeRequiredLevel value)
        {
            switch (value)
            {
                case AttributeRequiredLevel.None: return "Optional";
                case AttributeRequiredLevel.Recommended: return "Business required";
                case AttributeRequiredLevel.SystemRequired: return "System required";
                default: return "Application required";
            }
        }

        private string GetSourceTypeString(int v)
        {
            switch (v)
            {
                case 0: return "Simple";
                case 1: return "Calculated";
                case 2: return "Rollup";
                default: return "n/a";
            }
        }

        private void ProcessDetails(AttributeMetadata amd, EntityMetadata emd, ExcelWorksheet sheet, int line)
        {
            if (amd is StringAttributeMetadata samd)
            {
                string format = string.Empty;
                switch (samd.Format ?? StringFormat.Text)
                {
                    case StringFormat.Email:
                        format = "Email";
                        break;

                    case StringFormat.Phone:
                        format = "Phone";
                        break;

                    case StringFormat.PhoneticGuide:
                        format = "Phonetic guide";
                        break;

                    case StringFormat.Text:
                        format = "Text";
                        break;

                    case StringFormat.TextArea:
                        format = "Text area";
                        break;

                    case StringFormat.TickerSymbol:
                        format = "Ticker symbol";
                        break;

                    case StringFormat.Url:
                        format = "URL";
                        break;

                    case StringFormat.VersionNumber:
                        format = "Version number";
                        break;
                }

                sheet.Cells[line, 4].Value = "Single line of text";
                sheet.Cells[line, 12].Value = samd.MaxLength;
                sheet.Cells[line, 13].Value = format;
                sheet.Cells[line, 14].Value = samd.AutoNumberFormat;
            }
            else if (amd is MemoAttributeMetadata memoAmd)
            {
                sheet.Cells[line, 4].Value = "Multiple lines of text";
                sheet.Cells[line, 12].Value = memoAmd.MaxLength;
                sheet.Cells[line, 14].Value = memoAmd.AutoNumberFormat;
            }
            else if (amd is PicklistAttributeMetadata pamd)
            {
                sheet.Cells[line, 4].Value = "OptionSet";
                sheet.Cells[line, 16].Value = GetOptionSets(pamd.OptionSet);
                sheet.Cells[line, 17].Value = pamd.OptionSet.IsGlobal ?? false ? "Yes" : "No";
                sheet.Cells[line, 18].Value = pamd.DefaultFormValue ?? -1;
            }
            else if (amd is MultiSelectPicklistAttributeMetadata msamd)
            {
                sheet.Cells[line, 4].Value = "Multiselect OptionSet";
                sheet.Cells[line, 16].Value = GetOptionSets(msamd.OptionSet);
                sheet.Cells[line, 17].Value = msamd.OptionSet.IsGlobal ?? false ? "Yes" : "No";
                sheet.Cells[line, 18].Value = msamd.DefaultFormValue ?? -1;
            }
            else if (amd is BooleanAttributeMetadata bamd)
            {
                sheet.Cells[line, 4].Value = "Two options";
                sheet.Cells[line, 20].Value = $"{bamd.OptionSet.FalseOption.Value ?? -1}:{bamd.OptionSet.FalseOption?.Label?.UserLocalizedLabel?.Label}{Environment.NewLine}{bamd.OptionSet.TrueOption.Value ?? -1}:{bamd.OptionSet.TrueOption?.Label?.UserLocalizedLabel?.Label}";
                sheet.Cells[line, 21].Value = bamd.DefaultValue ?? false ? "True" : "False";
            }
            else if (amd is IntegerAttributeMetadata iamd)
            {
                string format = string.Empty;
                switch (iamd.Format ?? IntegerFormat.None)
                {
                    case IntegerFormat.None:
                        format = "None";
                        break;

                    case IntegerFormat.Duration:
                        format = "Duration";
                        break;

                    case IntegerFormat.Language:
                        format = "Language code";
                        break;

                    case IntegerFormat.Locale:
                        format = "Locale";
                        break;

                    case IntegerFormat.TimeZone:
                        format = "Timezone";
                        break;
                }

                sheet.Cells[line, 4].Value = "Whole number";
                sheet.Cells[line, 23].Value = format;
                sheet.Cells[line, 24].Value = iamd.MinValue ?? -1;
                sheet.Cells[line, 25].Value = iamd.MaxValue ?? -1;
            }
            else if (amd is DoubleAttributeMetadata damd)
            {
                sheet.Cells[line, 4].Value = "Float number";
                sheet.Cells[line, 27].Value = damd.Precision ?? -1;
                sheet.Cells[line, 28].Value = damd.MinValue ?? -1;
                sheet.Cells[line, 29].Value = damd.MaxValue ?? -1;
            }
            else if (amd is DecimalAttributeMetadata decAmd)
            {
                sheet.Cells[line, 4].Value = "Decimal number";
                sheet.Cells[line, 31].Value = decAmd.Precision ?? -1;
                sheet.Cells[line, 32].Value = decAmd.MinValue ?? -1;
                sheet.Cells[line, 33].Value = decAmd.MaxValue ?? -1;
            }
            else if (amd is MoneyAttributeMetadata mamd)
            {
                sheet.Cells[line, 4].Value = "Money";
                sheet.Cells[line, 35].Value = mamd.Precision ?? -1;
                sheet.Cells[line, 36].Value = mamd.MinValue ?? -1;
                sheet.Cells[line, 37].Value = mamd.MaxValue ?? -1;
            }
            else if (amd is DateTimeAttributeMetadata dtAmd)
            {
                string behavior = string.Empty;
                switch (dtAmd.DateTimeBehavior.Value)
                {
                    case "DateOnly":
                        behavior = "Date only";
                        break;

                    case "TimeZoneIndependent":
                        behavior = "Timezone independent";
                        break;

                    case "UserLocal":
                        behavior = "User local time";
                        break;
                }

                sheet.Cells[line, 4].Value = "Date and time";
                sheet.Cells[line, 39].Value = behavior;
                sheet.Cells[line, 40].Value = (dtAmd.Format ?? DateTimeFormat.DateOnly) == DateTimeFormat.DateOnly ? "Date only" : "Date and time";
            }
            else if (amd is LookupAttributeMetadata lamd)
            {
                var rel = emd.ManyToOneRelationships.First(r => r.ReferencingAttribute == amd.LogicalName);

                var behavior = string.Empty;
                switch (rel.AssociatedMenuConfiguration.Behavior ?? AssociatedMenuBehavior.UseCollectionName)
                {
                    case AssociatedMenuBehavior.DoNotDisplay:
                        behavior = "Do not display";
                        break;

                    case AssociatedMenuBehavior.UseCollectionName:
                        behavior = "Use plural name";
                        break;

                    case AssociatedMenuBehavior.UseLabel:
                        behavior = "Custom label";
                        break;
                }

                sheet.Cells[line, 4].Value = "Lookup";
                sheet.Cells[line, 42].Value = String.Join(",", lamd.Targets);
                sheet.Cells[line, 43].Value = lamd.IsValidForAdvancedFind.Value ? "Yes" : "No";
                sheet.Cells[line, 44].Value = rel.IsHierarchical ?? false ? "Yes" : "No";
                sheet.Cells[line, 45].Value = behavior;
                sheet.Cells[line, 46].Value = rel.AssociatedMenuConfiguration.Label?.UserLocalizedLabel?.Label;
                sheet.Cells[line, 47].Value = rel.AssociatedMenuConfiguration.Group ?? AssociatedMenuGroup.Details;
                sheet.Cells[line, 48].Value = rel.AssociatedMenuConfiguration.Order ?? -1;
                //sheet.Cells[line, 49].Value = rel.CascadeConfiguration.;
                //sheet.Cells[line, 50].Value = lamd.DateTimeBehavior.Value;
                //sheet.Cells[line, 51].Value = lamd.DateTimeBehavior.Value;
                //sheet.Cells[line, 52].Value = lamd.DateTimeBehavior.Value;
                //sheet.Cells[line, 53].Value = lamd.DateTimeBehavior.Value;
                //sheet.Cells[line, 54].Value = lamd.DateTimeBehavior.Value;
                //sheet.Cells[line, 55].Value = lamd.DateTimeBehavior.Value;
            }
        }
    }
}