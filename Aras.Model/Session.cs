/*  
  Copyright 2017 Processwall Limited

  Licensed under the Apache License, Version 2.0 (the "License");
  you may not use this file except in compliance with the License.
  You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

  Unless required by applicable law or agreed to in writing, software
  distributed under the License is distributed on an "AS IS" BASIS,
  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
  See the License for the specific language governing permissions and
  limitations under the License.
 
  Company: Processwall Limited
  Address: The Winnowing House, Mill Lane, Askham Richard, York, YO23 3NW, United Kingdom
  Tel:     +44 113 815 3440
  Web:     http://www.processwall.com
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

        public Boolean OptimiseAML { get; set; }

        public IO.Session IO { get; private set; }

        public String ID
        {
            get
            {
                return this.IO.UserID;
            }
        }

        private readonly object _cacheDirectoryLock = new object();
        private DirectoryInfo _cacheDirectory;
        internal DirectoryInfo CacheDirectory
        {
            get
            {
                lock (this._cacheDirectory)
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
        }

        public Transaction BeginTransaction()
        {
            return new Transaction(this);
        }

        private Dictionary<String, ItemType> ItemTypeNameCache;
        private Dictionary<String, ItemType> ItemTypeIDCache;

        public ItemType ItemType(String Name)
        {
            if (this.ItemTypeNameCache.ContainsKey(Name))
            {
                return this.ItemTypeNameCache[Name];
            }
            else
            {
                throw new Exceptions.ArgumentException("Invalid ItemType Name: " + Name);
            }
        }

        public ItemType ItemTypeByID(String ID)
        {
            if (this.ItemTypeIDCache.ContainsKey(ID))
            {
                return this.ItemTypeIDCache[ID];
            }
            else
            {
                throw new Exceptions.ArgumentException("Invalid ItemType ID: " + ID);
            }
        }

        private readonly object ItemTypeCacheLock = new object();
        private void BuildCaches()
        {
            lock (this.ItemTypeCacheLock)
            {
                this.ItemTypeNameCache = new Dictionary<String, ItemType>();
                this.ItemTypeIDCache = new Dictionary<String, ItemType>();

                // Build ItemType Cache
                IO.Request itemtyperequest = this.IO.Request(Aras.IO.Request.Operations.ApplyItem);
                IO.Item itemtypequery = itemtyperequest.NewItem("ItemType", "get");
                itemtypequery.Select = "id,name,is_relationship,class_structure,label,label_plural";

                IO.Item lifecyclemapquery = itemtyperequest.NewItem("ItemType Life Cycle", "get");
                lifecyclemapquery.Select = "class_path,related_id";
                itemtypequery.AddRelationship(lifecyclemapquery);

                IO.Response itemtyperesponse = itemtyperequest.Execute();

                if (!itemtyperesponse.IsError)
                {
                    foreach (IO.Item dbitem in itemtyperesponse.Items)
                    {
                        ItemType itemtype = null;

                        if (dbitem.GetProperty("is_relationship").Equals("1"))
                        {
                            itemtype = new RelationshipType(this, dbitem.ID, dbitem.GetProperty("name"), dbitem.GetProperty("label"), dbitem.GetProperty("label_plural"), dbitem.GetProperty("class_structure"));
                        }
                        else
                        {
                            itemtype = new ItemType(this, dbitem.ID, dbitem.GetProperty("name"), dbitem.GetProperty("label"), dbitem.GetProperty("label_plural"), dbitem.GetProperty("class_structure"));
                        }

                        this.ItemTypeIDCache[itemtype.ID] = itemtype;
                        this.ItemTypeNameCache[itemtype.Name] = itemtype;

                        foreach (IO.Item itemtypelifecyclemap in dbitem.Relationships)
                        {
                            itemtype.AddLifeCycleMap(itemtypelifecyclemap.GetProperty("class_path"), itemtypelifecyclemap.GetPropertyItem("related_id").ID);
                        }
                    }
                }
                else
                {
                    throw new Exceptions.ServerException(itemtyperesponse);
                }

                // Build RelationshipType Cache
                IO.Request relationshiptyperequest = this.IO.Request(Aras.IO.Request.Operations.ApplyItem);
                IO.Item relationshiptypequery = relationshiptyperequest.NewItem("RelationshipType", "get");
                relationshiptypequery.Select = "relationship_id,source_id(id),related_id(id),grid_view";
                IO.Response relationshiptyperesponse = relationshiptyperequest.Execute();

                if (!relationshiptyperesponse.IsError)
                {
                    foreach (IO.Item dbitem in relationshiptyperesponse.Items)
                    {
                        RelationshipType relationshiptype = (RelationshipType)this.ItemTypeIDCache[dbitem.GetProperty("relationship_id")];

                        String source_id = dbitem.GetProperty("source_id");

                        if (!String.IsNullOrEmpty(source_id))
                        {
                            relationshiptype.Source = this.ItemTypeIDCache[source_id];
                            relationshiptype.Source.AddRelationshipType(relationshiptype);
                        }

                        String related_id = dbitem.GetProperty("related_id");

                        if (!String.IsNullOrEmpty(related_id))
                        {
                            relationshiptype.Related = this.ItemTypeIDCache[related_id];
                        }

                        switch (dbitem.GetProperty("grid_view"))
                        {
                            case "right":
                                relationshiptype.RelationshipGridView = RelationshipGridViews.Right;
                                break;
                            case "intermix":
                                relationshiptype.RelationshipGridView = RelationshipGridViews.InterMix;
                                break;
                            default:
                                relationshiptype.RelationshipGridView = RelationshipGridViews.Left;
                                break;
                        }
                    }
                }
                else
                {
                    throw new Exceptions.ServerException(relationshiptyperesponse);
                }
            }
        }

        public Query Query(ItemType ItemType)
        {
            return new Query(ItemType);
        }

        public Query Query(String ItemType)
        {
            return this.Query(this.ItemType(ItemType));
        }

        private readonly object _lifeCycleMapsLock = new object();
        private Queries.LifeCycleMap _lifeCycleMaps;
        public Queries.LifeCycleMap LifeCycleMaps
        {
            get
            {
                lock (this._lifeCycleMapsLock)
                {
                    if (this._lifeCycleMaps == null)
                    {
                        this._lifeCycleMaps = new Queries.LifeCycleMap(this);
                    }

                    return this._lifeCycleMaps;
                }
            }
        }

        private readonly object _listsLock = new object();
        private Queries.List _lists;
        public Queries.List Lists
        {
            get
            {
                lock (this._listsLock)
                {
                    if (this._lists == null)
                    {
                        this._lists = new Queries.List(this);
                    }

                    return this._lists;
                }
            }
        }

        private readonly object _usersLock = new object();
        private Queries.User _users;
        public Queries.User Users
        {
            get
            {
                lock (this._usersLock)
                {
                    if (this._users == null)
                    {
                        this._users = new Queries.User(this);
                    }

                    return this._users;
                }
            }
        }

        private readonly object ItemCacheLock = new object();
        private Dictionary<String, Cache.Item> ItemCache;

        internal Cache.Item GetItemCache(ItemType ItemType)
        {
            lock (this.ItemCacheLock)
            {
                Cache.Item cacheitem = new Cache.Item(ItemType);
                this.ItemCache[cacheitem.ID] = cacheitem;
                return cacheitem;
            }
        }

        internal Cache.Item GetItemCache(ItemType ItemType, String ID, String ConfigID, Int32 Generation, Boolean IsCurrent)
        {
            lock (this.ItemCacheLock)
            {
                if (!this.ItemCache.ContainsKey(ID))
                {
                    this.ItemCache[ID] = new Cache.Item(ItemType, ID, ConfigID, Generation, IsCurrent);
                }

                return this.ItemCache[ID];
            }
        }

        internal Session(Database Database, IO.Session IO)
        {
            this.ItemCache = new Dictionary<String, Cache.Item>();
            this.OptimiseAML = true;
            this.Database = Database;
            this.IO = IO;
            this.BuildCaches();
        }
    }
}
