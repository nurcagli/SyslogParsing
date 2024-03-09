using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

public class Syslog
{
    private UdpClient _udpClient;
    private readonly IPAddress _ipAddress;
    private readonly int _port;

    public Syslog(IPAddress ipAddress, int port)
    {
        _ipAddress = ipAddress;
        _port = port;
    }

    public async Task StartListeningAsync()
    {
        try
        {
            // UDP istemcisini başlat ve belirtilen IP adresi ve port üzerinden dinleme başlat
            _udpClient = new UdpClient(new IPEndPoint(_ipAddress, _port));
            Console.WriteLine($"Listening for syslog messages on {_ipAddress}:{_port}...");

            while (true)
            {
                var result = await _udpClient.ReceiveAsync();
                SplitSyslogMessage(Encoding.ASCII.GetString(result.Buffer), result.RemoteEndPoint);
            }
        }
        catch (SocketException ex)
        {
            Console.WriteLine($"SocketException: {ex.Message}");
        }
        finally
        {
            _udpClient.Close();
        }
    }

    private void SplitSyslogMessage(string log, IPEndPoint remoteEndPoint)
    {
        // Düzenli ifade tanımlama
        string pattern = @"^<(\d+)>(\w{3}\s+\d{1,2}\s\d{2}:\d{2}:\d{2})\s([\w\s\[\]]+):\s(.*)$";

        // Regex eşleşmesini al
        Match match = Regex.Match(log, pattern);

        if (match.Success)
        {
            string priValue = match.Groups[1].Value;
            string timestamp = match.Groups[2].Value;
            string otherInfo = match.Groups[3].Value + " " + match.Groups[4].Value;

            Console.WriteLine("PRI değeri: " + priValue);
            Console.WriteLine("Timestamp değeri: " + timestamp);
            Console.WriteLine("Diğer bilgiler: " + otherInfo);
        }
        else
        {
            Console.WriteLine("Syslog verisi uygun formatta değil.");
        }

        //remote devıce ıp
        Console.WriteLine("IP:" + remoteEndPoint.Address.ToString());
    }
}

public class Program
{
    public static async Task Main()
    {
        var ipAddress = IPAddress.Parse("0.0.0.0"); // Tüm gelen bağlantıları dinle
        var port = 514; // Standart syslog portu

        var syslogListener = new Syslog(ipAddress, port);
        await syslogListener.StartListeningAsync();
    }
}
