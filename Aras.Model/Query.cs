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
using System.ComponentModel;

namespace Aras.Model
{
    public class Query : INotifyPropertyChanged
    {
        private const String SystemProperties = "id,config_id,is_current,generation,classification,locked_by_id,permission_id";

        public const Int32 MinPageSize = 5;
        public const Int32 DefaultPageSize = 25;
        public const Int32 MaxPageSize = 100;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(String Name)
        {
            if (this.PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(Name));
            }
        }

        public Query Parent { get; private set; }

        private Condition _condition;
        public Condition Condition
        {
            get
            {
                return this._condition;
            }
            set
            {
                this._condition = value;
            }
        }

        private Int32 _pageSize;
        public Int32 PageSize
        {
            get
            {
                return this._pageSize;
            }
            set
            {
                if (this._pageSize != value)
                {
                    if (value >= MinPageSize && value <= MaxPageSize)
                    {
                        this._pageSize = value;
                        this.OnPropertyChanged("PageSize");
                    }
                }
            }
        }

        private Int32 _page;
        public Int32 Page
        {
            get
            {
                return this._page;
            }
            set
            {
                if (this._page != value)
                {
                    if (value >= 1)
                    {
                        this._page = value;
                        this.OnPropertyChanged("Page");
                    }
                    else
                    {
                        throw new Exceptions.ArgumentException("Invalid Page: " + value);
                    }
                }
            }
        }

        private Boolean _paging;
        public Boolean Paging
        {
            get
            {
                return this._paging;
            }
            set
            {
                if (value != this._paging)
                {
                    this._paging = value;
                    this.OnPropertyChanged("Paging");
                }
            }
        }

        private Boolean _recursive;
        public Boolean Recursive
        {
            get
            {
                return this._recursive;
            }
            set
            {
                if (value != this._recursive)
                {
                    this._recursive = value;
                    this.OnPropertyChanged("Recursive");
                }
            }
        }

        public ItemType ItemType { get; private set; }

        public String OrderBy { get; set; }

        private Dictionary<String, PropertyType> SelectCache;
        private Dictionary<PropertyTypes.Item, Query> SelectPropertyCache;

        public String Select
        {
            get
            {
                List<String> names = new List<String>();

                foreach(PropertyType proptype in this.SelectCache.Values)
                {
                    names.Add(proptype.Name);
                }

                return String.Join(",", names);
            }
            set
            {
                this.SelectCache.Clear();
                this.SelectPropertyCache.Clear();

                if (!String.IsNullOrEmpty(value))
                {
                    String select = value;

                    // Ensure source_id selected for Relationships
                    if (this.ItemType is RelationshipType)
                    {
                        select += ",source_id";
                    }

                    foreach (String name in select.Split(new Char[] { ',' }))
                    {
                        PropertyType proptype = this.ItemType.PropertyType(name);

                        if (!this.SelectCache.ContainsKey(proptype.Name))
                        {
                            this.SelectCache[proptype.Name] = proptype;

                            if (proptype is PropertyTypes.Item)
                            {
                                if (proptype.Name.Equals("source_id") && (this.Parent != null))
                                {
                                    this.SelectPropertyCache[(PropertyTypes.Item)proptype] = this.Parent;
                                }
                                else if (proptype.Name.Equals("related_id") && (this.Parent != null) && this.Parent.Recursive && this.Parent.ItemType.Equals(((PropertyTypes.Item)proptype).ValueType))
                                {
                                    this.SelectPropertyCache[(PropertyTypes.Item)proptype] = this.Parent;
                                }
                                else
                                {
                                    this.SelectPropertyCache[(PropertyTypes.Item)proptype] = new Query(this, ((PropertyTypes.Item)proptype).ValueType);
                                }
                            }
                        }
                    }
                }
            }
        }

        public IEnumerable<PropertyType> PropertyTypes
        {
            get
            {
                return this.SelectCache.Values;
            }
        }

        public PropertyType PropertyType(String PropertyName)
        {
            if (this.SelectCache.ContainsKey(PropertyName))
            {
                return this.SelectCache[PropertyName];
            }
            else
            {
                throw new Exceptions.PropertyException(this.ItemType, PropertyName);
            }
        }

