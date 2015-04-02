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
using System.IO;

namespace Aras.Model
{
    public class Session
    {
        private static String[] IgnoreItemTypes = new String[] { "ItemType", "List" };

        public Database Database { get; private set; }

        public string Name
        {
            get
            {
                return this.Username;
            }
        }

        public Guid ID { get; private set; }

        private String UserID { get; set; }

        private Cache.Item _user;
        public Cache.Item User
        {
            get
            {
                if (this._user == null)
                {
                    Request.Item request = this.Create(this.ItemType("User").Action("get"));
                    request.AddSelection("keyed_name");
                    request.Condition.AddProperty("id", Conditions.Operator.Equals, this.UserID);
                    Response.IEnumerable<Response.Item> response = request.Execute();
                    this._user = response.First().Cache;
                }

                return this._user;
            }
        }

        public String Username { get; private set; }

        public String Password { get; private set; }

        public void Logout()
        {

        }

        private DirectoryInfo _workspace;
        public DirectoryInfo Workspace
        {
            get
            {
                return this._workspace;
            }
            set
            {
                this._workspace = value;

                if (this._workspace != null)
                {
                    if (!this._workspace.Exists)
                    {
                        this._workspace.Create();
                    }
                }
            }
        }

        public void ClearWorkspace()
        {
            if (this.Workspace != null)
            {
                foreach (FileInfo file in this.Workspace.GetFiles())
                {
                    file.Delete();
                }
            }
        }

        private Dictionary<String, ItemType> _itemTypesCache;
        internal Dictionary<String, ItemType> ItemTypesCache
        {
            get
            {
                if (this._itemTypesCache == null)
                {
                    this._itemTypesCache = new Dictionary<String, ItemType>();

                    IO.Item itemtypes = new IO.Item("ItemType", "get");
                    itemtypes.Select = "name,is_relationship,class_structure";

                    IO.SOAPRequest request = new IO.SOAPRequest(IO.SOAPOperation.ApplyItem, this, itemtypes);
                    IO.SOAPResponse response = request.Execute();

                    if (response.IsError)
                    {
                        throw new Exceptions.ServerException(response.ErrorMessage);
                    }

                    foreach (IO.Item thisitemtype in response.Items)
                    {
                        String name = thisitemtype.GetProperty("name");

                        if (thisitemtype.GetProperty("is_relationship", "0").Equals("1"))
                        {
                            this._itemTypesCache[name] = new RelationshipType(this, name, thisitemtype.GetProperty("class_structure"));
                        }
                        else
                        {
                            this._itemTypesCache[name] = new ItemType(this, name, thisitemtype.GetProperty("class_structure"));
                        }
                    }

                    IO.Item relationshiptypes = new IO.Item("RelationshipType", "get");
                    relationshiptypes.Select = "name,source_id(name),related_id(name)";

                    IO.SOAPRequest relrequest = new IO.SOAPRequest(IO.SOAPOperation.ApplyItem, this, relationshiptypes);
                    IO.SOAPResponse relresponse = relrequest.Execute();

                    if (relresponse.IsError)
                    {
                        throw new Exceptions.ServerException(relresponse.ErrorMessage);
                    }

                    foreach (IO.Item relationshiptype in relresponse.Items)
                    {
                        String name = relationshiptype.GetProperty("name");
                        IO.Item sourceitem = relationshiptype.GetPropertyItem("source_id");

                        if (sourceitem != null)
                        {
                            String sourcename = sourceitem.GetProperty("name");
                            ((RelationshipType)this._itemTypesCache[name]).SourceType = this._itemTypesCache[sourcename];
                        }

                        IO.Item relateditem = relationshiptype.GetPropertyItem("related_id");

                        if (relateditem != null)
                        {
                            String relatedname = relateditem.GetProperty("name");
                            ((RelationshipType)this._itemTypesCache[name]).RelatedType = this._itemTypesCache[relatedname];
                        }
                    }
                }

                return this._itemTypesCache;
            }
        }

        internal ItemType AnyItemType(String Name)
        {
            return this.ItemTypesCache[Name];
        }

        private List<ItemType> _itemTypes;
        public IEnumerable<ItemType> ItemTypes
        {
            get
            {
                if (this._itemTypes == null)
                {
                    this._itemTypes = new List<ItemType>();

                    foreach (ItemType itemtype in this.ItemTypesCache.Values)
                    {
                        if (!IgnoreItemTypes.Contains(itemtype.Name))
                        {
                            if (!(itemtype is RelationshipType))
                            {
                                this._itemTypes.Add(itemtype);
                            }
                        }
                    }
                }

                return this._itemTypes;
            }
        }

        public ItemType ItemType(String Name)
        {
            foreach (ItemType itemtype in this.ItemTypes)
            {
                if (itemtype.Name.Equals(Name))
                {
                    return itemtype;
                }
            }

            return null;
        }

        private Dictionary<ItemType, Dictionary<String, Cache.Item>> ItemsCache { get; set; }

        internal Cache.Item GetItemFromCache(ItemType ItemType, String ID)
        {
            if (this.ItemsCache.ContainsKey(ItemType))
            {
                if (this.ItemsCache[ItemType].ContainsKey(ID))
                {
                    return this.ItemsCache[ItemType][ID];
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        internal void AddItemToCache(Cache.Item Item)
        {
            if (!this.ItemsCache.ContainsKey(Item.ItemType))
            {
                this.ItemsCache[Item.ItemType] = new Dictionary<String, Cache.Item>();
            }

            this.ItemsCache[Item.ItemType][Item.ID] = Item;
        }

        internal void RemoveItemFromCache(Cache.Item Item)
        {
            if (this.ItemsCache.ContainsKey(Item.ItemType))
            {
                if (this.ItemsCache[Item.ItemType].ContainsKey(Item.ID))
                {
                    this.ItemsCache[Item.ItemType].Remove(Item.ID);
                }
            }
        }

        public Request.Item Create(Action Action)
        {
            Cache.Item cacheitem = new Cache.Item(Action.ItemType);
            Request.Item requestitem = new Request.Item(cacheitem, Action);
            return requestitem;
        }

        public override string ToString()
        {
            return this.User.ToString();
        }

        internal Session(Database Database, String UserID, String Username, String Password)
            :base()
        {
            this.ItemsCache = new Dictionary<ItemType, Dictionary<String, Cache.Item>>();
            this.Database = Database;
            this.Username = Username;
            this.Password = Password;
            this.UserID = UserID;
            this.ID = Guid.NewGuid();
        }
    }
}
