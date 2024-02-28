using System;
using System.Collections.Generic;
using System.Linq;

namespace PassengerTrainConfigurator
{
	internal class PassengerTrainConfigurator
	{
		static void Main()
		{
			const string CommandCreateDirection = "1";
			const string CommandExit = "2";

			Station station = new Station();

			bool isProgramActive = true;

			while (isProgramActive)
			{
				station.ShowStatus();
				Console.WriteLine("Список команд:\n" +
						$"{CommandCreateDirection} - создать направление\n" +
						$"{CommandExit} - выход из программы");

				Console.Write("Введите команду: ");
				string input = Console.ReadLine();

				switch (input)
				{
					case CommandCreateDirection:
						station.PerformTrainOperations();
						break;

					case CommandExit:
						isProgramActive = false;
						break;

					default:
						Console.WriteLine("Неизвестная команда!");
						break;
				}

				station.Clear();
			}
		}
	}
}

class Direction
{
	public Direction(string departureStation, string arrivalStation)
	{
		DepartureStation = departureStation;
		ArrivalStation = arrivalStation;
	}

	public string DepartureStation { get; private set; }
	public string ArrivalStation { get; private set; }
}

class Station
{
	private Dictionary<WagonType, int> _seatsPerWagon;

	private List<Train> _trains = new List<Train>();

	private Dictionary<WagonType, int> _numberPassengersInWagonsOfType;

	private Dictionary<WagonType, int> _maximumNumberPassengersInWagon;

	private int _totalNumberPassengers = 0;

	private Random _random = new Random();

	public Station()
	{
		_seatsPerWagon = new Dictionary<WagonType, int>
		{
			{WagonType.Sleeper, 18},
			{WagonType.Compartment, 36 },
			{WagonType.Plazkart, 54 }
		};

		_numberPassengersInWagonsOfType = new Dictionary<WagonType, int>();

		_maximumNumberPassengersInWagon = new Dictionary<WagonType, int>
		{
			{ WagonType.Sleeper, 167},
			{ WagonType.Compartment, 333 },
			{ WagonType.Plazkart, 500 }
		};
	}

	public void PerformTrainOperations()
	{
		if (CreateDirection() is Direction)
		{
			Clear();
			ShowStatus();
			SellTickets();
			Clear();
			ShowStatus();
			CreateWagons();
			Clear();
			ShowStatus();
			SendTrain();
		}
	}

	public void Clear()
	{
		Console.Write("Нажмите любую кнопку чтобы продолжить: ");
		Console.ReadKey();
		Console.Clear();
	}
	public Direction CreateDirection()
	{
		Console.Write("\nВведите станцию отправления: ");
		string departure = Console.ReadLine();

		Console.Write("Введите станцию назначения: ");
		string arrival = Console.ReadLine();

		if (departure.ToLower() == arrival.ToLower())
		{
			Console.WriteLine("Пункт отправления не может быть равен пункту прибытия!");
			return null;
		}

		Direction direction = new Direction(departure, arrival);

		Console.WriteLine("Создано новое направление.");

		_trains.Add(new Train(direction, 0, TrainStatus.DirectionCreated));
		return direction;
	}


	public void ShowStatus()
	{
		Console.SetCursorPosition(0, 0);
		Console.WriteLine("Действующие направления: ");


		foreach (Train train in _trains)
		{
			Direction direction = train.Direction;
			Console.WriteLine($"{direction.DepartureStation} - {direction.ArrivalStation}. {train.Status}");
			if (train.Status == TrainStatus.TrainFormed || train.Status == TrainStatus.TrainSent)
				train.DisplayInfo();
		}
	}

	public void SellTickets()
	{

		int minimumNumberPassengers = 1;

		int numberPassengersInWagon = 0;

		_numberPassengersInWagonsOfType = new Dictionary<WagonType, int>();

		foreach (WagonType wagonType in _maximumNumberPassengersInWagon.Keys)
		{
			numberPassengersInWagon = _random.Next(minimumNumberPassengers,
				_maximumNumberPassengersInWagon[wagonType] + 1);

			_numberPassengersInWagonsOfType.Add(wagonType, numberPassengersInWagon);

			_totalNumberPassengers += numberPassengersInWagon;

			Console.WriteLine($"Продано билетов в {wagonType}: {numberPassengersInWagon}");
		}

		Console.WriteLine("Билеты проданы");


		TrainStatus trainStatus = TrainStatus.TicketsSold;

		_trains[_trains.Count - 1].Update(_totalNumberPassengers, trainStatus, null);

	}

