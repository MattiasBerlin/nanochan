using System.Runtime.InteropServices;

namespace NanoChan
{
    [ComVisible(true)]
    public class JavascriptInterface
    {
        private MainWindow MainWindow;
        private DefinitionWindow DefinitionWindow;

        public JavascriptInterface(MainWindow mainWindow, DefinitionWindow definitionWindow)
        {
            MainWindow = mainWindow;
            DefinitionWindow = definitionWindow;
        }

        public void OnWordHover(int blockID, int wordID, int x, int y)
        {
            DefinitionWindow.Top = MainWindow.Top + MainWindow.GetTopBar().ActualHeight + y;
            DefinitionWindow.Left = MainWindow.Left + x;
            
            DefinitionWindow.ShowDefinition(MainWindow.WordHistory.GetWord(blockID, wordID));
            MainWindow.Focus();
        }

        public void OnWordStopHover()
        {
            if (!DefinitionWindow.IsMouseOver)
            {
                DefinitionWindow.Hide();
            }
        }
    }
}
