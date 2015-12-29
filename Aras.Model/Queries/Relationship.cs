﻿/*  
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
    public class Relationship : Query
    {
        private List<Model.Relationship> Relationships;

        public override System.Collections.IEnumerator GetEnumerator()
        {
            return this.Relationships.GetEnumerator();
        }

        public Model.Item Source { get; private set; }

        public override void Refresh()
        {
            this.Relationships.Clear();

            IO.Item item = new IO.Item(this.Type.Name, "get");
            item.Select = "id,config_id,related_id(id,config_id)," + this.Select;
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
                            related = this.Type.Session.ItemFromCache(dbrelated.ID, dbrelated.ConfigID, ((RelationshipType)this.Type).Related);
                        }
                    }

                    Model.Relationship relationship = this.Type.Session.RelationshipFromCache(dbitem.ID, dbitem.ConfigID, (RelationshipType)this.Type, this.Source, related);
                    relationship.UpdateProperties(dbitem);
                    this.Relationships.Add(relationship);
                }
            }
            else
            {
                if (!response.ErrorMessage.Equals("No items of type " + this.Type.Name + " found."))
                {
                    throw new Exceptions.ServerException(response);
                }
            }
        }

        internal Relationship(RelationshipType Type, String Select, Model.Item Source)
            :base(Type, Select)
        {
            this.Relationships = new List<Model.Relationship>();
            this.Source = Source;
            this.Refresh();
        }
    }
}
