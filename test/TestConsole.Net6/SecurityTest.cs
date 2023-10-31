using NetPs.Socket.Extras.Security.GuoMi;
using NetPs.Socket.Extras.Security.MessageDigest;
using NetPs.Socket.Extras.Security.Morse;
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
            Test_Morse();
            //Test_MD6();
            Test_Sha0();
            Test_Sha1();
            Test_Sha2();
            Test_Sha3();
            Test_SM3();
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
        private static void Test_Morse()
        {
            var morse = new Morse();
            var text = "hello world! @code: 1234567890abcdefghijklmnopqrstuvwxyz; @value: -=2023;.:,;?='/!-_\"()$@åæ çðéèĝĥĵñöŝþü AR AS SK =";
            var s = morse.Encode(text, text.Length);
            var w = morse.Decode(s, s.Length);
            Debug.Assert(text == w);
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
            text = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

            outtext = new SHA3_224().Make(Encoding.ASCII.GetBytes(text));
            Debug.Assert(outtext == "a67c289b8250a6f437a20137985d605589a8c163d45261b15419556e");
            outtext = new SHA3_256().Make(Encoding.ASCII.GetBytes(text));
            Debug.Assert(outtext == "a79d6a9da47f04a3b9a9323ec9991f2105d4c78a7bc7beeb103855a7a11dfb9f");
            outtext = new SHA3_384().Make(Encoding.ASCII.GetBytes(text));
            Debug.Assert(outtext == "d5b972302f5080d0830e0de7b6b2cf383665a008f4c4f386a61112652c742d20cb45aa51bd4f542fc733e2719e999291");
            outtext = new SHA3_512().Make(Encoding.ASCII.GetBytes(text));
            Debug.Assert(outtext == "d1db17b4745b255e5eb159f66593cc9c143850979fc7a3951796aba80165aab536b46174ce19e3f707f0e5c6487f5f03084bc0ec9461691ef20113e42ad28163");

            outtext = new SHA3_SHAKE128().Make(Encoding.ASCII.GetBytes(text));
            Debug.Assert(outtext == "54dd201e53249910db3c7d366574fbb6");
            outtext = new SHA3_SHAKE256().Make(Encoding.ASCII.GetBytes(text));
            Debug.Assert(outtext == "31f19a097c723e91fa59b0998dd8523c2a9e7e13b4025d6b48fcbc328973a108");
            text = "12345678901234567890123456789012345678901234567890123456789012345678901234567890";

            outtext = new SHA3_224().Make(Encoding.ASCII.GetBytes(text));
            Debug.Assert(outtext == "0526898e185869f91b3e2a76dd72a15dc6940a67c8164a044cd25cc8");
            outtext = new SHA3_256().Make(Encoding.ASCII.GetBytes(text));
            Debug.Assert(outtext == "293e5ce4ce54ee71990ab06e511b7ccd62722b1beb414f5ff65c8274e0f5be1d");
            outtext = new SHA3_384().Make(Encoding.ASCII.GetBytes(text));
            Debug.Assert(outtext == "3c213a17f514638acb3bf17f109f3e24c16f9f14f085b52a2f2b81adc0db83df1a58db2ce013191b8ba72d8fae7e2a5e");
            outtext = new SHA3_512().Make(Encoding.ASCII.GetBytes(text));
            Debug.Assert(outtext == "9524b9a5536b91069526b4f6196b7e9475b4da69e01f0c855797f224cd7335ddb286fd99b9b32ffe33b59ad424cc1744f6eb59137f5fb8601932e8a8af0ae930");

            outtext = new SHA3_SHAKE128().Make(Encoding.ASCII.GetBytes(text));
            Debug.Assert(outtext == "7bf451c92fdc77b9771e6c9056445894");
            outtext = new SHA3_SHAKE256().Make(Encoding.ASCII.GetBytes(text));
            Debug.Assert(outtext == "24c508adefdf5e3f2596e8b5a888fe10eb7b5b22e1f35d858e6eff3025c4cc18");
        }

        private static void Test_SM3()
        {
            string text, outtext;
            text = "abc";

            outtext = new SM3().Make(Encoding.ASCII.GetBytes(text));
            Debug.Assert(outtext == "66c7f0f462eeedd9d1f2d46bdc10e4e24167c4875cf2f7a2297da02b8f4ba8e0");
            text = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

            outtext = new SM3().Make(Encoding.ASCII.GetBytes(text));
            Debug.Assert(outtext == "2971d10c8842b70c979e55063480c50bacffd90e98e2e60d2512ab8abfdfcec5");
        }
    }
}
