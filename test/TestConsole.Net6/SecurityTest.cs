using NetPs.Socket.Extras.Security.SecureHash;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestConsole.Net6
{
    internal class SecurityTest
    {
        internal SecurityTest()
        {
            Test_Sha0();
            Test_Sha1();
        }

        private static void Test_Sha0()
        {
            string text;
            string outtext;
            SHA0 md;
            text = "a";
            for (var i = 0; i < 20; i++) text += text;
            md = new SHA0();
            outtext = md.Make(Encoding.ASCII.GetBytes(text.Substring(0, 1000000)));
            Debug.Assert(outtext == "3232affa48628a26653b5aaa44541fd90d690603");
            text = "abcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopq";
            md = new SHA0();
            outtext = md.Make(Encoding.ASCII.GetBytes(text));
            Debug.Assert(outtext == "d2516ee1acfa5baf33dfc1c471e438449ef134c8");
            text = "abc";
            md = new SHA0();
            outtext = md.Make(Encoding.ASCII.GetBytes(text));
            Debug.Assert(outtext == "0164b8a914cd2a5e74c4f7ff082c4d97f1edf880");
        }
        private static void Test_Sha1()
        {
            string text, outtext;
            SHA1 md;
            text = "abc";
            md = new SHA1();
            outtext = md.Make(Encoding.ASCII.GetBytes(text));
            Debug.Assert(outtext == "a9993e364706816aba3e25717850c26c9cd0d89d");
        }
    }
}
