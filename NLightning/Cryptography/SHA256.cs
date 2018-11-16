namespace NLightning.Cryptography
{
    public class SHA256
    {
        public static byte[] ComputeHash(byte[] data)  
        {  
            using (System.Security.Cryptography.SHA256 sha256Hash = System.Security.Cryptography.SHA256.Create())  
            {  
               return sha256Hash.ComputeHash(data);  
            }  
        }
    }
}