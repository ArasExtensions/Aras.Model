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

    public class Item : INotifyPropertyChanged, IEquatable<Item>
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
            this.DatabaseState = DatabaseStates.Deleted;

            if (this.Deleted != null)
            {
                Deleted(this, new DeletedEventArgs());
            }
        }

        public Session Session
        {
            get
            {
                return this.ItemType.Session;
            }
        }

        public enum Actions { Create, Read, Update, Delete };

        private Actions _action;
        public Actions Action 
        { 
            get
            {
                return this._action;
            }
            private set
            {
                if (this._action != value)
                {
                    this._action = value;
                    this.OnPropertyChanged("Action");
                }
            }
        }

        public enum DatabaseStates { New, Stored, Deleted };

        private DatabaseStates _databaseState;
        public DatabaseStates DatabaseState
        {
            get
            {
                return this._databaseState;
            }
            private set
            {
                if (this._databaseState != value)
                {
                    this._databaseState = value;
                    this.OnPropertyChanged("DatabaseState");
                }
            }
        }

        public ItemType ItemType { get; private set; }

        public String ID { get; private set; }

        public String ConfigID { get; private set; }

        public Int32 Generation { get; private set; }

        public Boolean IsCurrent { get; private set; }

        public Boolean Locked(Boolean Refresh)
        {
            if (this.Action == Actions.Create)
            {
                return true;
            }
            else
            {
                if (Refresh)
                {
                    this.Property("locked_by_id").Refresh();
                }

                if (this.LockedBy != null && this.LockedBy.Equals(this.Session.User))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

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
            private set
            {
                this.Property("locked_by_id").Value = value;
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
            if (this.HasProperty("keyed_name"))
            {
                return this.KeyedName;
            }
            else
            {
                return this.ID;
            }
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
                    case "Federated":
                        this.PropertyCache[PropertyType] = new Properties.Federated(this, (PropertyTypes.Federated)PropertyType);
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

                // Ensure selected in future
                this.ItemType.AddToSelect(PropertyType.Name);
            }

            return this.PropertyCache[PropertyType];
        }

        public Boolean HasProperty(String Name)
        {
            return this.PropertyCache.ContainsKey(this.ItemType.PropertyType(Name));
        }

        public Boolean HasProperty(PropertyType Type)
        {
            return this.PropertyCache.ContainsKey(Type);
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

        public virtual void Refresh()
        {
            if (this.DatabaseState == DatabaseStates.Stored)
            {
                List<String> propertynames = new List<String>();

                foreach(String sysprop in ItemType.SystemProperties)
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
                IO.SOAPRequest request = new IO.SOAPRequest(IO.SOAPOperation.ApplyItem, this.ItemType.Session, dbitem);
                IO.SOAPResponse response = request.Execute();

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
        }

        protected virtual void OnRefresh()
        {
            // Clear Item Access
            this._itemAccess = null;

            // Clear Permissions
            this._canGet = null;
            this._canUpdate = null;
            this._canDelete = null;
            this._canDiscover = null;
            this._canChangeAccess = null;
        }

        public void Update(Transaction Transaction, Boolean UnLock = false)
        {
            if (Transaction != null)
            {
                switch (this.Action)
                {
                    case Actions.Create:

                        break;

                    case Actions.Read:
                    case Actions.Update:

                        if (this.Lock(UnLock))
                        {
                            Transaction.Add("update", this);
                            this.Action = Actions.Update;
                            this.OnUpdate(Transaction);
                        }
                        else
                        {
                            throw new Exceptions.ServerException("Failed to lock Item");
                        }

                        break;

                    default:

                        break;
                }
            }
            else
            {
                throw new Exceptions.ArgumentException("Transaction must not be null");
            }
        }

        public void Delete(Transaction Transaction, Boolean UnLock = false)
        {
            if (Transaction != null)
            {
                if (this.DatabaseState == DatabaseStates.Stored)
                {
                    if (UnLock)
                    {
                        this.UnLock();
                    }

                    Transaction.Add("delete", this);
                }
                else
                {
                    Transaction.Remove(this);
                }
            }

            this.Action = Actions.Delete;
        }

        protected virtual void OnUpdate(Transaction Transaction)
        {
           
        }

        private Boolean Lock(Boolean UnLock)
        {
            Boolean ret = false;

            switch(this.DatabaseState)
            {
                case DatabaseStates.Stored:

                    if (this.LockedBy == null)
                    {
                        IO.Item lockitem = new IO.Item(this.ItemType.Name, "lock");
                        lockitem.ID = this.ID;
                        lockitem.Select = "locked_by_id";
                        IO.SOAPRequest request = new IO.SOAPRequest(IO.SOAPOperation.ApplyItem, this.ItemType.Session, lockitem);
                        IO.SOAPResponse response = request.Execute();

                        if (!response.IsError)
                        {
                            this.UpdateProperties(response.Items.First());
                            ret = true;
                        }
                        else
                        {
                            throw new Exceptions.ServerException(response);
                        }
                    }
                    else if (this.LockedBy.ID.Equals(this.ItemType.Session.UserID))
                    {
                        ret = true;
                    }
                    else if (!this.LockedBy.ID.Equals(this.ItemType.Session.UserID) && UnLock)
                    {
                        // Force Unlock
                        IO.Item unlockitem = new IO.Item(this.ItemType.Name, "unlock");
                        unlockitem.ID = this.ID;
                        IO.SOAPRequest request = new IO.SOAPRequest(IO.SOAPOperation.ApplyItem, this.ItemType.Session, unlockitem);
                        IO.SOAPResponse response = request.Execute();

                        if (!response.IsError)
                        {
                            IO.Item lockitem = new IO.Item(this.ItemType.Name, "lock");
                            lockitem.ID = this.ID;
                            lockitem.Select = "locked_by_id";
                            request = new IO.SOAPRequest(IO.SOAPOperation.ApplyItem, this.ItemType.Session, lockitem);
                            response = request.Execute();

                            if (!response.IsError)
                            {
                                this.UpdateProperties(response.Items.First());
                                ret = true;
                            }
                            else
                            {
                                throw new Exceptions.ServerException(response);
                            }
                        }
                        else
                        {
                            if (response.ErrorMessage.Equals("Aras.Server.Core.ItemIsLockedBySomeoneElseException"))
                            {
                                throw new Exceptions.UnLockException(this);
                            }
                            else
                            {
                                throw new Exceptions.ServerException(response);
                            }
                        }
                    }
                    else
                    {
                        ret = false;
                    }

                    break;

                case DatabaseStates.Deleted:

                    throw new Exceptions.ArgumentException("Item is Deleted");

                default:

                    ret = true;

                    break;
            }

            return ret;
        }

        internal Boolean UnLock()
        {
            if (this.LockedBy == null)
            {
                this.Action = Actions.Read;
                this.DatabaseState = DatabaseStates.Stored;
                return true;
            }
            else
            {
                if (this.LockedBy.ID.Equals(this.ItemType.Session.UserID))
                {
                    IO.Item unlockitem = new IO.Item(this.ItemType.Name, "unlock");
                    unlockitem.ID = this.ID;
                    IO.SOAPRequest request = new IO.SOAPRequest(IO.SOAPOperation.ApplyItem, this.ItemType.Session, unlockitem);
                    IO.SOAPResponse response = request.Execute();

                    if (!response.IsError)
                    {
                        this.UpdateProperties(response.Items.First());
                        this.Action = Actions.Read;
                        this.DatabaseState = DatabaseStates.Stored;
                        this.Property("locked_by_id").DBValue = null;
                        
                        return true;
                    }
                    else
                    {
                        throw new Exceptions.ServerException(response);
                    }
                }
                else
                {
                    return false;
                }
            }
        }

        internal virtual void UpdateProperties(IO.Item DBItem)
        {
            if (DBItem != null)
            {
                if (this.ID == DBItem.ID)
                {
                    this.IsCurrent = DBItem.GetProperty("is_current", "0").Equals("1");

                    foreach (String propname in DBItem.PropertyNames)
                    {
                        if (this.ItemType.HasPropertyType(propname))
                        {
                            this.Property(propname).DBValue = DBItem.GetProperty(propname);
                        }
                    }
                }
                else
                {
                    throw new Exceptions.ArgumentException("Invalid Item ID");
                }
            }
   
            this.DatabaseState = DatabaseStates.Stored;  
        }

        public Boolean IsManager
        {
            get
            {
                if (this.Session.Alias.Equals(this.ManagedBy))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public Boolean IsOwner
        {
            get
            {
                if (this.Session.Alias.Equals(this.OwnedBy) || this.Session.User.Equals(this.CreatedBy))
                {
                    return true;
                }
                else
                {
                    return false;
                }  
            }
        }

        private List<Access> _itemAccess;
        private IEnumerable<Access> ItemAccess
        {
            get
            {
                if (this._itemAccess == null)
                {
                    this._itemAccess = new List<Access>();

                    if (this.Permission != null)
                    {
                        foreach (Access access in this.Permission.Access)
                        {
                            switch (access.Identity.Name)
                            {
                                case "Owner":

                                    if (this.IsOwner)
                                    {
                                        this._itemAccess.Add(access);
                                    }

                                    break;
                                case "Manager":

                                    if (this.IsManager)
                                    {
                                        this._itemAccess.Add(access);
                                    }

                                    break;
                                default:

                                    if (this.Session.Identities.Contains(access.Identity))
                                    {
                                        this._itemAccess.Add(access);
                                    }

                                    break;
                            }
                        }
                    }
                }

                return this._itemAccess;
            }
        }

        private Boolean? _canGet;
        public Boolean CanGet
        {
            get
            {
                if (this._canGet == null)
                {
                    this._canGet = false;

                    foreach (Access access in this.ItemAccess)
                    {
                        if (access.IdentityCanGet)
                        {
                            this._canGet = true;
                            break;
                        }
                    }
                }

                return (Boolean)this._canGet;
            }
        }

        private Boolean? _canUpdate;
        public Boolean CanUpdate
        {
            get
            {
                if (this._canUpdate == null)
                {
                    this._canUpdate = false;

                    foreach (Access access in this.ItemAccess)
                    {
                        if (access.IdentityCanUpdate)
                        {
                            this._canUpdate = true;
                            break;
                        }
                    }
                }

                return (Boolean)this._canUpdate;
            }
        }

        private Boolean? _canDelete;
        public Boolean CanDelete
        {
            get
            {
                if (this._canDelete == null)
                {
                    this._canDelete = false;

                    foreach (Access access in this.ItemAccess)
                    {
                        if (access.IdentityCanDelete)
                        {
                            this._canDelete = true;
                            break;
                        }
                    }
                }

                return (Boolean)this._canDelete;
            }
        }

        private Boolean? _canDiscover;
        public Boolean CanDiscover
        {
            get
            {
                if (this._canDiscover == null)
                {
                    this._canDiscover = false;

                    foreach (Access access in this.ItemAccess)
                    {
                        if (access.IdentityCanDiscover)
                        {
                            this._canDiscover = true;
                            break;
                        }
                    }
                }

                return (Boolean)this._canDiscover;
            }
        }

        private Boolean? _canChangeAccess;
        public Boolean CanChangeAccess
        {
            get
            {
                if (this._canChangeAccess == null)
                {
                    this._canChangeAccess = false;

                    foreach (Access access in this.ItemAccess)
                    {
                        if (access.IdentityCanChangeAccess)
                        {
                            this._canChangeAccess = true;
                            break;
                        }
                    }
                }

                return (Boolean)this._canChangeAccess;
            }
        }

        public virtual void Process(Transaction Transaction)
        {

        }

        private Dictionary<RelationshipType, Stores.Relationship> StoresCache;

        public Stores.Relationship Store(RelationshipType RelationshipType)
        {
            if (!this.StoresCache.ContainsKey(RelationshipType))
            {
                this.StoresCache[RelationshipType] = new Stores.Relationship(RelationshipType, this);
            }

            return this.StoresCache[RelationshipType];
        }

        public Stores.Relationship Store(String RelationshipType)
        {
            return this.Store(this.ItemType.RelationshipType(RelationshipType));
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

        private void Initialise(ItemType ItemType)
        {
            this.PropertyCache = new Dictionary<PropertyType, Property>();
            this.StoresCache = new Dictionary<RelationshipType, Stores.Relationship>();
            this.ItemType = ItemType;
        }

        public Item(ItemType ItemType, Transaction Transaction)
        {
            this.Initialise(ItemType);
            this.ID = Server.NewID();
            this.ConfigID = this.ID;
            this.Generation = 1;
            this.IsCurrent = true;
            this._action = Actions.Create;

            if (!(this is Relationship))
            {
                // Add to Transaction
                Transaction.Add("add", this);
            }
        }

        public Item(ItemType ItemType, IO.Item DBItem)
        {
            this.Initialise(ItemType);
            this.ID = DBItem.ID;
            this.ConfigID = DBItem.ConfigID;
            this.Generation = DBItem.Generation;
            this.IsCurrent = DBItem.IsCurrent;
            this._action = Actions.Read;
            this._databaseState = DatabaseStates.Stored;
            this.UpdateProperties(DBItem);
        }
    }
}
