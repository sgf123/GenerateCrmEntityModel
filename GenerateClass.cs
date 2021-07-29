using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.CodeDom;

using System.CodeDom.Compiler;

using System.Reflection;
using Microsoft.Xrm.Sdk.Metadata; 
using GenerateCrmEntityMode;

namespace GenerateCrmEntityModel
{
    public class GenerateClass
    {
        public string filepath = string.Empty;
        public  void Generate(string className, string entityName, List<AttributeMetadataModel> attributes)
        {
            CodeCompileUnit unit = new CodeCompileUnit();
            CodeNamespace myNamespace = new CodeNamespace("Pcsd.Polaris.Common.Model.EntityModel");
            myNamespace.Imports.Add(new CodeNamespaceImport("System"));
            CodeTypeDeclaration myClass = new CodeTypeDeclaration(className);
            myClass.BaseTypes.Add("EntityBase");
            myClass.IsClass = true;
            myClass.TypeAttributes = TypeAttributes.Public;
            myNamespace.Types.Add(myClass);
            unit.Namespaces.Add(myNamespace);
            var fieldList = GenerateField(attributes).ToArray();
            myClass.Members.AddRange(fieldList);
            var propertyList = GenerateProperty(attributes).ToArray();
            myClass.Members.AddRange(propertyList);
            //添加特特性 
            var attrype = new CodePrimitiveExpression(entityName);
            myClass.CustomAttributes.Add(new CodeAttributeDeclaration(
                         new CodeTypeReference(typeof(EntityAttrAttribute)),
                           new CodeAttributeArgument(attrype)));

            CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");
            CodeGeneratorOptions options = new CodeGeneratorOptions();
            options.BracingStyle = "C";
            options.BlankLinesBetweenMembers = true;
            //输出文件路径
            string outputFile = filepath + className + ".cs";
            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(outputFile))
            {
                provider.GenerateCodeFromCompileUnit(unit, sw, options);
            }
        }


        public   List<CodeMemberProperty> GenerateProperty(List<AttributeMetadataModel> attributes)
        {

            var propertyList = new List<CodeMemberProperty>();
            foreach (var attr in attributes)
            {
                var properName = attr.AttrName;
                if (attr.AttrName.Contains("_"))
                {
                    properName = attr.AttrName.Substring(attr.AttrName.IndexOf("_") + 1);
                }

                //添加属性

                CodeMemberProperty property = new CodeMemberProperty()
                {
                    Attributes = MemberAttributes.Public,
                    Name = properName,
                    HasGet = true,
                    HasSet = true
                };
                //设置property的类型            
                switch (attr.AttrType)
                {
                    case AttributeTypeCode.Owner: property.Type = new CodeTypeReference(typeof(Guid?)); break;
                    case AttributeTypeCode.Integer: property.Type = new CodeTypeReference(typeof(int?)); break;
                    case AttributeTypeCode.Boolean: property.Type = new CodeTypeReference(typeof(bool?)); break;
                    case AttributeTypeCode.DateTime: property.Type = new CodeTypeReference(typeof(DateTime?)); break;
                    case AttributeTypeCode.Money: property.Type = new CodeTypeReference(typeof(decimal?)); break;
                    case AttributeTypeCode.Picklist: property.Type = new CodeTypeReference(typeof(int?)); break;
                    case AttributeTypeCode.String: property.Type = new CodeTypeReference(typeof(string)); break;
                    case AttributeTypeCode.Lookup: property.Type = new CodeTypeReference(typeof(Guid?)); break;
                    case AttributeTypeCode.Status: property.Type = new CodeTypeReference(typeof(int?)); break;
                    case AttributeTypeCode.Uniqueidentifier:
                        property.Type = new CodeTypeReference(typeof(Guid));
                        property.Name = "id";
                        property.Attributes = MemberAttributes.Public | MemberAttributes.Override;
                        break;
                    default:
                        continue;
                        break;
                }

                //get
                property.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), $"_{property.Name}")));

                //set

                property.SetStatements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), $"_{property.Name}"), new CodePropertySetValueReferenceExpression()));


                //set CustomAttributes
                var attrtype = new CodePrimitiveExpression(attr.AttrType.ToString());
                var attrname = new CodePrimitiveExpression(attr.AttrName.ToString());
                if (!string.IsNullOrEmpty(attr.LookUpEntityName))
                {
                    var lookupname = new CodePrimitiveExpression(attr.LookUpEntityName.ToString());
                    property.CustomAttributes.Add(new CodeAttributeDeclaration(
                      new CodeTypeReference(typeof(EntityFieldAttrAttribute)),
                      new CodeAttributeArgument(attrtype),
                      new CodeAttributeArgument(attrname),
                      new CodeAttributeArgument(lookupname)));
                }
                else
                {
                    property.CustomAttributes.Add(new CodeAttributeDeclaration(
                     new CodeTypeReference(typeof(EntityFieldAttrAttribute)),
                     new CodeAttributeArgument(attrtype),
                     new CodeAttributeArgument(attrname)));
                }

                propertyList.Add(property);



            }
            return propertyList;
        }

        public static List<CodeMemberField> GenerateField(List<AttributeMetadataModel> attributes)
        {
            var fieldList = new List<CodeMemberField>();
            foreach (var attr in attributes)
            {
                var fieldName = attr.AttrName;
                if (attr.AttrName.Contains("_"))
                {
                    fieldName = attr.AttrName.Substring(attr.AttrName.IndexOf("_") + 1);
                }

                //添加属性

                CodeMemberField field = new CodeMemberField()
                {
                    Attributes = MemberAttributes.Private,
                    Name = $"_{fieldName}"
                };
                //设置property的类型            
                switch (attr.AttrType)
                {
                    case AttributeTypeCode.Owner: field.Type = new CodeTypeReference(typeof(Guid?)); break;
                    case AttributeTypeCode.Integer: field.Type = new CodeTypeReference(typeof(int?)); break;
                    case AttributeTypeCode.Boolean: field.Type = new CodeTypeReference(typeof(bool?)); break;
                    case AttributeTypeCode.DateTime: field.Type = new CodeTypeReference(typeof(DateTime?)); break;
                    case AttributeTypeCode.Money: field.Type = new CodeTypeReference(typeof(decimal?)); break;
                    case AttributeTypeCode.Picklist: field.Type = new CodeTypeReference(typeof(int?)); break;
                    case AttributeTypeCode.String: field.Type = new CodeTypeReference(typeof(string)); break;
                    case AttributeTypeCode.Lookup: field.Type = new CodeTypeReference(typeof(Guid?)); break;
                    case AttributeTypeCode.Status: field.Type = new CodeTypeReference(typeof(int?)); break;
                    case AttributeTypeCode.Uniqueidentifier:
                        field.Type = new CodeTypeReference(typeof(Guid));
                        field.Name = "_id"; break;
                    default:
                        continue;
                        break;
                }

                fieldList.Add(field);
            }
            return fieldList;
        }

    }
}
