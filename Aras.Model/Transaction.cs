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
    public class Transaction : IDisposable
    {
        public Session Session { get; private set; }

        private Dictionary<String, Action> ActionsCache;

        internal void Add(String Name, Model.Item Item)
        {
            if (Item is Relationship)
            {
                Model.Relationship relationship = (Model.Relationship)Item;

                if (this.ActionsCache.ContainsKey(relationship.Source.ID))
                {
                    Action sourceaction = this.ActionsCache[relationship.Source.ID];

                    if (this.ActionsCache.ContainsKey(relationship.ID))
                    {
                        this.ActionsCache[relationship.ID].Name = Name;
                    }
                    else
                    {
                        Actions.Relationship relationshipaction = new Actions.Relationship(this, Name, relationship);
                        this.ActionsCache[relationship.Source.ID].AddRelationship(relationshipaction);
                        this.ActionsCache[relationship.ID] = relationshipaction;
                    }
                }
                else
                {
                    throw new Exceptions.ArgumentException("Source Item not present in Transaction");
                }
            }
            else
            {
                if (this.ActionsCache.ContainsKey(Item.ID))
                {
                    this.ActionsCache[Item.ID].Name = Name;
                }
                else
                {
                    this.ActionsCache[Item.ID] = new Actions.Item(this, Name, Item);
                }  
            }
        }

        internal Action Get(String ID)
        {
            if (this.ActionsCache.ContainsKey(ID))
            {
                return this.ActionsCache[ID];
            }
            else
            {
                return null;
            }
        }

        public Boolean Completed { get; private set; }

        public void Commit()
        {
            if (!this.Completed && this.ActionsCache.Count > 0)
            {
                foreach (Action action in this.ActionsCache.Values)
                {
                    action.Commit();
                }
            }

            this.Completed = true;
        }

        public void RollBack()
        {
            if (!this.Completed)
            {
                foreach (Action action in this.ActionsCache.Values)
                {
                    action.Rollback();
                }

                this.Completed = true;
            }
        }

        public void Dispose()
        {

        }

        internal Transaction(Session Session)
        {
            this.Session = Session;
            this.ActionsCache = new Dictionary<String, Action>();
            this.Completed = false;
        }
    }
}
