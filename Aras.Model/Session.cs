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
                dbrelationshiptype.Select = "source_id,related_id";
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

                    RelationshipType relationshiptype = new RelationshipType(this, DBItem.ID, DBItem.GetProperty("name"), DBItem.GetProperty("class_structure"), sourceitemtype, relateditemtype);
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
                ItemType itemtype = new ItemType(this, DBItem.ID, DBItem.GetProperty("name"), DBItem.GetProperty("class_structure"));
                this.ItemTypeNameCache[itemtype.Name] = itemtype;
                this.ItemTypeIDCache[itemtype.ID] = itemtype;
            }
        }

        public ItemType ItemType(String Name)
        {
            if (!this.ItemTypeNameCache.ContainsKey(Name))
            {
                IO.Item itemtype = new IO.Item("ItemType", "get");
                itemtype.Select = "id,name,is_relationship,class_structure";
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
                itemtype.Select = "id,name,is_relationship,class_structure";
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

        internal Item ItemFromCache(String ID, ItemType Type)
        {
            if (!this.ItemCache.ContainsKey(ID))
            {
                if (Type is RelationshipType)
                {
                    IO.Item dbitem = new IO.Item(Type.Name, "get");
                    dbitem.ID = ID;
                    dbitem.Select = "source_id,related_id";
                    IO.SOAPRequest request = new IO.SOAPRequest(IO.SOAPOperation.ApplyItem, this, dbitem);
                    IO.SOAPResponse response = request.Execute();

                    if (!response.IsError)
                    {
                        String sourceid = response.Items.First().GetProperty("source_id");
                        Item source = this.ItemFromCache(sourceid, ((RelationshipType)Type).Source);
                        Item related = null;

                        if (((RelationshipType)Type).Related != null)
                        {
                            String relatedid = response.Items.First().GetProperty("related_id");

                            if (relatedid != null)
                            {
                                related = this.ItemFromCache(relatedid, ((RelationshipType)Type).Related);
                            }
                        }

                        this.ItemCache[ID] = (Relationship)Type.Class.GetConstructor(new Type[] { typeof(String), typeof(RelationshipType), typeof(Item), typeof(Item) }).Invoke(new object[] { ID, Type, source, related });
                    }
                    else
                    {
                        throw new Exceptions.ServerException(response);
                    }
                }
                else
                {
                    this.ItemCache[ID] = (Item)Type.Class.GetConstructor(new Type[] { typeof(String), typeof(ItemType) }).Invoke(new object[] { ID, Type });
                }
            }

            return this.ItemCache[ID];
        }



        internal Relationship RelationshipFromCache(String ID, RelationshipType Type, Item Source, Item Related)
        {
            if (!this.ItemCache.ContainsKey(ID))
            {
                this.ItemCache[ID] = (Relationship)Type.Class.GetConstructor(new Type[] { typeof(String), typeof(RelationshipType), typeof(Item), typeof(Item)}).Invoke(new object[] { ID, Type, Source, Related });
            }

            return (Relationship)this.ItemCache[ID];
        }

        public Queries.Item Query(ItemType Type)
        {
            return new Queries.Item(Type);
        }

        public Queries.Item Query(String ItemTypeName)
        {
            return new Queries.Item(this.ItemType(ItemTypeName));
        }

        public Queries.Item Query(ItemType Type, Condition Condition)
        {
            return new Queries.Item(Type, Condition);
        }

        public Queries.Item Query(String ItemTypeName, Condition Condition)
        {
            return new Queries.Item(this.ItemType(ItemTypeName), Condition);
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
            if (!(Type is RelationshipType))
            {
                Item item = (Item)Type.Class.GetConstructor(new Type[] { typeof(String), typeof(ItemType) }).Invoke(new object[] { null, Type });                
                this.ItemCache[item.ID] = item;

                if (Transaction != null)
                {
                    Transaction.Add("add", item);
                }
                else
                {
                    item.Transaction = null;
                }

                return item;
            }
            else
            {
                throw new Exceptions.ArgumentException("Not possible to create a Relation");
            }
        }

        public Item Create(ItemType Type)
        {
            return this.Create(Type, null);
        }

        public Item Create(String Type)
        {
            return this.Create(Type, null);
        }

        internal Relationship Create(RelationshipType RelationshipType, Item Source, Item Related, Transaction Transaction)
        {
            Relationship relationship = (Relationship)RelationshipType.Class.GetConstructor(new Type[] { typeof(String), typeof(RelationshipType), typeof(Item), typeof(Item) }).Invoke(new object[] { null, RelationshipType, Source, Related });
            this.ItemCache[relationship.ID] = relationship;

            if (Transaction != null)
            {
                Transaction.Add("add", relationship);
            }

            return relationship;
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

            // Default Selections
            this.ItemType("Value").AddToSelect("value,label");
        }
    }
}
