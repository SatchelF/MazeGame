using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace MazeGame
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private SpriteFont titleFont;
        private SpriteFont buttonFont;
        private Texture2D backgroundTexture;
        private Texture2D pixelTexture;
        private const int MazeDisplayWidth = 525;
        private const int MazeDisplayHeight = 525;
        private KeyboardState previousKeyboardState;
        private GameState currentState;
        private MazeGenerator mazeGenerator;
        private TimeSpan gameTimer; // Timer to track elapsed game time
        private string highScore = ""; // Placeholder for high score display

        public enum GameState
        {
            MainMenu,
            Playing,
            HighScores,
            Credits
        }

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            graphics.PreferredBackBufferWidth = 1000;
            graphics.PreferredBackBufferHeight = 800;
            graphics.ApplyChanges();
            currentState = GameState.MainMenu;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            titleFont = Content.Load<SpriteFont>("Title");
            buttonFont = Content.Load<SpriteFont>("button");
            pixelTexture = new Texture2D(GraphicsDevice, 1, 1);
            pixelTexture.SetData(new[] { Color.White });
            backgroundTexture = Content.Load<Texture2D>("maze-background2");
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            ProcessInput();
            if (currentState == GameState.Playing)
            {
                gameTimer += gameTime.ElapsedGameTime; // Update game timer
            }
        }

        private void ProcessInput()
        {
            KeyboardState currentKeyboardState = Keyboard.GetState();

            // Process input for starting new games and navigating menus
            if ((currentState == GameState.MainMenu || currentState == GameState.Playing))
            {
                if (currentKeyboardState.IsKeyDown(Keys.F1) && previousKeyboardState.IsKeyUp(Keys.F1)) StartGame(5);
                else if (currentKeyboardState.IsKeyDown(Keys.F2) && previousKeyboardState.IsKeyUp(Keys.F2)) StartGame(10);
                else if (currentKeyboardState.IsKeyDown(Keys.F3) && previousKeyboardState.IsKeyUp(Keys.F3)) StartGame(15);
                else if (currentKeyboardState.IsKeyDown(Keys.F4) && previousKeyboardState.IsKeyUp(Keys.F4)) StartGame(20);
            }

            if (currentState == GameState.MainMenu)
            {
                if (currentKeyboardState.IsKeyDown(Keys.F5) && previousKeyboardState.IsKeyUp(Keys.F5)) currentState = GameState.HighScores;
                else if (currentKeyboardState.IsKeyDown(Keys.F6) && previousKeyboardState.IsKeyUp(Keys.F6)) currentState = GameState.Credits;
            }

            previousKeyboardState = currentKeyboardState;
        }

        private void StartGame(int size)
        {
            currentState = GameState.Playing;
            mazeGenerator = new MazeGenerator(size, size);
            mazeGenerator.GenerateMaze();
            gameTimer = TimeSpan.Zero; // Reset game timer
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin();

            // Draw game title
            Vector2 titleSize = titleFont.MeasureString("The Maze Game");
            spriteBatch.DrawString(titleFont, "The Maze Game", new Vector2((graphics.PreferredBackBufferWidth - titleSize.X) / 2, 20), Color.GhostWhite);

            // Draw menu choices
            if (currentState == GameState.MainMenu || currentState == GameState.Playing)
            {
                DrawMenuChoices();
            }

            // Draw maze and timer
            if (currentState == GameState.Playing)
            {
                DrawMaze();
                DrawTimer();
                DrawHighScore();
            }

            spriteBatch.End();
            base.Draw(gameTime);
        }

        private void DrawMenuChoices()
        {
            float menuStartY = 100;
            float menuItemSpacing = 60;

            spriteBatch.DrawString(buttonFont, "F1 - New Game 5x5", new Vector2(10, menuStartY), Color.GhostWhite);
            spriteBatch.DrawString(buttonFont, "F2 - New Game 10x10", new Vector2(10, menuStartY + menuItemSpacing), Color.GhostWhite);
            spriteBatch.DrawString(buttonFont, "F3 - New Game 15x15", new Vector2(10, menuStartY + menuItemSpacing * 2), Color.GhostWhite);
            spriteBatch.DrawString(buttonFont, "F4 - New Game 20x20", new Vector2(10, menuStartY + menuItemSpacing * 3), Color.GhostWhite);
            spriteBatch.DrawString(buttonFont, "F5 - High Scores", new Vector2(10, menuStartY + menuItemSpacing * 4), Color.GhostWhite);
            spriteBatch.DrawString(buttonFont, "F6 - Credits", new Vector2(10, menuStartY + menuItemSpacing * 5), Color.GhostWhite);
        }

        private void DrawMaze()
        {
            if (mazeGenerator == null) return;

            int cellSize = Math.Min(MazeDisplayWidth / mazeGenerator.width, MazeDisplayHeight / mazeGenerator.height);
            int mazeWidth = cellSize * mazeGenerator.width;
            int mazeHeight = cellSize * mazeGenerator.height;
            Vector2 mazePosition = new Vector2((graphics.PreferredBackBufferWidth - mazeWidth) / 2, (graphics.PreferredBackBufferHeight - mazeHeight) / 2);

            spriteBatch.Draw(backgroundTexture, new Rectangle((int)mazePosition.X, (int)mazePosition.Y, mazeWidth, mazeHeight), Color.White);

            int lineWidth = 2;

            spriteBatch.Draw(pixelTexture, new Rectangle((int)mazePosition.X, (int)mazePosition.Y - (lineWidth / 2), mazeWidth, lineWidth), null, Color.White);
            spriteBatch.Draw(pixelTexture, new Rectangle((int)mazePosition.X - (lineWidth / 2), (int)mazePosition.Y, lineWidth, mazeHeight), null, Color.White);

            for (int y = 0; y < mazeGenerator.height; y++)
            {
                for (int x = 0; x < mazeGenerator.width; x++)
                {
                    Cell cell = mazeGenerator.grid[x, y];
                    Vector2 cellPosition = new Vector2(x * cellSize, y * cellSize) + mazePosition;

                    if (cell.Edges["e"] == null || !cell.Edges["e"].Visited)
                    {
                        spriteBatch.Draw(pixelTexture, new Rectangle((int)cellPosition.X + cellSize - (lineWidth / 2), (int)cellPosition.Y, lineWidth, cellSize), null, Color.White);
                    }

                    if (cell.Edges["s"] == null || !cell.Edges["s"].Visited)
                    {
                        spriteBatch.Draw(pixelTexture, new Rectangle((int)cellPosition.X, (int)cellPosition.Y + cellSize - (lineWidth / 2), cellSize, lineWidth), null, Color.White);
                    }
                }
            }
        }

        private void DrawTimer()
        {
            // Draw timer above the maze
            string timerText = "Time: " + gameTimer.TotalSeconds.ToString("F1") + "s";
            Vector2 timerSize = buttonFont.MeasureString(timerText);
            spriteBatch.DrawString(buttonFont, timerText, new Vector2((graphics.PreferredBackBufferWidth - timerSize.X) / 2, 70), Color.GhostWhite);
        }

        private void DrawHighScore()
        {
            // Draw high score next to the timer
            string highScoreText = "High Score: " + highScore;
            Vector2 highScoreSize = buttonFont.MeasureString(highScoreText);
            spriteBatch.DrawString(buttonFont, highScoreText, new Vector2((graphics.PreferredBackBufferWidth - highScoreSize.X) / 2, 100), Color.GhostWhite);
        }
    }
}
