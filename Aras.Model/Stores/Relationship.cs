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
    public class Relationship<T> : Store<T> where T : Model.Relationship
    {
        public Caches.Relationship Cache { get; private set; }

        public override ItemType ItemType
        {
            get
            {
                return this.Cache.ItemType;
            }
        }

        public Model.Item Source
        {
            get
            {
                return ((Caches.Relationship)this.Cache).Source;
            }
        }

        public RelationshipType RelationshipType
        {
            get
            {
                return (RelationshipType)this.ItemType;
            }
        }

        private Dictionary<Model.Item, T> FirstByRelatedCache;
        public T FirstByRelated(Model.Item Related)
        {
            if (!this.FirstByRelatedCache.ContainsKey(Related))
            {
                this.FirstByRelatedCache[Related] = null;
            }

            if (this.FirstByRelatedCache[Related] == null)
            {
                foreach (T relationship in this.Items)
                {
                    if (relationship.Related.Equals(Related))
                    {
                        this.FirstByRelatedCache[Related] = relationship;
                        break;
                    }
                }
            }

            return this.FirstByRelatedCache[Related];

        }

        protected override void OnRefresh()
        {
            this.FirstByRelatedCache.Clear();
        }

        protected override List<T> Run()
        {
            IO.Item item = new IO.Item(this.ItemType.Name, "get");
            item.Select = this.Cache.Select;
            item.SetProperty("source_id", this.Source.ID);
            item.Where = this.Where;
            this.SetPaging(item);

            IO.SOAPRequest request = new IO.SOAPRequest(IO.SOAPOperation.ApplyItem, this.Cache.Session, item);
            IO.SOAPResponse response = request.Execute();

            List<T> ret = new List<T>();

            if (!response.IsError)
            {
                foreach (IO.Item dbitem in response.Items)
                {
                    T relationship = (T)this.Cache.Get(dbitem);
                    ret.Add(relationship);
                }

                this.UpdateNoPages(response);
            }
            else
            {
                if (!response.ErrorMessage.Equals("No items of type " + this.RelationshipType.Name + " found."))
                {
                    throw new Exceptions.ServerException(response);
                }
            }

            return ret;
        }

        public T Create(Model.Item Related, Transaction Transaction)
        {
            T relationship = (T)((Caches.Relationship)this.Cache).Create(Related, Transaction);
            this.NewItems.Add(relationship);
            this.Items.Add(relationship);
            this.OnStoreChanged();
            return relationship;
        }

        internal Relationship(Caches.Relationship Cache, Condition Condition)
            : base(Condition)
        {
            this.Cache = Cache;
            this.FirstByRelatedCache = new Dictionary<Item, T>();
        }

        internal Relationship(Caches.Relationship Cache)
            :this(Cache, null)
        {

        }

        public Relationship(Model.Item Source, RelationshipType RelationshipType, Condition Condition)
            :base(Condition)
        {
            this.Cache = Source.Cache(RelationshipType);
            this.FirstByRelatedCache = new Dictionary<Item, T>();
        }

        public Relationship(Model.Item Source, RelationshipType RelationshipType)
            :this(Source, RelationshipType, null)
        {

        }

        public Relationship(Model.Item Source, String Name, Condition Condition)
            :base(Condition)
        {
            this.Cache = Source.Cache(Name);
            this.FirstByRelatedCache = new Dictionary<Item, T>();
        }

        public Relationship(Model.Item Source, String Name)
            :this(Source, Name, null)
        {

        }
    }
}
