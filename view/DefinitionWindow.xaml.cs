using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NanoChan
{
    /// <summary>
    /// Interaction logic for Definition.xaml
    /// </summary>
    public partial class DefinitionWindow : Window
    {
        private DictWord word;

        public DefinitionWindow()
        {
            InitializeComponent();
            
            // Let kanji and kana elements automatically get sized height-wise
            Kanji.Height = Double.NaN;
            Kana.Height = Double.NaN;
        }

        public void ShowDefinition(DictWord word)
        {
            int P = word.CurrentPage;
            this.word = word;
            string content = "";
            for (int i = 0; i < word.Entries[P].Entry.Glossary.Count; i++)
            {
                content += i+1 + ". " + word.Entries[P].Entry.Glossary[i].ToString() + "\n";
            }
            Entries.Text = content;
            Kanji.Text = String.Join("\n", word.Entries[P].Entry.Kanji);
            Kana.Text = String.Join("\n", word.Entries[P].Entry.Reading);
            PageIndicator.Content = P+1 + " / " + word.Entries.Count;

            Show();
        }

        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            Hide();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Visibility = Visibility.Hidden;
        }

        private void GoToLeftPage(object sender, MouseEventArgs e)
        {
            word.DecPage();
            ShowDefinition(word);
        }

        private void GoToRightPage(object sender, MouseButtonEventArgs e)
        {
            word.IncPage();
            ShowDefinition(word);
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                word.IncPage();
            }
            else
            {
                word.DecPage();
            }
            ShowDefinition(word);
        }
    }
}
