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
        private Texture2D backgroundTexture; // Background texture for the maze area
        private Texture2D pixelTexture; // 1x1 pixel texture for drawing lines

        private const int MazeDisplayWidth = 525; // Adjusted width
        private const int MazeDisplayHeight = 525; // Adjusted height
        private KeyboardState previousKeyboardState;
        private TimeSpan gameTimer; // Timer to track elapsed game time
        private string highScore = ""; // Placeholder for high score display

        // Game state management
        public enum GameState
        {
            MainMenu,
            Playing,
            HighScores,
            Credits
        }

        private GameState currentState;

        // Maze Generator
        private MazeGenerator mazeGenerator;

        // Player variables
        private Texture2D playerTexture;
        private Point playerGridPosition;
        private Vector2 playerScreenPosition;
        private int cellSize;

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

            // Create and set a 1x1 white pixel texture
            pixelTexture = new Texture2D(GraphicsDevice, 1, 1);
            pixelTexture.SetData(new[] { Color.White });

            // Load the background texture
            backgroundTexture = Content.Load<Texture2D>("maze-background2");

            // Load the player texture
            playerTexture = Content.Load<Texture2D>("maze-player");
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

            // Check for new game key presses only if they were not pressed in the previous frame
            if ((currentState == GameState.MainMenu || currentState == GameState.Playing))
            {
                if (currentKeyboardState.IsKeyDown(Keys.F1) && previousKeyboardState.IsKeyUp(Keys.F1)) StartGame(5);
                else if (currentKeyboardState.IsKeyDown(Keys.F2) && previousKeyboardState.IsKeyUp(Keys.F2)) StartGame(10);
                else if (currentKeyboardState.IsKeyDown(Keys.F3) && previousKeyboardState.IsKeyUp(Keys.F3)) StartGame(15);
                else if (currentKeyboardState.IsKeyDown(Keys.F4) && previousKeyboardState.IsKeyUp(Keys.F4)) StartGame(20);
            }

            // Continue to allow navigation to HighScores and Credits only from the MainMenu
            if (currentState == GameState.MainMenu)
            {
                if (currentKeyboardState.IsKeyDown(Keys.F5) && previousKeyboardState.IsKeyUp(Keys.F5)) currentState = GameState.HighScores;
                else if (currentKeyboardState.IsKeyDown(Keys.F6) && previousKeyboardState.IsKeyUp(Keys.F6)) currentState = GameState.Credits;
            }

            // Update the previous keyboard state at the end of the method
            previousKeyboardState = currentKeyboardState;
        }

        private void StartGame(int size)
        {
            currentState = GameState.Playing;
            mazeGenerator = new MazeGenerator(size, size);
            mazeGenerator.GenerateMaze();

            // Calculate cell size and update player position
            cellSize = Math.Min(MazeDisplayWidth / size, MazeDisplayHeight / size);
            playerGridPosition = new Point(0, 0); // Start at the top-left corner of the maze
            UpdatePlayerScreenPosition();
            gameTimer = TimeSpan.Zero; // Reset game timer
        }

        private void UpdatePlayerScreenPosition()
        {
            Vector2 mazePosition = new Vector2((graphics.PreferredBackBufferWidth - MazeDisplayWidth) / 2, (graphics.PreferredBackBufferHeight - MazeDisplayHeight) / 2);
            playerScreenPosition = new Vector2(playerGridPosition.X * cellSize + mazePosition.X, playerGridPosition.Y * cellSize + mazePosition.Y);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin();

            // Draw the game title at the top center
            Vector2 titleSize = titleFont.MeasureString("The Maze Game");
            spriteBatch.DrawString(titleFont, "The Maze Game", new Vector2((graphics.PreferredBackBufferWidth - titleSize.X) / 2, 20), Color.GhostWhite);

            // Draw menu choices on the left if in MainMenu or Playing state
            if (currentState == GameState.MainMenu || currentState == GameState.Playing)
            {
                DrawMenuChoices();
            }

            // Center and draw the maze in the remaining space if in Playing state
            if (currentState == GameState.Playing)
            {
                DrawMaze();
                DrawPlayer();
                DrawHighScore();
                DrawTimer();
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

            int mazeWidth = cellSize * mazeGenerator.width;
            int mazeHeight = cellSize * mazeGenerator.height;
            Vector2 mazePosition = new Vector2((graphics.PreferredBackBufferWidth - mazeWidth) / 2, (graphics.PreferredBackBufferHeight - mazeHeight) / 2);

            // Draw the background scaled to the size of the maze
            spriteBatch.Draw(backgroundTexture, new Rectangle((int)mazePosition.X, (int)mazePosition.Y, mazeWidth, mazeHeight), Color.White);

            int lineWidth = 2; // Adjust the line width as needed

            // Draw top border
            spriteBatch.Draw(pixelTexture, new Rectangle((int)mazePosition.X, (int)mazePosition.Y - (lineWidth / 2), mazeWidth, lineWidth), null, Color.White);

            // Draw left border
            spriteBatch.Draw(pixelTexture, new Rectangle((int)mazePosition.X - (lineWidth / 2), (int)mazePosition.Y, lineWidth, mazeHeight), null, Color.White);

            for (int y = 0; y < mazeGenerator.height; y++)
            {
                for (int x = 0; x < mazeGenerator.width; x++)
                {
                    Cell cell = mazeGenerator.grid[x, y];
                    Vector2 cellPosition = new Vector2(x * cellSize, y * cellSize) + mazePosition;

                    // East wall
                    if (cell.Edges["e"] == null || !cell.Edges["e"].Visited)
                    {
                        spriteBatch.Draw(pixelTexture, new Rectangle((int)cellPosition.X + cellSize - (lineWidth / 2), (int)cellPosition.Y, lineWidth, cellSize), null, Color.White);
                    }

                    // South wall
                    if (cell.Edges["s"] == null || !cell.Edges["s"].Visited)
                    {
                        spriteBatch.Draw(pixelTexture, new Rectangle((int)cellPosition.X, (int)cellPosition.Y + cellSize - (lineWidth / 2), cellSize, lineWidth), null, Color.White);
                    }
                }
            }
        }

        private void DrawPlayer()
        {
            // Calculate the center of the player circle
            float circleCenterX = playerScreenPosition.X + cellSize / 2 + 10;
            float circleCenterY = playerScreenPosition.Y + cellSize / 2 + 10;

            // Calculate the radius of the player circle (half of the cell size plus additional size)
            float circleRadius = (cellSize / 2) + 80; // Adjust the additional size as needed

            // Draw the player circle
            spriteBatch.Draw(playerTexture, new Vector2(circleCenterX, circleCenterY), null, Color.White, 0f,
                             new Vector2(playerTexture.Width / 2, playerTexture.Height / 2), circleRadius / (playerTexture.Width / 2),
                             SpriteEffects.None, 0f);
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
