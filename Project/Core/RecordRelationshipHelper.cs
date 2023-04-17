using Sharara.EntityCodeGen.Core.Fields;
using FT = Sharara.EntityCodeGen.Core.Fields.Types.FieldType;

namespace Sharara.EntityCodeGen.Core
{
    internal class RecordRelationshipHelper
    {
        private readonly Schema schema;
        private readonly Dictionary<RecordEntity, IList<SimplePointer>> simplePointersByRecord = new();
        private readonly Dictionary<RecordEntity, IList<SelfPointer>> selfPointersByRecord = new();
        private readonly Dictionary<SimplePointer, ManyToManyPointer> many2ManyBySimple = new();
        private readonly Dictionary<SimplePointer, SelfPointer> selfBySimple = new();

        internal RecordRelationshipHelper(Schema schema)
        {
            this.schema = schema;
            foreach (RecordEntity record in schema.Records)
            {
                (IList<SimplePointer> simplePointers, SelfPointer? selfPointer) = FindPointers(record);
                if (simplePointers.Count > 0)
                {
                    simplePointersByRecord.Add(record, simplePointers);
                }
            }
        }

        internal void ProcessPointers()
        {
            MakeManyToManyPointers();
        }

        private void MakeManyToManyPointers()
        {
            foreach (var (primaryRecord, primaryRecordPointers) in simplePointersByRecord)
            {
                var toManyPointers = primaryRecordPointers.Where(r => r.TargetCount == TargetCount.Many).ToList();
                foreach (SimplePointer toManyPointer in toManyPointers)
                {
                    IList<SimplePointer> inverseToManyPointers = FindToManyPointer(toManyPointer.TargetRecord, primaryRecord);
                    switch (inverseToManyPointers.Count)
                    {
                        case 0:
                            break;

                        case 1:
                            primaryRecordPointers.Remove(toManyPointer);
                            var many2Many = new ManyToManyPointer(toManyPointer.Field, inverseToManyPointers[0].Field);
                            many2ManyBySimple.Add(toManyPointer, many2Many);
                            many2ManyBySimple.Add(inverseToManyPointers[0], many2Many);
                            break;

                        default:
                            throw new Exception($"Cannot map the many-many relationship between {primaryRecord.Name} and {toManyPointer.TargetRecord.Name}");
                    }
                }
            }
        }

        private void ProcessSelfPointers()
        {
            foreach (var (record, pointers) in selfPointersByRecord)
            {
            }
        }

        /// Finds list references (if they exists) from pointerRecord to targetRecord
        private IList<SimplePointer> FindToManyPointer(RecordEntity pointerRecord, RecordEntity targetRecord)
        {
            if (!simplePointersByRecord.ContainsKey(pointerRecord))
            {
                return Array.Empty<SimplePointer>();
            }
            IList<SimplePointer> pointers = simplePointersByRecord[pointerRecord];
            return pointers.Where(p => p.TargetCount == TargetCount.Many && p.TargetRecord == targetRecord).ToList();
        }

        record class Selfie(TargetCount TargetCount, Field Field);
        /// Finds all pointers from the pointerRecord
        private (IList<SimplePointer>, SelfPointer?) FindPointers(RecordEntity pointerRecord)
        {
            HashSet<SimplePointer> simplePointers = new();
            List<Selfie> temp = new();
            foreach (Field field in pointerRecord.Fields)
            {
                RecordEntity? targetRecord = GetTargetRecord(field);
                if (targetRecord == null)
                {
                    continue;
                }

                var targetCount = field is ListField listField ? TargetCount.Many : TargetCount.One;
                if (pointerRecord == targetRecord)
                {
                   temp.Add(new Selfie(targetCount, field));
                }
                else
                {
                    simplePointers.Add(new SimplePointer(targetCount, field, targetRecord));
                }
            }

            SelfPointer? selfPointer = temp.Count switch {
                0 => null,
                2 => new SelfPointer(temp.Where(t => t.TargetCount == TargetCount.One).First().Field,
                                (ListField)temp.Where(t => t.TargetCount == TargetCount.Many).First().Field),
                _ => throw new Exception("A self reference requires 2 inverse properties, e.g. 'Parent' and 'Children'")
            };

            return (simplePointers.ToList(), selfPointer);
        }

        /// Returns the Record which is referred to by this field or null if field is not a Ref
        private RecordEntity? GetTargetRecord(Field field)
        {
            if (FT.FieldTypeId.Entity == field.FieldType.Id)
            {
                var entity = ((FT.Entity)field.FieldType).GetEntity();
                if (entity.EntityType == EntityType.Record)
                {
                    return entity as RecordEntity;
                }
            }
            else if (FT.FieldTypeId.List == field.FieldType.Id)
            {
                var listField = (ListField)field;
                var entity = listField.FieldType.ItemType.GetEntity();
                if (entity.EntityType == EntityType.Record)
                {
                    return entity as RecordEntity;
                }
            }
            return null;
        }
    }

    internal enum TargetCount { One, Many };

    internal interface IPointer { }

    internal record class SimplePointer(TargetCount TargetCount, Field Field, RecordEntity TargetRecord) : IPointer
    {
    }

    internal record class ManyToManyPointer(Field Field, Field InverseField) : IPointer
    {
    }

    internal record class SelfPointer(Field Field, ListField InverseField) : IPointer
    {
    }
}
