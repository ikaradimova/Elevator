using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Collections;

namespace ElevatorForBaseArea51
{
    class Program
    {
        /**
         * Definitions
         */ 
        static Semaphore semaphore;
        private static object someLock = new object();

        static public Random rand = new Random();
        static int numberOfAgents = rand.Next(1, 10);
        static int securityLevel;
        static int currentFloor;
        static int dreamFloor;

        static int initialFloor = 0;

        static string[] floors = { "0", "G", "S", "T1", "T2" };


        static public void Agent()
        {
            /**
             * Definitions
             */
            string[] securityLevelNames = { "", "CONFIDENTIAL", "SECRET", "TOP SECRET" };
        
            /**
             * Generating random values 
             */
            securityLevel = rand.Next(1, 4);
            currentFloor = rand.Next(0, 5);
            dreamFloor = rand.Next(0, 5);

            /**
             * Checking security levels
             */ 
            switch (securityLevel)
            {
                case 1:
                case 2:
                    while (currentFloor > securityLevel)
                    {
                        currentFloor = rand.Next(0, 5);
                        while (dreamFloor == currentFloor)
                        {
                            dreamFloor = rand.Next(0, 5);
                        } 
                    }
                    break;
                case 3:
                    while (dreamFloor == currentFloor)
                    {
                        dreamFloor = rand.Next(0, 5);
                    }
                    break;
                default:
                    Console.WriteLine("ALERT! INTRUDER!");
                    return;
            }
            Console.WriteLine($"Security level {securityLevelNames[securityLevel]}. Current floor {floors[currentFloor]}, desired floor {floors[dreamFloor]}.");
        }

        static public void Elevator(object tag)
        {
            /**
             * Definitions
             */ 
            CancellationToken token = (CancellationToken)tag;
            var cts = new CancellationTokenSource();

            /**
             * Generating the agents
             */ 
            for (int i = 1; i <= numberOfAgents; i++)
            {
                /**
                 * Check if threads are being cancelled
                 */ 
                if (token.IsCancellationRequested)
                {
                    Console.WriteLine();
                    Console.WriteLine("Elevator has been stopped.");
                    return;
                }

                /**
                 * Output
                 */ 
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine($"Agent {i} enters the elevator.");

                /**
                 * Starting thread Agent
                 */ 
                lock (someLock)
                {
                    Thread agent = new Thread(Agent);
                    agent.Start();
                    agent.Join();
                }

                /**
                 * Access check
                 */
                while (((securityLevel == 1) && (dreamFloor > 1)) || ((securityLevel == 2) && (dreamFloor > 2)))
                {
                    Console.WriteLine($"Agent {i} doesn't have access to floor {floors[dreamFloor]}.");

                    do
                    {
                        dreamFloor = rand.Next(0, 5);
                    } while (dreamFloor == currentFloor);

                    Console.WriteLine($"Agent {i} chose new floor: {floors[dreamFloor]}.");
                }
                
                 /**
                 * Elevator moving towards the agent
                 */ 
                if (initialFloor < currentFloor)
                {
                    for (int j = initialFloor; j <= currentFloor; j++)
                    {
                        Thread.Sleep(1000);
                    }
                }
                else
                {
                    for (int j = initialFloor; j >= currentFloor; j--)
                    {
                        Thread.Sleep(1000);
                    }
                }

                /**
                 * Waiting
                 */ 
                semaphore.WaitOne();

                /**
                 * Elevator moving from current to desired floor
                 */ 
                if (currentFloor < dreamFloor)
                {
                    for (int j = currentFloor; j <= dreamFloor; j++)
                    {
                        Thread.Sleep(1000);
                    }
                    initialFloor = dreamFloor;
                } else
                {
                    for (int j = currentFloor; j >= dreamFloor; j--)
                    {
                        Thread.Sleep(1000);
                    }
                }

                /**
                 * Output
                 */ 
                Console.WriteLine($"Agent {i} goes off the elevator.");

                /**
                 * Release
                 */ 
                lock (someLock)
                {
                    semaphore.Release();
                }
                initialFloor = dreamFloor;
            }

            /**
             * Output
             */ 
            Console.WriteLine();
            Console.WriteLine("Press ENTER to exit.");
        }

        static void Main(string[] args)
        {
            /**
             * Definitions
             */
            semaphore = new Semaphore(1, 1);
            var cts = new CancellationTokenSource();
            Thread elevator = new Thread(Elevator);

            /**
             * Output
             */
            Console.WriteLine("Press Esc to cancel threads.");
            Console.WriteLine();
            Console.WriteLine($"Number of agents waiting for the elevator: {numberOfAgents}");

            /**
             * Cancelling thread
             */
            elevator.Start(cts.Token);
            if(Console.ReadKey().KeyChar == (char)ConsoleKey.Escape)
            {
                Console.WriteLine();
                Console.WriteLine("You decided to cancel operation. Start cancelling.");
                cts.Cancel();
                elevator.Join();
                Console.WriteLine("All threads cancelled successfully.");
                Console.WriteLine("Press ENTER to exit.");
            }

            Console.ReadLine();
        }
    }
}
