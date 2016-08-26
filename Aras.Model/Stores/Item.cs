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

namespace Aras.Model.Stores
{
    public class Item<T> : Store<T> where T : Model.Item
    {
        public Caches.Item Cache { get; private set; }

        public override ItemType ItemType
        {
            get
            {
                return this.Cache.ItemType;
            }
        }

        protected override List<T> Run()
        {
            IO.Item item = new IO.Item(this.ItemType.Name, "get");
            item.Select = this.Cache.Select;
            item.Where = this.Where;
            this.SetPaging(item);

            IO.SOAPRequest request = new IO.SOAPRequest(IO.SOAPOperation.ApplyItem, this.Cache.Session, item);
            IO.SOAPResponse response = request.Execute();

            List<T> ret = new List<T>();

            if (!response.IsError)
            {
                foreach (IO.Item dbitem in response.Items)
                {
                    T thisitem = (T)this.Cache.Get(dbitem);
                    ret.Add(thisitem);
                }

                this.UpdateNoPages(response);
            }
            else
            {
                if (!response.ErrorMessage.Equals("No items of type " + this.ItemType.Name + " found."))
                {
                    throw new Exceptions.ServerException(response);
                }
            }

            return ret;
        }

        public T Create(Transaction Transaction)
        {
            T item = (T)this.Cache.Create(Transaction);
            this.NewItems.Add(item);
            this.Items.Add(item);
            this.OnStoreChanged();
            return item;
        }

        public T Create()
        {
            return this.Create(null);
        }

        protected override void OnRefresh()
        {
           
        }

        internal Item(Caches.Item Cache, Condition Condition)
            : base(Condition)
        {
            this.Cache = Cache;
        }

        internal Item(Caches.Item Cache)
            :this(Cache, null)
        {

        }

        public Item(ItemType ItemType, Condition Condition)
            :base(Condition)
        {
            this.Cache = ItemType.Session.Cache(ItemType);
        }

        public Item(ItemType ItemType)
            :this(ItemType, null)
        {

        }

        public Item(Session Session, String Name, Condition Condition)
            :base(Condition)
        {
            this.Cache = Session.Cache(Name);
        }

        public Item(Session Session, String Name)
            :this(Session, Name, null)
        {

        }
    }
}
