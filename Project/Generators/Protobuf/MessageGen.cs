using Sharara.EntityCodeGen.Core;
using Sharara.EntityCodeGen.Core.Fields;
using Sharara.EntityCodeGen.Core.Rpc;

namespace Sharara.EntityCodeGen.Generators.Protobuf
{
    internal class MessageGen : IEntityVisitor, IFieldVisitor
    {
        private CodeWriter codeWriter;
        private Service service;

        public MessageGen(Service service, CodeWriter codeWriter)
        {
            this.codeWriter = codeWriter;
            this.service = service;
        }

        public void Generate()
        {
            var lines = new string[] {
                "syntax = \"proto3\";",
                $"option csharp_namespace = \"{service.Schema.Configuration.CSharpNamespace}.Proto\";",
                $"package {service.Schema.Configuration.ProtoPackage};"
            };

            foreach (var line in lines)
            {
                codeWriter.WriteLine(line);
            }

            codeWriter.WriteLine();

            foreach (var entity in service.Schema.Entities)
            {
                entity.Accept(this);
                codeWriter.WriteLine();
            }

            codeWriter.WriteLines(
                "",
                "// ----------------------------------------------",
                "//                       RPC                    //",
                "// ----------------------------------------------",
                ""
            );

            RpcServiceGen rpcServiceGen = new RpcServiceGen();
            rpcServiceGen.GenerateRpcService(service, codeWriter);

            codeWriter.Flush();
        }

        public void VisitRecord(RecordEntity entity)
        {
            codeWriter.WriteLine($"message {entity.Name} {{");
            codeWriter.Indent();

            foreach (var field in entity.fields)
            {
                field.Accept(this);
            }

            codeWriter.UnIndent();
            codeWriter.WriteLine("}");
            codeWriter.UnIndent();
            codeWriter.Flush();
        }

        public void VisitEnum(EnumEntity entity)
        {
            codeWriter.WriteLine($"enum {entity.Name} {{");
            codeWriter.Indent();

            for (int i = 0; i < entity.Values.Count; i++)
            {
                var val = entity.Values[i];
                val.Accept(this, i, entity.Values.Count);
            }

            codeWriter.UnIndent();
            codeWriter.WriteLine("}");
            codeWriter.Flush();
        }

        public void VisitField(Field field)
        {
            throw new NotImplementedException();
        }

        private string MapToGrpcType(FieldType fieldType)
        {
            return fieldType switch
            {
                FieldType.DateTime x => x.GrpcType,
                FieldType.Float64 x => x.GrpcType,
                FieldType.Int64 x => x.GrpcType,
                FieldType.Int32 x => x.GrpcType,
                FieldType.String x => x.GrpcType,
                FieldType.EntityRef entRef => entRef.Entity.Name,
                FieldType.EntityNameRef nameRef =>
                    nameRef.ResolvedEntity?.Name ??
                    throw new InvalidOperationException("Unresolved entity " + nameRef.EntityName),
                FieldType.List lst => $"repeated ${MapToGrpcType(lst.ItemType)}",
                _ => throw new InvalidOperationException($"{fieldType} does not have a matching protobuf type")
            };
        }

        private void WriteField(string fieldName, int protoId, string protoType)
        {
            codeWriter.WriteLine($"{protoType} {fieldName} = {protoId};");
        }

                void WriteField(Field field)
        {
            WriteField(field.Name, field.ProtoId, MapToGrpcType(field.FieldType));
        }

        public void VisitStringField(StringField field)
        {
            WriteField(field);
        }

        public void VisitInt64Field(Int64Field field)
        {
            WriteField(field);
        }

        public void VisitInt32Field(Int32Field field)
        {
            WriteField(field);
        }

        public void VisitFloat64Field(Float64Field field)
        {
            WriteField(field);
        }

        public void VisitDateTimeField(DateTimeField field)
        {
            WriteField(field);
        }

        public void VisitReferenceField(ReferenceField field)
        {
            Entity refEntity;
            if (field.FieldType is FieldType.EntityRef etyRef)
            {
                refEntity = etyRef.Entity;
            }
            else if (field.FieldType is FieldType.EntityNameRef nameRef)
            {
                if (!service.Schema.HasEntityName(nameRef.EntityName))
                {
                    throw new InvalidOperationException($"Unknown entity: {nameRef.EntityName}");
                }
                refEntity = service.Schema.GetEntityByName(nameRef.EntityName);
            }
            else
            {
                throw new NotImplementedException();
            }

            if (refEntity is RecordEntity refRecord)
            {
                int offset = 0;
                foreach (var fkField in refRecord.Keys())
                {
                    string fieldType = MapToGrpcType(fkField.FieldType);
                    string fieldName = field.Name + fkField.Name;
                    int protoId = field.ProtoId + offset++;
                    codeWriter.WriteLine($"{fieldType} {fieldName} = {protoId};");
                }
            }
            else if (refEntity is EnumEntity refEnum)
            {
                codeWriter.WriteLine($"{refEnum.Name} {field.Name} = {field.ProtoId};");
            }
        }

        public void VisitEnumValue(EnumValue value, int offset, int count)
        {
            codeWriter.WriteLine($"{value.Name} = {value.Value};");
        }

        public void VisitListField(ListField listField)
        {
            codeWriter.Write("repeated ");
            var type = (FieldType.List) listField.FieldType;
            WriteField(listField.Name, listField.ProtoId, MapToGrpcType(type.ItemType));
        }
    }
}