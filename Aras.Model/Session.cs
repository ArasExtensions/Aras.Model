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

        public Transaction BeginTransaction()
        {
            return new Transaction(this);
        }

        private Dictionary<String, ItemType> ItemTypeNameCache;
        private Dictionary<String, ItemType> ItemTypeIDCache;
        private Dictionary<String, List> ListIDCache;

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

        public List ListByID(String ID)
        {
            if (this.ListIDCache.ContainsKey(ID))
            {
                return this.ListIDCache[ID];
            }
            else
            {
                throw new Exceptions.ArgumentException("Invalid List ID: " + ID);
            }
        }

        private void BuildCaches()
        {
            this.ItemTypeNameCache = new Dictionary<String, ItemType>();
            this.ItemTypeIDCache = new Dictionary<String, ItemType>();
            this.ListIDCache = new Dictionary<String, List>();

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

                    foreach(IO.Item itemtypelifecyclemap in dbitem.Relationships)
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

            // Build List Cache
            IO.Request listrequest = this.IO.Request(Aras.IO.Request.Operations.ApplyItem);
            IO.Item listquery = listrequest.NewItem("List", "get");
            listquery.Select = "id,name";
            listquery.OrderBy = "sort_order";
            IO.Item valuequery = listrequest.NewItem("Value", "get");
            valuequery.Select = "value,label";
            listquery.AddRelationship(valuequery);
            IO.Response listresponse = listrequest.Execute();

            if (!listresponse.IsError)
            {
                foreach(IO.Item dbitem in listresponse.Items)
                {
                    List list = new List(this, dbitem.ID, dbitem.GetProperty("name"));

                    foreach(IO.Item dbvalue in dbitem.Relationships)
                    {
                        ListValue listvalue = new ListValue(list, dbvalue.GetProperty("value"), dbvalue.GetProperty("label"));
                        list.AddListValue(listvalue);
                    }

                    this.ListIDCache[list.ID] = list;
                }
            }
            else
            {
                throw new Exceptions.ServerException(listresponse);
            }

            // Refresh Life Cycle Map Store
            this.LifeCycleMaps.Store.Refresh();
        }

        public Query Query(ItemType ItemType)
        {
            return new Query(ItemType);
        }

        public Query Query(String ItemType)
        {
            return this.Query(this.ItemType(ItemType));
        }

        private Query _lifeCycleMaps;
        public Query LifeCycleMaps
        {
            get
            {
                if (this._lifeCycleMaps == null)
                {
                    this._lifeCycleMaps = this.Query("Life Cycle Map");
                    this._lifeCycleMaps.Select = "name";
                    this._lifeCycleMaps.Relationship("Life Cycle State").Select = "name";
                }

                return this._lifeCycleMaps;
            }
        }

        private Dictionary<String, Cache.Item> ItemCache;

        internal Cache.Item GetItemCache(ItemType ItemType)
        {
            Cache.Item cacheitem = new Cache.Item(ItemType);
            this.ItemCache[cacheitem.ID] = cacheitem;
            return cacheitem;
        }

        internal Cache.Item GetItemCache(ItemType ItemType, String ID, String ConfigID, Int32 Generation, Boolean IsCurrent)
        {
            if (!this.ItemCache.ContainsKey(ID))
            {
                this.ItemCache[ID] = new Cache.Item(ItemType, ID, ConfigID, Generation, IsCurrent);
            }

            return this.ItemCache[ID];
        }

        internal Session(Database Database, IO.Session IO)
        {
            this.ItemCache = new Dictionary<String, Cache.Item>();
            this.Database = Database;
            this.IO = IO;
            this.BuildCaches();
        }
    }
}
