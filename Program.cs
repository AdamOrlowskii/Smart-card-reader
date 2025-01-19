using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PCSC;
using PCSC.Exceptions;

namespace reder
{
    class Program
    {
        private static SCardReader reader;
        private static SCardContext context;
        private static System.IntPtr protocol;
        
        static void Main(string[] args)
        {
            context = new SCardContext();
            context.Establish(SCardScope.System);
            // Pobranie listy czytnikow
            string[] readers = context.GetReaders();
            // Wypisanie nazwy czytnika
            Console.WriteLine("Nazwa czytnika: \t" + readers[0]);
            // Polaczenie czytnika
            reader = new SCardReader(context);
            reader.Connect(readers[0], SCardShareMode.Shared, SCardProtocol.T0 | SCardProtocol.T1);
            if (reader.ActiveProtocol == SCardProtocol.T0)
                protocol = SCardPCI.T0;
            else
                protocol = SCardPCI.T1;
            // Wypisanie aktywnego protokolu
            Console.WriteLine("Aktywny protokol: \t" + reader.ActiveProtocol);
            // 0xA0 - karta GSM
            // 0xA4 00 00 02 - select file
            // 0x7F 10 - Dedicated File
            byte[] command_select_telecom = { 0xA0, 0xA4, 0x00, 0x00, 0x02, 0x7F, 0x10 };
            byte[] command_select_mf = { 0xA0, 0xA4, 0x00, 0x00, 0x02, 0x3f, 0x00 };
            byte[] received1 = new byte[256];
            byte[] received2 = new byte[256];
            byte[] received3 = new byte[256];
            byte[] received4 = new byte[256];
            byte[] received5 = new byte[256];
            byte[] received6 = new byte[256];

            reader.Transmit(protocol, command_select_mf, ref received6);
            Console.Write("Odpowiedz APDU na select_mf: ");
            foreach (byte b in received6)
            {
                Console.Write("{0:X2} ", b);
            }
            Console.WriteLine();

            reader.Transmit(protocol, command_select_telecom, ref received1);
            Console.Write("Odpowiedz APDU na select_telecom: ");
            foreach (byte b in received1)
            {
                Console.Write("{0:X2} ", b);
            }
            Console.WriteLine();

            byte[] command_select_book = { 0xA0, 0xA4, 0x00, 0x00, 0x02, 0x6F, 0x3A };
            reader.Transmit(protocol, command_select_book, ref received3);
            Console.Write("Odpowiedz APDU na select_book: ");
            foreach (byte b in received3)
            {
                Console.Write("{0:X2} ", b);
            }
            Console.WriteLine();
            byte SW2 = received3[1];

            byte[] get_response_book = { 0xA0, 0xC0, 0x00, 0x00, SW2 };
            reader.Transmit(protocol, get_response_book, ref received4);
            Console.Write("Odpowiedz APDU get_response_book: ");
            foreach (byte b in received4)
            {
                Console.Write("{0:X2} ", b);
            }
            Console.WriteLine();
            Console.Write("Odpowiedz APDU na read: ");
            for (byte i = 0x01; i < 0x0A; i++)
            {
                byte[] read = { 0xA0, 0xB2, i, 0x04, 0x1E };
                reader.Transmit(protocol, read, ref received5);
                foreach (byte b in received5)
                {
                    Console.Write("{0:X2} ", b);
                }
                Console.WriteLine();
                Console.WriteLine(System.Text.Encoding.ASCII.GetString(received5).ToString());
            }
            Console.WriteLine();
            var stop = Console.ReadLine();
        }
    }
}