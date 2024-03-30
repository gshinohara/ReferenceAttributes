using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using ReferenceAttributes.Kernel.Parameters;
using ReferenceAttributes.Kernel.Types;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ReferenceAttributes.Components
{
    public class Component_BakeLayer : GH_Component
    {
        private bool isready = false;

        public Component_BakeLayer()
          : base("Bake Layer", "Bake",
              "",
              "Params", "Util")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new Param_Layer(), "Layer", "L", "", GH_ParamAccess.item);
            int bake = pManager.AddBooleanParameter("Bake", "B", "", GH_ParamAccess.item);
            pManager[bake].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new Param_Layer(), "Layer", "L", "Baked layer", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Baked?", "B?", "", GH_ParamAccess.item);
        
        }

        protected override void BeforeSolveInstance()
        {
            IGH_Param param_bake = Params.Input.FirstOrDefault(p => p.Name == "Bake");
            if (param_bake.VolatileDataCount == 0)
            {
                isready = false;
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No bake solution because of no baking inputs, and the output has input data.");
            }
            else if (param_bake.VolatileDataCount > 1)
            {
                isready = false;
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Only one Bake input can be acceptable.");
            }
            else if (param_bake.VolatileData.AllData(true).FirstOrDefault(goo => goo is GH_Boolean) is GH_Boolean gh_bool)
            {
                isready = gh_bool.Value;
                if (!isready)
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No bake solution, and the output has input data.");
            }
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Layer layer = null;
            if (!DA.GetData("Layer", ref layer) || layer == null)
                return;

            bool baked = false;

            if (layer.IsReferenced)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"{layer.Value.FullPath} already exists, so it isn't bake.");
                DA.SetData("Layer", layer);
            }
            else if(isready)
            {
                LayerTable layers = Rhino.RhinoDoc.ActiveDoc.Layers;
                List<Layer> layers_to_bake = new List<Layer>();
                string current = layer.Value.Name;
                while (true)
                {
                    if (current == string.Empty)
                        break;

                    string leaf = Layer.GetLeafName(current);
                    current = Layer.GetParentName(current); //parent of the leaf
                    layers_to_bake.Add(new Layer { Name = leaf });

                    //When found a parent of the leaf
                    if (layers.FindByFullPath(current, -1) is int index && index != -1)
                    {
                        //last was just defined as the leaf.
                        if (layers_to_bake.LastOrDefault() is Layer last && layers.FindIndex(index) is Layer parent_of_last)
                            last.ParentLayerId = parent_of_last.Id;
                        break;
                    }
                }

                layers_to_bake.Reverse();

                Layer bakedlayer = null;
                bool successor = false;
                foreach (Layer layer_to_bake in layers_to_bake)
                {
                    if (successor && layers.LastOrDefault() is Layer parent)
                        layer_to_bake.ParentLayerId = parent.Id;
                    bakedlayer = layers.FindIndex(layers.Add(layer_to_bake));
                    successor = true;
                }

                if (bakedlayer == null)
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Baking wasn't be solved for some reasons.");
                else
                {
                    layers.Modify(layer.Value, bakedlayer.Id, true);
                    baked = true;
                }
                DA.SetData("Layer", new GH_Layer(bakedlayer));
            }
            else
                DA.SetData("Layer", layer);

            DA.SetData("Baked?", baked);
        }

        protected override System.Drawing.Bitmap Icon => null;

        public override Guid ComponentGuid => new Guid("875F66C1-8F75-4FDE-AA42-3E94865FCFD7");
    }
}