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

namespace Aras.Model.PropertyTypes
{
    public class Float : PropertyType
    {
        internal Float(ItemType Type, System.String Name, System.String Label, System.Boolean ReadOnly, System.Boolean Required, System.Int32 SortOrder, System.Boolean InSearch, System.Boolean InRelationshipGrid, System.Int32 ColumnWidth, System.Double? Default)
            :base(Type, Name, Label, ReadOnly, Required, SortOrder, InSearch, InRelationshipGrid, ColumnWidth, Default)
        {
  
        }
    }
}
