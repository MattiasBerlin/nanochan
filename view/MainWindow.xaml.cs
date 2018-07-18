using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using NanoChan.Dictionary;

namespace NanoChan
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TextParser parser;
        private ClipboardListener clipboard;
        private DefinitionWindow definitionWindow;
        public WordHistory WordHistory { get; }

        public MainWindow()
        {
            InitializeComponent();

            definitionWindow = new DefinitionWindow();

            browser.ObjectForScripting = new JavascriptInterface(this, definitionWindow);
            browser.Navigate("file://" + Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "html", "translation.html")));

            contextMenu.Items.Add("Test", null, Test_Click);
            contextMenu.Items.Add("Test 2", null, Test2_Click);

            EdictReader edict = new EdictReader(@"C:\Users\Mattias\Downloads\JMdict_e"); //SampleEdict.xml JMdict_e
            parser = new TextParser(edict);
            WordHistory = new WordHistory();

            //List<Word> words = parser.ProcessText("薪人「哀しいなら哀しいって、寂しいなら寂しいって、言ってくれていいんだ」");
            /*List<Word> words = parser.ProcessText("オーバック偸閑食べ");
            //List<Word> words = parser.ProcessText("澄");
            if (words.Count > 0)
            {
                Console.WriteLine(words.Count + " words found:");
                for (int i = 0; i < words.Count; i++)
                {
                    Console.WriteLine("Word " + (i+1) + ":\n" + words[i]);
                }
            }
            else
            {
                Console.WriteLine("No results found");
            }*/
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            ClipboardHandler clipboardHandler = OnClipboardUpdate;
            clipboard = new ClipboardListener(this, clipboardHandler);
            clipboard.Start();
        }

        private void OnClipboardUpdate(string text)
        {
            Console.WriteLine(text);
            string reconstructed = "";

            List<Word> words = parser.ProcessText(text);
            if (words.Count > 0)
            {
                string json = "[{0}]";
                List<string> jsonWords = new List<string>();
                Dictionary<int, DictWord> dictWords = new Dictionary<int, DictWord>();

                Console.WriteLine(words.Count + " words found:");
                for (int i = 0; i < words.Count; i++)
                {
                    jsonWords.Add(words[i].ToJSON(i));
                    reconstructed += words[i].OriginalForm;
                    Console.WriteLine("Word " + (i + 1) + ":\n" + words[i]);

                    if (words[i].GetType().Equals(typeof(DictWord)))
                    {
                        dictWords[i] = (DictWord)words[i];
                    }
                }

                int blockID = WordHistory.AddBlock(dictWords);
                Console.WriteLine("Sending JSON: " + String.Format(json, String.Join(",", jsonWords)));
                browser.Document.InvokeScript("addBlock", new object[] { blockID, String.Format(json, String.Join(",", jsonWords)) });
            }
            else
            {
                Console.WriteLine("No results found");
            }

            Console.WriteLine("Original:\n" + text);
            Console.WriteLine("Reconstructed:\n" + reconstructed);
        }

        public System.Windows.Controls.Grid GetTopBar()
        {
            return TopBar;
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Test_Click(object sender, EventArgs e)
        {
            //browser.Document.InvokeScript("appendWord", new object[] { 3, "漢字", true, "かんじ" });
            browser.Document.InvokeScript("addBlock", new object[] { parser.ProcessText("オーバック")[0] });
        }

        private void Test2_Click(object sender, EventArgs e)
        {
            browser.Document.InvokeScript("addWordEventListeners");
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settingsWindow = new SettingsWindow();
            settingsWindow.Show();
        }
    }

    public class WordHistory
    {
        private List<Dictionary<int, DictWord>> Blocks;

        public WordHistory()
        {
            Blocks = new List<Dictionary<int, DictWord>>();
        }

        public void AddBlock(int blockID, Dictionary<int, DictWord> words)
        {
            Blocks.Insert(blockID, words);
        }

        public int AddBlock(Dictionary<int, DictWord> words)
        {
            int blockID = Blocks.Count;
            Blocks.Insert(blockID, words);
            return blockID;
        }

        public Dictionary<int, DictWord> GetBlock(int blockID)
        {
            return Blocks[blockID];
        }

        public DictWord GetWord(int blockID, int wordID)
        {
            return Blocks[blockID][wordID];
        }
    }
}
