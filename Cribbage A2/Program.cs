using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using MySql.Data.MySqlClient;


namespace Cribbage
{
    class Program
    {
        static string[] InitalisePack()
        {
            string[] Pack = new string[52];
            int Counter = 0;
            foreach (char Suit in "shcd")
            {
                for (int CardValue = 1; CardValue <= 13; CardValue++) //Ace = 1, Jack = 11, Queen = 12, King = 13
                {
                    Pack[Counter] = Suit + CardValue.ToString();
                    Counter++;
                }
            }
            return Pack;
        }

        static int PlayerNumber()
        {
            int NoOfPlayers;
            Console.Write("How many players (between 2 and 4)? : ");
            NoOfPlayers = Convert.ToInt32(Console.ReadLine());
            bool Correct = false;
            while (Correct == false)
            {
                if (NoOfPlayers > 4 || NoOfPlayers < 2)
                {
                    Console.WriteLine("That is not a valid input. Please try again: ");
                    NoOfPlayers = Convert.ToInt32(Console.ReadLine());
                }
                Correct = true;
            }

            return NoOfPlayers;
        }

        static string[,] GetPlayerNames(int NoOfPlayers)
        {
            string[,] PlayerNames = new string[NoOfPlayers, 2];
            for (int Current = 0; Current < NoOfPlayers; Current++)
            {
                Console.Write("Input player {0} name: ", Current);
                PlayerNames[Current, 0] = (Console.ReadLine());
            }
            return PlayerNames;
        }

        static string[,] GetDatabasePlayerNames(int NoOfPlayers)
        {
            string[,] PlayerNames = new string[NoOfPlayers, 2];

            string MyConnectionstring = "Server=localhost;Database=cribbage_schema;Uid=root;Pwd=paintball";
            using (MySqlConnection connection = new MySqlConnection(MyConnectionstring))
            {
                using (MySqlCommand cmd = connection.CreateCommand())
                {
                    cmd.CommandText = @"SELECT * FROM player_info";
                    Console.WriteLine();
                    connection.Open();
                    MySqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        Console.Write(reader.GetInt16("Player_ID")+ " " + reader.GetString("First_Name") + " " + reader.GetString("Last_Name"));
                        Console.WriteLine();
                    }
                    reader.Close();
                    

                }
                using (MySqlCommand cmd = connection.CreateCommand())
                {
                    string DatabasePlayerID;
                    for (int Current = 0; Current < NoOfPlayers; Current++)
                    {
                        Console.Write("Input player {0} ID: ", Current);
                        DatabasePlayerID = (Console.ReadLine()); 
                        cmd.CommandText = @"SELECT * FROM player_info WHERE Player_ID = '" + DatabasePlayerID + "'";
                        MySqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            PlayerNames[Current, 0] = reader.GetString("First_Name") + " " + reader.GetString("Last_Name");
                            PlayerNames[Current, 1] = reader.GetString("Player_ID");
                        }
                        reader.Close();
                        Console.WriteLine(PlayerNames[Current, 0]);
                        Console.WriteLine(PlayerNames[Current, 1]);
                      
                    }
                    
                }
                connection.Close();
            }
            
