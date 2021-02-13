using System;
using System.Collections.Generic;
using System.Linq;

namespace commercial_controller_csharp
{

    //--------------------BATTERY CLASS--------------------//
    public class Battery
    {
        public int id;
        public int amountOfColumns;
        public string status;
        public int amountOfFloors;
        public int amountOfBasements;
        public int amountOfElevatorPerColumn;
        public int[] servedFloors;
        public int columnID = 1;
        public int floorRequestButtonID = 1;
        public List<Column> columnsList;
        public List<FloorRequestButton> floorRequestButtonsList;

        //--------CONSTRUCTOR & ATTRIBUTES--------//
        public Battery(int id, int amountOfColumns, string status, int amountOfFloors, int amountOfBasements, int amountOfElevatorPerColumn)
        {
            this.id = id;
            this.amountOfColumns = amountOfColumns;
            this.status = status;
            this.amountOfFloors = amountOfFloors;
            this.amountOfBasements = amountOfBasements;
            this.amountOfElevatorPerColumn = amountOfElevatorPerColumn;
            this.columnsList = new List<Column>();
            this.floorRequestButtonsList = new List<FloorRequestButton>();
        }

        //--------METHODS--------//

        public void createBasementColumn(int amountOfBasements, int amountOfElevatorPerColumn)
        {
            int[] servedFloors = new int[amountOfBasements + 1];
            int floor = -1;
            for (int i = 0; i < (amountOfBasements + 1); i++)
            {
                if (i == 0)
                {
                    servedFloors[i] = 1;
                }
                else
                {
                    servedFloors[i] = floor;
                    floor--;
                }
            }

            var column = new Column(columnID, "online", amountOfElevatorPerColumn, servedFloors, true);
            columnsList.Add(column);
            columnID++;
        }

        public void createColumns(int amountOfColumns, int amountOfFloors, int amountOfElevatorPerColumn)
        {
            int amountOfFloorsPerColumn = (int)Math.Ceiling((double)(amountOfFloors / amountOfColumns));
            int floor = 1;
            for (int i = 0; i < amountOfColumns; i++)
            {
                int[] servedFloors = new int[amountOfFloorsPerColumn + 1];
                for (int x = 0; x < amountOfFloorsPerColumn; x++)
                {
                    if (i == 0)
                    {
                        servedFloors[x] = floor;
                        floor++;
                    }
                    else
                    {
                        servedFloors[0] = 1;
                        servedFloors[x + 1] = floor;
                        floor++;
                    }
                }
                var column = new Column(columnID, "online", amountOfElevatorPerColumn, servedFloors, false);
                columnsList.Add(column);
                columnID++;
            }
        }

        public void createFloorRequestButtons(int amountOfFloors)
        {
            int buttonFloor = 1;
            for (int i = 0; i < amountOfFloors; i++)
            {
                var floorRequestButton = new FloorRequestButton(floorRequestButtonID, "off", buttonFloor);
                floorRequestButtonsList.Add(floorRequestButton);
                buttonFloor++;
                floorRequestButtonID++;
            }
        }

        public void createBasementFloorRequestButtons(int amountOfBasements)
        {
            int buttonFloor = -1;
            for (int i = 0; i < amountOfBasements; i++)
            {
                var floorRequestButton = new FloorRequestButton(floorRequestButtonID, "off", buttonFloor);
                floorRequestButtonsList.Add(floorRequestButton);
                buttonFloor--;
                floorRequestButtonID++;
            }
        }

        public Column findBestColumn(int requestedFloor)
        {
            Column bestColumn = null;
            foreach (Column column in this.columnsList)
            {
                if (column.servedFloors.Contains(requestedFloor))
                {
                    bestColumn = column;
                }
            }
            return bestColumn;
        }

        //Simulate when a user press a button at the lobby
        public void assignElevator(int requestedFloor, string direction)
        {
            int? tempFloor = null;
            Console.WriteLine($"||Passenger requests elevator at the lobby for floor {requestedFloor}||");
            Column column = this.findBestColumn(requestedFloor);
            Console.WriteLine($"||{column.ID} is the assigned column for this request||");
            Elevator elevator = column.findBestElevator(1, direction);
            Console.WriteLine($"||{elevator.ID} is the assigned elevator for this request||");
            elevator.currentFloor = 1;
            Door.operateDoors();
            elevator.floorRequestList.Add(requestedFloor);
            elevator.sortFloorList();
            Console.WriteLine("||Elevator is moving||");
            elevator.moveElevator(tempFloor);
            Console.WriteLine($"||Elevator is {elevator.status}||");
            Door.operateDoors();
            if (elevator.floorRequestList.Count() == 0)
            {
                elevator.direction = null;
                elevator.status = "idle";
            }
            Console.WriteLine($"||Elevator is {elevator.status}||");
        }

    }


