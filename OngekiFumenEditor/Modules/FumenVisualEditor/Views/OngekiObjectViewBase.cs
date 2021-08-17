﻿using OngekiFumenEditor.Modules.FumenVisualEditor.ViewModels;
using OngekiFumenEditor.Modules.FumenVisualEditor.ViewModels.OngekiObjects;
using OngekiFumenEditor.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static OngekiFumenEditor.Utils.StatusNotifyHelper;

namespace OngekiFumenEditor.Modules.FumenVisualEditor.Views
{
    public class OngekiObjectViewBase : UserControl
    {
        public OngekiObjectViewModelBase ViewModel => DataContext as OngekiObjectViewModelBase;

        public bool IsDragging
        {
            get { return (bool)GetValue(IsDraggingProperty); }
            set { SetValue(IsDraggingProperty, value); }
        }

        public static readonly DependencyProperty IsDraggingProperty =
            DependencyProperty.Register("IsDragging", typeof(bool), typeof(OngekiObjectViewBase), new PropertyMetadata(false));

        public bool IsMouseDown
        {
            get { return (bool)GetValue(IsMouseDownProperty); }
            set { SetValue(IsMouseDownProperty, value); }
        }

        public static readonly DependencyProperty IsMouseDownProperty =
            DependencyProperty.Register("IsMouseDown", typeof(bool), typeof(OngekiObjectViewBase), new PropertyMetadata(false));

        public bool IsPreventXAutoClose => ViewModel?.EditorViewModel?.IsPreventXAutoClose ?? false;

        public bool IsPreventTimelineAutoClose => ViewModel?.EditorViewModel?.IsPreventTimelineAutoClose ?? false;

        public OngekiObjectViewBase()
        {
            MouseDown += OnMouseDown;
            MouseMove += OnMouseMove;
            MouseUp += OnMouseUp;
            MouseLeave += OnMouseLeave;
        }

        protected virtual void OnDragStart()
        {

        }

        protected virtual void OnDragMove(Point relativePoint)
        {
            ViewModel.Y = relativePoint.Y;

            if (ViewModel.CanMoveX)
            {
                ViewModel.X = CheckAndAdjustX(relativePoint.X);
            }
        }

        public double CheckAndAdjustX(double x)
        {
            //todo 基于二分法查询最近
            var mid = ViewModel?.EditorViewModel?.XGridUnitLineLocations?.Select(z => new {
                distance = Math.Abs(z.X - x),
                x = z.X
            })?.Where(z => z.distance < 10)?.OrderBy(x => x.distance)?.ToList();
            var nearestUnitLine = mid?.FirstOrDefault();
            //Log.LogInfo($"nearestUnitLine in:{x:F2} distance:{nearestUnitLine?.distance:F2} x:{nearestUnitLine?.x:F2}");
            return nearestUnitLine != null ? nearestUnitLine.x : x;
        }

        protected virtual void OnDragEnd()
        {

        }

        protected virtual void OnMouseClick()
        {

        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            IsMouseDown = true;
            IsDragging = false;
            e.Handled = true;
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!(IsMouseDown && Parent is IInputElement parent))
                return;
            e.Handled = true;
            Action<Point> dragCall = IsDragging ? OnDragMove : _ => OnDragStart();
            IsDragging = true;

            var pos = e.GetPosition(parent);
            if (parent is Visual uiElement)
            {
                if (VisualTreeHelper.GetContentBounds(uiElement).Contains(pos))
                {
                    dragCall(pos);
                }
            }
            else
            {
                dragCall(pos);
            }
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (IsDragging)
                OnDragEnd();
            else if (IsMouseDown)
                OnMouseClick();

            IsMouseDown = false;
            IsDragging = false;
            e.Handled = true;
        }

        private void OnMouseLeave(object sender, MouseEventArgs e)
        {
            IsMouseDown = false;
            IsDragging = false;
            e.Handled = true;
        }
    }
}
