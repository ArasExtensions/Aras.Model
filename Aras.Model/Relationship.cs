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
    public class Relationship : Item
    {
        public RelationshipType RelationshipType
        {
            get
            {
                return (RelationshipType)this.ItemType;
            }
        }

        public override void Refresh()
        {
            List<String> propertynames = new List<String>();

            foreach(String sysprop in RelationshipType.SystemProperties)
            {
                propertynames.Add(sysprop);
            }

            foreach (Property property in this.Properties)
            {
                propertynames.Add(property.Type.Name);
            }

            IO.Item dbitem = new IO.Item(this.ItemType.Name, "get");
            dbitem.Select = String.Join(",", propertynames);
            dbitem.ID = this.ID;
            IO.Request request = this.Session.IO.Request(IO.Request.Operations.ApplyItem, dbitem);
            IO.Response response = request.Execute();

            if (!response.IsError)
            {
                this.UpdateProperties(response.Items.First());

                this.OnRefresh();
            }
            else
            {
                throw new Exceptions.ServerException(response);
            }
        }

        public Item Source { get; internal set; }

        private Item _related;
        public Item Related
        {
            get
            {
                return this._related;
            }
            set
            {
                if (value != null)
                {
                    this._related = value;

                    // Watch for Related Item Versioning
                    this._related.Superceded += Related_Superceded;
                }
                else
                {
                    if (this._related != null)
                    {
                        // Stop watching current Related Item
                        this._related.Superceded -= Related_Superceded;
                    }

                    this._related = null;
                }
            }
        }

        void Related_Superceded(object sender, SupercededEventArgs e)
        {
            // Stop watching current Related Item
            this._related.Superceded -= Related_Superceded;

            // Set new Related Item
            this._related = e.NewGeneration;

            // Watch for Related Item Superceded
            this._related.Superceded += Related_Superceded;
        }

        internal override void UpdateProperties(IO.Item DBItem)
        {
            base.UpdateProperties(DBItem);

            if (DBItem != null)
            {
                IO.Item dbrelated = DBItem.GetPropertyItem("related_id");

                if (dbrelated != null)
                {
                    this._related = this.ItemType.Session.Get(this.RelationshipType.RelatedItemType, dbrelated.ID);

                    // Watch for Related Item Versioning
                    this._related.Superceded += Related_Superceded;
                }
            }
        }

        public Relationship(RelationshipType RelationshipType, Transaction Transaction, Item Source, Item Related)
            : base(RelationshipType, Transaction)
        {
            this.Source = Source;
            this._related = Related;

            // Add to Transaction
            Transaction.Add("add", this);
        }

        public Relationship(RelationshipType RelationshipType, Item Source, IO.Item DBItem)
            : base(RelationshipType, DBItem)
        {
            this.Source = Source;
        }
    }
}
