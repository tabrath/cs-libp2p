using System;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using Multiformats.Hash;
using Multiformats.Hash.Algorithms;

namespace LibP2P.Crypto
{
    internal static class Utils
    {
        /// <summary>
        /// Computes the hash value of a byte array using SHA2 256 bit
        /// </summary>
        /// <param name="bytes">bytes to hash</param>
        /// <returns>hash digest</returns>
        public static byte[] Hash(byte[] bytes) => Multihash.Sum<SHA2_256>(bytes);

        /// <summary>
        /// Convert RSA public key string to RSA parameters
        /// </summary>
        /// <param name="s">input</param>
        /// <returns>RSA public key parameters</returns>
        public static RsaKeyParameters UnmarshalPKIXPublicKey(string s) => UnmarshalPKIXPublicKey(Convert.FromBase64String(s));

        /// <summary>
        /// Convert RSA public key bytes to RSA parameters
        /// </summary>
        /// <param name="bytes">input</param>
        /// <returns>RSA public key parameters</returns>
        public static RsaKeyParameters UnmarshalPKIXPublicKey(byte[] bytes) => (RsaKeyParameters)PublicKeyFactory.CreateKey(bytes);

        /// <summary>
        /// Convert RSA public key parameters to bytes
        /// </summary>
        /// <param name="parameters">RSA public key parameters</param>
        /// <returns>bytes</returns>
        public static byte[] MarshalPKIXPublicKey(RsaKeyParameters parameters) => SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(parameters).GetDerEncoded();

        /// <summary>
        /// Convert RSA private key string to RSA parameters
        /// </summary>
        /// <param name="s">input</param>
        /// <returns>RSA private key parameters</returns>
        public static RsaPrivateCrtKeyParameters UnmarshalPKCS1PrivateKey(string s) => UnmarshalPKCS1PrivateKey(Convert.FromBase64String(s));

        /// <summary>
        /// Convert RSA private key bytes to RSA parameters
        /// </summary>
        /// <param name="bytes">input</param>
        /// <returns>RSA private key parameters</returns>
        public static RsaPrivateCrtKeyParameters UnmarshalPKCS1PrivateKey(byte[] bytes)
        {
            var obj = Asn1Object.FromByteArray(bytes);
            var pk = RsaPrivateKeyStructure.GetInstance(obj);

            return new RsaPrivateCrtKeyParameters(pk.Modulus, pk.PublicExponent, pk.PrivateExponent, pk.Prime1,
                pk.Prime2, pk.Exponent1, pk.Exponent2, pk.Coefficient);
        }

        /// <summary>
        /// Convert RSA private key to bytes
        /// </summary>
        /// <param name="p">RSA private key parameters</param>
        /// <returns>bytes</returns>
        public static byte[] MarshalPKCS1PrivateKey(RsaPrivateCrtKeyParameters p)
        {
            var key = new RsaPrivateKeyStructure(p.Modulus, p.PublicExponent, p.Exponent, p.P, p.Q, p.DP, p.DQ, p.QInv);
            var seq = Asn1Sequence.GetInstance(key);
            return seq.GetDerEncoded();
        }
    }
}