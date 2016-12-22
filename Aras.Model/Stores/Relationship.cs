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

namespace Aras.Model.Stores
{
    public class Relationship : Store<Model.Relationship>
    {
        public Model.Item Source { get; private set; }
 
        public RelationshipType RelationshipType
        {
            get
            {
                return (RelationshipType)this.ItemType;
            }
        }

        internal override String Select
        {
            get
            {
                if (System.String.IsNullOrEmpty(this.ItemType.Select))
                {
                    return String.Join(",", this.RelationshipType.SystemProperties);
                }
                else
                {
                    return String.Join(",", this.RelationshipType.SystemProperties) + "," + this.ItemType.Select;
                }
            }
        }

        public override Model.Relationship Get(String ID)
        {
            Model.Relationship relationship = null;

            if (!this.InItemsCache(ID))
            {
                IO.Item dbitem = new IO.Item(this.ItemType.Name, "get");
                dbitem.ID = ID;
                dbitem.Select = this.Select;
                IO.SOAPRequest request = this.Session.IO.Request(IO.SOAPOperation.ApplyItem, dbitem);
                IO.SOAPResponse response = request.Execute();

                if (!response.IsError)
                {
                    if (response.Items.Count() > 0)
                    {
                        // Get Related Item
                        relationship = (Model.Relationship)this.RelationshipType.Class.GetConstructor(new Type[] { typeof(RelationshipType), typeof(Model.Item), typeof(IO.Item) }).Invoke(new object[] { this.RelationshipType, this.Source, response.Items.First() });
                        this.AddToItemsCache(relationship);
                    }
                    else
                    {
                        throw new Exceptions.ArgumentException("Invalid Relationship ID: " + ID);
                    }
                }
                else
                {
                    throw new Exceptions.ServerException(response);
                }
            }
            else
            {
                relationship = this.GetFromItemsCache(ID);
            }

            return relationship;
        }

        internal override Model.Relationship Get(IO.Item DBItem)
        {
            if (DBItem.ItemType.Equals(this.ItemType.Name))
            {
                Model.Relationship relationship = null;

                if (!this.InItemsCache(DBItem.ID))
                {
                    // Create Relationship and add to Cache
                    relationship = (Model.Relationship)this.RelationshipType.Class.GetConstructor(new Type[] { typeof(RelationshipType), typeof(Model.Item), typeof(IO.Item) }).Invoke(new object[] { this.RelationshipType, this.Source, DBItem });
                    this.AddToItemsCache(relationship);
                }
                else
                {
                    // Get Relationship from Cache and update Properties from Database Item
                    relationship = this.GetFromItemsCache(DBItem.ID);
                    relationship.UpdateProperties(DBItem);
                }

                return relationship;
            }
            else
            {
                throw new ArgumentException("Invalid ItemType");
            }
        }

        public Model.Relationship Create(Model.Item Related, Transaction Transaction)
        {
            Model.Relationship relationship = (Model.Relationship)this.RelationshipType.Class.GetConstructor(new Type[] { typeof(RelationshipType), typeof(Transaction), typeof(Model.Item), typeof(Model.Item) }).Invoke(new object[] { RelationshipType, Transaction, this.Source, Related });
            this.AddToItemsCache(relationship);
            this.AddToCreatedCache(relationship);
            return relationship;
        }

        public Queries.Relationship Query(Condition Condition)
        {
            return new Queries.Relationship(this, Condition);
        }

        public void UpdateRelated(IEnumerable<Model.Item> RelatedItems, Transaction Transaction)
        {
            // Ensure can update Source
            this.Source.Update(Transaction);

            // Ensure all Related Items are in Store
            foreach (Model.Item related in RelatedItems)
            {
                Boolean found = false;

                foreach (Model.Relationship relationship in this.CurrentItems())
                {
                    if (related.Equals(relationship.Related))
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    this.Create(related, Transaction);
                }
            }

            // Remove any Related Items no longer needed
            List<Model.Relationship> toberemoved = new List<Model.Relationship>();

            foreach(Model.Relationship relationship in this.CurrentItems())
            {
                if (relationship.Related == null)
                {
                    toberemoved.Add(relationship);
                }
                else
                {
                    if (!RelatedItems.Contains(relationship.Related))
                    {
                        toberemoved.Add(relationship);
                    }
                }
            }

            foreach(Model.Relationship relationship in toberemoved)
            {
                relationship.Delete(Transaction);
            }
        }

        protected override void ReadAllItems()
        {
            // List to hold ID's read from Server
            List<String> ret = new List<String>();

            // Read all Item from Database
            IO.Item dbitem = new IO.Item(this.ItemType.Name, "get");
            dbitem.SetProperty("source_id", this.Source.ID);
            dbitem.Select = this.Select;
            IO.SOAPRequest request = this.Session.IO.Request(IO.SOAPOperation.ApplyItem, dbitem);
            IO.SOAPResponse response = request.Execute();

            if (!response.IsError)
            {
                foreach (IO.Item thisdbitem in response.Items)
                {
                    Model.Relationship item = null;

                    if (this.InItemsCache(thisdbitem.ID))
                    {
                        // Update Item in Cache
                        item = this.GetFromItemsCache(thisdbitem.ID);
                        item.UpdateProperties(thisdbitem);

                        // Check if in CreatedCache
                        if(this.InCreatedCache(item))
                        {
                            this.RemoveFromCreatedCache(item);
                        }
                    }
                    else
                    {
                        item = (Model.Relationship)this.RelationshipType.Class.GetConstructor(new Type[] { typeof(RelationshipType), typeof(Model.Item), typeof(IO.Item) }).Invoke(new object[] { this.RelationshipType, this.Source, thisdbitem });
                        this.AddToItemsCache(item);
                    }

                    ret.Add(item.ID);
                }
            }
            else
            {
                if (!response.ErrorMessage.Equals("No items of type " + this.ItemType.Name + " found."))
                {
                    throw new Exceptions.ServerException(response);
                }
            }

            // Replace Cache
            this.ReplaceItemsCache(ret);
        }

        internal Relationship(RelationshipType RelationshipType, Model.Item Source)
            :base(RelationshipType)
        {
            this.Source = Source;
        }
    }
}