    //--------------------COLUMN CLASS--------------------//

    public class Column
    {
        public int ID;
        public string status;
        public int amountOfElevators;
        public int[] servedFloors;
        public bool isBasement;
        public List<Elevator> elevatorsList;
        public List<CallButton> callButtonsList;

        //--------CONSTRUCTOR & ATTRIBUTES--------//
        public Column(int ID, string status, int amountOfElevators, int[] servedFloors, bool isBasement)
        {
            this.ID = ID;
            this.status = status;
            this.amountOfElevators = amountOfElevators;
            this.servedFloors = servedFloors;
            this.isBasement = isBasement;
            this.elevatorsList = new List<Elevator>();
            this.callButtonsList = new List<CallButton>();
        }

        //--------METHODS--------//
        public void createCallButtons(int floorsServed, int amountOfBasements, bool isBasement)
        {
            int callButtonID = 1;
            if (isBasement)
            {
                int buttonFloor = -1;
                for (int i = 0; i < amountOfBasements; i++)
                {
                    var callButton = new CallButton(callButtonID, "off", buttonFloor, "up");
                    callButtonsList.Add(callButton);
                    buttonFloor--;
                    callButtonID++;
                }
            }
            else
            {
                int buttonFloor = 1;
                foreach (int floor in servedFloors)
                {
                    var callButton = new CallButton(callButtonID, "off", floor, "down");
                    callButtonsList.Add(callButton);
                    buttonFloor++;
                    callButtonID++;
                }
            }
        }

        public void createElevators(int[] servedFloors, int amountOfElevators)
        {
            int elevatorID = 1;
            for (int i = 0; i < amountOfElevators; i++)
            {
                var elevator = new Elevator(elevatorID, "idle", servedFloors, 1);
                elevatorsList.Add(elevator);
                elevatorID++;
            }
        }

        //Simulate when a user press a button on a floor to go back to the first floor
        public void requestElevator(int userFloor, string direction)
        {
            Console.WriteLine($"||Passenger requests elevator from {userFloor} going {direction} to the lobby||");
            Elevator elevator = this.findBestElevator(userFloor, direction);
            Console.WriteLine($"||{elevator.ID} is the assigned elevator for this request||");
            elevator.floorRequestList.Add(1);
            elevator.sortFloorList();
            elevator.moveElevator(userFloor);
            Door.operateDoors();
        }


        //We use a score system depending on the current elevators state. Since the bestScore and the referenceGap are 
        //higher values than what could be possibly calculated, the first elevator will always become the default bestElevator, 
        //before being compared with to other elevators. If two elevators get the same score, the nearest one is prioritized.
        public Elevator findBestElevator(int floor, string direction)
        {
            int requestedFloor = floor;
            string requestedDirection = direction;
            var bestElevatorInfo = new BestElevatorInfo(null, 6, 1000000);

            if (requestedFloor == 1)
            {
                foreach (Elevator elevator in this.elevatorsList)
                {
                    if (1 == elevator.currentFloor && elevator.status == "stopped")
                    {
                        this.checkBestElevator(1, elevator, bestElevatorInfo, requestedFloor);
                    }
                    else if (1 == elevator.currentFloor && elevator.status == "idle")
                    {
                        this.checkBestElevator(2, elevator, bestElevatorInfo, requestedFloor);
                    }
                    else if (1 > elevator.currentFloor && elevator.direction == "up")
                    {
                        this.checkBestElevator(3, elevator, bestElevatorInfo, requestedFloor);
                    }
                    else if (1 < elevator.currentFloor && elevator.direction == "down")
                    {
                        this.checkBestElevator(3, elevator, bestElevatorInfo, requestedFloor);
                    }
                    else if (elevator.status == "idle")
                    {
                        this.checkBestElevator(4, elevator, bestElevatorInfo, requestedFloor);
                    }
                    else
                    {
                        this.checkBestElevator(5, elevator, bestElevatorInfo, requestedFloor);
                    }
                }
            }
            else
            {
                foreach (Elevator elevator in this.elevatorsList)
                {
                    if (requestedFloor == elevator.currentFloor && elevator.status == "stopped" && requestedDirection == elevator.direction)
                    {
                        this.checkBestElevator(1, elevator, bestElevatorInfo, requestedFloor);
                    }
                    else if (requestedFloor > elevator.currentFloor && elevator.direction == "up" && requestedDirection == "up")
                    {
                        this.checkBestElevator(2, elevator, bestElevatorInfo, requestedFloor);
                    }
                    else if (requestedFloor < elevator.currentFloor && elevator.direction == "down" && requestedDirection == "down")
                    {
                        this.checkBestElevator(2, elevator, bestElevatorInfo, requestedFloor);
                    }
                    else if (elevator.status == "idle")
                    {
                        this.checkBestElevator(3, elevator, bestElevatorInfo, requestedFloor);
                    }
                    else
                    {
                        this.checkBestElevator(4, elevator, bestElevatorInfo, requestedFloor);
                    }

                }
            }
            return bestElevatorInfo.bestElevator;
        }

