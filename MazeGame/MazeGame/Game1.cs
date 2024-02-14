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

        // Game state management
        public enum GameState
        {
            MainMenu,
            Playing,
            HighScores,
            Credits
        }

        private GameState currentState;

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
            // Initialize and start a new game with the specified maze size
            currentState = GameState.Playing;
            // Your maze generation logic here...
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();

            if (currentState == GameState.MainMenu)
            {
                spriteBatch.DrawString(titleFont, "The Maze Game", new Vector2(250, 100), Color.LightGreen);
                spriteBatch.DrawString(buttonFont, "Press F1 for New Game 5x5", new Vector2(100, 200), Color.White);
                spriteBatch.DrawString(buttonFont, "Press F2 for New Game 10x10", new Vector2(100, 250), Color.White);
                spriteBatch.DrawString(buttonFont, "Press F3 for New Game 15x15", new Vector2(100, 300), Color.White);
                spriteBatch.DrawString(buttonFont, "Press F4 for New Game 20x20", new Vector2(100, 350), Color.White);
                spriteBatch.DrawString(buttonFont, "Press F5 for High Scores", new Vector2(100, 400), Color.White);
                spriteBatch.DrawString(buttonFont, "Press F6 for Credits", new Vector2(100, 450), Color.White); ;
            }
            else if (currentState == GameState.Playing)
            {
                // Draw the game playing screen...
            }
            // Handle drawing for other game states...

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }

}


