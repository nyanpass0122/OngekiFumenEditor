﻿using Caliburn.Micro;
using Gemini.Modules.Toolbox;
using OngekiFumenEditor.Base;
using OngekiFumenEditor.Base.EditorObjects.LaneCurve;
using OngekiFumenEditor.Base.OngekiObjects.Beam;
using OngekiFumenEditor.Base.OngekiObjects.ConnectableObject;
using OngekiFumenEditor.Base.OngekiObjects.Lane;
using OngekiFumenEditor.Base.OngekiObjects.Wall;
using OngekiFumenEditor.Modules.FumenObjectPropertyBrowser.ViewModels.DropActions;
using OngekiFumenEditor.Modules.FumenObjectPropertyBrowser.Views;
using OngekiFumenEditor.Modules.FumenVisualEditor.Base;
using OngekiFumenEditor.Modules.FumenVisualEditor.Base.DropActions;
using OngekiFumenEditor.Modules.FumenVisualEditor.ViewModels;
using OngekiFumenEditor.Modules.FumenVisualEditor.ViewModels.OngekiObjects;
using OngekiFumenEditor.Utils.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static OngekiFumenEditor.Modules.FumenObjectPropertyBrowser.ViewModels.ConnectableObjectOperationViewModel;

namespace OngekiFumenEditor.Modules.FumenObjectPropertyBrowser.ViewModels
{
    [MapToView(ViewType = typeof(MultiLanesOperationView))]
    public class MultiLanesOperationViewModel : PropertyChangedBase
    {
        private readonly ConnectableChildObjectBase frontChild;
        private readonly ConnectableStartObject laterStart;

        /**
            frontStart  frontChild
            o-----------o

                        o endDummy

                        o midChild
                            
                        o--------o---------o
                        laterStart
        */

        public MultiLanesOperationViewModel(ConnectableChildObjectBase frontChild, ConnectableStartObject laterStart)
        {
            this.frontChild = frontChild;
            this.laterStart = laterStart;
        }

        public void OnClick(ActionExecutionContext e)
        {
            if (IoC.Get<IFumenObjectPropertyBrowser>().Editor is not FumenVisualEditorViewModel editor)
                return;

            var frontStart = frontChild.ReferenceStartObject;
            var midChild = frontStart.CreateNextObject();
            var endDummy = default(ConnectableChildObjectBase);
            var frontEnd = default(ConnectableChildObjectBase);

            editor.UndoRedoManager.ExecuteAction(LambdaUndoAction.Create("合并轨道", () =>
            {
                midChild.XGrid = laterStart.XGrid.CopyNew();
                midChild.TGrid = laterStart.TGrid.CopyNew();

                if (frontStart.Children.OfType<ConnectableEndObject>().FirstOrDefault() is ConnectableChildObjectBase _end)
                {
                    frontEnd = _end;

                    endDummy = frontStart.CreateNextObject();
                    endDummy.XGrid = frontEnd.XGrid.CopyNew();
                    endDummy.TGrid = frontEnd.TGrid.CopyNew();

                    frontStart.RemoveChildObject(frontEnd);
                    frontStart.AddChildObject(endDummy);
                }

                frontStart.AddChildObject(midChild);

                foreach (var laterChild in laterStart.Children.ToArray())
                {
                    laterStart.RemoveChildObject(laterChild);
                    laterChild.CacheRecoveryChildIndex = -1;
                    frontStart.AddChildObject(laterChild);
                }

                editor.Fumen.RemoveObject(laterStart);
            }, () =>
            {
                if (endDummy is not null)
                {
                    frontStart.RemoveChildObject(endDummy);
                    frontEnd.CacheRecoveryChildIndex = -1;
                    frontStart.AddChildObject(frontEnd);
                }

                var next = midChild.NextObject;
                while (next != null)
                {
                    frontStart.RemoveChildObject(next);
                    next.CacheRecoveryChildIndex = -1;
                    laterStart.AddChildObject(next);
                    next = next.NextObject;
                }
                frontStart.RemoveChildObject(midChild);
                editor.Fumen.AddObject(laterStart);
            }));
        }
    }
}