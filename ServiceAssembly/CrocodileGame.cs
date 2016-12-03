using System;
using System.Linq;
using System.Timers;

namespace ServiceAssembly
{
    public class CrocodileGame
    {
        public GameSettings Settings { get; private set; }
        public bool IsActive { get; private set; }

        private GameRoomService GameService { get;}

        private Timer _sendLetterTimer;

        protected CrocodileGame()
        {
            Settings = null;
        }

        public CrocodileGame(GameRoomService service)
        {
            Settings = null;
            GameService = service;
        }
        
        public void StarGame(GameSettings settings)
        {
            Settings = settings;

            _sendLetterTimer = new Timer(40000);

            IsActive = true;

            _sendLetterTimer.Elapsed += _sendLetterTimer_Elapsed;


            _sendLetterTimer.Start();

            GameService.StartNewGame();
            GameService.SendWordInfo(settings.Word.Length);
        }

        public void EndGame()
        {
            if (!IsActive) return;

            Settings = null;
            IsActive = false;

            _sendLetterTimer.Stop();

            GameService.EndGame();
        }

        public void EndGame(Client client)
        {

            if(!IsActive) return;

            client.Score += 5;
            Settings.DrawingClient.Score += 10;

            GameService.Say(new Message(GameService.Admin,$"Game ended!\n {client.Name} guessed the word:{Settings.Word}\n"));

            EndGame();
        }

        private void _sendLetterTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (Settings.Shownletters.Count == Settings.Word.Length - 1)
            {
                GameService.Say(new Message(GameService.Admin, $"Time out!\nGame ended!\n No one guessed the word:{Settings.Word}\n"));
                EndGame();
            }
            else 
                SendRandomLetter();
        }

        private char SendRandomLetter()
        {
            string w = Settings.Word;
            
            Random rand=new Random();
            int randPosition;
            do
            {
                randPosition = rand.Next(0, w.Length);
            } while (Settings.Shownletters.Contains(randPosition));

            Settings.Shownletters.Add(randPosition);
            GameService.SendLetter(w.ElementAt(randPosition),randPosition);

            return w.ElementAt(randPosition);
        }


        public bool CheckWord(string word)
        {
            return string.Equals(Settings.Word.ToUpper(), word.Trim().ToUpper());
        }
    }
}