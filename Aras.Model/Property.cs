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

        protected virtual void Cache_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.OnPropertyChanged(e.PropertyName);
        }

        internal Property(Item Item, PropertyType Type)
        {
            this.Item = Item;
            this.Cache = this.Item.Cache.Property(Type);
            this.Cache.PropertyChanged += Cache_PropertyChanged;
        }
    }
}
