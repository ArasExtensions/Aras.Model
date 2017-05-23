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
using System.Xml;

namespace Aras.Model
{
    public class ItemType : IEquatable<ItemType>
    {
        private System.Int32 DefaultColumnWidth = 80;

        private readonly String[] SystemProperties = { "id", "config_id" };

        public Session Session { get; private set; }

        public String ID { get; private set; }

        public String Name { get; private set; }

        public String SingularLabel { get; private set; }

        public String PluralLabel { get; private set; }

        public Class ClassStructure { get; private set; }

        public Class GetClassFullname(String Fullname)
        {
            if (Fullname == null)
            {
                return this.ClassStructure;
            }
            else
            {
                String[] parts = Fullname.Split('/');
                Class currentclass = this.ClassStructure;

                foreach (String part in parts)
                {
                    Boolean found = false;

                    foreach (Class subclass in currentclass.Children)
                    {
                        if (subclass.Name.Equals(part))
                        {
                            currentclass = subclass;
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        return this.ClassStructure;
                    }
                }

                return currentclass;
            }
        }

        public Class GetClassName(String Name)
        {
            if (Name == null)
            {
                return this.ClassStructure;
            }
            else
            {
                Class ret = this.ClassStructure.Search(Name);

                if (ret != null)
                {
                    return ret;
                }
                else
                {
                    return this.ClassStructure;
                }
            }
        }

        protected Type _class;
        internal virtual Type Class
        {
            get
            {
                if (this._class == null)
                {
                    this._class = this.Session.Database.Server.ItemTypeClass(this.Name);

                    if (this._class == null)
                    {
                        if (this.Name.Equals("File"))
                        {
                            this._class = typeof(File);
                        }
                        else if (this is RelationshipType)
                        {
                            this._class = typeof(Relationship);
                        }
                        else
                        {
                            this._class = typeof(Item);
                        }
                    }
                }

                return this._class;
            }
        }

        private Dictionary<String, RelationshipType> RelationshipTypeNameCache;

        internal void AddRelationshipType(RelationshipType RelationshipType)
        {
            if (this.Equals(RelationshipType.Source))
            {
                this.RelationshipTypeNameCache[RelationshipType.Name] = RelationshipType;
            }
            else
            {
                throw new Exceptions.ArgumentException("Invalid RelationshipType Source");
            }
        }

        public IEnumerable<RelationshipType> RelationshipTypes
        {
            get
            {
                return this.RelationshipTypeNameCache.Values;
            }
        }

        public RelationshipType RelationshipType(String Name)
        {
            return this.RelationshipTypeNameCache[Name];
        }

        private String _tableName;
        internal String TableName
        {
            get
            {
                if (this._tableName == null)
                {
                    this._tableName = "[" + this.Name.ToLower().Replace(' ', '_') + "]";
                }

                return this._tableName;
            }
        }

        private Dictionary<String, PropertyType> _propertyTypeCache;
        private Dictionary<String, PropertyType> PropertyTypeCache
        {
            get
            {
                if (this._propertyTypeCache == null)
                {
                    this._propertyTypeCache = new Dictionary<String, PropertyType>();

                    IO.Item props = new IO.Item("Property", "get");
                    props.Select = "name,label,data_type,stored_length,readonly,default_value,data_source,is_required,sort_order,is_hidden,is_hidden2,column_width";
                    props.SetProperty("source_id", this.ID);
                    IO.Request request = this.Session.IO.Request(IO.Request.Operations.ApplyItem, props);
                    IO.Response response = request.Execute();

                    if (!response.IsError)
                    {
                        foreach (IO.Item thisprop in response.Items)
                        {
                            String name = thisprop.GetProperty("name");
                            String label = thisprop.GetProperty("label");
                            Boolean ReadOnly = "1".Equals(thisprop.GetProperty("readonly"));
                            Boolean Required = "1".Equals(thisprop.GetProperty("is_required"));
                            String DefaultString = thisprop.GetProperty("default_value");

                            // Sort Order
                            Int32 SortOrder = 0;

                            if (!Int32.TryParse(thisprop.GetProperty("sort_order"), out SortOrder))
                            {
                                SortOrder = 0;
                            }

                            Boolean InSearch = !("1".Equals(thisprop.GetProperty("is_hidden")));
                            Boolean InRelationshipGrid = !("1".Equals(thisprop.GetProperty("is_hidden2")));

                            // Column Width
                            Int32 ColumnWidth = 0;

                            if (!Int32.TryParse(thisprop.GetProperty("column_width"), out ColumnWidth))
                            {
                                ColumnWidth = DefaultColumnWidth;
                            }

                            if (!SystemProperties.Contains(name))
                            {
                                switch (thisprop.GetProperty("data_type"))
                                {
                                    case "string":
                                        Int32 length = 32;
                                        Int32.TryParse(thisprop.GetProperty("stored_length"), out length);
                                        this._propertyTypeCache[name] = new PropertyTypes.String(this, name, label, ReadOnly, Required, SortOrder, InSearch, InRelationshipGrid, ColumnWidth, DefaultString, length);
                                        break;
                                    case "ml_string":
                                        Int32 ml_length = 32;
                                        Int32.TryParse(thisprop.GetProperty("stored_length"), out ml_length);
                                        this._propertyTypeCache[name] = new PropertyTypes.MultilingualString(this, name, label, ReadOnly, Required, SortOrder, InSearch, InRelationshipGrid, ColumnWidth, DefaultString, ml_length);
                                        break;
                                    case "text":
                                        this._propertyTypeCache[name] = new PropertyTypes.Text(this, name, label, ReadOnly, Required, SortOrder, InSearch, InRelationshipGrid, ColumnWidth, DefaultString);
                                        break;
                                    case "formatted text":
                                        this._propertyTypeCache[name] = new PropertyTypes.FormattedText(this, name, label, ReadOnly, Required, SortOrder, InSearch, InRelationshipGrid, ColumnWidth, DefaultString);
                                        break;
                                    case "md5":
                                        this._propertyTypeCache[name] = new PropertyTypes.MD5(this, name, label, ReadOnly, Required, SortOrder, InSearch, InRelationshipGrid, ColumnWidth, DefaultString);
                                        break;
                                    case "image":
                                        this._propertyTypeCache[name] = new PropertyTypes.Image(this, name, label, ReadOnly, Required, SortOrder, InSearch, InRelationshipGrid, ColumnWidth, DefaultString);
                                        break;
                                    case "integer":

                                        if (DefaultString != null)
                                        {
                                            Int32 DefaultInteger = 0;
                                            Int32.TryParse(DefaultString, out DefaultInteger);
                                            this._propertyTypeCache[name] = new PropertyTypes.Integer(this, name, label, ReadOnly, Required, SortOrder, InSearch, InRelationshipGrid, ColumnWidth, DefaultInteger);
                                        }
                                        else
                                        {
                                            this._propertyTypeCache[name] = new PropertyTypes.Integer(this, name, label, ReadOnly, Required, SortOrder, InSearch, InRelationshipGrid, ColumnWidth, null);
                                        }

                                        break;
                                    case "item":

                                        String data_source = thisprop.GetProperty("data_source");

                                        if (!String.IsNullOrEmpty(data_source))
                                        {
                                            ItemType valueitemtype = this.Session.ItemTypeByID(data_source);
                                            this._propertyTypeCache[name] = new PropertyTypes.Item(this, name, label, ReadOnly, Required, SortOrder, InSearch, InRelationshipGrid, ColumnWidth, valueitemtype);
                                        }

                                        break;
                                    case "date":

                                        if (DefaultString != null)
                                        {
                                            DateTime DefaultDate;
                                            DateTime.TryParse(DefaultString, out DefaultDate);
                                            this._propertyTypeCache[name] = new PropertyTypes.Date(this, name, label, ReadOnly, Required, SortOrder, InSearch, InRelationshipGrid, ColumnWidth, DefaultDate);
                                        }
                                        else
                                        {
                                            this._propertyTypeCache[name] = new PropertyTypes.Date(this, name, label, ReadOnly, Required, SortOrder, InSearch, InRelationshipGrid, ColumnWidth, null);
                                        }

                                        break;
                                    case "list":
                                        this._propertyTypeCache[name] = new PropertyTypes.List(this, name, label, ReadOnly, Required, SortOrder, InSearch, InRelationshipGrid, ColumnWidth, thisprop.GetProperty("data_source"));
                                        break;
                                    case "decimal":

                                        if (DefaultString != null)
                                        {
                                            Decimal DefaultDecimal = 0;
                                            Decimal.TryParse(DefaultString, out DefaultDecimal);
                                            this._propertyTypeCache[name] = new PropertyTypes.Decimal(this, name, label, ReadOnly, Required, SortOrder, InSearch, InRelationshipGrid, ColumnWidth, DefaultDecimal);
                                        }
                                        else
                                        {
                                            this._propertyTypeCache[name] = new PropertyTypes.Decimal(this, name, label, ReadOnly, Required, SortOrder, InSearch, InRelationshipGrid, ColumnWidth, null);
                                        }

                                        break;
                                    case "float":

                                        if (DefaultString != null)
                                        {
                                            Double DefaultDouble = 0;
                                            Double.TryParse(DefaultString, out DefaultDouble);
                                            this._propertyTypeCache[name] = new PropertyTypes.Float(this, name, label, ReadOnly, Required, SortOrder, InSearch, InRelationshipGrid, ColumnWidth, DefaultDouble);
                                        }
                                        else
                                        {
                                            this._propertyTypeCache[name] = new PropertyTypes.Decimal(this, name, label, ReadOnly, Required, SortOrder, InSearch, InRelationshipGrid, ColumnWidth, null);
                                        }

                                        break;
                                    case "boolean":

                                        if (DefaultString != null)
                                        {
                                            Boolean DefaultBoolean = "1".Equals(DefaultString);
                                            this._propertyTypeCache[name] = new PropertyTypes.Boolean(this, name, label, ReadOnly, Required, SortOrder, InSearch, InRelationshipGrid, ColumnWidth, DefaultBoolean);
                                        }
                                        else
                                        {
                                            this._propertyTypeCache[name] = new PropertyTypes.Boolean(this, name, label, ReadOnly, Required, SortOrder, InSearch, InRelationshipGrid, ColumnWidth, null);
                                        }

                                        break;
                                    case "foreign":
                                        this._propertyTypeCache[name] = new PropertyTypes.Foreign(this, name, label, ReadOnly, Required, SortOrder, InSearch, InRelationshipGrid, ColumnWidth, DefaultString);
                                        break;
                                    case "federated":
                                        this._propertyTypeCache[name] = new PropertyTypes.Federated(this, name, label, ReadOnly, Required, SortOrder, InSearch, InRelationshipGrid, ColumnWidth, DefaultString);
                                        break;
                                    case "sequence":
                                        this._propertyTypeCache[name] = new PropertyTypes.Sequence(this, name, label, ReadOnly, Required, SortOrder, InSearch, InRelationshipGrid, ColumnWidth, DefaultString);
                                        break;
                                    case "filter list":
                                        Items.List valuefilterlist = (Items.List)this.Session.Lists.Store.Get(thisprop.GetProperty("data_source"));
                                        this._propertyTypeCache[name] = new PropertyTypes.FilterList(this, name, label, ReadOnly, Required, SortOrder, InSearch, InRelationshipGrid, ColumnWidth, valuefilterlist);
                                        break;
                                    default:
                                        throw new NotImplementedException("Property Type not implmented: " + thisprop.GetProperty("data_type"));
                                }
                            }
                        }
                    }
                    else
                    {
                        throw new Exceptions.ServerException(response);
                    }
                }

                return this._propertyTypeCache;
            }
        }

        public PropertyType PropertyType(String Name)
        {
            if (this.PropertyTypeCache.ContainsKey(Name))
            {
                return this.PropertyTypeCache[Name];
            }
            else
            {
                throw new Exceptions.PropertyException(this, Name);
            }
        }

        public Boolean HasPropertyType(String Name)
        {
            return this.PropertyTypeCache.ContainsKey(Name);
        }

        public IEnumerable<PropertyType> PropertyTypes
        {
            get
            {
                return this.PropertyTypeCache.Values;
            }
        }

        private List<PropertyType> _searchPropertyTypes;
        public IEnumerable<PropertyType> SearchPropertyTypes
        {
            get
            {
                if (this._searchPropertyTypes == null)
                {
                    this._searchPropertyTypes = new List<PropertyType>();

                    foreach (PropertyType proptype in this.PropertyTypes)
                    {
                        if (proptype.InSearch)
                        {
                            this._searchPropertyTypes.Add(proptype);
                        }
                    }

                    this._searchPropertyTypes.Sort();
                }

                return this._searchPropertyTypes;
            }
        }

        private String _searchSelect;
        public String SearchSelect
        {
            get
            {
                if (this._searchSelect == null)
                {
                    List<String> names = new List<String>();

                    foreach (PropertyType proptype in this.SearchPropertyTypes)
                    {
                        names.Add(proptype.Name);
                    }

                    this._searchSelect = String.Join(",", names);
                }

                return this._searchSelect;
            }
        }

        private List<PropertyType> _relationshipGridPropertyTypes;
        public IEnumerable<PropertyType> RelationshipGridPropertyTypes
        {
            get
            {
                if (this._relationshipGridPropertyTypes == null)
                {
                    this._relationshipGridPropertyTypes = new List<PropertyType>();

                    foreach (PropertyType proptype in this.PropertyTypes)
                    {
                        if (proptype.InRelationshipGrid)
                        {
                            this._relationshipGridPropertyTypes.Add(proptype);
                        }
                    }

                    this._relationshipGridPropertyTypes.Sort();
                }

                return this._relationshipGridPropertyTypes;
            }
        }

        private String _relationshipGridSelect;
        public String RelationshipGridSelect
        {
            get
            {
                if (this._relationshipGridPropertyTypes == null)
                {
                    List<String> names = new List<String>();

                    foreach(PropertyType proptype in this.RelationshipGridPropertyTypes)
                    {
                        names.Add(proptype.Name);
                    }

                    this._relationshipGridSelect = String.Join(",", names);
                }

                return this._relationshipGridSelect;
            }
        }

        private String DefaultLifeCycleMapCache;
        private Dictionary<String, String> LifeCycleMapCache;

        internal void AddLifeCycleMap(String Class, String ID)
        {
            if (String.IsNullOrEmpty(Class))
            {
                this.DefaultLifeCycleMapCache = ID;
            }
            else
            {
                this.LifeCycleMapCache[Class] = ID;
            }
        }

        public Items.LifeCycleMap LifeCycleMap(Class Class)
        {
            if (Class == null)
            {
                if (!String.IsNullOrEmpty(this.DefaultLifeCycleMapCache))
                {
                    return (Items.LifeCycleMap)this.Session.LifeCycleMaps.Store.Get(this.DefaultLifeCycleMapCache);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                if (Class.Fullname == null)
                {
                    if (!String.IsNullOrEmpty(this.DefaultLifeCycleMapCache))
                    {
                        return (Items.LifeCycleMap)this.Session.LifeCycleMaps.Store.Get(this.DefaultLifeCycleMapCache);
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    if (this.LifeCycleMapCache.ContainsKey(Class.Fullname))
                    {
                        return (Items.LifeCycleMap)this.Session.LifeCycleMaps.Store.Get(this.LifeCycleMapCache[Class.Fullname]);
                    }
                    else
                    {
                        if (!String.IsNullOrEmpty(this.DefaultLifeCycleMapCache))
                        {
                            return (Items.LifeCycleMap)this.Session.LifeCycleMaps.Store.Get(this.DefaultLifeCycleMapCache);
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }
        }

        public bool Equals(ItemType other)
        {
            if (other != null)
            {
                return this.ID.Equals(other.ID);
            }
            else
            {
                return false;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is ItemType)
            {
                return this.Equals((ItemType)obj);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return this.ID.GetHashCode();
        }

        public override string ToString()
        {
            return this.Name;
        }

        internal ItemType(Session Session, String ID, String Name, String SingularLabel, String PluralLabel, String ClassStructure)
        {
            this.RelationshipTypeNameCache = new Dictionary<string, RelationshipType>();
            this.LifeCycleMapCache = new Dictionary<String, String>();
            this.Session = Session;
            this.ID = ID;
            this.Name = Name;
            this.SingularLabel = SingularLabel;
            this.PluralLabel = PluralLabel;

            if (ClassStructure != null)
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(ClassStructure);
                XmlNode root = doc.SelectSingleNode("class");
                this.ClassStructure = new Class(this, null, root);
            }
            else
            {
                this.ClassStructure = new Class(this, null, null);
            }
        }
    }
}
