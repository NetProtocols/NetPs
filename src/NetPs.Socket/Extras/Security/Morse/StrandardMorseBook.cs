namespace NetPs.Socket.Extras.Security.Morse
{
    using System;
    using System.Text;

    public class StrandardMorseBook : IMorseBook
    {
        private static readonly uint[] BookENG = { 0b11101, 0b101010111, 0b10111010111, 0b1010111, 0b1, 0b101110101, 0b101110111, 0b1010101, 0b101, 0b1110111011101, 0b111010111, 0b101011101, 0b1110111, 0b10111, 0b11101110111, 0b10111011101, 0b1110101110111, 0b1011101, 0b10101, 0b111, 0b1110101, 0b111010101, 0b111011101, 0b11101010111, 0b1110111010111, 0b10101110111 };
        private static readonly uint[] BookNUM = { 0b1110111011101110111, 0b11101110111011101, 0b111011101110101, 0b1110111010101, 0b11101010101, 0b101010101, 0b10101010111, 0b1010101110111, 0b101011101110111, 0b10111011101110111 };
        private static readonly uint[] BookFH = { 0b11101011101011101, 0b10101011101110111, 0b1110111010101110111, 0b10111010111010111, 0b101011101110101, 0b1110101010111, 0b1011101110111011101, 0b1011101010111, 0b1110111010111010111, 0b111010101010111, 0b11101011101110101, 0b101110101011101, 0b1110101110111010111, 0b11101010111010101, 0b10111010111011101 };
        private static readonly uint[] BookSPEC = { 0b111010111011101, 0b11101011101, 0b111011101110111, 0b1010111010111, 0b1011101110101, 0b10101110101, 0b1110101011101, 0b101110101110111, 0b101110111010111, 0b101110111011101, 0b11101110101110111, 0b1011101110111, 0b10111010101, 0b1010111011101, 0b11101110101 };
        private const uint AR = 0b1011101011101;
        private const uint AS = 0b10101011101;
        private const uint SK = 0b111010111010101;
        private const uint BT = 0b1110101010111;
        private const string FH = ".:,;?='/!-_\"($@";
        private const string SPEC = "åæ çðéèĝĥĵñöŝþü";
        private bool kuohao_times = true;
        public uint Encode(char c)
        {
            var code = (ushort)c;
            if (code >= 65 && code <= 90)
            {
                return BookENG[code - 65];
            }
            else if (code >= 97 && code <= 122)
            {
                return BookENG[code - 97];
            }
            else if (code >= 48 && code <= 57)
            {
                return BookNUM[code - 48];
            }
            var i = FH.IndexOf(c);
            if (i != -1) return BookFH[i];
            i = SPEC.IndexOf(c);
            if (i != -1) return BookSPEC[i];
            switch (code)
            {
                case ')': return BookFH[12];
                case 'à': return BookSPEC[0];
                case 'ä': return BookSPEC[1];
                case 'ĉ': return BookSPEC[3];
                case 'ø': return BookSPEC[11];
                case 'ŭ': return BookSPEC[14];
            }
            return 0;
        }
        public uint Encode(char c1, char c2)
        {
            if (c1 == 'c' && c2 == 'h') return BookSPEC[2];
            else if (c1 == 'A' && c2 == 'R') return AR;
            else if (c1 == 'A' && c2 == 'S') return AS;
            else if (c1 == 'S' && c2 == 'K') return SK;
            else if (c1 == 'B' && c2 == 'T') return BT;
            return 0;
        }
        public void Decode(uint morse, ref StringBuilder s)
        {
            byte i = 0;
            for (; i < 26; i++)
            {
                if (BookENG[i] == morse)
                {
                    s.Append((char)(i + 97));
                    return;
                }
            }
            for (i = 0; i < 10; i++)
            {
                if (BookNUM[i] == morse)
                {
                    s.Append((char)(i + 48));
                    return;
                }
            }
            for (i = 0; i < 15; i++)
            {
                if (BookFH[i] == morse)
                {
                    if (i == 12)
                    {
                        kuohao_times = !kuohao_times;
                        if (kuohao_times)
                        {
                            s.Append(")");
                            return;
                        }
                    }
                    s.Append(FH[i]);
                    return;
                }
            }
            for (i = 0; i < 15; i++)
            {
                if (BookSPEC[i] == morse)
                {
                    if (i == 2)
                    {
                        s.Append("ch");
                        return;
                    }
                    s.Append(SPEC[i]);
                    return;
                }
            }
            switch (morse)
            {
                case AR: s.Append("AR"); break;
                case AS: s.Append("AS"); break;
                case SK: s.Append("SK"); break;
                case BT: s.Append("BT"); break;
            }
        }
    }
}