        public BestElevatorInfo checkBestElevator(int scoreToCheck, Elevator newElevator, BestElevatorInfo bestElevatorInfo, int floor)
        {
            if (scoreToCheck < bestElevatorInfo.bestScore)
            {
                bestElevatorInfo.bestScore = scoreToCheck;
                bestElevatorInfo.bestElevator = newElevator;
                bestElevatorInfo.referenceGap = Math.Abs(newElevator.currentFloor - floor);
            }
            else if (bestElevatorInfo.bestScore == scoreToCheck)
            {
                int gap = Math.Abs(newElevator.currentFloor - floor);
                if (bestElevatorInfo.referenceGap > gap)
                {
                    bestElevatorInfo.bestScore = scoreToCheck;
                    bestElevatorInfo.bestElevator = newElevator;
                    bestElevatorInfo.referenceGap = gap;
                }
            }
            return bestElevatorInfo;
        }

    }

    public class Elevator
    {
        public int ID;
        public string status;
        public int[] servedFloors;
        public int currentFloor;
        public string direction;
        public Door door;
        public List<int> floorRequestList;

        //--------CONSTRUCTOR & ATTRIBUTES--------//
        public Elevator(int id, string status, int[] servedFloors, int currentFloor)
        {
            this.ID = id;
            this.status = status;
            this.servedFloors = servedFloors;
            this.currentFloor = currentFloor;
            this.direction = "";
            this.door = new Door(id, "closed");
            this.floorRequestList = new List<int>();
        }

        //--------METHODS--------//

        public void moveElevator(int? tempFloor)
        {
            while (this.floorRequestList.Count() != 0)
            {
                int destination = this.floorRequestList[0];
                this.status = "moving";
                if (this.currentFloor < destination)
                {
                    this.direction = "up";
                    while (this.currentFloor < destination)
                    {
                        if (this.currentFloor == tempFloor)
                        {
                            this.status = "stopped";
                            Door.operateDoors();
                            this.currentFloor++;
                        }
                        else
                        {
                            this.currentFloor++;
                        }
                        if (this.currentFloor == 0)
                        {
                            ;
                        }
                        else
                        {
                            Console.WriteLine($"||Elevator is at floor: {this.currentFloor}||");
                        }

                    }
                }
                else if (this.currentFloor > destination)
                {
                    if (this.currentFloor > destination)
                    {
                        this.direction = "down";
                        while (this.currentFloor > destination)
                        {
                            if (this.currentFloor == tempFloor)
                            {
                                this.status = "stopped";
                                Door.operateDoors();
                                this.currentFloor--;
                            }
                            else
                            {
                                this.currentFloor--;
                            }
                            if (this.currentFloor == 0)
                            {
                                ;
                            }
                            else
                            {
                                Console.WriteLine($"||Elevator is at floor: {this.currentFloor}||");
                            }
                        }
                    }
                }
                this.status = "stopped";
                floorRequestList.RemoveAt(0);
            }
        }

        public void sortFloorList()
        {
            if (this.direction == "up")
            {
                this.floorRequestList.Sort();
            }
            else
            {
                this.floorRequestList.Sort();
                this.floorRequestList.Reverse();
            }
        }

    }

    public class BestElevatorInfo
    {
        public Elevator bestElevator;
        public int bestScore;
        public int referenceGap;

