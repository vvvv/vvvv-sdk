using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Lib
{
    public delegate void HolderEventDelegate(string key);

    public class DataHolder<T>
    {
        private Dictionary<string, List<T>> data = new Dictionary<string, List<T>>();
        private Dictionary<string, int> instancecount = new Dictionary<string,int>();

        public event HolderEventDelegate OnUpdate;
        public event HolderEventDelegate OnRemove;
        public event HolderEventDelegate OnAdd;
        

        public void RemoveInstance(string key)
        {
            if (this.instancecount.ContainsKey(key))
            {
                int i = this.instancecount[key];
                i--;
                if (i <= 0)
                {
                    this.instancecount.Remove(key);
                    this.data.Remove(key);
                    if (OnRemove != null)
                    {
                        OnRemove(key);
                    }
                }
            }
        }

        public void AddInstance(string key)
        {
            if (!this.instancecount.ContainsKey(key))
            {
                this.instancecount.Add(key, 1);
                this.data.Add(key, new List<T>());
                if (OnAdd != null)
                {
                    OnAdd(key);
                }
            }
            else
            {
                this.instancecount[key]++;
            }
        }

        public void UpdateData(string key, List<T> data)
        {
            if (this.data.ContainsKey(key))
            {
                this.data[key] = data;
                if (this.OnUpdate != null)
                {
                    OnUpdate(key);
                }
            }
        }

        public List<T> GetData(string key, out bool found)
        {
            if (this.data.ContainsKey(key))
            {
                found = true;
                return this.data[key];
            }
            else
            {
                found = false;
                return new List<T>();
            }
        }

    }
}
