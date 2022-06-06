using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using OfficeOpenXml;
using OfficeOpenXml.DataValidation;
using OfficeOpenXml.DataValidation.Contracts;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using StringFormat = Microsoft.Xrm.Sdk.Metadata.StringFormat;

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

        public void GenerateDocumentation(List<EntityMetadata> emds, string filePath, bool loadAllAttributes, bool loadDerivedAttributes, BackgroundWorker worker)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "Javista.AttributesFactory.Template.Attributes_Template.xlsm";

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

                    foreach (var emd in emds)
                    {
                        var fullEmd = ((RetrieveEntityResponse)Service.Execute(new RetrieveEntityRequest
                        {
                            EntityFilters = EntityFilters.Attributes | EntityFilters.Relationships,
                            LogicalName = emd.LogicalName
                        })).EntityMetadata;

                        foreach (var amd in fullEmd.Attributes.OrderBy(a => a.LogicalName))
                        {
                            if (!loadAllAttributes && !(amd.IsCustomAttribute ?? false))
                            {
                                continue;
                            }

                            if (!loadDerivedAttributes && IsBaseOrRollupDerivedAttribute(amd, fullEmd))
                            {
                                continue;
                            }

                            if (!amd.AttributeType.HasValue || amd.AttributeType.Value == AttributeTypeCode.Virtual
                                || amd.AttributeOf != null)
                            {
                                continue;
                            }

                            sheet.InsertRow(line, 1);
                            sheet.Cells[line + 1, 1, line + 1, 56].Copy(sheet.Cells[line, 1]);
                            sheet.Cells[line + 1, 1, line + 1, 56].Style.Border.BorderAround(ExcelBorderStyle.None);

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
                            sheet.Cells[line, 4].Value = amd.AttributeTypeName?.Value ?? amd.AttributeType.ToString();
                            sheet.Cells[line, 5].Value = amd.EntityLogicalName;
                            sheet.Cells[line, 6].Value = amd.Description?.UserLocalizedLabel?.Label;
                            sheet.Cells[line, 7].Value = GetRequiredLevelString(amd.RequiredLevel.Value);
                            sheet.Cells[line, 8].Value = amd.IsValidForAdvancedFind.Value ? "Yes" : "No";
                            sheet.Cells[line, 9].Value = amd.IsSecured ?? false ? "Yes" : "No";
                            sheet.Cells[line, 10].Value = amd.IsAuditEnabled.Value ? "Yes" : "No";
                            sheet.Cells[line, 11].Value = GetSourceTypeString(amd.SourceType ?? 0);

                            ProcessDetails(amd, fullEmd, sheet, line);

                            line++;
                        }
                    }

                    sheet.DeleteRow(line, line + 2);

                    ApplyDataValidation(sheet, line - 1);
                    ApplyConditionalFormatting(sheet, line - 1);

                    package.SaveAs(new FileInfo(filePath));
                }
            }
        }

        private void AddConditionalFormattingExpression(ExcelWorksheet sheet, string reference, int cellNumber, int endLineNumber, params string[] values)
        {
            var formatting = sheet.Cells[3, cellNumber, endLineNumber, cellNumber].ConditionalFormatting.AddExpression();
            formatting.Style.Fill.BackgroundColor.Color = Color.Gray;
            formatting.Style.Font.Color.Color = Color.Gray;

            if (values.Length == 1)
            {
                formatting.Formula = $"={reference}<>ValidationData!{values[0]}";
            }
            else
            {
                formatting.Formula = $"=AND({reference}<>ValidationData!{values[0]},{reference}<>ValidationData!{values[1]})";
            }
        }

        private void ApplyConditionalFormatting(ExcelWorksheet sheet, int line)
        {
            // Max Length
            AddConditionalFormattingExpression(sheet, "D3", 12, line, "$A$12", "$A$13");
            AddConditionalFormattingExpression(sheet, "D3", 13, line, "$A$13");
            AddConditionalFormattingExpression(sheet, "D3", 14, line, "$A$13");
            AddConditionalFormattingExpression(sheet, "D3", 16, line, "$A$2", "$A$3");
            AddConditionalFormattingExpression(sheet, "D3", 17, line, "$A$2", "$A$3");
            AddConditionalFormattingExpression(sheet, "D3", 18, line, "$A$2", "$A$3");
            AddConditionalFormattingExpression(sheet, "D3", 20, line, "$A$14");
            AddConditionalFormattingExpression(sheet, "D3", 21, line, "$A$14");
            AddConditionalFormattingExpression(sheet, "D3", 23, line, "$A$15");
            AddConditionalFormattingExpression(sheet, "D3", 24, line, "$A$15");
            AddConditionalFormattingExpression(sheet, "D3", 25, line, "$A$15");
            AddConditionalFormattingExpression(sheet, "D3", 27, line, "$A$8");
            AddConditionalFormattingExpression(sheet, "D3", 28, line, "$A$8");
            AddConditionalFormattingExpression(sheet, "D3", 29, line, "$A$8");
            AddConditionalFormattingExpression(sheet, "D3", 31, line, "$A$6");
            AddConditionalFormattingExpression(sheet, "D3", 32, line, "$A$6");
            AddConditionalFormattingExpression(sheet, "D3", 33, line, "$A$6");
            AddConditionalFormattingExpression(sheet, "D3", 35, line, "$A$11");
            AddConditionalFormattingExpression(sheet, "D3", 36, line, "$A$11");
            AddConditionalFormattingExpression(sheet, "D3", 37, line, "$A$11");
            AddConditionalFormattingExpression(sheet, "D3", 39, line, "$A$5");
            AddConditionalFormattingExpression(sheet, "D3", 40, line, "$A$5");

            for (var i = 42; i <= 55; i++)
            {
                AddConditionalFormattingExpression(sheet, "D3", i, line, "$A$4", "$A$10");
            }

            AddConditionalFormattingExpression(sheet, "D3", 57, line, "$A$7");
            AddConditionalFormattingExpression(sheet, "D3", 59, line, "$A$9");
            AddConditionalFormattingExpression(sheet, "D3", 60, line, "$A$9");
            AddConditionalFormattingExpression(sheet, "D3", 61, line, "$A$9");

            AddConditionalFormattingExpression(sheet, "AS3", 46, line, "$I$3");

            for (var i = 50; i <= 55; i++)
            {
                AddConditionalFormattingExpression(sheet, "AW3", i, line, "$K$5");
            }
        }

        private void ApplyDataValidation(ExcelWorksheet sheet, int line)
        {
            sheet.DataValidations.RemoveAll(x => x != null);

            var actionValidation = sheet.Cells[3, 1, line, 1].DataValidation.AddListDataValidation();
            actionValidation.Formula.ExcelFormula = "=ValidationData!$O$2:$O$3";
            var typeValidation = sheet.Cells[3, 4, line, 4].DataValidation.AddListDataValidation();
            typeValidation.Formula.ExcelFormula = "=ValidationData!$A$2:$A$15";
            var levelValidation = sheet.Cells[3, 7, line, 7].DataValidation.AddListDataValidation();
            levelValidation.Formula.ExcelFormula = "=ValidationData!$E$2:$E$4";
            var boolValidation = sheet.Cells[3, 8, line, 8].DataValidation.AddListDataValidation();
            boolValidation.Formula.ExcelFormula = "=ValidationData!$D$2:$D$3";
            var boolValidation2 = sheet.Cells[3, 9, line, 9].DataValidation.AddListDataValidation();
            boolValidation2.Formula.ExcelFormula = "=ValidationData!$D$2:$D$3";
            var boolValidation3 = sheet.Cells[3, 10, line, 10].DataValidation.AddListDataValidation();
            boolValidation3.Formula.ExcelFormula = "=ValidationData!$D$2:$D$3";

            var sourceTypeValidation = sheet.Cells[3, 11, line, 11].DataValidation.AddListDataValidation();
            sourceTypeValidation.Formula.ExcelFormula = "=ValidationData!$C$2:$C$4";

            var textFormatValidation = sheet.Cells[3, 13, line, 13].DataValidation.AddListDataValidation();
            textFormatValidation.Formula.ExcelFormula = "=ValidationData!$B$2:$B$9";
            var intFormatValidation = sheet.Cells[3, 23, line, 23].DataValidation.AddListDataValidation();
            intFormatValidation.Formula.ExcelFormula = "=ValidationData!$F$2:$F$6";

            var datetimeBehaviorValidation = sheet.Cells[3, 39, line, 39].DataValidation.AddListDataValidation();
            datetimeBehaviorValidation.Formula.ExcelFormula = "=ValidationData!$H$2:$H$4";
            var datetimeFormatValidation = sheet.Cells[3, 40, line, 40].DataValidation.AddListDataValidation();
            datetimeFormatValidation.Formula.ExcelFormula = "=ValidationData!$G$2:$G$3";

            var relValidForAfValidation = sheet.Cells[3, 43, line, 43].DataValidation.AddListDataValidation();
            relValidForAfValidation.Formula.ExcelFormula = "=ValidationData!$D$2:$D$34";
            var relIsHierarchicalValidation = sheet.Cells[3, 44, line, 44].DataValidation.AddListDataValidation();
            relIsHierarchicalValidation.Formula.ExcelFormula = "=ValidationData!$D$2:$D$3";
            var relDisplayBehaviorValidation = sheet.Cells[3, 45, line, 45].DataValidation.AddListDataValidation();
            relDisplayBehaviorValidation.Formula.ExcelFormula = "=ValidationData!$I$2:$I$4";
            var relDisplayZoneValidation = sheet.Cells[3, 47, line, 47].DataValidation.AddListDataValidation();
            relDisplayZoneValidation.Formula.ExcelFormula = "=ValidationData!$J$2:$J$5";
            var relBehaviorValidation = sheet.Cells[3, 49, line, 49].DataValidation.AddListDataValidation();
            relBehaviorValidation.Formula.ExcelFormula = "=ValidationData!$K$2:$K$5";
            var relCascadeValidation1 = sheet.Cells[3, 50, line, 53].DataValidation.AddListDataValidation();
            relCascadeValidation1.Formula.ExcelFormula = "=ValidationData!$L$2:$L$5";
            var relCascadeDelValidation = sheet.Cells[3, 54, line, 54].DataValidation.AddListDataValidation();
            relCascadeDelValidation.Formula.ExcelFormula = "=ValidationData!$M$2:$M$4";
            var relCascadeValidation2 = sheet.Cells[3, 55, line, 55].DataValidation.AddListDataValidation();
            relCascadeValidation2.Formula.ExcelFormula = "=ValidationData!$L$2:$L$5";
        }

        private string GetCascadeText(CascadeType type, bool isDeleteBehavior = false)
        {
            switch (type)
            {
                case CascadeType.Active:
                    return "Active";

                case CascadeType.NoCascade:
                    return "None";

                case CascadeType.UserOwned:
                    return "Owner";

                case CascadeType.RemoveLink:
                    return "Remove link";

                case CascadeType.Restrict:
                    return "Restrict";

                default:
                    return isDeleteBehavior ? "All" : "Cascade";
            }
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
                case AttributeRequiredLevel.Recommended: return "Business recommended";
                case AttributeRequiredLevel.SystemRequired: return "Business required";
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

        private bool IsBaseOrRollupDerivedAttribute(AttributeMetadata amd, EntityMetadata emd)
        {
            if ((amd.LogicalName.EndsWith("_state")
                || amd.LogicalName.EndsWith("_date")
                || amd.LogicalName.EndsWith("_sum")
                || amd.LogicalName.EndsWith("_count")
                )
                && emd.Attributes.Any(a => a.LogicalName == amd.LogicalName.Replace("_state", "").Replace("_date", "").Replace("_sum", "").Replace("_count", "") && a.SourceType == 2))
            {
                return true;
            }

            if (amd.LogicalName.EndsWith("_base") && emd.Attributes.Any(a => a.LogicalName == amd.LogicalName.Replace("_base", "") && a.AttributeType == AttributeTypeCode.Money))
            {
                return true;
            }

            return false;
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
                sheet.Cells[line, 4].Value = "Choice";
                sheet.Cells[line, 16].Value = GetOptionSets(pamd.OptionSet);
                sheet.Cells[line, 17].Value = pamd.OptionSet.IsGlobal ?? false ? pamd.OptionSet.Name : "";
                sheet.Cells[line, 18].Value = pamd.DefaultFormValue.HasValue ? pamd.DefaultFormValue.Value.ToString() : "";
            }
            else if (amd is MultiSelectPicklistAttributeMetadata msamd)
            {
                sheet.Cells[line, 4].Value = "Choices";
                sheet.Cells[line, 16].Value = GetOptionSets(msamd.OptionSet);
                sheet.Cells[line, 17].Value = msamd.OptionSet.IsGlobal ?? false ? msamd.OptionSet.Name : "";
                sheet.Cells[line, 18].Value = msamd.DefaultFormValue.HasValue ? msamd.DefaultFormValue.Value.ToString() : "";
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
                switch (dtAmd.DateTimeBehavior?.Value ?? "UserLocal")
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
                var rel = emd.ManyToOneRelationships.FirstOrDefault(r => r.ReferencingAttribute == amd.LogicalName);
                if (rel == null)
                {
                    return;
                }

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

                sheet.Cells[line, 50].Value = GetCascadeText(rel.CascadeConfiguration.Assign ?? CascadeType.Cascade);
                sheet.Cells[line, 51].Value = GetCascadeText(rel.CascadeConfiguration.Share ?? CascadeType.Cascade);
                sheet.Cells[line, 52].Value = GetCascadeText(rel.CascadeConfiguration.Unshare ?? CascadeType.Cascade);
                sheet.Cells[line, 53].Value = GetCascadeText(rel.CascadeConfiguration.Reparent ?? CascadeType.Cascade);
                sheet.Cells[line, 54].Value = GetCascadeText(rel.CascadeConfiguration.Delete ?? CascadeType.Cascade, true);
                sheet.Cells[line, 55].Value = GetCascadeText(rel.CascadeConfiguration.Merge ?? CascadeType.Cascade);

                if ((rel.CascadeConfiguration.Assign ?? CascadeType.Cascade) == CascadeType.Cascade
                    && (rel.CascadeConfiguration.Share ?? CascadeType.Cascade) == CascadeType.Cascade
                    && (rel.CascadeConfiguration.Assign ?? CascadeType.Cascade) == CascadeType.Cascade
                    && (rel.CascadeConfiguration.Unshare ?? CascadeType.Cascade) == CascadeType.Cascade
                    && (rel.CascadeConfiguration.Reparent ?? CascadeType.Cascade) == CascadeType.Cascade
                    && (rel.CascadeConfiguration.Delete ?? CascadeType.Cascade) == CascadeType.Cascade
                    && (rel.CascadeConfiguration.Merge ?? CascadeType.Cascade) == CascadeType.Cascade)
                {
                    sheet.Cells[line, 49].Value = "Parental";
                }
                else if ((rel.CascadeConfiguration.Assign ?? CascadeType.Cascade) == CascadeType.NoCascade
                   && (rel.CascadeConfiguration.Share ?? CascadeType.Cascade) == CascadeType.NoCascade
                   && (rel.CascadeConfiguration.Assign ?? CascadeType.Cascade) == CascadeType.NoCascade
                   && (rel.CascadeConfiguration.Unshare ?? CascadeType.Cascade) == CascadeType.NoCascade
                   && (rel.CascadeConfiguration.Reparent ?? CascadeType.Cascade) == CascadeType.NoCascade
                   && (rel.CascadeConfiguration.Delete ?? CascadeType.Cascade) == CascadeType.RemoveLink
                   && (rel.CascadeConfiguration.Merge ?? CascadeType.Cascade) == CascadeType.Cascade)
                {
                    sheet.Cells[line, 49].Value = "Referential";
                }
                else if ((rel.CascadeConfiguration.Assign ?? CascadeType.Cascade) == CascadeType.NoCascade
                    && (rel.CascadeConfiguration.Share ?? CascadeType.Cascade) == CascadeType.NoCascade
                    && (rel.CascadeConfiguration.Assign ?? CascadeType.Cascade) == CascadeType.NoCascade
                    && (rel.CascadeConfiguration.Unshare ?? CascadeType.Cascade) == CascadeType.NoCascade
                    && (rel.CascadeConfiguration.Reparent ?? CascadeType.Cascade) == CascadeType.NoCascade
                    && (rel.CascadeConfiguration.Delete ?? CascadeType.Cascade) == CascadeType.Restrict
                    && (rel.CascadeConfiguration.Merge ?? CascadeType.Cascade) == CascadeType.Cascade)
                {
                    sheet.Cells[line, 49].Value = "Referential, restrict delete";
                }
                else
                    sheet.Cells[line, 49].Value = "Custom";
            }
        }
    }
}