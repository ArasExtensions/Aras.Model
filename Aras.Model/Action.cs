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
    internal abstract class Action
    {
        internal Transaction Transaction { get; private set; }

        private String _name;
        internal String Name 
        { 
            get
            {
                return this._name;
            }
            set
            {
                if (value != null)
                {
                    if ((this._name != null) && (this._name.Equals("add")) && value.Equals("delete"))
                    {
                        // No Action needed on database
                        this._name = null;
                    }
                    else
                    {
                        this._name = value;
                    }
                }
                else
                {
                    throw new Exceptions.ArgumentException("Must specify Name of Action");
                }
            }
        }

        protected Item Item { get; set; }

        private Dictionary<String, Actions.Relationship> RelationshipsCache;

        internal void AddRelationship(Actions.Relationship Relationship)
        {
            this.RelationshipsCache[Relationship.Item.ID] = Relationship;
        }

        internal void RemoveRelationship(String ID)
        {
            this.RelationshipsCache.Remove(ID);
        }

        protected IEnumerable<Actions.Relationship> Relationships
        {
            get
            {
                return this.RelationshipsCache.Values;
            }
        }

        protected Boolean Completed;

        internal abstract void Rollback();

        internal abstract IO.Item Commit();

        internal abstract void UpdateStore();

        protected IO.Item BuildItem()
        {
            // Create IO Item
            IO.Item dbitem = new IO.Item(this.Item.ItemType.Name, this.Name);
            dbitem.ID = this.Item.ID;

            // Add Classification

            if (this.Item.Class == null)
            {
                dbitem.SetProperty("classification", null);
            }
            else
            {
                dbitem.SetProperty("classification", this.Item.Class.Fullname);
            }

            // Add Properties
            foreach (Property prop in this.Item.Properties)
            {
                if ((!this.Transaction.Session.OptimiseAML || prop.Modified) && !prop.Type.ReadOnly && !prop.Type.Name.Equals("source_id"))
                {
                    if (prop is Properties.Item)
                    {
                        // Check that related Item is processed
                        Properties.Item itemprop = (Properties.Item)prop;

                        if (itemprop.Value != null)
                        {
                            Action itempropaction = this.Transaction.Get(((Model.Item)itemprop.Value).ID);

                            if (itempropaction != null)
                            {
                                itempropaction.Commit();
                            }
                        }
                    }

                    dbitem.SetProperty(prop.Type.Name, prop.DBValue);
                }
            }

            // Add Relations
            foreach (Actions.Relationship relationshipaction in this.RelationshipsCache.Values)
            {
                IO.Item dbrelationship = relationshipaction.Commit();
                dbitem.AddRelationship(dbrelationship);
            }

            return dbitem;
        }

        protected IO.Response SendItem(IO.Item DBItem)
        {
            // Send to Server
            IO.Request request = this.Transaction.Session.IO.Request(IO.Request.Operations.ApplyItem, DBItem);
            return request.Execute();
        }

        internal void CheckLock(Boolean UnLock)
        {
            if (this.Item.Action != Model.Item.Actions.Delete)
            {
                if (UnLock)
                {
                    // Unlock
                    this.Item.UnLock();
                }
                else
                {
                    // Ensure Locked
                    this.Item.Update(this.Transaction);
                }
            }
        }

        protected void UpdateItem(IO.Item DBItem)
        {
            // Update Properties
            this.Item.UpdateProperties(DBItem);

            foreach (Actions.Relationship relation in this.RelationshipsCache.Values)
            {
                Boolean found = false;

                foreach (IO.Item dbrelationship in DBItem.Relationships)
                {
                    if (dbrelationship.ID.Equals(relation.Item.ID))
                    {
                        relation.Item.UpdateProperties(dbrelationship);
                        found = true;
                    }
                }

                if (!found)
                {
                    relation.Item.UpdateProperties(null);
                }
            }
        }

        public override string ToString()
        {
            return this.Name + " " + this.Item.ID;
        }

        internal Action(Transaction Transaction, String Name, Item Item)
        {
            this.RelationshipsCache = new Dictionary<string, Actions.Relationship>();
            this.Transaction = Transaction;
            this.Name = Name;
            this.Item = Item;
            this.Completed = false;
        }
    }
}
