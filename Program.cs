using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace tracsv
{
    class Program
    {
        static void Main(string[] args)
        {

            Ping pingSender = new Ping();
            PingOptions options = new PingOptions();

            // Create a buffer of data to be transmitted.
            string data = "Hi Transit router. Tell me RoundtripTime.";
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            int timeout = 3000;

            int myTTL = 25;
            string myFrom = "dummy";
            string myTo = "dummy";
            string myType;
            long myLinkText;

            //This is CSV header
            Console.WriteLine("timestamp,from,to,linktext,type");

            for (int i = 1; i <= myTTL; i++)
            {

                // Count up TTL and check the ping reply
                options.Ttl = i;

                // Send ping using the TTL set above
                PingReply reply = pingSender.Send(args[0], timeout, buffer, options);

                //Console.WriteLine("Here is {0}", i);
                // Change the action depending on ICMP request reply

                switch (reply.Status)
                {

                    // Transit router replied TtlExpired Time-to-live exceeded
                    case IPStatus.TtlExpired:

                        // To get RoundtripTime, directly send ICMP echo reply to the transit router
                        PingReply newreply = pingSender.Send(reply.Address.ToString(), timeout, buffer, options);

                        if(newreply.Status == IPStatus.Success)
                        {

                            if (myFrom == "dummy")
                            {
                                myFrom = GetLocalIPAddress();
                                myTo = newreply.Address.ToString();
                                myType = "desktop";
                                myLinkText = newreply.RoundtripTime;

                                ConsoleOutput(myFrom, myTo, myLinkText, myType);
                            }
                            else
                            {
                                myFrom = myTo;
                                myTo = newreply.Address.ToString();
                                myType = "network-wired";
                                myLinkText = newreply.RoundtripTime;

                                ConsoleOutput(myFrom, myTo, myLinkText, myType);
                            }

                        }
                        else
                        {
                            // Do nothing
                            // Console.WriteLine("Hre is {0}, Status: {1}", i, newreply.Status);
                        }
    
                        break;

                    // Finaly got ICMP echo reply directly from the deistination 
                    case IPStatus.Success:

                        myFrom = myTo;
                        myTo = reply.Address.ToString();
                        myType = "network-wired";
                        myLinkText = reply.RoundtripTime;

                        ConsoleOutput(myFrom, myTo, myLinkText, myType);

                        //set myTTL to exit for loop
                        i = myTTL;

                        break;

                    // Couldn't get ICMP echo reply
                    default:

                        //Console.WriteLine("Here is {0}. Status is {1}", i, reply.Status.ToString());
                        //Console.ReadLine();

                        break;

                }
            }
        }

        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }

            return "My Computer";

            //throw new Exception("No network adapters with an IPv4 address in the system!");
        }

        public static void ConsoleOutput(string myFrom, string myTo, long myLinkText, string myType )
        {
            DateTime dt = DateTime.Now;
            string MyTimeStamp = dt.ToString("yyyy/MM/dd HH:mm:ss");
            Console.WriteLine("{0},{1},{2},{3}ms,{4}", MyTimeStamp, myFrom, myTo, myLinkText, myType);
        }
    }

}
