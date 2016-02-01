/*  
  Aras.Model provides a .NET cient library for Aras Innovator

  Copyright (C) 2015 Processwall Limited.

  This program is free software: you can redistribute it and/or modify
  it under the terms of the GNU Affero General Public License as published
  by the Free Software Foundation, either version 3 of the License, or
  (at your option) any later version.

  This program is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU Affero General Public License for more details.

  You should have received a copy of the GNU Affero General Public License
  along with this program.  If not, see http://opensource.org/licenses/AGPL-3.0.
 
  Company: Processwall Limited
  Address: The Winnowing House, Mill Lane, Askham Richard, York, YO23 3NW, United Kingdom
  Tel:     +44 113 815 3440
  Email:   support@processwall.com
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aras.Model
{
    public abstract class Store<T> : System.Collections.Generic.IEnumerable<T> where T : Model.Item
    {
        public delegate void StoreChangedEventHandler(object sender, EventArgs e);

        public event StoreChangedEventHandler StoreChanged;

        protected void OStoreChanged()
        {
            if (this.StoreChanged != null)
            {
                StoreChanged(this, new EventArgs());
            }
        }

        public Session Session
        {
            get
            {
                return this.ItemType.Session;
            }
        }

        public ItemType ItemType { get; private set; }

        internal Dictionary<String, T> Cache;

        public System.Collections.Generic.IEnumerator<T> GetEnumerator()
        {
            if (!this.Loaded)
            {
                this.Load();
                this.Loaded = true;
            }

            return this.Cache.Values.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public List<T> Copy()
        {
            List<T> ret = new List<T>();

            foreach (T item in this)
            {
                ret.Add(item);
            }

            return ret;
        }

        public T this[int Index]
        {
            get
            {
                return this.ElementAt(Index);
            }
        }

        public abstract T Get(String ID);

        public abstract T Create(Transaction Transaction);

        public abstract T Create();

        private Boolean Loaded;

        protected abstract void Load();

        public void Refresh()
        {
            this.Load();
            this.Loaded = true;
        }

        public abstract Query<T> Query(Condition Condition);

        public Query<T> Query()
        {
            return this.Query(null);
        }

        internal abstract String Select { get; }

        internal Store(ItemType ItemType)
        {
            this.ItemType = ItemType;
            this.Cache = new Dictionary<String, T>();
            this.Loaded = false;
        }
    }
}
