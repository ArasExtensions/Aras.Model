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
using System.ComponentModel;

namespace Aras.Model
{
    public abstract class Store<T> : System.Collections.Generic.IEnumerable<T> where T : Model.Item
    {
        public Session Session
        {
            get
            {
                return this.ItemType.Session;
            }
        }

        public ItemType ItemType { get; private set; }

        internal abstract String Select { get; }

        // Cache of all Items Read from Server and Created in Session
        private Dictionary<String, T> ItemsCache;

        protected Boolean InItemsCache(String ID)
        {
            return this.ItemsCache.ContainsKey(ID);
        }

        protected void AddToItemsCache(T Item)
        {
            this.ItemsCache[Item.ID] = Item;
        }

        protected void RemoveFromItemsCache(T Item)
        {
            if (this.ItemsCache.ContainsKey(Item.ID))
            {
                this.ItemsCache.Remove(Item.ID);
            }
        }

        protected T GetFromItemsCache(String ID)
        {
            if (this.ItemsCache.ContainsKey(ID))
            {
                return this.ItemsCache[ID];
            }
            else
            {
                throw new Exceptions.ArgumentException("Invalid Items Cache ID: " + ID);
            }
        }

        protected void ReplaceItemsCache(List<T> AllItems)
        {
            List<String> toberemoved = new List<String>();

            foreach(String key in this.ItemsCache.Keys)
            {
                if(!AllItems.Contains(this.ItemsCache[key]))
                {
                    toberemoved.Add(key);
                }
            }

            foreach(String key in toberemoved)
            {
                this.ItemsCache.Remove(key);
            }
        }

        // Cache of all Created Items
        private Dictionary<String, T> CreatedCache;

        protected Boolean InCreatedCache(T Item)
        {
            return this.CreatedCache.ContainsKey(Item.ID);
        }

        protected void AddToCreatedCache(T Item)
        {
            this.CreatedCache[Item.ID] = Item;
        }

        protected void RemoveFromCreatedCache(T Item)
        {
            if (this.CreatedCache.ContainsKey(Item.ID))
            {
                this.CreatedCache.Remove(Item.ID);
            }
        }

        protected T GetFromCreatedCache(String ID)
        {
            if (this.CreatedCache.ContainsKey(ID))
            {
                return this.CreatedCache[ID];
            }
            else
            {
                throw new Exceptions.ArgumentException("Invalid Created Cache ID: " + ID);
            }
        }

        protected void RefreshCreatedCache()
        {
            // Remove any Items that are now in database from Created Cache
            foreach(String key in this.CreatedCache.Keys)
            {
                if (this.CreatedCache[key].Action != Item.Actions.Create)
                {
                    this.CreatedCache.Remove(key);
                }
            }
        }

        public IEnumerable<T> CreatedItems()
        {
            this.RefreshCreatedCache();
            return this.CreatedCache.Values;
        }

        public abstract T Get(String ID);

        internal abstract T Get(IO.Item DBItem);

        internal void Delete(T Item)
        {
            if (this.ItemsCache.ContainsKey(Item.ID))
            {
                this.ItemsCache.Remove(Item.ID);
            }
            else
            {
                if (this.CreatedCache.ContainsKey(Item.ID))
                {
                    this.CreatedCache.Remove(Item.ID);
                }
            }
        }

        public System.Collections.Generic.IEnumerator<T> GetEnumerator()
        {
            this.Index();
            return this.ItemsCache.Values.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        private Boolean Indexed;

        protected abstract void ReadAllItems();

        private void Index()
        {
            if (!this.Indexed)
            {
                this.ReadAllItems();
                this.Indexed = true;
            }
        }

        public void Refesh()
        {
            // Only Refresh if already Indexed
            if (this.Indexed)
            {
                this.ReadAllItems();
            }
        }

        public IEnumerable<T> CurrentItems()
        {
            List<T> currentitems = new List<T>();

            foreach (T item in this)
            {
                if (item.Action != Item.Actions.Delete)
                {
                    currentitems.Add(item);
                }
            }

            return currentitems;
        }

        internal Store(ItemType ItemType)
        {
            this.ItemsCache = new Dictionary<String, T>();
            this.CreatedCache = new Dictionary<String, T>();
            this.Indexed = false;
            this.ItemType = ItemType;
        }


    }
}
