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
using Aras.Model;
using System.IO;

namespace Aras.Model.Design.Debug
{
    class Program
    {
        static void Main(string[] args)
        {
            // Connect to Server
            Model.Server server = new Model.Server("http://localhost/11SP9");

            // Load Solution
            server.LoadAssembly("Aras.Model.Design");
            
            // Get Database
            Model.Database database = server.Database("Development");
            
            // Start Session
            Model.Session session = database.Login("admin", IO.Server.PasswordHash("innovator"));

            // Find Document
            Model.Query docquery = session.Query("Document");
            docquery.Select = "item_number,name,description";
            Model.Design.Document document = (Model.Design.Document)docquery.Store.First();

            using (Transaction trans = session.BeginTransaction())
            {
                document.Update(trans);
                document.Name = "Test";
                trans.Commit(true);
            }
            
        }
    }
}
