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
                    return String.Join(",", ItemType.SystemProperties);
                }
                else
                {
                    return String.Join(",", ItemType.SystemProperties) + "," + this.ItemType.Select;
                }
            }
        }

        public override Model.Item Get(String ID)
        {
            Model.Item item = null;

            if (!this.InItemsCache(ID))
            {
                // Read Item from Database
                IO.Item dbitem = new IO.Item(this.ItemType.Name, "get");
                dbitem.ID = ID;
                dbitem.Select = this.Select;
                IO.SOAPRequest request = this.Session.IO.Request(IO.SOAPOperation.ApplyItem, dbitem);
                IO.SOAPResponse response = request.Execute();

                if (!response.IsError)
                {
                    if (response.Items.Count() > 0)
                    {
                        item = (Model.Item)this.ItemType.Class.GetConstructor(new Type[] { typeof(ItemType), typeof(IO.Item) }).Invoke(new object[] { this.ItemType, response.Items.First() });
                        this.AddToItemsCache(item);
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
            else
            {
                // Get Item from Cache
                item = this.GetFromItemsCache(ID);
            }

            return item;
        }

        protected override void ReadAllItems()
        {
            // List to hold all ID's read from Server
            List<String> ret = new List<String>();

            // Read all Item from Database
            IO.Item dbitem = new IO.Item(this.ItemType.Name, "get");
            dbitem.Select = this.Select;
            IO.SOAPRequest request = this.Session.IO.Request(IO.SOAPOperation.ApplyItem, dbitem);
            IO.SOAPResponse response = request.Execute();

            if (!response.IsError)
            {
                foreach(IO.Item thisdbitem in response.Items)
                {
                    Model.Item item = null;

                    if (this.InItemsCache(thisdbitem.ID))
                    {
                        item = this.GetFromItemsCache(thisdbitem.ID);
                        item.UpdateProperties(thisdbitem);

                        // Check if in CreatedCache
                        if (this.InCreatedCache(item))
                        {
                            this.RemoveFromCreatedCache(item);
                        }
                    }
                    else
                    {
                        item = (Model.Item)this.ItemType.Class.GetConstructor(new Type[] { typeof(ItemType), typeof(IO.Item) }).Invoke(new object[] { this.ItemType, response.Items.First() });
                        this.AddToItemsCache(item);
                    }

                    ret.Add(item.ID);
                }
            }
            else
            {
                throw new Exceptions.ServerException(response);
            }

            // Replace Cache
            this.ReplaceItemsCache(ret);
        }

        internal override Model.Item Get(IO.Item DBItem)
        {
            if (DBItem.ItemType.Equals(this.ItemType.Name))
            {
                Model.Item item = null;

                if (!this.InItemsCache(DBItem.ID))
                {
                    // Create new Item from Database Item
                    item = (Model.Item)this.ItemType.Class.GetConstructor(new Type[] { typeof(ItemType), typeof(IO.Item) }).Invoke(new object[] { this.ItemType, DBItem });
                    this.AddToItemsCache(item);
                    return item;
                }
                else
                {
                    // Get Existing Item from Cache and update Properties from Database Item
                    item = this.GetFromItemsCache(DBItem.ID);
                    item.UpdateProperties(DBItem);
                }

                return item;
            }
            else
            {
                throw new ArgumentException("Invalid ItemType");
            }
        }

        public Model.Item Create(Transaction Transaction)
        {
            Model.Item item = (Model.Item)this.ItemType.Class.GetConstructor(new Type[] { typeof(ItemType), typeof(Model.Transaction) }).Invoke(new object[] { this.ItemType, Transaction });
            this.AddToItemsCache(item);
            this.AddToCreatedCache(item);
            return item;
        }

        public Queries.Item Query(Condition Condition)
        {
            return new Queries.Item(this, Condition);
        }

        public Queries.Item Query()
        {
            return new Queries.Item(this, null);
        }

        internal Item(ItemType ItemType)
            : base(ItemType)
        {

        }
    }
}
