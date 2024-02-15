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

        private const int MazeDisplayWidth = 550; // Adjusted width
        private const int MazeDisplayHeight = 550; // Adjusted height
        private KeyboardState previousKeyboardState;



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
            backgroundTexture = Content.Load<Texture2D>("maze-background");
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            ProcessInput();
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
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin();

            // Draw the game title at the top center
            Vector2 titleSize = titleFont.MeasureString("The Maze Game");
            spriteBatch.DrawString(titleFont, "The Maze Game", new Vector2((graphics.PreferredBackBufferWidth - titleSize.X) / 2, 20), Color.LightGreen);

            // Draw menu choices on the left if in MainMenu or Playing state
            if (currentState == GameState.MainMenu || currentState == GameState.Playing)
            {
                DrawMenuChoices();
            }

            // Center and draw the maze in the remaining space if in Playing state
            if (currentState == GameState.Playing)
            {
                DrawMaze();
            }

            spriteBatch.End();
            base.Draw(gameTime);
        }

        private void DrawMenuChoices()
        {
            float menuStartY = 100;
            float menuItemSpacing = 60;

            spriteBatch.DrawString(buttonFont, "F1 - New Game 5x5", new Vector2(10, menuStartY), Color.IndianRed);
            spriteBatch.DrawString(buttonFont, "F2 - New Game 10x10", new Vector2(10, menuStartY + menuItemSpacing), Color.IndianRed);
            spriteBatch.DrawString(buttonFont, "F3 - New Game 15x15", new Vector2(10, menuStartY + menuItemSpacing * 2), Color.IndianRed);
            spriteBatch.DrawString(buttonFont, "F4 - New Game 20x20", new Vector2(10, menuStartY + menuItemSpacing * 3), Color.IndianRed);
            spriteBatch.DrawString(buttonFont, "F5 - High Scores", new Vector2(10, menuStartY + menuItemSpacing * 4), Color.IndianRed);
            spriteBatch.DrawString(buttonFont, "F6 - Credits", new Vector2(10, menuStartY + menuItemSpacing * 5), Color.IndianRed);
        }

        private void DrawMaze()
        {
            if (mazeGenerator == null) return;

            int cellSize = Math.Min(MazeDisplayWidth / mazeGenerator.width, MazeDisplayHeight / mazeGenerator.height);
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

    }
}
