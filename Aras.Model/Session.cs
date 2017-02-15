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
using System.IO;
using System.Net;

namespace Aras.Model
{
    public class Session
    {
        public Database Database { get; private set; }

        public IO.Session IO { get; private set; }

        public String ID
        {
            get
            {
                return this.IO.UserID;
            }
        }

        private DirectoryInfo _cacheDirectory;
        internal DirectoryInfo CacheDirectory
        {
            get
            {
                if (this._cacheDirectory == null)
                {
                    this._cacheDirectory = new DirectoryInfo(Path.GetTempPath() + "\\Aras\\Session\\Cache\\" + this.ID);

                    if (!this._cacheDirectory.Exists)
                    {
                        this._cacheDirectory.Create();
                    }
                }

                return this._cacheDirectory;
            }
        }

        private User _user;
        public User User
        {
            get
            {
                if (this._user == null)
                {
                    this._user = (User)this.Store("User").Get(this.IO.UserID);
                }

                return this._user;
            }
        }

        public Identity Alias
        {
            get
            {
                return this.User.Identity;
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

                    IO.SOAPRequest request = this.IO.Request(Aras.IO.SOAPOperation.ApplyItem);
                    IO.Item identityrequest = request.NewItem("Identity", "get");
                    identityrequest.Select = "id,name";
                    identityrequest.SetProperty("is_alias", "0");
                    IO.Item memberrequest = identityrequest.NewRelationship("Member", "get");
                    memberrequest.Select = "related_id";
                    IO.SOAPResponse response = request.Execute();

                    if (!response.IsError)
                    {
                        IdentityCache = new Dictionary<String, Identity>();
                        IdentityMemberCache = new Dictionary<Identity, List<Identity>>();

                        foreach(IO.Item dbidentity in response.Items)
                        {
                            this.IdentityCache[dbidentity.ID] = (Identity)this.Store("Identity").Get(dbidentity);
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

        private Dictionary<String, ItemType> ItemTypeNameCache;
        private Dictionary<String, ItemType> ItemTypeIDCache;

        private void BuildItemType(IO.Item DBItem)
        {
            if (DBItem.GetProperty("is_relationship").Equals("1"))
            {
                IO.SOAPRequest request = this.IO.Request(Aras.IO.SOAPOperation.ApplyItem);
                IO.Item dbrelationshiptype = request.NewItem("RelationshipType", "get");
                dbrelationshiptype.SetProperty("relationship_id", DBItem.ID);
                dbrelationshiptype.Select = "source_id,related_id,grid_view";
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

                    RelationshipGridViews RelationshipGridView = RelationshipGridViews.Left;

                    switch(dbreltype.GetProperty("grid_view"))
                    {
                        case "right":
                            RelationshipGridView = RelationshipGridViews.Right;
                            break;
                        case "intermix":
                            RelationshipGridView = RelationshipGridViews.InterMix;
                            break;
                        default:
                            RelationshipGridView = RelationshipGridViews.Left;
                            break;
                    }

                    RelationshipType relationshiptype = new RelationshipType(this, DBItem.ID, DBItem.GetProperty("name"), DBItem.GetProperty("class_structure"), sourceitemtype, relateditemtype, RelationshipGridView);
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
                IO.SOAPRequest request = this.IO.Request(Aras.IO.SOAPOperation.ApplyItem);
                IO.Item itemtype = request.NewItem("ItemType", "get");
                itemtype.Select = "id,name,is_relationship,class_structure";
                itemtype.SetProperty("name", Name);
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
                IO.SOAPRequest request = this.IO.Request(Aras.IO.SOAPOperation.ApplyItem);
                IO.Item itemtype = request.NewItem("ItemType", "get");
                itemtype.Select = "id,name,is_relationship,class_structure";
                itemtype.ID = ID;
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

        private Dictionary<ItemType, Stores.Item> StoresCache;

        public Stores.Item Store(ItemType ItemType)
        {
            if (!(ItemType is RelationshipType))
            {
                if (!this.StoresCache.ContainsKey(ItemType))
                {
                    this.StoresCache[ItemType] = new Stores.Item(ItemType);
                }

                return this.StoresCache[ItemType];
            }
            else
            {
                throw new ArgumentException("Can not access Cache for a RelationshipType");
            }
        }

        public Stores.Item Store(String Name)
        {
            return this.Store(this.ItemType(Name));
        }

        internal Item Get(ItemType ItemType, String ID)
        {
            Item ret = null;

            if (!String.IsNullOrEmpty(ID))
            {
                if (ItemType is RelationshipType)
                {
                    // Get Source Item
                    IO.SOAPRequest request = this.IO.Request(Aras.IO.SOAPOperation.ApplyItem);
                    IO.Item dbitem = request.NewItem(ItemType.Name, "get");
                    dbitem.ID = ID;
                    dbitem.Select = "source_id";
                    IO.SOAPResponse response = request.Execute();

                    if (!response.IsError)
                    {
                        Item source = this.Get(((RelationshipType)ItemType).SourceItemType, response.Items.First().GetProperty("source_id"));
                        ret = source.Store((RelationshipType)ItemType).Get(ID);
                    }
                    else
                    {
                        throw new Exceptions.ServerException(response);
                    }
                }
                else
                {
                    // Get Item from Store
                    ret = this.Store(ItemType).Get(ID);
                }
            }

            return ret;
        }

        public override string ToString()
        {
            return this.User.KeyedName;
        }

        internal Session(Database Database, IO.Session IO)
        {
            this.Database = Database;
            this.IO = IO;

            this.ItemTypeNameCache = new Dictionary<String, ItemType>();
            this.ItemTypeIDCache = new Dictionary<String, ItemType>();
            this.StoresCache = new Dictionary<ItemType, Stores.Item>();
            

            // Default Selections
            this.ItemType("Value").AddToSelect("value,label");
            this.ItemType("Access").AddToSelect("can_get,can_update,can_delete,can_discover,can_change_access");
            this.ItemType("User").AddToSelect("default_vault");
            this.ItemType("File").AddToSelect("filename");
            this.ItemType("Vault").AddToSelect("vault_url");
        }
    }
}
