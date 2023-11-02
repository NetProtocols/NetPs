using NetPs.Socket.Extras.Security.GuoMi;
using NetPs.Socket.Extras.Security.MessageDigest;
using NetPs.Socket.Extras.Security.Morse;
using NetPs.Socket.Extras.Security.OtherHash;
using NetPs.Socket.Extras.Security.SecureHash;
using System;
using System.Diagnostics;
using System.Text;

namespace TestConsole.Net6
{
    internal class SecurityTest
    {
        internal SecurityTest()
        {
            Test_Morse();
            Test_MD2();
            Test_MD4();
            Test_MD5();
            Test_MD6();

            Test_Sha0();
            Test_Sha1();
            Test_Sha2();
            Test_Sha3();
            Test_SM3();
            Test_RIPEMD();
            Test_WHIRLPOOL();
            Test_TIGER();
            Test_SNEFRU();
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
        private static void Test_MD2()
        {
            string text, outtext;
            text = "abc";

            outtext = new MD2().Make(Encoding.ASCII.GetBytes(text));
            Debug.Assert(outtext == "da853b0d3f88d99b30283a69e6ded6bb");
            text = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

            outtext = new MD2().Make(Encoding.ASCII.GetBytes(text));
            Debug.Assert(outtext == "da33def2a42df13975352846c30338cd");
            text = "abcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabc";

            outtext = new MD2().Make(Encoding.ASCII.GetBytes(text));
            Debug.Assert(outtext == "95133809c29ad15c70d4d7fb3c5f9d78");
        }
        private static void Test_MD4()
        {
            string text, outtext;
            text = "abc";

            outtext = new MD4().Make(Encoding.ASCII.GetBytes(text));
            Debug.Assert(outtext == "a448017aaf21d8525fc10ae87aa6729d");
            text = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

            outtext = new MD4().Make(Encoding.ASCII.GetBytes(text));
            Debug.Assert(outtext == "043f8582f241db351ce627e153e7f0e4");

        }
        private static void Test_MD5()
        {
            string text, outtext;
            text = "abc";

            outtext = new MD5().Make(Encoding.ASCII.GetBytes(text));
            Debug.Assert(outtext == "900150983cd24fb0d6963f7d28e17f72");
            text = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

            outtext = new MD5().Make(Encoding.ASCII.GetBytes(text));
            Debug.Assert(outtext == "d174ab98d277d9f5a5611c2c9f419d9f");
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
            text = "abc";
            outtext = new SHA1().Make(Encoding.ASCII.GetBytes(text));
            Debug.Assert(outtext == "a9993e364706816aba3e25717850c26c9cd0d89d");
            text = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            outtext = new SHA1().Make(Encoding.ASCII.GetBytes(text));
            Debug.Assert(outtext == "761c457bf73b14d27e9e9265c46f4b4dda11f940");
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
            text = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

            outtext = new SHA224().Make(Encoding.ASCII.GetBytes(text));
            Debug.Assert(outtext == "bff72b4fcb7d75e5632900ac5f90d219e05e97a7bde72e740db393d9");
            outtext = new SHA256().Make(Encoding.ASCII.GetBytes(text));
            Debug.Assert(outtext == "db4bfcbd4da0cd85a60c3c37d3fbd8805c77f15fc6b1fdfe614ee0a7c8fdb4c0");
            outtext = new SHA384().Make(Encoding.ASCII.GetBytes(text));
            Debug.Assert(outtext == "1761336e3f7cbfe51deb137f026f89e01a448e3b1fafa64039c1464ee8732f11a5341a6f41e0c202294736ed64db1a84");
            outtext = new SHA512().Make(Encoding.ASCII.GetBytes(text));
            Debug.Assert(outtext == "1e07be23c26a86ea37ea810c8ec7809352515a970e9253c26f536cfc7a9996c45c8370583e0a78fa4a90041d71a4ceab7423f19c71b9d5a3e01249f0bebd5894");

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
        private static void Test_RIPEMD()
        {
            string text, outtext;

            text = "abc";
            outtext = new RIPEMD128().Make(Encoding.ASCII.GetBytes(text));
            Debug.Assert(outtext == "c14a12199c66e4ba84636b0f69144c77");
            outtext = new RIPEMD160().Make(Encoding.ASCII.GetBytes(text));
            Debug.Assert(outtext == "8eb208f7e05d987a9b044a8e98c6b087f15a0bfc");
            outtext = new RIPEMD256().Make(Encoding.ASCII.GetBytes(text));
            Debug.Assert(outtext == "afbd6e228b9d8cbbcef5ca2d03e6dba10ac0bc7dcbe4680e1e42d2e975459b65");
            outtext = new RIPEMD320().Make(Encoding.ASCII.GetBytes(text));
            Debug.Assert(outtext == "de4c01b3054f8930a79d09ae738e92301e5a17085beffdc1b8d116713e74f82fa942d64cdbc4682d");

            text = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            outtext = new RIPEMD128().Make(Encoding.ASCII.GetBytes(text));
            Debug.Assert(outtext == "d1e959eb179c911faea4624c60c5c702");
            outtext = new RIPEMD160().Make(Encoding.ASCII.GetBytes(text));
            Debug.Assert(outtext == "b0e20b6e3116640286ed3a87a5713079b21f5189");
            outtext = new RIPEMD256().Make(Encoding.ASCII.GetBytes(text));
            Debug.Assert(outtext == "5740a408ac16b720b84424ae931cbb1fe363d1d0bf4017f1a89f7ea6de77a0b8");
            outtext = new RIPEMD320().Make(Encoding.ASCII.GetBytes(text));
            Debug.Assert(outtext == "ed544940c86d67f250d232c30b7b3e5770e0c60c8cb9a4cafe3b11388af9920e1b99230b843c86a4");
        }
        private static void Test_WHIRLPOOL()
        {
            string text, outtext;
            text = "abc";
            outtext = new WHIRLPOOL().Make(Encoding.ASCII.GetBytes(text));
            Debug.Assert(outtext == "4e2448a4c6f486bb16b6562c73b4020bf3043e3a731bce721ae1b303d97e6d4c7181eebdb6c57e277d0e34957114cbd6c797fc9d95d8b582d225292076d4eef5");

            text = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            outtext = new WHIRLPOOL().Make(Encoding.ASCII.GetBytes(text));
            Debug.Assert(outtext == "dc37e008cf9ee69bf11f00ed9aba26901dd7c28cdec066cc6af42e40f82f3a1e08eba26629129d8fb7cb57211b9281a65517cc879d7b962142c65f5a7af01467");

        }
        private static void Test_TIGER()
        {
            string text, outtext;
            text = "abc";
            outtext = new TIGER192_3().Make(Encoding.ASCII.GetBytes(text));
            Debug.Assert(outtext == "2aab1484e8c158f2bfb8c5ff41b57a525129131c957b5f93");
            outtext = new TIGER192_4().Make(Encoding.ASCII.GetBytes(text));
            Debug.Assert(outtext == "538883c8fc5f28250299018e66bdf4fdb5ef7b65f2e91753");
            text = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            outtext = new TIGER192_3().Make(Encoding.ASCII.GetBytes(text));
            Debug.Assert(outtext == "8dcea680a17583ee502ba38a3c368651890ffbccdc49a8cc");
            outtext = new TIGER192_4().Make(Encoding.ASCII.GetBytes(text));
            Debug.Assert(outtext == "ac2ca58530529697d1ca33b191203d111b73ab1884f16e06");
        }
        private static void Test_SNEFRU()
        {
            string text, outtext;
            text = "abc";
            outtext = new SNEFRU128().Make(Encoding.ASCII.GetBytes(text));
            Debug.Assert(outtext == "553d0648928299a0f22a275a02c83b10");
            outtext = new SNEFRU256().Make(Encoding.ASCII.GetBytes(text));
            Debug.Assert(outtext == "7d033205647a2af3dc8339f6cb25643c33ebc622d32979c4b612b02c4903031b");
            text = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            outtext = new SNEFRU128().Make(Encoding.ASCII.GetBytes(text));
            Debug.Assert(outtext == "0efd7f93a549f023b79781090458923e");
            outtext = new SNEFRU256().Make(Encoding.ASCII.GetBytes(text));
            Debug.Assert(outtext == "83aa9193b62ffd269faa43d31e6ac2678b340e2a85849470328be9773a9e5728");
        }
        private static void Test_SM3()
        {
            string text, outtext;
            
            text = "a";
            outtext = new SM3().Make(Encoding.ASCII.GetBytes(text));
            Debug.Assert(outtext == "623476ac18f65a2909e43c7fec61b49c7e764a91a18ccb82f1917a29c86c5e88");
            
            text = "abc";
            outtext = new SM3().Make(Encoding.ASCII.GetBytes(text));
            Debug.Assert(outtext == "66c7f0f462eeedd9d1f2d46bdc10e4e24167c4875cf2f7a2297da02b8f4ba8e0");
            
            text = "abcdefghijklmnopqrstuvwxyz";
            outtext = new SM3().Make(Encoding.ASCII.GetBytes(text));
            Debug.Assert(outtext == "b80fe97a4da24afc277564f66a359ef440462ad28dcc6d63adb24d5c20a61595");

            text = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            outtext = new SM3().Make(Encoding.ASCII.GetBytes(text));
            Debug.Assert(outtext == "2971d10c8842b70c979e55063480c50bacffd90e98e2e60d2512ab8abfdfcec5");

            text = "12345678901234567890123456789012345678901234567890123456789012345678901234567890";
            outtext = new SM3().Make(Encoding.ASCII.GetBytes(text));
            Debug.Assert(outtext == "ad81805321f3e69d251235bf886a564844873b56dd7dde400f055b7dde39307a");
        }
    }
}
