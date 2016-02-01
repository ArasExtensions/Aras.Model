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

        public override Model.Item Get(String ID)
        {
            if (!this.Cache.ContainsKey(ID))
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
                        this.Cache[ID] = (Model.Item)this.ItemType.Class.GetConstructor(new Type[] { typeof(ItemType), typeof(IO.Item) }).Invoke(new object[] { this.ItemType, response.Items.First() });
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

            return this.Cache[ID];
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
                item.Transaction = Transaction;
            }

            this.Cache[item.ID] = item;
            return item;
        }

        protected override void Load()
        {
            // Load all Items into Cache
            IO.Item item = new IO.Item(this.ItemType.Name, "get");
            item.Select = this.Select;

            IO.SOAPRequest request = new IO.SOAPRequest(IO.SOAPOperation.ApplyItem, this.Session, item);
            IO.SOAPResponse response = request.Execute();

            if (!response.IsError)
            {
                foreach (IO.Item dbitem in response.Items)
                {
                    if (!this.Cache.ContainsKey(dbitem.ID))
                    {
                        this.Cache[dbitem.ID] = (Model.Item)this.ItemType.Class.GetConstructor(new Type[] { typeof(ItemType), typeof(IO.Item) }).Invoke(new object[] { this.ItemType, dbitem });
                    }
                }
            }
            else
            {
                if (!response.ErrorMessage.Equals("No items of type " + this.ItemType.Name + " found."))
                {
                    throw new Exceptions.ServerException(response);
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
