namespace NetPs.Socket.Extras.Security.GuoMi
{
    using System;
    /// <summary>
    /// https://sca.gov.cn/sca/xwdt/2010-12/17/1002386/files/b791a9f908bb4803875ab6aeeb7b4e03.pdf
    /// </summary>
    internal class SM2
    {
        internal static readonly ulong[] P =
        {
            0xffffffff, 0xffffffff, 0x00000000, 0xffffffff,
            0xffffffff, 0xffffffff, 0xffffffff, 0xfffffffe,
        };
        internal static readonly ulong[] B =
        {
            0x4d940e93, 0xddbcbd41, 0x15ab8f92, 0xf39789f5,
            0xcf6509a7, 0x4d5a9e4b, 0x9d9f5e34, 0x28e9fa9e,
        };
        internal static readonly ulong[] G =
        {
            0x334c74c7, 0x715a4589, 0xf2660be1, 0x8fe30bbf,
            0x6a39c994, 0x5f990446, 0x1f198119, 0x32c4ae2c,
            0x2139f0a0, 0x02df32e5, 0xc62a4740, 0xd0a9877c,
            0x6b692153, 0x59bdcee3, 0xf4f6779c, 0xbc3736a2,
            1, 0, 0, 0, 0, 0, 0, 0,
        };
        internal static readonly ulong[] N = {
            0x39d54123, 0x53bbf409, 0x21c6052b, 0x7203df6b,
            0xffffffff, 0xffffffff, 0xffffffff, 0xfffffffe,
        };
    }
}
