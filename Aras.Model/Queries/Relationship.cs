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

namespace Aras.Model.Queries
{
    public class Relationship : Query<Model.Relationship>
    {
        private ObservableList<Model.Relationship> CreatedRelationships;
        private ObservableList<Model.Relationship> Relationships;

        public override System.Collections.Generic.IEnumerator<Model.Relationship> GetEnumerator()
        {
            return this.Relationships.GetEnumerator();
        }

        public Model.Item Source { get; private set; }

        public Model.Item Create(Model.Item Related, Transaction Transaction)
        {
            Model.Relationship relationship = this.Type.Session.Create((RelationshipType)this.Type, this.Source, Related, Transaction);
            this.CreatedRelationships.Add(relationship);
            this.Relationships.Add(relationship);
            return relationship;
        }

        public Model.Item Create(Transaction Transaction)
        {
            return this.Create(null, Transaction);
        }

        public Model.Item Create(Model.Item Related)
        {
            return this.Create(Related, null);
        }

        public Model.Item Create()
        {
            return this.Create(null, null);
        }

        public override void Refresh()
        {
            this.Relationships.NotifyListChanged = false;

            this.Relationships.Clear();

            IO.Item item = new IO.Item(this.Type.Name, "get");

            if (System.String.IsNullOrEmpty(this.Type.Select))
            {
                item.Select = "id,related_id";
            }
            else
            {
                item.Select = "id,related_id," + this.Type.Select;
            }

            item.SetProperty("source_id", this.Source.ID);
            IO.SOAPRequest request = new IO.SOAPRequest(IO.SOAPOperation.ApplyItem, this.Type.Session, item);
            IO.SOAPResponse response = request.Execute();

            if (!response.IsError)
            {
                foreach (IO.Item dbitem in response.Items)
                {
                    Model.Item related = null;
                    
                    if (((RelationshipType)this.Type).Related != null)
                    {
                        IO.Item dbrelated = dbitem.GetPropertyItem("related_id");

                        if (dbrelated != null)
                        {
                            related = this.Type.Session.ItemFromCache(dbrelated.ID, ((RelationshipType)this.Type).Related);
                        }
                    }

                    Model.Relationship relationship = this.Type.Session.RelationshipFromCache(dbitem.ID, (RelationshipType)this.Type, this.Source, related);
                    relationship.UpdateProperties(dbitem);
                    this.Relationships.Add(relationship);
                }

                foreach(Model.Relationship relationship in this.CreatedRelationships)
                {
                    this.Relationships.Add(relationship);
                }

                this.Relationships.NotifyListChanged = true;
            }
            else
            {
                this.Relationships.NotifyListChanged = true;

                if (!response.ErrorMessage.Equals("No items of type " + this.Type.Name + " found."))
                {
                    throw new Exceptions.ServerException(response);
                }
            }
        }

        void Relationships_ListChanged(object sender, EventArgs e)
        {
            this.OnQueryChanged();
        }

        internal Relationship(RelationshipType Type, Condition Condition, Model.Item Source)
            : base(Type, Condition)
        {
            this.Relationships = new ObservableList<Model.Relationship>();
            this.Relationships.ListChanged += Relationships_ListChanged;
            this.CreatedRelationships = new ObservableList<Model.Relationship>();
            this.Source = Source;
            this.Refresh();
        }

        internal Relationship(RelationshipType Type, Model.Item Source)
            :this(Type, null, Source)
        {

        }
    }
}