            return PlayerNames;
        }

        static int ChooseDealer(int NoOfPlayers)
        {
            Random r = new Random();
            int Dealer = r.Next(0, NoOfPlayers - 1);
            return Dealer;
        }

        static string[] Shuffle(String[] Pack)
        {
            // Fisher - Yates shuffle
            Random r = new Random();
            for (int Count = Pack.Length - 1; Count > 0; Count--)
            {
                int RandomCard = r.Next(Count + 1);
                string Temp = Pack[Count];
                Pack[Count] = Pack[RandomCard];
                Pack[RandomCard] = Temp;
            }
            return Pack;
        }

        static void PlayTheGame(string[] Pack, int NoOfPlayers, string[,] PlayerNames, int Dealer)
        {
            int CardsDealtPlayer = 0;
            int CardsDealtBox = 0;
            int CardsDiscard = 0;
            string DiscardCard;




            switch (NoOfPlayers)
            {
                case 2:
                    CardsDealtPlayer = 6;
                    CardsDealtBox = 0;
                    CardsDiscard = 2;
                    break;
                case 3:
                    CardsDealtPlayer = 5;
                    CardsDealtBox = 1;
                    CardsDiscard = 1;
                    break;
                case 4:
                    CardsDealtPlayer = 5;
                    CardsDealtBox = 0;
                    CardsDiscard = 1;
                    break;
            }

            string[,] Hands = new string[NoOfPlayers, CardsDealtPlayer];
            string[] Box = new string[CardsDealtBox + (NoOfPlayers * CardsDiscard)];
            int[] PlayerScores = new int[NoOfPlayers];

            bool GameFinish = false;

                Pack = Shuffle(Pack);
                int TopCard = 0;
                for (int CardsDealt = 0; CardsDealt < CardsDealtPlayer; CardsDealt++)
                {
                    for (int EachPlayer = 0; EachPlayer < NoOfPlayers; EachPlayer++)
                    {
                        Console.WriteLine("{0} {1} {2}", EachPlayer, CardsDealt, TopCard);
                        Hands[EachPlayer, CardsDealt] = Pack[TopCard];
                        Pack[TopCard] = "";
                        TopCard = TopCard + 1;
                    }
                }
                for (int BoxCards = 0; BoxCards < CardsDealtBox; BoxCards++)
                {
                    Box[BoxCards] = Pack[TopCard];
                    Pack[TopCard] = "";
                    TopCard = TopCard + 1;
                }
                Console.WriteLine("Please choose which card(s) you wish to discard.");
                for (int Player = 0; Player < NoOfPlayers; Player++)
                {
                    for (int NumberOfDiscards = 0; NumberOfDiscards < CardsDiscard; NumberOfDiscards++)
                    {
                        Console.WriteLine("This is player {0} hand.", PlayerNames[Player, 0]);
                        DisplayHand(Hands, Player);
                        DiscardCard = (Console.ReadLine()); //to do, add cheak for mistake input
                        Hands = RemoveCardHand(Hands, Player, DiscardCard);
                        Box = AddCardPile(Box, DiscardCard);
                        DisplayHand(Hands, Player);

                        Console.WriteLine("Box");
                        DisplayBox(Box);
                    }
                }

                //Play the Play

                int CurrentPlayer = NextPlayer(Dealer, NoOfPlayers);
                string[,] LiveHands = Hands; //Need to have the same hand for "dab down"
            while (GameFinish == false) //here prob
            {
                int ScoreRound = 0;
                string[] CardsPlayedInRound = new string[NoOfPlayers * CardsDealtPlayer];
                
                bool RoundFinished = false;
                while (!RoundFinished)
                {
                    Console.WriteLine("Player {0} turn", PlayerNames[CurrentPlayer, 0]);
                    DisplayHand(LiveHands, CurrentPlayer);
                    if (CanPlayerPlay(ScoreRound, LiveHands, CurrentPlayer))
                    {
                        Console.WriteLine("Choose card to play");
                        string CardPlayed = (Console.ReadLine());
                        LiveHands = RemoveCardHand(LiveHands, CurrentPlayer, CardPlayed);
                        CardsPlayedInRound = AddCardPile(CardsPlayedInRound, CardPlayed);
                        ScoreRound = ScoreRound + CardScore(CardPlayed);
                        PlayerScores = CheckPlayScore(CardsPlayedInRound, PlayerScores, CurrentPlayer, ScoreRound);
                        Console.WriteLine("Score for card played: {0}", PlayerScores[CurrentPlayer]);
                    }
                    RoundFinished = !CanCardBePlayed(ScoreRound, LiveHands);
                    CurrentPlayer = NextPlayer(CurrentPlayer, NoOfPlayers);
                    Console.WriteLine("Score for round: {0}", ScoreRound);
                }

                Console.WriteLine("End");
                Console.ReadLine();
            }
        }

        static void DisplayHand(string[,] Hands, int Player)
        {
            for (int Cards = 0; Cards <= Hands.GetUpperBound(1); Cards++)
            {
                Console.Write("{0} - ", Hands[Player, Cards]);
            }
            Console.WriteLine();
        }

        static void DisplayBox(string[] Box)
        {
            for (int Cards = 0; Cards <= Box.GetUpperBound(0); Cards++)
            {
                Console.Write("{0} - ", Box[Cards]);
            }
            Console.WriteLine();
        }

        static string[,] RemoveCardHand(string[,] Hands, int Player, string CardToRemove)
        {
            for (int Cards = 0; Cards <= Hands.GetUpperBound(1); Cards++)
            {
                if (Hands[Player, Cards] == CardToRemove)
                {
                    Hands[Player, Cards] = "";
                }
            }
            return Hands;
        }

        static string[] AddCardPile(string[] Pile, string CardToAdd)
        {
            int Cards = 0;
            while (!string.IsNullOrEmpty(Pile[Cards]))
            {
                Cards++;
            }
            Pile[Cards] = CardToAdd;
            return Pile;
        }

        static int NextPlayer(int Player, int NoPlayer)
        {
            if (Player == NoPlayer - 1)
            {
                Player = 0;
            }
            else
            {
                Player++;
            }
            return Player;
        }

        static int CardScore(string Card)
        {
            if(Card == "")
            {
                return 0;
            }
            int CardValue = Convert.ToInt32(Card.Substring(1));
            if (CardValue > 10)
            {
                CardValue = 10;
            }
            return CardValue;
        }

        static int[] CheckPlayScore(string[] CardsPlayed, int[] PlayerScores, int Player, int ScoreRound)
        {
            int NumberOfCardsPlayed = 0;
            while (!string.IsNullOrEmpty(CardsPlayed[NumberOfCardsPlayed]))
            {
                NumberOfCardsPlayed++;
            }
            //Console.Write("Cards Played . Length: ");
            //Console.WriteLine(NumberOfCardsPlayed);
            if (NumberOfCardsPlayed < 2)
            {
                return PlayerScores;
            }
            else
            {
                if (NumberOfCardsPlayed > 2)
                {
                    //Check 3 of a kind
                    if (CardsPlayed[NumberOfCardsPlayed - 1].Substring(1) == CardsPlayed[NumberOfCardsPlayed - 2].Substring(1) && CardsPlayed[NumberOfCardsPlayed - 2].Substring(1) == CardsPlayed[NumberOfCardsPlayed - 3].Substring(1))
                    {
                        PlayerScores[Player] = PlayerScores[Player] + 6;
                    }
                }
                if (NumberOfCardsPlayed > 3)
                {
                    //Check 4 of a kind
                    if (CardsPlayed[NumberOfCardsPlayed - 1].Substring(1) == CardsPlayed[NumberOfCardsPlayed - 2].Substring(1) && CardsPlayed[NumberOfCardsPlayed - 2].Substring(1) == CardsPlayed[NumberOfCardsPlayed - 3].Substring(1) && CardsPlayed[NumberOfCardsPlayed - 3].Substring(1) == CardsPlayed[NumberOfCardsPlayed - 4].Substring(1))
                    {
                        PlayerScores[Player] = PlayerScores[Player] + 12;
                    }
                }
                //Check Pair
                if (CardsPlayed[NumberOfCardsPlayed - 1].Substring(1) == CardsPlayed[NumberOfCardsPlayed - 2].Substring(1))
                {
                    PlayerScores[Player] = PlayerScores[Player] + 2;
                }
                //Check total 15
                if (ScoreRound == 15)
                {
                    PlayerScores[Player] = PlayerScores[Player] + 2;
                }
                //Check total 31
                if (ScoreRound == 31)
                {
                    PlayerScores[Player] = PlayerScores[Player] + 2;
                }

                return PlayerScores;
            }
        }

        static bool CanCardBePlayed(int ScoreRound, string[,] LiveHands)
        {
            int NoOfPlayers = LiveHands.GetUpperBound(0);
            int NoOfCards = LiveHands.GetUpperBound(1);
            if(ScoreRound == 31)
            {
                Console.WriteLine("31 reached");
                return false;
            }
            for(int Player = 0; Player <= NoOfPlayers; Player++)
            {
                for(int Card = 0; Card <= NoOfCards; Card++)
                {
                    if (CardScore(LiveHands[Player, Card]) == 0)
                    {
                        Console.WriteLine("Blank card");
                    }
                    else if(CardScore(LiveHands[Player, Card]) <= 31 - ScoreRound)
                    {
                        Console.WriteLine("Card Score {0}, card {1}", CardScore(LiveHands[Player, Card]), Card);
                        return true;
                    }
                    
                }
            }
            Console.WriteLine("No more cards can be played.");
            return false;
        }

        static bool CanPlayerPlay(int Score, string[,] LiveHands, int PlayersTurn)
        {
            int NoOfCards = LiveHands.GetUpperBound(1);
                for (int Card = 0; Card <= NoOfCards; Card++)
                {
                    Console.WriteLine(LiveHands[PlayersTurn, Card]);
                    if (CardScore(LiveHands[PlayersTurn, Card]) == 0 )
                    {
                        Console.WriteLine("Blank card");
                    }
                    else if(CardScore(LiveHands[PlayersTurn, Card]) <= 31 - Score)
                    {

                        return true;
                    }

                }
            Console.WriteLine("Player {0} can't play a card. Moving to next player.", PlayersTurn);
            return false;
        }

        static void AddNewPersonToDatabase()
        {
            int input = 1;

            while (input == 1)
            {
                string a, b, d;
                int c;
                Console.WriteLine("Please input data: 1. First Name");
                a = (Console.ReadLine());
                Console.WriteLine("2. Last Name");
                b = (Console.ReadLine());
                Console.WriteLine("3. Player Rank");
                c = int.Parse(Console.ReadLine());
                Console.WriteLine("4. Email");
                d = (Console.ReadLine());
                string MyConnectionstring = "Server=localhost;Database=cribbage_schema;Uid=root;Pwd=paintball";

                MySqlConnection connection = new MySqlConnection(MyConnectionstring);
                MySqlCommand cmd;
                connection.Open();
                cmd = connection.CreateCommand();
                cmd.CommandText = "INSERT INTO Player_info(First_Name, Last_Name, Player_Rank, Email)VALUES(";
                cmd.CommandText = cmd.CommandText + "'" + a + "','" + b + "'," + c + ",'" + d + "')";
                Console.WriteLine(cmd.CommandText);
                cmd.ExecuteNonQuery();
                connection.Close();
                Console.WriteLine("Would you like to enter a new contestant? 1 = yes, 0 = no");
                input = int.Parse(Console.ReadLine());
            }
            Console.WriteLine("Player names inputed.");
        }

        static void PlayStandardGame()
        {
            string[] LivePack = new string[52];
            int NoOfPlayers;
            int Dealer;
            NoOfPlayers = PlayerNumber();
            string[,] PlayerNames = new string[NoOfPlayers, 2];
            PlayerNames = GetPlayerNames(NoOfPlayers);
            LivePack = InitalisePack();
            Dealer = ChooseDealer(NoOfPlayers);
            Console.WriteLine(Dealer);
            PlayTheGame(LivePack, NoOfPlayers, PlayerNames, Dealer);
        }

        static void PlayTournamentGame()
        {
            string[] LivePack = new string[52];
            int NoOfPlayers;
            int Dealer;
            NoOfPlayers = PlayerNumber();
            string[,] PlayerNames = new string[NoOfPlayers, 2];
            PlayerNames = GetDatabasePlayerNames(NoOfPlayers);
            LivePack = InitalisePack();
            Dealer = ChooseDealer(NoOfPlayers);
            Console.WriteLine(Dealer);
            PlayTheGame(LivePack, NoOfPlayers, PlayerNames, Dealer);
        }

        static void Main(string[] args)
        {
            int Thng = 1;
            Console.WriteLine("Welcome to the cribbage program!");
            Console.WriteLine("");
            while (Thng != 0)
            {
                Console.WriteLine("Enter something: \n 0 = End Program \n 1 = Play Standard Game \n 2 = Play tournament game \n 3 = Add new player(s) to tournament");
                Thng = Convert.ToInt32(Console.ReadLine());
                switch (Thng)
                {
                    case 0:
                        Console.WriteLine("Thats all folks");
                        System.Environment.Exit(0);
                        break;
                    case 1:
                        Console.WriteLine("You pressed 1, welcome to the game!");
                        PlayStandardGame();
                        break;
                    case 2:
                        Console.WriteLine("Please choose players to take part in the tournament");
                        PlayTournamentGame();
                        break;
                    case 3:
                        Console.WriteLine("Please enter new player imformation");
                        AddNewPersonToDatabase();
                        break;
                    default:
                        Console.WriteLine("You pressed the wrong button");
                        break;
                }
            }
            /*
            LivePack = InitalisePack();
            Dealer = ChooseDealer(NoOfPlayers);
            Console.WriteLine(Dealer);



            PlayTheGame(LivePack, NoOfPlayers, PlayerNames, Dealer);

            //Hands = DealHands(LivePack, CardsDealtPlayer, CardsDealtBox, NoOfPlayers);
            //Box = DealBox(LivePack, CardsDiscard);
            for (int i = 0; i < 52; i++)
            {
                Console.WriteLine(LivePack[i]);
            }
            //PlayThePlay(NoOfPlayers, PlayerNames, Dealer, LivePack);
            Console.Read();
             */
        }
    }
}
