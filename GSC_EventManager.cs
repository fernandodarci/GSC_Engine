using System;
using System.Collections.Generic;
using System.Linq;

namespace GSC_Engine
{
    public sealed class GSC_EventManager
    {
        private class GSC_GameEvent
        {
            public readonly string EventName;
            public GSC_Message[] EventData;

            public GSC_GameEvent(string eventName, GSC_Message[] eventData)
            {
                EventName = eventName;
                EventData = eventData;
            }
        }

        private static GSC_EventManager instance;
        private Dictionary<string, GSC_GameEvent> StateTransitions;
        public string CurrentState { get; private set; }

        private GSC_EventManager() => StateTransitions = new Dictionary<string, GSC_GameEvent>();
        
        public static GSC_EventManager Instance
        {
            get
            {
                if (instance == null) instance = new GSC_EventManager();
                return instance;
            }
        }

        #region AUXILIAR METHODS

        private GSC_Message[] RedirectToElementManager(GSC_Message[] messages)
        {
            List<GSC_Message> response = new List<GSC_Message>();
            foreach(GSC_Message message in messages)
            {
                response.Add(GSC_ElementManager.Instance.Process(message));
            }

            return response.ToArray();
        }
        
        private GSC_Message[] RedirectToInterfaceBridge(GSC_Message[] messages)
        {
            List<GSC_Message> response = new List<GSC_Message>();
            foreach (GSC_Message message in messages)
            {
                response.Add(GSC_InterfaceBridge.Instance.Process(message));
            }

            return response.ToArray();
        }

        #endregion
    }
}