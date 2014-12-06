using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Shared
{
public class Token
{
  public Token(string subscription, string key)
  {
    Subscription = subscription;
    MAC = GenerateMAC(subscription, key);
  }

  public Token()
  {

  }

  public string Subscription { get; set; }
  public string MAC { get; set; }

  private static string GenerateMAC(string subscription, string key)
  {
    HMACSHA1 hmac = new HMACSHA1(Encoding.ASCII.GetBytes(key));
    byte[] hmacBytes = hmac.ComputeHash(Encoding.ASCII.GetBytes(subscription));
    return Convert.ToBase64String(hmacBytes);
  }

  public string Serialize()
  {
    return JsonConvert.SerializeObject(this);
  }

  public bool Validate(string key)
  {
    return MAC.Equals(GenerateMAC(Subscription, key));
  }

  public static bool TryDeserialize(string json, out Token token)
  {
    try
    {
      token = JsonConvert.DeserializeObject<Token>(json);
      return true;
    }
    catch (Exception)
    {
      token = null;
      return false;
    }
  }
}
}
