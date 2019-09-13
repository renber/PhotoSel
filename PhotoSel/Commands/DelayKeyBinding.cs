using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PhotoSel.Commands
{
    class DelayKeyBinding : KeyBinding
    {
        // Using a DependencyProperty as the backing store for RepeatDelay.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RepeatDelayProperty =
            DependencyProperty.Register(nameof(RepeatDelay), typeof(int), typeof(DelayKeyBinding), new PropertyMetadata(0, RepeatDelayPropertyChangedCallback));

        /// <summary>
        /// The repeat delay in milliseconds
        /// </summary>
        public int RepeatDelay
        {
            get { return (int)GetValue(RepeatDelayProperty); }
            set { SetValue(RepeatDelayProperty, value); }
        }

        [System.ComponentModel.TypeConverter(typeof(System.Windows.Input.KeyGestureConverter))]
        public override InputGesture Gesture {
            get => base.Gesture;
            set => base.Gesture = value == null ? null : new DelayKeyGesture(value as KeyGesture, RepeatDelay); }

        static void RepeatDelayPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {            
            if ((d as DelayKeyBinding).Gesture is DelayKeyGesture gesture)
            {
                gesture.RepeatDelay = (int)e.NewValue;
            }            
        }        
    }
}
