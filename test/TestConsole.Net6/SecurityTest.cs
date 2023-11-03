using NetPs.Socket.Extras.Security;
using NetPs.Socket.Extras.Security.Morse;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace TestConsole.Net6
{
    internal class SecurityTest
    {
        internal SecurityTest()
        {
            Test_Morse();
            Test_Hashs();
        }

        private static void Test_Morse()
        {
            var morse = new Morse();
            var text = "hello world! @code: 1234567890abcdefghijklmnopqrstuvwxyz; @value: -=2023;.:,;?='/!-_\"()$@åæ çðéèĝĥĵñöŝþü AR AS SK =";
            var s = morse.Encode(text, text.Length);
            var w = morse.Decode(s, s.Length);
            Debug.Assert(text == w);
        }
        private static void Test_Hashs()
        {
            string outtext;
            foreach (var test in hash_tests)
            {
                IHash? hash = GetHashObj(test.Key);
                Debug.Assert(hash != null);
                if (hash == null) continue;
                foreach(var eg in test.Value)
                {
                    outtext = hash.Make(Encoding.ASCII.GetBytes(eg.Key));
                    Debug.Assert(outtext.ToLower() == eg.Value.ToLower());
                }
            }
        }
        private static readonly IList<Type> HASH_TYPES = Assembly.Load(NETPS_NAMESPACE.NETPS_SOCKET).GetTypes().Where(typ => typ.GetInterfaces().Contains(typeof(IHash))).ToList();
        private static IHash? GetHashObj(string name)
        {
            var typ = HASH_TYPES.FirstOrDefault(typ => typ.Name == name);
            if (typ == null) return null;
            return Activator.CreateInstance(typ) as IHash;
        }
        private static readonly IDictionary<string, Dictionary<string, string>> hash_tests =
        new Dictionary<string, Dictionary<string, string>>
        {
            ["MD2"] = new Dictionary<string, string>
            {
                ["abc"]
                = "da853b0d3f88d99b30283a69e6ded6bb",
                ["ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"]
                = "da33def2a42df13975352846c30338cd",
                ["abcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabc"]
                = "95133809c29ad15c70d4d7fb3c5f9d78"
            },
            ["MD4"] = new Dictionary<string, string>
            {
                ["abc"]
                = "a448017aaf21d8525fc10ae87aa6729d",
                ["ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"]
                = "043f8582f241db351ce627e153e7f0e4"
            },
            ["MD5"] = new Dictionary<string, string>
            {
                ["abc"]
                = "900150983cd24fb0d6963f7d28e17f72",
                ["ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"]
                = "d174ab98d277d9f5a5611c2c9f419d9f",
            },
            ["MD6_128"] = new Dictionary<string, string>
            {
                ["123"]
                = "650cdaa202ca22b1d8d7697f98267ae5",
                ["abc"]
                = "8db50d79cf42fe7d1807ebaa15329c61",
                ["ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"]
                = "a525350f8f57eaa672d26d3fccd6c5e5",
            },
            ["MD6_256"] = new Dictionary<string, string>
            {
                ["123"]
                = "4a56c5d61a2bf080e4bb945b0b6cd8a98e8812749dbc104881e35c35e29202d1",
                ["abc"]
                = "230637d4e6845cf0d092b558e87625f03881dd53a7439da34cf3b94ed0d8b2c5",
                ["ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"]
                = "c95dbfe4347644f14a8697066be24a85bcc296b18b738c058b8fc5a5553d8463",
            },
            ["MD6_512"] = new Dictionary<string, string>
            {
                ["123"]
                = "4054D49A4FEFCE5A1958242FEE07A2A25E831347A351AEF7E28D1E31922E9FC1F1634523BFCFB35DB78292E3B20A3B8F300C6E63C143D88462D4733B520333CC",
                ["abc"]
                = "00918245271E377A7FFB202B90F3BDA5477D8FEAB12D8A3A8994EBC55FE6E74CA8341520032EEEA3FDEF892F2882378F636212AF4B2683CCF80BF025B7D9B457",
                ["ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"]
                = "487acbba2934b087bd7c508e3e9d1a34dc23a69459513ed933f2a74e6d1d3c573d862f4882df1d7049c5ad1ed1989c781386e28d512159a4c3240489a447516b",
            },
            ["SHA0"] = new Dictionary<string, string>
            {
                ["123"]
                = "4f797c608cab4a00501447df8ee8833f363ea7d3",
                ["abc"]
                = "0164b8a914cd2a5e74c4f7ff082c4d97f1edf880",
                ["ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"]
                = "79e966f7a3a990df33e40e3d7f8f18d2caebadfa",
            },
            ["SHA1"] = new Dictionary<string, string>
            {
                ["abc"]
                = "a9993e364706816aba3e25717850c26c9cd0d89d",
                ["ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"]
                = "761c457bf73b14d27e9e9265c46f4b4dda11f940"
            },
            ["SHA224"] = new Dictionary<string, string>
            {
                ["abc"]
                = "23097d223405d8228642a477bda255b32aadbce4bda0b3f7e36c9da7",
                ["ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"]
                = "bff72b4fcb7d75e5632900ac5f90d219e05e97a7bde72e740db393d9"
            },
            ["SHA256"] = new Dictionary<string, string>
            {
                ["abc"]
                = "ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad",
                ["ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"]
                = "db4bfcbd4da0cd85a60c3c37d3fbd8805c77f15fc6b1fdfe614ee0a7c8fdb4c0"
            },
            ["SHA384"] = new Dictionary<string, string>
            {
                ["abc"]
                = "cb00753f45a35e8bb5a03d699ac65007272c32ab0eded1631a8b605a43ff5bed8086072ba1e7cc2358baeca134c825a7",
                ["ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"]
                = "1761336e3f7cbfe51deb137f026f89e01a448e3b1fafa64039c1464ee8732f11a5341a6f41e0c202294736ed64db1a84"
            },
            ["SHA512"] = new Dictionary<string, string>
            {
                ["abc"]
                = "ddaf35a193617abacc417349ae20413112e6fa4e89a97ea20a9eeee64b55d39a2192992a274fc1a836ba3c23a3feebbd454d4423643ce80e2a9ac94fa54ca49f",
                ["ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"]
                = "1e07be23c26a86ea37ea810c8ec7809352515a970e9253c26f536cfc7a9996c45c8370583e0a78fa4a90041d71a4ceab7423f19c71b9d5a3e01249f0bebd5894"
            },
            ["SHA3_224"] = new Dictionary<string, string>
            {
                ["abc"]
                = "e642824c3f8cf24ad09234ee7d3c766fc9a3a5168d0c94ad73b46fdf",
                ["ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"]
                = "a67c289b8250a6f437a20137985d605589a8c163d45261b15419556e",
                ["12345678901234567890123456789012345678901234567890123456789012345678901234567890"]
                = "0526898e185869f91b3e2a76dd72a15dc6940a67c8164a044cd25cc8",
            },
            ["SHA3_256"] = new Dictionary<string, string>
            {
                ["abc"]
                = "3a985da74fe225b2045c172d6bd390bd855f086e3e9d525b46bfe24511431532",
                ["ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"]
                = "a79d6a9da47f04a3b9a9323ec9991f2105d4c78a7bc7beeb103855a7a11dfb9f",
                ["12345678901234567890123456789012345678901234567890123456789012345678901234567890"]
                = "293e5ce4ce54ee71990ab06e511b7ccd62722b1beb414f5ff65c8274e0f5be1d",
            },
            ["SHA3_384"] = new Dictionary<string, string>
            {
                ["abc"]
                = "ec01498288516fc926459f58e2c6ad8df9b473cb0fc08c2596da7cf0e49be4b298d88cea927ac7f539f1edf228376d25",
                ["ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"]
                = "d5b972302f5080d0830e0de7b6b2cf383665a008f4c4f386a61112652c742d20cb45aa51bd4f542fc733e2719e999291",
                ["12345678901234567890123456789012345678901234567890123456789012345678901234567890"]
                = "3c213a17f514638acb3bf17f109f3e24c16f9f14f085b52a2f2b81adc0db83df1a58db2ce013191b8ba72d8fae7e2a5e",
            },
            ["SHA3_512"] = new Dictionary<string, string>
            {
                ["abc"]
                = "b751850b1a57168a5693cd924b6b096e08f621827444f70d884f5d0240d2712e10e116e9192af3c91a7ec57647e3934057340b4cf408d5a56592f8274eec53f0",
                ["ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"]
                = "d1db17b4745b255e5eb159f66593cc9c143850979fc7a3951796aba80165aab536b46174ce19e3f707f0e5c6487f5f03084bc0ec9461691ef20113e42ad28163",
                ["12345678901234567890123456789012345678901234567890123456789012345678901234567890"]
                = "9524b9a5536b91069526b4f6196b7e9475b4da69e01f0c855797f224cd7335ddb286fd99b9b32ffe33b59ad424cc1744f6eb59137f5fb8601932e8a8af0ae930",
            },
            ["SHA3_SHAKE128"] = new Dictionary<string, string>
            {
                ["abc"]
                = "5881092dd818bf5cf8a3ddb793fbcba7",
                ["ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"]
                = "54dd201e53249910db3c7d366574fbb6",
                ["12345678901234567890123456789012345678901234567890123456789012345678901234567890"]
                = "7bf451c92fdc77b9771e6c9056445894",
            },
            ["SHA3_SHAKE256"] = new Dictionary<string, string>
            {
                ["abc"]
                = "483366601360a8771c6863080cc4114d8db44530f8f1e1ee4f94ea37e78b5739",
                ["ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"]
                = "31f19a097c723e91fa59b0998dd8523c2a9e7e13b4025d6b48fcbc328973a108",
                ["12345678901234567890123456789012345678901234567890123456789012345678901234567890"]
                = "24c508adefdf5e3f2596e8b5a888fe10eb7b5b22e1f35d858e6eff3025c4cc18",
            },
            ["RIPEMD128"] = new Dictionary<string, string>
            {
                ["abc"]
                = "c14a12199c66e4ba84636b0f69144c77",
                ["ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"]
                = "d1e959eb179c911faea4624c60c5c702",
            },
            ["RIPEMD160"] = new Dictionary<string, string>
            {
                ["abc"]
                = "8eb208f7e05d987a9b044a8e98c6b087f15a0bfc",
                ["ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"]
                = "b0e20b6e3116640286ed3a87a5713079b21f5189",
            },
            ["RIPEMD256"] = new Dictionary<string, string>
            {
                ["abc"]
                = "afbd6e228b9d8cbbcef5ca2d03e6dba10ac0bc7dcbe4680e1e42d2e975459b65",
                ["ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"]
                = "5740a408ac16b720b84424ae931cbb1fe363d1d0bf4017f1a89f7ea6de77a0b8",
            },
            ["RIPEMD320"] = new Dictionary<string, string>
            {
                ["abc"]
                = "de4c01b3054f8930a79d09ae738e92301e5a17085beffdc1b8d116713e74f82fa942d64cdbc4682d",
                ["ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"]
                = "ed544940c86d67f250d232c30b7b3e5770e0c60c8cb9a4cafe3b11388af9920e1b99230b843c86a4",
            },
            ["WHIRLPOOL"] = new Dictionary<string, string>
            {
                ["abc"]
                = "4e2448a4c6f486bb16b6562c73b4020bf3043e3a731bce721ae1b303d97e6d4c7181eebdb6c57e277d0e34957114cbd6c797fc9d95d8b582d225292076d4eef5",
                ["ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"]
                = "dc37e008cf9ee69bf11f00ed9aba26901dd7c28cdec066cc6af42e40f82f3a1e08eba26629129d8fb7cb57211b9281a65517cc879d7b962142c65f5a7af01467",
            },
            ["TIGER192_3"] = new Dictionary<string, string>
            {
                ["abc"]
                = "2aab1484e8c158f2bfb8c5ff41b57a525129131c957b5f93",
                ["ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"]
                = "8dcea680a17583ee502ba38a3c368651890ffbccdc49a8cc",
            },
            ["TIGER192_4"] = new Dictionary<string, string>
            {
                ["abc"]
                = "538883c8fc5f28250299018e66bdf4fdb5ef7b65f2e91753",
                ["ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"]
                = "ac2ca58530529697d1ca33b191203d111b73ab1884f16e06",
            },
            ["SNEFRU128"] = new Dictionary<string, string>
            {
                ["abc"]
                = "553d0648928299a0f22a275a02c83b10",
                ["ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"]
                = "0efd7f93a549f023b79781090458923e",
            },
            ["SNEFRU256"] = new Dictionary<string, string>
            {
                ["abc"]
                = "7d033205647a2af3dc8339f6cb25643c33ebc622d32979c4b612b02c4903031b",
                ["ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"]
                = "83aa9193b62ffd269faa43d31e6ac2678b340e2a85849470328be9773a9e5728",
            },
            ["GOST94"] = new Dictionary<string, string>
            {
                ["abc"]
                = "F3134348C44FB1B2A277729E2285EBB5CB5E0F29C975BC753B70497C06A4D51D",
                ["ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"]
                = "95C1AF627C356496D80274330B2CFF6A10C67B5F597087202F94D06D2338CF8E",
                ["UUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUU"]
                = "53A3A3ED25180CEF0C1D85A074273E551C25660A87062A52D926A9E8FE5733A4",
            },
            ["GOST94_CRYPT"] = new Dictionary<string, string>
            {
                ["abc"]
                = "B285056DBF18D7392D7677369524DD14747459ED8143997E163B2986F92FD42C",
                ["ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"]
                = "73B70A39497DE53A6E08C67B6D4DB853540F03E9389299D9B0156EF7E85D0F61",
                ["UUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUU"]
                = "1C4AC7614691BBF427FA2316216BE8F10D92EDFD37CD1027514C1008F649C4E8",
            },
            ["GOST2012_256"] = new Dictionary<string, string>
            {
                ["abc"]
                = "4E2919CF137ED41EC4FB6270C61826CC4FFFB660341E0AF3688CD0626D23B481",
                ["ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"]
                = "74F945F366AB17DD1E7D114AB9ADF68B97A8D6A1CBBE299CBA06B77735457F94",
                ["12345678901234567890123456789012345678901234567890123456789012345678901234567890"]
                = "2C6644B1F5AB3E0AB56ADF1FEEB4D6A8742FCFC61B53B69C3B536AC283AB88AA",
            },
            ["GOST2012_512"] = new Dictionary<string, string>
            {
                ["abc"]
                = "28156E28317DA7C98F4FE2BED6B542D0DAB85BB224445FCEDAF75D46E26D7EB8D5997F3E0915DD6B7F0AAB08D9C8BEB0D8C64BAE2AB8B3C8C6BC53B3BF0DB728",
                ["ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"]
                = "CC49B68C195D18D3FEF26F3D4A6554DB62298B96D19FBEFA52A139E8558D0528535569EBDEA172692857EDE3351C02FE9D749EF7273DCECACA5B3E295511650B",
                ["12345678901234567890123456789012345678901234567890123456789012345678901234567890"]
                = "116201023E88D93A4D076BA77207E8702C6CFA6FCC69B82BB22AE6BE9B63F16B19BAAF8771E01E6DC25C2B4486FA3BBF8601905762CBBAD5BA25A1E034879192",
            },
            ["SM3"] = new Dictionary<string, string>
            {
                ["a"]
                = "623476ac18f65a2909e43c7fec61b49c7e764a91a18ccb82f1917a29c86c5e88",
                ["abc"]
                = "66c7f0f462eeedd9d1f2d46bdc10e4e24167c4875cf2f7a2297da02b8f4ba8e0",
                ["abcdefghijklmnopqrstuvwxyz"]
                = "b80fe97a4da24afc277564f66a359ef440462ad28dcc6d63adb24d5c20a61595",
                ["ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"]
                = "2971d10c8842b70c979e55063480c50bacffd90e98e2e60d2512ab8abfdfcec5",
                ["12345678901234567890123456789012345678901234567890123456789012345678901234567890"]
                = "ad81805321f3e69d251235bf886a564844873b56dd7dde400f055b7dde39307a",
            }
        };
    }
}