        public Query Property(PropertyTypes.Item PropertyType)
        {
            if (this.SelectPropertyCache.ContainsKey(PropertyType))
            {
                return this.SelectPropertyCache[PropertyType];
            }
            else
            {
                throw new Exceptions.ArgumentException("Property is not selected in query: " + PropertyType.ToString());
            }
        }

        public Query Property(String PropertyType)
        {
            PropertyType proptype = this.ItemType.PropertyType(PropertyType);

            if (proptype is PropertyTypes.Item)
            {
                return this.Property((PropertyTypes.Item)proptype);
            }
            else
            {
                throw new Exceptions.ArgumentException("Invalid PropertyType: " + PropertyType);
            }
        }

        public IEnumerable<Query> Properties
        {
            get
            {
                return this.SelectPropertyCache.Values;
            }
        }

        private Dictionary<RelationshipType, Query> RelationshipCache;

        public Query Relationship(String RelationshipType)
        {
            return this.Relationship(this.ItemType.RelationshipType(RelationshipType));
        }

        public Query Relationship(RelationshipType RelationshipType)
        {
            if (this.ItemType.RelationshipTypes.Contains(RelationshipType))
            {
                if (!this.RelationshipCache.ContainsKey(RelationshipType))
                {
                    this.RelationshipCache[RelationshipType] = new Query(this, RelationshipType);
                }

                return this.RelationshipCache[RelationshipType];
            }
            else
            {
                throw new Exceptions.ArgumentException("Invalid RelationshipType: " + RelationshipType.ToString());
            }
        }

        public IEnumerable<Query> Relationships
        {
            get
            {
                return this.RelationshipCache.Values;
            }
        }

        internal IO.Item DBQuery(String ID)
        {
            IO.Item query = new IO.Item(this.ItemType.Name, "get");
            query.ID = ID;

            // Set Select
            if (String.IsNullOrEmpty(this.Select))
            {
                // Use default selection
                this.Select = "keyed_name";
            }

            query.Select = SystemProperties + "," + this.Select;

            foreach (PropertyTypes.Item proptype in this.SelectPropertyCache.Keys)
            {
                IO.Item propquery = this.SelectPropertyCache[proptype].DBQuery();
                query.SetPropertyItem(proptype.Name, propquery);
            }

            foreach (Query relquery in this.Relationships)
            {
                query.AddRelationship(relquery.DBQuery());
            }

            return query;
        }

        internal IO.Item DBQuery()
        {
            if (this.Condition != null)
            {
                IO.Item query = new IO.Item(this.ItemType.Name, "get");

                // Set Select
                if (String.IsNullOrEmpty(this.Select))
                {
                    // Use default selection
                    this.Select = "keyed_name";
                }

                query.OrderBy = this.OrderBy;

                query.Select = SystemProperties + "," + this.Select;

                // Set Where from Condtion
                query.Where = this.Condition.Where(this.ItemType);

                // Set Paging
                if (this.Paging)
                {
                    query.PageSize = this.PageSize;
                    query.Page = this.Page;
                }

                foreach (PropertyTypes.Item proptype in this.SelectPropertyCache.Keys)
                {
                    if (!proptype.Name.Equals("source_id") && !proptype.Name.Equals("related_id"))
                    {
                        IO.Item propquery = this.SelectPropertyCache[proptype].DBQuery();
                        query.SetPropertyItem(proptype.Name, propquery);
                    }
                }

                foreach (Query relquery in this.Relationships)
                {
                    query.AddRelationship(relquery.DBQuery());
                }

                return query;
            }
            else
            {
                return null;
            }
        }

        private Store _store;
        public Store Store
        {
            get
            {
                if (this._store == null)
                {
                    this._store = new Store(this);
                }

                return this._store;
            }
        }

        public override string ToString()
        {
            return this.ItemType.ToString();
        }

        public Query(ItemType ItemType)
            :this(null, ItemType)
        {

        }

        internal Query(Query Parent, ItemType ItemType)
        {
            this.SelectCache = new Dictionary<String, PropertyType>();
            this.SelectPropertyCache = new Dictionary<PropertyTypes.Item, Query>();
            this.RelationshipCache = new Dictionary<RelationshipType, Query>();
            this.Parent = Parent;
            this.ItemType = ItemType;
            this._pageSize = DefaultPageSize;
            this._paging = false;
            this._page = 1;
            this._recursive = false;
            this._condition = Aras.Conditions.All();
        }
    }
}
