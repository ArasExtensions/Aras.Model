/*  
  Copyright 2017 Processwall Limited

  Licensed under the Apache License, Version 2.0 (the "License");
  you may not use this file except in compliance with the License.
  You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

  Unless required by applicable law or agreed to in writing, software
  distributed under the License is distributed on an "AS IS" BASIS,
  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
  See the License for the specific language governing permissions and
  limitations under the License.
 
  Company: Processwall Limited
  Address: The Winnowing House, Mill Lane, Askham Richard, York, YO23 3NW, United Kingdom
  Tel:     +44 113 815 3440
  Web:     http://www.processwall.com
  Email:   support@processwall.com
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aras.Model
{
    public class ObservableList<T> : List<T>, IEnumerable<T>
    {
        private int NotifyCount;
        private Boolean _notifiyListChanged;
        public Boolean NotifyListChanged
        {
            get
            {
                return this._notifiyListChanged;
            }
            set
            {
                if (value)
                {
                    if (!this._notifiyListChanged)
                    {
                        this._notifiyListChanged = true;

                        if (this.NotifyCount > 0)
                        {
                            this.OnListChanged();
                        }
                    }
                }
                else
                {
                    if (this._notifiyListChanged)
                    {
                        this._notifiyListChanged = false;
                        this.NotifyCount = 0;
                    }
                }
            }
        }

        public event EventHandler ListChanged;

        private void OnListChanged()
        {
            if (this.ListChanged != null)
            {
                if (this._notifiyListChanged)
                {
                    this.ListChanged(this, EventArgs.Empty);
                }
                else
                {
                    this.NotifyCount++;
                }
            }
        }

        public new void Add(T item)
        {
            base.Add(item);
            this.OnListChanged();
        }

        public new void AddRange(IEnumerable<T> collection)
        {
            base.AddRange(collection);
            this.OnListChanged();
        }

        public new void Clear()
        {
            if (this.Count > 0)
            {
                base.Clear();
                this.OnListChanged();
            }
        }

        public new bool Remove(T item)
        {
            bool ret = base.Remove(item);
            this.OnListChanged();
            return ret;
        }

        public new int RemoveAll(Predicate<T> match)
        {
            int ret = base.RemoveAll(match);
            this.OnListChanged();
            return ret;
        }

        public new void RemoveAt(int index)
        {
            base.RemoveAt(index);
            this.OnListChanged();
        }

        public new void RemoveRange(int index, int count)
        {
            base.RemoveRange(index, count);
            this.OnListChanged();
        }

        public new void Reverse()
        {
            base.Reverse();
            this.OnListChanged();
        }

        public new void Reverse(int index, int count)
        {
            base.Reverse(index, count);
            this.OnListChanged();
        }

        public new void Sort()
        {
            base.Sort();
            this.OnListChanged();
        }

        public new void Sort(Comparison<T> comparison)
        {
            base.Sort(comparison);
            this.OnListChanged();
        }

        public new void Sort(IComparer<T> comparer)
        {
            base.Sort();
            this.OnListChanged();
        }

        public new void Sort(int index, int count, IComparer<T> comparer)
        {
            base.Sort(index, count, comparer);
            this.OnListChanged();
        }

        public void Replace(IEnumerable<T> Values)
        {
            this.NotifyListChanged = false;

            List<T> newvalues = Values.ToList();

            int thislength = this.Count();
            int newvalueslength = newvalues.Count();

            if (thislength > newvalueslength)
            {
                this.RemoveRange(newvalueslength, (thislength - newvalueslength));
            }

            for (int i = 0; i < newvalueslength; i++)
            {
                if (i < thislength)
                {
                    if (!this[i].Equals(newvalues[i]))
                    {
                        this[i] = newvalues[i];
                        this.OnListChanged();
                    }
                }
                else
                {
                    this.Add(newvalues[i]);
                }
            }

            this.NotifyListChanged = true;
        }

        public ObservableList()
        {
            this.NotifyListChanged = true;
        }
    }
}
