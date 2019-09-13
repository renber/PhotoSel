using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PhotoSel.Commands
{
    class DelayKeyGesture : KeyGesture
    {
        long lastExecutionTick;

        InputGesture WrappedGesture { get; }

        /// <summary>
        /// The repeat delay in milliseconds
        /// </summary>
        public int RepeatDelay { get; set; }

        public DelayKeyGesture(KeyGesture wrappedGesture, int repeatDelay)
            : base (wrappedGesture.Key, wrappedGesture.Modifiers, wrappedGesture.DisplayString)
        {
            WrappedGesture = wrappedGesture;
            RepeatDelay = repeatDelay;
        }

        public bool KeysMatch(object targetElement, InputEventArgs inputEventArgs)
        {
            return WrappedGesture.Matches(targetElement, inputEventArgs);
        }

        public override bool Matches(object targetElement, InputEventArgs inputEventArgs)
        {
            bool baseResult = WrappedGesture.Matches(targetElement, inputEventArgs);

            if (baseResult)
            {
                if (inputEventArgs is KeyEventArgs kargs)
                {
                    // only delay repetitions (i.e. when the user keeps the key pressed)
                    if (kargs.IsRepeat)
                    {
                        // check if the time elapsed already
                        if (Environment.TickCount - lastExecutionTick >= RepeatDelay)
                        {
                            lastExecutionTick = Environment.TickCount;
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        lastExecutionTick = Environment.TickCount;
                    }
                }
            }

            return baseResult;
        }
    }
}
