using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSC_Engine
{
    public static class GSC_GameManager
    {
        public static void ShowProcessResult(GSC_Message message)
        {
            Console.WriteLine("Result of process:");
            GSC_Message result = GSC_ElementManager.Instance.Process(message);
            if (result is GSC_Message<int> @integer) Console.WriteLine($"{integer.Message} - {integer.Arg1}");
            else if (result is GSC_Message<string> @str) Console.WriteLine(str.Arg1);
            else if (result is GSC_Message<Guid[]> @choose) PromptToChoose(choose);
        }
        
        public static void PromptToChoose(GSC_Message<Guid[]> choose)
        {
            if (choose.Arg1.IsNullOrEmpty()) return;

            while (true)
            {
                Console.Clear();
                Console.WriteLine("Please choose one object from list");
                for (int i = 0; i < choose.Arg1.Length; i++)
                {
                    Console.WriteLine($"{i} => {choose.Arg1[i]}");
                }

                string result = Console.ReadLine();
                if (int.TryParse(result, out int chosen) && chosen >= 0 && chosen < choose.Arg1.Length)
                {
                    ShowProcessResult(new GSC_Message<string>("@getbyid", choose.Arg1[chosen].ToString()));
                    return;
                }
            }
        }

        public static void Main(string[] args)
        {
            string script = string.Empty;
            while (true)
            {
                Console.WriteLine("Enter the script line to parse:");
                script = Console.ReadLine();
                Console.WriteLine("The result of the script line was:");
                List<GSC_ScriptToken> tokens = GSC_LexicalParser.Parse(script);

                foreach (GSC_ScriptToken token in tokens) Console.Write($"{token.TokenType} ");
                Console.WriteLine();
                List<GSC_Message> message = GSC_Lexicon.Parse(tokens);
                if (message.IsNullOrEmpty()) Console.WriteLine("No message processed");
                else
                {
                    foreach(GSC_Message msg in message)
                    {
                        if (msg is GSC_Message<string, string> @ss) Console.WriteLine($"GSC_Message (string,string) {ss.Message} - {ss.Arg1},{ss.Arg2}");
                        else if (msg is GSC_Message<string, int> @si) Console.WriteLine($"GSC_Message (string,integer) {si.Message} - {si.Arg1},{si.Arg2}");
                        else if (msg is GSC_Message<string> @s) Console.WriteLine($"GSC_Message (string) {s.Message} - {s.Arg1}");
                        else if (msg is GSC_Message) Console.WriteLine($"GSC_Message: {msg.Message}");
                        else
                        {
                            Console.WriteLine("Some unexpected error");
                            break;
                        }

                        ShowProcessResult(msg);

                    }
                }
                Console.WriteLine();
                Console.WriteLine("Press ESC to exit or any other key to continue...");
                if (Console.ReadKey().Key == ConsoleKey.Escape) break;
                Console.Clear();
            }
        }
    }

   
}
