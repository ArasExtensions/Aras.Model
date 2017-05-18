/*  
  Copyright 2017 Processwall Limited

  Licensed under the Apache License, Version 2.0 (the "License");
  you may not use this file except in compliance with the License.
  You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

  Unless required by applicable law or agreed to in writing, software
  distributed under the License is distributed on an "AS IS" BASIS,
  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
  See the License for the specific language governing permissions and
  limitations under the License.
 
  Company: Processwall Limited
  Address: The Winnowing House, Mill Lane, Askham Richard, York, YO23 3NW, United Kingdom
  Tel:     +44 113 815 3440
  Web:     http://www.processwall.com
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

        private Class _class;
        internal Class Class
        {
            get
            {
                return this._class;
            }
            set
            {
                if (this._class != value)
                {
                    this._class = value;
                    this._lifeCycleMap = null;
                    this.OnPropertyChanged("Class");
                }
            }
        }

        private Items.LifeCycleMap _lifeCycleMap;
        public Items.LifeCycleMap LifeCycleMap
        {
            get
            {
                if (this._lifeCycleMap == null)
                {
                    this._lifeCycleMap = this.ItemType.LifeCycleMap(this.Class);
                }

                return this._lifeCycleMap;
            }
        }

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
                                this.Action = Model.Item.Actions.Update;
                            }
                            else
                            {
                                throw new Exceptions.ServerException(response);
                            }

                            break;

                        case Model.Item.Locks.User:

                            // Already locked by User
                            this.Action = Model.Item.Actions.Update;

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
                                this.Action = Model.Item.Actions.Read;
                            }
                            else
                            {
                                if (response.ErrorMessage == "Aras.Server.Core.ItemIsNotLockedException")
                                {
                                    this.Locked = Model.Item.Locks.None;
                                }
                                else
                                {
                                    throw new Exceptions.ServerException(response);
                                }
                            }

                            break;

                        case Model.Item.Locks.None:

                            // Item not Locked
                            this.Action = Model.Item.Actions.Read;

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

        public IEnumerable<Relationships.LifeCycleState> NextStates()
        {
            if (this.State == Model.Item.States.Stored)
            {
                List<Relationships.LifeCycleState> ret = new List<Relationships.LifeCycleState>();

                if (this.LifeCycleMap != null)
                {
                    IO.Request request = this.ItemType.Session.IO.Request(IO.Request.Operations.GetItemNextStates);
                    IO.Item item = request.NewItem(this.ItemType.Name, "get");
                    item.ID = this.ID;
                    IO.Response response = request.Execute();

                    if (!response.IsError)
                    {
                        if (response.Items.Count() > 0)
                        {
                            IO.Item lifecycletransisiton = response.Items.First();

                            foreach (IO.Item dblifecyclestate in lifecycletransisiton.ToStates)
                            {
                                foreach (Relationships.LifeCycleState lifecyclestate in this.LifeCycleMap.Relationships("Life Cycle State"))
                                {
                                    if (lifecyclestate.ID.Equals(dblifecyclestate.ID))
                                    {
                                        ret.Add(lifecyclestate);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        throw new Exceptions.ServerException(response);
                    }
                }

                return ret;
            }
            else
            {
                throw new Exceptions.ArgumentException("Item must be stored in Database before Promotion");
            }
        }

        public void Promote(Relationships.LifeCycleState NewState)
        {
            IO.Request request = this.ItemType.Session.IO.Request(IO.Request.Operations.PromoteItem);
            IO.Item item = request.NewItem(this.ItemType.Name, "get");
            item.ID = this.ID;
            item.SetProperty("state", NewState.Name);
            IO.Response response = request.Execute();

            if (!response.IsError)
            {
                // Update Current State
                this.Property(this.ItemType.PropertyType("current_state")).SetValue(NewState);
                this.Property(this.ItemType.PropertyType("state")).SetValue(NewState.Name);        
            }
            else
            {
                throw new Exceptions.ServerException(response);
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

        internal void Delete(Model.Item Item, Transaction Transaction)
        {
            if (Transaction != null)
            {
                if (this.State == Model.Item.States.Stored)
                {
                    // Add to Transaction
                    Transaction.Add("delete", Item);
                }
                else
                {
                    Transaction.Remove(Item);
                }
            }

            this.Action = Model.Item.Actions.Delete;
        }

        private Dictionary<PropertyType, Property> PropertyCache;

        internal Property Property(PropertyType PropertyType)
        {
            if (!this.PropertyCache.ContainsKey(PropertyType))
            {
                this.PropertyCache[PropertyType] = new Property(this, PropertyType);
            }

            return this.PropertyCache[PropertyType];
        }

        internal Item(ItemType ItemType, String ID, String ConfigID, Int32 Generation, Boolean IsCurrent)
        {
            this.ItemType = ItemType;
            this.ID = ID;
            this.ConfigID = ConfigID;
            this.Generation = Generation;
            this.IsCurrent = IsCurrent;
            this.State = Model.Item.States.Stored;
            this.Action = Model.Item.Actions.Read;
            this._lockChecked = false;
            this.PropertyCache = new Dictionary<PropertyType, Property>();
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
            this._lockChecked = false;
            this.PropertyCache = new Dictionary<PropertyType, Property>();
        }
    }
}
