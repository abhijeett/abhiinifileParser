using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace IniFileParser
{
    public class IniFile
    {
        public List<Section> Sections { get; }

        public string Path { get; set; }

        public IniFile(string filePath)
        {
            Path = filePath;
            Sections = new List<Section>();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var section in Sections)
            {
                sb.AppendLine(section.ToString());
            }

            return sb.ToString();
        }

        public void Parse()
        {
            List<string> lines = File.ReadAllLines(Path).Select(e => e.Trim()).ToList();
            Section currentSection = null;
            foreach (var line in lines)
            {
                if (string.IsNullOrEmpty(line)) continue;
                if (IsComment(line)) continue;

                else if (IsSection(line))
                {
                    if(FindOrCreate(line, out currentSection))
                    {
                        this.Sections.Add(currentSection);
                    }
                }
                else
                {
                    currentSection.ParseLine(line);
                }

                if (currentSection == null)
                {
                    throw new ApplicationException("something is wrong");
                }

            }

        }

        private bool IsComment(string line)
        {
            return line[0] == '#';
        }

        private bool IsSection(string line)
        {
            return line[0] == '[';
        }

        private bool FindOrCreate(string sectionName, out Section section)
        {
            sectionName = sectionName
                .Substring(1, sectionName.EndsWith("]") ? sectionName.Length - 2 : sectionName.Length - 1);
            section = this.Sections.FirstOrDefault(e => e.Name == sectionName.Trim());
            if (section == null)
            {
                section = new Section(sectionName);
                return true;
            }
            return false;
        }
    }

    public class Section
    {
        public string Name { get; set; }

        public Dictionary<string, List<string>> KeyValuePairs { get; set; }

        public Section(string name)
        {
            Name = name;
            KeyValuePairs = new Dictionary<string, List<string>>();
        }

        public void ParseLine(string line)
        {
            var key = line.Substring(0, line.IndexOf('='));
            var value = line.Substring(line.IndexOf('=') + 1);
            var values = FindOrCreateKey(key);
            values.Add(value);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Section Name:- " + Name);
            foreach (var keyValuePair in KeyValuePairs)
            {
                sb.Append(keyValuePair.Key + " = ");
                foreach (string s in keyValuePair.Value)
                {
                    sb.Append(s + ",");
                }

                sb = new StringBuilder(sb.ToString().TrimEnd(','));
                sb.AppendLine();
            }

            return sb.ToString().TrimEnd(',');
        }

        private List<string> FindOrCreateKey(string key)
        {
            key = key.Trim();
            if (KeyValuePairs.ContainsKey(key))
            {
                return KeyValuePairs[key];
            }
            else
            {
                var keyValues = new List<string>();
                KeyValuePairs.Add(key, keyValues);
                return keyValues;
            }
        }
    }
}
