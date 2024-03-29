using GH_IO.Serialization;
using Grasshopper.Kernel.Types;
using Rhino;
using Rhino.DocObjects;
using Rhino.FileIO;
using System;

namespace ReferenceAttributes.Kernel.Types
{
    public class GH_Layer : GH_Goo<Layer>
    {
        public GH_Layer()
        {
        }

        public GH_Layer(Layer layer) : base(layer)
        {
        }

        public GH_Layer(GH_Layer other) : base(other)
        {
        }

        public virtual Guid ReferenceID { get => Value.Id; set { Value.Id = value; } }

        public virtual bool IsReferenced => !ReferenceID.Equals(Guid.Empty);

        public override bool IsValid => (Value == null) ? false : Value.IsValid;

        public override string TypeName => "Layer";

        public override string TypeDescription => "Layer of Rhino document";

        public override string ToString()
        {
            if (Value == null)
                return "Null Layer";
            else
                return (IsReferenced ? "Referenced ":"") + $"Layer : {Value.FullPath}";
        }

        public override IGH_Goo Duplicate()
        {
            return new GH_Layer(Value);
        }

        public override object ScriptVariable()
        {
            return Value;
        }

        public override bool CastFrom(object source)
        {
            RhinoDoc doc = RhinoDoc.ActiveDoc;
            if (source is IGH_GeometricGoo geo && geo.IsReferencedGeometry)
            {
                Value = doc.Layers.FindIndex(doc.Objects.FindId(geo.ReferenceID).Attributes.LayerIndex);
                return true;
            }
            else if (source is GH_Guid guid && doc.Layers.FindId(guid.Value) is Layer layer)
            {
                Value = layer;
                return true;
            }
            else
                return false;
        }

        public override bool CastTo<Q>(ref Q target)
        {
            if (target is GH_Guid guid && IsReferenced)
            {
                guid.Value = ReferenceID;
                return true;
            }
            else
                return false;
        }

        public override bool Write(GH_IWriter writer)
        {
            writer.SetBoolean("Ref", IsReferenced);
            if (IsReferenced)
                writer.SetGuid("Guid", Value.Id);
            else
                writer.SetString("Layer", Value.ToJSON(new SerializationOptions { WriteUserData = true }));
            return true;
        }

        public override bool Read(GH_IReader reader)
        {
            bool isref = reader.GetBoolean("Ref");
            if (isref)
                Value = RhinoDoc.ActiveDoc.Layers.FindId(reader.GetGuid("Guid"));
            else
                Value = Layer.FromJSON(reader.GetString("Layer")) as Layer;
            return true;
        }
    }
}
