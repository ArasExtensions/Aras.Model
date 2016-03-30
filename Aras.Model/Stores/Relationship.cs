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
        public RelationshipType RelationshipType
        {
            get
            {
                return (RelationshipType)this.ItemType;
            }
        }

        public Model.Item Source { get; private set; }

        internal override String Select
        {
            get
            {
                if (System.String.IsNullOrEmpty(this.ItemType.Select))
                {
                    return "id,related_id,source_id";
                }
                else
                {
                    return "id,related_id,source_id," + this.ItemType.Select;
                }
            }
        }

        protected override void Load()
        {
            if (!this.Source.IsNew)
            {
                // Load all Relaitonships into Cache
                IO.Item item = new IO.Item(this.ItemType.Name, "get");
                item.Select = this.Select;
                item.SetProperty("source_id", this.Source.ID);
                IO.SOAPRequest request = new IO.SOAPRequest(IO.SOAPOperation.ApplyItem, this.Session, item);
                IO.SOAPResponse response = request.Execute();

                if (!response.IsError)
                {
                    foreach (IO.Item dbitem in response.Items)
                    {
                        if (!this.ItemInCache(dbitem.ID))
                        {
                            this.AddItemToCache((Model.Relationship)this.RelationshipType.Class.GetConstructor(new Type[] { typeof(RelationshipType), typeof(Model.Item), typeof(IO.Item) }).Invoke(new object[] { this.RelationshipType, this.Source, dbitem }));
                        }
                        else
                        {
                            this.GetItemFromCache(dbitem.ID).UpdateProperties(dbitem);
                        }
                    }
                }
                else
                {
                    if (!response.ErrorMessage.Equals("No items of type " + this.RelationshipType.Name + " found."))
                    {
                        throw new Exceptions.ServerException(response);
                    }
                }
            }
        }

        internal Model.Item GetByDBItem(IO.Item Item)
        {
            if (Item.ItemType.Equals(this.ItemType.Name))
            {
                if (!this.ItemInCache(Item.ID))
                {
                    this.AddItemToCache((Model.Relationship)this.RelationshipType.Class.GetConstructor(new Type[] { typeof(RelationshipType), typeof(Model.Item), typeof(IO.Item) }).Invoke(new object[] { this.RelationshipType, this.Source, Item }));
                }

                return this.GetItemFromCache(Item.ID);
            }
            else
            {
                throw new ArgumentException("Invalid ItemType");
            }
        }

        public override Model.Relationship Get(String ID)
        {
            if (!this.ItemInCache(ID))
            {
                IO.Item dbitem = new IO.Item(this.ItemType.Name, "get");
                dbitem.ID = ID;
                dbitem.Select = this.Select;
                IO.SOAPRequest request = new IO.SOAPRequest(IO.SOAPOperation.ApplyItem, this.Session, dbitem);
                IO.SOAPResponse response = request.Execute();

                if (!response.IsError)
                {
                    if (response.Items.Count() > 0)
                    {
                        // Get Related Item
                        this.AddItemToCache((Model.Relationship)this.RelationshipType.Class.GetConstructor(new Type[] { typeof(RelationshipType), typeof(Model.Item), typeof(IO.Item) }).Invoke(new object[] { this.RelationshipType, this.Source, response.Items.First() }));
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

            return this.GetItemFromCache(ID);
        }

        public override Model.Relationship Create()
        {
            return this.Create(null, null);
        }

        public override Model.Relationship Create(Transaction Transaction)
        {
            return this.Create(null, Transaction);
        }

        public Model.Relationship Create(Model.Item Related)
        {
            return this.Create(Related, null);
        }

        public Model.Relationship Create(Model.Item Related, Transaction Transaction)
        {
            Model.Relationship relationship = (Model.Relationship)this.RelationshipType.Class.GetConstructor(new Type[] { typeof(RelationshipType), typeof(Model.Item), typeof(Model.Item) }).Invoke(new object[] { RelationshipType, this.Source, Related });
            this.AddItemToCache(relationship);

            if (Transaction != null)
            {
                Transaction.Add("add", relationship);
            }

            return relationship;
        }

        public override Query<Model.Relationship> Query(Condition Condition)
        {
            return new Queries.Relationship(this, Condition);
        }

        internal Relationship(RelationshipType Type, Model.Item Source)
            :base(Type)
        {
            this.Source = Source;
        }
    }
}
