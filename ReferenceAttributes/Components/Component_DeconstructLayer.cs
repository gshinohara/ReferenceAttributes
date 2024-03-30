using Grasshopper.Kernel;
using ReferenceAttributes.Kernel.Parameters;
using ReferenceAttributes.Kernel.Types;
using Rhino.DocObjects;
using System;

namespace ReferenceAttributes.Components
{
    public class Component_DeconstructLayer : GH_Component
    {
        public Component_DeconstructLayer()
          : base("Deconstruct Layer", "DeconLayer",
              "",
              "Params", "Util")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new Param_Layer(), "Layer", "L", "", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Fullpath", "FP", "", GH_ParamAccess.item);
            pManager.AddTextParameter("Name", "N", "", GH_ParamAccess.item);
            pManager.AddColourParameter("Colour", "C", "", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Exsist?", "E?", "", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Layer layer = null;
            if (!DA.GetData("Layer", ref layer))
                return;
            DA.SetData("Fullpath", layer.Value.FullPath);
            DA.SetData("Name", (layer.IsReferenced) ? layer.Value.Name : Layer.GetLeafName(layer.Value));
            DA.SetData("Colour", layer.Value.Color);
            DA.SetData("Exsist?", layer.IsReferenced);
        }

        protected override System.Drawing.Bitmap Icon => null;

        public override Guid ComponentGuid => new Guid("297D1E85-4085-4C89-B8B9-C1A954D4A215");
    }
}