	public void CreateWagons()
	{

		Train currentTrain = _trains[_trains.Count - 1];

		List<Wagon> wagons = new List<Wagon>();

		foreach (WagonType wagonType in _numberPassengersInWagonsOfType.Keys)
		{
			int numberWagonsOfType = CalculateWagonCount(wagonType, _numberPassengersInWagonsOfType[wagonType]);
			FillWagons(wagons, numberWagonsOfType, wagonType, _numberPassengersInWagonsOfType[wagonType]);
		}

		TrainStatus trainStatus = TrainStatus.TrainFormed;

		currentTrain.Update(_totalNumberPassengers, trainStatus, wagons);

		Console.WriteLine($"Поезд успешно сформирован для направления {currentTrain.Direction.DepartureStation}" +
						  $" - {currentTrain.Direction.ArrivalStation}.");
	}

	private void FillWagons(List<Wagon> wagons, int wagonCount, WagonType wagonType, int numberPassengers)
	{
		int seatsPerWagon = GetSeatsPerWagon(wagonType);

		for (int i = 0; i < wagonCount; i++)
			wagons.Add(new Wagon(wagonType, numberPassengers, seatsPerWagon));
	}

	public void SendTrain()
	{
		Train selectedTrain = _trains[_trains.Count - 1];

		Console.WriteLine($"Поезд отправлен.");
		TrainStatus trainStatus = TrainStatus.TrainSent;
		selectedTrain.Update(_totalNumberPassengers, trainStatus, null);
		selectedTrain.DisplayInfo();
	}

	private int GetSeatsPerWagon(WagonType type)
	{
		if (_seatsPerWagon.TryGetValue(type, out var seats))
		{
			return seats;
		}
		else
		{
			Console.WriteLine("Неизвестный тип вагона.");
			return 0;
		}
	}

	private int CalculateWagonCount(WagonType type, int numberPassengers)
	{
		int seats = GetSeatsPerWagon(type);
		return (int)Math.Ceiling((double)numberPassengers / seats);
	}
}

enum WagonType
{
	Sleeper,
	Compartment,
	Plazkart
}

class Wagon
{
	public Wagon(WagonType type, int numberOccupiedSeats, int totalNumberSeats)
	{
		Type = type;
		NumberOccupiedSeats = numberOccupiedSeats;
		TotalNumberSeats = totalNumberSeats;
	}

	public WagonType Type { get; private set; }
	public int NumberOccupiedSeats { get; private set; }
	public int TotalNumberSeats { get; private set; }
}

public enum TrainStatus
{
	NoDirection,
	DirectionCreated,
	TicketsSold,
	TrainFormed,
	TrainSent
}

class Train
{
	private int _totalNumberPassengers;

	private List<Wagon> _wagons;

	public Train(Direction direction, int totalNumberPassengers, TrainStatus status = TrainStatus.NoDirection)
	{
		Direction = direction;
		_wagons = new List<Wagon>();
		_totalNumberPassengers = totalNumberPassengers;
		Status = status;
	}

	public Direction Direction { get; private set; }
	public TrainStatus Status { get; private set; }


	public void Update(int totalNumberPassengers, TrainStatus status, List<Wagon> wagons)
	{
		_totalNumberPassengers = totalNumberPassengers;
		Status = status;
		SetWagons(wagons);
	}

	public void DisplayInfo()
	{
		Console.WriteLine($"Информация о рейсе: маршрут - {Direction.DepartureStation} - {Direction.ArrivalStation}, \n" +
			$"вместимость поезда - {CalculateTotalNumberSeats()}, общее количество вагонов - {_wagons.Count}, \n" +
			$"общее количество пассажиров - {_totalNumberPassengers}.");

		Console.WriteLine($"Св вагонов - {CalculateWagonCount(WagonType.Sleeper)}, \n" +
			$"купе - {CalculateWagonCount(WagonType.Compartment)}, плацкарт - {CalculateWagonCount(WagonType.Plazkart)}");
	}

	private int CalculateTotalNumberSeats()
	{
		int totalNumberSeats = 0;

		foreach (Wagon wagon in _wagons)
			totalNumberSeats += wagon.TotalNumberSeats;

		return totalNumberSeats;
	}

	private int CalculateWagonCount(WagonType type)
	{
		int wagonCount = 0;

		foreach (Wagon wagon in _wagons)
		{
			if (wagon.Type == type)
				wagonCount++;
		}

		return wagonCount;
	}
	private void SetWagons(List<Wagon> wagons)
	{
		if (wagons == null)
			return;

		for (int i = 0; i < wagons.Count; i++)
		{
			_wagons.Add(wagons.ElementAt(i));
		}
	}
}