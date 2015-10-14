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
using System.Xml;
using System.IO;

namespace Aras.Model
{
    public class ItemType
    {
        private static String[] ReadOnlyProperties = new String[] { "id", "config_id", "keyed_name", "generation", "is_current", "created_by_id", "created_on", "locked_by_id", "modified_by_id", "modified_on" };

        public Session Session { get; private set; }

        public String Name { get; private set; }

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

        private Dictionary<String, Action> _actionsCache;

        private Dictionary<String, Action> ActionCache
        {
            get
            {
                if (this._actionsCache == null)
                {
                    this._actionsCache = new Dictionary<String, Action>();
                    this._actionsCache["add"] = new Action(this, "add");
                    this._actionsCache["get"] = new Action(this, "get");
                    this._actionsCache["update"] = new Action(this, "update");
                    this._actionsCache["edit"] = new Action(this, "edit");
                    this._actionsCache["delete"] = new Action(this, "delete");
                    this._actionsCache["lock"] = new Action(this, "lock");
                    this._actionsCache["unlock"] = new Action(this, "unlock");
                }

                return this._actionsCache;
            }
        }

        public IEnumerable<Action> Actions
        {
            get
            {
                return this.ActionCache.Values;
            }
        }

        public Action Action(String Name)
        {
            if (!this.ActionCache.ContainsKey(Name))
            {
                this.ActionCache[Name] = new Action(this, Name);
            }

            return this.ActionCache[Name];
        }

        private Dictionary<String, RelationshipType> RelationshipTypesCache;

        public IEnumerable<RelationshipType> RelationshipTypes
        {
            get
            {
                return this.RelationshipTypesCache.Values;
            }
        }

        public RelationshipType RelationshipType(String Name)
        {
            return this.RelationshipTypesCache[Name];
        }

        internal void AddRelationshipType(RelationshipType RelationshipType)
        {
            this.RelationshipTypesCache[RelationshipType.Name] = RelationshipType;
        }

        private Dictionary<String, PropertyType> _propertyTypesCache;
        private Dictionary<String, PropertyType> PropertyTypesCache
        {
            get
            {
                if (this._propertyTypesCache == null)
                {
                    this._propertyTypesCache = new Dictionary<String, PropertyType>();

                    IO.Item itemtype = new IO.Item("ItemType", "get");
                    itemtype.Select = "id";
                    itemtype.SetProperty("name", this.Name);
                    IO.Item properties = new IO.Item("Property", "get");
                    properties.Select = "name,label,data_type,data_source(name),stored_length";
                    itemtype.AddRelationship(properties);

                    IO.SOAPRequest proprequest = new IO.SOAPRequest(IO.SOAPOperation.ApplyItem, this.Session, itemtype);
                    IO.SOAPResponse propresponse = proprequest.Execute();

                    if (propresponse.IsError)
                    {
                        throw new Exceptions.ServerException(propresponse.ErrorMessage);
                    }

                    foreach (IO.Item property in propresponse.Items.First().Relationships)
                    {
                        String name = property.GetProperty("name");
                        String label = property.GetProperty("label");

                        if ((name == "id") || (name == "config_id"))
                        {
                            this._propertyTypesCache[name] = new PropertyTypes.String(this, name, label, true, 32);
                        }
                        else
                        {
                            Boolean ReadOnly = ReadOnlyProperties.Contains(name);

                            switch (property.GetProperty("data_type"))
                            {
                                case "string":
                                    int length = Int32.Parse(property.GetProperty("stored_length"));
                                    this._propertyTypesCache[name] = new PropertyTypes.String(this, name, label, ReadOnly, length);
                                    break;
                                case "ml_string":
                                    int mllength = Int32.Parse(property.GetProperty("stored_length"));
                                    this._propertyTypesCache[name] = new PropertyTypes.MultilingualString(this, name, label, ReadOnly, mllength);
                                    break;
                                case "text":
                                    this._propertyTypesCache[name] = new PropertyTypes.Text(this, name, label, ReadOnly);
                                    break;
                                case "item":
                                    IO.Item relateditemtype = property.GetPropertyItem("data_source");

                                    if (relateditemtype == null)
                                    {
                                        this._propertyTypesCache[name] = new PropertyTypes.Item(this, name, label, ReadOnly, null);
                                    }
                                    else
                                    {
                                        this._propertyTypesCache[name] = new PropertyTypes.Item(this, name, label, ReadOnly, this.Session.AnyItemType(property.GetPropertyItem("data_source").GetProperty("name")));
                                    }

                                    break;
                                case "date":
                                    this._propertyTypesCache[name] = new PropertyTypes.Date(this, name, label, ReadOnly);
                                    break;
                                case "integer":
                                    this._propertyTypesCache[name] = new PropertyTypes.Integer(this, name, label, ReadOnly);
                                    break;
                                case "boolean":
                                    this._propertyTypesCache[name] = new PropertyTypes.Boolean(this, name, label, ReadOnly);
                                    break;
                                case "list":
                                    this._propertyTypesCache[name] = new PropertyTypes.List(this, name, label, ReadOnly, this.Session.ListFromCache(property.GetProperty("data_source")));
                                    break;
                                case "image":
                                    this._propertyTypesCache[name] = new PropertyTypes.Image(this, name, label, ReadOnly);
                                    break;
                                case "float":
                                    this._propertyTypesCache[name] = new PropertyTypes.Float(this, name, label, ReadOnly);
                                    break;
                                case "federated":
                                    this._propertyTypesCache[name] = new PropertyTypes.Federated(this, name, label, ReadOnly);
                                    break;
                                case "sequence":
                                    this._propertyTypesCache[name] = new PropertyTypes.Sequence(this, name, label, true);
                                    break;
                                case "formatted text":
                                    this._propertyTypesCache[name] = new PropertyTypes.FormattedText(this, name, label, ReadOnly);
                                    break;
                                case "filter list":
                                    this._propertyTypesCache[name] = new PropertyTypes.FilterList(this, name, label, ReadOnly);
                                    break;
                                case "color":
                                    this._propertyTypesCache[name] = new PropertyTypes.Color(this, name, label, ReadOnly);
                                    break;
                                case "md5":
                                    this._propertyTypesCache[name] = new PropertyTypes.MD5(this, name, label, ReadOnly);
                                    break;
                                case "foreign":
                                    this._propertyTypesCache[name] = new PropertyTypes.Foreign(this, name, label, ReadOnly);
                                    break;
                                case "decimal":
                                    this._propertyTypesCache[name] = new PropertyTypes.Decimal(this, name, label, ReadOnly);
                                    break;
                                case "color list":
                                    this._propertyTypesCache[name] = new PropertyTypes.ColorList(this, name, label, ReadOnly);
                                    break;
                                default:
                                    throw new ArgumentException("Property Data Type not implemented: " + property.GetProperty("data_type"));
                            }
                        }
                    }

                }

                return this._propertyTypesCache;
            }
        }

        public IEnumerable<PropertyType> PropertyTypes
        {
            get
            {
                return this.PropertyTypesCache.Values;
            }
        }

        public PropertyType PropertyType(String Name)
        {
            if (this.PropertyTypesCache.ContainsKey(Name))
            {
                return this.PropertyTypesCache[Name];
            }
            else
            {
                return null;
            }
        }

        public override string ToString()
        {
            return this.Name;
        }

        internal ItemType(Session Session, String Name, String ClassStructure)
            : base()
        {
            this.RelationshipTypesCache = new Dictionary<String, RelationshipType>();
            this.Session = Session;
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
