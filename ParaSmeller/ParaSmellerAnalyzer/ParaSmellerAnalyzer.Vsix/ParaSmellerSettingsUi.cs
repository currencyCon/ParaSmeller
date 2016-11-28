using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ParaSmeller.Vsix
{
    public partial class ParaSmellerSettingsUi : UserControl
    {
        public ParaSmellerSettingsUi()
        {
            InitializeComponent();
        }

        internal ParaSmellerSettings OptionsPage;

        public void Initialize()
        {

            var selectedSmells = OptionsPage.SelectedSmells;
            if (selectedSmells == null)
            {
                selectedSmells = new List<string>();
            }
            
            var items = checkedListBox1.Items;
            foreach (var smell in OptionsPage.Smells)
            {
                items.Add(smell, IsSelected(selectedSmells, smell));
            }

            numericUpDown1.Value = OptionsPage.MaxDepthAsync;
        }

        private static bool IsSelected(ICollection<string> selectedSmells, Smell smell)
        {
            return selectedSmells.Contains(smell.ToString());
        }
        
        private void checkedListBox1_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            OptionsPage.SelectedSmells = new List<string>();
            foreach (var selectedItem in checkedListBox1.CheckedItems)
            {
                var smell = selectedItem.ToString();
                OptionsPage.SelectedSmells.Add(smell);
            }

            var value = checkedListBox1.Items[e.Index].ToString();
            if (e.NewValue == CheckState.Checked)
            {
                OptionsPage.SelectedSmells.Add(value);
            }
            else if (e.NewValue == CheckState.Unchecked)
            {
                OptionsPage.SelectedSmells.Remove(value);
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            OptionsPage.MaxDepthAsync = (int)numericUpDown1.Value;
        }
    }
}