        public BestElevatorInfo(Elevator bestElevator, int bestScore, int referenceGap)
        {
            this.bestElevator = bestElevator;
            this.bestScore = bestScore;
            this.referenceGap = referenceGap;
        }
    }

    //Button on a floor or basement to go back to lobby
    public class CallButton
    {
        public int ID;
        public string status;
        public int floor;
        public string direction;

        public CallButton(int id, string status, int floor, string direction)
        {
            this.ID = id;
            this.status = status;
            this.floor = floor;
            this.direction = direction;
        }


    }

    //Button on the pannel at the lobby to request any floor
    public class FloorRequestButton
    {
        public int ID;
        public string status;
        public int floor;

        public FloorRequestButton(int id, string status, int floor)
        {
            this.ID = id;
            this.status = status;
            this.floor = floor;
        }

    }


    public class Door
    {
        public int ID;
        public string status;

        public Door(int ID, string status)
        {
            this.ID = ID;
            this.status = status;
        }

        public static void operateDoors()
        {
            bool overweight = false;
            bool obstruction = false;
            string status = "opened";
            Console.WriteLine($"||Elevator doors are {status}||");
            Console.WriteLine($"||Allow passengers to get on/off||");
            if (!overweight)
            {
                status = "closing";
                Console.WriteLine($"||Elevator doors are {status}||");
                if (!obstruction)
                {
                    status = "closed";
                    Console.WriteLine($"||Elevator doors are {status}||");
                }
                else
                {
                    obstruction = false;
                    operateDoors();
                }
            }
            else
            {
                while (overweight)
                {
                    overweight = false;
                }
                operateDoors();
            }
        }

    }



