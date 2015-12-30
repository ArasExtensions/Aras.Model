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
    public class Item : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(String Name)
        {
            if (this.PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(Name));
            }
        }

        public enum States { Create, Read, Update, Deleted };

        private States _status;
        public States Status 
        { 
            get
            {
                return this._status;
            }
            private set
            {
                if (this._status != value)
                {
                    this._status = value;
                    this.OnPropertyChanged("Status");
                }
            }
        }

        public String ID { get; private set; }

        public String ConfigID { get; private set; }

        public ItemType ItemType { get; private set; }

        public Class Class
        {
            get
            {
                return this.ItemType.ClassStructure.Search((String)this.Property("classification").Value);
            }
            set
            {
                if (value != null)
                {
                    if (value.ItemType.Equals(this.ItemType))
                    {
                        this.Property("classification").Value = value.Fullname;
                    }
                    else
                    {
                        throw new Exceptions.ArgumentException("Class is from another ItemType");
                    }
                }
                else
                {
                    this.Property("classification").Value = null;
                }
            }
        }

        public String KeyedName
        {
            get
            {
                return (String)this.Property("keyed_name").Value;
            }
        }

        public User CreatedBy
        {
            get
            {
                return (User)this.Property("created_by_id").Value;
            }
        }

        public DateTime CreatedOn
        {
            get
            {
                return (DateTime)this.Property("created_on").Value;
            }
        }

        public LifeCycleState CurrentState
        {
            get
            {
                return (LifeCycleState)this.Property("current_state").Value;
            }
        }

        public Int32 Generation
        {
            get
            {
                return (Int32)this.Property("generation").Value;
            }
        }

        public Boolean IsCurrent
        {
            get
            {
                return (Boolean)this.Property("is_current").Value;
            }
        }

        public Boolean IsReleased
        {
            get
            {
                return (Boolean)this.Property("is_released").Value;
            }
        }

        public User LockedBy
        {
            get
            {
                return (User)this.Property("locked_by_id").Value;
            }
        }

        public String MajorRev
        {
            get
            {
                return (String)this.Property("major_rev").Value;
            }
        }

        public Identity ManagedBy
        {
            get
            {
                return (Identity)this.Property("managed_by_id").Value;
            }
        }

        public String MinorRev
        {
            get
            {
                return (String)this.Property("minor_rev").Value;
            }
        }

        public User ModifiedBy
        {
            get
            {
                return (User)this.Property("modified_by_id").Value;
            }
        }

        public DateTime ModifiedOn
        {
            get
            {
                return (DateTime)this.Property("modified_on").Value;
            }
        }

        public Identity OwnedBy
        {
            get
            {
                return (Identity)this.Property("owned_by_id").Value;
            }
        }

        public Permission Permission
        {
            get
            {
                return (Permission)this.Property("permission_id").Value;
            }
        }

        public override string ToString()
        {
            return this.KeyedName;
        }

        public String State
        {
            get
            {
                return (String)this.Property("state").Value;
            }
        }

        public Team Team
        {
            get
            {
                return (Team)this.Property("team_id").Value;
            }
        }

        private Dictionary<PropertyType, Property> PropertyCache;

        public Property Property(PropertyType PropertyType)
        {
            if (!this.PropertyCache.ContainsKey(PropertyType))
            {
                switch (PropertyType.GetType().Name)
                {
                    case "String":
                        this.PropertyCache[PropertyType] = new Properties.String(this, (PropertyTypes.String)PropertyType);
                        break;
                    case "MultilingualString":
                        this.PropertyCache[PropertyType] = new Properties.MultilingualString(this, (PropertyTypes.MultilingualString)PropertyType);
                        break;
                    case "Text":
                        this.PropertyCache[PropertyType] = new Properties.Text(this, (PropertyTypes.Text)PropertyType);
                        break;
                    case "Integer":
                        this.PropertyCache[PropertyType] = new Properties.Integer(this, (PropertyTypes.Integer)PropertyType);
                        break;
                    case "Item":
                        this.PropertyCache[PropertyType] = new Properties.Item(this, (PropertyTypes.Item)PropertyType);
                        break;
                    case "Date":
                        this.PropertyCache[PropertyType] = new Properties.Date(this, (PropertyTypes.Date)PropertyType);
                        break;
                    case "List":
                        this.PropertyCache[PropertyType] = new Properties.List(this, (PropertyTypes.List)PropertyType);
                        break;
                    case "Decimal":
                        this.PropertyCache[PropertyType] = new Properties.Decimal(this, (PropertyTypes.Decimal)PropertyType);
                        break;
                    case "Float":
                        this.PropertyCache[PropertyType] = new Properties.Float(this, (PropertyTypes.Float)PropertyType);
                        break;
                    case "Boolean":
                        this.PropertyCache[PropertyType] = new Properties.Boolean(this, (PropertyTypes.Boolean)PropertyType);
                        break;
                    case "Image":
                        this.PropertyCache[PropertyType] = new Properties.Image(this, (PropertyTypes.Image)PropertyType);
                        break;
                    default:
                        throw new NotImplementedException("Property Type not implmented: " + PropertyType.GetType().Name);
                }
            }

            return this.PropertyCache[PropertyType];
        }

        public Property Property(String Name)
        {
            return this.Property(this.ItemType.PropertyType(Name));
        }

        public IEnumerable<Property> Properties
        {
            get
            {
                return this.PropertyCache.Values;
            }
        }

        public void Update(Transaction Transaction)
        {
            if (this.Lock())
            {
                Transaction.Add("update", this);
                this.Status = States.Update;
            }
            else
            {
                throw new Exceptions.ServerException("Failed to lock Item");
            }
        }

        public Transaction Transaction { get; internal set; }

        private Boolean Lock()
        {
            this.Property("locked_by_id").Refresh();
            Item lockedby = (Item)this.Property("locked_by_id").Value;

            if (lockedby == null)
            {
                IO.Item lockitem = new IO.Item(this.ItemType.Name, "lock");
                lockitem.ID = this.ID;
                lockitem.Select = "locked_by_id";
                IO.SOAPRequest request = new IO.SOAPRequest(IO.SOAPOperation.ApplyItem, this.ItemType.Session, lockitem);
                IO.SOAPResponse response = request.Execute();

                if (!response.IsError)
                {
                    this.UpdateProperties(response.Items.First());
                    return true;
                }
                else
                {
                    throw new Exceptions.ServerException(response);
                }
            }
            else if (lockedby.ID.Equals(this.ItemType.Session.UserID))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        internal Boolean UnLock()
        {
            Item lockedby = (Item)this.Property("locked_by_id").Value;

            if (lockedby.ID.Equals(this.ItemType.Session.UserID))
            {
                IO.Item unlockitem = new IO.Item(this.ItemType.Name, "unlock");
                unlockitem.ID = this.ID;
                IO.SOAPRequest request = new IO.SOAPRequest(IO.SOAPOperation.ApplyItem, this.ItemType.Session, unlockitem);
                IO.SOAPResponse response = request.Execute();

                if (!response.IsError)
                {
                    this.UpdateProperties(response.Items.First());
                    this.Status = States.Read;
                    return true;
                }
                else
                {
                    throw new Exceptions.ServerException(response);
                }
            }
            else if (lockedby == null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        internal void UpdateProperties(IO.Item DBItem)
        {
            if (this.ID == DBItem.ID)
            {
                foreach (String propname in DBItem.PropertyNames)
                {
                    this.Property(propname).DBValue = DBItem.GetProperty(propname);
                }
            }
            else
            {
                throw new Exceptions.ArgumentException("Invalid Item ID");
            }
        }

        private Dictionary<RelationshipType, Queries.Relationship> RelationshipsCache;

        public Queries.Relationship Relationships(RelationshipType RelationshipType, String Select)
        {
            if (!this.RelationshipsCache.ContainsKey(RelationshipType))
            {
                this.RelationshipsCache[RelationshipType] = new Queries.Relationship(RelationshipType, Select, this);
            }

            return this.RelationshipsCache[RelationshipType];
        }

        public Queries.Relationship Relationships(String RelationshipType, String Select)
        {
            return this.Relationships(this.ItemType.RelationshipType(RelationshipType), Select);
        }

        public Item(String ID, String ConfigID, ItemType Type)
        {
            this.PropertyCache = new Dictionary<PropertyType, Property>();
            this.RelationshipsCache = new Dictionary<RelationshipType, Queries.Relationship>();
            this.ItemType = Type;

            if (ID == null)
            {
                this.ID = Server.NewID();
                this.ConfigID = this.ID;
                this._status = States.Create;
            }
            else
            {
                this.ID = ID;
                this.ConfigID = ConfigID;
                this._status = States.Read;
            }

        }
    }
}
