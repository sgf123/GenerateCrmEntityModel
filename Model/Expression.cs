using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GenerateCrmEntityMode
{
    public static class ExtensionsService
    {

        public static T IsNullAttr<T>(this Entity entity, string obj)
        {
            if (entity.Contains(obj))
            {
                return (T)entity[obj];
            }
            return default;
        }
        public static CreateRequest SetTargetByModel(this CreateRequest request, EntityBase model)
        {
            request.Target = model.ToEntity();
            return request;
        }
        public static UpdateRequest SetTargetByModel(this UpdateRequest request, EntityBase model)
        {
            request.Target = model.ToEntity();
            return request;
        }
        public static DeleteRequest SetTargetByModel(this DeleteRequest request, EntityBase model)
        {
            var entity = model.ToEntity();
            request.Target = new EntityReference(entity.LogicalName, entity.Id);
            return request;
        }
        public static Guid CreateByModel(this OrganizationServiceProxy service, EntityBase model)
        {
            return service.Create(model.ToEntity());
        }
        public static void UpdateByModel(this OrganizationServiceProxy service, EntityBase model)
        {
            service.Update(model.ToEntity());
        }
        public static void DeleteByModel(this OrganizationServiceProxy service, EntityBase model)
        {
            var entitytype = model.GetType();
            var attrlist = entitytype.GetCustomAttributes(false);
            var EntityAttr = (EntityAttrAttribute)attrlist.FirstOrDefault();
            service.Delete(EntityAttr.EntityName, model.id);
        }

        public static void Execute(this OrganizationServiceProxy service, List<OrganizationRequest> requests)
        {
            ExecuteTransactionRequest req = new ExecuteTransactionRequest();
            req.Requests.AddRange(requests);
            if (req.Requests.Count > 0)
            {
                service.Execute(req);
            }
        }


        public static List<AttributeMetadataModel> GetAttribute(string entityName, IOrganizationService service)
        {
            var list = new List<AttributeMetadataModel>();
            QueryExpression query = new QueryExpression(entityName);
            query.NoLock = false;
            query.TopCount = 1;
            var entity = service.RetrieveMultiple(query)[0];
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
                    AttrName = retrieveAttributeResponse.AttributeMetadata.SchemaName,
                    AttrType = retrieveAttributeResponse.AttributeMetadata.AttributeType
                };
                list.Add(attrmodel);
            }
            return list;

        }

        public static Dictionary<int, string> GetOptionsSetText(IOrganizationService service, string entityName, string attributeName)
        {
            var retrieveAttributeRequest = new
            RetrieveAttributeRequest
            {
                EntityLogicalName = entityName,
                LogicalName = attributeName,
                RetrieveAsIfPublished = true
            };
            var retrieveAttributeResponse = (RetrieveAttributeResponse)service.Execute(retrieveAttributeRequest);
            var retrievedPicklistAttributeMetadata = (PicklistAttributeMetadata)
            retrieveAttributeResponse.AttributeMetadata;
            OptionMetadata[] optionList = retrievedPicklistAttributeMetadata.OptionSet.Options.ToArray();
            var dic = new Dictionary<int, string>();
            foreach (OptionMetadata oMD in optionList)
            {
                dic.Add(oMD.Value.Value, oMD.Label.LocalizedLabels[0].Label.ToString());

            }
            return dic;
        }

        public static Entity ToEntity(this EntityBase t)
        {
            var proplist = t.GetType().GetProperties();
            var entitytype = t.GetType();
            var attrlist = entitytype.GetCustomAttributes(false);
            var EntityAttr = (EntityAttrAttribute)attrlist.FirstOrDefault();
            var entity = new Entity(EntityAttr.EntityName);
            if (t.id != null)
                entity.Id = t.id;
            foreach (var prop in proplist)
            {
                Object[] obj = prop.GetCustomAttributes(false);
                var propval = prop.GetValue(t);
                if (propval == null)
                    continue;
                foreach (Attribute a in obj)
                {
                    var attr = a as EntityFieldAttrAttribute;
                    if (attr == null) continue;
                    var attrtypecode = (AttributeTypeCode)Enum.Parse(typeof(AttributeTypeCode), attr.AttrType);
                    switch (attrtypecode)
                    {
                        case AttributeTypeCode.Boolean:
                            entity[attr.AttrName] = (bool)propval; break;
                        case AttributeTypeCode.Uniqueidentifier:
                            entity[attr.AttrName] = (Guid)propval; break;
                        case AttributeTypeCode.String:
                            var stringval = prop.GetValue(t);
                            entity[attr.AttrName] = (string)propval; break;
                        case AttributeTypeCode.Lookup:
                            entity[attr.AttrName] = new EntityReference(attr.LookUpEntityName, (Guid)propval); break;
                        case AttributeTypeCode.Picklist:
                            entity[attr.AttrName] = new OptionSetValue((int)propval
); break;
                        case AttributeTypeCode.DateTime:
                            entity[attr.AttrName] = (DateTime)propval
; break;
                        case AttributeTypeCode.Status:
                            entity[attr.AttrName] = new OptionSetValue((int)propval
); break;
                        default:
                            break;
                    }
                }
            }
            return entity;
        }

        public static T ToModel<T>(this Entity entity)
        {
            var model = (T)Activator.CreateInstance(typeof(T));
            var modelproplist = typeof(T).GetProperties();
            foreach (var key in entity.Attributes.Keys)
            {
                foreach (var modelprop in modelproplist)
                {
                    Object[] obj = modelprop.GetCustomAttributes(false);
                    var attr = (EntityFieldAttrAttribute)obj.FirstOrDefault(x => x is EntityFieldAttrAttribute);
                    if (attr == null) continue;
                    if (key == attr.AttrName.ToLower())
                    {
                        var attrtypecode = (AttributeTypeCode)Enum.Parse(typeof(AttributeTypeCode), attr.AttrType);
                        switch (attrtypecode)
                        {
                            case AttributeTypeCode.Boolean:
                                modelprop.SetValue(model, (bool)entity[key]); break;
                            case AttributeTypeCode.String:
                                modelprop.SetValue(model, (string)entity[key]); break;
                            case AttributeTypeCode.Lookup:
                                modelprop.SetValue(model, ((EntityReference)entity[key]).Id); break;
                            case AttributeTypeCode.Uniqueidentifier:
                                modelprop.SetValue(model, ((Guid)entity[key])); break;
                            case AttributeTypeCode.Picklist:
                                modelprop.SetValue(model, ((OptionSetValue)entity[key]).Value); break;
                            case AttributeTypeCode.Integer:
                                modelprop.SetValue(model, ((int)entity[key])); break;
                            case AttributeTypeCode.Owner:
                                modelprop.SetValue(model, ((EntityReference)entity[key]).Id); break;
                            default:
                                break;
                        }
                    }
                }
            }
            return model;
        }

    }
    public class ModelRequest
    {
        public enum RequestType
        {
            create, update, delete
        }
        public RequestType requestType { get; set; }
        public EntityBase model { get; set; }
    }
    public class EntityBase
    {
        public virtual Guid id { get; set; }
    }

    public class AttributeMetadataModel
    {
        public string AttrName { get; set; }
        public AttributeTypeCode? AttrType { get; set; }
        public string LookUpEntityName { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class EntityFieldAttrAttribute : Attribute
    {
        public string AttrType;
        public string AttrName;
        public string LookUpEntityName;
        public EntityFieldAttrAttribute(string attrType, string attrName, string lookUpEntityName = null)
        {
            AttrName = attrName;
            AttrType = attrType;
            LookUpEntityName = lookUpEntityName;
        }
    }
    public class EntityAttrAttribute : Attribute
    {
        public string EntityName { get; set; }
        public EntityAttrAttribute(string entityname)
        {
            EntityName = entityname;
        }
    }
    [AttributeUsage(AttributeTargets.Property)]
    public class EntityReferenceAttrAttribute : Attribute
    {
        public string LookUpEntityName;
        public EntityReferenceAttrAttribute(string lookupentityName)
        {
            LookUpEntityName = lookupentityName;
        }
    }
}
