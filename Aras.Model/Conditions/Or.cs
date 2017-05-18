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

namespace Aras.Model.Conditions
{
    public class Or : Condition
    {
        public void Add(Condition Condition)
        {
            this.AddChild(Condition);
        }

        internal override String Where(ItemType ItemType)
        {
            switch (this.Children.Count())
            {
                case 0:
                    return null;
                case 1:
                    return this.Children.First().Where(ItemType);
                default:
                    String ret = "(" + this.Children.First().Where(ItemType);

                    for (int i = 1; i < this.Children.Count(); i++)
                    {
                        ret += " or " + this.Children.ElementAt(i).Where(ItemType);
                    }

                    ret += ")";

                    return ret;
            }
        }

        public override bool Equals(Condition other)
        {
            if (other != null && other is Or && (this.Children.Count() == other.Children.Count()))
            {
                Boolean ret = true;

                for (int i = 0; i < this.Children.Count(); i++)
                {
                    if (!this.Children.ElementAt(i).Equals(other.Children.ElementAt(i)))
                    {
                        ret = false;
                        break;
                    }
                }

                return ret;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            int ret = 0;

            foreach(Condition child in this.Children)
            {
                ret = ret ^ child.GetHashCode();
            }

            return ret;
        }

        internal Or(Condition Left, Condition Right)
            : base()
        {
            this.AddChild(Left);
            this.AddChild(Right);
        }
    }
}
