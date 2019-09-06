using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PhotoSel.Commands
{
    /// <summary>
    /// A command for which a (minimum) repetition delay can be defined
    /// </summary>
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

        static DelayKeyBinding()
        {
            // register a changed callback to the command property
            CommandProperty.OverrideMetadata(typeof(DelayKeyBinding), new FrameworkPropertyMetadata(CommandPropertyChangedCallback));
        }        

        static void CommandPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null && !(e.NewValue is DelayedWrapperCommand))
            {
                ((DelayKeyBinding)d).Command = new DelayedWrapperCommand((ICommand)e.NewValue, ((DelayKeyBinding)d).RepeatDelay);
            }
        }

        static void RepeatDelayPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (((DelayKeyBinding)d).Command is DelayedWrapperCommand)
            {
                ((DelayedWrapperCommand)((DelayKeyBinding)d).Command).RepeatDelay = (int)e.NewValue;
            }
        }
    }

    class DelayedWrapperCommand : ICommand
    {
        ICommand InnerCommand { get; }
        int lastExecutionTick = 0;
        public int RepeatDelay { get; set; }

        public event EventHandler CanExecuteChanged
        {
            add => InnerCommand.CanExecuteChanged += value;
            remove => InnerCommand.CanExecuteChanged -= value;
        }

        public DelayedWrapperCommand(ICommand innerCommand, int repeatDelay)
        {
            InnerCommand = innerCommand;
            RepeatDelay = repeatDelay;
        }

        public bool CanExecute(object parameter) => InnerCommand.CanExecute(parameter);

        public void Execute(object parameter)
        {
            if (Environment.TickCount - lastExecutionTick >= RepeatDelay)
            {
                InnerCommand.Execute(parameter);
                lastExecutionTick = Environment.TickCount;
            }
        }
    }
}
