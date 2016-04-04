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
    public class Item : Store<Model.Item>
    {
        internal override String Select
        {
            get
            {
                if (System.String.IsNullOrEmpty(this.ItemType.Select))
                {
                    return "id";
                }
                else
                {
                    return "id," + this.ItemType.Select;
                }
            }
        }

        internal Model.Item GetByDBItem(IO.Item Item)
        {
            if (Item.ItemType.Equals(this.ItemType.Name))
            {
                if (!this.ItemInCache(Item.ID))
                {
                    Model.Item newitem = (Model.Item)this.ItemType.Class.GetConstructor(new Type[] { typeof(ItemType), typeof(IO.Item) }).Invoke(new object[] { this.ItemType, Item });
                    this.AddItemToCache(newitem);
                }

                return this.GetItemFromCache(Item.ID);
            }
            else
            {
                throw new ArgumentException("Invalid ItemType");
            }
        }

        public override Model.Item Get(String ID)
        {
            if (!this.ItemInCache(ID))
            {
                IO.Item dbitem = new IO.Item(this.ItemType.Name, "get");
                dbitem.ID = ID;
                dbitem.Select = this.Select;
                IO.SOAPRequest request = new IO.SOAPRequest(IO.SOAPOperation.ApplyItem, this.Session, dbitem);
                IO.SOAPResponse response = request.Execute();

                if (!response.IsError)
                {
                    if (response.Items.Count() > 0)
                    {
                        Model.Item newitem = (Model.Item)this.ItemType.Class.GetConstructor(new Type[] { typeof(ItemType), typeof(IO.Item) }).Invoke(new object[] { this.ItemType, response.Items.First() });
                        this.AddItemToCache(newitem);
                    }
                    else
                    {
                        throw new Exceptions.ArgumentException("ID does not exist in database");
                    }
                }
                else
                {
                    throw new Exceptions.ServerException(response);
                }
            }

            return this.GetItemFromCache(ID);
        }

        public override Model.Item Create()
        {
            return this.Create(null);
        }

        public override Model.Item Create(Transaction Transaction)
        {
            Model.Item item = (Model.Item)this.ItemType.Class.GetConstructor(new Type[] { typeof(ItemType) }).Invoke(new object[] { this.ItemType });

            if (Transaction != null)
            {
                Transaction.Add("add", item);
            }
                
            this.AddItemToCache(item);
            return item;
        }

        protected override void Load()
        {
            // Load all Items into Cache
            IO.Item item = new IO.Item(this.ItemType.Name, "get");
            item.Select = this.Select;

            IO.SOAPRequest request = new IO.SOAPRequest(IO.SOAPOperation.ApplyItem, this.Session, item);
            IO.SOAPResponse response = request.Execute();

            List<String> ids = new List<String>();

            if (!response.IsError)
            {
                foreach (IO.Item dbitem in response.Items)
                {
                    if (!this.ItemInCache(dbitem.ID))
                    {
                        this.AddItemToCache((Model.Item)this.ItemType.Class.GetConstructor(new Type[] { typeof(ItemType), typeof(IO.Item) }).Invoke(new object[] { this.ItemType, dbitem }));
                    }
                    else
                    {
                        this.GetItemFromCache(dbitem.ID).UpdateProperties(dbitem);
                    }

                    ids.Add(dbitem.ID);
                }
            }
            else
            {
                if (!response.ErrorMessage.Equals("No items of type " + this.ItemType.Name + " found."))
                {
                    throw new Exceptions.ServerException(response);
                }
            }


            // Remove any Relationships that are no longer in the database
            foreach (String id in this.CacheIDS())
            {
                if (!ids.Contains(id))
                {
                    this.RemoveItemFromCache(id);
                }
            }
        }

        public override Query<Model.Item> Query(Condition Condition)
        {
            return new Queries.Item(this, Condition);
        }

        internal Item(ItemType Type)
            :base(Type)
        {
            
        }
    }
}
