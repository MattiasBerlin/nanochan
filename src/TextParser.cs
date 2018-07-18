using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NanoChan.Dictionary;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace NanoChan
{
    class TextParser
    {
        private EdictReader edict;
        private List<Inflection> inflections;

        private readonly Char[] breakPoints = new Char[] { '。', '「', '」', '『', '』', '…', '\n', '\t' };

        public TextParser(EdictReader edict)
        {
            this.edict = edict;

            if (inflections == null)
            {
                XDocument inflectionsDoc;
                inflectionsDoc = XDocument.Load(@"data/inflections.xml");
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(Inflections));

                Inflections root = (Inflections)xmlSerializer.Deserialize(inflectionsDoc.CreateReader());
                inflections = root.InflectionList;
            }
        }

        public List<Word> ProcessText(string text)
        {
            // Check for particles after words. Maybe "extensions" like やべぇ too?
            // What to do about exceptions? I.e. する -> した etc.
            // Support base form! 食べ etc. Maybe check common suffixes after base? E.g. ながら
            // Stop search at closest punctuation? Would be better performance-wise. Check dictionary if any entries contain this
            // Result: No 。「」…  A few 、．・
            // Support for ー extension, e.g. とーぜん -> とうぜん
            // Pre-check for (self-added) names in lookup
            // Handle "-chan to" being interpreted chanto?
            // Masu-form! いいます -> いう
            // Have "extensions" (e.g. ます, ている) as inflections or find it from stem then look for extensions from a list?
            // Skip character if leftmost is punctuation or similar.
            // Need hira -> kata and vice versa converter.
            // Jesus, come up with better variable and function names

            // TODO Add option to retain newlines
            text = text.Replace(Environment.NewLine, String.Empty);

            List<Word> words = CheckTextBeginning(text);
            text = text.Substring(words.Count);

            int lookupPos = FindNextLookupPos(text);

            while (text.Length > 0)
            {
                string lookup = text.Substring(0, lookupPos);
                List<WordEntry> matches = new List<WordEntry>();

                // Simply try the selection
                //List<Entry> entries = edict.FindEntries(lookup);
                //matches.AddRange(entries);
                matches.AddRange(ToWordEntries(edict.FindEntries(lookup), null, lookup, 0));

                if (lookupPos > 1)
                {
                    // Try all known inflections
                    //entries = CheckInflections(lookup);
                    //matches.AddRange(entries);
                    matches.AddRange(CheckInflections(lookup));
                }

                if (matches.Count > 0)
                {
                    words.Add(new DictWord(lookup, matches));

                    // Don't continue if there's nothing left to look up
                    /*if (text.Length <= 1)
                    {
                        return words;
                    }*/

                    //List<Word> beginWords = CheckTextBeginning(text.Substring(lookupPos));
                    //words.AddRange(beginWords);
                    //text = text.Substring(lookupPos);
                    text = text.Substring(lookupPos); // + beginWords.Count); // TODO Any checks? Probably good though with the while condition
                    //lookupPos = FindNextLookupPos(text);//text.Length;
                    lookupPos = NewLookupPos(ref text, words);
                }
                else
                {
                    // If nothing found and lookupPos about to become 0, give up on this character
                    if (lookupPos <= 1)
                    {
                        words.Add(new UnknownWord(lookup));

                        text = text.Substring(1);
                        lookupPos = NewLookupPos(ref text, words);//FindNextLookupPos(text);//text.Length;
                    }
                    else
                    {
                        lookupPos--;
                    }
                }
            }

            return words;
        }

        private List<Word> CheckTextBeginning(string text)
        {
            List<Word> words = new List<Word>();

            for (int i = 0; i < text.Length; i++)
            {
                bool foundWord = false;

                for (int k = 0; k < breakPoints.Length; k++)
                {
                    if (text[i] == breakPoints[k])
                    {
                        words.Add(new UnknownWord(text[i].ToString()));
                        foundWord = true;
                        break;
                    }
                }

                if (!foundWord)
                {
                    break;
                }
            }

            return words;
        }

        private int FindNextLookupPos(string text)
        {
            int breakPoint = text.IndexOfAny(breakPoints);
            return breakPoint >= 0 ? breakPoint : text.Length;
        }

        private int NewLookupPos(ref string text, List<Word> words)
        {
            List<Word> beginWords = CheckTextBeginning(text);
            words.AddRange(beginWords);
            text = text.Substring(beginWords.Count);
            return FindNextLookupPos(text);
        }

        private List<WordEntry> CheckInflections(string text)
        {
            List<WordEntry> matches = new List<WordEntry>();

            foreach (Inflection c in inflections)
            {
                if (text.EndsWith(c.Ending))
                {
                    string baseForm = text.Substring(0, text.Length - c.Ending.Length);
                    foreach (string end in c.Original)
                    {
                        matches.AddRange(ToWordEntries(edict.FindEntries(baseForm + end), c, baseForm, c.Ending.Length));
                        /*foreach (Entry entry in entries)
                        {
                            entry.Inflection = c;
                            entry.SetFurigana(baseForm, c.Ending.Length);
                            matches.Add(entry);
                        }*/
                    }
                }
            }
            // Search for baseForm too if it fails finding one?
            // Past me, I doubt there would be an entry for stem if there's none for the actual verb

            // Check for godan stem
            // Could potentially be done using inflections
            // Only if i-kana!!
            /*char? uKana = KanaExpert.GetCorrespondingUHiragana(text.Last());
            if (uKana != null)
            {
                entries = edict.FindEntries(text.Substring(0, text.Length - 1) + uKana);
                if (entries?.Count > 0)
                {
                    return new Word(entries, new Inflection("Stem"));
                }
            }*/

            // Check for ichidan stem
            matches.AddRange(ToWordEntries(edict.FindIchidanEntries(text + "る"), new Inflection("Stem"), text, 1));
            /*foreach (Entry entry in ichidanEntries)
            {
                entry.Inflection = new Inflection("Stem");
                entry.SetFurigana(text, 1);
                matches.Add(entry);
            }*/

            return matches;
        }

        public List<WordEntry> ToWordEntries(List<Entry> entries, Inflection inflection, string original, int cutOff)
        {
            List<WordEntry> wordEntries = new List<WordEntry>(entries.Count);
            foreach (Entry entry in entries)
            {
                wordEntries.Add(new WordEntry(entry, inflection, original, cutOff));
            }
            return wordEntries;
        }
    }

    public abstract class Word
    {
        public string OriginalForm { get; set; }

        protected Word(string originalForm)
        {
            OriginalForm = originalForm;
        }

        abstract public string ToJSON(int id);
        abstract public override string ToString();
    }

    public class DictWord : Word
    {
        public List<WordEntry> Entries { get; set; }
        public int CurrentPage { get; set; }
        // Particle? Or separate? <- I was talking about particle expectation. Likely separate, probably variable in recursive function?

        public DictWord(string originalForm, List<WordEntry> entries) : base(originalForm)
        {
            Entries = entries;
        }

        public int DecPage()
        {
            return CurrentPage > 0 ? --CurrentPage : CurrentPage;
        }

        public int IncPage()
        {
            return CurrentPage < Entries.Count-1 ? ++CurrentPage : CurrentPage;
        }

        public override string ToJSON(int id)
        {
            // TODO Later on this should return the entry's reading that was last selected
            if (Entries?.Count > 0)
            {
                //string furigana = Entries[0].Kanji?.Count > 0 ? ",\"furigana\":\"" + Entries[0].Reading[0] + "\"": "";
                string furigana = !KanaExpert.ContainsOnlyKana(OriginalForm) ? ",\"furigana\":\"" + Entries[0].Furigana.Text + "\",\"furiganaLength\":\"" + Entries[0].Furigana.Length + "\"" : "";
                return "{\"id\":" + id + ",\"hasDefinition\":true,\"word\":\"" +  OriginalForm + "\"" + furigana + "}";
            }
            else
            {
                Console.WriteLine("DictWord without entry! This shouldn't happen!");
                return "{}";
            }
        }

        public override string ToString()
        {
            string s = "";
            for (int i = 0; i < Entries.Count; i++)
            {
                s += Entries[i].ToString();
            }

            return s;
        }
    }

    public class UnknownWord : Word
    {
        public UnknownWord(string originalForm) : base(originalForm) {}

        public override string ToJSON(int id)
        {
            return "{\"id\":" + id + ",\"word\":\"" + OriginalForm + "\"}";
        }

        public override string ToString()
        {
            return OriginalForm;
        }
    }

    public class WordEntry
    {
        public Entry Entry { get; set; }
        public Inflection Inflection { get; set; }
        // TODO This will most likely need to be a list the day I decide to make recursive inflection checks
        public Furigana Furigana { get; set; }

        public WordEntry(Entry entry, string original, int cutOff)
        {
            Entry = entry;
            SetFurigana(original, cutOff);
        }

        public WordEntry(Entry entry, Inflection inflection, string original, int cutOff) : this(entry, original, cutOff)
        {
            Inflection = inflection;
        }

        private void SetFurigana(string original, int cutOff)
        {
            if (cutOff < Entry.Reading[0].KanaReading.Length)
            {
                string reading = Entry.Reading[0].KanaReading.Substring(0, Entry.Reading[0].KanaReading.Length - cutOff);
                while (original.Length > 0 && reading.Length > 0 && original[original.Length - 1] == reading[reading.Length - 1])
                {
                    original = original.Substring(0, original.Length - 1);
                    reading = reading.Substring(0, reading.Length - 1);
                }

                Furigana = new Furigana(reading, original.Length);
            }
            else
            {
                Furigana = new Furigana(Entry.Reading[0].KanaReading, original.Length);
            }
        }

        public override string ToString()
        {
            string s = String.Format("Kanji: {0}\nKana: {1}\n",
            String.Join(", ", Entry.Kanji), String.Join(", ", Entry.Reading));

            for (int i = 0; i < Entry.Glossary.Count; i++)
            {
                s += String.Format("{0}. {1}\n", i + 1, Entry.Glossary[i]);
            }
            if (Inflection != null)
            {
                s += "Inflection: " + Inflection.Description + "\n";
            }

            return s;
        }
    }

    public class Furigana
    {
        public string Text { get; set; }
        public int Length { get; set; }

        public Furigana(string text, int length)
        {
            Text = text;
            Length = length;
        }
    }

    [XmlRoot("entry")]
    public class Inflection
    {
        [XmlElement("ending")]
        public string Ending { get; set; }
        [XmlElement("original")]
        public List<string> Original { get; set; }
        [XmlElement("desc")]
        public string Description { get; set; }

        // Constructor for XML serializer
        public Inflection() { }

        // Constructor for "custom" inflections done in the parser
        public Inflection(string description)
        {
            Description = description;
        }
    }

    [XmlRoot("conjugations")]
    public class Inflections
    {
        [XmlElement("entry")]
        public List<Inflection> InflectionList { get; set; }
    }
}
