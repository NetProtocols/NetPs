using NetPs.Socket.Extras.Security.MessageDigest;
using NetPs.Socket.Extras.Security.SecureHash;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace TestConsole.Net6
{
    internal class SecurityTest
    {
        internal SecurityTest()
        {
            //Test_MD6();
            Test_Sha0();
            Test_Sha1();
            Test_Sha2();
            Test_Sha3();
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
            string text, outtext;
            text = "abc";

            outtext = new SHA224().Make(Encoding.ASCII.GetBytes(text));
            Debug.Assert(outtext == "23097d223405d8228642a477bda255b32aadbce4bda0b3f7e36c9da7");
            outtext = new SHA256().Make(Encoding.ASCII.GetBytes(text));
            Debug.Assert(outtext == "ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad");
            outtext = new SHA384().Make(Encoding.ASCII.GetBytes(text));
            Debug.Assert(outtext == "cb00753f45a35e8bb5a03d699ac65007272c32ab0eded1631a8b605a43ff5bed8086072ba1e7cc2358baeca134c825a7");
            outtext = new SHA512().Make(Encoding.ASCII.GetBytes(text));
            Debug.Assert(outtext == "ddaf35a193617abacc417349ae20413112e6fa4e89a97ea20a9eeee64b55d39a2192992a274fc1a836ba3c23a3feebbd454d4423643ce80e2a9ac94fa54ca49f");
        }

        private static void Test_Sha3()
        {
            string text, outtext;
            text = "abc";

            outtext = new SHA3_224().Make(Encoding.ASCII.GetBytes(text));
            Debug.Assert(outtext == "e642824c3f8cf24ad09234ee7d3c766fc9a3a5168d0c94ad73b46fdf");
            outtext = new SHA3_256().Make(Encoding.ASCII.GetBytes(text));
            Debug.Assert(outtext == "3a985da74fe225b2045c172d6bd390bd855f086e3e9d525b46bfe24511431532");
            outtext = new SHA3_384().Make(Encoding.ASCII.GetBytes(text));
            Debug.Assert(outtext == "ec01498288516fc926459f58e2c6ad8df9b473cb0fc08c2596da7cf0e49be4b298d88cea927ac7f539f1edf228376d25");
            outtext = new SHA3_512().Make(Encoding.ASCII.GetBytes(text));
            Debug.Assert(outtext == "b751850b1a57168a5693cd924b6b096e08f621827444f70d884f5d0240d2712e10e116e9192af3c91a7ec57647e3934057340b4cf408d5a56592f8274eec53f0");

            outtext = new SHA3_SHAKE128().Make(Encoding.ASCII.GetBytes(text));
            Debug.Assert(outtext == "5881092dd818bf5cf8a3ddb793fbcba7");
            outtext = new SHA3_SHAKE256().Make(Encoding.ASCII.GetBytes(text));
            Debug.Assert(outtext == "483366601360a8771c6863080cc4114d8db44530f8f1e1ee4f94ea37e78b5739");
        }
    }
}
