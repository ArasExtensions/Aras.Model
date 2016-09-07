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
using System.Reflection;

namespace Aras.Model.Caches
{
    public class Item : Cache<Model.Item>
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

        internal override Model.Item Get(IO.Item DBItem)
        {
            if (DBItem.ItemType.Equals(this.ItemType.Name))
            {
                Model.Item item = null;

                if (!this.IsInCache(DBItem.ID))
                {
                    // Create new Item from Database Item
                    item = (Model.Item)this.ItemType.Class.GetConstructor(new Type[] { typeof(ItemType), typeof(IO.Item) }).Invoke(new object[] { this.ItemType, DBItem });
                    this.AddToCache(item);
                    return item;
                }
                else
                {
                    // Get Existing Item from Cache and update Properties from Database Item
                    item = this.GetFromCache(DBItem.ID);
                    item.UpdateProperties(DBItem);
                }

                return item;
            }
            else
            {
                throw new ArgumentException("Invalid ItemType");
            }
        }

        public override Model.Item Get(String ID)
        {
            Model.Item item = null;

            if (!this.IsInCache(ID))
            {
                // Read Item from Database
                IO.Item dbitem = new IO.Item(this.ItemType.Name, "get");
                dbitem.ID = ID;
                dbitem.Select = this.Select;
                IO.SOAPRequest request = new IO.SOAPRequest(IO.SOAPOperation.ApplyItem, this.Session, dbitem);
                IO.SOAPResponse response = request.Execute();

                if (!response.IsError)
                {
                    if (response.Items.Count() > 0)
                    {
                        item = (Model.Item)this.ItemType.Class.GetConstructor(new Type[] { typeof(ItemType), typeof(IO.Item) }).Invoke(new object[] { this.ItemType, response.Items.First() });
                        this.AddToCache(item);
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
                item = this.GetFromCache(ID);
            }

            return item;
        }

        public override Model.Item Create()
        {
            return this.Create(null);
        }

        public override Model.Item Create(Transaction Transaction)
        {
            Model.Item item = (Model.Item)this.ItemType.Class.GetConstructor(new Type[] { typeof(ItemType), typeof(Transaction) }).Invoke(new object[] { this.ItemType, Transaction });                
            this.AddToCache(item);

            return item;
        }

        internal Item(ItemType Type)
            :base(Type)
        {
            
        }
    }
}
