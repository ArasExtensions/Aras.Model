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
using System.ComponentModel;

namespace Aras.Model
{
    public abstract class Property : INotifyPropertyChanged
    {
        public Session Session
        {
            get
            {
                return this.Item.Session;
            }
        }

        public Item Item { get; private set; }

        public PropertyType PropertyType { get; private set; }

        public String Name
        {
            get
            {
                return this.PropertyType.Name;
            }
        }

        public Boolean ReadOnly
        {
            get
            {
                return this.PropertyType.ReadOnly;
            }
        }

        private object _object;

        internal void SetObject(object Value)
        {
            if (Value == null)
            {
                if (this._object != null)
                {
                    this._object = null;
                    this.OnPropertyChanged("Object");
                    this.OnPropertyChanged("Value");
                }
            }
            else
            {
                if (this._object == null)
                {
                    this._object = Value;
                    this.OnPropertyChanged("Object");
                    this.OnPropertyChanged("Value");
                }
                else
                {
                    if (!this._object.Equals(Value))
                    {
                        this._object = Value;
                        this.OnPropertyChanged("Object");
                        this.OnPropertyChanged("Value");
                    }
                }
            }
        }

        public virtual object Object
        {
            get
            {
                return this._object;
            }
            set
            {
                if (!this.ReadOnly)
                {
                    this.SetObject(value);
                }
                else
                {
                    throw new Exceptions.PropertyReadOnlyException(this.Name);
                }
            }
        }

        internal abstract String ValueString { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(String Name)
        {
            if (this.PropertyChanged != null)
            {
                PropertyChangedEventArgs args = new PropertyChangedEventArgs(Name);
                this.PropertyChanged(this, args);
            }
        }

        protected void OnAllPropertiesChanged()
        {
            if (this.PropertyChanged != null)
            {
                PropertyChangedEventArgs args = new PropertyChangedEventArgs(String.Empty);
                this.PropertyChanged(this, args);
            }
        }

        public override string ToString()
        {
            if (this.ValueString == null)
            {
                return this.PropertyType.Name + ": null";
            }
            else
            {
                return this.PropertyType.Name + ": " + this.ValueString;
            }
        }

        internal Property(Item Item, PropertyType PropertyType)
            :base()
        {
            this.Item = Item;
            this.PropertyType = PropertyType;
        }
    }
}