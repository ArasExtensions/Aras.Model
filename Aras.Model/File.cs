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
using System.IO;
using System.Net;
using System.Web;

namespace Aras.Model
{
    [Attributes.ItemType("File")]
    public class File : Item
    {
        const int bufferlength = 4096;

        public String CheckedOutPath
        {
            get
            {
                return (String)this.Property("checkedout_path").Value;
            }
            set
            {
                this.Property("checkedout_path").Value = value;
            }
        }

        public String Checksum
        {
            get
            {
                return (String)this.Property("checksum").Value;
            }
        }

        public String Filename
        {
            get
            {
                return (String)this.Property("filename").Value;
            }
        }

        public Int32 FileSize
        {
            get
            {
                return (Int32)this.Property("file_size").Value;
            }
        }

        public FileType FileType
        {
            get
            {
                return (FileType)this.Property("file_type").Value;
            }
            set
            {
                this.Property("file_type").Value = value;
            }
        }

        private String _downloadToken;
        private String DownloadToken
        {
            get
            {
                if (this._downloadToken == null)
                {
                    
                    byte[] buffer = new byte[bufferlength];
                    int read = 0;

                    String url = this.ItemType.Session.Database.Server.AuthenticationBrokerURL + "/GetFileDownloadToken?rnd=" + this.ItemType.Session.DownloadRandom().ToString();
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                    request.CookieContainer = this.ItemType.Session.Database.Server.Cookies;
                    request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                    request.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
                    request.Headers.Add("Cache-Control", "no-cache");
                    request.Method = "POST";
                    request.ContentType = "application/json; charset=utf-8";
                    request.Headers.Add("AUTHPASSWORD", this.ItemType.Session.Password);
                    request.Headers.Add("AUTHUSER", this.ItemType.Session.Username);
                    request.Accept = "application/json; charset=utf-8";
                    request.Headers.Add("DATABASE", this.ItemType.Session.Database.Name);
                    request.Headers.Add("SOAPACTION", "GetFileDownloadToken");
                    request.Headers.Add("TIMEZONE_NAME", "GMT Standard Time");

                    String body = "{\"param\":{\"fileId\":\"" + this.ID + "\"}}";
                    byte[] bodybytes = System.Text.Encoding.ASCII.GetBytes(body);

                    request.ContentLength = bodybytes.Length;

                    using (Stream poststream = request.GetRequestStream())
                    {
                        poststream.Write(bodybytes, 0, bodybytes.Length);
                    }

                    using (HttpWebResponse webresponse = (HttpWebResponse)request.GetResponse())
                    {
                        // Store Cookies
                        this.ItemType.Session.Database.Server.Cookies.Add(webresponse.Cookies);

                        using (Stream result = webresponse.GetResponseStream())
                        {
                            String resultstring = "";

                            while ((read = result.Read(buffer, 0, bufferlength)) > 0)
                            {
                                resultstring = resultstring + Encoding.UTF8.GetString(buffer, 0, read);
                            }

                            this._downloadToken = resultstring.Substring(6, resultstring.Length - 8);
                        }
                    }
                }

                return this._downloadToken;
            }
        }

        private Vault _userVault;
        private Vault UserVault
        {
            get
            {
                if (this._userVault == null)
                {
                    foreach(Located located in this.Store("Located"))
                    {
                        if (this.ItemType.Session.User.Vault.Equals(located.Vault))
                        {
                            this._userVault = located.Vault;
                            break;
                        }
                    }

                    if (this._userVault == null)
                    {
                        throw new Exceptions.ServerException("File is not located in Users Vault");
                    }
                }

                return this._userVault;
            }
        }

        private String _uRL;
        public String URL
        {
            get
            {
                if (this._uRL == null)
                {
                    this._uRL = this.UserVault.URL + "?dbname=" + this.ItemType.Session.Database.Name + "&fileId=" + this.ID + "&fileName=" + HttpUtility.UrlEncode(this.Filename) + "&vaultId=" + this.UserVault.ID + "&token=" + this.DownloadToken;
                }

                return this._uRL;
            }
        }

        public void Read(Stream Output)
        {
            byte[] buffer = new byte[bufferlength];
            int read = 0;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(this.URL);
            request.CookieContainer = this.ItemType.Session.Database.Server.Cookies;
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
            request.Headers.Add("Cache-Control", "no-cache");
            request.Method = "GET";
            
            using( HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                using(Stream stream = response.GetResponseStream())
                {
                    while ((read = stream.Read(buffer, 0, bufferlength)) > 0)
                    {
                        Output.Write(buffer, 0, read);
                    }
                }
            }
        }

        private FileInfo _cacheFilename;
        private FileInfo CacheFilename
        {
            get
            {
                if (this._cacheFilename == null)
                {
                    this._cacheFilename = new FileInfo(this.Session.CacheDirectory.FullName + "\\" + this.ID + ".dat");
                }

                return this._cacheFilename;
            }
        }

        internal byte[] GetCacheBytes()
        {
            if (this.CacheFilename.Exists)
            {
                byte[] buffer = new byte[this.CacheFilename.Length];

                using(FileStream cachefile = new FileStream(this.CacheFilename.FullName, FileMode.Open))
                {
                    cachefile.Read(buffer, 0, buffer.Length);
                }

                return buffer;
            }
            else
            {
                return new byte[0];
            }
        }

        internal String VaultFilename { get; private set; }

        public void Write(Stream Input, String Filename)
        {
            byte[] buffer = new byte[bufferlength];
            int read = 0;

            using (FileStream cache = new FileStream(this.CacheFilename.FullName, FileMode.Create))
            {
                while ((read = Input.Read(buffer, 0, bufferlength)) > 0)
                {
                    cache.Write(buffer, 0, read);
                }
            }

            // Store Filename
            this.VaultFilename = Path.GetFileName(Filename);
        }

        protected override void OnRefresh()
        {
            base.OnRefresh();
        }

        public File(ItemType ItemType, Transaction Transaction)
            : base(ItemType, Transaction)
        {
           
        }

        public File(ItemType ItemType, IO.Item DBItem)
            : base(ItemType, DBItem)
        {

        }
    }
}
