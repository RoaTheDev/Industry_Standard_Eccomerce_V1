using System.Security.Cryptography;

namespace Ecommerce_site.Util;

public class OtpGenerator
{
    public uint GenerateSecureOtp() // Generate a number in the range 100000–999999
    {
        using (var rng = RandomNumberGenerator.Create())
        {
            var randomNumber = new byte[4];
            rng.GetBytes(randomNumber);

            var otp = 100000 + BitConverter.ToUInt32(randomNumber, 0) % 900000;
            return otp;
        }
    }
}