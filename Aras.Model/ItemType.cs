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
    public class ItemType
    {
        private readonly String[] SystemProperties = new String[] { "id", "config_id", "source_id", "related_id" };

        public Session Session { get; private set; }

        public String ID { get; private set; }

        public String Name { get; private set; }

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

        private Dictionary<String, PropertyType> _propertyTypeCache;
        private Dictionary<String, PropertyType> PropertyTypeCache
        {
            get
            {
                if (this._propertyTypeCache == null)
                {
                    this._propertyTypeCache = new Dictionary<String, PropertyType>();

                    IO.Item props = new IO.Item("Property", "get");
                    props.Select = "name,data_type,stored_length,readonly,default_value,data_source";
                    props.SetProperty("source_id", this.ID);
                    IO.SOAPRequest request = new IO.SOAPRequest(IO.SOAPOperation.ApplyItem, this.Session, props);
                    IO.SOAPResponse response = request.Execute();

                    if (!response.IsError)
                    {
                        foreach(IO.Item thisprop in response.Items)
                        {
                            String name = thisprop.GetProperty("name");
                            Boolean ReadOnly = "1".Equals(thisprop.GetProperty("readonly"));
                            String DefaultString = thisprop.GetProperty("default_value");

                            if (!SystemProperties.Contains(name))
                            {
                                switch (thisprop.GetProperty("data_type"))
                                {
                                    case "string":
                                        Int32 length = 32;
                                        Int32.TryParse(thisprop.GetProperty("stored_length"), out length);
                                        this._propertyTypeCache[name] = new PropertyTypes.String(this, name, ReadOnly, DefaultString, length);
                                        break;
                                    case "ml_string":
                                        Int32 ml_length = 32;
                                        Int32.TryParse(thisprop.GetProperty("stored_length"), out ml_length);
                                        this._propertyTypeCache[name] = new PropertyTypes.MultilingualString(this, name, ReadOnly, DefaultString, ml_length);
                                        break;
                                    case "text":
                                        this._propertyTypeCache[name] = new PropertyTypes.Text(this, name, ReadOnly, DefaultString);
                                        break;
                                    case "md5":
                                        this._propertyTypeCache[name] = new PropertyTypes.MD5(this, name, ReadOnly, DefaultString);
                                        break;
                                    case "image":
                                        this._propertyTypeCache[name] = new PropertyTypes.Image(this, name, ReadOnly, DefaultString);
                                        break;
                                    case "integer":

                                        if (DefaultString != null)
                                        {
                                            Int32 DefaultInteger = 0;
                                            Int32.TryParse(DefaultString, out DefaultInteger);
                                            this._propertyTypeCache[name] = new PropertyTypes.Integer(this, name, ReadOnly, DefaultInteger);
                                        }
                                        else
                                        {
                                            this._propertyTypeCache[name] = new PropertyTypes.Integer(this, name, ReadOnly, null);
                                        }

                                        break;
                                    case "item":
                                        ItemType valueitemtype = this.Session.ItemTypeByID(thisprop.GetProperty("data_source"));
                                        this._propertyTypeCache[name] = new PropertyTypes.Item(this, name, ReadOnly, valueitemtype);

                                        break;
                                    case "date":

                                        if (DefaultString != null)
                                        {
                                            DateTime DefaultDate;
                                            DateTime.TryParse(DefaultString, out DefaultDate);
                                            this._propertyTypeCache[name] = new PropertyTypes.Date(this, name, ReadOnly, DefaultDate);
                                        }
                                        else
                                        {
                                            this._propertyTypeCache[name] = new PropertyTypes.Date(this, name, ReadOnly, null);
                                        }

                                        break;
                                    case "list":
                                        List valuelist = this.Session.ListByID(thisprop.GetProperty("data_source"));
                                        this._propertyTypeCache[name] = new PropertyTypes.List(this, name, ReadOnly, valuelist);

                                        break;
                                    case "decimal":

                                        if (DefaultString != null)
                                        {
                                            Decimal DefaultDecimal = 0;
                                            Decimal.TryParse(DefaultString, out DefaultDecimal);
                                            this._propertyTypeCache[name] = new PropertyTypes.Decimal(this, name, ReadOnly, DefaultDecimal);
                                        }
                                        else
                                        {
                                            this._propertyTypeCache[name] = new PropertyTypes.Decimal(this, name, ReadOnly, null);
                                        }

                                        break;
                                    case "float":

                                        if (DefaultString != null)
                                        {
                                            Double DefaultDouble = 0;
                                            Double.TryParse(DefaultString, out DefaultDouble);
                                            this._propertyTypeCache[name] = new PropertyTypes.Float(this, name, ReadOnly, DefaultDouble);
                                        }
                                        else
                                        {
                                            this._propertyTypeCache[name] = new PropertyTypes.Decimal(this, name, ReadOnly, null);
                                        }

                                        break;
                                    case "boolean":

                                        if (DefaultString != null)
                                        {
                                            Boolean DefaultBoolean = "1".Equals(DefaultString);
                                            this._propertyTypeCache[name] = new PropertyTypes.Boolean(this, name, ReadOnly, DefaultBoolean);
                                        }
                                        else
                                        {
                                            this._propertyTypeCache[name] = new PropertyTypes.Boolean(this, name, ReadOnly, null);
                                        }

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
            return this.PropertyTypeCache[Name];
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
                    foreach(IO.Item reltype in response.Items)
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

        public override string ToString()
        {
            return this.Name;
        }

        internal ItemType(Session Session, String ID, String Name)
        {
            this.RelationshipTypeCache = new Dictionary<String, RelationshipType>();
            this.RelationshipTypesLoaded = false;
            this.Session = Session;
            this.ID = ID;
            this.Name = Name;
        }
    }
}
