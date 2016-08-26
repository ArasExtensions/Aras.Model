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
using System.Xml;
using System.IO;
using System.Net;

namespace Aras.Model
{
    public class ItemType : IEquatable<ItemType>
    {
        public Session Session { get; private set; }

        public String ID { get; private set; }

        public String Name { get; private set; }

        private static String[] _itemSystemProperties = { "id", "config_id", "is_current", "generation" };
        internal virtual IEnumerable<String> SystemProperties
        {
            get
            {
                return _itemSystemProperties;
            }
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
                    this._class = this.Session.Database.ItemType(this.Name);

                    if (this._class == null)
                    {
                        this._class = typeof(Item);
                    }
                }

                return this._class;
            }
        }

        private Icon ReadIcon(String PropertyName)
        {
            IO.Item itemtype = new IO.Item("ItemType", "get");
            itemtype.Select = PropertyName;
            itemtype.ID = this.ID;
            IO.SOAPRequest request = new IO.SOAPRequest(IO.SOAPOperation.ApplyItem, this.Session, itemtype);
            IO.SOAPResponse response = request.Execute();

            if (!response.IsError)
            {
                return this.Session.Database.Server.GetIconByURL(response.Items.First().GetProperty(PropertyName));
            }
            else
            {
                throw new Exceptions.ServerException(response);
            }
        }

        private Icon _icon;
        public Icon Icon
        {
            get
            {
                if (this._icon == null)
                {
                    this._icon = this.ReadIcon("large_icon");
                }

                return this._icon;
            }
        }

        private Icon _openIcon;
        public Icon OpenIcon
        {
            get
            {
                if (this._openIcon == null)
                {
                    this._openIcon = this.ReadIcon("open_icon");
                }

                return this._openIcon;
            }
        }

        private Icon _closedIcon;
        public Icon ClosedIcon
        {
            get
            {
                if (this._closedIcon == null)
                {
                    this._closedIcon = this.ReadIcon("closed_icon");
                }

                return this._closedIcon;
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
                    props.Select = "name,label,data_type,stored_length,readonly,default_value,data_source";
                    props.SetProperty("source_id", this.ID);
                    IO.SOAPRequest request = new IO.SOAPRequest(IO.SOAPOperation.ApplyItem, this.Session, props);
                    IO.SOAPResponse response = request.Execute();

                    if (!response.IsError)
                    {
                        foreach (IO.Item thisprop in response.Items)
                        {
                            String name = thisprop.GetProperty("name");
                            String label = thisprop.GetProperty("label");
                            Boolean ReadOnly = "1".Equals(thisprop.GetProperty("readonly"));
                            String DefaultString = thisprop.GetProperty("default_value");

                            if (!SystemProperties.Contains(name))
                            {
                                switch (thisprop.GetProperty("data_type"))
                                {
                                    case "string":
                                        Int32 length = 32;
                                        Int32.TryParse(thisprop.GetProperty("stored_length"), out length);
                                        this._propertyTypeCache[name] = new PropertyTypes.String(this, name, label, ReadOnly, false, DefaultString, length);
                                        break;
                                    case "ml_string":
                                        Int32 ml_length = 32;
                                        Int32.TryParse(thisprop.GetProperty("stored_length"), out ml_length);
                                        this._propertyTypeCache[name] = new PropertyTypes.MultilingualString(this, name, label, ReadOnly, false, DefaultString, ml_length);
                                        break;
                                    case "text":
                                        this._propertyTypeCache[name] = new PropertyTypes.Text(this, name, label, ReadOnly, false, DefaultString);
                                        break;
                                    case "md5":
                                        this._propertyTypeCache[name] = new PropertyTypes.MD5(this, name, label, ReadOnly, false, DefaultString);
                                        break;
                                    case "image":
                                        this._propertyTypeCache[name] = new PropertyTypes.Image(this, name, label, ReadOnly, false, DefaultString);
                                        break;
                                    case "integer":

                                        if (DefaultString != null)
                                        {
                                            Int32 DefaultInteger = 0;
                                            Int32.TryParse(DefaultString, out DefaultInteger);
                                            this._propertyTypeCache[name] = new PropertyTypes.Integer(this, name, label, ReadOnly, false, DefaultInteger);
                                        }
                                        else
                                        {
                                            this._propertyTypeCache[name] = new PropertyTypes.Integer(this, name, label, ReadOnly, false, null);
                                        }

                                        break;
                                    case "item":
                                        ItemType valueitemtype = this.Session.ItemTypeByID(thisprop.GetProperty("data_source"));
                                        this._propertyTypeCache[name] = new PropertyTypes.Item(this, name, label, ReadOnly, false, valueitemtype);

                                        break;
                                    case "date":

                                        if (DefaultString != null)
                                        {
                                            DateTime DefaultDate;
                                            DateTime.TryParse(DefaultString, out DefaultDate);
                                            this._propertyTypeCache[name] = new PropertyTypes.Date(this, name, label, ReadOnly, false, DefaultDate);
                                        }
                                        else
                                        {
                                            this._propertyTypeCache[name] = new PropertyTypes.Date(this, name, label, ReadOnly, false, null);
                                        }

                                        break;
                                    case "list":
                                        List valuelist = (List)this.Session.Cache("List").Get(thisprop.GetProperty("data_source"));
                                        this._propertyTypeCache[name] = new PropertyTypes.List(this, name, label, ReadOnly, false, valuelist);
                                        break;
                                    case "decimal":

                                        if (DefaultString != null)
                                        {
                                            Decimal DefaultDecimal = 0;
                                            Decimal.TryParse(DefaultString, out DefaultDecimal);
                                            this._propertyTypeCache[name] = new PropertyTypes.Decimal(this, name, label, ReadOnly, false, DefaultDecimal);
                                        }
                                        else
                                        {
                                            this._propertyTypeCache[name] = new PropertyTypes.Decimal(this, name, label, ReadOnly, false, null);
                                        }

                                        break;
                                    case "float":

                                        if (DefaultString != null)
                                        {
                                            Double DefaultDouble = 0;
                                            Double.TryParse(DefaultString, out DefaultDouble);
                                            this._propertyTypeCache[name] = new PropertyTypes.Float(this, name, label, ReadOnly, false, DefaultDouble);
                                        }
                                        else
                                        {
                                            this._propertyTypeCache[name] = new PropertyTypes.Decimal(this, name, label, ReadOnly, false, null);
                                        }

                                        break;
                                    case "boolean":

                                        if (DefaultString != null)
                                        {
                                            Boolean DefaultBoolean = "1".Equals(DefaultString);
                                            this._propertyTypeCache[name] = new PropertyTypes.Boolean(this, name, label, ReadOnly, false, DefaultBoolean);
                                        }
                                        else
                                        {
                                            this._propertyTypeCache[name] = new PropertyTypes.Boolean(this, name, label, ReadOnly, false, null);
                                        }

                                        break;
                                    case "foreign":
                                        this._propertyTypeCache[name] = new PropertyTypes.Foreign(this, name, label, ReadOnly, false, DefaultString);
                                        break;
                                    case "federated":
                                        this._propertyTypeCache[name] = new PropertyTypes.Federated(this, name, label, ReadOnly, false, DefaultString);
                                        break;
                                    case "sequence":
                                        this._propertyTypeCache[name] = new PropertyTypes.Sequence(this, name, label, ReadOnly, false, DefaultString);
                                        break;
                                    case "filter list":
                                        List valuefilterlist = (List)this.Session.Cache("List").Get(thisprop.GetProperty("data_source"));
                                        this._propertyTypeCache[name] = new PropertyTypes.FilterList(this, name, label, ReadOnly, false, valuefilterlist);
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

        public void AddBooleanRuntime(String Name, String Label, System.Boolean ReadOnly, System.Boolean? Default)
        {
            if (!this.PropertyTypeCache.ContainsKey(Name))
            {
                this.PropertyTypeCache[Name] = new PropertyTypes.Boolean(this, Name, Label, ReadOnly, true, Default);
            }
            else
            {
                throw new Exceptions.ArgumentException("Duplicate Property Name");
            }
        }

        public void AddDateRuntime(String Name, String Label, System.Boolean ReadOnly, System.DateTime? Default)
        {
            if (!this.PropertyTypeCache.ContainsKey(Name))
            {
                this.PropertyTypeCache[Name] = new PropertyTypes.Date(this, Name, Label, ReadOnly, true, Default);
            }
            else
            {
                throw new Exceptions.ArgumentException("Duplicate Property Name");
            }
        }

        public void AddDecimalRuntime(String Name, String Label, System.Boolean ReadOnly, System.Decimal? Default)
        {
            if (!this.PropertyTypeCache.ContainsKey(Name))
            {
                this.PropertyTypeCache[Name] = new PropertyTypes.Decimal(this, Name, Label, ReadOnly, true, Default);
            }
            else
            {
                throw new Exceptions.ArgumentException("Duplicate Property Name");
            }
        }

        public void AddFloatRuntime(String Name, String Label, System.Boolean ReadOnly, System.Double? Default)
        {
            if (!this.PropertyTypeCache.ContainsKey(Name))
            {
                this.PropertyTypeCache[Name] = new PropertyTypes.Float(this, Name, Label, ReadOnly, true, Default);
            }
            else
            {
                throw new Exceptions.ArgumentException("Duplicate Property Name");
            }
        }

        public void AddImageRuntime(String Name, String Label, System.Boolean ReadOnly)
        {
            if (!this.PropertyTypeCache.ContainsKey(Name))
            {
                this.PropertyTypeCache[Name] = new PropertyTypes.Image(this, Name, Label, ReadOnly, true, null);
            }
            else
            {
                throw new Exceptions.ArgumentException("Duplicate Property Name");
            }
        }

        public void AddIntegerRuntime(String Name, String Label, System.Boolean ReadOnly, System.Int32? Default)
        {
            if (!this.PropertyTypeCache.ContainsKey(Name))
            {
                this.PropertyTypeCache[Name] = new PropertyTypes.Integer(this, Name, Label, ReadOnly, true, Default);
            }
            else
            {
                throw new Exceptions.ArgumentException("Duplicate Property Name");
            }
        }

        public void AddItemRuntime(String Name, String Label, System.Boolean ReadOnly, ItemType ValueType)
        {
            if (!this.PropertyTypeCache.ContainsKey(Name))
            {
                this.PropertyTypeCache[Name] = new PropertyTypes.Item(this, Name, Label, ReadOnly, true, ValueType);
            }
            else
            {
                throw new Exceptions.ArgumentException("Duplicate Property Name");
            }
        }

        public void AddListRuntime(String Name, String Label, System.Boolean ReadOnly, List ValueList)
        {
            if (!this.PropertyTypeCache.ContainsKey(Name))
            {
                this.PropertyTypeCache[Name] = new PropertyTypes.List(this, Name, Label, ReadOnly, true, ValueList);
            }
            else
            {
                throw new Exceptions.ArgumentException("Duplicate Property Name");
            }
        }

        public void AddMD5Runtime(String Name, String Label, System.Boolean ReadOnly, System.String Default)
        {
            if (!this.PropertyTypeCache.ContainsKey(Name))
            {
                this.PropertyTypeCache[Name] = new PropertyTypes.MD5(this, Name, Label, ReadOnly, true, Default);
            }
            else
            {
                throw new Exceptions.ArgumentException("Duplicate Property Name");
            }
        }

        public void AddMultilingualStringRuntime(String Name, String Label, System.Boolean ReadOnly, System.String Default, System.Int32 Length)
        {
            if (!this.PropertyTypeCache.ContainsKey(Name))
            {
                this.PropertyTypeCache[Name] = new PropertyTypes.MultilingualString(this, Name, Label, ReadOnly, true, Default, Length);
            }
            else
            {
                throw new Exceptions.ArgumentException("Duplicate Property Name");
            }
        }

        public void AddStringRuntime(String Name, String Label, System.Boolean ReadOnly, System.String Default, System.Int32 Length)
        {
            if (!this.PropertyTypeCache.ContainsKey(Name))
            {
                this.PropertyTypeCache[Name] = new PropertyTypes.String(this, Name, Label, ReadOnly, true, Default, Length);
            }
            else
            {
                throw new Exceptions.ArgumentException("Duplicate Property Name");
            }
        }

        public void AddTextRuntime(String Name, String Label, System.Boolean ReadOnly, System.String Default)
        {
            if (!this.PropertyTypeCache.ContainsKey(Name))
            {
                this.PropertyTypeCache[Name] = new PropertyTypes.Text(this, Name, Label, ReadOnly, true, Default);
            }
            else
            {
                throw new Exceptions.ArgumentException("Duplicate Property Name");
            }
        }

        internal PropertyTypes.VariableList AddVariableListRuntime(String Name, String Label, System.Boolean ReadOnly)
        {
            if (!this.PropertyTypeCache.ContainsKey(Name))
            {
                this.PropertyTypeCache[Name] = new PropertyTypes.VariableList(this, Name, Label, ReadOnly);
            }
            else
            {
                if (!(this.PropertyTypeCache[Name] is PropertyTypes.VariableList))
                {
                    throw new Exceptions.ArgumentException("Duplicate Property Name");
                }
            }

            return (PropertyTypes.VariableList)this.PropertyTypeCache[Name];
        }

        public PropertyType PropertyType(String Name)
        {
            if (this.PropertyTypeCache.ContainsKey(Name))
            {
                return this.PropertyTypeCache[Name];
            }
            else
            {
                throw new Exceptions.ArgumentException("Invalid property name: " + Name);
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

        private Dictionary<String, RelationshipType> RelationshipTypeCache;

        internal void AddRelationshipType(RelationshipType RelationshipType)
        {
            this.RelationshipTypeCache[RelationshipType.Name] = RelationshipType;
        }

        private Boolean RelationshipTypesLoaded { get; set; }

        private void LoadRelationshipTypes()
        {
            if (!this.RelationshipTypesLoaded)
            {
                IO.Item reltypes = new IO.Item("RelationshipType", "get");
                reltypes.Select = "relationship_id";
                reltypes.SetProperty("source_id", this.ID);

                IO.SOAPRequest request = new IO.SOAPRequest(IO.SOAPOperation.ApplyItem, this.Session, reltypes);
                IO.SOAPResponse response = request.Execute();

                if (!response.IsError)
                {
                    foreach (IO.Item reltype in response.Items)
                    {
                        ItemType itemtype = this.Session.ItemTypeByID(reltype.GetProperty("relationship_id"));
                    }

                    this.RelationshipTypesLoaded = true;
                }
                else
                {
                    throw new Exceptions.ServerException(response);
                }
            }
        }

        public IEnumerable<RelationshipType> RelationshipTypes
        {
            get
            {
                this.LoadRelationshipTypes();
                return this.RelationshipTypeCache.Values;
            }
        }

        public RelationshipType RelationshipType(String Name)
        {
            this.LoadRelationshipTypes();
            return this.RelationshipTypeCache[Name];
        }

        private List<PropertyType> SelectCache;

        public String _select;
        public String Select
        {
            get
            {
                if (this._select == null)
                {
                    List<String> names = new List<String>();

                    foreach (PropertyType proptype in this.SelectCache)
                    {
                        names.Add(proptype.Name);
                    }

                    this._select = String.Join(",", names);
                }

                return this._select;
            }
        }

        public void AddToSelect(String Names)
        {
            foreach (String name in Names.Split(','))
            {
                PropertyType proptype = this.PropertyType(name);

                if (!this.SelectCache.Contains(proptype))
                {
                    this.SelectCache.Add(proptype);
                    this._select = null;
                }
            }
        }

        public bool Equals(ItemType other)
        {
            if (other != null && other is ItemType)
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
            if (obj != null && obj is ItemType)
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

        internal ItemType(Session Session, String ID, String Name, String ClassStructure)
        {
            this.RelationshipTypeCache = new Dictionary<String, RelationshipType>();
            this.SelectCache = new List<PropertyType>();
            this.RelationshipTypesLoaded = false;
            this.Session = Session;
            this.ID = ID;
            this.Name = Name;

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
