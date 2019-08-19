using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Html5Uploader.Controls
{
    public class ClientEventsCollection : List<ClientEvent>
    {
        public ClientEvent this[ClientEventsNames name]
        {
            get
            {
                return this.Find(p => p.EventName == name);
            }
            set
            {
                ClientEvent par = this[name];
                if (par == null)
                {
                    par.EventName = value.EventName;
                    par.Handle = value.Handle;
                }
                else { Add(value); }
            }
        }

    }

    public class ClientEvent
    { 
        public ClientEvent() { }
        public ClientEvent(ClientEventsNames n, string v)
        { EventName = n; Handle = v; }
        public ClientEventsNames EventName { get; set; }
        public string Handle { get; set; }
    
    }

}
