using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DragControl
{
    /// <summary>
    /// 按照步骤 1a 或 1b 操作，然后执行步骤 2 以在 XAML 文件中使用此自定义控件。
    ///
    /// 步骤 1a) 在当前项目中存在的 XAML 文件中使用该自定义控件。
    /// 将此 XmlNamespace 特性添加到要使用该特性的标记文件的根
    /// 元素中:
    ///
    ///     xmlns:MyNamespace="clr-namespace:DragControl"
    ///
    ///
    /// 步骤 1b) 在其他项目中存在的 XAML 文件中使用该自定义控件。
    /// 将此 XmlNamespace 特性添加到要使用该特性的标记文件的根
    /// 元素中:
    ///
    ///     xmlns:MyNamespace="clr-namespace:DragControl;assembly=DragControl"
    ///
    /// 您还需要添加一个从 XAML 文件所在的项目到此项目的项目引用，
    /// 并重新生成以避免编译错误:
    ///
    ///     在解决方案资源管理器中右击目标项目，然后依次单击
    ///     “添加引用”->“项目”->[浏览查找并选择此项目]
    ///
    ///
    /// 步骤 2)
    /// 继续操作并在 XAML 文件中使用控件。
    ///
    ///     <MyNamespace:Legend/>
    ///
    /// </summary>
    public class Legend : Control
    {
        static Legend()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Legend), new FrameworkPropertyMetadata(typeof(Legend)));
        }

        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            //      if (e.LeftButton == MouseButtonState.Pressed)
            //      {
            //       ((Border)VTH.GetChild(VTH.GetChild(this, 0), 0)).Margin = 
            //		new Thickness(-Canvas.GetLeft(this), -Canvas.GetTop(this), 0, 0);
            //}

            base.OnPreviewMouseMove(e);
        }

        //public Brush Blur
        //{
        //    get { return (Brush)GetValue(BlurProperty); }
        //    set { SetValue(BlurProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for Blur.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty BlurProperty =
        //    DependencyProperty.Register("Blur", typeof(Brush), typeof(Legend), new PropertyMetadata(null));
    }

    public class CustomThumb : Thumb
    {
        public DragDirection DragDirection { get; set; }
    }
    public class DragChangedEventArgs : RoutedEventArgs
    {
        public DragChangedEventArgs(RoutedEvent Event, Rect NewBound, object Target = null) : base(Event)
        {
            this.NewBound = NewBound;
            DragTargetElement = Target;
        }
        public Rect NewBound { get; private set; }

        public object DragTargetElement { get; private set; }
    }
    public delegate void DragChangedEventHandler(object Sender, DragChangedEventArgs e);
    public class DragControlHelper : DragHelperBase
    {
        #region Cotr & Events
        public DragControlHelper()
        {
            this.Loaded += OnLoaded;
            this.Unloaded += OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            AttachParentEvents();
            this.Loaded -= OnLoaded;
        }
        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            DetachParentEvents();
            this.Unloaded -= OnUnloaded;
        }
        #endregion

        #region DependencyProperty

        #region IsEditable
        public static bool GetIsEditable(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsEditableProperty);
        }

        public static void SetIsEditable(DependencyObject obj, bool value)
        {
            obj.SetValue(IsEditableProperty, value);
        }

        // Using a DependencyProperty as the backing store for IsEditable.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsEditableProperty =
            DependencyProperty.RegisterAttached("IsEditable", typeof(bool), typeof(DragHelperBase), new PropertyMetadata(false));
        #endregion

        #region IsSelectable
        public static bool GetIsSelectable(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsSelectableProperty);
        }

        public static void SetIsSelectable(DependencyObject obj, bool value)
        {
            obj.SetValue(IsSelectableProperty, value);
        }

        // Using a DependencyProperty as the backing store for IsSelectable.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsSelectableProperty =
            DependencyProperty.RegisterAttached("IsSelectable", typeof(bool), typeof(DragHelperBase), new PropertyMetadata(false));
        #endregion

        #region TargetElement
        public FrameworkElement TargetElement
        {
            get { return (FrameworkElement)GetValue(TargetElementProperty); }
            set { SetValue(TargetElementProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TargetElement.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TargetElementProperty =
            DependencyProperty.Register("TargetElement", typeof(FrameworkElement), typeof(DragControlHelper),
                                        new FrameworkPropertyMetadata(default(FrameworkElement),
                                            new PropertyChangedCallback(TargetElementChanged)));
        #endregion

        #region TargetElementChanged
        private static void TargetElementChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DragControlHelper Helper = d as DragControlHelper;

            if (Helper == null)
            {
                return;
            }

            FrameworkElement Target = e.OldValue as FrameworkElement;

            Helper.DetachTatgetEvents(Target);

            Target = e.NewValue as FrameworkElement;

            if (Target == null
                || Target is DragControlHelper
                || Target.RenderSize.IsEmpty
                || double.IsNaN(Target.RenderSize.Width)
                || double.IsNaN(Target.RenderSize.Height)
                || !GetIsSelectable(Target))
            {
                Helper.Visibility = Visibility.Collapsed;
                return;
            }

            Helper.AttachTatgetEvents(Target);

            int Zindex = Panel.GetZIndex(Target);

            if (Zindex >= Panel.GetZIndex(Helper))
            {
                Panel.SetZIndex(Helper, Zindex + 1);
            }

            double Y = CorrectDoubleValue(Canvas.GetTop(Target));
            double X = CorrectDoubleValue(Canvas.GetLeft(Target));

            Canvas.SetTop(Helper, Y);
            Canvas.SetLeft(Helper, X);
            Helper.Width = Target.ActualWidth;
            Helper.Height = Target.ActualHeight;
        }
        #endregion

        #endregion

        #region Parent Event Handler

        #region AttachParentEvents
        private void AttachParentEvents()
        {
            Canvas CanvasParent = Parent as Canvas;

            if (CanvasParent == null)
            {
                throw new Exception("DragControlHelper Must place into Canvas!");
            }

            CanvasParent.MouseLeftButtonDown += OnParentMouseLeftButtonDown;
        }
        #endregion

        #region DetachParentEvents
        private void DetachParentEvents()
        {
            Canvas CanvasParent = Parent as Canvas;

            if (CanvasParent != null)
            {
                CanvasParent.MouseLeftButtonDown -= OnParentMouseLeftButtonDown;
            }
        }
        #endregion

        #region OnParentMouseLeftButtonDown
        private void OnParentMouseLeftButtonDown(object Sender, MouseButtonEventArgs e)
        {
            FrameworkElement SelectedElement = e.OriginalSource as FrameworkElement;

            if (CheckTargetIsSelectable(SelectedElement))
            {
                TargetElement = SelectedElement;
            }
            else
            {
                TargetElement = null;
            }

            SelectedElement.Focus();
            Visibility = Visibility.Visible;
        }
        #endregion

        #region CheckTargetIsSelectable
        private bool CheckTargetIsSelectable(FrameworkElement Target)
        {
            return (Target != null) && !Target.Equals(Parent) && !Target.Equals(this) && GetIsSelectable(Target);
        }
        #endregion

        #endregion

        #region Target Event Handler

        #region AttachTatgetEvents
        private void AttachTatgetEvents(FrameworkElement Target)
        {
            if (Target == null)
            {
                throw new ArgumentNullException("Target");
            }

            Target.Focusable = true;
            Target.GotFocus += TargetElement_GotFocus;
            Target.LostFocus += TargetElement_LostFocus;
            //Target.PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;


            double Thickness = 1.0;

            if (Target is Shape)
            {
                Thickness = (Target as Shape).StrokeThickness;
            }

            bool CanEdit = GetIsEditable(Target);

            SetupVisualPropertes(Thickness, CanEdit);
        }
        #endregion

        #region DetachTatgetEvents
        private void DetachTatgetEvents(FrameworkElement Target)
        {
            if (Target != null)
            {
                Target.GotFocus -= TargetElement_GotFocus;
                Target.LostFocus -= TargetElement_LostFocus;
                //Target.PreviewMouseLeftButtonDown -= OnPreviewMouseLeftButtonDown;

            }
        }
        #endregion

        #region TargetElement_GotFocus
        private void TargetElement_GotFocus(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Visible;
        }
        #endregion

        #region TargetElement_LostFocus
        private void TargetElement_LostFocus(object sender, RoutedEventArgs e)
        {
            TargetElement = null;
            Visibility = Visibility.Collapsed;
        }
        #endregion

        #endregion

        #region DragHelperBase Member

        #region GetTargetActualBound
        protected override Rect GetTargetActualBound()
        {
            if (TargetElement == null)
            {
                return Rect.Empty;
            }

            double Top = CorrectDoubleValue(Canvas.GetTop(TargetElement));
            double Left = CorrectDoubleValue(Canvas.GetLeft(TargetElement));



            return new Rect
            {
                X = Left,
                Y = Top,
                Width = TargetElement.ActualWidth,
                Height = TargetElement.ActualHeight
            };
        }
        #endregion

        #region SetTargetActualBound
        protected override void SetTargetActualBound(Rect NewBound)
        {
            if (TargetElement != null)
            {
                TargetElement.Width = NewBound.Width;
                TargetElement.Height = NewBound.Height;
                Canvas.SetTop(TargetElement, NewBound.Y);
                Canvas.SetLeft(TargetElement, NewBound.X);
            }
        }
        #endregion

        #region RaisenDragCompletedEvent
        protected override void RaisenDragCompletedEvent(Rect NewBound)
        {
            RaiseEvent(new DragChangedEventArgs(DragCompletedEvent, NewBound, TargetElement));
        }

        protected override void RaisenDragChangingEvent(Rect NewBound)
        {
            RaiseEvent(new DragChangedEventArgs(DragChangingEvent, NewBound, TargetElement));
        }

        protected override bool GetTargetIsEditable()
        {
            return GetIsEditable(TargetElement);
        }
        #endregion 

        #endregion
    }
    public enum DragDirection
    {
        TopLeft = 1,
        TopCenter = 2,
        TopRight = 4,
        MiddleLeft = 16,
        MiddleCenter = 32,
        MiddleRight = 64,
        BottomLeft = 256,
        BottomCenter = 512,
        BottomRight = 1024,
    }
    [TemplatePart(Name = "PART_MainGrid", Type = typeof(Grid))]
    public abstract class DragHelperBase : ContentControl
    {
        #region Ctor
        static DragHelperBase()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DragHelperBase),
                new FrameworkPropertyMetadata(typeof(DragHelperBase)));
        }
        public DragHelperBase()
        {
            SetResourceReference(StyleProperty, typeof(DragHelperBase));
        }
        #endregion

        #region Propertes

        #region CornerWidth
        public int CornerWidth
        {
            get { return (int)GetValue(CornerWidthProperty); }
            set { SetValue(CornerWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CornerWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CornerWidthProperty =
            DependencyProperty.Register("CornerWidth", typeof(int), typeof(DragHelperBase), new PropertyMetadata(4));
        #endregion

        #region PART Object

        #region MainGrid
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        private Grid MainGrid;
        #endregion

        #endregion

        #region DragHelperParent
        protected FrameworkElement DragHelperParent
        {
            get
            {
                return Parent as FrameworkElement;
            }
        }
        #endregion

        #endregion

        #region Drag & Resize

        #region DragElement
        private Rect DragElement(double HorizontalChange, double VerticalChange)
        {
            Rect TargetActualBound = GetTargetActualBound();

            double TopOld = CorrectDoubleValue(TargetActualBound.Y);
            double LeftOld = CorrectDoubleValue(TargetActualBound.X);
            double TopNew = CorrectDoubleValue(TopOld + VerticalChange);
            double LeftNew = CorrectDoubleValue(LeftOld + HorizontalChange);

            TopNew = CorrectNewTop(DragHelperParent, TopNew, TargetActualBound.Height);
            LeftNew = CorrectNewLeft(DragHelperParent, LeftNew, TargetActualBound.Width);

            Canvas.SetTop(this, TopNew);
            Canvas.SetLeft(this, LeftNew);

            return new Rect
            {
                Y = TopNew,
                X = LeftNew,
                Width = TargetActualBound.Width,
                Height = TargetActualBound.Height
            };
        }
        #endregion

        #region ResizeElement
        private Rect ResizeElement(CustomThumb HitedThumb, double HorizontalChange, double VerticalChange)
        {
            #region Get Old Value

            if (HitedThumb == null) return Rect.Empty;


            Rect TargetActualBound = GetTargetActualBound();

            double TopOld = CorrectDoubleValue(TargetActualBound.Y);
            double LeftOld = CorrectDoubleValue(TargetActualBound.X);
            double WidthOld = CorrectDoubleValue(TargetActualBound.Width);
            double HeightOld = CorrectDoubleValue(TargetActualBound.Height);

            double TopNew = TopOld;
            double LeftNew = LeftOld;
            double WidthNew = WidthOld;
            double HeightNew = HeightOld;

            #endregion

            if (HitedThumb.DragDirection == DragDirection.TopLeft
                || HitedThumb.DragDirection == DragDirection.MiddleLeft
                || HitedThumb.DragDirection == DragDirection.BottomLeft)
            {
                ResizeFromLeft(DragHelperParent, LeftOld, WidthOld, HorizontalChange, out LeftNew, out WidthNew);
            }

            if (HitedThumb.DragDirection == DragDirection.TopLeft
                || HitedThumb.DragDirection == DragDirection.TopCenter
                || HitedThumb.DragDirection == DragDirection.TopRight)
            {
                ResizeFromTop(DragHelperParent, TopOld, HeightOld, VerticalChange, out TopNew, out HeightNew);
            }

            if (HitedThumb.DragDirection == DragDirection.TopRight
                || HitedThumb.DragDirection == DragDirection.MiddleRight
                || HitedThumb.DragDirection == DragDirection.BottomRight)
            {
                ResizeFromRight(DragHelperParent, LeftOld, WidthOld, HorizontalChange, out WidthNew);
            }

            if (HitedThumb.DragDirection == DragDirection.BottomLeft
                || HitedThumb.DragDirection == DragDirection.BottomCenter
                || HitedThumb.DragDirection == DragDirection.BottomRight)
            {
                ResizeFromBottom(DragHelperParent, TopOld, HeightOld, VerticalChange, out HeightNew);
            }

            this.Width = WidthNew;
            this.Height = HeightNew;
            Canvas.SetTop(this, TopNew);
            Canvas.SetLeft(this, LeftNew);

            return new Rect
            {
                X = LeftNew,
                Y = TopNew,
                Width = WidthNew,
                Height = HeightNew
            };
        }
        #endregion

        #region Resize Base Methods

        #region ResizeFromTop
        private static void ResizeFromTop(FrameworkElement Parent, double TopOld, double HeightOld, double VerticalChange, out double TopNew, out double HeightNew)
        {
            double MiniHeight = 10;

            double top = TopOld + VerticalChange;
            TopNew = ((top + MiniHeight) > (HeightOld + TopOld)) ? HeightOld + TopOld - MiniHeight : top;
            TopNew = TopNew < 0 ? 0 : TopNew;

            HeightNew = HeightOld + TopOld - TopNew;

            HeightNew = CorrectNewHeight(Parent, TopNew, HeightNew);
        }
        #endregion

        #region ResizeFromLeft
        private static void ResizeFromLeft(FrameworkElement Parent, double LeftOld, double WidthOld, double HorizontalChange, out double LeftNew, out double WidthNew)
        {
            double MiniWidth = 10;
            double left = LeftOld + HorizontalChange;

            LeftNew = ((left + MiniWidth) > (WidthOld + LeftOld)) ? WidthOld + LeftOld - MiniWidth : left;

            LeftNew = LeftNew < 0 ? 0 : LeftNew;

            WidthNew = WidthOld + LeftOld - LeftNew;

            WidthNew = CorrectNewWidth(Parent, LeftNew, WidthNew);
        }
        #endregion

        #region ResizeFromRight
        private static void ResizeFromRight(FrameworkElement Parent, double LeftOld, double WidthOld, double HorizontalChange, out double WidthNew)
        {
            if (LeftOld + WidthOld + HorizontalChange < Parent.ActualWidth)
            {
                WidthNew = WidthOld + HorizontalChange;
            }
            else
            {
                WidthNew = Parent.ActualWidth - LeftOld;
            }

            WidthNew = WidthNew < 0 ? 0 : WidthNew;
        }
        #endregion

        #region ResizeFromBottom
        private static void ResizeFromBottom(FrameworkElement Parent, double TopOld, double HeightOld, double VerticalChange, out double HeightNew)
        {
            if (TopOld + HeightOld + VerticalChange < Parent.ActualWidth)
            {
                HeightNew = HeightOld + VerticalChange;
            }
            else
            {
                HeightNew = Parent.ActualWidth - TopOld;
            }

            HeightNew = HeightNew < 0 ? 0 : HeightNew;
        }
        #endregion

        #region CorrectNewTop
        private static double CorrectNewTop(FrameworkElement Parent, double Top, double Height)
        {
            double NewHeight = ((Top + Height) > Parent.ActualHeight) ? (Parent.ActualHeight - Height) : Top;
            return NewHeight < 0 ? 0 : NewHeight;
        }
        #endregion

        #region CorrectNewLeft
        private static double CorrectNewLeft(FrameworkElement Parent, double Left, double Width)
        {
            double NewLeft = ((Left + Width) > Parent.ActualWidth) ? (Parent.ActualWidth - Width) : Left;

            return NewLeft < 0 ? 0 : NewLeft;
        }
        #endregion

        #region CorrectNewWidth
        private static double CorrectNewWidth(FrameworkElement Parent, double Left, double WidthNewToCheck)
        {
            double Width = ((Left + WidthNewToCheck) > Parent.ActualWidth) ? (Parent.ActualWidth - Left) : WidthNewToCheck;

            return Width < 0 ? 0 : Width;
        }
        #endregion

        #region CorrectNewHeight
        private static double CorrectNewHeight(FrameworkElement Parent, double Top, double HeightNewToCheck)
        {
            double Height = ((Top + HeightNewToCheck) > Parent.ActualHeight) ? (Parent.ActualHeight - Top) : HeightNewToCheck;
            return Height < 0 ? 0 : Height;
        }
        #endregion

        #region CorrectDoubleValue
        protected static double CorrectDoubleValue(double Value)
        {
            return (double.IsNaN(Value) || (Value < 0.0)) ? 0 : Value;
        }
        #endregion

        #endregion

        protected abstract bool GetTargetIsEditable();
        protected abstract Rect GetTargetActualBound();
        protected abstract void SetTargetActualBound(Rect NewBound);
        protected abstract void RaisenDragChangingEvent(Rect NewBound);
        protected abstract void RaisenDragCompletedEvent(Rect NewBound);

        #endregion

        #region Layout & Display    

        #region OnApplyTemplate
        public sealed override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            MainGrid = GetPartFormTemplate<Grid>("PART_MainGrid");

            AddLogicalChild(MainGrid);

            AddHandler(Thumb.DragDeltaEvent, new DragDeltaEventHandler(OnDragDelta));
            AddHandler(Thumb.DragCompletedEvent, new RoutedEventHandler(OnDragCompleted));


            Visibility = Visibility.Collapsed;
        }

        #endregion

        #region GetPartFormTemplate
        private T GetPartFormTemplate<T>(string name)
        {
            return (T)Template.FindName(name, this);
        }
        #endregion

        #region SetupVisualPropertes
        protected void SetupVisualPropertes(double TargetThickness, bool IsEditable)
        {
            Visibility IsCornerVisibe = IsEditable ? Visibility.Visible : Visibility.Collapsed;

            double ActualMargin = (CornerWidth - TargetThickness) / 2.0;

            MainGrid.Margin = new Thickness(0 - ActualMargin);

            foreach (CustomThumb item in MainGrid.Children)
            {
                if (item != null)
                {
                    item.BorderThickness = new Thickness(TargetThickness);

                    if (item.DragDirection == DragDirection.MiddleCenter)
                    {
                        item.Margin = new Thickness(ActualMargin);
                    }
                    else
                    {
                        item.Visibility = IsCornerVisibe;
                    }
                }
            }
        }
        #endregion

        #endregion

        #region DragCompletedEvent

        public static readonly RoutedEvent DragChangingEvent
            = EventManager.RegisterRoutedEvent("DragChangingEvent", RoutingStrategy.Bubble, typeof(DragChangedEventHandler), typeof(DragHelperBase));

        public event DragChangedEventHandler DragChanging
        {
            add
            {
                AddHandler(DragChangingEvent, value);
            }
            remove
            {
                RemoveHandler(DragChangingEvent, value);
            }
        }

        public static readonly RoutedEvent DragCompletedEvent
                    = EventManager.RegisterRoutedEvent("DragCompletedEvent", RoutingStrategy.Bubble, typeof(DragChangedEventHandler), typeof(DragHelperBase));

        public event DragChangedEventHandler DragCompleted
        {
            add
            {
                AddHandler(DragCompletedEvent, value);
            }
            remove
            {
                RemoveHandler(DragCompletedEvent, value);
            }
        }
        #endregion

        #region Drag Event Handler
        private void OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            if (!GetTargetIsEditable())
            {
                e.Handled = true;
                return;
            }

            CustomThumb thumb = e.OriginalSource as CustomThumb;

            if (thumb == null)
            {
                return;
            }

            double VerticalChange = e.VerticalChange;
            double HorizontalChange = e.HorizontalChange;

            Rect NewBound = Rect.Empty;

            if (thumb.DragDirection == DragDirection.MiddleCenter)
            {
                NewBound = DragElement(HorizontalChange, VerticalChange);
            }
            else
            {
                NewBound = ResizeElement(thumb, HorizontalChange, VerticalChange);
            }

            RaisenDragChangingEvent(NewBound);
            SetTargetActualBound(NewBound);

            e.Handled = true;
        }

        private void OnDragCompleted(object sender, RoutedEventArgs e)
        {
            Rect NewBound = new Rect
            {
                Y = Canvas.GetTop(this),
                X = Canvas.GetLeft(this),
                Width = this.ActualWidth,
                Height = this.ActualHeight
            };

            RaisenDragCompletedEvent(NewBound);

            e.Handled = true;
        }
        #endregion
    }

}
