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
    public class Session
    {
        public String ID { get; private set; }

        public Database Database { get; private set; }

        public String UserID { get; private set; }

        private Item _user;
        public Item User
        {
            get
            {
                if (this._user == null)
                {
                    this._user = this.ItemFromCache(this.UserID, this.ItemType("User"));
                }

                return this._user;
            }
        }

        public String Username { get; private set; }

        public String Password { get; private set; }

        private Dictionary<String, ItemType> ItemTypeNameCache;
        private Dictionary<String, ItemType> ItemTypeIDCache;

        public ItemType ItemType(String Name)
        {
            if (!this.ItemTypeNameCache.ContainsKey(Name))
            {
                IO.Item itemtype = new IO.Item("ItemType", "get");
                itemtype.Select = "id,name";
                itemtype.SetProperty("name", Name);
                IO.SOAPRequest request = new IO.SOAPRequest(IO.SOAPOperation.ApplyItem, this, itemtype);
                IO.SOAPResponse response = request.Execute();

                if (!response.IsError)
                {
                    this.ItemTypeNameCache[Name] = new ItemType(this, response.Items.First().GetProperty("id"), response.Items.First().GetProperty("name"));
                    this.ItemTypeIDCache[this.ItemTypeNameCache[Name].ID] = this.ItemTypeNameCache[Name];
                }
                else
                {
                    throw new Exceptions.ServerException(response.ErrorMessage);
                }
            }

            return this.ItemTypeNameCache[Name];
        }

        internal ItemType ItemTypeByID(String ID)
        {
            if (!this.ItemTypeIDCache.ContainsKey(ID))
            {
                IO.Item itemtype = new IO.Item("ItemType", "get");
                itemtype.Select = "id,name";
                itemtype.ID = ID;
                IO.SOAPRequest request = new IO.SOAPRequest(IO.SOAPOperation.ApplyItem, this, itemtype);
                IO.SOAPResponse response = request.Execute();

                if (!response.IsError)
                {
                    this.ItemTypeIDCache[ID] = new ItemType(this, response.Items.First().ID, response.Items.First().GetProperty("name"));
                    this.ItemTypeNameCache[this.ItemTypeIDCache[ID].Name] = this.ItemTypeIDCache[ID];
                }
                else
                {
                    throw new Exceptions.ServerException(response.ErrorMessage);
                }
            }

            return this.ItemTypeIDCache[ID];
        }

        private Dictionary<String, List> ListCache;

        internal List ListByID(String ID)
        {
            if (!this.ListCache.ContainsKey(ID))
            {
                IO.Item list = new IO.Item("List", "get");
                list.Select = "id,name";
                list.ID = ID;
                IO.SOAPRequest request = new IO.SOAPRequest(IO.SOAPOperation.ApplyItem, this, list);
                IO.SOAPResponse response = request.Execute();

                if (!response.IsError)
                {
                    this.ListCache[ID] = new List(this, response.Items.First().ID, response.Items.First().GetProperty("name"));
                }
                else
                {
                    throw new Exceptions.ServerException(response.ErrorMessage);
                }
            }

            return this.ListCache[ID];
        }

        private Dictionary<String, Item> ItemCache;

        internal Item ItemFromCache(String ID, String ConfigID, ItemType Type)
        {
            if (!this.ItemCache.ContainsKey(ID))
            {
                this.ItemCache[ID] = new Item(ID, ConfigID, Type);
            }

            return this.ItemCache[ID];
        }

        internal Item ItemFromCache(String ID, ItemType Type)
        {
            if (!this.ItemCache.ContainsKey(ID))
            {
                IO.Item dbitem = new IO.Item(Type.Name, "get");
                dbitem.ID = ID;
                IO.SOAPRequest request = new IO.SOAPRequest(IO.SOAPOperation.ApplyItem, this, dbitem);
                IO.SOAPResponse response = request.Execute();

                if (!response.IsError)
                {
                    this.ItemCache[ID] = new Item(ID, response.Items.First().ConfigID, Type);
                }
                else
                {
                    throw new Exceptions.ServerException(response.ErrorMessage);
                }
            }

            return this.ItemCache[ID];
        }

        public Queries.Item Query(ItemType Type)
        {
            return new Queries.Item(Type);
        }

        public Queries.Item Query(String ItemTypeName)
        {
            return new Queries.Item(this.ItemType(ItemTypeName));
        }

        public Transaction BeginTransaction()
        {
            return new Transaction(this);
        }

        public Item Create(String Type, Transaction Transaction)
        {
            return this.Create(this.ItemType(Type), Transaction);
        }

        public Item Create(ItemType Type, Transaction Transaction)
        {
            Item item = new Item(null, null, Type);
            this.ItemCache[item.ID] = item;
            Transaction.Add("add", item);
            return item;
        }

        internal Session(Database Database, String UserID, String Username, String Password)
        {
            this.ID = Server.NewID();
            this.Database = Database;
            this.UserID = UserID;
            this.Username = Username;
            this.Password = Password;
            this.ItemTypeNameCache = new Dictionary<String, ItemType>();
            this.ItemTypeIDCache = new Dictionary<String, ItemType>();
            this.ItemCache = new Dictionary<String, Item>();
            this.ListCache = new Dictionary<String, List>();
        }
    }
}
