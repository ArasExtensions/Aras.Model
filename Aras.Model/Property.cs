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
    public abstract class Property : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(String Name)
        {
            if (this.PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(Name));
            }
        }

        internal Cache.Property Cache { get; private set; }

        public Item Item { get; private set; }

        public PropertyType Type
        {
            get
            {
                return this.Cache.Type;
            }
        }

        public Boolean Modified 
        { 
            get
            {
                return this.Cache.Modified;
            }
        }

        public Boolean Required
        {
            get
            {
                return this.Type.Required;
            }
        }

        public Boolean ReadOnly
        {
            get
            {
                return this.Cache.ReadOnly;
            }
        }

        public virtual Object Value
        {
            get
            {
                return this.Cache.Value;
            }
            set
            {
                this.Cache.Value = value;
            }
        }

        protected void SetValue(Object Value)
        {
            this.Cache.SetValue(Value);
        }

        internal abstract String DBValue { get; set; }

        public override string ToString()
        {
            if (this.Value == null)
            {
                return this.Type.Name + ": null";
            }
            else
            {
                return this.Type.Name + ": " + this.Value.ToString();
            }
        }

        private void Cache_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.OnPropertyChanged(e.PropertyName);
        }

        internal Property(Item Item, PropertyType Type)
        {
            this.Item = Item;
            this.Cache = this.Item.Cache.Property(Type);
            this.Item.Cache.PropertyChanged += Cache_PropertyChanged;
        }
    }
}
