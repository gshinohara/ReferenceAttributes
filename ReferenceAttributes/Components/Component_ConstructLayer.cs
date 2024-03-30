using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using ReferenceAttributes.Kernel.Parameters;
using ReferenceAttributes.Kernel.Types;
using Rhino.DocObjects;
using System;
using System.Drawing;

namespace ReferenceAttributes.Components
{
    public class Component_ConstructLayer : GH_Component
    {
        public Component_ConstructLayer()
          : base("Construct Layer", "ConLayer",
              "",
              "Params", "Util")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Fullpath", "FP", "", GH_ParamAccess.item);
            pManager.AddColourParameter("Colour", "C", "", GH_ParamAccess.item, Color.Black);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new Param_Layer(), "Layer", "L", "", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string fullpath = default;
            Color color = default;

            if (!DA.GetData("Fullpath", ref fullpath) || !DA.GetData("Colour", ref color))
                return;

            Layer layer = new Layer
            {
                Name = fullpath,
                Color = color
            };

            DA.SetData("Layer", new GH_Layer(layer));
        }

        protected override System.Drawing.Bitmap Icon => null;

        public override Guid ComponentGuid => new Guid("5B102D22-D2E2-4B4A-ADDD-472C7B86646A");
    }
}