    class Program
    {
        static void Main(string[] args)
        {
            var battery = new Battery(1, 4, "online", 60, 6, 5);

            if (battery.amountOfBasements > 0)
            {
                battery.createBasementFloorRequestButtons(battery.amountOfBasements);
                battery.createBasementColumn(battery.amountOfBasements, battery.amountOfElevatorPerColumn);
                battery.amountOfColumns--;
            }

            battery.createFloorRequestButtons(battery.amountOfFloors);
            battery.createColumns(battery.amountOfColumns, battery.amountOfFloors, battery.amountOfElevatorPerColumn);

            for (int i = 0; i < battery.amountOfColumns + 1; i++)
            {
                battery.columnsList[i].createCallButtons(battery.amountOfFloors, battery.amountOfBasements, battery.columnsList[i].isBasement);
            }

            for (int i = 0; i < battery.amountOfColumns + 1; i++)
            {
                battery.columnsList[i].createElevators(battery.columnsList[i].servedFloors, battery.columnsList[i].amountOfElevators);
            }

            //Instruction; TO SIMULATE A SCENARIO, SIMPLY UNCOMMENT THE CONTENTS OF THE DESIRED FUNCTION BY REMOVING /* & */ + UNCOMMENT FUNCTION CALL

            /*
            void scenario1()
            {

                battery.columnsList[1].elevatorsList[0].currentFloor = 20;
                battery.columnsList[1].elevatorsList[0].direction = "down";
                battery.columnsList[1].elevatorsList[0].status = "moving";
                battery.columnsList[1].elevatorsList[0].floorRequestList.Add(5);

                battery.columnsList[1].elevatorsList[1].currentFloor = 3;
                battery.columnsList[1].elevatorsList[1].direction = "up";
                battery.columnsList[1].elevatorsList[1].status = "moving";
                battery.columnsList[1].elevatorsList[1].floorRequestList.Add(15);

                battery.columnsList[1].elevatorsList[2].currentFloor = 13;
                battery.columnsList[1].elevatorsList[2].direction = "down";
                battery.columnsList[1].elevatorsList[2].status = "moving";
                battery.columnsList[1].elevatorsList[2].floorRequestList.Add(1);

                battery.columnsList[1].elevatorsList[3].currentFloor = 15;
                battery.columnsList[1].elevatorsList[3].direction = "down";
                battery.columnsList[1].elevatorsList[3].status = "moving";
                battery.columnsList[1].elevatorsList[3].floorRequestList.Add(2);

                battery.columnsList[1].elevatorsList[4].currentFloor = 6;
                battery.columnsList[1].elevatorsList[4].direction = "down";
                battery.columnsList[1].elevatorsList[4].status = "moving";
                battery.columnsList[1].elevatorsList[4].floorRequestList.Add(1);

                battery.assignElevator(20, "up");

            }
            */


            /*
            void scenario2()
            {

                battery.columnsList[2].elevatorsList[0].currentFloor = 1;
                battery.columnsList[2].elevatorsList[0].direction = "up";
                battery.columnsList[2].elevatorsList[0].status = "stopped";
                battery.columnsList[2].elevatorsList[0].floorRequestList.Add(21);

                battery.columnsList[2].elevatorsList[1].currentFloor = 23;
                battery.columnsList[2].elevatorsList[1].direction = "up";
                battery.columnsList[2].elevatorsList[1].status = "moving";
                battery.columnsList[2].elevatorsList[1].floorRequestList.Add(28);

                battery.columnsList[2].elevatorsList[2].currentFloor = 33;
                battery.columnsList[2].elevatorsList[2].direction = "down";
                battery.columnsList[2].elevatorsList[2].status = "moving";
                battery.columnsList[2].elevatorsList[2].floorRequestList.Add(1);

                battery.columnsList[2].elevatorsList[3].currentFloor = 40;
                battery.columnsList[2].elevatorsList[3].direction = "down";
                battery.columnsList[2].elevatorsList[3].status = "moving";
                battery.columnsList[2].elevatorsList[3].floorRequestList.Add(24);

                battery.columnsList[2].elevatorsList[4].currentFloor = 39;
                battery.columnsList[2].elevatorsList[4].direction = "down";
                battery.columnsList[2].elevatorsList[4].status = "moving";
                battery.columnsList[2].elevatorsList[4].floorRequestList.Add(1);

                battery.assignElevator(36, "up");

            }
            */

            /*
            void scenario3()
            {

                battery.columnsList[3].elevatorsList[0].currentFloor = 58;
                battery.columnsList[3].elevatorsList[0].direction = "down";
                battery.columnsList[3].elevatorsList[0].status = "moving";
                battery.columnsList[3].elevatorsList[0].floorRequestList.Add(1);

                battery.columnsList[3].elevatorsList[1].currentFloor = 50;
                battery.columnsList[3].elevatorsList[1].direction = "up";
                battery.columnsList[3].elevatorsList[1].status = "moving";
                battery.columnsList[3].elevatorsList[1].floorRequestList.Add(60);

                battery.columnsList[3].elevatorsList[2].currentFloor = 46;
                battery.columnsList[3].elevatorsList[2].direction = "up";
                battery.columnsList[3].elevatorsList[2].status = "moving";
                battery.columnsList[3].elevatorsList[2].floorRequestList.Add(58);

                battery.columnsList[3].elevatorsList[3].currentFloor = 1;
                battery.columnsList[3].elevatorsList[3].direction = "up";
                battery.columnsList[3].elevatorsList[3].status = "moving";
                battery.columnsList[3].elevatorsList[3].floorRequestList.Add(54);

                battery.columnsList[3].elevatorsList[4].currentFloor = 60;
                battery.columnsList[3].elevatorsList[4].direction = "down";
                battery.columnsList[3].elevatorsList[4].status = "moving";
                battery.columnsList[3].elevatorsList[4].floorRequestList.Add(1);

                battery.columnsList[3].requestElevator(54, "down");

            }
            */


            /*
            void scenario4()
            {

                battery.columnsList[0].elevatorsList[0].currentFloor = -4;

                battery.columnsList[0].elevatorsList[1].currentFloor = 1;

                battery.columnsList[0].elevatorsList[2].currentFloor = -3;
                battery.columnsList[0].elevatorsList[2].direction = "down";
                battery.columnsList[0].elevatorsList[2].status = "moving";
                battery.columnsList[0].elevatorsList[2].floorRequestList.Add(-5);

                battery.columnsList[0].elevatorsList[3].currentFloor = -6;
                battery.columnsList[0].elevatorsList[3].direction = "up";
                battery.columnsList[0].elevatorsList[3].status = "moving";
                battery.columnsList[0].elevatorsList[3].floorRequestList.Add(1);

                battery.columnsList[0].elevatorsList[4].currentFloor = -1;
                battery.columnsList[0].elevatorsList[4].direction = "down";
                battery.columnsList[0].elevatorsList[4].status = "moving";
                battery.columnsList[0].elevatorsList[4].floorRequestList.Add(-6);

                battery.columnsList[0].requestElevator(-3, "up");

            }
            */

            //scenario1();
            //scenario2();
            //scenario3();
            //scenario4();

        }
    }
}
