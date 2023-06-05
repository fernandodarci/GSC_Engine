namespace GSC_Engine
{
    public class GSC_Message 
    {
        public readonly string Message;

        public GSC_Message(string message)
        {
            Message = message;
        }
    }

    public class GSC_Message<T1> : GSC_Message
    {
        public readonly T1 Arg1;

        public GSC_Message(string message, T1 arg1) : base(message)
        {
            Arg1 = arg1;
        }
    }

    public class GSC_Message<T1,T2> : GSC_Message<T1>
    {
        public readonly T2 Arg2;

        public GSC_Message(string message, T1 arg1, T2 arg2) : base(message,arg1)
        {
            Arg2 = arg2;
        }
    }

    public class GSC_Message<T1,T2,T3> : GSC_Message<T1,T2>
    {
        public readonly T3 Arg3;

        public GSC_Message(string message, T1 arg1, T2 arg2, T3 arg3) : base(message, arg1, arg2)
        {
            Arg3 = arg3;
        }
    }

}