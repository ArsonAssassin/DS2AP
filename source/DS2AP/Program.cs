using Archipelago.Core;
using Archipelago.Core.GUI;
using Archipelago.Core.Models;
using Archipelago.Core.Util;
using Serilog;
using System.Reflection.Metadata;

namespace DS2AP
{
    internal static class Program
    {
        public static ArchipelagoClient Client { get; set; }
        public static MainForm MainForm { get; set; }
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            var options = new GuiDesignOptions
            {
                BackgroundColor = Color.Black,
                ButtonColor = Color.DarkRed,
                ButtonTextColor = Color.Black,
                Title = "DS2AP - Dark Souls 2 SotfS Archipelago",

            };
            MainForm = new MainForm(options);
#if __DEBUG__
            LoggerConfig.SetLogLevel(LogEventLevel.Verbose);
#endif
            MainForm.ConnectClicked += MainForm_ConnectClicked;
            Application.Run(MainForm);
        }

        private static async void MainForm_ConnectClicked(object? sender, ConnectClickedEventArgs e)
        {
            if (Client != null)
            {
                Client.Connected -= OnConnected;
                Client.Disconnected -= OnDisconnected;
            }
            GenericGameClient client = new GenericGameClient("DarkSoulsII");
            var connected = client.Connect();
            if (!connected)
            {
                Log.Logger.Information("Dark Souls not running, open Dark Souls before connecting!");
                return;
            }



            Client = new ArchipelagoClient(client);

            Client.Connected += OnConnected;
            Client.Disconnected += OnDisconnected;
            var isOnline = Helpers.GetIsPlayerOnline();
            if (isOnline)
            {
                Log.Logger.Information("YOU ARE PLAYING ONLINE. THIS APPLICATION WILL NOT PROCEED.");
                return;
            }
            await Client.Connect(e.Host, "Dark Souls Remastered");
            await Client.Login(e.Slot, !string.IsNullOrWhiteSpace(e.Password) ? e.Password : null);
            Client.ItemReceived += Client_ItemReceived;


            //RemoveItems();
        }
        public static void AddItem(int item, short amount, byte upgrade, byte infusion)
        {
            var command = Helpers.GetItemCommand();
            var mem = Memory.Allocate(0x8A);
            var playerPointers = Helpers.GetPlayerPointers();

            Memory.WriteByteArray((ulong)mem + 0x4, BitConverter.GetBytes(item));
            Memory.WriteByteArray((ulong)mem + 0x8, BitConverter.GetBytes(float.MaxValue));
            Memory.WriteByteArray((ulong)mem + 0xC, BitConverter.GetBytes(amount));
            Memory.WriteByte((ulong)(mem + 0xE), upgrade);
            Memory.WriteByte((ulong)(mem + 0xF), infusion);

            var modifications = new Dictionary<int, long>
            {
                { 0x9, 1 },  // Quantity (always 1 in this case)
                { 0xF, mem.ToInt64() },  // Item struct address
                { 0x1C, Memory.ReadLong(playerPointers.AvailableBag) },  // Item bag address
                { 0x29, Memory.ReadLong(Helpers.GetItemGiveFuncOffset()) },  // Add item function address
                { 0x36, 1 },  // Unknown purpose, always 1
                { 0x3C, mem.ToInt64() },  // Item struct address (again)
                { 0x54, Memory.ReadLong(Helpers.GetItemStruct2dDisplayOffset()) },  // Display struct address
                { 0x66, Memory.ReadLong(Helpers.GetItemGiveWindow()) },  // Item give window address
                { 0x70, Memory.ReadLong(Helpers.GetDisplayItemOffset()) }  // Display item function address
            };

            foreach (var mod in modifications)
            {
                Array.Copy(BitConverter.GetBytes(mod.Value), 0, command, mod.Key, 8);
            }
            Memory.ExecuteCommand(command);
            Memory.FreeMemory(mem);
        }
        private static void Client_ItemReceived(object? sender, ItemReceivedEventArgs e)
        {
            var itemId = e.Item.Id;
        }

        private static void OnConnected(object sender, EventArgs args)
        {
            Log.Logger.Information("Connected to Archipelago");
            Log.Logger.Information($"Playing {Client.CurrentSession.ConnectionInfo.Game} as {Client.CurrentSession.Players.GetPlayerName(Client.CurrentSession.ConnectionInfo.Slot)}");

        }

        private static void OnDisconnected(object sender, EventArgs args)
        {
            Log.Logger.Information("Disconnected from Archipelago");
        }
    }
}