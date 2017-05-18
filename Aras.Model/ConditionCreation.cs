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

namespace Aras
{
    public class Conditions
    {

        public static Model.Conditions.None None()
        {
            return new Model.Conditions.None();
        }

        public static Model.Conditions.All All()
        {
            return new Model.Conditions.All();
        }

        public static Model.Conditions.Property Eq(String Name, Object Value)
        {
            return new Model.Conditions.Property(Name, Model.Conditions.Operators.eq, Value);
        }

        public static Model.Conditions.Property Ge(String Name, Object Value)
        {
            return new Model.Conditions.Property(Name, Model.Conditions.Operators.ge, Value);
        }

        public static Model.Conditions.Property Gt(String Name, Object Value)
        {
            return new Model.Conditions.Property(Name, Model.Conditions.Operators.gt, Value);
        }

        public static Model.Conditions.Property Le(String Name, Object Value)
        {
            return new Model.Conditions.Property(Name, Model.Conditions.Operators.le, Value);
        }

        public static Model.Conditions.Property Lt(String Name, Object Value)
        {
            return new Model.Conditions.Property(Name, Model.Conditions.Operators.lt, Value);
        }

        public static Model.Conditions.Property Ne(String Name, Object Value)
        {
            return new Model.Conditions.Property(Name, Model.Conditions.Operators.ne, Value);
        }

        public static Model.Conditions.Property Like(String Name, Object Value)
        {
            return new Model.Conditions.Property(Name, Model.Conditions.Operators.like, Value);
        }

        public static Model.Conditions.And And(Model.Condition Left, Model.Condition Right)
        {
            return new Model.Conditions.And(Left, Right);
        }

        public static Model.Conditions.Or Or(Model.Condition Left, Model.Condition Right)
        {
            return new Model.Conditions.Or(Left, Right);
        }
    }
}
