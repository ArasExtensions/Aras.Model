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

        private void BuildItemType(IO.Item DBItem)
        {
            if (DBItem.GetProperty("is_relationship").Equals("1"))
            {
                IO.Item dbrelationshiptype = new IO.Item("RelationshipType", "get");
                dbrelationshiptype.SetProperty("relationship_id", DBItem.ID);
                dbrelationshiptype.Select = "source_id(id),related_id(id)";
                IO.SOAPRequest request = new IO.SOAPRequest(IO.SOAPOperation.ApplyItem, this, dbrelationshiptype);
                IO.SOAPResponse response = request.Execute();

                if (!response.IsError)
                {
                    IO.Item dbreltype = response.Items.First();
                    String sourceitemtypeid = dbreltype.GetProperty("source_id");
                    IO.Item dbrelateditemtype = dbreltype.GetPropertyItem("related_id");
                    ItemType sourceitemtype = this.ItemTypeByID(sourceitemtypeid);
                    ItemType relateditemtype = null;

                    if (dbrelateditemtype != null)
                    {
                        relateditemtype = this.ItemTypeByID(dbrelateditemtype.ID);
                    }

                    RelationshipType relationshiptype = new RelationshipType(this, DBItem.ID, DBItem.GetProperty("name"), sourceitemtype, relateditemtype);
                    this.ItemTypeNameCache[relationshiptype.Name] = relationshiptype;
                    this.ItemTypeIDCache[relationshiptype.ID] = relationshiptype;
                }
                else
                {
                    throw new Exceptions.ServerException(response);
                }

            }
            else
            {
                ItemType itemtype = new ItemType(this, DBItem.ID, DBItem.GetProperty("name"));
                this.ItemTypeNameCache[itemtype.Name] = itemtype;
                this.ItemTypeIDCache[itemtype.ID] = itemtype;
            }
        }

        public ItemType ItemType(String Name)
        {
            if (!this.ItemTypeNameCache.ContainsKey(Name))
            {
                IO.Item itemtype = new IO.Item("ItemType", "get");
                itemtype.Select = "id,name,is_relationship";
                itemtype.SetProperty("name", Name);
                IO.SOAPRequest request = new IO.SOAPRequest(IO.SOAPOperation.ApplyItem, this, itemtype);
                IO.SOAPResponse response = request.Execute();

                if (!response.IsError)
                {
                    this.BuildItemType(response.Items.First());
                }
                else
                {
                    throw new Exceptions.ServerException(response);
                }
            }

            return this.ItemTypeNameCache[Name];
        }

        internal ItemType ItemTypeByID(String ID)
        {
            if (!this.ItemTypeIDCache.ContainsKey(ID))
            {
                IO.Item itemtype = new IO.Item("ItemType", "get");
                itemtype.Select = "id,name,is_relationship";
                itemtype.ID = ID;
                IO.SOAPRequest request = new IO.SOAPRequest(IO.SOAPOperation.ApplyItem, this, itemtype);
                IO.SOAPResponse response = request.Execute();

                if (!response.IsError)
                {
                    this.BuildItemType(response.Items.First());
                }
                else
                {
                    throw new Exceptions.ServerException(response);
                }
            }

            return this.ItemTypeIDCache[ID];
        }

        internal List ListByID(String ID)
        {
            return (List)this.ItemFromCache(ID, this.ItemType("List"));
        }

        private Dictionary<String, Item> ItemCache;

        internal Item ItemFromCache(String ID, String ConfigID, ItemType Type)
        {

            if (!this.ItemCache.ContainsKey(ID))
            {
                if (Type is RelationshipType)
                {
                    IO.Item dbitem = new IO.Item(Type.Name, "get");
                    dbitem.ID = ID;
                    dbitem.Select = "source_id(id,config_id),related_id(id,config_id)";
                    IO.SOAPRequest request = new IO.SOAPRequest(IO.SOAPOperation.ApplyItem, this, dbitem);
                    IO.SOAPResponse response = request.Execute();

                    if (!response.IsError)
                    {
                        IO.Item dbsource = response.Items.First().GetPropertyItem("source_id");
                        Item source = this.ItemFromCache(dbsource.ID, dbsource.ConfigID, ((RelationshipType)Type).Source);
                        Item related = null;

                        if (((RelationshipType)Type).Related != null)
                        {
                            IO.Item dbrelated = response.Items.First().GetPropertyItem("related_id");
                            
                            if (dbrelated != null)
                            {
                                related = this.ItemFromCache(dbrelated.ID, dbrelated.ConfigID, ((RelationshipType)Type).Related);
                            }
                        }

                        this.ItemCache[ID] = (Relationship)Type.Class.GetConstructor(new Type[] { typeof(String), typeof(String), typeof(RelationshipType), typeof(Item), typeof(Item) }).Invoke(new object[] { ID, ConfigID, Type, source, related });
                    }
                    else
                    {
                        throw new Exceptions.ServerException(response);
                    }
                }
                else
                {
                    this.ItemCache[ID] = (Item)Type.Class.GetConstructor(new Type[] { typeof(String), typeof(String), typeof(ItemType) }).Invoke(new object[] { ID, ConfigID, Type });
                }
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
                    return this.ItemFromCache(ID, response.Items.First().ConfigID, Type);
                }
                else
                {
                    throw new Exceptions.ServerException(response);
                }
            }
            else
            {
                return this.ItemCache[ID];
            }
        }

        internal Relationship RelationshipFromCache(String ID, String ConfigID, RelationshipType Type, Item Source, Item Related)
        {
            if (!this.ItemCache.ContainsKey(ID))
            {
                this.ItemCache[ID] = (Relationship)Type.Class.GetConstructor(new Type[] { typeof(String), typeof(String), typeof(RelationshipType), typeof(Item), typeof(Item)}).Invoke(new object[] { ID, ConfigID, Type, Source, Related });
            }

            return (Relationship)this.ItemCache[ID];
        }

        public Queries.Item Query(ItemType Type, String Select)
        {
            return new Queries.Item(Type, Select);
        }

        public Queries.Item Query(String ItemTypeName, String Select)
        {
            return new Queries.Item(this.ItemType(ItemTypeName), Select);
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
        }
    }
}
