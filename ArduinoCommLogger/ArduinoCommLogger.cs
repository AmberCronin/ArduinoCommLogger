using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;

namespace ArduinoCommLogger
{
	class ArduinoCommLogger
	{
		static SerialPort arduino;

		static void Main(string[] args)
		{
			Console.WriteLine("Welcome to the Arduino Serial Communication Logger, created and maintained by Benjamin Cronin (bcronin720@gmail.com)");
			Console.WriteLine("For more information about the usage of this software, please consult the GitHub wiki at https://github.com/BenjaminCronin/ArduinoCommLogger/wiki");
			Console.WriteLine("To report bugs, please contact the maintainer or leave a bug report at https://github.com/BenjaminCronin/ArduinoCommLogger/issues");
			Console.WriteLine("The current time is " + DateTime.Now.ToLongTimeString());


			Console.WriteLine("Enter the COM port of the Arduino");
			string comPort = Console.ReadLine();
			if (int.TryParse(comPort, out _))
			{
				comPort = "COM" + comPort;
			}
			else
			{
				comPort = comPort.ToUpper();
			}

			Console.WriteLine("Enter the serial communication speed");
			string comSpeed = Console.ReadLine();

			Console.WriteLine("Enter the path to save the file to");
			string filePath = Console.ReadLine();

			bool writeTime = AskYesNoQuestion("Write time to .csv file?");

			Console.WriteLine("Enter a comma seperated list of commands to send to the Arduino before beginning logging");
			string commandsCSV = Console.ReadLine();
			commandsCSV = commandsCSV.Replace(" ", "");
			string[] commands = commandsCSV.Split(',');
			

			Console.WriteLine("Opening serial port with standard Arduino communication settings...");
			Console.WriteLine("(No parity, 8 data bits, one stop bit, no handshake)");

			arduino = new SerialPort();
			arduino.PortName = comPort;
			try
			{
				arduino.BaudRate = int.Parse(comSpeed);
			}
			catch(Exception e)
			{
				HoldProgramWithMessage("Failed to parse the provided baud rate");
				return;
			}
			arduino.Parity = Parity.None;
			arduino.DataBits = 8;
			arduino.StopBits = StopBits.One;
			arduino.Handshake = Handshake.None;

			arduino.ReadTimeout = 5000; // wait 5 seconds for boot and the like
			arduino.WriteTimeout = 5000;

			try
			{
				arduino.Open();
				Console.WriteLine("Successfully opened " + comPort);
			}
			catch (Exception e)
			{
				HoldProgramWithMessage("Failed to open serial port " + comPort);
				return;
			}

			//wait for the arduino to send one message before attempting to communicate
			// aka basic one-way handshake, but we lose the first mesage (which may be the initial startup message)
			try
			{
				Console.WriteLine("Attempting to communicate with the Arduino...");
				arduino.ReadLine();
			}
			catch (Exception e)
			{
				HoldProgramWithMessage("Timed out attempting to communicate with the arduino on " + comPort + " at " + comSpeed + " baud with standard settings");
				return;
			}
			Console.WriteLine("Communication established with the Arduino.");
			foreach(string s in commands)
			{
				arduino.Write(s);
			}
			string timeDateString = DateTime.Now.ToLongTimeString() + " " + DateTime.Now.ToShortDateString();
			timeDateString = timeDateString.Replace(' ', '_');
			timeDateString = timeDateString.Replace('/', '-');
			timeDateString = timeDateString.Replace(':', '-');
			timeDateString = timeDateString.Replace('.', '-');
			string savePath = filePath + "log_" + timeDateString + ".csv";
			using (System.IO.StreamWriter file = new System.IO.StreamWriter(savePath))
			{
				Console.WriteLine("Writing csv log file to " + savePath + "...");
				Console.WriteLine("Press the enter key at any time to exit the program");
				while (!(Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Enter))
				{
					try
					{
						string line = arduino.ReadLine();
						file.Write((writeTime ? DateTime.Now.ToLongTimeString() : "") + line);
						Console.WriteLine(line);
					}
					catch (TimeoutException te)
					{
						Console.WriteLine("Timed out at " + DateTime.Now.ToString());
						break;
					}
				}
				Console.WriteLine("Saving file...");
			}
			Console.WriteLine("File saved to " + savePath);
			Console.WriteLine("Press any key to exit");
			Console.ReadKey();
		}

		//Asks a y/n question
		// (appends " (y/n)" to the question
		static bool AskYesNoQuestion(string question)
		{
			string[] yesAnswers = { "y", "yes", "yeah", "sure", "okay", "ok", "fine", "why not" };
			string[] noAnswers = { "n", "no", "nah", "nope", "no man", "not today" };

			Console.WriteLine(question + " (y/n)");
			string answer = Console.ReadLine();
			answer = answer.ToLower();

			if (yesAnswers.Contains(answer))
			{
				return true;
			}
			else if (noAnswers.Contains(answer))
			{
				return false;
			}
			else
			{
				Console.WriteLine("Answer not understood. Please try again.");
				return AskYesNoQuestion(question);
			}
		}
		static void HoldProgramWithMessage(string message)
		{
			Console.WriteLine(message);
			Console.WriteLine("Press any key to continue");
			Console.ReadKey();
		}
	}
}
