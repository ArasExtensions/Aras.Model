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
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Net;
using System.IO;

namespace Aras.Model.Requests
{
    public class Item
    {
        public Request Request { get; private set; }

        public Cache.Item Cache { get; private set; }

        public Action Action { get; private set; }

        public Session Session
        {
            get
            {
                return this.Request.Session;
            }
        }

        public ItemType ItemType
        {
            get
            {
                return this.Cache.ItemType;
            }
        }

        public Boolean Paging { get; set; }

        public int Page { get; set; }

        public int PageSize { get; set; }

        private Dictionary<String, PropertyType> _selection;
        public IEnumerable<PropertyType> Selection
        {
            get
            {
                return this._selection.Values;
            }
        }

        public void AddSelection(PropertyType PropertyType)
        {
            if (this.ItemType.Equals(PropertyType.ItemType))
            {
                this._selection[PropertyType.Name] = PropertyType;
            }
            else
            {
                throw new Exceptions.ArgumentException("PropertyType not associated with Request ItemType");
            }
        }

        public void AddSelection(String Name)
        {
            String[] names = Name.Split(',');

            foreach (String name in names)
            {
                PropertyType proptype = this.ItemType.PropertyType(name);

                if (proptype != null)
                {
                    this.AddSelection(proptype);
                }
                else
                {
                    throw new Exceptions.ArgumentException("PropertyType does not exist");
                }
            }
        }

        internal virtual String SelectionString
        {
            get
            {
                PropertyType[] proptypes = this._selection.Values.ToArray();
                StringBuilder ret = new StringBuilder(proptypes[0].Name);

                for (int i = 1; i < proptypes.Count(); i++)
                {
                    ret.Append(',');
                    ret.Append(proptypes[i].Name);
                }

                return ret.ToString();
            }
        }

        public Condition Condition { get; private set; }

        private List<Relationship> _relationships;
        public IEnumerable<Relationship> Relationships
        {
            get
            {
                return this._relationships;
            }
        }

        public IEnumerable<RelationshipType> RelationshipTypes
        {
            get
            {
                return this.ItemType.RelationshipTypes;
            }
        }

        public Relationship AddRelationship(Action Action, Item Related)
        {
            if (Action.ItemType is RelationshipType)
            {
                RelationshipType relationshiptype = (RelationshipType)Action.ItemType;

                if (relationshiptype.SourceType.Equals(this.ItemType))
                {
                    if ((Related != null) && (relationshiptype.RelatedType != null))
                    {
                        if (!Related.ItemType.Equals(relationshiptype.RelatedType))
                        {
                            throw new Exceptions.ArgumentException("Related must have ItemType: " + relationshiptype.RelatedType.Name);
                        }
                    }

                    Relationship relationship = new Relationship(this.Request, new Model.Cache.Relationship(this.Cache, relationshiptype), Action, this, Related);
                    this._relationships.Add(relationship);
                    return relationship;
                }
                else
                {
                    throw new Exceptions.ArgumentException("Source Type of RelationshipType must be: " + this.ItemType.Name);
                }
            }
            else
            {
                throw new Exceptions.ArgumentException("Action must be from a RelationshipType");
            }
        }

        public Relationship AddRelationship(Action Action)
        {
            return this.AddRelationship(Action, null);
        }

        public Relationship AddRelationship(String RelationshipType, String Action, Item Related)
        {
            RelationshipType relationshiptype = this.ItemType.RelationshipType(RelationshipType);

            if (relationshiptype != null)
            {
                Action action = relationshiptype.Action(Action);

                if (action != null)
                {
                    return this.AddRelationship(action, Related);
                }
                else
                {
                    throw new Exceptions.ArgumentException("Invalid Action");
                }
            }
            else
            {
                throw new Exceptions.ArgumentException("Invalid RelationshipType");
            }
        }

        public Relationship AddRelationship(String RelationshipType, String Action)
        {
            return this.AddRelationship(RelationshipType, Action, null);
        }

        internal IO.Item BuildRequest()
        {
            IO.Item ret = new IO.Item(this.ItemType.Name, this.Action.Name);
            ret.Select = this.SelectionString;

            // Add Properties
            foreach (Cache.Property prop in this.Cache.Properties)
            {
                if (!prop.ReadOnly)
                {
                    ret.SetProperty(prop.Name, prop.ValueString);
                }
            }

            // Where
            if (this.Cache.ID != null)
            {
                ret.ID = this.Cache.ID;
            }
            else
            {
                if (this.Condition.WhereClause != null)
                {
                    ret.Where = this.Condition.WhereClause;
                }
            }

            // Paging
            if (this.Paging)
            {
                ret.Page = this.Page;
                ret.PageSize = this.PageSize;
            }

            // Relationships
            foreach (Requests.Relationship relationship in this.Relationships)
            {
                IO.Item relitem = relationship.BuildRequest();
                ret.AddRelationship(relitem);
            }

            return ret;
        }

        public Response Execute()
        {
            return this.Request.Execute();
        }

        public async Task<Response> ExecuteAsync()
        {
            return await this.Request.ExecuteAsync();
        }

        internal Item(Request Request, Cache.Item Cache, Action Action)
        {
            this._selection = new Dictionary<String, PropertyType>();
            this._relationships = new List<Relationship>();

            this.Request = Request;
            this.Cache = Cache;
            this.Action = Action;
            this.Condition = new Conditions.Base(this);
            
            this.AddSelection("id");
            this.Paging = false;
            this.Page = 1;
            this.PageSize = 25;
        }
    }
}
