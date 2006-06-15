/*
 * Copyright (c) 2006, Second Life Reverse Engineering Team
 * All rights reserved.
 *
 * - Redistribution and use in source and binary forms, with or without 
 *   modification, are permitted provided that the following conditions are met:
 *
 * - Redistributions of source code must retain the above copyright notice, this
 *   list of conditions and the following disclaimer.
 * - Neither the name of the Second Life Reverse Engineering Team nor the names 
 *   of its contributors may be used to endorse or promote products derived from
 *   this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE 
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF 
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE 
 * POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections;
using libsecondlife;

namespace sldump
{
	class sldump
	{
		//
		public static void DefaultHandler(Packet packet, Circuit circuit)
		{
			string output = "";
			ArrayList blocks = packet.Blocks();

			output += "---- " + packet.Layout.Name + " ---- (" + packet.Data[0] + ") (" + packet.Sequence + ")\n";

			foreach (Block block in blocks)
			{
				output += " -- " + block.Layout.Name + " --\n";

				foreach (Field field in block.Fields)
				{
					output += "  " + field.Layout.Name + ": " + field.Data.ToString() + "\n";
				}
			}

			Console.Write(output);
		}

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			SecondLife client;

			if (args.Length == 0 || (args.Length < 3 && args[0] != "--protocol"))
			{
				Console.WriteLine("Usage: sldump [--decrypt] [inputfile] [outputfile] [--protocol] [firstname] " +
					"[lastname] [password]");
				return;
			}

			if (args[0] == "--decrypt")
			{
				try
				{
					ProtocolManager.DecodeMapFile(args[1], args[2]);
				}
				catch (Exception e)
				{
					Console.WriteLine(e.ToString());
				}

				return;
			}

			try
			{
				client = new SecondLife("keywords.txt", "protocol.txt");
			}
			catch (Exception e)
			{
				// Error initializing the client, probably missing file(s)
				Console.WriteLine(e.ToString());
				return;
			}

			if (args[0] == "--protocol")
			{
				client.Protocol.PrintMap();
				return;
			}

			// Setup the callback
			PacketCallback defaultCallback = new PacketCallback(DefaultHandler);
			client.Network.UserCallbacks["Default"] = defaultCallback;

			Hashtable loginParams = NetworkManager.DefaultLoginValues(args[0], args[1], args[2], "00:00:00:00:00:00",
				"last", 1, 10, 3, 3, "Win", "0", "sldump", "jhurliman@wsu.edu");

			// An example of how to pass additional options to the login server
			ArrayList optionsArray = new ArrayList();
			optionsArray.Add("inventory-root");
			optionsArray.Add("inventory-skeleton");
			loginParams["options"] = optionsArray;

			Hashtable loginReply = new Hashtable();

			if (!client.Network.Login(loginParams, out loginReply))
			{
				// Login failed
				Console.WriteLine("Error logging in: " + client.Network.LoginError);
				return;
			}

			// Login was successful
			Console.WriteLine("Message of the day: " + loginReply["message"]);

			while (true)
			{
				client.Tick();
			}
		}
	}
}
