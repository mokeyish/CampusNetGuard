using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace CampusNetGuard.Libraries
{
    class KeyValuePair<TKey,TValue>
    {

        public KeyValuePair(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }
        public TKey Key { get; private set; }
        public TValue Value { get; private set; }
    }
    class ListKeyValue
    {
        public IStringCrypt Crypt { get; set; }
        private List<string> _list;
        private Dictionary<string, string> _dict;
        public int Capacity { get; set; }
        public string Titile { get; private set; }
        public void SetTitile(string notification,string keyName,string valueName)
        {
            Titile = string.Format("{0}\t{1}\t{2}", notification, keyName, valueName);
        }
        public ListKeyValue(int capacity)
        {
            _list = new List<string>();
            _dict = new Dictionary<string, string>();
            Capacity = capacity;
        }
        public ListKeyValue() : this(5) { }
        public int Count
        {
            get
            {
                return _list.Count;
            }
        }
        public bool IsChanged = false;
        public List<string> ListKey
        {
            get
            {
                return _list;
            }
        }
        public string this[string index]
        {
            get
            {
                try
                {
                    return _dict[index];
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
        public KeyValuePair<string, string> this[int index]
        {
            get
            {

                try
                {
                    string k = _list[index];
                    return new KeyValuePair<string,string>(k,_dict[k]);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
        public void Load(string source)
        {
            if (string.IsNullOrEmpty(source)) return;
            if (Count != 0) Clear();
            using (StringReader sr = new StringReader(source))
            {
                Titile = sr.ReadLine();
                string[] x, t;
                t = Titile.Split('\t');
                while (sr.Peek() > -1)
                {
                    x = sr.ReadLine().Split('\t');
                    x[1] = x[1].Substring(t[1].Length+1);
                    x[2] = x[2].Substring(t[2].Length+1);
                    _list.Add(x[1]);
                    _dict.Add(x[1], Crypt == null ? x[2] : Crypt.Decode(x[2]));
                }
            }
        }

        public void Add(string key,string value)
        {
            if (key == null)
            {
                throw new ArgumentNullException();
            }
            //如果包含此key
            if (Contains(key))
            {
                //如果完全一样，不执行操作返回
                if (_list[0] == key && _dict[key] == value)
                {
                    return;
                }
                //移除
                InternalRemove(key);
            }
            InternalAdd(key, value);
            FireOnChangedEvent();
        }
        public void RemoveAt(int index)
        {
            try
            {
                Remove(_list[index]);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void Remove(string key)
        {
            try
            {
                if (_dict.ContainsKey(key))
                {
                    InternalRemove(key);
                    FireOnChangedEvent();
                }
                else
                {
                    throw new IndexOutOfRangeException();
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public void Clear()
        {
            _list.Clear();
            _dict.Clear();
            FireOnChangedEvent();
        }

        public bool Contains(string key)
        {
            return _list.Contains(key);
        }
        private void InternalRemove(string key)
        {
            try
            {
                _list.Remove(key);
                _dict.Remove(key);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        private void InternalAdd(string key, string value)
        {
            _list.Insert(0, key);
            _dict.Add(key, value);
            //维持容量,移除最后一个
            if (Capacity != 0 && Capacity < _list.Count)
            {
                RemoveAt(Capacity);
            }
        }
        private void FireOnChangedEvent()
        {
            if (OnChanged != null)
            {
                OnChanged(this, EventArgs.Empty);
            }
            IsChanged = true;
        }
        public event EventHandler OnChanged;
        public override string ToString()
        {
            if (_list.Count == 0) return string.Empty;
            using (StringWriter sw = new StringWriter())
            {
                if (string.IsNullOrEmpty(Titile))
                {
                    Titile = "key and value Infomation.\tKey\tvalue";
                }
                string m, n;
                string[] t = Titile.Split('\t');
                sw.WriteLine(Titile);
                for (int i = 0; i < Count; i++)
                {
                    m = _list[i];
                    n = Crypt == null ? _dict[m] : Crypt.Encode(_dict[m]);
                    sw.WriteLine(string.Format("{0}\t{1}:{2}\t{3}:{4}", i, t[1], m, t[2], n));
                }
                return sw.ToString();
            }

        }
    }
}
