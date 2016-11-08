using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ConcurrencyChecker.Vsix
{
    public partial class ConcurrencyCheckerSettingsUi : UserControl
    {
        public ConcurrencyCheckerSettingsUi()
        {
            InitializeComponent();
        }

        internal ConcurrencyCheckerSettings optionsPage;

        public void Initialize()
        {

            var selectedSmells = optionsPage.SelectedSmells;
            if (selectedSmells == null)
            {
                selectedSmells = new List<string>();
            }
            
            var items = checkedListBox1.Items;
            foreach (var smell in optionsPage.Smells)
            {
                items.Add(smell, IsSelected(selectedSmells, smell));
            }

            numericUpDown1.Value = optionsPage.MaxDepthAsync;
        }

        private static bool IsSelected(List<string> selectedSmells, Smell smell)
        {
            return selectedSmells.Contains(smell.ToString());
        }
        
        private void checkedListBox1_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            optionsPage.SelectedSmells = new List<string>();
            foreach (var selectedItem in checkedListBox1.CheckedItems)
            {
                var smell = selectedItem.ToString();
                optionsPage.SelectedSmells.Add(smell);
            }

            var value = checkedListBox1.Items[e.Index].ToString();
            if (e.NewValue == CheckState.Checked)
            {
                optionsPage.SelectedSmells.Add(value);
            }
            else if (e.NewValue == CheckState.Unchecked)
            {
                optionsPage.SelectedSmells.Remove(value);
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            optionsPage.MaxDepthAsync = (int)numericUpDown1.Value;
        }
    }
}
