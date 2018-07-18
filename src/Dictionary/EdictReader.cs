using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace NanoChan.Dictionary
{
    class EdictReader
    {
        private const int dictEntries = 370000;

        // TODO: Make into dictionary, much faster lookup
        //private XDocument doc;
        private Dictionary<string, List<Entry>> dictionary;
        // Bidirectional dictionary for looking up tags and their descriptions.
        // Keyed on both tag and description.
        private Dictionary<string, string> entities;

        public EdictReader(string path)
        {
            XDocument doc = XDocument.Load(path);
            entities = new Dictionary<string, string>();
            dictionary = new Dictionary<string, List<Entry>>(dictEntries);

            // Make a dictionary out of the entity tags in the header
            ProcessHeader(doc.DocumentType.InternalSubset);
            
            // Make a dictionary out of the entries
            ProcessDocument(doc);
        }

        // Put the entities in the header into the entities dictionary. This is done in both directions: from tag to description and vice versa.
        private void ProcessHeader(string header)
        {
            string[] headerLines = header.Split('\n');
            foreach (string line in headerLines)
            {
                if (line.StartsWith("<!ENTITY "))
                {
                    string[] entity = line.Substring(9).Split(new[] { ' ' }, 2);
                    if (entity.Length < 2)
                    {
                        Console.WriteLine("Failed to parse entity: " + line);
                        continue;
                    }
                    string desc = entity[1].Substring(1, entity[1].Length - 3);
                    entities.Add(desc, entity[0]);
                    if (!entities.ContainsKey(entity[0]))
                    {
                        entities.Add(entity[0], desc);
                    }
                }
            }
        }

        // Put the entries in the document into the dictionary. It will be keyed on all kanji and kana.
        private void ProcessDocument(XDocument doc)
        {
            IEnumerable<XElement> elements = from el in doc.Element("JMdict").Elements("entry")
                                             select el;

            List<Entry> entries = SerializeEntries(elements);
            foreach (Entry entry in entries)
            {
                if (entry.Kanji != null)
                {
                    foreach (Kanji kanji in entry.Kanji)
                    {
                        AddDictionaryEntry(kanji.ToString(), entry);
                    }
                }
                foreach (Reading kana in entry.Reading)
                {
                    AddDictionaryEntry(kana.ToString(), entry);
                }
            }
        }

        private void AddDictionaryEntry(string key, Entry entry)
        {
            if (dictionary.ContainsKey(key))
            {
                List<Entry> entries = dictionary[key];
                entries.Add(entry);
            }
            else
            {
                List<Entry> entries = new List<Entry>() { entry };
                dictionary[key] = entries;
            }
        }

        public List<Entry> FindEntries(string searchTerm)
        {
            if (dictionary.ContainsKey(searchTerm))
            {
                return dictionary[searchTerm];
            }

            return new List<Entry>();
        }

        public List<Entry> FindIchidanEntries(string searchTerm)
        {
            if (dictionary.ContainsKey(searchTerm))
            {
                List<Entry> ichidanEntries = new List<Entry>();
                foreach (Entry entry in dictionary[searchTerm])
                {
                    foreach (Meaning glossary in entry.Glossary)
                    {
                        foreach (string pos in glossary.Pos)
                        {
                            if (pos == entities["v1"])
                            {
                                ichidanEntries.Add(entry);
                            }
                        }
                    }
                }

                return ichidanEntries;
            }

            return new List<Entry>();
        }

            /*public List<Entry> FindEntry(string searchTerm)
            {
                // 1. Try to find in kanji element (k_ele). There can be multiple for one entry, or it can be missing!
                // 2. Try to find in reading element (r_ele). Can be multiple!

                IEnumerable<XElement> elements = from el in doc.Element("JMdict").Elements("entry").Elements("k_ele").Elements("keb")
                                              where el.Value == searchTerm
                                              select el.Parent.Parent;

                IEnumerable<XElement> kanaElements = from el in doc.Element("JMdict").Elements("entry").Elements("r_ele").Elements("reb")
                                                where el.Value == searchTerm
                                                select el.Parent.Parent;

                elements = elements.Concat(kanaElements);

                return SerializeEntries(elements);
            }*/

        /*    public List<Entry> FindIchidanEntry(string searchTerm)
        {
            IEnumerable<XElement> elements = from el in doc.Element("JMdict").Elements("entry").Elements("k_ele").Elements("keb")
                                             where el.Value == searchTerm &&
                                             el.Parent.Parent.Element("sense").Elements("pos").Any(r => r.Value == "Ichidan verb")
                                             select el.Parent.Parent;

            return SerializeEntries(elements);
        }*/

        private List<Entry> SerializeEntries(IEnumerable<XElement> elements)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(Entry));
            List<Entry> entries = new List<Entry>();

            foreach (XElement el in elements)
            {
                //Console.WriteLine(el);
                Entry e = (Entry)xmlSerializer.Deserialize(el.CreateReader());
                entries.Add(e);
                //Console.WriteLine(e.ToString());
            }

            return entries;
        }
    }

    [XmlRoot("entry")]
    public class Entry
    {
        [XmlElement("k_ele")]
        public List<Kanji> Kanji { get; set; }
        [XmlElement("r_ele")]
        public List<Reading> Reading { get; set; }
        [XmlElement("sense")]
        public List<Meaning> Glossary { get; set; }

        public override string ToString()
        {
            string s = String.Format("Kanji: {0}\nKana: {1}\n",
                String.Join(", ", Kanji), String.Join(", ", Reading));

            for (int i = 0; i < Glossary.Count; i++)
            {
                s += String.Format("{0}. {1}\n", i+1, Glossary[i]);
            }

            return s;
        }
    }

    public class Kanji
    {
        [XmlElement("keb")]
        public string Reading { get; set; }

        public override string ToString()
        {
            return Reading;
        }
    }

    public class Reading
    {
        [XmlElement("reb")]
        public string KanaReading { get; set; }

        public override string ToString()
        {
            return KanaReading;
        }
    }

    public class Meaning
    {
        [XmlElement("gloss")]
        public List<string> Glossary { get; set; }
        [XmlElement("pos")]
        public List<string> Pos { get; set; }
        // TODO Make a pos class with both tag and its desc

        public override string ToString()
        {
            string s = String.Join(", ", Glossary);
            foreach (string pos in Pos)
            {
                s += "\n[" + pos + "]";
            }

            return s;
        }
    }
}
