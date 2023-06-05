using System;
using System.Collections.Generic;

namespace GSC_Engine
{
    public class GSC_InterfaceBridge
    {
        private static GSC_InterfaceBridge instance;

        public static GSC_InterfaceBridge Instance
        {
            get
            {
                if (instance == null) instance = new GSC_InterfaceBridge();
                return instance;
            }
        }

        private GSC_InterfaceBridge() { }
       
        public GSC_Message Process(GSC_Message message)
        {
            //return new GSC_Message<Guid[]>("@result",GSC_GameManager.Instance.HandleMessages(message).ToArray());
            return null;
        }

    }

}