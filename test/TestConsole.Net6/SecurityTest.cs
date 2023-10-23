using NetPs.Socket.Extras.Security.MessageDigest;
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
            //Test_MD6();
            Test_Sha0();
            Test_Sha1();
        }
        internal static IEnumerable<(byte[], string)> TestInputDatas(params string[] outs)
        {
            string text;
            text = "netps";
            for (var i = 0; i < 10; i++) text += text;
            yield return (Encoding.ASCII.GetBytes(text), outs[0]);
            text = "123";
            yield return (Encoding.ASCII.GetBytes(text), outs[1]);
            text = "abc";
            yield return (Encoding.ASCII.GetBytes(text), outs[2]);
        }
        private static void Test_MD6()
        {
            string outtext;
            MD6 md;
            foreach (var data in TestInputDatas(
                "c0c807f1e4b72ac3fa58b6213a735f0a"
                , "650cdaa202ca22b1d8d7697f98267ae5"
                , "8db50d79cf42fe7d1807ebaa15329c61"
            ))
            {
                md = new MD6();
                outtext = md.Make(data.Item1);
                Debug.Assert(outtext == data.Item2);
            }
        }
        private static void Test_Sha0()
        {
            string outtext;
            SHA0 md;
            foreach (var data in TestInputDatas(
                "1260fe7001e4761c67983e1d41c2765d5d2ef187"
                , "4f797c608cab4a00501447df8ee8833f363ea7d3"
                , "0164b8a914cd2a5e74c4f7ff082c4d97f1edf880"
            ))
            {
                md = new SHA0();
                outtext = md.Make(data.Item1);
                Debug.Assert(outtext == data.Item2);
            }
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
        private static void Test_Sha2()
        {

        }
    }
}
