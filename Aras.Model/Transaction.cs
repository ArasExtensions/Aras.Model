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

namespace Aras.Model
{
    public class Transaction : IDisposable
    {
        public Session Session { get; private set; }

        private Dictionary<String, Action> ActionsCache;

        internal void Remove(Model.Item Item)
        {
            if (Item is Relationship)
            {
                Model.Relationship relationship = (Model.Relationship)Item;

                if (this.ActionsCache.ContainsKey(relationship.Source.ID))
                {
                    Action sourceaction = this.ActionsCache[relationship.Source.ID];
                    sourceaction.RemoveRelationship(relationship.ID);

                    if (this.ActionsCache.ContainsKey(relationship.ID))
                    {
                        this.ActionsCache.Remove(relationship.ID);
                    }
                }

            }
            else
            {
                if (this.ActionsCache.ContainsKey(Item.ID))
                {
                    this.ActionsCache.Remove(Item.ID);
                }
            }
        }

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
            else if (Item is File)
            {
                if (this.ActionsCache.ContainsKey(Item.ID))
                {
                    this.ActionsCache[Item.ID].Name = Name;
                }
                else
                {
                    this.ActionsCache[Item.ID] = new Actions.File(this, Name, Item);
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

        public void Commit(Boolean UnLock)
        {
            if (!this.Completed && this.ActionsCache.Count > 0)
            {
                // Process Actions
                foreach (Action action in this.ActionsCache.Values)
                {
                    action.Commit();
                }

                // Update Stores
                foreach (Action action in this.ActionsCache.Values)
                {
                    action.UpdateStore();
                }

                // Check Lock
                foreach (Action action in this.ActionsCache.Values)
                {
                    action.CheckLock(UnLock);
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
