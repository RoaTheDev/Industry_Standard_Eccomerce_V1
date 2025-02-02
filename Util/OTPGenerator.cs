using System.Security.Cryptography;

namespace Ecommerce_site.Util;

public class OtpGenerator
{
    public uint GenerateSecureOtp() // Generate a number in the range 100000–999999
    {
        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            byte[] randomNumber = new byte[4];
            rng.GetBytes(randomNumber);

            uint otp = 100000 + (BitConverter.ToUInt32(randomNumber, 0) % 900000);
            return otp;
        }
    }
}