using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MazeGame
{
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont titleFont;
        SpriteFont buttonFont;
        Texture2D backgroundTexture; // Background texture field
        Texture2D pixelTexture;


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
            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 600;
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
            pixelTexture.SetData(new[] { Color.White }); // A white 1x1 pixel texture

            backgroundTexture = Content.Load<Texture2D>("grey-background"); // Load the background texture
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            ProcessInput();
        }

        private void ProcessInput()
        {
            KeyboardState state = Keyboard.GetState();

            if (currentState == GameState.MainMenu)
            {
                if (state.IsKeyDown(Keys.F1)) StartGame(5);
                else if (state.IsKeyDown(Keys.F2)) StartGame(10);
                else if (state.IsKeyDown(Keys.F3)) StartGame(15);
                else if (state.IsKeyDown(Keys.F4)) StartGame(20);
                else if (state.IsKeyDown(Keys.F5)) currentState = GameState.HighScores;
                else if (state.IsKeyDown(Keys.F6)) currentState = GameState.Credits;
            }
        }

        private void StartGame(int size)
        {
            currentState = GameState.Playing;
            mazeGenerator = new MazeGenerator(size, size); // Initialize with the selected size
            mazeGenerator.GenerateMaze(); // Generate the maze
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin();

            // Draw the background first
            spriteBatch.Draw(backgroundTexture, new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight), Color.White);

            if (currentState == GameState.MainMenu)
            {
                spriteBatch.DrawString(titleFont, "The Maze Game", new Vector2(250, 100), Color.LightGreen);
                spriteBatch.DrawString(buttonFont, "Press F1 for New Game 5x5", new Vector2(100, 200), Color.White);
                spriteBatch.DrawString(buttonFont, "Press F2 for New Game 10x10", new Vector2(100, 250), Color.White);
                spriteBatch.DrawString(buttonFont, "Press F3 for New Game 15x15", new Vector2(100, 300), Color.White);
                spriteBatch.DrawString(buttonFont, "Press F4 for New Game 20x20", new Vector2(100, 350), Color.White);
                spriteBatch.DrawString(buttonFont, "Press F5 for High Scores", new Vector2(100, 400), Color.White);
                spriteBatch.DrawString(buttonFont, "Press F6 for Credits", new Vector2(100, 450), Color.White);
            }
            else if (currentState == GameState.Playing)
            {
                DrawMaze();
            }
            // Handle drawing for other game states...

            spriteBatch.End();
            base.Draw(gameTime);
        }

        private void DrawMaze()
        {
            if (mazeGenerator == null) return;

            int cellSize = 32; // Size of each cell in pixels
            int lineWidth = 2; // Thickness of the maze walls

            for (int y = 0; y < mazeGenerator.height; y++)
            {
                for (int x = 0; x < mazeGenerator.width; x++)
                {
                    Cell cell = mazeGenerator.grid[x, y];
                    Vector2 cellPosition = new Vector2(x * cellSize, y * cellSize);

                    // Draw the East wall if necessary
                    if (cell.Edges["e"] == null || cell.Edges["e"].Visited == false)
                    {
                        // Draw a vertical line
                        spriteBatch.Draw(pixelTexture, new Rectangle((int)cellPosition.X + cellSize, (int)cellPosition.Y, lineWidth, cellSize + lineWidth), null, Color.White);
                    }

                    // Draw the South wall if necessary
                    if (cell.Edges["s"] == null || cell.Edges["s"].Visited == false)
                    {
                        // Draw a horizontal line
                        spriteBatch.Draw(pixelTexture, new Rectangle((int)cellPosition.X, (int)cellPosition.Y + cellSize, cellSize + lineWidth, lineWidth), null, Color.White);
                    }

                    // Optionally, draw North and West walls for the border cells
                    if (x == 0) // West wall for the first column
                    {
                        spriteBatch.Draw(pixelTexture, new Rectangle((int)cellPosition.X, (int)cellPosition.Y, lineWidth, cellSize + lineWidth), null, Color.White);
                    }
                    if (y == 0) // North wall for the first row
                    {
                        spriteBatch.Draw(pixelTexture, new Rectangle((int)cellPosition.X, (int)cellPosition.Y, cellSize + lineWidth, lineWidth), null, Color.White);
                    }
                }
            }
        }

    }
}
