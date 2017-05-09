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
using System.ComponentModel;

namespace Aras.Model
{
    public class SupercededEventArgs : EventArgs
    {
        public Item NewGeneration { get; private set; }

        public SupercededEventArgs(Item NewGeneration)
            : base()
        {
            this.NewGeneration = NewGeneration;
        }
    }

    public delegate void SupercededEventHandler(object sender, SupercededEventArgs e);

    public class DeletedEventArgs : EventArgs
    {
        public DeletedEventArgs()
            : base()
        {
        }
    }

    public delegate void DeletedEventHandler(object sender, DeletedEventArgs e);

    public class Item : IEquatable<Item>, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(String Name)
        {
            if (this.PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(Name));
            }
        }

        public event SupercededEventHandler Superceded;

        internal void OnSuperceded(Item NewGeneration)
        {
            if (this.Superceded != null)
            {
                Superceded(this, new SupercededEventArgs(NewGeneration));
            }
        }

        public event DeletedEventHandler Deleted;

        internal void OnDeleted()
        {
            if (this.Deleted != null)
            {
                Deleted(this, new DeletedEventArgs());
            }
        }

        public enum States { New, Stored, Deleted };

        public enum Actions { Create, Read, Update, Delete };

        public enum Locks { None, User, OtherUser };

        internal Cache.Item Cache { get; private set; }

        public Store Store { get; private set; }

        public ItemType ItemType
        {
            get
            {
                return this.Cache.ItemType;
            }
        }

        public String ID
        {
            get
            {
                return this.Cache.ID;
            }
        }

        public String ConfigID
        {
            get
            {
                return this.Cache.ConfigID;
            }
        }

        public Int32 Generation
        {
            get
            {
                return this.Cache.Generation;
            }
        }

        public Boolean IsCurrent
        {
            get
            {
                return this.Cache.IsCurrent;
            }
        }

        public States State
        {
            get
            {
                return this.Cache.State;
            }
        }

        public Actions Action
        {
            get
            {
                return this.Cache.Action;
            }
        }

        public Class Class
        {
            get
            {
                return this.Cache.Class;
            }
            set
            {
                this.Cache.Class = value;
            }
        }

        public Items.LifeCycleMap LifeCycleMap
        {
            get
            {
                return this.Cache.LifeCycleMap;
            }
        }

        public String KeyedName
        {
            get
            {
                return (String)this.Property("keyed_name").Value;
            }
        }

        public Items.User CreatedBy
        {
            get
            {
                return (Items.User)this.Property("created_by_id").Value;
            }
        }

        public DateTime CreatedOn
        {
            get
            {
                return (DateTime)this.Property("created_on").Value;
            }
        }

        public String MajorRev
        {
            get
            {
                return (String)this.Property("major_rev").Value;
            }
        }

        public Items.Identity ManagedBy
        {
            get
            {
                return (Items.Identity)this.Property("managed_by_id").Value;
            }
        }

        public String MinorRev
        {
            get
            {
                return (String)this.Property("minor_rev").Value;
            }
        }

        public Items.User ModifiedBy
        {
            get
            {
                return (Items.User)this.Property("modified_by_id").Value;
            }
        }

        public DateTime ModifiedOn
        {
            get
            {
                return (DateTime)this.Property("modified_on").Value;
            }
        }

        public Items.Identity OwnedBy
        {
            get
            {
                return (Items.Identity)this.Property("owned_by_id").Value;
            }
        }

        public Locks Locked
        {
            get
            {
                return this.Cache.Locked;
            }
        }

        internal void UnLock()
        {
            this.Cache.UnLock();
        }

        public Boolean CanUpdate
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public void Update(Transaction Transaction)
        {
            this.Cache.Update(this, Transaction);
            this.OnUpdate(Transaction);
        }

        protected virtual void OnUpdate(Transaction Transaction)
        {

        }

        public void Delete(Transaction Transaction)
        {
            this.Cache.Delete(this, Transaction);
        }

        private Dictionary<PropertyType, Property> PropertyCache;

        public Property Property(PropertyType PropertyType)
        {
            if (this.PropertyCache.ContainsKey(PropertyType))
            {
                return this.PropertyCache[PropertyType];
            }
            else
            {
                throw new Exceptions.ArgumentException("Invalid PropertyType: " + PropertyType.ToString());
            }
        }

        public Boolean HasProperty(String PropertyName)
        {
            PropertyType proptype = this.ItemType.PropertyType(PropertyName);
            return this.PropertyCache.ContainsKey(proptype);
        }

        public Boolean HasProperty(PropertyType PropertyType)
        {
            return this.PropertyCache.ContainsKey(PropertyType);
        }

        public Property Property(String PropertyName)
        {
            PropertyType proptype = this.Store.Query.PropertyType(PropertyName);
            return this.Property(proptype);
        }

        public IEnumerable<Property> Properties
        {
            get
            {
                return this.PropertyCache.Values;
            }
        }

        private Dictionary<RelationshipType, Store> RelationshipsCache;

        public IEnumerable<RelationshipType> RelationshipTypes
        {
            get
            {
                return this.RelationshipsCache.Keys;
            }
        }

        public Store Relationships(RelationshipType RelationshipType)
        {
            if (this.RelationshipsCache.ContainsKey(RelationshipType))
            {
                return this.RelationshipsCache[RelationshipType];
            }
            else
            {
                throw new Exceptions.ArgumentException("Invalid Relationship Type");
            }
        }

        public Store Relationships(String RelationshipTypeName)
        {
            RelationshipType reltype = (RelationshipType)this.Store.Session.ItemType(RelationshipTypeName);
            return this.Relationships(reltype);
        }

        private void Initalise()
        {
            // Watch for Changes in Cache
            this.Cache.PropertyChanged += ItemCache_PropertyChanged;

            this.PropertyCache = new Dictionary<PropertyType, Property>();

            foreach (PropertyType proptype in this.Store.Query.PropertyTypes)
            {
                switch (proptype.GetType().Name)
                {
                    case "String":
                        this.PropertyCache[proptype] = new Properties.String(this, (PropertyTypes.String)proptype);
                        break;
                    case "Federated":
                        this.PropertyCache[proptype] = new Properties.Federated(this, (PropertyTypes.Federated)proptype);
                        break;
                    case "MultilingualString":
                        this.PropertyCache[proptype] = new Properties.MultilingualString(this, (PropertyTypes.MultilingualString)proptype);
                        break;
                    case "Text":
                        this.PropertyCache[proptype] = new Properties.Text(this, (PropertyTypes.Text)proptype);
                        break;
                    case "FormattedText":
                        this.PropertyCache[proptype] = new Properties.FormattedText(this, (PropertyTypes.FormattedText)proptype);
                        break;
                    case "Integer":
                        this.PropertyCache[proptype] = new Properties.Integer(this, (PropertyTypes.Integer)proptype);
                        break;
                    case "Item":
                        this.PropertyCache[proptype] = new Properties.Item(this, (PropertyTypes.Item)proptype);
                        break;
                    case "Date":
                        this.PropertyCache[proptype] = new Properties.Date(this, (PropertyTypes.Date)proptype);
                        break;
                    case "List":
                        this.PropertyCache[proptype] = new Properties.List(this, (PropertyTypes.List)proptype);
                        break;
                    case "Decimal":
                        this.PropertyCache[proptype] = new Properties.Decimal(this, (PropertyTypes.Decimal)proptype);
                        break;
                    case "Float":
                        this.PropertyCache[proptype] = new Properties.Float(this, (PropertyTypes.Float)proptype);
                        break;
                    case "Boolean":
                        this.PropertyCache[proptype] = new Properties.Boolean(this, (PropertyTypes.Boolean)proptype);
                        break;
                    case "Image":
                        this.PropertyCache[proptype] = new Properties.Image(this, (PropertyTypes.Image)proptype);
                        break;
                    case "Sequence":
                        this.PropertyCache[proptype] = new Properties.Sequence(this, (PropertyTypes.Sequence)proptype);
                        break;
                    case "Foreign":
                        this.PropertyCache[proptype] = new Properties.Foreign(this, (PropertyTypes.Foreign)proptype);
                        break;
                    default:
                        throw new NotImplementedException("Property Type not implmented: " + proptype.GetType().Name);
                }
            }

            this.RelationshipsCache = new Dictionary<RelationshipType, Store>();

            foreach(Query relquery in this.Store.Query.Relationships)
            {
                this.RelationshipsCache[(RelationshipType)relquery.ItemType] = new Store(relquery, this);
            }
        }

        private void ItemCache_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.OnPropertyChanged(e.PropertyName);
        }

        internal void UpdateProperties(IO.Item DBItem)
        {
            if (DBItem != null)
            {
                // Update Classification
                this.Class = this.ItemType.GetClassFullname(DBItem.GetProperty("classification"));

                if (this.ID.Equals(DBItem.ID))
                {
                    foreach (Property property in this.PropertyCache.Values)
                    {
                        if (property is Properties.Item)
                        {
                            if (property.Type.Name.Equals("source_id"))
                            {
                                ((Properties.Item)property).SetDBValue(this.Store.Source);
                            }
                            else
                            {
                                IO.Item dbpropitem = DBItem.GetPropertyItem(property.Type.Name);

                                if (dbpropitem != null)
                                {
                                    property.DBValue = this.Store.Query.Property((PropertyTypes.Item)property.Type).Store.Create(dbpropitem).ID;
                                }
                                else
                                {
                                    property.DBValue = null;
                                }
                            }
                        }
                        else
                        {
                            property.DBValue = DBItem.GetProperty(property.Type.Name);
                        }
                    }

                    Dictionary<RelationshipType, List<IO.Item>> dbrels = new Dictionary<RelationshipType, List<IO.Item>>();

                    foreach (IO.Item dbrel in DBItem.Relationships)
                    {
                        ItemType reltype = this.Store.Session.ItemType(dbrel.ItemType);

                        if (reltype is RelationshipType)
                        {
                            if (!dbrels.ContainsKey((RelationshipType)reltype))
                            {
                                dbrels[(RelationshipType)reltype] = new List<IO.Item>();
                            }

                            dbrels[(RelationshipType)reltype].Add(dbrel);
                        }
                    }

                    foreach (RelationshipType reltype in dbrels.Keys)
                    {
                        this.RelationshipsCache[reltype].Load(dbrels[reltype]);
                    }
                }
                else
                {
                    throw new Exceptions.ArgumentException("Invalid Item ID: " + DBItem.ID);
                }
            }
        }

        public IEnumerable<Relationships.LifeCycleState> NextStates()
        {
            return this.Cache.NextStates();
        }

        public void Promote(Relationships.LifeCycleState NewState)
        {
            this.Cache.Promote(NewState);
        }

        public Boolean Equals(Item other)
        {
            if (other == null)
            {
                return false;
            }
            else
            {
                return this.ID.Equals(other.ID);
            }
        }

        public override Boolean Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            else
            {
                if (obj is Item)
                {
                    return this.ID.Equals(((Item)obj).ID);
                }
                else
                {
                    return false;
                }
            }
        }

        public override int GetHashCode()
        {
            return this.ID.GetHashCode();
        }

        public Item(Store Store, Transaction Transaction)
        {
            this.Cache = Store.Session.GetItemCache(Store.ItemType);
            this.Store = Store;
            this.Initalise();
        }

        public Item(Store Store, IO.Item DBItem)
        {
            this.Cache = Store.Session.GetItemCache(Store.ItemType, DBItem.ID, DBItem.ConfigID, DBItem.Generation, DBItem.IsCurrent);
            this.Store = Store;
            this.Initalise();
            this.UpdateProperties(DBItem);
        }
    }
}
