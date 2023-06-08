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
                QualitativeValues.Add(PresentIn, new HashSet<string>());
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

        public GSC_Message Process(GSC_Message message)
        {
            if (message is GSC_Message<string, string, string> @msss)
            {
                switch (msss.Message)
                {
                    default: return new GSC_Message("@N");
                }
            }
            else if (message is GSC_Message<string, string, int> @mssi)
            {
                switch (mssi.Message)
                {
                    default: return new GSC_Message("@N");
                }
            }
            else if (message is GSC_Message<string, string> @mss)
            {
                switch (mss.Message)
                {
                    default: return new GSC_Message("@N");
                }
            }
            else if (message is GSC_Message<string, int> @msi)
            {
                switch (msi.Message)
                {
                    default: return new GSC_Message("@N");
                }
            }
            else if (message is GSC_Message<string> @ms)
            {
                switch (ms.Message)
                {
                    default: return new GSC_Message("@N");
                }
            }
            else
            {
                switch (message.Message)
                {
                    default: return new GSC_Message("@N");
                }
            }
        }

        #region METHODS THAT MANIPULATES THE INNER DICTIONARY
        private int SetKeyInDictionary(string key)
        {
            if (ElementGroups.ContainsKey(key)) return 0;
            else ElementGroups.Add(key, new List<GSC_Element>());
            return ElementGroups.Keys.Count;
        }
        private int RemoveKeyInDictionary(string key)
        {
            if (!ElementGroups.ContainsKey(key)) return 0;
            else
            {
                ElementGroups[key].ForEach(x => x.QualitativeValues[PresentIn].Remove(key));
                ElementGroups[key].Clear();
            }
            return ElementGroups.Keys.Count;
        }
        private int MoveFromDictionaryToPooling(string key)
        {
            if (!ElementGroups.ContainsKey(key)) return 0;
            else
            {
                List<GSC_Element> elements = new List<GSC_Element>();
                elements.AddRange(ElementGroups[key]);
                if (IsPooling)
                {
                    elements.AddRange(ElementGroups[PoolingKey]);
                    elements = elements.DistinctBy(x => x.ElementID).ToList();
                }
                
                elements.ForEach(x => x.QualitativeValues[PresentIn].Remove(key));
                ElementGroups[PoolingKey] = elements;
                ElementGroups[key].Clear();
                ElementGroups.Remove(key);
                IsPooling = true;
            }
            return ElementGroups[PoolingKey].Count;
        }
        private int MoveFromDefaultToPooling()
        {
            ElementGroups[PoolingKey] = ElementGroups[DefaultKey].ToList();
            IsPooling = true;
            return ElementGroups[PoolingKey].Count;
        }
        private int MoveFromPoolingToDictionary(string key)
        {
            if (ElementGroups[PoolingKey].Count == 0) return 0;
            List<GSC_Element> elements = ElementGroups[PoolingKey].ToList();
            ElementGroups[PoolingKey].Clear();

            if (ElementGroups.ContainsKey(key))
            {
                elements.AddRange(ElementGroups[key]);
                elements = elements.DistinctBy(x => x.ElementID).ToList();
            }

            elements.ForEach(x => x.QualitativeValues[PresentIn].Add(key));
            ElementGroups[key] = elements;
            IsPooling = false;
            return ElementGroups[key].Count;
        }
        private int FlushPool()
        {
            int count = ElementGroups[PoolingKey].Count;
            ElementGroups[PoolingKey].Clear();
            IsPooling = false;
            return count;
        }
        private int CountElementsFromDictionary(string key)
        {
            return ElementGroups.ContainsKey(key) ? ElementGroups[key].Count : 0;
        }
        private int CountElements()
        {
            return ElementGroups[DefaultKey].Count;
        }
        private string[] GetAllElementNamesInDictionary(string key)
        {
            return ElementGroups.ContainsKey(key) ? ElementGroups[key].Select(x => x.Name).ToArray() : new string[0];
        }
        private string[] GetAllElementNames()
        {
            return ElementGroups[DefaultKey].Select(x => x.Name).ToArray();
        }
        private string[] GetAllElementNamesInPool()
        {
            if (!IsPooling) return new string[0];
            else return ElementGroups[PoolingKey].Select(x => x.Name).ToArray();
        }
        private string[] GetElementsByIDFromDictionary(string key)
        {
            return ElementGroups.ContainsKey(key) ? ElementGroups[key].Select(x => x.ElementID.ToString()).ToArray() :
                new string[0];
        }
        private string[] GetAllElementsID()
        {
            return ElementGroups[DefaultKey].Select(x => x.ElementID.ToString()).ToArray();
        }
        private string[] GetAllElementsIDInPool()
        {
            if (!IsPooling) return new string[0];
            return ElementGroups[PoolingKey].Select(x => x.ElementID.ToString()).ToArray();
        }
        #endregion
    }
}

