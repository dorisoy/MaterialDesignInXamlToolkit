using Microsoft.UI.Input.Experimental;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Windows.Foundation;
using Windows.UI.Core;

namespace MaterialDesignThemes.WinUI
{
    //[TemplateVisualState(GroupName = "CommonStates", Name = TemplateStateNormal)]
    //[TemplateVisualState(GroupName = "CommonStates", Name = TemplateStateMousePressed)]
    //[TemplateVisualState(GroupName = "CommonStates", Name = TemplateStateMouseOut)]
    public class Ripple : ContentControl
    {
        public const string TemplateStateNormal = "Normal";
        public const string TemplateStateMousePressed = "MousePressed";
        public const string TemplateStateMouseOut = "MouseOut";

        private static readonly HashSet<Ripple> PressedInstances = new();

       private PointerEventHandler Foo = new PointerEventHandler(EndPointerEvent);
        public Ripple()
        {
            DefaultStyleKey = typeof(Ripple);

            SizeChanged += OnSizeChanged;

            PointerExited += Ripple_PointerExited;
            PointerPressed += Ripple_PointerPressed;

            //AddHandler(PointerReleasedEvent, Foo, true);
            //AddHandler(PointerCanceledEvent, Foo, true);
            //AddHandler(PointerCaptureLostEvent, Foo, true);
            //PointerCanceled += EndPointerEvent;
            //PointerCaptureLost += EndPointerEvent;
            PointerReleased += EndPointerEvent;
        }

        private void Ripple_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, TemplateStateMouseOut, true);
        }

        private static void EndPointerEvent(object sender, PointerRoutedEventArgs e)
        {
            Debug.WriteLine("Ripple Releases");

            foreach (var ripple in PressedInstances)
            {
                // adjust the transition scale time according to the current animated scale
                var scaleTrans = ripple.GetTemplateChild("ScaleTransform") as ScaleTransform;
                if (scaleTrans != null)
                {
                    double currentScale = scaleTrans.ScaleX;
                    var newTime = TimeSpan.FromMilliseconds(300 * (1.0 - currentScale));

                    // change the scale animation according to the current scale
                    var scaleXKeyFrame = ripple.GetTemplateChild("MousePressedToNormalScaleXKeyFrame") as EasingDoubleKeyFrame;
                    if (scaleXKeyFrame != null)
                    {
                        scaleXKeyFrame.KeyTime = KeyTime.FromTimeSpan(newTime);
                    }
                    var scaleYKeyFrame = ripple.GetTemplateChild("MousePressedToNormalScaleYKeyFrame") as EasingDoubleKeyFrame;
                    if (scaleYKeyFrame != null)
                    {
                        scaleYKeyFrame.KeyTime = KeyTime.FromTimeSpan(newTime);
                    }
                }

                VisualStateManager.GoToState(ripple, TemplateStateNormal, true);
                //ripple.ReleasePointerCapture(e.Pointer);
            }
            PressedInstances.Clear();
        }

        public static readonly DependencyProperty FeedbackProperty = DependencyProperty.Register(
            nameof(Feedback), typeof(Brush), typeof(Ripple), new PropertyMetadata(default(Brush)));

        public Brush? Feedback
        {
            get => (Brush?)GetValue(FeedbackProperty);
            set => SetValue(FeedbackProperty, value);
        }

        private void Ripple_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            Debug.WriteLine("Ripple Pressed");
            e.Handled = false;
            if (RippleAssist.GetIsCentered(this))
            {
                var innerContent = Content as FrameworkElement;

                if (innerContent != null)
                {
                    Point position = innerContent.TransformToVisual(this)
                        .TransformPoint(new Point(0, 0));

                    if (FlowDirection == FlowDirection.RightToLeft)
                        RippleX = position.X - innerContent.ActualWidth / 2 - RippleSize / 2;
                    else
                        RippleX = position.X + innerContent.ActualWidth / 2 - RippleSize / 2;
                    RippleY = position.Y + innerContent.ActualHeight / 2 - RippleSize / 2;
                }
                else
                {
                    RippleX = ActualWidth / 2 - RippleSize / 2;
                    RippleY = ActualHeight / 2 - RippleSize / 2;
                }
            }
            else
            {
                Point point = e.GetCurrentPoint(this).Position;
                RippleX = point.X - RippleSize / 2;
                RippleY = point.Y - RippleSize / 2;
            }

            if (!RippleAssist.GetIsDisabled(this))
            {
                //We need to capture to get the PointerReleased event
                //But this also appears to prevent the click from propogating up
                VisualStateManager.GoToState(this, TemplateStateNormal, false);
                VisualStateManager.GoToState(this, TemplateStateMousePressed, true);
                PressedInstances.Add(this);
                //await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => CapturePointer(e.Pointer));
            }
        }

        private static readonly DependencyProperty RippleSizeProperty =
            DependencyProperty.Register(
                "RippleSize", typeof(double), typeof(Ripple),
                new PropertyMetadata(default(double)));

        public double RippleSize
        {
            get => (double)GetValue(RippleSizeProperty);
            private set => SetValue(RippleSizeProperty, value);
        }

        private static readonly DependencyProperty RippleXProperty =
            DependencyProperty.Register(
                "RippleX", typeof(double), typeof(Ripple),
                new PropertyMetadata(default(double)));

        public double RippleX
        {
            get => (double)GetValue(RippleXProperty);
            private set => SetValue(RippleXProperty, value);
        }

        private static readonly DependencyProperty RippleYProperty =
            DependencyProperty.Register(
                "RippleY", typeof(double), typeof(Ripple),
                new PropertyMetadata(default(double)));

        public double RippleY
        {
            get => (double)GetValue(RippleYProperty);
            private set => SetValue(RippleYProperty, value);
        }

        /// <summary>
        ///   The DependencyProperty for the RecognizesAccessKey property. 
        ///   Default Value: false 
        /// </summary> 
        public static readonly DependencyProperty RecognizesAccessKeyProperty =
            DependencyProperty.Register(
                nameof(RecognizesAccessKey), typeof(bool), typeof(Ripple),
                new PropertyMetadata(default(bool)));

        /// <summary> 
        ///   Determine if Ripple should use AccessText in its style
        /// </summary> 
        public bool RecognizesAccessKey
        {
            get => (bool)GetValue(RecognizesAccessKeyProperty);
            set => SetValue(RecognizesAccessKeyProperty, value);
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            VisualStateManager.GoToState(this, TemplateStateNormal, false);
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            //TODO: respect clipping from attached property?
            Clip = new RectangleGeometry
            {
                Rect = new Rect(0, 0, ActualWidth, ActualHeight)
            };

            var innerContent = Content as FrameworkElement;

            double width, height;

            if (RippleAssist.GetIsCentered(this) && innerContent != null)
            {
                width = innerContent.ActualWidth;
                height = innerContent.ActualHeight;
            }
            else
            {
                width = sizeChangedEventArgs.NewSize.Width;
                height = sizeChangedEventArgs.NewSize.Height;
            }

            var radius = Math.Sqrt(Math.Pow(width, 2) + Math.Pow(height, 2));

            RippleSize = 2 * radius * RippleAssist.GetRippleSizeMultiplier(this);
        }
    }
}
