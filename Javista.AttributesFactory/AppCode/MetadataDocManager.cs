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
                    ExcelWorksheet nnWorkSheet = package.Workbook.Worksheets[package.Compatibility.IsWorksheets1Based ? 2 : 1];

                    int line = 3;
                    int lineNn = 3;

                    var rels = new List<string>();

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

                            if ((!amd.AttributeType.HasValue || amd.AttributeType.Value == AttributeTypeCode.Virtual
                                || amd.AttributeType.Value == AttributeTypeCode.Uniqueidentifier
                                || amd.AttributeOf != null) && !(amd is ImageAttributeMetadata) && !(amd is FileAttributeMetadata))
                            {
                                continue;
                            }

                            AddLine(sheet, amd, line);
                            ProcessDetails(amd, fullEmd, sheet, ref line);

                            line++;
                        }

                        foreach (var rmd in fullEmd.ManyToManyRelationships.OrderBy(a => a.SchemaName))
                        {
                            if (!loadAllAttributes && !(rmd.IsCustomRelationship ?? false))
                            {
                                continue;
                            }

                            if (rels.Contains(rmd.SchemaName)) continue;
                            rels.Add(rmd.SchemaName);

                            AddLine(nnWorkSheet, rmd, lineNn);

                            lineNn++;
                        }
                    }

                    sheet.DeleteRow(line, line + 2);
                    nnWorkSheet.DeleteRow(line, line + 1);

                    ApplyDataValidation(sheet, line - 1);
                    ApplyDataValidationNN(nnWorkSheet, line - 1);
                    ApplyConditionalFormatting(sheet, line - 1);

                    sheet.Cells[1, 66, sheet.Dimension.Rows, 66].Merge = true;

                    package.SaveAs(new FileInfo(filePath));
                }
            }
        }

        private void AddConditionalFormattingExpressionForHiding(ExcelWorksheet sheet, string reference, int cellNumber, int endLineNumber, params string[] values)
        {
            var formatting = sheet.Cells[3, cellNumber, endLineNumber, cellNumber].ConditionalFormatting.AddExpression();
            formatting.Style.Fill.BackgroundColor.Color = Color.Gray;
            formatting.Style.Font.Color.Color = Color.Gray;

            if (values.Length == 1)
            {
                formatting.Formula = $"={reference}<>VD!{values[0]}";
            }
            else
            {
                formatting.Formula = $"=AND({reference}<>VD!{string.Join($",{reference}<>VD!", values)})";
            }
        }

        private void AddConditionalFormattingExpressionForShowing(ExcelWorksheet sheet, string reference, int cellNumber, int endLineNumber, params string[] values)
        {
            var formatting = sheet.Cells[3, cellNumber, endLineNumber, cellNumber].ConditionalFormatting.AddExpression();
            formatting.Style.Fill.BackgroundColor.Color = Color.Transparent;
            formatting.Style.Font.Color.Color = Color.Black;

            if (values.Length == 1)
            {
                formatting.Formula = $"={reference}=VD!{values[0]}";
            }
            else
            {
                formatting.Formula = $"=OR({reference}=VD!{string.Join($",{reference}=VD!", values)})";
            }
        }

        private void AddLine(ExcelWorksheet sheet, ManyToManyRelationshipMetadata rmd, int line)
        {
            sheet.InsertRow(line, 1);
            sheet.Cells[line + 1, 1, line + 1, "M".PositionFromExcelColumn()].Copy(sheet.Cells[line, 1]);
            sheet.Cells[line + 1, 1, line + 1, "M".PositionFromExcelColumn()].Style.Border.BorderAround(ExcelBorderStyle.None);

            for (int i = 1; i <= "M".PositionFromExcelColumn(); i++)
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

            var behavior = string.Empty;
            switch (rmd.Entity1AssociatedMenuConfiguration.Behavior ?? AssociatedMenuBehavior.UseCollectionName)
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

            sheet.Cells[line, 1].Value = "Process";
            sheet.Cells[line, 2].Value = rmd.SchemaName;
            sheet.Cells[line, 3].Value = rmd.IsValidForAdvancedFind ?? false ? "Yes" : "No";
            sheet.Cells[line, 4].Value = rmd.Entity1LogicalName;
            sheet.Cells[line, 5].Value = behavior;
            sheet.Cells[line, 6].Value = rmd.Entity1AssociatedMenuConfiguration.Label?.UserLocalizedLabel?.Label;
            sheet.Cells[line, 7].Value = rmd.Entity1AssociatedMenuConfiguration.Group ?? AssociatedMenuGroup.Details;
            sheet.Cells[line, 8].Value = rmd.Entity1AssociatedMenuConfiguration.Order;

            behavior = string.Empty;
            switch (rmd.Entity2AssociatedMenuConfiguration.Behavior ?? AssociatedMenuBehavior.UseCollectionName)
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

            sheet.Cells[line, 9].Value = rmd.Entity2LogicalName;
            sheet.Cells[line, 10].Value = behavior;
            sheet.Cells[line, 11].Value = rmd.Entity2AssociatedMenuConfiguration.Label?.UserLocalizedLabel?.Label;
            sheet.Cells[line, 12].Value = rmd.Entity2AssociatedMenuConfiguration.Group ?? AssociatedMenuGroup.Details;
            sheet.Cells[line, 13].Value = rmd.Entity2AssociatedMenuConfiguration.Order;
        }

        private void AddLine(ExcelWorksheet sheet, AttributeMetadata amd, int line)
        {
            sheet.InsertRow(line, 1);
            sheet.Cells[line + 1, 1, line + 1, 65].Copy(sheet.Cells[line, 1]);
            sheet.Cells[line + 1, 1, line + 1, 65].Style.Border.BorderAround(ExcelBorderStyle.None);

            for (int i = 1; i <= 65; i++)
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
        }

        private void ApplyConditionalFormatting(ExcelWorksheet sheet, int line)
        {
            // Max Length
            AddConditionalFormattingExpressionForHiding(sheet, "D3", 12, line, "$A$13", "$A$14");
            AddConditionalFormattingExpressionForHiding(sheet, "D3", 13, line, "$A$13", "$A$14");
            AddConditionalFormattingExpressionForHiding(sheet, "D3", 14, line, "$A$14");
            AddConditionalFormattingExpressionForHiding(sheet, "D3", 16, line, "$A$2", "$A$3", "$A$18", "$A$19");
            AddConditionalFormattingExpressionForHiding(sheet, "D3", 17, line, "$A$2", "$A$3", "$A$18", "$A$19");
            AddConditionalFormattingExpressionForHiding(sheet, "D3", 18, line, "$A$2", "$A$3", "$A$18", "$A$19");
            AddConditionalFormattingExpressionForHiding(sheet, "D3", 20, line, "$A$15");
            AddConditionalFormattingExpressionForHiding(sheet, "D3", 21, line, "$A$15");
            AddConditionalFormattingExpressionForHiding(sheet, "D3", 23, line, "$A$16");
            AddConditionalFormattingExpressionForHiding(sheet, "D3", 24, line, "$A$16");
            AddConditionalFormattingExpressionForHiding(sheet, "D3", 25, line, "$A$16");
            AddConditionalFormattingExpressionForHiding(sheet, "D3", 27, line, "$A$8");
            AddConditionalFormattingExpressionForHiding(sheet, "D3", 28, line, "$A$8");
            AddConditionalFormattingExpressionForHiding(sheet, "D3", 29, line, "$A$8");
            AddConditionalFormattingExpressionForHiding(sheet, "D3", 31, line, "$A$6");
            AddConditionalFormattingExpressionForHiding(sheet, "D3", 32, line, "$A$6");
            AddConditionalFormattingExpressionForHiding(sheet, "D3", 33, line, "$A$6");
            AddConditionalFormattingExpressionForHiding(sheet, "D3", 35, line, "$A$12");
            AddConditionalFormattingExpressionForHiding(sheet, "D3", 36, line, "$A$12");
            AddConditionalFormattingExpressionForHiding(sheet, "D3", 37, line, "$A$12");
            AddConditionalFormattingExpressionForHiding(sheet, "D3", 38, line, "$A$12");
            AddConditionalFormattingExpressionForHiding(sheet, "D3", 40, line, "$A$5");
            AddConditionalFormattingExpressionForHiding(sheet, "D3", 41, line, "$A$5");

            for (var i = 43; i <= 57; i++)
            {
                AddConditionalFormattingExpressionForHiding(sheet, "D3", i, line, "$A$4", "$A$10", "$A$11", "$A$17");
            }

            AddConditionalFormattingExpressionForShowing(sheet, "AT3", 47, line, "$I$3");

            for (var i = 51; i <= 56; i++)
            {
                AddConditionalFormattingExpressionForHiding(sheet, "AX3", i, line, "$K$5");
                AddConditionalFormattingExpressionForShowing(sheet, "AX3", i, line, "$K$5");
            }

            AddConditionalFormattingExpressionForHiding(sheet, "D3", 59, line, "$A$7");
            AddConditionalFormattingExpressionForHiding(sheet, "D3", 61, line, "$A$9");
            AddConditionalFormattingExpressionForHiding(sheet, "D3", 62, line, "$A$9");
            AddConditionalFormattingExpressionForHiding(sheet, "D3", 63, line, "$A$9");

            AddConditionalFormattingExpressionForHiding(sheet, "AS3", 47, line, "$I$3");

            AddConditionalFormattingExpressionForHiding(sheet, "K3", 65, line, "$C$5", "$C$9");
        }

        private void ApplyDataValidation(ExcelWorksheet sheet, int line)
        {
            sheet.DataValidations.RemoveAll(x => x != null);

            var actionValidation = sheet.Cells[3, 1, line, 1].DataValidation.AddListDataValidation();
            actionValidation.Formula.ExcelFormula = "=VD!$O$2:$O$3";
            var typeValidation = sheet.Cells[3, 4, line, 4].DataValidation.AddListDataValidation();
            typeValidation.Formula.ExcelFormula = "=VD!$A$2:$A$15";
            var levelValidation = sheet.Cells[3, 7, line, 7].DataValidation.AddListDataValidation();
            levelValidation.Formula.ExcelFormula = "=VD!$E$2:$E$4";
            var boolValidation = sheet.Cells[3, 8, line, 8].DataValidation.AddListDataValidation();
            boolValidation.Formula.ExcelFormula = "=VD!$D$2:$D$3";
            var boolValidation2 = sheet.Cells[3, 9, line, 9].DataValidation.AddListDataValidation();
            boolValidation2.Formula.ExcelFormula = "=VD!$D$2:$D$3";
            var boolValidation3 = sheet.Cells[3, 10, line, 10].DataValidation.AddListDataValidation();
            boolValidation3.Formula.ExcelFormula = "=VD!$D$2:$D$3";

            var sourceTypeValidation = sheet.Cells[3, 11, line, 11].DataValidation.AddListDataValidation();
            sourceTypeValidation.Formula.ExcelFormula = "=IF(D4=VD!$A$2,VD!$C$2:$C$3,IF(OR(D4=VD!$A$5,D4=VD!$A$6),VD!$C$2:$C$5,IF(OR(D4=VD!$A$12,D4=VD!$A$16),VD!$C$2:$C$4,IF(OR(D4=VD!$A$14,D4=VD!$A$15),VD!$C$7:$C$9,VD!$C$2))))";

            var textFormatValidation = sheet.Cells[3, 13, line, 13].DataValidation.AddListDataValidation();
            textFormatValidation.Formula.ExcelFormula = "=IF(D3=VD!$A$14,SingleList,MultiList)";
            var intFormatValidation = sheet.Cells[3, 23, line, 23].DataValidation.AddListDataValidation();
            intFormatValidation.Formula.ExcelFormula = "=VD!$F$2:$F$6";

            var datetimeBehaviorValidation = sheet.Cells[3, 40, line, 40].DataValidation.AddListDataValidation();
            datetimeBehaviorValidation.Formula.ExcelFormula = "=VD!$H$2:$H$4";
            var datetimeFormatValidation = sheet.Cells[3, 41, line, 41].DataValidation.AddListDataValidation();
            datetimeFormatValidation.Formula.ExcelFormula = "=VD!$G$2:$G$3";

            var relValidForAfValidation = sheet.Cells[3, 44, line, 44].DataValidation.AddListDataValidation();
            relValidForAfValidation.Formula.ExcelFormula = "=VD!$D$2:$D$34";
            var relIsHierarchicalValidation = sheet.Cells[3, 45, line, 45].DataValidation.AddListDataValidation();
            relIsHierarchicalValidation.Formula.ExcelFormula = "=VD!$D$2:$D$3";
            var relDisplayBehaviorValidation = sheet.Cells[3, 46, line, 46].DataValidation.AddListDataValidation();
            relDisplayBehaviorValidation.Formula.ExcelFormula = "=VD!$I$2:$I$4";
            var relDisplayZoneValidation = sheet.Cells[3, 48, line, 48].DataValidation.AddListDataValidation();
            relDisplayZoneValidation.Formula.ExcelFormula = "=VD!$J$2:$J$5";
            var relBehaviorValidation = sheet.Cells[3, 50, line, 50].DataValidation.AddListDataValidation();
            relBehaviorValidation.Formula.ExcelFormula = "=VD!$K$2:$K$5";
            var relCascadeValidation1 = sheet.Cells[3, 51, line, 54].DataValidation.AddListDataValidation();
            relCascadeValidation1.Formula.ExcelFormula = "=VD!$L$2:$L$5";
            var relCascadeDelValidation = sheet.Cells[3, 55, line, 55].DataValidation.AddListDataValidation();
            relCascadeDelValidation.Formula.ExcelFormula = "=VD!$M$2:$M$4";
            var relCascadeValidation2 = sheet.Cells[3, 56, line, 56].DataValidation.AddListDataValidation();
            relCascadeValidation2.Formula.ExcelFormula = "=VD!$L$2:$L$5";
        }

        private void ApplyDataValidationNN(ExcelWorksheet sheet, int line)
        {
            sheet.DataValidations.RemoveAll(x => x != null);

            var actionValidation = sheet.Cells[3, 1, line, 1].DataValidation.AddListDataValidation();
            actionValidation.Formula.ExcelFormula = "=VD!$O$2:$O$3";
            var boolValidation = sheet.Cells[3, 3, line, 3].DataValidation.AddListDataValidation();
            boolValidation.Formula.ExcelFormula = "=VD!$D$2:$D$3";

            var relDisplayBehaviorValidation = sheet.Cells[3, 5, line, 5].DataValidation.AddListDataValidation();
            relDisplayBehaviorValidation.Formula.ExcelFormula = "=VD!$I$2:$I$4";
            var relDisplayBehaviorValidation2 = sheet.Cells[3, 10, line, 10].DataValidation.AddListDataValidation();
            relDisplayBehaviorValidation2.Formula.ExcelFormula = "=VD!$I$2:$I$4";

            var relDisplayZoneValidation = sheet.Cells[3, 7, line, 7].DataValidation.AddListDataValidation();
            relDisplayZoneValidation.Formula.ExcelFormula = "=VD!$J$2:$J$5";
            var relDisplayZoneValidation2 = sheet.Cells[3, 12, line, 12].DataValidation.AddListDataValidation();
            relDisplayZoneValidation2.Formula.ExcelFormula = "=VD!$J$2:$J$5";
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

        private string GetPrecisionSourceString(int v)
        {
            switch (v)
            {
                case 0: return "Specific Precision";
                case 1: return "Pricing Decimal Precision";
                case 2: return "Currency Precision";
                default: return "n/a";
            }
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
                case 3: return "Formula";
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

        private void ProcessDetails(AttributeMetadata amd, EntityMetadata emd, ExcelWorksheet sheet, ref int line)
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

                sheet.Cells[line, 65].Value = samd.FormulaDefinition;
            }
            else if (amd is MemoAttributeMetadata memoAmd)
            {
                sheet.Cells[line, 4].Value = "Multiple lines of text";
                sheet.Cells[line, 12].Value = memoAmd.MaxLength;
                sheet.Cells[line, 13].Value = memoAmd.FormatName.Value == "RichText" ? "Rich Text" : "Text";
                sheet.Cells[line, 14].Value = memoAmd.AutoNumberFormat;
            }
            else if (amd is PicklistAttributeMetadata pamd)
            {
                sheet.Cells[line, 4].Value = "Choice";
                sheet.Cells[line, 16].Value = GetOptionSets(pamd.OptionSet);
                sheet.Cells[line, 17].Value = pamd.OptionSet.IsGlobal ?? false ? pamd.OptionSet.Name : "";
                sheet.Cells[line, 18].Value = pamd.DefaultFormValue.HasValue ? pamd.DefaultFormValue.Value.ToString() : "";
            }
            else if (amd is StateAttributeMetadata stateAmd)
            {
                sheet.Cells[line, 4].Value = "StateType";
                sheet.Cells[line, 16].Value = GetOptionSets(stateAmd.OptionSet);
                sheet.Cells[line, 17].Value = stateAmd.OptionSet.IsGlobal ?? false ? stateAmd.OptionSet.Name : "";
                sheet.Cells[line, 18].Value = stateAmd.DefaultFormValue.HasValue ? stateAmd.DefaultFormValue.Value.ToString() : "";
            }
            else if (amd is StatusAttributeMetadata statusAmd)
            {
                sheet.Cells[line, 4].Value = "StatusType";
                sheet.Cells[line, 16].Value = GetOptionSets(statusAmd.OptionSet);
                sheet.Cells[line, 17].Value = statusAmd.OptionSet.IsGlobal ?? false ? statusAmd.OptionSet.Name : "";
                sheet.Cells[line, 18].Value = statusAmd.DefaultFormValue.HasValue ? statusAmd.DefaultFormValue.Value.ToString() : "";
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

                sheet.Cells[line, 65].Value = bamd.FormulaDefinition;
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

                sheet.Cells[line, 65].Value = decAmd.FormulaDefinition;
            }
            else if (amd is MoneyAttributeMetadata mamd)
            {
                sheet.Cells[line, 4].Value = "Money";
                sheet.Cells[line, 35].Value = mamd.Precision ?? -1;
                sheet.Cells[line, 36].Value = GetPrecisionSourceString(mamd.PrecisionSource ?? -1);
                sheet.Cells[line, 37].Value = mamd.MinValue ?? -1;
                sheet.Cells[line, 38].Value = mamd.MaxValue ?? -1;
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
                sheet.Cells[line, 40].Value = behavior;
                sheet.Cells[line, 41].Value = (dtAmd.Format ?? DateTimeFormat.DateOnly) == DateTimeFormat.DateOnly ? "Date only" : "Date and time";

                sheet.Cells[line, 65].Value = dtAmd.FormulaDefinition;
            }
            else if (amd is LookupAttributeMetadata lamd)
            {
                for (int i = 0; i < lamd.Targets.Length; i++)
                {
                    var targetEntity = lamd.Targets[i];

                    if (i != 0)
                    {
                        line++;
                        AddLine(sheet, amd, line);
                    }

                    var attr = lamd.LogicalName == "ownerid" ? (targetEntity == "systemuser" ? "owninguser" : "owningteam") : lamd.LogicalName;

                    var rel = emd.ManyToOneRelationships.FirstOrDefault(r => r.ReferencingAttribute == attr && r.ReferencedEntity == targetEntity);
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

                    sheet.Cells[line, 4].Value = lamd.Targets.Length > 1 ? "Lookup (Multi table)" : "Lookup";
                    sheet.Cells[line, 43].Value = targetEntity;
                    sheet.Cells[line, 44].Value = lamd.IsValidForAdvancedFind.Value ? "Yes" : "No";
                    sheet.Cells[line, 45].Value = rel.IsHierarchical ?? false ? "Yes" : "No";
                    sheet.Cells[line, 46].Value = behavior;
                    sheet.Cells[line, 47].Value = rel.AssociatedMenuConfiguration.Label?.UserLocalizedLabel?.Label;
                    sheet.Cells[line, 48].Value = rel.AssociatedMenuConfiguration.Group ?? AssociatedMenuGroup.Details;
                    sheet.Cells[line, 49].Value = rel.AssociatedMenuConfiguration.Order ?? -1;

                    sheet.Cells[line, 51].Value = GetCascadeText(rel.CascadeConfiguration.Assign ?? CascadeType.Cascade);
                    sheet.Cells[line, 52].Value = GetCascadeText(rel.CascadeConfiguration.Share ?? CascadeType.Cascade);
                    sheet.Cells[line, 53].Value = GetCascadeText(rel.CascadeConfiguration.Unshare ?? CascadeType.Cascade);
                    sheet.Cells[line, 54].Value = GetCascadeText(rel.CascadeConfiguration.Reparent ?? CascadeType.Cascade);
                    sheet.Cells[line, 55].Value = GetCascadeText(rel.CascadeConfiguration.Delete ?? CascadeType.Cascade, true);
                    sheet.Cells[line, 56].Value = GetCascadeText(rel.CascadeConfiguration.Merge ?? CascadeType.Cascade);

                    if ((rel.CascadeConfiguration.Assign ?? CascadeType.Cascade) == CascadeType.Cascade
                        && (rel.CascadeConfiguration.Share ?? CascadeType.Cascade) == CascadeType.Cascade
                        && (rel.CascadeConfiguration.Assign ?? CascadeType.Cascade) == CascadeType.Cascade
                        && (rel.CascadeConfiguration.Unshare ?? CascadeType.Cascade) == CascadeType.Cascade
                        && (rel.CascadeConfiguration.Reparent ?? CascadeType.Cascade) == CascadeType.Cascade
                        && (rel.CascadeConfiguration.Delete ?? CascadeType.Cascade) == CascadeType.Cascade
                        && (rel.CascadeConfiguration.Merge ?? CascadeType.Cascade) == CascadeType.Cascade)
                    {
                        sheet.Cells[line, 50].Value = "Parental";
                    }
                    else if ((rel.CascadeConfiguration.Assign ?? CascadeType.Cascade) == CascadeType.NoCascade
                       && (rel.CascadeConfiguration.Share ?? CascadeType.Cascade) == CascadeType.NoCascade
                       && (rel.CascadeConfiguration.Assign ?? CascadeType.Cascade) == CascadeType.NoCascade
                       && (rel.CascadeConfiguration.Unshare ?? CascadeType.Cascade) == CascadeType.NoCascade
                       && (rel.CascadeConfiguration.Reparent ?? CascadeType.Cascade) == CascadeType.NoCascade
                       && (rel.CascadeConfiguration.Delete ?? CascadeType.Cascade) == CascadeType.RemoveLink
                       && (rel.CascadeConfiguration.Merge ?? CascadeType.Cascade) == CascadeType.Cascade)
                    {
                        sheet.Cells[line, 50].Value = "Referential";
                    }
                    else if ((rel.CascadeConfiguration.Assign ?? CascadeType.Cascade) == CascadeType.NoCascade
                        && (rel.CascadeConfiguration.Share ?? CascadeType.Cascade) == CascadeType.NoCascade
                        && (rel.CascadeConfiguration.Assign ?? CascadeType.Cascade) == CascadeType.NoCascade
                        && (rel.CascadeConfiguration.Unshare ?? CascadeType.Cascade) == CascadeType.NoCascade
                        && (rel.CascadeConfiguration.Reparent ?? CascadeType.Cascade) == CascadeType.NoCascade
                        && (rel.CascadeConfiguration.Delete ?? CascadeType.Cascade) == CascadeType.Restrict
                        && (rel.CascadeConfiguration.Merge ?? CascadeType.Cascade) == CascadeType.Cascade)
                    {
                        sheet.Cells[line, 50].Value = "Referential, restrict delete";
                    }
                    else
                        sheet.Cells[line, 50].Value = "Custom";

                    sheet.Cells[line, 57].Value = rel.SchemaName;
                }
            }
            else if (amd is FileAttributeMetadata fileAmd)
            {
                sheet.Cells[line, 4].Value = "File";
                sheet.Cells[line, 59].Value = fileAmd.MaxSizeInKB.Value.ToString();
            }
            else if (amd is ImageAttributeMetadata imageAmd)
            {
                sheet.Cells[line, 4].Value = "Image";
                sheet.Cells[line, 61].Value = imageAmd.MaxSizeInKB ?? -1;
                sheet.Cells[line, 62].Value = imageAmd.CanStoreFullImage ?? false;
                sheet.Cells[line, 63].Value = imageAmd.IsPrimaryImage ?? false;
            }
        }
    }
}