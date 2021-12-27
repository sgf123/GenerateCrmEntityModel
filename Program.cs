using GenerateCrmEntityMode;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Collections.Generic;
using System.Configuration;
namespace GenerateCrmEntityModel
{
    class Program
    {
        static void Main(string[] args)
        {
            //1.2.3.4.5.7.8
            //1.2.3.5.6.7.8.9
            //1.2.3.4.5
            //1.2.3.5.6.7.8.9.a.b
            Generate("entityname", "modelname");
            Console.WriteLine("success!"); 
            Console.WriteLine("success!");
            Console.WriteLine("success!");
            Console.ReadLine();
        }

        /// <summary>
        /// 生成
        /// </summary>
        /// <param name="entityname">entity name</param>
        /// <param name="modelname">model name</param>
        static void Generate(string entityname, string modelname)
        {
            using (CrmServiceClient cli = new CrmServiceClient(ConfigurationManager.ConnectionStrings["CrmDev"].ConnectionString))
            {
                if (cli.IsReady)
                {
                    var service = cli.OrganizationServiceProxy;
                    var data = GetAttribute(entityname, service);

                    var generatehelp = new GenerateClass() { filepath = @"D:\dynamics\GenerateCrmEntityModel\Model\EntityModel\" };
                    generatehelp.Generate($"{modelname}DO", entityname, data);
                }
            }
        }

        /// <summary>
        /// 根据现有一条记录返回的attr,创建对应的字段（有可能attr没值，没返回的情况）
        /// 如下：	cf640b3a-e1c9-eb11-bacc-000d3aa2a49a
        /// </summary>
        public static List<AttributeMetadataModel> GetAttribute(string entityName, IOrganizationService service)
        {
            var list = new List<AttributeMetadataModel>();
            var entity = service.Retrieve(entityName, Guid.Parse("cf640b3a-e1c9-eb11-bacc-000d3aa2a49a"), new ColumnSet(true));

            foreach (var attr in entity.Attributes)
            {
                var retrieveAttributeRequest = new
           RetrieveAttributeRequest
                {
                    EntityLogicalName = entityName,
                    LogicalName = attr.Key,
                    RetrieveAsIfPublished = true
                };
                var retrieveAttributeResponse = (RetrieveAttributeResponse)service.Execute(retrieveAttributeRequest);
                var attrmodel = new AttributeMetadataModel()
                {
                    AttrName = retrieveAttributeResponse.AttributeMetadata.LogicalName,
                    AttrType = retrieveAttributeResponse.AttributeMetadata.AttributeType

                };
                if (retrieveAttributeResponse.AttributeMetadata is LookupAttributeMetadata)
                {
                    attrmodel.LookUpEntityName = ((LookupAttributeMetadata)retrieveAttributeResponse.AttributeMetadata).Targets[0];
                }
                list.Add(attrmodel);
            }
            return list;
        }
    }
}
