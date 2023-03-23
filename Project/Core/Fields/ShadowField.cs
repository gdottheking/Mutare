namespace Sharara.EntityCodeGen.Core.Fields
{
    record class ShadowField(string Name, Field Pointer, Field Target)
    {
        public override string ToString() => Name;
    }

}