namespace NetPs.Socket.Extras.Security.Morse
{
    using System;
    using System.Text;

    public class Morse
    {
        protected virtual IMorseBook book { get; set; }
        public Morse()
        {
            this.book = new StrandardMorseBook();
        }
        public Morse(IMorseBook book)
        {
            this.book = book;
        }

        public byte[] Encode(string text, int length)
        {
            if (length == 0) return null;
            var bytes = new byte[length * 3];
            ulong x;
            ulong total = 0;
            void push(uint n, byte count = 0)
            {
                if (count > 0)
                {
                    total += count;
                }
                else
                {
                    while (n != 0)
                    {
                        x = total >> 3;
                        n >>= 1;
                        if ((n & 0b1) == 0)
                        {
                            bytes[x] |= (byte)(1 << (byte)(7 - total % 8));
                            total += 1;
                            n >>= 1;
                        }
                        else
                        {
                            if (total % 8 == 6)
                            {
                                bytes[x] |= 0b11;
                                bytes[x + 1] |= 0b10000000;
                            }
                            else if (total % 8 == 7)
                            {
                                bytes[x] |= 1;
                                bytes[x + 1] |= 0b11000000;
                            }
                            else
                            {
                                bytes[x] |= (byte)(0b111 << (byte)(5 - total % 8));
                            }
                            total += 3;
                            n >>= 3;
                        }
                        if (n != 0) total++;
                    }
                }
            }
            int i = 1;
            char a = text[0];
            uint w;
            for (; i < length; i++)
            {
                if (a == ' ')
                {
                    push(0, 6);
                    a = text[i];
                    continue;
                }

                w = book.Encode(a, text[i]);
                if (w != 0)
                {
                    push(w);
                    push(0, 3);
                    i++;
                }
                else
                {
                    w = book.Encode(a);
                    if (w != 0)
                    {
                        push(w);
                        push(0, 3);
                    }
                }
                if (i < length) a = text[i];
            }
            if (i == length)
            {
                w = book.Encode(a);
                if (w != 0)
                {
                    push(w);
                }
            }
            var o = new byte[(total >> 3) + 1];
            Array.Copy(bytes, o, (int)(total >> 3) + 1);
            return o;
        }
        public string Decode(byte[] data, int length)
        {
            if (length == 0) return string.Empty;
            var s = new StringBuilder();
            uint j = 0;
            byte no = 0, i;
            uint load = 0;
            uint zero = 0;
            while (load < length)
            {
                for (i = 7; i <= 7; i--)
                {
                    if ((data[load] >> i & 0b1) == 1)
                    {
                        if (zero >= 3)
                        {
                            if (j != 0) book.Decode(j, ref s);
                            if (zero >= 6) s.Append(" ");
                            j = 0;
                            no = 0;
                            zero = 0;
                        }
                        else
                        {
                            no += (byte)zero;
                            zero = 0;
                        }
                        j |= (uint)1 << no;
                        no++;
                    }
                    else
                    {
                        zero++;
                    }
                }
                load++;
            }
            if (j != 0) book.Decode(j, ref s);

            return s.ToString();
        }
    }
}
