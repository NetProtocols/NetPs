namespace NetPs.Socket.Extras.Security.Morse
{
    using System;
    using System.Text;

    public interface IMorseBook
    {
        /// <summary>字符转为morse</summary>
        /// <remarks>
        /// start low to high; e.g. 0b101110101: . . - .
        /// </remarks>
        uint Encode(char c);
        /// <summary>双字符转为morse</summary>
        /// <remarks>
        /// start low to high; e.g. 0b101110101: . . - .
        /// </remarks>
        uint Encode(char c1, char c2);
        /// <summary>morse转为文本</summary>
        /// <remarks>
        /// start low to high; e.g. 0b101110101: . . - .
        /// </remarks>
        void Decode(uint morse, ref StringBuilder s);
    }
}
