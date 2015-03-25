
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

namespace Aras.Model.Conditions
{
    public class OR : Condition
    {
        private List<Condition> _children;
        public override IEnumerable<Condition> Children
        {
            get 
            { 
                return this._children; 
            }
        }

        protected override void AddChild(Condition Condition)
        {
            this._children.Add(Condition);
        }

        private System.String _whereClause;
        internal override System.String WhereClause
        {
            get 
            {
                if (this._whereClause == null)
                {
                    switch(this._children.Count)
                    {
                        case 0:
                            this._whereClause = null;
                            break;
                        case 1:
                            this._whereClause = this._children[0].WhereClause;
                            break;
                        default:
                            this._whereClause = "(" + this._children[0].WhereClause;

                            for (int i = 1; i < this._children.Count; i++)
                            {
                                this._whereClause += " or " + this._children[i].WhereClause;
                            }

                            this._whereClause += ")";

                            break;
                    }
                }

                return this._whereClause;
            }
        }

        internal OR(Request.Item Item)
            :base(Item)
        {
            this._children = new List<Condition>();
        }
    }
}
