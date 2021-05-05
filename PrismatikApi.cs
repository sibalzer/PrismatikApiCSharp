using System.Net.Sockets;
using System.Threading;
using PrimS.Telnet;

/// <summary>
/// Just a simple Wrapper for the Prismatik API
/// </summary>
static class PrismatikApiClient
{
    public static Client PRISMATIC_CLIENT;
    private static readonly object ro_PRISMATIC_CLIENT_LOCK_OBJ = new object();

    /// <summary>Setup the connection to the Prismatik API.</summary>
    /// <param name="apiKey">Your API-Key.</param>
    public static void SetupTelnetClient(string apiKey)
    {
        lock (ro_PRISMATIC_CLIENT_LOCK_OBJ)
        {
            if (PRISMATIC_CLIENT != null && PRISMATIC_CLIENT.IsConnected) return;

            try
            {
                PRISMATIC_CLIENT = new Client("127.0.0.1", 3636, new CancellationToken());
            }
            catch (SocketException)
            {
                PRISMATIC_CLIENT = null;
                return;
            }

            if (!PRISMATIC_CLIENT.IsConnected)
            {
                PRISMATIC_CLIENT = null;
                return;
            }

            var welcomeMessage = PRISMATIC_CLIENT.ReadAsync().Result;
            if (!welcomeMessage.Contains("Lightpack API"))
            {
                PRISMATIC_CLIENT = null;
                return;
            }

            if (!AuthenticateTelnet(apiKey)) PRISMATIC_CLIENT = null;
        }
    }

    private static bool AuthenticateTelnet(string apiKey)
    {
        lock (ro_PRISMATIC_CLIENT_LOCK_OBJ)
        {
            if (!PRISMATIC_CLIENT.IsConnected) return false;

            PRISMATIC_CLIENT.WriteLine($"apikey:{apiKey}");
            var authResponse = PRISMATIC_CLIENT.ReadAsync().Result;

            return authResponse.Contains("ok");
        }
    }

    private static bool GetStatusApi()
    {
        lock (ro_PRISMATIC_CLIENT_LOCK_OBJ)
        {
            if (!PRISMATIC_CLIENT.IsConnected) return false;

            PRISMATIC_CLIENT.WriteLine("getstatusapi");
            var useResponse = PRISMATIC_CLIENT.ReadAsync().Result;

            return useResponse.Contains("statusapi:idle");
        }
    }

    private static bool Lock()
    {
        lock (ro_PRISMATIC_CLIENT_LOCK_OBJ)
        {
            if (!PRISMATIC_CLIENT.IsConnected) return false;

            PRISMATIC_CLIENT.WriteLine($"lock");
            var authResponse = PRISMATIC_CLIENT.ReadAsync().Result;

            return authResponse.Contains("lock:success");
        }
    }
    
    private static bool Unlock()
    {
        lock (ro_PRISMATIC_CLIENT_LOCK_OBJ)
        {
            if (!PRISMATIC_CLIENT.IsConnected) return false;

            PRISMATIC_CLIENT.WriteLine($"unlock");
            var authResponse = PRISMATIC_CLIENT.ReadAsync().Result;

            return authResponse.Contains("unlock:success") || authResponse.Contains("unlock:not");
        }
    }

    /// <summary>Returnes full profiles list of controlling program settings. Profiles are kept in Profiles folder.</summary>
    /// <returns>srray with profile-names</returns>
    public static string[] GetProfiles()
    {
        lock (ro_PRISMATIC_CLIENT_LOCK_OBJ)
        {
            if (!PRISMATIC_CLIENT.IsConnected) return new string[0];

            if (!Lock()) return new string[0];

            PRISMATIC_CLIENT.WriteLine("getprofiles");
            var useResponse = PRISMATIC_CLIENT.ReadAsync().Result;
            useResponse = useResponse.Replace("profiles:", "");
            Unlock();
            return useResponse.Split(';');
        }
    }

    /// <summary>Returnes the name of the current active profile.</summary>
    /// <returns>profil-name</returns>
    public static string GetProfile()
    {
        lock (ro_PRISMATIC_CLIENT_LOCK_OBJ)
        {
            if (!PRISMATIC_CLIENT.IsConnected) return "";

            if (!Lock()) return "";

            PRISMATIC_CLIENT.WriteLine("getprofiles");
            var useResponse = PRISMATIC_CLIENT.ReadAsync().Result;
            useResponse = useResponse.Replace("profile:", "");
            Unlock();
            return useResponse;
        }
    }
    
    /// <summary>Returnes the status of device, which, while being connected to PC, might be turned off (LEDs are not burning) or might be turned on in the capturing software settings. The command returnes device error in case of error of the access to device (the icon in tray changes to the forbidding one). Unknown is a hardly probable event that occurs upon the expiry of request timeout to GUI (~1 s.)</summary>
    /// <returns>on, off, device error or unknown</returns>
    public static string GetStatus()
    {
        lock (ro_PRISMATIC_CLIENT_LOCK_OBJ)
        {
            if (!PRISMATIC_CLIENT.IsConnected) return "";

            if (!Lock()) return "";

            PRISMATIC_CLIENT.WriteLine("getstatus");
            var useResponse = PRISMATIC_CLIENT.ReadAsync().Result;
            useResponse = (useResponse.Replace("status:", ""));

            Unlock();
            return useResponse;
        }
    }

    /// <summary>Sets brightness level to all device's LEDs at the same time within the range of 0 (lighting disabled) to 100.</summary>
    /// <param name="brightness">brigtness from 0-100</param>
    /// <returns>true if success false if it fails</returns>
    public static bool SetBrightness(uint brightness)
    {
        lock (ro_PRISMATIC_CLIENT_LOCK_OBJ)
        {
            if (!PRISMATIC_CLIENT.IsConnected) return false;
            
            if (!Lock()) return false;

            PRISMATIC_CLIENT.WriteLine($"setbrightness:{brightness}");
            var useResponse = PRISMATIC_CLIENT.ReadAsync().Result;
            Unlock();
            return useResponse.Contains("ok");
        }
    }


    /// <summary>Turns on the defined settings profile. Profiles list is available on command getprofiles.</summary>
    /// <param name="profile">profile-name as string</param>
    /// <returns>true if success false if it fails</returns>
    public static bool SetProfile(string profile)
    {
        lock (ro_PRISMATIC_CLIENT_LOCK_OBJ)
        {
            if (!PRISMATIC_CLIENT.IsConnected) return false;
            
            if (!Lock()) return false;

            PRISMATIC_CLIENT.WriteLine($"setprofile:{profile}");
            var useResponse = PRISMATIC_CLIENT.ReadAsync().Result;
            Unlock();
            return useResponse.Contains("ok");
        }
    }


    /// <summary>Turnes the device on or off. Represents the "turn the lighting on/off" button in the software settings.</summary>
    /// <param name="status">desired  status</param>
    /// <returns>true if success false if it fails</returns>
    public static bool SetStatus(bool status)
    {
        lock (ro_PRISMATIC_CLIENT_LOCK_OBJ)
        {
            if (!PRISMATIC_CLIENT.IsConnected) return false;
            
            if (!Lock()) return false;

            var statusString = status ? "on" : "off" ;

            PRISMATIC_CLIENT.WriteLine($"setprofile:{statusString}");
            var useResponse = PRISMATIC_CLIENT.ReadAsync().Result;
            Unlock();
            return useResponse.Contains("ok");
        }
    }
}