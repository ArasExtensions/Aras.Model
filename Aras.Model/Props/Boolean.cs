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
    public class Boolean : Property
    {
        public override Object Value
        {
            get
            {
                return base.Value;
            }
            set
            {
                if (value == null)
                {
                    if (base.Value != null)
                    {
                        base.Value = value;
                    }
                }
                else if (value is System.Boolean)
                {
                    if (base.Value == null)
                    {
                        base.Value = value;
                    }
                    else
                    {
                        if (!((System.Boolean)base.Value).Equals((System.Boolean)value))
                        {
                            base.Value = value;
                        }
                    }
                }
                else
                {
                    throw new Exceptions.ArgumentException("Value must be a System.Boolean");
                }
            }
        }

        internal override string DBValue
        {
            get
            {
                if (this.Value == null)
                {
                    return null;
                }
                else
                {
                    if ((System.Boolean)this.Value)
                    {
                        return "1";
                    }
                    else
                    {
                        return "0";
                    }
                }
            }
            set
            {
                if (value == null)
                {
                    this.SetValue(null);
                }
                else
                {
                    this.SetValue("1".Equals(value));
                }
            }
        }

        internal Boolean(Model.Item Item, PropertyTypes.Boolean Type)
            :base(Item, Type)
        {

        }
    }
}
