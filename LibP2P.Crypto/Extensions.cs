using Org.BouncyCastle.Crypto;

namespace LibP2P.Crypto
{
    internal static class Extensions
    {
        /// <summary>
        /// Sign bytes with the given signer
        /// </summary>
        /// <param name="signer">the signer to use</param>
        /// <param name="data">input data</param>
        /// <returns>signature</returns>
        public static byte[] Sign(this ISigner signer, byte[] data)
        {
            signer.Reset();
            signer.BlockUpdate(data, 0, data.Length);
            return signer.GenerateSignature();
        }

        /// <summary>
        /// Verify data with the given signer and signature
        /// </summary>
        /// <param name="signer">the signer to use</param>
        /// <param name="data">input data</param>
        /// <param name="signature">signature</param>
        /// <returns>validity</returns>
        public static bool Verify(this ISigner signer, byte[] data, byte[] signature)
        {
            signer.Reset();
            signer.BlockUpdate(data, 0, data.Length);
            return signer.VerifySignature(signature);
        }
    }
}
