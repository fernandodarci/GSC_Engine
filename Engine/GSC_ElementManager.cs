using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

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
        }

        #endregion

        private static GSC_ElementManager instance;
        private Dictionary<string, List<GSC_Element>> ElementGroups;
        private const string DefaultKey = "@all";
        private const string PoolingKey = "@result";
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
            return message.Message switch
            {
                "@from" => new GSC_Message<int>("@result", From((message as GSC_Message<string>).Arg1)),
                "@to" => new GSC_Message<int>("@result", To((message as GSC_Message<string>).Arg1)),
                "@getall" => new GSC_Message<Guid[]>("@result", GetElementGuids().ToArray()),
                "@get" => new GSC_Message<bool>("@result",CreateElement((message as GSC_Message<Guid,string>).Arg1,(message as GSC_Message<Guid,string>).Arg2)),
                "@erase" => new GSC_Message<bool>("@result",RemoveElement((message as GSC_Message<Guid>).Arg1)),
                //Quantitative methods
                //Validators
                "@name" => new GSC_Message<int>("@result", SelectByName((message as GSC_Message<string>).Arg1)),
                "@keyexist" => new GSC_Message<int>("@result", SelectQuantitativeKeyExists((message as GSC_Message<string>).Arg1)),
                "@keynotexist" => new GSC_Message<int>("@result", SelectQuantitativeKeyNotExists((message as GSC_Message<string>).Arg1)),
                "@equals" => new GSC_Message<int>("@result", SelectQuantitativeValueEquals((message as GSC_Message<string, int>).Arg1, (message as GSC_Message<string, int>).Arg2)),
                "@notequals" => new GSC_Message<int>("@result", SelectQuantitativeValueNotEquals((message as GSC_Message<string, int>).Arg1, (message as GSC_Message<string, int>).Arg2)),
                "@greater" => new GSC_Message<int>("@result", SelectQuantitativeValueGreater((message as GSC_Message<string, int>).Arg1, (message as GSC_Message<string, int>).Arg2)),
                "@greaterequals" => new GSC_Message<int>("@result", SelectQuantitativeValueGreaterEquals((message as GSC_Message<string, int>).Arg1, (message as GSC_Message<string, int>).Arg2)),
                "@less" => new GSC_Message<int>("@result", SelectQuantitativeValueLess((message as GSC_Message<string, int>).Arg1, (message as GSC_Message<string, int>).Arg2)),
                "@lessequals" => new GSC_Message<int>("@result", SelectQuantitativeValueLessEquals((message as GSC_Message<string, int>).Arg1, (message as GSC_Message<string, int>).Arg2)),
                "@higher" => new GSC_Message<int>("@result", SelectQuantitativeValueHighAmong((message as GSC_Message<string>).Arg1)),
                "@lowest" => new GSC_Message<int>("@result", SelectQuantitativeValueLessAmong((message as GSC_Message<string>).Arg1)),
                "@mostcommon" => new GSC_Message<int>("@result", SelectQuantitativeValueMostCommomAmong((message as GSC_Message<string>).Arg1)),
                "@lesscommon" => new GSC_Message<int>("@result", SelectQuantitativeValueLessCommonAmong((message as GSC_Message<string>).Arg1)),
                "@distinct" => new GSC_Message<int>("@result", SelectQuantitativeDistinctValues((message as GSC_Message<string>).Arg1)),
                //Operators
                "@set" => new GSC_Message<int>("@result", SetQuantitativeValue((message as GSC_Message<string, int>).Arg1, (message as GSC_Message<string, int>).Arg2)),
                "@remove" => new GSC_Message<int>("@result", RemoveQuantitativeValue((message as GSC_Message<string>).Arg1)),
                "@add" => new GSC_Message<int>("@result", SetQuantitativeValue((message as GSC_Message<string, int>).Arg1, (message as GSC_Message<string, int>).Arg2)),
                "@subtract" => new GSC_Message<int>("@result", AddQuantitativeValue((message as GSC_Message<string, int>).Arg1, (message as GSC_Message<string, int>).Arg2)),
                "@multiply" => new GSC_Message<int>("@result", SubtractQuantitativeValue((message as GSC_Message<string, int>).Arg1, (message as GSC_Message<string, int>).Arg2)),
                "@divide" => new GSC_Message<int>("@result", MultiplyQuantitativeValue((message as GSC_Message<string, int>).Arg1, (message as GSC_Message<string, int>).Arg2)),
                "@dividerounded" => new GSC_Message<int>("@result", DivideQuantitativeValue((message as GSC_Message<string, int>).Arg1, (message as GSC_Message<string, int>).Arg2)),
                "@remainder" => new GSC_Message<int>("@result", DivideRoundedUpQuantitativeValue((message as GSC_Message<string, int>).Arg1, (message as GSC_Message<string, int>).Arg2)),
                "@min" => new GSC_Message<int>("@result", MinQuantitativeValue((message as GSC_Message<string, int>).Arg1, (message as GSC_Message<string, int>).Arg2)),
                "@max" => new GSC_Message<int>("@result", MaxQuantitativeValue((message as GSC_Message<string, int>).Arg1, (message as GSC_Message<string, int>).Arg2)),
                //Qualitative Methods
                //Validators
                "@isgroup" => new GSC_Message<int>("@result",SelectQuantitativeKeyExists((message as GSC_Message<string>).Arg1)),
                "@notgroup" => new GSC_Message<int>("@result",SelectQuantitativeKeyNotExists((message as GSC_Message<string>).Arg1)),
                "@isqly" => new GSC_Message<int>("@result", SelectQualitativeValuePresent((message as GSC_Message<string, string>).Arg1, (message as GSC_Message<string, string>).Arg2)),
                "@notqly" => new GSC_Message<int>("@result", SelectQualitativeValueNotPresent((message as GSC_Message<string, string>).Arg1, (message as GSC_Message<string, string>).Arg2)),
                //Operators
                "@setqly" => new GSC_Message<int>("@result", SetQualitativeValue((message as GSC_Message<string, string>).Arg1, (message as GSC_Message<string, string>).Arg2)),
                "@removeqly" => new GSC_Message<int>("@result", SetQualitativeValue((message as GSC_Message<string, string>).Arg1, (message as GSC_Message<string, string>).Arg2)),
                _ => new GSC_Message("Unrecognized")
            };
        }

        #region MANAGER METHODS

        //1. Key method to all operations. Indicates that a dictionary entry need to be "pushed" to pool. 
        private int From(string origin)
        {
            if (IsPooling == true || string.IsNullOrEmpty(origin) || !ElementGroups.ContainsKey(origin)) return 0;
            ElementGroups[PoolingKey] = ElementGroups[origin].ToList();
            IsPooling = true;
            return ElementGroups[PoolingKey].Count;
        }

        //2. Key method to all operations. Indicates that the pool need to be release, or allocated in another key.
        private int To(string target)
        {
            if (IsPooling == false || string.IsNullOrEmpty(target) || ElementGroups[PoolingKey].Count == 0) return 0;
            if (ElementGroups.ContainsKey(target))
                ElementGroups[target] = ElementGroups[target].Union(ElementGroups[PoolingKey]).ToList();
            else ElementGroups[target] = ElementGroups[PoolingKey].ToList();
            IsPooling = false;
            return ElementGroups[target].Count;
        }

        //3. Method to get the pooling element guid to pass to interface
        public List<Guid> GetElementGuids()
        {
            if (ElementGroups.TryGetValue(PoolingKey, out var rangedElements))
                return rangedElements.Select(e => e.ElementID).ToList();

            return new List<Guid>();
        }

        //4. Create an empty element from a guid and a name.
        public bool CreateElement(Guid guid, string name)
        {
            if (ElementGroups[DefaultKey].Any(x => x.ElementID == guid)) return false;
            GSC_Element newElement = new GSC_Element(guid, name);
            ElementGroups[DefaultKey].Add(newElement);
            return true;
        }

        //5. Remove element
        public bool RemoveElement(Guid guid)
        {
            if (!ElementGroups[DefaultKey].Any(x => x.ElementID == guid)) return false;

            foreach (var list in ElementGroups.Values)
            {
                list.RemoveAll(x => x.ElementID == guid);
            }

            return true;
        }

        #endregion

        #region QUANTITATIVE METHODS

        #region QUANTITATIVE VALIDATIONS

        private int SelectByName(string name)
        {
            if (string.IsNullOrEmpty(name)) return 0;

            var pooledElements = ElementGroups[PoolingKey];
            var selectedElements = pooledElements.Where(e => e.Name.Equals(name)).ToList();

            if (selectedElements.Count > 0)
            {
                ElementGroups[PoolingKey] = selectedElements;
                return selectedElements.Count;
            }

            return 0;
        }

        private int SelectQuantitativeKeyExists(string key)
        {
            if (string.IsNullOrEmpty(key)) return 0;

            var pooledElements = ElementGroups[PoolingKey];
            var selectedElements = pooledElements.Where(e => e.QuantitativeValues.ContainsKey(key)).ToList();

            if (selectedElements.Count > 0)
            {
                ElementGroups[PoolingKey] = selectedElements;
                return selectedElements.Count;
            }

            return 0;
        }

        private int SelectQuantitativeKeyNotExists(string key)
        {
            if (string.IsNullOrEmpty(key)) return 0;

            var pooledElements = ElementGroups[PoolingKey];
            var selectedElements = pooledElements.Where(e => !e.QuantitativeValues.ContainsKey(key)).ToList();

            if (selectedElements.Count > 0)
            {
                ElementGroups[PoolingKey] = selectedElements;
                return selectedElements.Count;
            }

            return 0;
        }

        private int SelectQuantitativeValueEquals(string key, int value)
        {
            if (string.IsNullOrEmpty(key)) return 0;

            var pooledElements = ElementGroups[PoolingKey];
            var selectedElements = pooledElements.Where(e => e.QuantitativeValues.TryGetValue(key, out int val) && val == value).ToList();

            if (selectedElements.Count > 0)
            {
                ElementGroups[PoolingKey] = selectedElements;
                return selectedElements.Count;
            }

            return 0;
        }

        private int SelectQuantitativeValueNotEquals(string key, int value)
        {
            if (string.IsNullOrEmpty(key)) return 0;

            var pooledElements = ElementGroups[PoolingKey];
            var selectedElements = pooledElements.Where(e => !e.QuantitativeValues.TryGetValue(key, out int val) || val != value).ToList();

            if (selectedElements.Count > 0)
            {
                ElementGroups[PoolingKey] = selectedElements;
                return selectedElements.Count;
            }

            return 0;
        }

        private int SelectQuantitativeValueGreater(string key, int value)
        {
            if (string.IsNullOrEmpty(key)) return 0;

            var pooledElements = ElementGroups[PoolingKey];
            var selectedElements = pooledElements.Where(e => e.QuantitativeValues.TryGetValue(key, out int val) && val > value).ToList();

            if (selectedElements.Count > 0)
            {
                ElementGroups[PoolingKey] = selectedElements;
                return selectedElements.Count;
            }

            return 0;
        }

        private int SelectQuantitativeValueGreaterEquals(string key, int value)
        {
            if (string.IsNullOrEmpty(key)) return 0;

            var pooledElements = ElementGroups[PoolingKey];
            var selectedElements = pooledElements.Where(e => e.QuantitativeValues.TryGetValue(key, out int val) && val >= value).ToList();

            if (selectedElements.Count > 0)
            {
                ElementGroups[PoolingKey] = selectedElements;
                return selectedElements.Count;
            }

            return 0;
        }

        private int SelectQuantitativeValueLess(string key, int value)
        {
            if (string.IsNullOrEmpty(key)) return 0;

            var pooledElements = ElementGroups[PoolingKey];
            var selectedElements = pooledElements.Where(e => e.QuantitativeValues.TryGetValue(key, out int val) && val < value).ToList();

            if (selectedElements.Count > 0)
            {
                ElementGroups[PoolingKey] = selectedElements;
                return selectedElements.Count;
            }

            return 0;
        }

        private int SelectQuantitativeValueLessEquals(string key, int value)
        {
            if (string.IsNullOrEmpty(key)) return 0;

            var pooledElements = ElementGroups[PoolingKey];
            var selectedElements = pooledElements.Where(e => e.QuantitativeValues.TryGetValue(key, out int val) && val <= value).ToList();

            if (selectedElements.Count > 0)
            {
                ElementGroups[PoolingKey] = selectedElements;
                return selectedElements.Count;
            }

            return 0;
        }

        private int SelectQuantitativeValueHighAmong(string key)
        {
            if (string.IsNullOrEmpty(key)) return 0;

            var pooledElements = ElementGroups[PoolingKey];
            var selectedElements = pooledElements.OrderByDescending(e => e.QuantitativeValues.GetValueOrDefault(key)).ToList();

            if (selectedElements.Count > 0)
            {
                ElementGroups[PoolingKey] = selectedElements;
                return selectedElements.Count;
            }

            return 0;
        }

        private int SelectQuantitativeValueLessAmong(string key)
        {
            if (string.IsNullOrEmpty(key)) return 0;

            var pooledElements = ElementGroups[PoolingKey];
            var selectedElements = pooledElements.OrderBy(e => e.QuantitativeValues.GetValueOrDefault(key)).ToList();

            if (selectedElements.Count > 0)
            {
                ElementGroups[PoolingKey] = selectedElements;
                return selectedElements.Count;
            }

            return 0;
        }

        private int SelectQuantitativeValueMostCommomAmong(string key)
        {
            if (string.IsNullOrEmpty(key)) return 0;

            var pooledElements = ElementGroups[PoolingKey];
            var selectedElements = pooledElements
                .OrderByDescending(e => e.QuantitativeValues.TryGetValue(key, out int val) ? val : 0)
                .ToList();

            if (selectedElements.Count > 0)
            {
                ElementGroups[PoolingKey] = selectedElements;
                return selectedElements.Count;
            }

            return 0;
        }

        private int SelectQuantitativeValueLessCommonAmong(string key)
        {
            if (string.IsNullOrEmpty(key)) return 0;

            var pooledElements = ElementGroups[PoolingKey];
            var selectedElements = pooledElements
                .OrderBy(e => e.QuantitativeValues.TryGetValue(key, out int val) ? val : 0)
                .ToList();

            if (selectedElements.Count > 0)
            {
                ElementGroups[PoolingKey] = selectedElements;
                return selectedElements.Count;
            }

            return 0;
        }

        private int SelectQuantitativeDistinctValues(string key)
        {
            if (string.IsNullOrEmpty(key)) return 0;

            var pooledElements = ElementGroups[PoolingKey];
            var selectedElements = pooledElements
                .Where(e => e.QuantitativeValues.ContainsKey(key))
                .GroupBy(e => e.QuantitativeValues[key])
                .Select(g => g.Key)
                .ToList();

            if (selectedElements.Count > 0)
            {
                ElementGroups[PoolingKey] = selectedElements.SelectMany(v => pooledElements.Where(e => e.QuantitativeValues[key] == v)).ToList();
                return selectedElements.Count;
            }

            return 0;
        }

        #endregion

        #region QUANTITATIVE OPERATIONS
        private int SetQuantitativeValue(string key, int value)
        {
            if (IsPooling)
            {
                foreach (var element in ElementGroups[PoolingKey])
                {
                    element.QuantitativeValues[key] = value;
                }
                return ElementGroups[PoolingKey].Count;
            }
            return 0;
        }

        private int RemoveQuantitativeValue(string key)
        {
            if (IsPooling)
            {
                foreach (var element in ElementGroups[PoolingKey])
                {
                    element.QuantitativeValues.Remove(key);
                }
                return ElementGroups[PoolingKey].Count;
            }
            return 0;
        }

        private int AddQuantitativeValue(string key, int value)
        {
            if (IsPooling && value != 0)
            {
                foreach (var element in ElementGroups[PoolingKey])
                {
                    if (element.QuantitativeValues.ContainsKey(key))
                    {
                        element.QuantitativeValues[key] += value;
                    }
                }
                return ElementGroups[PoolingKey].Count;
            }
            return 0;
        }

        private int SubtractQuantitativeValue(string key, int value)
        {
            if (IsPooling && value != 0)
            {
                foreach (var element in ElementGroups[PoolingKey])
                {
                    if (element.QuantitativeValues.ContainsKey(key))
                    {
                        element.QuantitativeValues[key] -= value;
                    }
                }
                return ElementGroups[PoolingKey].Count;
            }
            return 0;
        }

        private int MultiplyQuantitativeValue(string key, int value)
        {
            if (IsPooling && value != 0)
            {
                foreach (var element in ElementGroups[PoolingKey])
                {
                    if (element.QuantitativeValues.ContainsKey(key))
                    {
                        element.QuantitativeValues[key] *= value;
                    }
                }
                return ElementGroups[PoolingKey].Count;
            }
            return 0;
        }

        private int DivideQuantitativeValue(string key, int value)
        {
            if (IsPooling && value != 0)
            {
                foreach (var element in ElementGroups[PoolingKey])
                {
                    if (element.QuantitativeValues.ContainsKey(key))
                    {
                        element.QuantitativeValues[key] /= value;
                    }
                }
                return ElementGroups[PoolingKey].Count;
            }
            return 0;
        }

        private int DivideRoundedUpQuantitativeValue(string key, int value)
        {
            if (IsPooling && value != 0)
            {
                foreach (var element in ElementGroups[PoolingKey])
                {
                    if (element.QuantitativeValues.ContainsKey(key))
                    {
                        element.QuantitativeValues[key] = Mathf.CeilToInt((float)element.QuantitativeValues[key] / value);
                    }
                }
                return ElementGroups[PoolingKey].Count;
            }
            return 0;
        }

        private int RemainderQuantitativeValue(string key, int value)
        {
            if (IsPooling && value != 0)
            {
                foreach (var element in ElementGroups[PoolingKey])
                {
                    if (element.QuantitativeValues.ContainsKey(key))
                    {
                        element.QuantitativeValues[key] %= value;
                    }
                }
                return ElementGroups[PoolingKey].Count;
            }
            return 0;
        }

        private int MinQuantitativeValue(string key, int value)
        {
            if (IsPooling && value != 0)
            {
                foreach (var element in ElementGroups[PoolingKey])
                {
                    if (element.QuantitativeValues.ContainsKey(key))
                    {
                        element.QuantitativeValues[key] = Mathf.Min(element.QuantitativeValues[key], value);
                    }
                }
                return ElementGroups[PoolingKey].Count;
            }
            return 0;
        }

        private int MaxQuantitativeValue(string key, int value)
        {
            if (IsPooling && value != 0)
            {
                foreach (var element in ElementGroups[PoolingKey])
                {
                    if (element.QuantitativeValues.ContainsKey(key))
                    {
                        element.QuantitativeValues[key] = Mathf.Max(element.QuantitativeValues[key], value);
                    }
                }
                return ElementGroups[PoolingKey].Count;
            }
            return 0;
        }

        #endregion

        #endregion

        #region QUALITATIVE METHODS

        #region QUALITATIVE VALIDATION METHODS
        private int SelectQualitativeKeyContains(string key)
        {
            if (!IsPooling) return 0;

            ElementGroups[PoolingKey] = ElementGroups[PoolingKey]
                .Where(x => x.QualitativeValues.ContainsKey(key)).ToList();
            return ElementGroups[PoolingKey].Count;
        }

        private int SelectQualitativeKeyNotContains(string key)
        {
            if (!IsPooling) return 0;

            ElementGroups[PoolingKey] = ElementGroups[PoolingKey]
                .Where(x => !x.QualitativeValues.ContainsKey(key)).ToList();
            return ElementGroups[PoolingKey].Count;
        }

        private int SelectQualitativeValuePresent(string key, string value)
        {
            if (!IsPooling) return 0;
            
            ElementGroups[PoolingKey] = ElementGroups[PoolingKey]
                .Where(x => x.QualitativeValues.TryGetValue(key, out HashSet<string> val) && val.Contains(value))
                .ToList();
            return ElementGroups[PoolingKey].Count;
        }

        private int SelectQualitativeValueNotPresent(string key, string value)
        {
            if (!IsPooling) return 0;
            
            ElementGroups[PoolingKey] = ElementGroups[PoolingKey]
                .Where(x => x.QualitativeValues.TryGetValue(key, out HashSet<string> val) && val.Contains(value)).ToList();
            return ElementGroups[PoolingKey].Count;
        }

        private string[] GetQualitiesFromKey(string key)
        {
            if (!IsPooling) return null;
            
            var qualities = ElementGroups[PoolingKey]
              .Where(element => element.QualitativeValues.ContainsKey(key))
              .SelectMany(element => element.QualitativeValues[key])
              .Distinct()
              .ToArray();

            return qualities;
        }

      
        #endregion

        #region QUALITATIVE OPERATION METHODS

        private int SetQualitativeValue(string key,string value)
        {
            if (!IsPooling) return 0;

            ElementGroups[PoolingKey].ForEach(element =>
            {
                if (element.QualitativeValues is null)
                    element.QualitativeValues = new Dictionary<string, HashSet<string>>();
                
                if (element.QualitativeValues.ContainsKey(key)) element.QualitativeValues[key].Add(value);
                else element.QualitativeValues.Add(key, new HashSet<string>() { value });
            });

            return ElementGroups[PoolingKey].Count();
        }

        private int RemoveQualitativeValue(string key, string value)
        {
            if (!IsPooling) return 0;
            ElementGroups[PoolingKey].ForEach(element =>
            { 
                if(element.QualitativeValues.TryGetValue(key,out var data)) data.RemoveWhere(x => x == value);
            });

            return ElementGroups[PoolingKey].Count;
        }

        #endregion

        #endregion
    }
}

