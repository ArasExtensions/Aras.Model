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
    public abstract class Cache<T> where T : Model.Item
    {
        public ItemType ItemType { get; private set; }

        public Session Session
        {
            get
            {
                return this.ItemType.Session;
            }
        }

        private Dictionary<String, T> Items;

        protected T GetFromCache(String ID)
        {
            return this.Items[ID];
        }

        protected void AddToCache(T Item)
        {
            this.Items[Item.ID] = Item;
        }

        protected Boolean IsInCache(String ID)
        {
            return this.Items.ContainsKey(ID);
        }

        protected void RemoveFromCache(String ID)
        {
            if (this.IsInCache(ID))
            {
                this.Items.Remove(ID);
            }
        }

        public abstract T Get(String ID);

        internal abstract T Get(IO.Item DBItem);

        public abstract T Create(Transaction Transaction);

        internal void Delete(T Item)
        {
            if (this.IsInCache(Item.ID))
            {
                this.RemoveFromCache(Item.ID);
            }
        }

        internal abstract String Select { get; }

        internal Cache(ItemType ItemType)
        {
            this.ItemType = ItemType;
            this.Items = new Dictionary<String, T>();
        }
    }
}
