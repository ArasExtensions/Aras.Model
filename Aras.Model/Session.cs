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

        private User _user;
        public User User
        {
            get
            {
                if (this._user == null)
                {
                    this._user = (User)this.Store("User").Get(this.UserID);
                }

                return this._user;
            }
        }

        private Identity _alias;
        public Identity Alias
        {
            get
            {
                if (this._alias == null)
                {
                    this._alias = (Identity)this.User.Store("Alias").First().Related;
                }

                return this._alias;
            }
        }

        private List<Identity> GetParentIdentities(Identity Identity)
        {
            List<Identity> identities = new List<Identity>();

            foreach(Identity thisidentity in this.IdentityCache.Values)
            {
                foreach (Identity thischildidentity in this.IdentityMemberCache[thisidentity])
                {
                    if (thischildidentity.Equals(Identity))
                    {
                        if (!identities.Contains(thisidentity))
                            identities.Add(thisidentity);
                    }
                }
            }

            List<Identity> ret = new List<Identity>();

            foreach(Identity identity in identities)
            {
                ret.Add(identity);

                foreach(Identity parentidentiy in this.GetParentIdentities(identity))
                {
                    if (!ret.Contains(parentidentiy))
                    {
                        ret.Add(parentidentiy);
                    }
                }
            }

            return identities;
        }

        Dictionary<String, Identity> IdentityCache;
        Dictionary<Identity, List<Identity>> IdentityMemberCache;
        private List<Identity> _identities;
        public IEnumerable<Identity> Identities
        {
            get
            {
                if (this._identities == null)
                {
                    this._identities = new List<Identity>();

                    IO.Item identityrequest = new IO.Item("Identity", "get");
                    identityrequest.Select = "id,name";
                    identityrequest.SetProperty("is_alias", "0");
                    IO.Item memberrequest = new IO.Item("Member", "get");
                    memberrequest.Select = "related_id";
                    identityrequest.AddRelationship(memberrequest);

                    IO.SOAPRequest request = new IO.SOAPRequest(IO.SOAPOperation.ApplyItem, this, identityrequest);
                    IO.SOAPResponse response = request.Execute();

                    if (!response.IsError)
                    {
                        IdentityCache = new Dictionary<String, Identity>();
                        IdentityMemberCache = new Dictionary<Identity, List<Identity>>();

                        foreach(IO.Item dbidentity in response.Items)
                        {
                            this.IdentityCache[dbidentity.ID] = (Identity)this.Store("Identity").GetByDBItem(dbidentity);
                            this.IdentityMemberCache[this.IdentityCache[dbidentity.ID]] = new List<Identity>();
                        }

                        foreach (IO.Item dbidentity in response.Items)
                        {
                            Identity identity = this.IdentityCache[dbidentity.ID];

                            foreach (IO.Item dbmember in dbidentity.Relationships)
                            {
                                String childidentityid = dbmember.GetPropertyItem("related_id").ID;

                                if (childidentityid.Equals(this.Alias.ID))
                                {
                                    this.IdentityMemberCache[identity].Add(this.Alias);
                                }
                                else if (this.IdentityCache.ContainsKey(childidentityid))
                                {
                                    Identity childidentity = this.IdentityCache[childidentityid];
                                    this.IdentityMemberCache[identity].Add(childidentity);
                                }
                            }
                        }

                        this._identities = this.GetParentIdentities(this.Alias);

                        // Add Alias
                        this._identities.Add(this.Alias);

                        // Add Owner and Manager
                        foreach(Identity identity in this.IdentityCache.Values)
                        {
                            if (identity.Name.Equals("Owner") || identity.Equals("Manager"))
                            {
                                this._identities.Add(identity);
                            }
                        }
                    }
                    else
                    {
                        throw new Exceptions.ServerException(response);
                    }

                }

                return this._identities;
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
 
        public Transaction BeginTransaction()
        {
            return new Transaction(this);
        }

        private Dictionary<ItemType, Stores.Item> StoreCache;

        public Stores.Item Store(ItemType ItemType)
        {
            if (!(ItemType is RelationshipType))
            {
                if (!this.StoreCache.ContainsKey(ItemType))
                {
                    this.StoreCache[ItemType] = new Stores.Item(ItemType);
                }

                return this.StoreCache[ItemType];
            }
            else
            {
                throw new ArgumentException("Can not access store for a RelationshipType");
            }
        }

        public Stores.Item Store(String Name)
        {
            return this.Store(this.ItemType(Name));
        }

        private Dictionary<String, Item> SourceCache;

        internal Item Get(ItemType ItemType, String ID)
        {
            if (String.IsNullOrEmpty(ID))
            {
                return null;
            }
            else
            {
                if (ItemType is RelationshipType)
                {
                    if (!this.SourceCache.ContainsKey(ID))
                    {
                        IO.Item dbitem = new IO.Item(ItemType.Name, "get");
                        dbitem.ID = ID;
                        dbitem.Select = "source_id";
                        IO.SOAPRequest request = new IO.SOAPRequest(IO.SOAPOperation.ApplyItem, this, dbitem);
                        IO.SOAPResponse response = request.Execute();

                        if (!response.IsError)
                        {
                            this.SourceCache[ID] = this.Get(((RelationshipType)ItemType).Source, response.Items.First().GetProperty("source_id"));
                        }
                        else
                        {
                            throw new Exceptions.ServerException(response);
                        }
                    }

                    // Return Item from Source Store
                    return this.SourceCache[ID].Store((RelationshipType)ItemType).Get(ID);
                }
                else
                {
                    // Return Item from Store
                    return this.Store(ItemType).Get(ID);
                }
            }
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
            this.StoreCache = new Dictionary<ItemType, Stores.Item>();
            this.SourceCache = new Dictionary<String, Item>();

            // Default Selections
            this.ItemType("Value").AddToSelect("value,label");
            this.ItemType("Access").AddToSelect("can_get,can_update,can_delete,can_discover,can_change_access");
        }
    }
}
