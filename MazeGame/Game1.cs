using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MazeGame
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        // Add fields for the player's texture and position
        private Texture2D playerTexture;
        private Vector2 playerPosition;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // Initialize player position here
            playerPosition = new Vector2(_graphics.PreferredBackBufferWidth / 2, _graphics.PreferredBackBufferHeight / 2);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Load the player texture
            playerTexture = Content.Load<Texture2D>("player"); // Ensure there's a "player.png" in the Content folder, added via the Pipeline Tool
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // Basic movement logic
            var keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.Right))
                playerPosition.X += 2;
            if (keyboardState.IsKeyDown(Keys.Left))
                playerPosition.X -= 2;
            if (keyboardState.IsKeyDown(Keys.Up))
                playerPosition.Y -= 2;
            if (keyboardState.IsKeyDown(Keys.Down))
                playerPosition.Y += 2;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();
            _spriteBatch.Draw(playerTexture, playerPosition, Color.White);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
