using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace PhotoSel.Behaviors
{
    public class ScrollIntoViewForListBox : Behavior<ListBox>
    {
        int itemWidth = 80;

        /// <summary>
        ///  When Beahvior is attached
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.SelectionChanged += AssociatedObject_SelectionChanged;
        }

        /// <summary>
        /// On Selection Changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void AssociatedObject_SelectionChanged(object sender,
                                               SelectionChangedEventArgs e)
        {
            if (sender is ListBox)
            {
                ListBox listBox = (sender as ListBox);

                double displayedItems = listBox.ActualWidth / itemWidth - 1;
                int scrolloffset = (int)(displayedItems / 2);
                scrolloffset -= (scrolloffset % 2 == 0) ? 2 : 1;

                if (listBox.SelectedItem != null)
                {
                    listBox.Dispatcher.BeginInvoke(
                        (Action)(() =>
                        {
                            listBox.UpdateLayout();
                            if (listBox.SelectedItem != null)
                            {
                                int idx = listBox.Items.IndexOf(listBox.SelectedItem);
                                int scrollToIdx = Math.Min(listBox.Items.Count - 1, idx + scrolloffset);

                                if (e.RemovedItems.Count > 0)
                                {
                                    if (listBox.Items.IndexOf(e.RemovedItems[0]) > idx)
                                    {
                                        // user is scrolling backwards
                                        scrollToIdx = Math.Max(0, idx - scrolloffset - 1);
                                    }
                                }                                

                                listBox.ScrollIntoView(listBox.Items[scrollToIdx]);
                            }                                
                        }));
                    
                }
            }
        }

        /// <summary>
        /// When behavior is detached
        /// </summary>
        protected override void OnDetaching()
        {
            base.OnDetaching();
            this.AssociatedObject.SelectionChanged -=
                AssociatedObject_SelectionChanged;

        }
    }
}
