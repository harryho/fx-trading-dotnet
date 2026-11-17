// -----
// MIT License
//
// Copyright (c) 2005-2026 Harry Ho
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// -----

using System;
using System.Collections.Generic;
using System.Text;

using System.IO;

using System.Configuration;


namespace fxpa
{
    [CustomName("FxpaSource")]
    public class FxpaSource:IFxpaBaseCompoent
    {
       public static string folderPath=AppContext.APP_PATH+"\\fxd";
        //public string FolderPath
        //{
        //    get { lock (this) { return folderPath; } }
        //    set { lock (this) { folderPath = value; } }
        //}

        public static SettingsBase settings;
        FxpaBase _platform;
        protected FxpaBase Platform
        {
            get { return _platform; }
        }

        public virtual bool IsInitialized
        {
            get { return _platform != null; }
        }

        //protected Dictionary<SessionInfo, List<TransportInfo>> _sessions = new Dictionary<SessionInfo, List<TransportInfo>>();

        private static  List <SessionInfo> sessionList= new List<SessionInfo>();
        private static List<SessionInfo> availableSessionList = new List<SessionInfo>();

        public static List<SessionInfo> AvailableSessionList
        {
            get { return FxpaSource.availableSessionList; }
            //set { FxpaSource.availableSessionList = value; }
        }

        public static List<SessionInfo> SessionList
        {
            get { lock (sessionList) { return sessionList; } }
        }

        private void AddSessionInfo(SessionInfo info)
        {
            availableSessionList.Add(info);
        }

        
        public static  void AddAvailableSession(string symbol){
            SessionInfo info = new SessionInfo(symbol, symbol, Guid.NewGuid(), TimeSpan.FromMinutes(30), 100000, 2);
            if(!availableSessionList.Contains(info))
                 availableSessionList.Add( info);
        }


        protected void AddSession(SessionInfo sessionInfo)
        {
            sessionList.Add(sessionInfo);
        }

        protected void RemoveSession(SessionInfo sessionInfo)
        {
         sessionList.Remove(sessionInfo);
            
        }

        public bool SetInitialState(SettingsBase data)
        {
            return OnSetInitialState(data);
        }

        public bool Initialize(FxpaBase platform)
        {
            //SystemMonitor.CheckThrow(_platform == null);

            if (OnInitialize(platform) == false)
            {
                return false;
            }
            _platform = platform;

            return true;
        }

        public bool UnInitialize()
        {
            if (IsInitialized == false)
            {
                return true;
            }

            bool result = OnUnInitialize();
            _platform = null;

            return result;
        }
        protected virtual bool OnUnInitialize()
        {
            return true;
        }


        string name;
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        public FxpaSource()
            //: base(false)
        {
            this.Name = CustomNameAttribute.GetClassAttributeName(typeof(FxpaSource));
            //this._id.Name = this.Name;
        }

        protected bool OnSetInitialState(SettingsBase data)
        {
            settings = data;
            //folderPath = (string)data["QuoteDataFolder"];
            return true;
        }

        protected bool OnInitialize(FxpaBase platform)
        {
            UpdateAvailableSourceSessions();
            return true;
        }

        void UpdateAvailableSourceSessions()
        {
            string current = Directory.GetCurrentDirectory();
            Console.WriteLine(" XXXXXXXXXXXXXX  file  >>>>>>>>> " + current );

            foreach (string symbol in Enum.GetNames(Symbol.AUDJPY.GetType()))
            {
                SessionInfo info = new SessionInfo(symbol , symbol, Guid.NewGuid(), TimeSpan.FromMinutes(30) , 100000, 2);
                this.AddSession(info);
            }
        }
    }
}

