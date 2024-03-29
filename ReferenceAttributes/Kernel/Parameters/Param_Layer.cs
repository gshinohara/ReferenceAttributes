using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using ReferenceAttributes.Kernel.Types;
using ReferenceAttributes.Utility;
using Rhino;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;
using Rhino.Input.Custom;
using Rhino.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace ReferenceAttributes.Kernel.Parameters
{
    public class Param_Layer : GH_PersistentParam<GH_Layer>
    {
        public Param_Layer() : base(new GH_InstanceDescription("Layer", "Layer", "Contains a collection of Layer", "Params", "Primitive"))
        {
        }

        public override void AddedToDocument(GH_Document document)
        {
            RhinoDoc.LayerTableEvent += RhinoDoc_LayerTableEvent;
            RhinoDoc.ModifyObjectAttributes += RhinoDoc_ModifyObjectAttributes;
            base.AddedToDocument(document);
        }

        public override void RemovedFromDocument(GH_Document document)
        {
            RhinoDoc.LayerTableEvent -= RhinoDoc_LayerTableEvent;
            RhinoDoc.ModifyObjectAttributes -= RhinoDoc_ModifyObjectAttributes;
            base.RemovedFromDocument(document);
        }

        private void RhinoDoc_LayerTableEvent(object sender, LayerTableEventArgs e)
        {
            bool compute = false;
            switch (e.EventType)
            {
                case LayerTableEventType.Deleted:
                case LayerTableEventType.Modified:
                    compute = VolatileData.AllData(true).Any(goo => goo is GH_Layer layer && layer.Value.Index == e.OldState.Index);
                    break;
                default:
                    break;
            }
            if (compute)
                ExpireSolution(true);
        }

        private void RhinoDoc_ModifyObjectAttributes(object sender, Rhino.DocObjects.RhinoModifyObjectAttributesEventArgs e)
        {
            bool compute = Sources.Any(src =>
            {
                if (e.OldAttributes.LayerIndex == e.NewAttributes.LayerIndex)
                    return false;
                foreach (IGH_Goo goo in src.VolatileData.AllData(true))
                {
                    if (goo is IGH_GeometricGoo geogoo && geogoo.ReferenceID == e.RhinoObject.Id)
                        return true;
                }
                return false;
            });
            if (compute)
                ExpireSolution(true);
        }

        public override Guid ComponentGuid => new Guid("FB8D2AEA-254B-4A48-92A3-BE32F95A965C");

        protected override GH_GetterResult Prompt_Singular(ref GH_Layer value)
        {
            var layers = RhinoDoc.ActiveDoc.Layers;
            LayerSelector layerSelector = new LayerSelector(layers, false);
            if (layerSelector.ShowModal(RhinoEtoApp.MainWindow) && layerSelector.Layers.Any())
                value = new GH_Layer(layerSelector.Layers.FirstOrDefault());

            return (value == null) ? GH_GetterResult.cancel : GH_GetterResult.success;
        }

        protected override GH_GetterResult Prompt_Plural(ref List<GH_Layer> values)
        {
            LayerSelector layerSelector = new LayerSelector(RhinoDoc.ActiveDoc.Layers, true);
            if (layerSelector.ShowModal(RhinoEtoApp.MainWindow) && layerSelector.Layers.Any())
                values = layerSelector.Layers.Select(layer => new GH_Layer(layer)).ToList();

            return (values == null || values.Count == 0) ? GH_GetterResult.cancel : GH_GetterResult.success;
        }

        protected override void Menu_AppendPromptOne(ToolStripDropDown menu)
        {
            base.Menu_AppendPromptOne(menu);
            Menu_AppendItem(menu, "Set one Geometry for reference", SetGeometry, SourceCount == 0, @checked: false);
        }

        private void SetGeometry(object sender, EventArgs e)
        {
            PrepareForPrompt();

            GetObject getObject = new GetObject();
            getObject.SetCommandPrompt("Set one Geometry for reference");
            getObject.AcceptNothing(true);
            getObject.AcceptUndo(true);
            switch (getObject.Get())
            {
                case Rhino.Input.GetResult.Object:
                    Layer layer = RhinoDoc.ActiveDoc.Layers.FindIndex(getObject.Object(0).Object().Attributes.LayerIndex);
                    PersistentData.Clear();
                    if (layer != null)
                        SetPersistentData(new GH_Layer(layer));
                    break;
                default:
                    break;
            }

            RecoverFromPrompt();
            OnPingDocument()?.ClearReferenceTable();
            ExpireSolution(recompute: true);
        }

        protected override void Menu_AppendPromptMore(ToolStripDropDown menu)
        {
            base.Menu_AppendPromptMore(menu);
            Menu_AppendItem(menu, "Set Multiple Geometries for reference", SetGeometries, SourceCount == 0, @checked: false);
        }

        private void SetGeometries(object sender, EventArgs e)
        {
            PrepareForPrompt();

            GetObject getObject = new GetObject();
            getObject.SetCommandPrompt("Set Multiple Geometries for reference");
            getObject.AcceptNothing(true);
            getObject.AcceptUndo(true);
            switch (getObject.GetMultiple(1, 0))
            {
                case Rhino.Input.GetResult.Object:
                    IEnumerable<Layer> layers = getObject.Objects().Select(o => RhinoDoc.ActiveDoc.Layers.FindIndex(o.Object().Attributes.LayerIndex));
                    PersistentData.Clear();
                    if (layers.Any())
                        PersistentData.AppendRange(layers.Select(layer => new GH_Layer(layer)));
                    OnObjectChanged(GH_ObjectEventType.PersistentData);
                    break;
                default:
                    break;
            }

            RecoverFromPrompt();
            OnPingDocument()?.ClearReferenceTable();
            ExpireSolution(recompute: true);
        }
    }
}