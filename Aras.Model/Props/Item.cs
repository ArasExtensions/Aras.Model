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

namespace Aras.Model.Properties
{
    public class Item : Property
    {
        private Model.Item _propertyItem;

        public override Object Value
        {
            get
            {
                return this._propertyItem;
            }
            set
            {
                if (value == null)
                {
                    if (base.Value != null)
                    {
                        this._propertyItem = null;
                        base.Value = null;
                    }
                }
                else if (value is Model.Item)
                {
                    if (this._propertyItem == null)
                    {
                        this._propertyItem = (Model.Item)value;
                        base.Value = this._propertyItem.Cache;
                    }
                    else
                    {
                        if (!this._propertyItem.Equals((Model.Item)value))
                        {
                            this._propertyItem = (Model.Item)value;
                            base.Value = this._propertyItem.Cache;
                        }
                    }
                }
                else
                {
                    throw new Exceptions.ArgumentException("Value must be a Aras.Model.Item");
                }
            }
        }

        internal override string DBValue
        {
            get
            {
               if (this._propertyItem != null)
               {
                   return this._propertyItem.ID;
               }
               else
               {
                   return null;
               }
            }
            set
            {
                if (value == null)
                {
                    this._propertyItem = null;
                    this.SetValue(null);
                }
                else
                {
                    this._propertyItem = this.Store.Get(value);

                    if (this._propertyItem != null)
                    {
                        this.SetValue(this._propertyItem.Cache);
                    }
                    else
                    {
                        this.SetValue(null);
                    }
                }
            }
        }

        public Store Store
        {
            get
            {
                return this.Item.Store.Query.Property((PropertyTypes.Item)this.Type).Store;
            }
        }

        internal void SetDBValue(Model.Item Item)
        {
            this._propertyItem = Item;

            if (Item != null)
            {
                this.SetValue(Item.Cache);
            }
            else
            {
                this.SetValue(null);
            }
        }

        protected override void Cache_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("Value"))
            {
                if (this.Cache.Value == null)
                {
                    this._propertyItem = null;
                }
                else
                {
                    if (this._propertyItem != null)
                    {
                        if (!((Cache.Item)this.Cache.Value).ID.Equals(this._propertyItem.ID))
                        {
                            this._propertyItem = this.Store.Get(((Cache.Item)this.Cache.Value).ID);
                        }
                    }
                    else
                    {
                        this._propertyItem = this.Store.Get(((Cache.Item)this.Cache.Value).ID);
                    }
                }
            }

            base.Cache_PropertyChanged(sender, e);
        }

        internal Item(Model.Item Item, PropertyTypes.Item Type)
            :base(Item, Type)
        {

        }
    }
}
