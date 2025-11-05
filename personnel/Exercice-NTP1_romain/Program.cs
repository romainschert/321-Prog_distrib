
using System;
using System.Net;
using System.Net.Sockets;

namespace ntp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string[] ntpServer = {
                "0.pool.ntp.org",
                "1.pool.ntp.org",
                "time.google.com",
                "time.cloudflare.com",
                "0.ch.pool.ntp.org"
            };  // 0.ch .... = nom de domaine d'un serveur NTP publique

            byte[] timeMessage = new byte[48];// variable de type byte[]

            timeMessage[0] = 0x1B;// 0x1B type de requete eenvoyer au serveur

            IPEndPoint ntpReference = new IPEndPoint(Dns.GetHostAddresses(ntpServer[4])[0], 123);

            UdpClient client = new UdpClient();
            client.Connect(ntpReference);

            client.Send(timeMessage, timeMessage.Length);
            timeMessage = client.Receive(ref ntpReference);
            DateTime ntpTime = NtpPacket.ToDateTime(timeMessage);


            Console.WriteLine(ntpTime.ToLongDateString());
            Console.WriteLine(ntpTime.ToString("dd/MM/yyyy HH:mm:ss"));
            Console.WriteLine(ntpTime.ToString("dd/MM/yyyy"));

            Console.WriteLine(ntpTime.ToString("yyyy-mm-ddThh-mm-ssZ"));
            client.Close();

            TimeSpan Difftime = DateTime.UtcNow - ntpTime;
            Console.WriteLine(Difftime.TotalSeconds);

            DateTime localTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.Local);
            Console.WriteLine($"Heure locale : {localTime}");

        
            TimeZoneInfo swissTimeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
            DateTime swissTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, swissTimeZone);
            Console.WriteLine($"Heure suisse : {swissTime}");

            TimeZoneInfo utcTimeZone = TimeZoneInfo.Utc;
            DateTime backToUtc = TimeZoneInfo.ConvertTime(localTime, TimeZoneInfo.Local, utcTimeZone);
            Console.WriteLine($"Retour vers UTC : {backToUtc}");

            DateTime i = DateTime.UtcNow;
            DisplayWorldClocks(i);
        }
        public static void DisplayWorldClocks(DateTime utcTime)
        {
            var timeZones = new[]
            {
                ("UTC", TimeZoneInfo.Utc),
                ("New York", TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time")),
                ("London", TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time")),
                ("Tokyo", TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time")),
                ("Sydney", TimeZoneInfo.FindSystemTimeZoneById("AUS Eastern Standard Time"))
            };

            foreach (var (name, tz) in timeZones)
            {
                var localTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, tz);
                Console.WriteLine($"{name}: {localTime:yyyy-MM-dd HH:mm:ss}");
            }
        }

    }

    static class NtpPacket
    {
        public static DateTime ToDateTime(byte[] ntpData)
        {
            ulong intPart = (ulong)ntpData[40] << 24 | (ulong)ntpData[41] << 16 | (ulong)ntpData[42] << 8 | (ulong)ntpData[43];
            ulong fractPart = (ulong)ntpData[44] << 24 | (ulong)ntpData[45] << 16 | (ulong)ntpData[46] << 8 | (ulong)ntpData[47];

            var milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);
            var networkDateTime = (new DateTime(1900, 1, 1)).AddMilliseconds((long)milliseconds);

            return networkDateTime;
        }   
    }
    



}
