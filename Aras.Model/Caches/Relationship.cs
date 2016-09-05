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
using System.Reflection;

namespace Aras.Model.Caches
{
    public class Relationship : Cache<Model.Relationship>
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
                    return String.Join(",", this.RelationshipType.SystemProperties);
                }
                else
                {
                    return String.Join(",", this.RelationshipType.SystemProperties) + "," + this.ItemType.Select;
                }
            }
        }

        internal override Model.Relationship Get(IO.Item DBItem)
        {
            if (DBItem.ItemType.Equals(this.ItemType.Name))
            {
                Model.Relationship relationship = null;

                if (!this.IsInCache(DBItem.ID))
                {
                    // Create Relationship and add to Cache
                    relationship = (Model.Relationship)this.RelationshipType.Class.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(RelationshipType), typeof(Model.Item), typeof(IO.Item) }, null).Invoke(new object[] { this.RelationshipType, this.Source, DBItem });
                    this.AddToCache(relationship);
                }
                else
                {
                    // Get Relationship from Cache and update Properties from Database Item
                    relationship = this.GetFromCache(DBItem.ID);
                    relationship.UpdateProperties(DBItem);
                }

                return relationship;
            }
            else
            {
                throw new ArgumentException("Invalid ItemType");
            }
        }

        public override Model.Relationship Get(String ID)
        {
            Model.Relationship relationship = null;

            if (!this.IsInCache(ID))
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
                        relationship = (Model.Relationship)this.RelationshipType.Class.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(RelationshipType), typeof(Model.Item), typeof(IO.Item) }, null).Invoke(new object[] { this.RelationshipType, this.Source, response.Items.First() });
                        this.AddToCache(relationship);
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
                relationship = this.GetFromCache(ID);
            }

            return relationship;
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
            Model.Relationship relationship = (Model.Relationship)this.RelationshipType.Class.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(RelationshipType), typeof(Transaction), typeof(Model.Item), typeof(Model.Item) }, null).Invoke(new object[] { RelationshipType, Transaction, this.Source, Related });
            this.AddToCache(relationship);

            if (Transaction != null)
            {
                Transaction.Add("add", relationship);
            }

            return relationship;
        }

        internal Relationship(RelationshipType Type, Model.Item Source)
            :base(Type)
        {
            this.Source = Source;
        }
    }
}
