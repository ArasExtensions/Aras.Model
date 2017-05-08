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

namespace Aras.Model.Cache
{
    public class PropertyValueChangedEventArgs : EventArgs
    {
        public PropertyType PropertyType { get; private set; }

        public PropertyValueChangedEventArgs(PropertyType PropertyType)
            : base()
        {
            this.PropertyType = PropertyType;
        }
    }

    public delegate void PropertyValueChangedEventHandler(object sender, PropertyValueChangedEventArgs e);


    internal class Item : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(String Name)
        {
            if (this.PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(Name));
            }
        }

        public event PropertyValueChangedEventHandler PropertyValueChanged;

        private void OnPropertyValueChanged(PropertyType PropertyType)
        {
            if (this.PropertyValueChanged != null)
            {
                PropertyValueChanged(this, new PropertyValueChangedEventArgs(PropertyType));
            }
        }

        internal ItemType ItemType { get; private set; }

        internal String ID { get; private set; }

        internal String ConfigID { get; private set; }

        internal Int32 Generation { get; private set; }

        private Boolean _isCurrent;
        internal Boolean IsCurrent
        {
            get
            {
                return this._isCurrent;
            }
            private set
            {
                if (this._isCurrent != value)
                {
                    this._isCurrent = value;
                    this.OnPropertyChanged("IsCurrent");
                }
            }
        }

        internal Boolean IsRelationship { get; private set; }

        private Model.Item.States _state;
        internal Model.Item.States State
        {
            get
            {
                return this._state;
            }
            private set
            {
                if (this._state != value)
                {
                    this._state = value;
                    this.OnPropertyChanged("State");
                }
            }
        }

        private Model.Item.Actions _action;
        internal Model.Item.Actions Action
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

        private Model.Item.Locks _locked;
        private Boolean _lockChecked;
        internal Model.Item.Locks Locked
        {
            get
            {
                if (!this._lockChecked)
                {
                    IO.Item checklockitem = new IO.Item(this.ItemType.Name, "get");
                    checklockitem.ID = this.ID;
                    checklockitem.Select = "locked_by_id";
                    IO.Request request = this.ItemType.Session.IO.Request(IO.Request.Operations.ApplyItem, checklockitem);
                    IO.Response response = request.Execute();

                    if (!response.IsError)
                    {
                        if (response.Items.Count() == 1)
                        {
                            String locked_by_id = response.Items.First().GetProperty("locked_by_id");

                            if (String.IsNullOrEmpty(locked_by_id))
                            {
                                this._locked = Model.Item.Locks.None;
                            }
                            else
                            {
                                if (locked_by_id.Equals(this.ItemType.Session.IO.UserID))
                                {
                                    this._locked = Model.Item.Locks.User;
                                }
                                else
                                {
                                    this._locked = Model.Item.Locks.OtherUser;
                                }
                            }

                            this._lockChecked = true;
                        }
                        else
                        {
                            throw new Exceptions.ServerException("Failed to check lock status");
                        }

                        
                    }
                    else
                    {
                        throw new Exceptions.ServerException(response);
                    }  
                }

                return this._locked;
            }
            private set
            {
                if (this._locked != value)
                {
                    this._locked = value;
                    this.OnPropertyChanged("Locked");
                }
            }
        }

        private void Lock()
        {
            switch (this.State)
            {
                case Model.Item.States.Stored:

                    switch (this.Locked)
                    {

                        case Model.Item.Locks.None:
                            
                            // Lock Item
                            IO.Item lockitem = new IO.Item(this.ItemType.Name, "lock");
                            lockitem.ID = this.ID;
                            lockitem.DoGetItem = false;
                            IO.Request request = this.ItemType.Session.IO.Request(IO.Request.Operations.ApplyItem, lockitem);
                            IO.Response response = request.Execute();

                            if (!response.IsError)
                            {
                                this.Locked = Model.Item.Locks.User;
                            }
                            else
                            {
                                throw new Exceptions.ServerException(response);
                            }

                            break;

                        case Model.Item.Locks.User:

                            // Already locked by User

                            break;

                        case Model.Item.Locks.OtherUser:

                            throw new Exceptions.ServerException("Item locked by another user");
                    }

                    break;
            
                case Model.Item.States.Deleted:

                    throw new Exceptions.ArgumentException("Item is Deleted");

                case Model.Item.States.New:

                    // New Item not stored in database, no need to lock

                    break;
            }
        }

        internal void UnLock()
        {
            switch (this.State)
            {
                case Model.Item.States.Stored:

                    switch (this.Locked)
                    {
                        case Model.Item.Locks.User:

                            IO.Item lockitem = new IO.Item(this.ItemType.Name, "unlock");
                            lockitem.ID = this.ID;
                            lockitem.DoGetItem = false;
                            IO.Request request = this.ItemType.Session.IO.Request(IO.Request.Operations.ApplyItem, lockitem);
                            IO.Response response = request.Execute();

                            if (!response.IsError)
                            {
                                this.Locked = Model.Item.Locks.None;
                            }
                            else
                            {
                                throw new Exceptions.ServerException(response);
                            }

                            break;

                        case Model.Item.Locks.None:

                            // Item not Locked

                            break;
                        case Model.Item.Locks.OtherUser:

                            throw new Exceptions.ServerException("Item locked by another User");
                    }

                    break;

                case Model.Item.States.Deleted:

                    throw new Exceptions.ArgumentException("Item is Deleted");

                default:

                    break;
            }
        }

        internal void Update(Model.Item Item, Transaction Transaction)
        {
            if (Transaction != null)
            {
                switch (this.Action)
                {
                    case Model.Item.Actions.Create:
                        Transaction.Add("add", Item);
                        break;

                    case Model.Item.Actions.Read:
                    case Model.Item.Actions.Update:

                        switch (this.Locked)
                        {
                            case Model.Item.Locks.None:
                                this.Lock();
                                this.Action = Model.Item.Actions.Update;
                                Transaction.Add("update", Item);
                                break;

                            case Model.Item.Locks.User:
                                this.Action = Model.Item.Actions.Update;
                                Transaction.Add("update", Item);
                                break;

                            case Model.Item.Locks.OtherUser:
                                throw new Exceptions.ServerException("Item Locked by another User");
                        }

                        break;

                    case Model.Item.Actions.Delete:

                        if (this.State != Model.Item.States.Deleted)
                        {
                            switch (this.Locked)
                            {
                                case Model.Item.Locks.None:
                                    this.Lock();
                                    this.Action = Model.Item.Actions.Update;
                                    Transaction.Add("update", Item);
                                    break;

                                case Model.Item.Locks.User:
                                    this.Action = Model.Item.Actions.Update;
                                    Transaction.Add("update", Item);
                                    break;

                                case Model.Item.Locks.OtherUser:
                                    throw new Exceptions.ServerException("Item Locked by another User");
                            }
                        }
                        else
                        {
                            throw new Exceptions.ArgumentException("Not able to Update a Deleted Item");
                        }

                        break;
                }
            }
            else
            {
                throw new Exceptions.ArgumentException("Transaction must not be null");
            }
        }

        private Dictionary<PropertyType, Object> PropertyCache;

        internal Object GetPropertyValue(PropertyType PropertyType)
        {
            if (!this.PropertyCache.ContainsKey(PropertyType))
            {
                this.PropertyCache[PropertyType] = PropertyType.Default;
            }

            return this.PropertyCache[PropertyType];
        }

        internal Boolean SetPropertyValue(PropertyType PropertyType, Object Value, Boolean FromDatabase)
        {
            Boolean ret = false;

            if (!this.PropertyCache.ContainsKey(PropertyType))
            {
                this.PropertyCache[PropertyType] = PropertyType.Default;
            }

            if (this.PropertyCache[PropertyType] == null)
            {
                if (Value != null)
                {
                    this.PropertyCache[PropertyType] = Value;

                    if (!FromDatabase)
                    {
                        this.OnPropertyValueChanged(PropertyType);
                    }

                    ret = true;
                }
            }
            else
            {
                if (!this.PropertyCache[PropertyType].Equals(Value))
                {
                    this.PropertyCache[PropertyType] = Value;

                    if (!FromDatabase)
                    {
                        this.OnPropertyValueChanged(PropertyType);
                    }

                    ret = true;
                }
            }

            return ret;
        }

        internal Item(ItemType ItemType, String ID, String ConfigID, Int32 Generation, Boolean IsCurrent)
        {
            this.ItemType = ItemType;
            this.ID = ID;
            this.ConfigID = ConfigID;
            this.Generation = Generation;
            this.IsCurrent = IsCurrent;
            this.IsRelationship = (ItemType is RelationshipType);
            this.State = Model.Item.States.Stored;
            this.Action = Model.Item.Actions.Read;
            this.PropertyCache = new Dictionary<PropertyType, object>();
            this._lockChecked = false;
        }

        internal Item(ItemType ItemType)
        {
            this.ItemType = ItemType;
            this.ID = IO.Server.NewID();
            this.ConfigID = this.ID;
            this.Generation = 1;
            this.IsCurrent = true;
            this.State = Model.Item.States.New;
            this.Action = Model.Item.Actions.Create;
            this.PropertyCache = new Dictionary<PropertyType, object>();
            this._lockChecked = false;
        }
    }
}
