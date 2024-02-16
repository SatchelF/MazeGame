using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace MazeGame
{
    public class Game1 : Game
    {
        // Graphics and drawing fields
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private Texture2D backgroundTexture; // Background texture for the maze area
        private Texture2D pixelTexture; // 1x1 pixel texture for drawing lines
        private Texture2D playerTexture; // Player texture
        private SpriteFont titleFont;
        private SpriteFont buttonFont;
        private Texture2D breadcrumbTexture; // Breadcrumb texture
        private bool showBreadcrumbs = false; // Flag to toggle breadcrumbs visibility
        private bool showHint = false; // Tracks hint visibility


        // Display and gameplay settings
        private List<Point> breadcrumbs = new List<Point>(); // List to track visited cells
        private const int MazeDisplayWidth = 525;
        private const int MazeDisplayHeight = 525;
        private int cellSize;

        // Game state management
        private KeyboardState previousKeyboardState;
        private GameState currentState;
        public enum GameState { MainMenu, Playing, HighScores, Credits }

        // Timing and scoring
        private TimeSpan gameTimer;
        private int gameScore = 0;
        private int highScore = 0;
        private bool gameActive;

        // Maze and player
        private MazeGenerator mazeGenerator;
        private Point playerGridPosition;
        private Point endPoint;
        private Vector2 playerScreenPosition;
        private List<Point> shortestPath = new List<Point>();
        private bool showShortestPath = false;

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
            breadcrumbTexture = Content.Load<Texture2D>("breadcrumb");

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
            if (currentState == GameState.Playing && gameActive)
            {
                if (playerGridPosition != endPoint)
                {
                    gameTimer += gameTime.ElapsedGameTime; // Update game timer
                }
                else
                {
                    gameActive = false; // Player reached the end, stop updating the timer
                                        // Trigger any end game logic here (e.g., display victory message)
                }
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

            if (currentState == GameState.Playing)
            {
                // Up movement - Arrow Up, W, or I
                if ((currentKeyboardState.IsKeyDown(Keys.Up) || currentKeyboardState.IsKeyDown(Keys.W) || currentKeyboardState.IsKeyDown(Keys.I)) && previousKeyboardState.IsKeyUp(Keys.Up) && previousKeyboardState.IsKeyUp(Keys.W) && previousKeyboardState.IsKeyUp(Keys.I))
                    MovePlayer("n");

                // Down movement - Arrow Down, S, or K
                if ((currentKeyboardState.IsKeyDown(Keys.Down) || currentKeyboardState.IsKeyDown(Keys.S) || currentKeyboardState.IsKeyDown(Keys.K)) && previousKeyboardState.IsKeyUp(Keys.Down) && previousKeyboardState.IsKeyUp(Keys.S) && previousKeyboardState.IsKeyUp(Keys.K))
                    MovePlayer("s");

                // Left movement - Arrow Left, A, or J
                if ((currentKeyboardState.IsKeyDown(Keys.Left) || currentKeyboardState.IsKeyDown(Keys.A) || currentKeyboardState.IsKeyDown(Keys.J)) && previousKeyboardState.IsKeyUp(Keys.Left) && previousKeyboardState.IsKeyUp(Keys.A) && previousKeyboardState.IsKeyUp(Keys.J))
                    MovePlayer("w");

                // Right movement - Arrow Right, D, or L
                if ((currentKeyboardState.IsKeyDown(Keys.Right) || currentKeyboardState.IsKeyDown(Keys.D) || currentKeyboardState.IsKeyDown(Keys.L)) && previousKeyboardState.IsKeyUp(Keys.Right) && previousKeyboardState.IsKeyUp(Keys.D) && previousKeyboardState.IsKeyUp(Keys.L))
                    MovePlayer("e");
            }

            if (currentKeyboardState.IsKeyDown(Keys.B) && previousKeyboardState.IsKeyUp(Keys.B))
            {
                showBreadcrumbs = !showBreadcrumbs;
            }

            // Toggle shortest path visibility
            if (currentKeyboardState.IsKeyDown(Keys.P) && previousKeyboardState.IsKeyUp(Keys.P))
            {
                showShortestPath = !showShortestPath;
                // Recalculate path if turning on the shortest path visibility
                if (showShortestPath || showHint)
                {
                    UpdateShortestPath();
                }
            }

            // Toggle hint visibility
            if (currentKeyboardState.IsKeyDown(Keys.Y) && previousKeyboardState.IsKeyUp(Keys.Y))
            {
                showHint = !showHint;
                // Always recalculate path when toggling the hint to ensure it's available
                UpdateShortestPath();
            }



            // Update the previous keyboard state at the end of the method
            previousKeyboardState = currentKeyboardState;
        }

        private void MovePlayer(string direction)
        {
            // Retrieve the current cell that the player is in using their grid position.
            Cell currentCell = mazeGenerator.grid[playerGridPosition.X, playerGridPosition.Y];
            // Determine the new position based on the direction of movement
            if (currentCell.Edges.ContainsKey(direction) && currentCell.Edges[direction] != null)
            {
                playerGridPosition = new Point(currentCell.Edges[direction].X, currentCell.Edges[direction].Y);

                // After updating the player's grid position, add the new position to the breadcrumbs list
                if (!breadcrumbs.Contains(playerGridPosition)) // Check if the new position is not already in the breadcrumbs
                {
                    breadcrumbs.Add(playerGridPosition); // Add the new position to the breadcrumbs list
                }

                if (showShortestPath || showHint)
                {
                    shortestPath = mazeGenerator.FindPath(playerGridPosition, endPoint); // Recalculate path
                }

                UpdatePlayerScreenPosition(); // Update the player's screen position
            }
        }




        private void StartGame(int size)
        {
            currentState = GameState.Playing;
            mazeGenerator = new MazeGenerator(size, size);
            mazeGenerator.GenerateMaze();

            cellSize = Math.Min(MazeDisplayWidth / size, MazeDisplayHeight / size);
            playerGridPosition = new Point(0, 0); // Start at the top-left corner of the maze.
            endPoint = new Point(size - 1, size - 1); // End at the bottom-right corner.
            gameActive = true; // Game is now active.
            gameTimer = TimeSpan.Zero; // Reset game timer.

            breadcrumbs.Clear(); // Clear any existing breadcrumbs from previous games.
            breadcrumbs.Add(playerGridPosition); // Optionally, add the starting position as the first breadcrumb.
            gameScore = 0; // Reset score at the start of the game
            showShortestPath = true;
            UpdateShortestPath();
            showHint = false; // Same as above, start with hints off
            showShortestPath = false;

            UpdatePlayerScreenPosition();
        }


        private void UpdatePlayerScreenPosition()
        {
            // Calculate the offset to center the maze on the screen
            Vector2 mazeOffset = new Vector2((graphics.PreferredBackBufferWidth - MazeDisplayWidth) / 2, (graphics.PreferredBackBufferHeight - MazeDisplayHeight) / 2);
            playerScreenPosition = new Vector2(playerGridPosition.X * cellSize + mazeOffset.X, playerGridPosition.Y * cellSize + mazeOffset.Y);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin();

            // Draw the current score
            spriteBatch.DrawString(buttonFont, $"Score: {gameScore}", new Vector2(20, 40), Color.White);



            // Draw the game title at the top center
            Vector2 titleSize = titleFont.MeasureString("The Maze Game");
            spriteBatch.DrawString(titleFont, "The Maze Game", new Vector2((graphics.PreferredBackBufferWidth - titleSize.X) / 2, 20), Color.GhostWhite);

            // Draw menu choices on the left if in MainMenu or Playing state
            DrawMenuChoices();

            // Center and draw the maze in the remaining space if in Playing state
            if (currentState == GameState.Playing)
            {
                DrawMaze();
                DrawPlayer();
                DrawHighScore();
                DrawTimer();

                // Draw breadcrumbs if toggled on
                if (showBreadcrumbs)
                {
                    foreach (var breadcrumb in breadcrumbs)
                    {
                        Vector2 breadcrumbPosition = new Vector2(breadcrumb.X * cellSize, breadcrumb.Y * cellSize) + new Vector2((graphics.PreferredBackBufferWidth - MazeDisplayWidth) / 2, (graphics.PreferredBackBufferHeight - MazeDisplayHeight) / 2);
                        spriteBatch.Draw(breadcrumbTexture, new Rectangle((int)breadcrumbPosition.X, (int)breadcrumbPosition.Y, cellSize, cellSize), Color.LightYellow);
                    }
                }
            }

            if (showShortestPath)
            {
                foreach (var point in shortestPath)
                {
                    Vector2 pathPosition = new Vector2(point.X * cellSize, point.Y * cellSize) + new Vector2((graphics.PreferredBackBufferWidth - MazeDisplayWidth) / 2, (graphics.PreferredBackBufferHeight - MazeDisplayHeight) / 2);
                    spriteBatch.Draw(breadcrumbTexture, new Rectangle((int)pathPosition.X, (int)pathPosition.Y, cellSize, cellSize), Color.Red); 
                }
            }

            if (showHint && shortestPath.Count > 1) // Ensure there's a path and it's more than the current position
            {
                DrawHint(shortestPath[1]); // Draw hint for the next move
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
            // Early exit if maze generator is not initialized
            if (mazeGenerator == null) return;

            // Setup for drawing the maze
            int mazeWidth = cellSize * mazeGenerator.width;
            int mazeHeight = cellSize * mazeGenerator.height;
            Vector2 mazePosition = new Vector2(
                (graphics.PreferredBackBufferWidth - mazeWidth) / 2,
                (graphics.PreferredBackBufferHeight - mazeHeight) / 2
            );

            // Drawing maze background
            spriteBatch.Draw(
                backgroundTexture,
                new Rectangle((int)mazePosition.X, (int)mazePosition.Y, mazeWidth, mazeHeight),
                Color.White
            );

            // Prepare for drawing walls
            int lineWidth = 3;

            // Drawing walls within the maze
            for (int y = 0; y < mazeGenerator.height; y++)
            {
                for (int x = 0; x < mazeGenerator.width; x++)
                {
                    DrawWalls(x, y, mazePosition, lineWidth);
                }
            }

            // Draw start and end indicators
            DrawIndicator(mazePosition, Color.LightGreen, 0, 0); // Start indicator
            DrawIndicator(mazePosition, Color.IndianRed, endPoint.X, endPoint.Y); // End indicator
        }

        private void DrawWalls(int x, int y, Vector2 mazePosition, int lineWidth)
        {
            Cell cell = mazeGenerator.grid[x, y];
            Vector2 cellPosition = new Vector2(x * cellSize, y * cellSize) + mazePosition;

            // North wall (drawn for the first row of cells)
            if (y == 0)
            {
                spriteBatch.Draw(
                    pixelTexture,
                    new Rectangle((int)cellPosition.X, (int)cellPosition.Y - (lineWidth / 2), cellSize, lineWidth),
                    null, Color.White
                );
            }

            // West wall (drawn for the first column of cells)
            if (x == 0)
            {
                spriteBatch.Draw(
                    pixelTexture,
                    new Rectangle((int)cellPosition.X - (lineWidth / 2), (int)cellPosition.Y, lineWidth, cellSize),
                    null, Color.White
                );
            }

            // East wall
            if (cell.Edges["e"] == null || !cell.Edges["e"].Visited)
            {
                spriteBatch.Draw(
                    pixelTexture,
                    new Rectangle((int)cellPosition.X + cellSize - (lineWidth / 2), (int)cellPosition.Y, lineWidth, cellSize),
                    null, Color.White
                );
            }

            // South wall
            if (cell.Edges["s"] == null || !cell.Edges["s"].Visited)
            {
                spriteBatch.Draw(
                    pixelTexture,
                    new Rectangle((int)cellPosition.X, (int)cellPosition.Y + cellSize - (lineWidth / 2), cellSize, lineWidth),
                    null, Color.White
                );
            }
        }


        private void DrawIndicator(Vector2 mazePosition, Color color, int cellX, int cellY)
        {
            float scale = 0.8f;
            int squareSize = (int)(cellSize * scale);
            int offset = (cellSize - squareSize) / 2; // Center the square within the cell

            Vector2 squarePosition = new Vector2(
                mazePosition.X + (cellX * cellSize) + offset,
                mazePosition.Y + (cellY * cellSize) + offset
            );

            spriteBatch.Draw(
                pixelTexture,
                new Rectangle((int)squarePosition.X, (int)squarePosition.Y, squareSize, squareSize),
                color
            );
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

        private void DrawHint(Point nextMove)
        {
            Vector2 hintPosition = new Vector2(nextMove.X * cellSize, nextMove.Y * cellSize) +
                                   new Vector2((graphics.PreferredBackBufferWidth - MazeDisplayWidth) / 2,
                                               (graphics.PreferredBackBufferHeight - MazeDisplayHeight) / 2);

            // Assuming you have a texture for the hint or use a simple colored rectangle
            spriteBatch.Draw(pixelTexture,
                             new Rectangle((int)hintPosition.X, (int)hintPosition.Y, cellSize, cellSize),
                             Color.Yellow); // Use a distinct color for the hint
        }

        private void UpdateShortestPath()
        {
            // Calculate the shortest path if either display is enabled
            if (showShortestPath || showHint)
            {
                shortestPath = mazeGenerator.FindPath(playerGridPosition, endPoint);
            }
        }

        private void UpdateScore(Point playerPosition)
        {
            var cell = mazeGenerator.grid[playerPosition.X, playerPosition.Y];
            if (!cell.HasBeenScored && !cell.PlayerVisited)
            {
                if (shortestPath.Contains(playerPosition))
                {
                    gameScore += 5;
                    cell.HasBeenScored = true;
                }
                else if (IsAdjacentToShortestPath(playerPosition))
                {
                    gameScore -= 1;
                    // Adjacent cells may not be marked as scored to allow -1 scoring once
                }
                else
                {
                    gameScore -= 2;
                }
            }

            // Mark the cell as visited by the player after scoring
            cell.PlayerVisited = true;

            if (gameScore > highScore)
            {
                highScore = gameScore;
            }
        }


        private bool IsAdjacentToShortestPath(Point playerPosition)
        {
            foreach (var pathPoint in shortestPath)
            {
                if (Math.Abs(pathPoint.X - playerPosition.X) <= 1 && Math.Abs(pathPoint.Y - playerPosition.Y) <= 1)
                {
                    return true;
                }
            }
            return false;
        }

    }
}
