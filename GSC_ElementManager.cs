using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace GSC_Engine
{
    public class GSC_ElementManager
    {
        #region Auxiliary Classes

        private class GSC_Element
        {
            public readonly Guid ElementID;
            public string Name;
            public Dictionary<string, int> QuantitativeValues;
            public Dictionary<string, HashSet<string>> QualitativeValues;

            public GSC_Element(Guid guid, string name)
            {
                ElementID = guid;
                Name = name;
                QuantitativeValues = new Dictionary<string, int>();
                QualitativeValues = new Dictionary<string, HashSet<string>>();
            }

            public GSC_Element(string name) : this(Guid.NewGuid(), name) { }
            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"ElementID: {ElementID}");
                sb.AppendLine($"Name: {Name}");
                sb.AppendLine("QuantitativeValues:");
                foreach (var kvp in QuantitativeValues)
                {
                    sb.AppendLine($"- {kvp.Key}: {kvp.Value}");
                }
                sb.AppendLine("QualitativeValues:");
                foreach (var kvp in QualitativeValues)
                {
                    sb.AppendLine($"- {kvp.Key}:\n{string.Join(", ", kvp.Value)}");
                }
                return sb.ToString();
            }
        }

        private class GSC_ElementSummary
        {
            public class GSC_Range
            {
                public int Min;
                public int Max;
                public int Average;
            }

            public Dictionary<string, GSC_Range> QuantitativeRanges;
            public Dictionary<string, HashSet<string>> QualitativeRanges;

            public GSC_ElementSummary()
            {
                QuantitativeRanges = new Dictionary<string, GSC_Range>();
                QualitativeRanges = new Dictionary<string, HashSet<string>>();
            }
        }

        #endregion

        private static GSC_ElementManager instance;
        private Dictionary<string, List<GSC_Element>> ElementGroups;
        private GSC_ElementSummary Summary;
        private const string DefaultKey = "@all";
        private const string PoolingKey = "@result";
        private const string PresentIn = "@in";
        private bool IsPooling = false;

        private GSC_ElementManager()
        {
            ElementGroups = new Dictionary<string, List<GSC_Element>>
            {
                { DefaultKey, new List<GSC_Element>() },
                { PoolingKey, new List<GSC_Element>() }
            };
        }
        public static GSC_ElementManager Instance
        {
            get
            {
                if (instance == null) instance = new GSC_ElementManager();
                return instance;
            }
        }

        /// <summary>
        /// Process is a method that receives a message, a structure that contains a readonly string
        /// passed for another classes. This message have a format that carry arguments
        /// that are applied in methods that operates in the class data, resulting in another message
        /// that is the response of each method.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public GSC_Message Process(GSC_Message message)
        {
            if (message is GSC_Message<string, string, string> @m_sss)
            {
                switch (m_sss.Message)
                {
                    case "@sss1": return new GSC_Message<int>("@r", SelectElementsWithQualitativeValue(m_sss.Arg1, m_sss.Arg2, m_sss.Arg3));
                    case "@sss2": return new GSC_Message<int>("@r", SelectElementsWithNotQualitativeValue(m_sss.Arg1, m_sss.Arg2, m_sss.Arg3));
                    default: return new GSC_Message("@f");
                }
            }
            else if (message is GSC_Message<string, string, int> @m_ssi)
            {
                switch (m_ssi.Message)
                {
                    case "@ssi1": return new GSC_Message<int>("@r", SelectElementsWithQuantitativeValueEquals(m_ssi.Arg1, m_ssi.Arg2, m_ssi.Arg3));
                    case "@ssi2": return new GSC_Message<int>("@r", SelectElementsWithQuantitativeValueNotEquals(m_ssi.Arg1, m_ssi.Arg2, m_ssi.Arg3));
                    case "@ssi3": return new GSC_Message<int>("@r", SelectElementsWithQuantitativeValueGreater(m_ssi.Arg1, m_ssi.Arg2, m_ssi.Arg3));
                    case "@ssi4": return new GSC_Message<int>("@r", SelectElementsWithQuantitativeValueGreaterEquals(m_ssi.Arg1, m_ssi.Arg2, m_ssi.Arg3));
                    case "@ssi5": return new GSC_Message<int>("@r", SelectElementsWithQuantitativeValueLess(m_ssi.Arg1, m_ssi.Arg2, m_ssi.Arg3));
                    case "@ssi6": return new GSC_Message<int>("@r", SelectElementsWithQuantitativeValueLessEquals(m_ssi.Arg1, m_ssi.Arg2, m_ssi.Arg3));

                    default: return new GSC_Message("@f");
                }
            }
            else if (message is GSC_Message<string, string> @m_ss)
            {
                switch (m_ss.Message)
                {
                    case "ss01": return new GSC_Message<Guid[]>("@r", GetElement(m_ss.Arg1, m_ss.Arg2).ToArray());
                    case "ss02": return new GSC_Message<string>("@r", GetElement(m_ss.Arg1, Guid.Parse(m_ss.Arg2)));
                    case "ss03": return new GSC_Message<int>("@r", RemoveElement(Guid.Parse(m_ss.Arg1), m_ss.Arg2));
                    case "ss04": return new GSC_Message<int>("@r", RemoveElementsByName(m_ss.Arg1, m_ss.Arg2));
                    case "ss05": return new GSC_Message<int>("@r", SelectElementsPresentInBothDictionaries(m_ss.Arg1, m_ss.Arg2));
                    case "ss06": return new GSC_Message<int>("@r", SelectElementsNotPresentOnSecondDictionary(m_ss.Arg1, m_ss.Arg2));
                    case "ss07": return new GSC_Message<int>("@r", SelectElementsNotPresentOnBothDictionaries(m_ss.Arg1, m_ss.Arg2));
                    case "ss08": return new GSC_Message<int>("@r", SelectElementsWithQuantitativeKey(m_ss.Arg1, m_ss.Arg2));
                    case "ss09": return new GSC_Message<int>("@r", SelectElementsWithNotQuantitativeKey(m_ss.Arg1, m_ss.Arg2));
                    case "ss10": return new GSC_Message<int>("@r", SelectElementsWithQualitativeKey(m_ss.Arg1, m_ss.Arg2));
                    case "ss11": return new GSC_Message<int>("@r", SelectElementsWithNotQualitativeKey(m_ss.Arg1, m_ss.Arg2));
                    case "ss12": return new GSC_Message<int>("@r", SetQualitativeValueToElement(m_ss.Arg1, m_ss.Arg2));
                    case "ss13": return new GSC_Message<int>("@r", RemoveQuantitativeValueFromElement(m_ss.Arg1, m_ss.Arg2));
                    case "ss14": return new GSC_Message<int>("@r", RemoveQuantitativeKeyFromElement(m_ss.Arg1, m_ss.Arg2));
                    case "ss15": return new GSC_Message<int>("@r", SelectElementsFromMaxValue(m_ss.Arg1, m_ss.Arg2));
                    case "ss16": return new GSC_Message<int>("@r", SelectElementsFromNotMaxValue(m_ss.Arg1, m_ss.Arg2));
                    case "ss17": return new GSC_Message<int>("@r", SelectElementsFromMinValue(m_ss.Arg1, m_ss.Arg2));
                    case "ss18": return new GSC_Message<int>("@r", SelectElementsFromNotMinValue(m_ss.Arg1, m_ss.Arg2));
                    case "ss19": return new GSC_Message<int>("@r", SelectElementsFromMinMaxValue(m_ss.Arg1, m_ss.Arg2));
                    case "ss20": return new GSC_Message<int>("@r", SelectElementsFromAverageValue(m_ss.Arg1, m_ss.Arg2));
                    case "ss21": return new GSC_Message<int>("@r", SelectElementsFromNotAverageValue(m_ss.Arg1, m_ss.Arg2));
                    case "ss22": return new GSC_Message<int>("@r", SelectElementsFromRanges(m_ss.Arg1, m_ss.Arg2));
                    case "ss23": return new GSC_Message<int>("@r", SelectElementsNotFromRanges(m_ss.Arg1, m_ss.Arg2));
                    default: return new GSC_Message("@f");
                }
            }
            else if (message is GSC_Message<string, int> @m_si)
            {
                switch (m_si.Message)
                {
                    case "si01": return new GSC_Message<int>("@r", SelectElementsWithQuantitativeValueEquals(m_si.Arg1, m_si.Arg2));
                    case "si02": return new GSC_Message<int>("@r", SelectElementsWithQuantitativeValueNotEquals(m_si.Arg1, m_si.Arg2));
                    case "si03": return new GSC_Message<int>("@r", SelectElementsWithQuantitativeValueGreater(m_si.Arg1, m_si.Arg2));
                    case "si04": return new GSC_Message<int>("@r", SelectElementsWithQuantitativeValueGreaterEquals(m_si.Arg1, m_si.Arg2));
                    case "si05": return new GSC_Message<int>("@r", SelectElementsWithQuantitativeValueLess(m_si.Arg1, m_si.Arg2));
                    case "si06": return new GSC_Message<int>("@r", SelectElementsWithQuantitativeValueLessEquals(m_si.Arg1, m_si.Arg2));
                    case "si07": return new GSC_Message<int>("@r", SetQuantitativeValueToElement(m_si.Arg1, m_si.Arg2));
                    case "si08": return new GSC_Message<int>("@r", AddQuantitativeValueToElement(m_si.Arg1, m_si.Arg2));
                    case "si09": return new GSC_Message<int>("@r", SubtractQuantitativeValueToElement(m_si.Arg1, m_si.Arg2));
                    case "si10": return new GSC_Message<int>("@r", MultiplyQuantitativeValueToElement(m_si.Arg1, m_si.Arg2));
                    case "si11": return new GSC_Message<int>("@r", DivideQuantitativeValueToElement(m_si.Arg1, m_si.Arg2));
                    case "si12": return new GSC_Message<int>("@r", DivideRoundedUpQuantitativeValueToElement(m_si.Arg1, m_si.Arg2));
                    case "si13": return new GSC_Message<int>("@r", RemainderQuantitativeValueToElement(m_si.Arg1, m_si.Arg2));
                    case "si14": return new GSC_Message<int>("@r", SetMinQuantitativeValueToElement(m_si.Arg1, m_si.Arg2));
                    case "si15": return new GSC_Message<int>("@r", SetMaxQuantitativeValueToElement(m_si.Arg1, m_si.Arg2));

                    default: return new GSC_Message("@f");
                }
            }
            else if (message is GSC_Message<string> @m_s)
            {
                switch (m_s.Message)
                {
                    case "s01": return new GSC_Message<int>("@r", LoadElementsFromString(m_s.Arg1));
                    case "s02": return new GSC_Message<int>("@r", CreateElement(m_s.Arg1));
                    case "s03": return new GSC_Message<Guid[]>("@r", GetElement(m_s.Arg1).ToArray());
                    case "s04": return new GSC_Message<Guid[]>("@r", GetElement(m_s.Arg1).ToArray());
                    case "s05": return new GSC_Message<Guid[]>("@r", GetAll(m_s.Arg1).ToArray());
                    case "s06": return new GSC_Message<string[]>("@r", GetDictionaries(Guid.Parse(m_s.Arg1)).ToArray());
                    case "s07": return new GSC_Message<int>("@r", From(m_s.Arg1));
                    case "s08": return new GSC_Message<string[]>("@r", GetDictionariesByName(m_s.Arg1).ToArray());
                    case "s09": return new GSC_Message<int>("@r", To(m_s.Arg1));
                    case "s10": return new GSC_Message<int>("@r", RemoveKey(m_s.Arg1));
                    case "s11": return new GSC_Message<int>("@r", RemoveFromDictionaries(Guid.Parse(m_s.Arg1)));
                    case "s12": return new GSC_Message<int>("@r", RemoveAllElementsByName(m_s.Arg1));
                    case "s13": return new GSC_Message<int>("@r", RemoveElement(Guid.Parse(m_s.Arg1)));
                    case "s14": return new GSC_Message<int>("@r", RemoveElementsByName(m_s.Arg1));
                    case "s15": return new GSC_Message<int>("@r", SelectElementsWithQuantitativeKey(m_s.Arg1));
                    case "s16": return new GSC_Message<int>("@r", SelectElementsWithNotQuantitativeKey(m_s.Arg1));
                    case "s17": return new GSC_Message<int>("@r", RemoveQuantitativeKeyFromElement(m_s.Arg1));
                    case "s18": return new GSC_Message<int>("@r", SelectElementsFromMaxValue(m_s.Arg1));
                    case "s19": return new GSC_Message<int>("@r", SelectElementsFromNotMaxValue(m_s.Arg1));
                    case "s20": return new GSC_Message<int>("@r", SelectElementsFromMinValue(m_s.Arg1));
                    case "s21": return new GSC_Message<int>("@r", SelectElementsFromNotMinValue(m_s.Arg1));
                    case "s22": return new GSC_Message<int>("@r", SelectElementsFromMinMaxValue(m_s.Arg1));
                    case "s23": return new GSC_Message<int>("@r", SelectElementsFromAverageValue(m_s.Arg1));
                    case "s24": return new GSC_Message<int>("@r", SelectElementsFromNotAverageValue(m_s.Arg1));
                    case "s25": return new GSC_Message<int>("@r", SelectElementsFromRanges(m_s.Arg1));
                    case "s26": return new GSC_Message<int>("@r", SelectElementsNotFromRanges(m_s.Arg1));

                    default: return new GSC_Message("@f");
                }
            }
            else
            {
                switch (message.Message)
                {
                    case "m1": return new GSC_Message<string>("@r", GetElementsAsString());
                    case "m2": return new GSC_Message<Guid[]>("@r", GetAll().ToArray());
                    case "m3": return new GSC_Message<int>("@r", From());
                    case "m4": return new GSC_Message<int>("@r", Summarize());
                    default: return new GSC_Message("@f");
                }
            }

        }

        #region MANAGER METHODS

        //1. Transforms all data in manager to a flattened string.
        private string GetElementsAsString()
        {
            StringBuilder sb = new StringBuilder();

            // Append elements in the DefaultKey
            foreach (GSC_Element element in ElementGroups[DefaultKey])
            {
                sb.Append("@@@"); // Start of element
                sb.Append(element.Name); // Element name

                // Quantitative values
                sb.Append("@@@");
                foreach (KeyValuePair<string, int> entry in element.QuantitativeValues)
                {
                    sb.Append(entry.Key);
                    sb.Append("@");
                    sb.Append(entry.Value);
                    sb.Append("@@");
                }

                // Qualitative values
                sb.Append("@@@");
                foreach (KeyValuePair<string, HashSet<string>> entry in element.QualitativeValues)
                {
                    sb.Append(entry.Key);
                    sb.Append("@");
                    sb.Append(string.Join("@", entry.Value));
                    sb.Append("@@");
                }
            }

            return sb.ToString();
        }
        //2.Transform a flatten string to an element dictionary.
        private int LoadElementsFromString(string elementsString)
        {
            if (string.IsNullOrEmpty(elementsString)) return 0;

            // Clear existing elements in the DefaultKey
            ElementGroups[DefaultKey].Clear();

            string[] elementTokens = elementsString.Split(new string[] { "@@@" }, StringSplitOptions.RemoveEmptyEntries);

            // Process each element token
            foreach (string elementToken in elementTokens)
            {
                string[] elementData = elementToken.Split(new string[] { "@@@" }, StringSplitOptions.RemoveEmptyEntries);

                if (elementData.Length < 2)
                    continue;

                string elementName = elementData[0];

                // Create a new element
                GSC_Element element = new GSC_Element(elementName);

                // Process quantitative values
                if (elementData.Length >= 3)
                {
                    string[] quantitativeTokens = elementData[1].Split(new string[] { "@@" }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string quantitativeToken in quantitativeTokens)
                    {
                        string[] quantitativeData = quantitativeToken.Split(new char[] { '@' }, StringSplitOptions.RemoveEmptyEntries);

                        if (quantitativeData.Length != 2)
                            continue;

                        string key = quantitativeData[0];
                        int value = 0;

                        int.TryParse(quantitativeData[1], out value);

                        element.QuantitativeValues[key] = value;
                    }
                }

                // Process qualitative values
                if (elementData.Length >= 4)
                {
                    string[] qualitativeTokens = elementData[2].Split(new string[] { "@@" }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string qualitativeToken in qualitativeTokens)
                    {
                        string[] qualitativeData = qualitativeToken.Split(new char[] { '@' }, StringSplitOptions.RemoveEmptyEntries);

                        if (qualitativeData.Length < 2)
                            continue;

                        string key = qualitativeData[0];
                        HashSet<string> values = new HashSet<string>(qualitativeData.Skip(1));

                        element.QualitativeValues[key] = values;
                    }
                }

                // Add the element to the DefaultKey
                ElementGroups[DefaultKey].Add(element);
            }

            return ElementGroups[DefaultKey].Count;
        }
        //3. Create an element from a string
        private int CreateElement(string name)
        {
            GSC_Element element = new GSC_Element(name);
            ElementGroups[DefaultKey].Add(element);
            return ElementGroups[DefaultKey].Count;
        }
        //4. Get the guid list from elements with the same name
        private List<Guid> GetElement(string dictionary, string name)
        {
            return ElementGroups[dictionary]
                .Where(element => element.Name == name)
                .Select(element => element.ElementID)
                .ToList();
        }
        private List<Guid> GetElement(string name)
            => IsPooling ? GetElement(PoolingKey, name) : GetElement(DefaultKey, name);
        //5. Get the name from a guid
        private string GetElement(string dictionaryKey, Guid guid)
        {
            GSC_Element element = ElementGroups[dictionaryKey].Find(e => e.ElementID == guid);
            return element is null ? string.Empty : element.Name;
        }
        private string GetElement(Guid guid) => IsPooling ? GetElement(PoolingKey, guid) : GetElement(DefaultKey, guid);
        //6. Get all elements from a dictionary
        private List<Guid> GetAll(string dictionaryKey)
        {
            if (ElementGroups.ContainsKey(dictionaryKey)) return new List<Guid>();
            else return ElementGroups[dictionaryKey].Select(x => x.ElementID).ToList();
        }
        private List<Guid> GetAll() => IsPooling ? GetAll(PoolingKey) : new List<Guid>();
        //7. Get all dictionaries that contains a reference for an element by guid.
        private List<string> GetDictionaries(Guid guid)
        {
            GSC_Element element = ElementGroups[DefaultKey].Find(x => x.ElementID == guid);
            if (element != null) return new List<string>(element.QualitativeValues[PresentIn]);
            else return new List<string>();
        }
        //8. Get all dictionaries that contains a reference for an element by name.
        private List<string> GetDictionariesByName(string name, bool removeKeys = false)
        {
            List<GSC_Element> elements = ElementGroups[DefaultKey].FindAll(x => x.Name == name);
            if (elements.IsNullOrEmpty()) return new List<string>();
            else
            {
                HashSet<string> dictionaries = new HashSet<string>();
                elements.ForEach(x => dictionaries.UnionWith(x.QualitativeValues[PresentIn]));
                if (removeKeys is true) elements.ForEach(x => x.QualitativeValues[PresentIn].Clear());
                return dictionaries.ToList();
            }
        }
        //9. Push all elements from a dictionary to the pool.
        private int From(string dictionary)
        {
            if (ElementGroups.TryGetValue(dictionary, out List<GSC_Element> elements))
            {
                if (IsPooling)
                {
                    ElementGroups[PoolingKey] = ElementGroups[PoolingKey]
                        .Union(elements)
                        .DistinctBy(x => x.ElementID)
                        .ToList();
                }
                else
                {
                    ElementGroups[PoolingKey] = elements.DistinctBy(x => x.ElementID).ToList();
                    IsPooling = true;
                }

                return ElementGroups[PoolingKey].Count;
            }
            return 0;
        }
        private int From() => IsPooling ? From(DefaultKey) : 0;
        //10. Push all elements from Pooling Key to a dictionary
        private int To(string dictionaryKey)
        {
            if (IsPooling)
            {
                foreach (var element in ElementGroups[PoolingKey])
                {
                    element.QualitativeValues[PresentIn].Add(dictionaryKey);
                }

                if (ElementGroups.TryGetValue(dictionaryKey, out var elements))
                {
                    ElementGroups[dictionaryKey] = ElementGroups[PoolingKey]
                        .Union(elements)
                        .DistinctBy(x => x.ElementID)
                        .ToList();
                }
                else
                {
                    ElementGroups[dictionaryKey] = ElementGroups[PoolingKey]
                        .DistinctBy(x => x.ElementID)
                        .ToList();
                }

                IsPooling = false;
                return ElementGroups[dictionaryKey].Count;
            }

            return 0;
        }
        //11.Remove an entry for the dictionary. DefaultKey and PoolingKey can´t be removed.
        private int RemoveKey(string dictionaryKey)
        {
            if (ElementGroups.ContainsKey(dictionaryKey))
            {
                int count = ElementGroups[dictionaryKey].Count;
                if (count > 0) ElementGroups[dictionaryKey]
                        .ForEach(x => x.QualitativeValues[PresentIn].Remove(dictionaryKey));
                ElementGroups[dictionaryKey].Clear();
                ElementGroups.Remove(dictionaryKey);
                return count;
            }

            return 0;
        }
        //12. Remove an element from all dictionaries that have a guid
        private int RemoveFromDictionaries(Guid guid)
        {
            GSC_Element element = ElementGroups[DefaultKey].Find(x => x.ElementID == guid);
            if (element is null) return 0;
            string[] keys = element.QualitativeValues[PresentIn].ToArray();
            element.QualitativeValues[PresentIn].Clear();

            int removed = 0;
            foreach (string key in keys) removed += ElementGroups[key].RemoveAll(x => x.ElementID == guid);
            return removed;
        }
        //13. Remove all elements that match a string name that have a name
        private int RemoveAllElementsByName(string name)
        {
            List<string> keys = GetDictionariesByName(name, true);
            if (keys.Count == 0) return 0;

            int removed = 0;
            foreach (string key in keys) removed += ElementGroups[key].RemoveAll(x => x.Name == name);
            return removed;
        }
        //14. Remove an element from a dictionary that has a guid
        private int RemoveElement(Guid guid, string dictionaryKey)
        {
            if (!ElementGroups.ContainsKey(dictionaryKey)) return 0;
            else return ElementGroups[dictionaryKey].RemoveAll(x => x.ElementID == guid);
        }
        private int RemoveElement(Guid guid) => IsPooling ? RemoveElement(guid, PoolingKey) : 0;
        //15. Remove all elements that match a string name in a given dictionary
        private int RemoveElementsByName(string name, string dictionaryKey)
        {
            if (!ElementGroups.ContainsKey(dictionaryKey)) return 0;
            return ElementGroups[dictionaryKey].RemoveAll(x => x.Name == name);
        }
        private int RemoveElementsByName(string name) => IsPooling ? RemoveElementsByName(name, PoolingKey) : 0;
        //16. Select all elements that are present in both dictionaries
        public int SelectElementsPresentInBothDictionaries(string key1, string key2)
        {
            if (key1 == key2)
            {
                ElementGroups[PoolingKey].Clear();
                return 0;
            }

            ElementGroups[PoolingKey] = ElementGroups[key1]
                .Where(x => x.QualitativeValues[PresentIn].Contains(key2)).ToList();

            return ElementGroups[PoolingKey].Count;
        }
        //17. Select all elements not present in dictionary 2
        public int SelectElementsNotPresentOnSecondDictionary(string key1, string key2)
        {
            if (key1 == key2)
            {
                ElementGroups[PoolingKey].Clear();
                return 0;
            }

            ElementGroups[PoolingKey] = ElementGroups[key1]
                .Where(x => !x.QualitativeValues[PresentIn].Contains(key2)).ToList();

            return ElementGroups[PoolingKey].Count;
        }
        //18. Select all elements not present in both dictionaries
        public int SelectElementsNotPresentOnBothDictionaries(string key1, string key2)
        {
            if (key1 == key2)
            {
                ElementGroups[PoolingKey].Clear();
                return 0;
            }

            ElementGroups[PoolingKey] = ElementGroups[DefaultKey]
                .Where(x => (x.QualitativeValues[PresentIn].Contains(key1)
                && !x.QualitativeValues[PresentIn].Contains(key2)) ||
                (x.QualitativeValues[PresentIn].Contains(key2)
                && !x.QualitativeValues[PresentIn].Contains(key1))).ToList();

            return ElementGroups[PoolingKey].Count;
        }
        #endregion

        #region QUANTITATIVE SELECTORS

        //19. Select elements for a quantitative key present and move them to the pooling key
        private int SelectElementsWithQuantitativeKey(string dictionaryKey, string key)
        {
            if (!ElementGroups.ContainsKey(dictionaryKey))
                return 0;

            List<GSC_Element> elements = ElementGroups[dictionaryKey]
                .Where(x => x.QuantitativeValues.ContainsKey(key))
                .ToList();

            if (IsPooling)
            {
                ElementGroups[PoolingKey] = ElementGroups[PoolingKey]
                    .Union(elements)
                    .DistinctBy(x => x.ElementID)
                    .ToList();
            }
            else
            {
                ElementGroups[PoolingKey] = elements.DistinctBy(x => x.ElementID).ToList();
                IsPooling = true;
            }

            return ElementGroups[PoolingKey].Count;
        }
        private int SelectElementsWithQuantitativeKey(string key) =>
            SelectElementsWithQuantitativeKey(PoolingKey, key);
        //20. Select elements for a quantitative key not present and move them to the pooling key
        private int SelectElementsWithNotQuantitativeKey(string dictionaryKey, string key)
        {
            if (!ElementGroups.ContainsKey(dictionaryKey)) return 0;

            List<GSC_Element> elements = ElementGroups[dictionaryKey]
                .Where(x => !x.QuantitativeValues.ContainsKey(key))
                .ToList();

            if (IsPooling)
            {
                ElementGroups[PoolingKey] = ElementGroups[PoolingKey]
                    .Union(elements)
                    .DistinctBy(x => x.ElementID)
                    .ToList();
            }
            else
            {
                ElementGroups[PoolingKey] = elements.DistinctBy(x => x.ElementID).ToList();
                IsPooling = true;
            }

            return ElementGroups[PoolingKey].Count;
        }
        private int SelectElementsWithNotQuantitativeKey(string key) =>
            SelectElementsWithNotQuantitativeKey(PoolingKey, key);
        //21. Select elements for a quantitative value equals and move to pooling key. 
        private int SelectElementsWithQuantitativeValueEquals(string dictionaryKey, string key, int value)
        {
            if (!ElementGroups.ContainsKey(dictionaryKey)) return 0;

            List<GSC_Element> elements = ElementGroups[dictionaryKey]
                .Where(x => x.QuantitativeValues.ContainsKey(key) && x.QuantitativeValues[key] == value)
                .ToList();

            if (IsPooling)
            {
                ElementGroups[PoolingKey] = ElementGroups[PoolingKey]
                    .Union(elements)
                    .DistinctBy(x => x.ElementID)
                    .ToList();
            }
            else
            {
                ElementGroups[PoolingKey] = elements.DistinctBy(x => x.ElementID).ToList();
                IsPooling = true;
            }

            return ElementGroups[PoolingKey].Count;
        }
        private int SelectElementsWithQuantitativeValueEquals(string key, int value) =>
            SelectElementsWithQuantitativeValueEquals(PoolingKey, key, value);
        //22. Select elements for a quantitative value not equals and move to pooling key. 
        private int SelectElementsWithQuantitativeValueNotEquals(string dictionaryKey, string key, int value)
        {
            if (!ElementGroups.ContainsKey(dictionaryKey)) return 0;

            List<GSC_Element> elements = ElementGroups[dictionaryKey]
                .Where(x => x.QuantitativeValues.ContainsKey(key) && x.QuantitativeValues[key] != value)
                .ToList();

            if (IsPooling)
            {
                ElementGroups[PoolingKey] = ElementGroups[PoolingKey]
                    .Union(elements)
                    .DistinctBy(x => x.ElementID)
                    .ToList();
            }
            else
            {
                ElementGroups[PoolingKey] = elements.DistinctBy(x => x.ElementID).ToList();
                IsPooling = true;
            }

            return ElementGroups[PoolingKey].Count;
        }
        private int SelectElementsWithQuantitativeValueNotEquals(string key, int value) =>
            SelectElementsWithQuantitativeValueNotEquals(PoolingKey, key, value);
        //23. Select elements for a quantitative value greater and move to pooling key. 
        private int SelectElementsWithQuantitativeValueGreater(string dictionaryKey, string key, int value)
        {
            if (!ElementGroups.ContainsKey(dictionaryKey)) return 0;

            List<GSC_Element> elements = ElementGroups[dictionaryKey]
                .Where(x => x.QuantitativeValues.ContainsKey(key) && x.QuantitativeValues[key] > value)
                .ToList();

            if (IsPooling)
            {
                ElementGroups[PoolingKey] = ElementGroups[PoolingKey]
                    .Union(elements)
                    .DistinctBy(x => x.ElementID)
                    .ToList();
            }
            else
            {
                ElementGroups[PoolingKey] = elements.DistinctBy(x => x.ElementID).ToList();
                IsPooling = true;
            }

            return ElementGroups[PoolingKey].Count;
        }
        private int SelectElementsWithQuantitativeValueGreater(string key, int value) =>
            SelectElementsWithQuantitativeValueGreater(PoolingKey, key, value);
        //24. Select elements for a quantitative value greater or equals and move to pooling key. 
        private int SelectElementsWithQuantitativeValueGreaterEquals(string dictionaryKey, string key, int value)
        {
            if (!ElementGroups.ContainsKey(dictionaryKey)) return 0;

            List<GSC_Element> elements = ElementGroups[dictionaryKey]
                .Where(x => x.QuantitativeValues.ContainsKey(key) && x.QuantitativeValues[key] >= value)
                .ToList();

            if (IsPooling)
            {
                ElementGroups[PoolingKey] = ElementGroups[PoolingKey]
                    .Union(elements)
                    .DistinctBy(x => x.ElementID)
                    .ToList();
            }
            else
            {
                ElementGroups[PoolingKey] = elements.DistinctBy(x => x.ElementID).ToList();
                IsPooling = true;
            }

            return ElementGroups[PoolingKey].Count;
        }
        private int SelectElementsWithQuantitativeValueGreaterEquals(string key, int value) =>
            SelectElementsWithQuantitativeValueGreaterEquals(PoolingKey, key, value);
        //25. Select elements for a quantitative value less and move to pooling key. 
        private int SelectElementsWithQuantitativeValueLess(string dictionaryKey, string key, int value)
        {
            if (!ElementGroups.ContainsKey(dictionaryKey)) return 0;

            List<GSC_Element> elements = ElementGroups[dictionaryKey]
                .Where(x => x.QuantitativeValues.ContainsKey(key) && x.QuantitativeValues[key] < value)
                .ToList();

            if (IsPooling)
            {
                ElementGroups[PoolingKey] = ElementGroups[PoolingKey]
                    .Union(elements)
                    .DistinctBy(x => x.ElementID)
                    .ToList();
            }
            else
            {
                ElementGroups[PoolingKey] = elements.DistinctBy(x => x.ElementID).ToList();
                IsPooling = true;
            }

            return ElementGroups[PoolingKey].Count;
        }
        private int SelectElementsWithQuantitativeValueLess(string key, int value) =>
            SelectElementsWithQuantitativeValueLess(PoolingKey, key, value);
        //26. Select elements for a quantitative value less or equals and move to pooling key. 
        private int SelectElementsWithQuantitativeValueLessEquals(string dictionaryKey, string key, int value)
        {
            if (!ElementGroups.ContainsKey(dictionaryKey)) return 0;

            List<GSC_Element> elements = ElementGroups[dictionaryKey]
                .Where(x => x.QuantitativeValues.ContainsKey(key) && x.QuantitativeValues[key] <= value)
                .ToList();

            if (IsPooling)
            {
                ElementGroups[PoolingKey] = ElementGroups[PoolingKey]
                    .Union(elements)
                    .DistinctBy(x => x.ElementID)
                    .ToList();
            }
            else
            {
                ElementGroups[PoolingKey] = elements.DistinctBy(x => x.ElementID).ToList();
                IsPooling = true;
            }

            return ElementGroups[PoolingKey].Count;
        }
        private int SelectElementsWithQuantitativeValueLessEquals(string key, int value) =>
            SelectElementsWithQuantitativeValueLessEquals(PoolingKey, key, value);

        #endregion //QUANTITATIVE SELECTORS

        #region QUANTITATIVE OPERATORS

        //27. Set a quantitative value to all elements in the pooling.
        private int SetQuantitativeValueToElement(string key, int value)
        {
            ElementGroups[PoolingKey].ForEach(x => x.QuantitativeValues[key] = value);
            return ElementGroups[PoolingKey].Count;
        }
        //28. Remove a quantitative key from all elements in the pooling.
        private int RemoveQuantitativeKeyFromElement(string key)
        {
            ElementGroups[PoolingKey].ForEach(x => x.QuantitativeValues.Remove(key));
            return ElementGroups[PoolingKey].Count;
        }
        //29. Add a quantitative value to all elements in the pooling.
        private int AddQuantitativeValueToElement(string key, int value)
        {
            if (value is 0) return 0;
            ElementGroups[PoolingKey].ForEach(x => x.QuantitativeValues[key] += value);
            return ElementGroups[PoolingKey].Count;
        }
        //21. Subtract a quantitative value to all elements in the pooling.
        private int SubtractQuantitativeValueToElement(string key, int value)
        {
            if (value is 0) return 0;
            ElementGroups[PoolingKey].ForEach(x => x.QuantitativeValues[key] -= value);
            return ElementGroups[PoolingKey].Count;
        }
        //22. Multiply a quantitative value to all elements in the pooling.
        private int MultiplyQuantitativeValueToElement(string key, int value)
        {
            if (value is 0) return 0;
            ElementGroups[PoolingKey].ForEach(x => x.QuantitativeValues[key] *= value);
            return ElementGroups[PoolingKey].Count;
        }
        //23. Divide a quantitative value to all elements in the pooling.
        private int DivideQuantitativeValueToElement(string key, int value)
        {
            if (value is 0) return 0;
            ElementGroups[PoolingKey].ForEach(x => x.QuantitativeValues[key] /= value);
            return ElementGroups[PoolingKey].Count;
        }
        //24. Divide rounded a quantitative value to all elements in the pooling.
        private int DivideRoundedUpQuantitativeValueToElement(string key, int value)
        {
            if (value is 0) return 0;
            ElementGroups[PoolingKey].ForEach(x => x.QuantitativeValues[key] =
            (int)Math.Ceiling((double)x.QuantitativeValues[key] / value));
            return ElementGroups[PoolingKey].Count;
        }
        //25. Set the remainder of a quantitative value to all elements in the pooling.
        private int RemainderQuantitativeValueToElement(string key, int value)
        {
            if (value is 0) return 0;
            ElementGroups[PoolingKey].ForEach(x => x.QuantitativeValues[key] %= value);
            return ElementGroups[PoolingKey].Count;
        }
        //26. Set the smallest value from a quantitative key to all elements in the pooling.
        private int SetMinQuantitativeValueToElement(string key, int value)
        {
            ElementGroups[PoolingKey].ForEach(x => x.QuantitativeValues[key] =
            Math.Min(x.QuantitativeValues[key], value));
            return ElementGroups[PoolingKey].Count;
        }
        //27. Set a quantitative value to all elements in the pooling.
        private int SetMaxQuantitativeValueToElement(string key, int value)
        {
            ElementGroups[PoolingKey].ForEach(x => x.QuantitativeValues[key] =
            Math.Max(x.QuantitativeValues[key], value));
            return ElementGroups[PoolingKey].Count;
        }

        #endregion

        #region QUALITATIVE SELECTORS

        //28. Select elements that have a qualitative key
        private int SelectElementsWithQualitativeKey(string dictionaryKey, string key)
        {
            if (!ElementGroups.ContainsKey(dictionaryKey))
                return 0;

            List<GSC_Element> elements = ElementGroups[dictionaryKey]
                .Where(x => x.QualitativeValues.ContainsKey(key))
                .ToList();

            if (IsPooling)
            {
                ElementGroups[PoolingKey] = ElementGroups[PoolingKey]
                    .Union(elements)
                    .DistinctBy(x => x.ElementID)
                    .ToList();
            }
            else
            {
                ElementGroups[PoolingKey] = elements.DistinctBy(x => x.ElementID).ToList();
                IsPooling = true;
            }

            return ElementGroups[PoolingKey].Count;
        }
        private int SelectElementsWithQualitativeKey(string key) =>
            SelectElementsWithQualitativeKey(PoolingKey, key);
        //29. Select elements that don´t have a qualitative key
        private int SelectElementsWithNotQualitativeKey(string dictionaryKey, string key)
        {
            if (!ElementGroups.ContainsKey(dictionaryKey))
                return 0;

            List<GSC_Element> elements = ElementGroups[dictionaryKey]
                .Where(x => !x.QualitativeValues.ContainsKey(key))
                .ToList();

            if (IsPooling)
            {
                ElementGroups[PoolingKey] = ElementGroups[PoolingKey]
                    .Union(elements)
                    .DistinctBy(x => x.ElementID)
                    .ToList();
            }
            else
            {
                ElementGroups[PoolingKey] = elements.DistinctBy(x => x.ElementID).ToList();
                IsPooling = true;
            }

            return ElementGroups[PoolingKey].Count;
        }
        private int SelectElementsWithNotQualitativeKey(string key) =>
            SelectElementsWithNotQualitativeKey(PoolingKey, key);
        //30. Select elements that have a qualitative key
        private int SelectElementsWithQualitativeValue(string dictionaryKey, string key, string value)
        {
            if (!ElementGroups.ContainsKey(dictionaryKey))
                return 0;

            List<GSC_Element> elements = ElementGroups[dictionaryKey]
                .Where(x => x.QualitativeValues.TryGetValue(key, out HashSet<string> values) &&
                values.Contains(value)).ToList();

            if (IsPooling)
            {
                ElementGroups[PoolingKey] = ElementGroups[PoolingKey]
                    .Union(elements)
                    .DistinctBy(x => x.ElementID)
                    .ToList();
            }
            else
            {
                ElementGroups[PoolingKey] = elements.DistinctBy(x => x.ElementID).ToList();
                IsPooling = true;
            }

            return ElementGroups[PoolingKey].Count;
        }
        private int SelectElementsWithQualitativeValue(string key, string value) =>
            SelectElementsWithQualitativeValue(PoolingKey, key, value);
        //31. Select elements that have a qualitative key
        private int SelectElementsWithNotQualitativeValue(string dictionaryKey, string key, string value)
        {
            if (!ElementGroups.ContainsKey(dictionaryKey))
                return 0;

            List<GSC_Element> elements = ElementGroups[dictionaryKey]
                .Where(x => x.QualitativeValues.TryGetValue(key, out HashSet<string> values) &&
                !values.Contains(value)).ToList();

            if (IsPooling)
            {
                ElementGroups[PoolingKey] = ElementGroups[PoolingKey]
                    .Union(elements)
                    .DistinctBy(x => x.ElementID)
                    .ToList();
            }
            else
            {
                ElementGroups[PoolingKey] = elements.DistinctBy(x => x.ElementID).ToList();
                IsPooling = true;
            }

            return ElementGroups[PoolingKey].Count;
        }
        private int SelectElementsWithNotQualitativeValue(string key, string value) =>
            SelectElementsWithNotQualitativeValue(PoolingKey, key, value);

        #endregion

        #region QUALITATIVE OPERATORS

        //32. Set a qualiitative value to all elements in the pooling.
        private int SetQualitativeValueToElement(string key, string value)
        {
            ElementGroups[PoolingKey].ForEach(x => x.QualitativeValues[key].Add(value));
            return ElementGroups[PoolingKey].Count;
        }
        //33. Remove a qualitative value from all elements in the pooling.
        private int RemoveQuantitativeValueFromElement(string key, string value)
        {
            ElementGroups[PoolingKey].ForEach(x =>
            {
                if (x.QualitativeValues.ContainsKey(key))
                {
                    x.QualitativeValues[key].Remove(value);
                }
            });
            return ElementGroups[PoolingKey].Count;
        }
        //34. Remove a entire key of qualitative values from all elements in the pooling.
        private int RemoveQuantitativeKeyFromElement(string key, string value)
        {
            ElementGroups[PoolingKey].ForEach(x => x.QualitativeValues.Remove(key));
            return ElementGroups[PoolingKey].Count;
        }

        #endregion

        #region SUMMARIZATION
        private int Summarize()
        {
            // Clear the old summary data
            Summary.QuantitativeRanges.Clear();
            Summary.QualitativeRanges.Clear();

            int result = 0;
            // Get all quantitative values for each key

            var quantitativeValues = ElementGroups[DefaultKey]
                .SelectMany(element => element.QuantitativeValues)
                .GroupBy(kv => kv.Key)
                .ToDictionary(group => group.Key, group => group.Select(kv => kv.Value).ToList());

            if (quantitativeValues.Count != 0)
            {
                // Calculate min, max, and mode for each key
                foreach (string key in quantitativeValues.Keys)
                {
                    int minValue = quantitativeValues[key].Min();
                    int maxValue = quantitativeValues[key].Max();
                    int modeValue = quantitativeValues[key].GroupBy(x => x)
                                          .OrderByDescending(group => group.Count())
                                          .Select(group => group.Key)
                                          .FirstOrDefault();

                    Summary.QuantitativeRanges[key] = new GSC_ElementSummary.GSC_Range
                    {
                        Min = minValue,
                        Max = maxValue,
                        Average = modeValue
                    };
                }

                result++;
            }

            // Collect qualitative values
            Summary.QualitativeRanges = ElementGroups[DefaultKey]
                .SelectMany(element => element.QualitativeValues)
                .GroupBy(kv => kv.Key)
                .ToDictionary(group => group.Key, group => group.SelectMany(kv => kv.Value).ToHashSet());

            return result;
        }
        private int SelectElementsFromMaxValue(string dictionaryKey, string key)
        {
            if (!Summary.QuantitativeRanges.ContainsKey(key))
            {
                ElementGroups[PoolingKey].Clear();
                return 0;
            }

            ElementGroups[PoolingKey] = ElementGroups[dictionaryKey]
                .Where(x => x.QuantitativeValues.TryGetValue(key, out int value) &&
                Summary.QuantitativeRanges[key].Max == value).ToList();

            return ElementGroups[PoolingKey].Count;
        }
        private int SelectElementsFromMaxValue(string key) => SelectElementsFromMaxValue(PoolingKey, key);
        private int SelectElementsFromNotMaxValue(string dictionaryKey, string key)
        {
            if (!Summary.QuantitativeRanges.ContainsKey(key))
            {
                ElementGroups[PoolingKey].Clear();
                return 0;
            }

            ElementGroups[PoolingKey] = ElementGroups[dictionaryKey]
                .Where(x => x.QuantitativeValues.TryGetValue(key, out int value) &&
                Summary.QuantitativeRanges[key].Max != value).ToList();

            return ElementGroups[PoolingKey].Count;
        }
        private int SelectElementsFromNotMaxValue(string key) => SelectElementsFromNotMaxValue(PoolingKey, key);
        private int SelectElementsFromMinValue(string dictionaryKey, string key)
        {
            if (!Summary.QuantitativeRanges.ContainsKey(key))
            {
                ElementGroups[PoolingKey].Clear();
                return 0;
            }

            ElementGroups[PoolingKey] = ElementGroups[dictionaryKey]
                .Where(x => x.QuantitativeValues.TryGetValue(key, out int value) &&
                Summary.QuantitativeRanges[key].Min == value).ToList();

            return ElementGroups[PoolingKey].Count;
        }
        private int SelectElementsFromMinValue(string key) => SelectElementsFromMinValue(PoolingKey, key);
        private int SelectElementsFromNotMinValue(string dictionaryKey, string key)
        {
            if (!Summary.QuantitativeRanges.ContainsKey(key))
            {
                ElementGroups[PoolingKey].Clear();
                return 0;
            }

            ElementGroups[PoolingKey] = ElementGroups[dictionaryKey]
                .Where(x => x.QuantitativeValues.TryGetValue(key, out int value) &&
                Summary.QuantitativeRanges[key].Min != value).ToList();

            return ElementGroups[PoolingKey].Count;
        }
        private int SelectElementsFromNotMinValue(string key) => SelectElementsFromNotMinValue(PoolingKey, key);
        private int SelectElementsFromMinMaxValue(string dictionaryKey, string key)
        {
            if (!Summary.QuantitativeRanges.ContainsKey(key))
            {
                ElementGroups[PoolingKey].Clear();
                return 0;
            }

            ElementGroups[PoolingKey] = ElementGroups[dictionaryKey]
                .Where(x => x.QuantitativeValues.TryGetValue(key, out int value) &&
                Summary.QuantitativeRanges[key].Max == value
                || Summary.QuantitativeRanges[key].Min == value).ToList();

            return ElementGroups[PoolingKey].Count;
        }
        private int SelectElementsFromMinMaxValue(string key) => SelectElementsFromMinMaxValue(PoolingKey, key);
        private int SelectElementsFromNotMinMaxValue(string dictionaryKey, string key)
        {
            if (!Summary.QuantitativeRanges.ContainsKey(key))
            {
                ElementGroups[PoolingKey].Clear();
                return 0;
            }

            ElementGroups[PoolingKey] = ElementGroups[dictionaryKey]
                .Where(x => x.QuantitativeValues.TryGetValue(key, out int value) &&
                Summary.QuantitativeRanges[key].Max != value
                && Summary.QuantitativeRanges[key].Min != value).ToList();

            return ElementGroups[PoolingKey].Count;
        }
        private int SelectElementsFromNotMinMaxValue(string key) => SelectElementsFromNotMinMaxValue(PoolingKey, key);
        private int SelectElementsFromAverageValue(string dictionaryKey, string key)
        {
            if (!Summary.QuantitativeRanges.ContainsKey(key))
            {
                ElementGroups[PoolingKey].Clear();
                return 0;
            }

            ElementGroups[PoolingKey] = ElementGroups[dictionaryKey]
                .Where(x => x.QuantitativeValues.TryGetValue(key, out int value) &&
                Summary.QuantitativeRanges[key].Average == value).ToList();

            return ElementGroups[PoolingKey].Count;
        }
        private int SelectElementsFromAverageValue(string key) => SelectElementsFromAverageValue(PoolingKey, key);
        private int SelectElementsFromNotAverageValue(string dictionaryKey, string key)
        {
            if (!Summary.QuantitativeRanges.ContainsKey(key))
            {
                ElementGroups[PoolingKey].Clear();
                return 0;
            }

            ElementGroups[PoolingKey] = ElementGroups[dictionaryKey]
                .Where(x => x.QuantitativeValues.TryGetValue(key, out int value) &&
                Summary.QuantitativeRanges[key].Average != value).ToList();

            return ElementGroups[PoolingKey].Count;
        }
        private int SelectElementsFromNotAverageValue(string key) => SelectElementsFromNotAverageValue(PoolingKey, key);
        private int SelectElementsFromRanges(string dictionaryKey, string key)
        {
            if (!Summary.QuantitativeRanges.ContainsKey(key))
            {
                ElementGroups[PoolingKey].Clear();
                return 0;
            }

            ElementGroups[PoolingKey] = ElementGroups[dictionaryKey]
                .Where(x => x.QuantitativeValues.TryGetValue(key, out int value) &&
                (Summary.QuantitativeRanges[key].Min == value ||
                Summary.QuantitativeRanges[key].Max == value ||
                Summary.QuantitativeRanges[key].Average == value)).ToList();

            return ElementGroups[PoolingKey].Count;

        }
        private int SelectElementsFromRanges(string key) => SelectElementsFromRanges(PoolingKey, key);
        private int SelectElementsNotFromRanges(string dictionaryKey, string key)
        {
            if (!Summary.QuantitativeRanges.ContainsKey(key))
            {
                ElementGroups[PoolingKey].Clear();
                return 0;
            }

            ElementGroups[PoolingKey] = ElementGroups[dictionaryKey]
                .Where(x => x.QuantitativeValues.TryGetValue(key, out int value) &&
                Summary.QuantitativeRanges[key].Min != value &&
                Summary.QuantitativeRanges[key].Max != value &&
                Summary.QuantitativeRanges[key].Average != value).ToList();

            return ElementGroups[PoolingKey].Count;

        }
        private int SelectElementsNotFromRanges(string key) => SelectElementsNotFromRanges(PoolingKey, key);

        #endregion
    }
}

