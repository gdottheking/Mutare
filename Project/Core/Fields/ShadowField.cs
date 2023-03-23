namespace Sharara.EntityCodeGen.Core.Fields
{
    record ShadowField(Field Pointer, Field Target)
    {
        public string Name => Pointer.Name + Target.Name;
        public override string ToString() => Name;
    }
}