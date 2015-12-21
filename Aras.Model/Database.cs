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
using System.Threading;
using System.Xml;

namespace Aras.Model
{
    public class Database
    {
        public Server Server { get; private set; }

        public String Name { get; private set; }

        public Session Login(String Username, String Password)
        {
            IO.SOAPRequest request = new IO.SOAPRequest(IO.SOAPOperation.ValidateUser, this, Username, Password);
            IO.SOAPResponse response = request.Execute();

            if (!response.IsError)
            {
                String id = response.Result.SelectSingleNode("id").InnerText;
                Session session = new Session(this, id, Username, Password);
                return session;
            }
            else
            {
                throw new Exceptions.ServerException(response.ErrorMessage);
            }
        }

        public override string ToString()
        {
            return this.Name;
        }

        internal Database(Server Server, String Name)
            : base()
        {
            this.Server = Server;
            this.Name = Name;
        }
    }
}
